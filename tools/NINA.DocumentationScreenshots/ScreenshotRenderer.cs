#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using NINA.Sequencer.Container;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Trigger;
using NINA.Profile.Interfaces;
using NINA.ViewModel.Sequencer;

namespace NINA.DocumentationScreenshots;

public sealed class ScreenshotRenderer(FixtureRegistry fixtures) {
    public void Render(ScreenshotAsset asset, string outputPath) {
        using BindingTraceScope bindingTrace = new(asset.Id);
        FrameworkElement fixture = fixtures.Create(asset);
        int renderWidth = asset.RenderWidth ?? asset.Width;
        int renderHeight = asset.RenderHeight ?? asset.Height;
        bool contextMenuCapture = RequiresContextMenu(asset);
        bool desktopCapture = RequiresDesktopContextMenus(asset);
        using CursorPositionScope? cursorScope = contextMenuCapture ? CursorPositionScope.MoveAway(asset.Id) : null;
        bool requestsCrop = asset.Crop is not null || !string.IsNullOrWhiteSpace(asset.CropTarget);
        IReadOnlyList<ScreenshotCallout> renderCallouts = requestsCrop ? [] : asset.Callouts;
        FrameworkElement content = ScreenshotChrome.Wrap(fixture, renderWidth, renderHeight, renderCallouts);
        using RenderWindow host = new() {
            Width = renderWidth,
            Height = renderHeight,
            Left = desktopCapture ? 32 : -32000,
            Top = desktopCapture ? 32 : -32000,
            ShowInTaskbar = false,
            ShowActivated = desktopCapture,
            Topmost = desktopCapture,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            Content = content
        };

        host.Show();
        content.Measure(new Size(renderWidth, renderHeight));
        content.Arrange(new Rect(0, 0, renderWidth, renderHeight));
        content.UpdateLayout();
        DrainDispatcher();
        NamedStateController.Apply(fixture, asset);
        DrainDispatcher();
        bindingTrace.ThrowIfErrors();

        ScreenshotCrop? resolvedCrop = ResolveCrop(content, fixture, asset, renderWidth, renderHeight);
        if (desktopCapture) {
            MakeOpenMenusTopmost(content, asset.Id);
            Thread.Sleep(300);
            DrainDispatcher();
            KeepSimpleContainerMenuOpen(content, asset);
        }
        BitmapSource rendered = desktopCapture
            ? RenderWithDesktopContextMenus(content, asset, renderWidth, renderHeight)
            : RenderWithOpenPopups(content, asset, renderWidth, renderHeight);
        if (!desktopCapture) {
            CloseOpenMenus(content);
        }
        BitmapSource finalBitmap = ApplyCrop(rendered, asset, resolvedCrop);
        if (resolvedCrop is not null && asset.Callouts.Count > 0) {
            System.Windows.Controls.Image image = new() { Source = finalBitmap, Width = asset.Width, Height = asset.Height, Stretch = Stretch.Fill };
            FrameworkElement annotated = ScreenshotChrome.Wrap(image, asset.Width, asset.Height, asset.Callouts);
            annotated.Measure(new Size(asset.Width, asset.Height));
            annotated.Arrange(new Rect(0, 0, asset.Width, asset.Height));
            annotated.UpdateLayout();
            finalBitmap = RenderElement(annotated, asset.Width, asset.Height);
        }
        ValidateNonBlank(finalBitmap, asset.Id);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        using FileStream stream = File.Create(outputPath);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(finalBitmap));
        encoder.Save(stream);
    }

    private static BitmapSource RenderWithOpenPopups(FrameworkElement content, ScreenshotAsset asset, int renderWidth, int renderHeight) {
        RenderTargetBitmap baseLayer = RenderElement(content, renderWidth, renderHeight);
        List<(Popup Popup, FrameworkElement? Anchor)> openPopups = FindVisualDescendants<Popup>(content)
            .Where(popup => popup.IsOpen)
            .Select(popup => (popup, popup.PlacementTarget as FrameworkElement))
            .ToList();
        foreach (ComboBox comboBox in FindVisualDescendants<ComboBox>(content)
            .Where(comboBox => comboBox.IsDropDownOpen || NamedStateController.ShouldCaptureDropDown(comboBox))) {
            comboBox.ApplyTemplate();
            Popup? popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup
                ?? comboBox.Template.FindName("Popup", comboBox) as Popup;
            if (popup is not null) {
                if (!popup.IsOpen && popup.Child is FrameworkElement closedPopupChild) {
                    closedPopupChild.Measure(new Size(renderWidth, renderHeight));
                    Size desired = closedPopupChild.DesiredSize;
                    closedPopupChild.Arrange(new Rect(
                        0,
                        0,
                        Math.Max(comboBox.ActualWidth, desired.Width),
                        Math.Max(1, desired.Height)));
                    closedPopupChild.UpdateLayout();
                }
                int existingIndex = openPopups.FindIndex(item => ReferenceEquals(item.Popup, popup));
                if (existingIndex >= 0) {
                    openPopups[existingIndex] = (popup, comboBox);
                } else {
                    openPopups.Add((popup, comboBox));
                }
            }
        }
        List<(Popup Popup, FrameworkElement Child, FrameworkElement? Anchor)> popupLayers = openPopups
            .Where(item => item.Popup.Child is FrameworkElement)
            .Select(item => (item.Popup, (FrameworkElement)item.Popup.Child, item.Anchor))
            .ToList();
        List<(ContextMenu Menu, FrameworkElement Anchor)> contextMenuLayers = FindVisualDescendants<Button>(content)
            .Where(button => button.ContextMenu?.IsOpen == true)
            .Select(button => (button.ContextMenu!, (FrameworkElement)button))
            .ToList();
        List<(ToolTip ToolTip, FrameworkElement Anchor)> toolTipLayers = FindVisualDescendants<FrameworkElement>(content)
            .Where(element => element.ToolTip is ToolTip { IsOpen: true })
            .Select(element => ((ToolTip)element.ToolTip, element))
            .ToList();
        bool requiresPopup = RequiresPopup(asset);
        if (requiresPopup && popupLayers.Count == 0) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested an open dropdown state but its production Popup was not available.");
        }
        bool requiresContextMenu = RequiresContextMenu(asset);
        if (requiresContextMenu && contextMenuLayers.Count == 0) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested an add menu state but its production ContextMenu was not available.");
        }
        if (popupLayers.Count == 0 && contextMenuLayers.Count == 0 && toolTipLayers.Count == 0) {
            return baseLayer;
        }

        Point contentOrigin = content.PointToScreen(new Point(0, 0));
        DrawingVisual composite = new();
        using (DrawingContext drawing = composite.RenderOpen()) {
            drawing.DrawImage(baseLayer, new Rect(0, 0, renderWidth, renderHeight));
            foreach ((Popup popup, FrameworkElement popupChild, FrameworkElement? anchor) in popupLayers) {
                int width = Math.Max(1, (int)Math.Ceiling(popupChild.ActualWidth));
                int height = Math.Max(1, (int)Math.Ceiling(popupChild.ActualHeight));
                if (requiresPopup && (width <= 1 || height < renderHeight / 2)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' opened its production Popup but it had invalid dimensions {width}x{height}.");
                }
                Point popupOrigin = popup.IsOpen
                    ? popupChild.PointToScreen(new Point(0, 0))
                    : new Point();
                popupChild.Measure(new Size(width, height));
                popupChild.Arrange(new Rect(0, 0, width, height));
                popupChild.UpdateLayout();
                BitmapSource popupLayer = RenderElement(popupChild, width, height);
                ValidateNonBlank(popupLayer, asset.Id + " production Popup");
                Point relativeOrigin = new(popupOrigin.X - contentOrigin.X, popupOrigin.Y - contentOrigin.Y);
                if (anchor is not null) {
                    relativeOrigin = anchor.TranslatePoint(
                        new Point(0, anchor.ActualHeight),
                        content);
                }
                drawing.DrawImage(popupLayer, new Rect(relativeOrigin.X, relativeOrigin.Y, width, height));
            }
            foreach ((ContextMenu menu, FrameworkElement anchor) in contextMenuLayers) {
                Size renderedSize = GetRenderedSize(menu);
                int width = Math.Max(1, (int)Math.Ceiling(renderedSize.Width));
                int height = Math.Max(1, (int)Math.Ceiling(renderedSize.Height));
                if (requiresContextMenu && (width <= 1 || height <= 1)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' opened its production ContextMenu but it had invalid dimensions {width}x{height}.");
                }
                BitmapSource menuLayer = RenderVisualBrush(menu, width, height);
                ValidateNonBlank(menuLayer, asset.Id + " production ContextMenu");
                Point relativeOrigin = GetContextMenuOrigin(anchor, menu, width, content, renderWidth);
                drawing.DrawImage(menuLayer, new Rect(relativeOrigin.X, relativeOrigin.Y, width, height));
            }
            foreach ((ToolTip tooltip, FrameworkElement anchor) in toolTipLayers) {
                Size renderedSize = GetRenderedSize(tooltip);
                int width = Math.Max(1, (int)Math.Ceiling(renderedSize.Width));
                int height = Math.Max(1, (int)Math.Ceiling(renderedSize.Height));
                BitmapSource tooltipLayer = RenderVisualBrush(tooltip, width, height);
                ValidateNonBlank(tooltipLayer, asset.Id + " production ToolTip");
                Point relativeOrigin = GetToolTipOrigin(
                    anchor,
                    tooltip,
                    new Size(width, height),
                    content,
                    renderWidth,
                    renderHeight);
                drawing.DrawImage(tooltipLayer, new Rect(relativeOrigin.X, relativeOrigin.Y, width, height));
            }
        }

        RenderTargetBitmap result = new(renderWidth, renderHeight, 96, 96, PixelFormats.Pbgra32);
        result.Render(composite);
        return result;
    }

    private static bool RequiresPopup(ScreenshotAsset asset) => asset.Output.Replace('\\', '/').EndsWith(
        "/sequencer/conditions/loopuntiltime.png",
        StringComparison.OrdinalIgnoreCase);

    private static bool RequiresContextMenu(ScreenshotAsset asset) {
        string output = asset.Output.Replace('\\', '/');
        return output.EndsWith("/sequencer/Sequencer_AddTrigger.png", StringComparison.OrdinalIgnoreCase)
            || output.EndsWith("/sequencer/Sequencer_AddLoopCondition.png", StringComparison.OrdinalIgnoreCase)
            || output.EndsWith("/sequencer/Sequencer_AddInstruction.png", StringComparison.OrdinalIgnoreCase)
            || output.EndsWith("/sequencer/Sequencer_AddInstructionSet.png", StringComparison.OrdinalIgnoreCase);
    }

    private static bool RequiresDesktopContextMenus(ScreenshotAsset asset) {
        string output = asset.Output.Replace('\\', '/');
        return output.EndsWith("/sequencer/Sequencer_AddInstruction.png", StringComparison.OrdinalIgnoreCase)
            || output.EndsWith("/sequencer/Sequencer_AddInstructionSet.png", StringComparison.OrdinalIgnoreCase);
    }

    private static void KeepSimpleContainerMenuOpen(FrameworkElement content, ScreenshotAsset asset) {
        string output = asset.Output.Replace('\\', '/');
        string? buttonName = output.EndsWith("/sequencer/Sequencer_AddTrigger.png", StringComparison.OrdinalIgnoreCase)
            ? "AddTriggerButton"
            : output.EndsWith("/sequencer/Sequencer_AddLoopCondition.png", StringComparison.OrdinalIgnoreCase)
                ? "AddConditionButton"
                : null;
        if (buttonName is null) {
            return;
        }

        IEnumerable<Button> candidates = FindVisualDescendants<Button>(content)
            .Where(control => control.Name == buttonName && control.ContextMenu is not null);
        Button button = candidates
            .FirstOrDefault(control => control.DataContext is ISequenceContainer container && container.Parent is not null)
            ?? candidates.FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not restore NINA's production '{buttonName}' menu before capture.");
        button.ContextMenu!.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
        DrainDispatcher();
        if (!button.ContextMenu.IsOpen) {
            throw new CatalogException($"Screenshot '{asset.Id}' could not keep NINA's production '{buttonName}' menu open for capture.");
        }
        MakeOpenMenusTopmost(content, asset.Id);
    }

    private static BitmapSource RenderWithDesktopContextMenus(
            FrameworkElement content,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        RenderTargetBitmap baseLayer = RenderElement(content, renderWidth, renderHeight);
        List<ContextMenu> menus = FindVisualDescendants<Button>(content)
            .Where(button => button.ContextMenu?.IsOpen == true)
            .Select(button => button.ContextMenu!)
            .ToList();
        if (menus.Count == 0) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested a production menu but none was open.");
        }
        List<HwndSource> popupSources = GetPopupSources();
        if (popupSources.Count == 0) {
            throw new CatalogException($"Screenshot '{asset.Id}' opened production menus but WPF exposed no popup windows.");
        }
        Point contentOrigin = content.PointToScreen(new Point(0, 0));
        DpiScale dpi = VisualTreeHelper.GetDpi(content);
        DrawingVisual composite = new();
        using (DrawingContext drawing = composite.RenderOpen()) {
            drawing.DrawImage(baseLayer, new Rect(0, 0, renderWidth, renderHeight));
            foreach (HwndSource source in popupSources) {
                if (!GetWindowRect(source.Handle, out WindowRect bounds)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' could not determine its production popup bounds.");
                }
                FrameworkElement popupVisual = source.RootVisual as FrameworkElement
                    ?? throw new CatalogException($"Screenshot '{asset.Id}' opened a production popup without a renderable visual tree.");
                Size popupSize = GetRenderedSize(popupVisual);
                int popupWidth = Math.Max(1, (int)Math.Ceiling(popupSize.Width));
                int popupHeight = Math.Max(1, (int)Math.Ceiling(popupSize.Height));
                BitmapSource popup = RenderVisualBrush(popupVisual, popupWidth, popupHeight);
                ValidateNonBlank(popup, asset.Id + " production popup");
                double x = (bounds.Left - contentOrigin.X) / dpi.DpiScaleX;
                double y = (bounds.Top - contentOrigin.Y) / dpi.DpiScaleY;
                drawing.DrawImage(popup, new Rect(x, y, popupSize.Width, popupSize.Height));
            }
        }
        RenderTargetBitmap result = new(renderWidth, renderHeight, 96, 96, PixelFormats.Pbgra32);
        result.Render(composite);
        return result;
    }

    private static void MakeOpenMenusTopmost(FrameworkElement content, string screenshotId) {
        List<ContextMenu> menus = FindVisualDescendants<Button>(content)
            .Where(button => button.ContextMenu?.IsOpen == true)
            .Select(button => button.ContextMenu!)
            .ToList();
        if (menus.Count == 0) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not find its open production menu for desktop capture.");
        }
        foreach (HwndSource source in GetPopupSources()) {
            if (source.Handle == IntPtr.Zero
                || !SetWindowPos(source.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010)) {
                throw new CatalogException($"Screenshot '{screenshotId}' could not place its production popup above the capture window.");
            }
        }
    }

    private static List<HwndSource> GetPopupSources() => PresentationSource.CurrentSources
        .OfType<HwndSource>()
        .Where(source => source.RootVisual?.GetType().Name == "PopupRoot"
            && source.Handle != IntPtr.Zero
            && IsWindow(source.Handle))
        .ToList();

    private static void CloseOpenMenus(DependencyObject root) {
        List<HwndSource> popupSources = GetPopupSources();
        foreach (HwndSource source in popupSources) {
            if (source.RootVisual is null) {
                continue;
            }
            foreach (MenuItem item in FindVisualDescendants<MenuItem>(source.RootVisual).Reverse()) {
                item.IsSubmenuOpen = false;
            }
            _ = SendMessage(source.Handle, 0x001F, IntPtr.Zero, IntPtr.Zero);
        }
        foreach (ComboBox comboBox in FindVisualDescendants<ComboBox>(root)) {
            comboBox.ApplyTemplate();
            Popup? popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup
                ?? comboBox.Template.FindName("Popup", comboBox) as Popup;
            if (popup is not null) {
                popup.PopupAnimation = PopupAnimation.None;
                popup.IsOpen = false;
            }
            comboBox.IsDropDownOpen = false;
        }
        foreach (Button button in FindVisualDescendants<Button>(root)) {
            if (button.ContextMenu?.IsOpen == true) {
                button.ContextMenu.IsOpen = false;
            }
        }
        foreach (FrameworkElement element in FindVisualDescendants<FrameworkElement>(root)
            .Where(element => element.ToolTip is ToolTip { IsOpen: true })) {
            ((ToolTip)element.ToolTip).IsOpen = false;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr window,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr window, out WindowRect bounds);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out WindowPoint point);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetClipCursor(out WindowRect bounds);

    [DllImport("user32.dll")]
    private static extern bool ClipCursor(ref WindowRect bounds);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowRect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPoint {
        public int X;
        public int Y;
    }

    private static RenderTargetBitmap RenderElement(Visual visual, int width, int height) {
        RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        return bitmap;
    }

    private static RenderTargetBitmap RenderVisualBrush(Visual visual, int width, int height) {
        DrawingVisual drawingVisual = new();
        using (DrawingContext drawing = drawingVisual.RenderOpen()) {
            drawing.DrawRectangle(new VisualBrush(visual) { Stretch = Stretch.Fill }, null, new Rect(0, 0, width, height));
        }
        RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(drawingVisual);
        return bitmap;
    }

    private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root) where T : DependencyObject {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++) {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T result) {
                yield return result;
            }
            foreach (T descendant in FindVisualDescendants<T>(child)) {
                yield return descendant;
            }
        }
    }

    private static BitmapSource ApplyCrop(BitmapSource source, ScreenshotAsset asset, ScreenshotCrop? crop) {
        if (crop is null) {
            return source;
        }

        Int32Rect sourceRect = new(
            (int)Math.Round(crop.X * source.PixelWidth),
            (int)Math.Round(crop.Y * source.PixelHeight),
            Math.Max(1, (int)Math.Round(crop.Width * source.PixelWidth)),
            Math.Max(1, (int)Math.Round(crop.Height * source.PixelHeight)));
        sourceRect.Width = Math.Min(sourceRect.Width, source.PixelWidth - sourceRect.X);
        sourceRect.Height = Math.Min(sourceRect.Height, source.PixelHeight - sourceRect.Y);

        CroppedBitmap cropped = new(source, sourceRect);
        if (cropped.PixelWidth == asset.Width && cropped.PixelHeight == asset.Height) {
            return cropped;
        }

        TransformedBitmap scaled = new(cropped, new ScaleTransform(
            asset.Width / (double)cropped.PixelWidth,
            asset.Height / (double)cropped.PixelHeight));
        return scaled;
    }

    private static ScreenshotCrop? ResolveCrop(
            FrameworkElement content,
            FrameworkElement fixture,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        if (string.IsNullOrWhiteSpace(asset.CropTarget)) {
            return asset.Crop;
        }
        if (asset.CropTarget == "root-add-menu") {
            return ResolveRootAddMenuCrop(content, asset, renderWidth, renderHeight);
        }
        if (asset.CropTarget == "settings:meridian-flip") {
            return ResolveSettingsGroupCrop<IMeridianFlipSettings>(content, asset, renderWidth, renderHeight);
        }
        if (asset.CropTarget.StartsWith("framing:", StringComparison.Ordinal)) {
            return ResolveFramingCrop(content, asset, renderWidth, renderHeight);
        }
        if (asset.CropTarget.StartsWith("simple:", StringComparison.Ordinal)) {
            return ResolveSimpleSequencerCrop(content, fixture, asset, renderWidth, renderHeight);
        }
        if (asset.CropTarget == "sidebar:filtered-item") {
            return ResolveFilteredSidebarItemCrop(content, fixture, asset, renderWidth, renderHeight);
        }
        if (asset.CropTarget.StartsWith("target-area:first-expression:", StringComparison.Ordinal)) {
            return ResolveExpressionCrop(content, fixture, asset, renderWidth, renderHeight);
        }
        bool instructionsOnly = asset.CropTarget == "target-area:first-item-instructions";
        bool triggerOnly = asset.CropTarget == "target-area:first-item-trigger";
        bool targetAreaAllItems = asset.CropTarget == "target-area:all-items";
        bool startAreaAllItems = asset.CropTarget == "start-area:all-items";
        bool endAreaAllItems = asset.CropTarget == "end-area:all-items";
        bool allItems = targetAreaAllItems || startAreaAllItems || endAreaAllItems;
        if (asset.CropTarget != "target-area:first-item" && !instructionsOnly && !triggerOnly && !allItems) {
            throw new CatalogException($"Screenshot '{asset.Id}' has unknown crop target '{asset.CropTarget}'.");
        }
        int areaIndex = startAreaAllItems ? 0 : endAreaAllItems ? 2 : 1;
        if (fixture.DataContext is not ISequence2VM viewModel
            || viewModel.Sequencer.MainContainer.Items.ElementAtOrDefault(areaIndex) is not ISequenceContainer sequenceArea
            || sequenceArea.Items.FirstOrDefault() is not ISequenceItem firstAreaEntity) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested sequence area {areaIndex} but the production sequence has no items there.");
        }

        HashSet<NINA.Sequencer.ISequenceEntity> modelEntities = allItems
            ? sequenceArea.Items.SelectMany(CollectSequenceEntities).ToHashSet()
            : CollectSequenceEntities(firstAreaEntity);
        if (startAreaAllItems && viewModel.Sequencer.MainContainer is ITriggerable rootTriggerable) {
            modelEntities.UnionWith(rootTriggerable.Triggers.SelectMany(CollectSequenceEntities));
        }
        List<FrameworkElement> targets = FindVisualDescendants<FrameworkElement>(content)
            .Where(element => element.DataContext is NINA.Sequencer.ISequenceEntity entity && modelEntities.Contains(entity))
            .Where(element => {
                if (triggerOnly) {
                    return element is Expander
                        && element.GetType().Name == "DetachingExpander"
                        && element.DataContext is ISequenceTrigger;
                }
                if (element.GetType().Name != "SequenceBlockView") {
                    return !instructionsOnly
                        && element.GetType().Name is "HierarchicalSequenceContainerView" or "SequenceContainerView";
                }
                return true;
            })
            .Where(element => element.ActualWidth > 1 && element.ActualHeight > 1)
            .ToList();
        if (targets.Count == 0) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate the production sequence view for '{firstAreaEntity.Name}'.");
        }

        Rect bounds = Rect.Empty;
        foreach (FrameworkElement target in targets) {
            Point topLeft = target.TranslatePoint(new Point(0, 0), content);
            Rect targetBounds = new(topLeft, new Size(target.ActualWidth, target.ActualHeight));
            bounds.Union(targetBounds);
        }
        UnionOpenOverlayBounds(content, ref bounds, renderWidth, renderHeight);
        double availableRight = renderWidth;
        TabControl? sidebar = FindVisualDescendants<TabControl>(content).FirstOrDefault(control => control.Items.Count == 5);
        if (sidebar is not null) {
            double sidebarLeft = sidebar.TranslatePoint(new Point(0, 0), content).X;
            if (sidebarLeft > bounds.Left && bounds.Right <= sidebarLeft + 1) {
                bounds.Intersect(new Rect(bounds.Left, bounds.Top, sidebarLeft - bounds.Left, bounds.Height));
                availableRight = sidebarLeft;
            }
        }
        bounds = ExpandBoundsToAspect(
            bounds,
            asset.Width / (double)asset.Height,
            new Rect(0, 0, availableRight, renderHeight),
            asset.Id);
        double left = Math.Clamp(bounds.Left, 0, renderWidth - 1);
        double top = Math.Clamp(bounds.Top, 0, renderHeight - 1);
        double width = Math.Clamp(bounds.Right - left, 1, renderWidth - left);
        double height = Math.Clamp(bounds.Bottom - top, 1, renderHeight - top);
        return new ScreenshotCrop {
            X = left / renderWidth,
            Y = top / renderHeight,
            Width = width / renderWidth,
            Height = height / renderHeight
        };
    }

    private static ScreenshotCrop ResolveSimpleSequencerCrop(
            FrameworkElement content,
            FrameworkElement fixture,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        if (fixture is not NINA.View.SimpleSequencer.SimpleSequenceView
            || fixture.DataContext is not NINA.ViewModel.Interfaces.ISimpleSequenceVM viewModel
            || viewModel.SelectedTarget is not NINA.Sequencer.Container.SimpleDSOContainer selectedTarget) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' requested a Legacy Sequencer crop without NINA's production SimpleSequenceView and selected target.");
        }

        NINA.View.SimpleSequencer.SimpleDSOContainerView targetView =
            FindVisualDescendants<NINA.View.SimpleSequencer.SimpleDSOContainerView>(fixture)
                .FirstOrDefault(candidate => ReferenceEquals(candidate.DataContext, selectedTarget))
            ?? throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer target view.");
        DataGrid exposureGrid = FindVisualDescendants<DataGrid>(targetView)
            .FirstOrDefault(candidate => ReferenceEquals(candidate.DataContext, selectedTarget))
            ?? throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer exposure grid.");
        List<Expander> targetExpanders = FindVisualDescendants<Expander>(targetView)
            .Where(expander => ReferenceEquals(expander.DataContext, selectedTarget) && expander.IsVisible)
            .OrderBy(expander => expander.TranslatePoint(new Point(0, 0), targetView).Y)
            .ToList();
        if (targetExpanders.Count != 2) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' expected the two production Legacy Sequencer target expanders but found {targetExpanders.Count}.");
        }

        Rect bounds;
        Rect cropAvailable = new(0, 0, renderWidth, renderHeight);
        bool preserveMeasuredBounds = false;
        switch (asset.CropTarget) {
            case "simple:set-options": {
                List<Expander> setOptions = FindVisualDescendants<Expander>(fixture)
                    .Where(expander => expander.DataContext is
                        NINA.ViewModel.Sequencer.SimpleSequence.SimpleStartContainer or
                        NINA.ViewModel.Sequencer.SimpleSequence.SimpleEndContainer)
                    .Where(expander => expander.IsVisible)
                    .ToList();
                if (setOptions.Count != 2) {
                    throw new CatalogException(
                        $"Screenshot '{asset.Id}' expected the two production Legacy Sequencer set option expanders but found {setOptions.Count}.");
                }
                bounds = UnionBounds(setOptions, content);
                preserveMeasuredBounds = true;
                break;
            }
            case "simple:target-tabs": {
                ListView tabs = FindVisualDescendants<ListView>(fixture)
                    .FirstOrDefault(list => list.Items.Count > 0
                        && list.Items.Cast<object>().All(item => item is NINA.Sequencer.Container.SimpleDSOContainer))
                    ?? throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer target tabs.");
                Grid tabsRegion = VisualAncestors(tabs).OfType<Grid>()
                    .Where(grid => grid.IsVisible
                        && grid.ActualWidth >= tabs.ActualWidth
                        && grid.ActualHeight >= tabs.ActualHeight
                        && grid.ActualHeight <= 100)
                    .OrderByDescending(grid => grid.ActualWidth)
                    .FirstOrDefault()
                    ?? throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer target tab toolbar.");
                bounds = ElementBounds(tabsRegion, content);
                preserveMeasuredBounds = true;
                break;
            }
            case "simple:target-general": {
                Point origin = targetView.TranslatePoint(new Point(0, 0), content);
                double expanderTop = targetExpanders[0].TranslatePoint(new Point(0, 0), targetView).Y;
                bounds = new Rect(origin.X, origin.Y, targetView.ActualWidth / 2, expanderTop);
                cropAvailable = bounds;
                preserveMeasuredBounds = true;
                break;
            }
            case "simple:target-information": {
                Point origin = targetView.TranslatePoint(new Point(0, 0), content);
                double gridTop = exposureGrid.TranslatePoint(new Point(0, 0), targetView).Y;
                bounds = new Rect(
                    origin.X + targetView.ActualWidth / 2,
                    origin.Y,
                    targetView.ActualWidth / 2,
                    gridTop);
                cropAvailable = bounds;
                preserveMeasuredBounds = true;
                break;
            }
            case "simple:target-options":
                bounds = ElementBounds(targetExpanders[0], content);
                break;
            case "simple:autofocus": {
                List<FrameworkElement> autofocusControls =
                    FindVisualDescendants<FrameworkElement>(targetExpanders[1])
                        .Where(element => element.IsVisible
                            && element.ActualWidth > 1
                            && element.ActualHeight > 1
                            && element is TextBlock or TextBox or CheckBox)
                        .ToList();
                if (autofocusControls.Count == 0) {
                    throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer autofocus controls.");
                }
                bounds = UnionBounds(autofocusControls, content);
                break;
            }
            case "simple:imaging-details": {
                List<FrameworkElement> rows = FindVisualDescendants<DataGridRow>(exposureGrid)
                    .Where(row => row.IsVisible
                        && selectedTarget.Items.Any(item => ReferenceEquals(item, row.DataContext)))
                    .Cast<FrameworkElement>()
                    .ToList();
                DataGridColumnHeadersPresenter header =
                    FindVisualDescendants<DataGridColumnHeadersPresenter>(exposureGrid).FirstOrDefault()
                    ?? throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer exposure headers.");
                rows.Add(header);
                bounds = UnionBounds(rows, content);
                double gridTop = exposureGrid.TranslatePoint(new Point(0, 0), content).Y;
                cropAvailable = new Rect(0, gridTop, renderWidth, renderHeight - gridTop);
                preserveMeasuredBounds = true;
                break;
            }
            case "simple:transform-button": {
                Button buildButton = FindVisualDescendants<Button>(fixture)
                    .FirstOrDefault(button => ReferenceEquals(button.Command, viewModel.BuildSequenceCommand))
                    ?? throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Build Sequence button.");
                StackPanel actionRegion = VisualAncestors(buildButton).OfType<StackPanel>()
                    .FirstOrDefault(panel => panel.IsVisible
                        && panel.Orientation == Orientation.Horizontal
                        && panel.HorizontalAlignment == HorizontalAlignment.Right)
                    ?? throw new CatalogException(
                        $"Screenshot '{asset.Id}' could not locate NINA's production Legacy Sequencer action toolbar.");
                bounds = ElementBounds(actionRegion, content);
                break;
            }
            default:
                throw new CatalogException(
                    $"Screenshot '{asset.Id}' has unknown Legacy Sequencer crop target '{asset.CropTarget}'.");
        }

        bounds.Inflate(6, 6);
        bounds.Intersect(cropAvailable);
        if (preserveMeasuredBounds) {
            return new ScreenshotCrop {
                X = bounds.Left / renderWidth,
                Y = bounds.Top / renderHeight,
                Width = bounds.Width / renderWidth,
                Height = bounds.Height / renderHeight
            };
        }
        return BoundsToCrop(bounds, asset, renderWidth, renderHeight, cropAvailable);
    }

    private static Rect UnionBounds(IEnumerable<FrameworkElement> elements, FrameworkElement relativeTo) {
        Rect bounds = Rect.Empty;
        foreach (FrameworkElement element in elements) {
            bounds.Union(ElementBounds(element, relativeTo));
        }
        return bounds;
    }

    private static Rect ElementBounds(FrameworkElement element, FrameworkElement relativeTo) {
        Point topLeft = element.TranslatePoint(new Point(0, 0), relativeTo);
        return new Rect(topLeft, new Size(element.ActualWidth, element.ActualHeight));
    }

    private static ScreenshotCrop ResolveFilteredSidebarItemCrop(
            FrameworkElement content,
            FrameworkElement fixture,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        if (fixture.DataContext is not ISequence2VM viewModel
            || string.IsNullOrWhiteSpace(viewModel.SequencerFactory.ViewFilter)) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested a filtered sidebar item without a production sidebar filter.");
        }
        string expectedName = viewModel.SequencerFactory.ViewFilter;
        TextBlock label = FindVisualDescendants<TextBlock>(content)
            .FirstOrDefault(text => text.IsVisible
                && string.Equals(text.Text, expectedName, StringComparison.Ordinal))
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not locate the filtered production sidebar item '{expectedName}'.");
        FrameworkElement row = VisualAncestors(label)
            .OfType<Grid>()
            .Where(grid => grid.IsVisible
                && grid.ActualHeight >= 25
                && grid.ActualHeight <= 45
                && grid.ActualWidth >= 200)
            .OrderByDescending(grid => grid.ActualWidth)
            .FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not locate the production sidebar row for '{expectedName}'.");
        Point topLeft = row.TranslatePoint(new Point(0, 0), content);
        Rect bounds = new(topLeft, new Size(row.ActualWidth, row.ActualHeight));
        return BoundsToCrop(bounds, asset, renderWidth, renderHeight);
    }

    private static ScreenshotCrop ResolveExpressionCrop(
            FrameworkElement content,
            FrameworkElement fixture,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        if (fixture.DataContext is not ISequence2VM viewModel
            || viewModel.Sequencer.MainContainer.Items.ElementAtOrDefault(1) is not ISequenceContainer targetArea
            || targetArea.Items.FirstOrDefault() is not ISequenceItem firstAreaEntity) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested an expression crop but the production target area is empty.");
        }
        NINA.Sequencer.SequenceItem.Imaging.TakeExposure exposure = CollectSequenceEntities(firstAreaEntity)
            .OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>()
            .FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find a production Take Exposure instruction for its expression crop.");
        NINA.Sequencer.Logic.Expression expression = asset.CropTarget!.EndsWith(":gain", StringComparison.Ordinal)
            ? exposure.GainExpression
            : exposure.ExposureTimeExpression;
        NINA.Sequencer.Logic.ExprControl control = FindVisualDescendants<NINA.Sequencer.Logic.ExprControl>(content)
            .Where(candidate => candidate.IsVisible
                && candidate.ActualWidth > 1
                && candidate.ActualHeight > 1)
            .FirstOrDefault(candidate =>
                ReferenceEquals(candidate.DataContext, exposure)
                && (ReferenceEquals(
                        candidate.GetValue(NINA.Sequencer.Logic.ExprControl.ExpProperty),
                        expression)
                    || string.Equals(
                        candidate.GetValue(NINA.Sequencer.Logic.ExprControl.LabelProperty) as string,
                        asset.CropTarget!.EndsWith(":gain", StringComparison.Ordinal)
                            ? NINA.Core.Locale.Loc.Instance["LblGain"]
                            : NINA.Core.Locale.Loc.Instance["LblTime"],
                        StringComparison.Ordinal)))
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not locate NINA's production expression control.");
        Point topLeft = control.TranslatePoint(new Point(0, 0), content);
        Rect bounds = new(topLeft, new Size(control.ActualWidth, control.ActualHeight));
        UnionOpenOverlayBounds(content, ref bounds, renderWidth, renderHeight);
        return BoundsToCrop(bounds, asset, renderWidth, renderHeight);
    }

    private static void UnionOpenOverlayBounds(
            FrameworkElement content,
            ref Rect bounds,
            int renderWidth,
            int renderHeight) {
        foreach (ComboBox comboBox in FindVisualDescendants<ComboBox>(content)
            .Where(control => control.IsDropDownOpen || NamedStateController.ShouldCaptureDropDown(control))) {
            comboBox.ApplyTemplate();
            Popup? popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup
                ?? comboBox.Template.FindName("Popup", comboBox) as Popup;
            if (popup?.Child is not FrameworkElement popupChild) {
                continue;
            }
            popupChild.Measure(new Size(renderWidth, renderHeight));
            Size popupSize = popupChild.DesiredSize;
            popupChild.Arrange(new Rect(
                0,
                0,
                Math.Max(comboBox.ActualWidth, popupSize.Width),
                Math.Max(1, popupSize.Height)));
            popupChild.UpdateLayout();
            popupSize = GetRenderedSize(popupChild);
            Point popupOrigin = comboBox.TranslatePoint(new Point(0, comboBox.ActualHeight), content);
            bounds.Union(new Rect(popupOrigin, popupSize));
        }
        foreach (Button button in FindVisualDescendants<Button>(content)
            .Where(control => control.ContextMenu?.IsOpen == true)) {
            ContextMenu menu = button.ContextMenu!;
            menu.Measure(new Size(renderWidth, renderHeight));
            Size menuSize = GetRenderedSize(menu);
            Point menuOrigin = GetContextMenuOrigin(button, menu, menuSize.Width, content, renderWidth);
            bounds.Union(new Rect(menuOrigin, menuSize));
        }
        foreach (FrameworkElement anchor in FindVisualDescendants<FrameworkElement>(content)
            .Where(element => element.ToolTip is ToolTip { IsOpen: true })) {
            ToolTip tooltip = (ToolTip)anchor.ToolTip;
            Size tooltipSize = GetRenderedSize(tooltip);
            Point tooltipOrigin = GetToolTipOrigin(
                anchor,
                tooltip,
                tooltipSize,
                content,
                renderWidth,
                renderHeight);
            bounds.Union(new Rect(tooltipOrigin, tooltipSize));
        }
    }

    private static ScreenshotCrop BoundsToCrop(
            Rect bounds,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight,
            Rect? availableBounds = null) {
        Rect available = availableBounds ?? new Rect(0, 0, renderWidth, renderHeight);
        bounds = ExpandBoundsToAspect(
            bounds,
            asset.Width / (double)asset.Height,
            available,
            asset.Id);
        double left = Math.Clamp(bounds.Left, 0, renderWidth - 1);
        double top = Math.Clamp(bounds.Top, 0, renderHeight - 1);
        double width = Math.Clamp(bounds.Right - left, 1, renderWidth - left);
        double height = Math.Clamp(bounds.Bottom - top, 1, renderHeight - top);
        return new ScreenshotCrop {
            X = left / renderWidth,
            Y = top / renderHeight,
            Width = width / renderWidth,
            Height = height / renderHeight
        };
    }

    private static IEnumerable<DependencyObject> VisualAncestors(DependencyObject element) {
        DependencyObject? current = VisualTreeHelper.GetParent(element);
        while (current is not null) {
            yield return current;
            current = VisualTreeHelper.GetParent(current);
        }
    }

    private static ScreenshotCrop ResolveSettingsGroupCrop<TSettings>(
            FrameworkElement content,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        GroupBox group = FindVisualDescendants<GroupBox>(content)
            .FirstOrDefault(candidate => candidate.DataContext is TSettings
                && candidate.ActualWidth > 1
                && candidate.ActualHeight > 1)
            ?? throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate the production settings group for '{typeof(TSettings).Name}'.");
        Point topLeft = group.TranslatePoint(new Point(0, 0), content);
        Rect bounds = new(topLeft, new Size(group.ActualWidth, group.ActualHeight));
        bounds.Inflate(8, 8);
        bounds.Intersect(new Rect(0, 0, renderWidth, renderHeight));
        bounds = ExpandBoundsToAspect(
            bounds,
            asset.Width / (double)asset.Height,
            new Rect(0, 0, renderWidth, renderHeight),
            asset.Id);
        return new ScreenshotCrop {
            X = bounds.Left / renderWidth,
            Y = bounds.Top / renderHeight,
            Width = bounds.Width / renderWidth,
            Height = bounds.Height / renderHeight
        };
    }

    private static ScreenshotCrop ResolveFramingCrop(
            FrameworkElement content,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        FrameworkElement? marker = asset.CropTarget switch {
            "framing:image-source" => (FrameworkElement?)FindVisualDescendants<ComboBox>(content)
                .FirstOrDefault(control => control.Name == "PART_FramingAssistantSource"),
            "framing:coordinates" => (FrameworkElement?)FindVisualDescendants<TextBox>(content)
                .FirstOrDefault(control => GetBindingPath(control, TextBox.TextProperty) == "DeepSkyObjectSearchVM.TargetName"),
            "framing:mosaic-plan" => (FrameworkElement?)FindVisualDescendants<ListView>(content)
                .FirstOrDefault(control => GetBindingPath(control, ItemsControl.ItemsSourceProperty) == "CameraRectangles"),
            _ => null
        };
        if (marker is null) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate its production Framing Assistant crop marker '{asset.CropTarget}'.");
        }
        GroupBox group = FindVisualAncestors<GroupBox>(marker).FirstOrDefault()
            ?? throw new CatalogException(
                $"Screenshot '{asset.Id}' could not locate the production Framing Assistant group for '{asset.CropTarget}'.");
        Point topLeft = group.TranslatePoint(new Point(0, 0), content);
        Rect bounds = new(topLeft, new Size(group.ActualWidth, group.ActualHeight));
        if (asset.CropTarget == "framing:image-source") {
            foreach (Popup popup in FindVisualDescendants<Popup>(content).Where(candidate => candidate.IsOpen)) {
                if (popup.Child is not FrameworkElement child || popup.PlacementTarget is not FrameworkElement anchor) {
                    continue;
                }
                Point origin = anchor.TranslatePoint(new Point(0, anchor.ActualHeight), content);
                bounds.Union(new Rect(origin, GetRenderedSize(child)));
            }
        }
        bounds.Inflate(8, 8);
        bounds.Intersect(new Rect(0, 0, renderWidth, renderHeight));
        bounds = ExpandBoundsToAspect(
            bounds,
            asset.Width / (double)asset.Height,
            new Rect(0, 0, renderWidth, renderHeight),
            asset.Id);
        return new ScreenshotCrop {
            X = bounds.Left / renderWidth,
            Y = bounds.Top / renderHeight,
            Width = bounds.Width / renderWidth,
            Height = bounds.Height / renderHeight
        };
    }

    private static string? GetBindingPath(DependencyObject target, DependencyProperty property) =>
        System.Windows.Data.BindingOperations.GetBinding(target, property)?.Path?.Path;

    private static IEnumerable<T> FindVisualAncestors<T>(DependencyObject source) where T : DependencyObject {
        DependencyObject? current = VisualTreeHelper.GetParent(source);
        while (current is not null) {
            if (current is T result) {
                yield return result;
            }
            current = VisualTreeHelper.GetParent(current);
        }
    }

    private static Rect ExpandBoundsToAspect(Rect bounds, double targetAspect, Rect available, string screenshotId) {
        if (bounds.IsEmpty || targetAspect <= 0 || available.IsEmpty) {
            return bounds;
        }

        double width = bounds.Width;
        double height = bounds.Height;
        if (width / height > targetAspect) {
            height = width / targetAspect;
        } else {
            width = height * targetAspect;
        }

        if (width > available.Width) {
            width = available.Width;
            height = width / targetAspect;
        }
        if (height > available.Height) {
            height = available.Height;
            width = height * targetAspect;
        }
        if (width + 0.01 < bounds.Width || height + 0.01 < bounds.Height) {
            throw new CatalogException(
                $"Screenshot '{screenshotId}' requested an output aspect ratio that cannot contain its real UI crop " +
                $"(crop {bounds.Width:F0}x{bounds.Height:F0}, available {available.Width:F0}x{available.Height:F0}). " +
                "Increase renderWidth or renderHeight instead of clipping or distorting the view.");
        }

        double x = bounds.Left + (bounds.Width - width) / 2;
        double y = bounds.Top + (bounds.Height - height) / 2;
        x = Math.Clamp(x, available.Left, available.Right - width);
        y = Math.Clamp(y, available.Top, available.Bottom - height);
        return new Rect(x, y, width, height);
    }

    private static ScreenshotCrop ResolveRootAddMenuCrop(
            FrameworkElement content,
            ScreenshotAsset asset,
            int renderWidth,
            int renderHeight) {
        Point contentOrigin = content.PointToScreen(new Point(0, 0));
        DpiScale dpi = VisualTreeHelper.GetDpi(content);
        Rect bounds = Rect.Empty;
        foreach (HwndSource source in GetPopupSources()) {
            if (!GetWindowRect(source.Handle, out WindowRect popup)) {
                continue;
            }
            bounds.Union(new Rect(
                (popup.Left - contentOrigin.X) / dpi.DpiScaleX,
                (popup.Top - contentOrigin.Y) / dpi.DpiScaleY,
                (popup.Right - popup.Left) / dpi.DpiScaleX,
                (popup.Bottom - popup.Top) / dpi.DpiScaleY));
        }
        if (bounds.IsEmpty) {
            throw new CatalogException($"Screenshot '{asset.Id}' requested a root add menu crop but no production popup bounds were available.");
        }
        Rect available = new(0, 0, renderWidth, renderHeight);
        if (!available.Contains(bounds)) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' cannot contain its complete production add menu inside the configured " +
                $"{renderWidth}x{renderHeight} render canvas (menu bounds: {bounds}). " +
                "Increase renderWidth or renderHeight instead of clipping the popup.");
        }
        bounds.Inflate(8, 8);
        bounds.Intersect(available);
        bounds = ExpandBoundsToAspect(bounds, asset.Width / (double)asset.Height, available, asset.Id);
        return new ScreenshotCrop {
            X = bounds.Left / renderWidth,
            Y = bounds.Top / renderHeight,
            Width = bounds.Width / renderWidth,
            Height = bounds.Height / renderHeight
        };
    }

    private static Point GetContextMenuOrigin(
            FrameworkElement anchor,
            ContextMenu menu,
            double menuWidth,
            FrameworkElement content,
            double renderWidth) {
        try {
            Point contentOrigin = content.PointToScreen(new Point(0, 0));
            Point menuOrigin = menu.PointToScreen(new Point(0, 0));
            DpiScale dpi = VisualTreeHelper.GetDpi(content);
            Point relative = new(
                (menuOrigin.X - contentOrigin.X) / dpi.DpiScaleX,
                (menuOrigin.Y - contentOrigin.Y) / dpi.DpiScaleY);
            if (double.IsFinite(relative.X)
                && double.IsFinite(relative.Y)
                && relative.X >= 0
                && relative.X <= renderWidth
                && relative.Y >= 0
                && relative.Y <= content.ActualHeight) {
                return relative;
            }
        } catch (InvalidOperationException) {
            // Fall back to the anchor when WPF has not yet created the popup presentation source.
        }
        Point anchorTopLeft = anchor.TranslatePoint(new Point(0, 0), content);
        double menuHeight = GetRenderedSize(menu).Height;
        Point origin = menu.Placement switch {
            PlacementMode.Left => new Point(anchorTopLeft.X - menuWidth, anchorTopLeft.Y),
            PlacementMode.Right => new Point(anchorTopLeft.X + anchor.ActualWidth, anchorTopLeft.Y),
            PlacementMode.Top => new Point(anchorTopLeft.X, anchorTopLeft.Y - menuHeight),
            _ => new Point(anchorTopLeft.X, anchorTopLeft.Y + anchor.ActualHeight)
        };
        if (origin.X + menuWidth > renderWidth) {
            origin.X = Math.Max(0, anchorTopLeft.X + anchor.ActualWidth - menuWidth);
        }
        origin.X = Math.Max(0, origin.X);
        origin.Y = Math.Max(0, origin.Y);
        return origin;
    }

    private static Point GetToolTipOrigin(
            FrameworkElement anchor,
            ToolTip tooltip,
            Size tooltipSize,
            FrameworkElement content,
            double renderWidth,
            double renderHeight) {
        Point anchorOrigin = anchor.TranslatePoint(new Point(0, 0), content);
        Point origin = tooltip.Placement switch {
            PlacementMode.Right => new Point(anchorOrigin.X + anchor.ActualWidth, anchorOrigin.Y),
            PlacementMode.Left => new Point(anchorOrigin.X - tooltipSize.Width, anchorOrigin.Y),
            PlacementMode.Top => new Point(anchorOrigin.X, anchorOrigin.Y - tooltipSize.Height),
            _ => new Point(anchorOrigin.X, anchorOrigin.Y + anchor.ActualHeight)
        };
        origin.Offset(tooltip.HorizontalOffset, tooltip.VerticalOffset);

        if (origin.X + tooltipSize.Width > renderWidth && anchorOrigin.X >= tooltipSize.Width) {
            origin.X = anchorOrigin.X - tooltipSize.Width;
        }
        if (origin.Y + tooltipSize.Height > renderHeight && anchorOrigin.Y >= tooltipSize.Height) {
            origin.Y = anchorOrigin.Y - tooltipSize.Height;
        }
        origin.X = Math.Clamp(origin.X, 0, Math.Max(0, renderWidth - tooltipSize.Width));
        origin.Y = Math.Clamp(origin.Y, 0, Math.Max(0, renderHeight - tooltipSize.Height));
        return origin;
    }

    private static Size GetRenderedSize(FrameworkElement root) {
        IEnumerable<FrameworkElement> elements = FindVisualDescendants<FrameworkElement>(root).Prepend(root);
        return new Size(
            Math.Max(1, elements.Max(element => Math.Max(element.ActualWidth, element.DesiredSize.Width))),
            Math.Max(1, elements.Max(element => Math.Max(element.ActualHeight, element.DesiredSize.Height))));
    }


    private static HashSet<NINA.Sequencer.ISequenceEntity> CollectSequenceEntities(NINA.Sequencer.ISequenceEntity root) {
        HashSet<NINA.Sequencer.ISequenceEntity> result = new(ReferenceEqualityComparer.Instance);
        Collect(root, result);
        return result;

        static void Collect(NINA.Sequencer.ISequenceEntity entity, ISet<NINA.Sequencer.ISequenceEntity> entities) {
            if (!entities.Add(entity)) {
                return;
            }
            if (entity is ISequenceContainer container) {
                foreach (ISequenceItem item in container.Items) {
                    Collect(item, entities);
                }
            }
            if (entity is IConditionable conditionable) {
                foreach (ISequenceCondition condition in conditionable.Conditions) {
                    Collect(condition, entities);
                }
            }
            if (entity is ITriggerable triggerable) {
                foreach (ISequenceTrigger trigger in triggerable.Triggers) {
                    Collect(trigger, entities);
                }
            }
        }
    }

    private static void ValidateNonBlank(BitmapSource bitmap, string id) {
        int stride = bitmap.PixelWidth * 4;
        byte[] pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);
        uint first = BitConverter.ToUInt32(pixels, 0);
        bool differs = false;
        for (int index = 4; index < pixels.Length; index += 4) {
            if (BitConverter.ToUInt32(pixels, index) != first) {
                differs = true;
                break;
            }
        }
        if (!differs) {
            throw new CatalogException($"Screenshot '{id}' rendered as a single blank color.");
        }
    }

    private static void DrainDispatcher() {
        DispatcherFrame frame = new();
        bool timedOut = false;
        DispatcherTimer timeout = new(DispatcherPriority.Send) {
            Interval = TimeSpan.FromSeconds(5)
        };
        timeout.Tick += (_, _) => {
            timedOut = true;
            frame.Continue = false;
        };
        timeout.Start();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        timeout.Stop();
        if (timedOut) {
            string sources = string.Join(", ", PresentationSource.CurrentSources
                .OfType<HwndSource>()
                .Select(source => $"{source.RootVisual?.GetType().Name ?? "none"}@{source.Handle}"));
            throw new CatalogException($"The WPF dispatcher did not become idle within five seconds. Active presentation sources: {sources}.");
        }
    }

    private sealed class BindingTraceScope : TraceListener, IDisposable {
        private readonly string screenshotId;
        private readonly SourceLevels originalLevel;
        private readonly List<string> errors = [];

        public BindingTraceScope(string screenshotId) {
            this.screenshotId = screenshotId;
            originalLevel = PresentationTraceSources.DataBindingSource.Switch.Level;
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            PresentationTraceSources.DataBindingSource.Listeners.Add(this);
        }

        public override void Write(string? message) {
            if (!string.IsNullOrWhiteSpace(message)) {
                errors.Add(message.Trim());
            }
        }

        public override void WriteLine(string? message) => Write(message);

        public void ThrowIfErrors() {
            if (errors.Count > 0) {
                string message = string.Join(" ", errors.Take(3));
                throw new CatalogException($"Screenshot '{screenshotId}' has binding errors: {message}");
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(this);
                PresentationTraceSources.DataBindingSource.Switch.Level = originalLevel;
            }
            base.Dispose(disposing);
        }
    }

    private sealed class RenderWindow : Window, IDisposable {
        public void Dispose() {
            if (Content is DependencyObject root) {
                CloseOpenMenus(root);
            }
            Close();
            DrainDispatcher();
        }
    }

    private sealed class CursorPositionScope : IDisposable {
        private readonly WindowPoint originalPosition;
        private readonly WindowRect originalClip;
        private bool disposed;

        private CursorPositionScope(WindowPoint originalPosition, WindowRect originalClip) {
            this.originalPosition = originalPosition;
            this.originalClip = originalClip;
        }

        public static CursorPositionScope MoveAway(string screenshotId) {
            if (!GetCursorPos(out WindowPoint originalPosition) || !GetClipCursor(out WindowRect originalClip)) {
                throw new CatalogException($"Screenshot '{screenshotId}' could not preserve the desktop cursor for deterministic menu capture.");
            }
            WindowRect captureClip = new() { Left = 0, Top = 0, Right = 1, Bottom = 1 };
            if (!ClipCursor(ref captureClip) || !SetCursorPos(0, 0)) {
                _ = ClipCursor(ref originalClip);
                throw new CatalogException($"Screenshot '{screenshotId}' could not move the desktop cursor away from the production menu.");
            }
            return new CursorPositionScope(originalPosition, originalClip);
        }

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;
            WindowRect restoredClip = originalClip;
            _ = ClipCursor(ref restoredClip);
            _ = SetCursorPos(originalPosition.X, originalPosition.Y);
        }
    }
}
