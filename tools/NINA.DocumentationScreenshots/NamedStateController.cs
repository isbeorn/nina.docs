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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Xaml.Behaviors;
using NINA.Core.Enum;
using NINA.Core.Locale;
using NINA.Sequencer;
using NINA.Sequencer.Behaviors;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem.Imaging;

namespace NINA.DocumentationScreenshots;

/// <summary>
/// Applies catalog states by manipulating controls from NINA's compiled production visual tree.
/// </summary>
public static class NamedStateController {
    private static readonly DependencyProperty CaptureDropDownProperty = DependencyProperty.RegisterAttached(
        "CaptureDropDown",
        typeof(bool),
        typeof(NamedStateController),
        new PropertyMetadata(false));
    internal static bool ShouldCaptureDropDown(ComboBox comboBox) =>
        (bool)comboBox.GetValue(CaptureDropDownProperty);

    public static void Apply(FrameworkElement fixture, ScreenshotAsset asset) {
        ResetProductionMenuMode(fixture);
        string output = asset.Output.Replace('\\', '/').ToLowerInvariant();
        ApplyAdvancedSequencerSidebarState(fixture, output, asset.Id);
        PinAltitudeChartNowMarkers(fixture);
        ApplyAdvancedSequencerDragState(fixture, output, asset.Id);
        ApplySimpleSequencerState(fixture, asset);

        if (output.EndsWith("/sequencer/trigger/customtrigger.png", StringComparison.Ordinal)) {
            Expander trigger = FindDescendants<Expander>(fixture)
                .Where(expander => expander.GetType().Name == "DetachingExpander"
                    && expander.DataContext is NINA.Sequencer.Trigger.Utility.CustomTrigger
                    && expander.IsVisible)
                .OrderByDescending(expander => expander.ActualWidth * expander.ActualHeight)
                .FirstOrDefault()
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find the production Custom Trigger editor.");
            trigger.IsExpanded = true;
            trigger.UpdateLayout();
        }

        if (output.EndsWith("/sequencer/sequencer_addtrigger.png", StringComparison.Ordinal)) {
            OpenProductionContextMenu(fixture, "AddTriggerButton", asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_addloopcondition.png", StringComparison.Ordinal)) {
            OpenProductionContextMenu(fixture, "AddConditionButton", asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_addinstruction.png", StringComparison.Ordinal)) {
            OpenProductionRootMenu(fixture, "Camera", asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_addinstructionset.png", StringComparison.Ordinal)) {
            OpenProductionRootMenu(fixture, "Instruction Set", asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_addtargettotargettab.png", StringComparison.Ordinal)) {
            OpenProductionSaveTargetTooltip(fixture, asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_issues.png", StringComparison.Ordinal)) {
            OpenProductionValidationTooltip(fixture, asset.Id);
        }

        if (output.EndsWith("/sequencer/conditions/loopuntiltime.png", StringComparison.Ordinal)) {
            ComboBox comboBox = FindDescendants<ComboBox>(fixture).FirstOrDefault()
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find the production time-provider ComboBox.");
            PrepareProductionComboBox(comboBox, asset.Id, "time-provider");
        }
        if (output.EndsWith("/sequencer/instructions/instruction_settings.png", StringComparison.Ordinal)) {
            OpenProductionInstructionSettings(fixture, asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_symbolvalues.png", StringComparison.Ordinal)) {
            OpenProductionExpressionSymbolsTooltip(fixture, asset.Id);
        }
        if (output.EndsWith("/sequencer/sequencer_undefined.png", StringComparison.Ordinal)
            || output.EndsWith("/sequencer/sequencer_expressionwarning.png", StringComparison.Ordinal)) {
            OpenProductionExpressionErrorTooltip(fixture, asset.Id);
        }
        if (output.EndsWith("/advanced/framing/imagesources.png", StringComparison.Ordinal)) {
            ComboBox source = FindDescendants<ComboBox>(fixture)
                .FirstOrDefault(control => control.Name == "PART_FramingAssistantSource")
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find the production image-source selector.");
            PrepareProductionComboBox(source, asset.Id, "image-source");
        }
    }

    private static void ApplySimpleSequencerState(FrameworkElement fixture, ScreenshotAsset asset) {
        if (asset.State is not ("legacy-simple-documentation" or "simple-to-advanced-legacy")) {
            return;
        }
        if (fixture is not NINA.View.SimpleSequencer.SimpleSequenceView
            || fixture.DataContext is not NINA.ViewModel.Interfaces.ISimpleSequenceVM viewModel) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' requested a Legacy Sequencer state without NINA's production SimpleSequenceView.");
        }

        List<Expander> targetExpanders = FindDescendants<Expander>(fixture)
            .Where(expander => ReferenceEquals(expander.DataContext, viewModel.SelectedTarget))
            .Where(expander => expander.IsVisible)
            .OrderBy(expander => expander.TranslatePoint(new Point(0, 0), fixture).Y)
            .ToList();
        if (targetExpanders.Count != 2) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' expected the two production Legacy Sequencer target expanders but found {targetExpanders.Count}.");
        }
        foreach (Expander expander in targetExpanders) {
            expander.IsExpanded = true;
        }
        fixture.UpdateLayout();
    }

    private static void OpenProductionInstructionSettings(
            FrameworkElement fixture,
            string screenshotId) {
        ComboBox errorBehavior = FindDescendants<ComboBox>(fixture)
            .FirstOrDefault(control => control.Name == "PART_ErrorBehavior"
                && control.IsVisible)
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production instruction error behavior control.");
        PrepareProductionComboBox(errorBehavior, screenshotId, "instruction error behavior");
    }

    private static void PrepareProductionComboBox(
            ComboBox comboBox,
            string screenshotId,
            string description) {
        comboBox.ApplyTemplate();
        Popup? popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup
            ?? comboBox.Template.FindName("Popup", comboBox) as Popup;
        if (popup is null) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production {description} popup.");
        }
        popup.PopupAnimation = PopupAnimation.None;
        comboBox.SetValue(CaptureDropDownProperty, true);
    }

    private static void OpenProductionExpressionSymbolsTooltip(
            FrameworkElement fixture,
            string screenshotId) {
        NINA.Sequencer.Logic.ExprControl control = FindExposureExpressionControl(
            fixture,
            screenshotId,
            false);
        TextBox input = FindDescendants<TextBox>(control).FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production expression input.");
        NINA.Sequencer.Logic.UserSymbol.ShowSymbols(input);
        OpenProductionTooltip(input, screenshotId, "expression symbols");
    }

    private static void OpenProductionExpressionErrorTooltip(
            FrameworkElement fixture,
            string screenshotId) {
        NINA.Sequencer.Logic.ExprControl control = FindExposureExpressionControl(
            fixture,
            screenshotId,
            true);
        TextBlock warning = FindDescendants<TextBlock>(control)
            .FirstOrDefault(text => text.IsVisible
                && text.ToolTip is not null
                && text.Text.Contains("\u26A0", StringComparison.Ordinal))
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production expression warning indicator.");
        OpenProductionTooltip(warning, screenshotId, "expression error");
    }

    private static NINA.Sequencer.Logic.ExprControl FindExposureExpressionControl(
            FrameworkElement fixture,
            string screenshotId,
            bool gain) {
        string expectedLabel = gain ? Loc.Instance["LblGain"] : Loc.Instance["LblTime"];
        return FindDescendants<NINA.Sequencer.Logic.ExprControl>(fixture)
            .Where(control => control.IsVisible
                && control.ActualWidth > 1
                && control.ActualHeight > 1)
            .FirstOrDefault(control => control.DataContext is TakeExposure exposure
                && (ReferenceEquals(
                        control.GetValue(NINA.Sequencer.Logic.ExprControl.ExpProperty),
                        gain ? exposure.GainExpression : exposure.ExposureTimeExpression)
                    || string.Equals(
                        control.GetValue(NINA.Sequencer.Logic.ExprControl.LabelProperty) as string,
                        expectedLabel,
                        StringComparison.Ordinal)))
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production Take Exposure expression control.");
    }

    private static void OpenProductionTooltip(
            FrameworkElement anchor,
            string screenshotId,
            string description) {
        object? productionContent = anchor.ToolTip;
        if (productionContent is null) {
            throw new CatalogException($"Screenshot '{screenshotId}' found NINA's {description} anchor without its production tooltip.");
        }
        ToolTip tooltip = productionContent as ToolTip ?? new ToolTip { Content = productionContent };
        anchor.ToolTip = tooltip;
        tooltip.PlacementTarget = anchor;
        tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        tooltip.IsOpen = true;
        tooltip.UpdateLayout();
        anchor.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        if (!tooltip.IsOpen) {
            throw new CatalogException($"Screenshot '{screenshotId}' opened NINA's production {description} tooltip but WPF closed it before capture.");
        }
    }

    private static void PinAltitudeChartNowMarkers(FrameworkElement fixture) {
        double fixedNow = OxyPlot.Axes.DateTimeAxis.ToDouble(DocumentationApplicationHost.FixedDateTime.Now);
        foreach (OxyPlot.Wpf.LineAnnotation marker in FindDescendants<OxyPlot.Wpf.Plot>(fixture)
            .SelectMany(plot => plot.Annotations.OfType<OxyPlot.Wpf.LineAnnotation>())
            .Where(annotation => System.Windows.Data.BindingOperations
                .GetBinding(annotation, OxyPlot.Wpf.LineAnnotation.XProperty)
                ?.Path.Path == "Data.Ticker.OxyNow")) {
            System.Windows.Data.BindingOperations.ClearBinding(marker, OxyPlot.Wpf.LineAnnotation.XProperty);
            marker.X = fixedNow;
        }
    }

    private static void ApplyAdvancedSequencerDragState(
            FrameworkElement fixture,
            string output,
            string screenshotId) {
        if (!ContainsAny(
                output,
                "sequencer_dragdrop.png",
                "sequencer_addtarget.png",
                "sequencer_applytarget.png",
                "sequencer_droptargettotab.png",
                "sequencer_saveastemplatedragdrop.png")) {
            return;
        }
        if (fixture.DataContext is not NINA.ViewModel.Sequencer.ISequence2VM viewModel) {
            throw new CatalogException($"Screenshot '{screenshotId}' requested a sequencer drag state without NINA's production sequencer view model.");
        }

        Grid layoutRoot = FindDescendants<Grid>(fixture)
            .FirstOrDefault(grid => ReferenceEquals(VisualTreeHelper.GetParent(grid), fixture))
            ?? FindDescendants<Grid>(fixture).FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find the production sequencer layout grid.");
        List<(FrameworkElement Element, DragOverBehavior Behavior)> dropTargets = FindDescendants<FrameworkElement>(fixture)
            .Where(element => element.IsVisible && element.ActualWidth > 1 && element.ActualHeight > 1)
            .SelectMany(element => Interaction.GetBehaviors(element)
                .OfType<DragOverBehavior>()
                .Select(behavior => (element, behavior)))
            .ToList();
        List<FrameworkElement> dragSources = FindDescendants<FrameworkElement>(fixture)
            .Where(element => element.IsVisible && element.ActualWidth > 1 && element.ActualHeight > 1)
            .Where(element => Interaction.GetBehaviors(element).OfType<DragDropBehavior>().Any())
            .ToList();

        FrameworkElement source;
        (FrameworkElement Element, DragOverBehavior Behavior) target;
        if (output.EndsWith("/sequencer/sequencer_dragdrop.png", StringComparison.Ordinal)) {
            ISequenceContainer startArea = (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[0];
            SequentialContainer instructionSet = startArea.Items.OfType<SequentialContainer>().Single();
            source = dragSources.FirstOrDefault(element =>
                    element.DataContext is TakeExposure)
                ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production Take Exposure sidebar item.");
            source = ExpandProductionDragSource(source);
            target = FindDropTarget(
                dropTargets,
                candidate => ReferenceEquals(candidate.Element.DataContext, instructionSet)
                    && candidate.Behavior.AllowDragCenter
                    && string.Equals(
                        candidate.Behavior.DragOverCenterText,
                        Loc.Instance["LblDragOver_CenterText"],
                        StringComparison.Ordinal),
                screenshotId,
                "empty instruction set");
        } else if (output.EndsWith("/sequencer/sequencer_saveastemplatedragdrop.png", StringComparison.Ordinal)) {
            ISequenceContainer targetArea = (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
            ISequenceContainer sequence = targetArea.Items.OfType<ISequenceContainer>().Single();
            source = FindContainerDragSource(dragSources, sequence, screenshotId);
            target = FindDropTarget(
                dropTargets,
                candidate => string.Equals(
                    candidate.Behavior.DragOverCenterText,
                    Loc.Instance["LblDragOver_AddTemplate"],
                    StringComparison.Ordinal),
                screenshotId,
                "template sidebar");
        } else {
            TargetSequenceContainer savedTarget = dragSources
                .Select(element => element.DataContext)
                .OfType<TargetSequenceContainer>()
                .FirstOrDefault()
                ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production saved-target item.");
            source = dragSources.First(element => ReferenceEquals(element.DataContext, savedTarget));
            source = ExpandProductionDragSource(source);
            if (output.EndsWith("/sequencer/sequencer_addtarget.png", StringComparison.Ordinal)) {
                ISequenceContainer targetArea = (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
                target = FindDropTarget(
                    dropTargets,
                    candidate => ReferenceEquals(candidate.Element.DataContext, targetArea)
                        && candidate.Behavior.AllowDragCenter,
                    screenshotId,
                    "target area");
            } else if (output.EndsWith("/sequencer/sequencer_applytarget.png", StringComparison.Ordinal)) {
                target = FindDropTarget(
                    dropTargets,
                    candidate => string.Equals(
                        candidate.Behavior.DragOverCenterText,
                        Loc.Instance["Lbl_SequenceContainer_DeepSkyObjectContainer_UpdateTarget"],
                        StringComparison.Ordinal),
                    screenshotId,
                    "deep-sky target header");
            } else {
                ISequenceContainer targetArea = (ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
                ISequenceContainer sequence = targetArea.Items.OfType<ISequenceContainer>().Single();
                source = FindContainerDragSource(dragSources, sequence, screenshotId);
                target = FindDropTarget(
                    dropTargets,
                    candidate => string.Equals(
                        candidate.Behavior.DragOverCenterText,
                        Loc.Instance["Lbl_Sequencer_TargetSidebar_DragOver_AddTarget"],
                        StringComparison.Ordinal),
                    screenshotId,
                    "target sidebar");
            }
        }

        AddProductionDragAdorners(layoutRoot, source, target.Element, target.Behavior, screenshotId);
    }

    private static (FrameworkElement Element, DragOverBehavior Behavior) FindDropTarget(
            IEnumerable<(FrameworkElement Element, DragOverBehavior Behavior)> candidates,
            Func<(FrameworkElement Element, DragOverBehavior Behavior), bool> predicate,
            string screenshotId,
            string description) => candidates.FirstOrDefault(predicate) is var match && match.Element is not null
        ? match
        : throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production {description} drop target.");

    private static FrameworkElement FindContainerDragSource(
            IEnumerable<FrameworkElement> candidates,
            ISequenceContainer container,
            string screenshotId) {
        FrameworkElement source = candidates.FirstOrDefault(element => ReferenceEquals(element.DataContext, container))
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production '{container.Name}' drag source.");
        return ExpandProductionDragSource(source);
    }

    private static FrameworkElement ExpandProductionDragSource(FrameworkElement source) {
        object? dataContext = source.DataContext;
        FrameworkElement result = source;
        DependencyObject? current = VisualTreeHelper.GetParent(source);
        while (current is FrameworkElement parent) {
            if (!ReferenceEquals(parent.DataContext, dataContext)) {
                break;
            }
            result = parent;
            current = VisualTreeHelper.GetParent(parent);
        }
        return result;
    }

    private static void AddProductionDragAdorners(
            Grid layoutRoot,
            FrameworkElement source,
            FrameworkElement target,
            DragOverBehavior targetBehavior,
            string screenshotId) {
        RenderTargetBitmap sourceImage = RenderDragSource(source, screenshotId);
        DragDropBehavior dragBehavior = new(layoutRoot) { OriginalParentedObject = source, IsClone = true };
        UIElement dragAdorner = CreateProductionAdorner(
            "NINA.Sequencer.Behaviors.DragDropAdorner",
            screenshotId,
            dragBehavior,
            layoutRoot,
            sourceImage);
        dragAdorner.IsHitTestVisible = false;
        SpanLayoutGrid(dragAdorner, layoutRoot);
        Point sourceOrigin = source.TranslatePoint(new Point(0, 0), layoutRoot);
        Point targetOrigin = target.TranslatePoint(new Point(0, 0), layoutRoot);
        Point sourceCenter = new(sourceOrigin.X + source.ActualWidth / 2, sourceOrigin.Y + source.ActualHeight / 2);
        Point targetCenter = new(targetOrigin.X + target.ActualWidth / 2, targetOrigin.Y + target.ActualHeight / 2);
        Point dragCenter = new(
            sourceCenter.X + (targetCenter.X - sourceCenter.X) * 0.7,
            sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * 0.7);
        dragAdorner.RenderTransform = new TranslateTransform(
            dragCenter.X - sourceImage.PixelWidth / 2.0,
            dragCenter.Y - sourceImage.PixelHeight / 2.0);
        Panel.SetZIndex(dragAdorner, 1000);
        layoutRoot.Children.Add(dragAdorner);
        source.Effect = new BlurEffect { Radius = 10 };

        bool leftOfTarget = targetBehavior.DragOverDisplayAnchor == DragOverDisplayAnchor.Left;
        UIElement dropAdorner = CreateProductionAdorner(
            "NINA.Sequencer.Behaviors.DragOverAdorner",
            screenshotId,
            target.ActualWidth,
            target.ActualHeight,
            targetBehavior.DragOverCenterText,
            leftOfTarget,
            DropTargetEnum.Center,
            target);
        SpanLayoutGrid(dropAdorner, layoutRoot);
        double adornerWidth = ReadProductionAdornerDimension(dropAdorner, "AdornerWidth", screenshotId);
        double adornerHeight = ReadProductionAdornerDimension(dropAdorner, "AdornerHeight", screenshotId);
        dropAdorner.RenderTransform = new TranslateTransform(
            targetOrigin.X + (leftOfTarget ? -(adornerWidth - target.ActualWidth) : target.ActualWidth),
            targetOrigin.Y + target.ActualHeight / 2 - adornerHeight / 2);
        Panel.SetZIndex(dropAdorner, 1001);
        layoutRoot.Children.Add(dropAdorner);
        layoutRoot.UpdateLayout();
    }

    private static RenderTargetBitmap RenderDragSource(FrameworkElement source, string screenshotId) {
        int width = Math.Max(1, (int)Math.Ceiling(source.ActualWidth));
        int height = Math.Max(1, (int)Math.Ceiling(source.ActualHeight));
        if (width <= 1 || height <= 1) {
            throw new CatalogException($"Screenshot '{screenshotId}' found a zero-sized production drag source.");
        }
        DrawingVisual clone = new();
        using (DrawingContext drawing = clone.RenderOpen()) {
            VisualBrush brush = new(source) { Stretch = Stretch.None, Opacity = 0.4 };
            drawing.DrawRectangle(brush, null, new Rect(0, 0, width, height));
        }
        RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(clone);
        return bitmap;
    }

    private static UIElement CreateProductionAdorner(string typeName, string screenshotId, params object[] arguments) {
        Type type = typeof(DragDropBehavior).Assembly.GetType(typeName, throwOnError: true)!;
        try {
            return (UIElement)(Activator.CreateInstance(
                type,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: arguments,
                culture: null)
                ?? throw new InvalidOperationException("The constructor returned null."));
        } catch (Exception ex) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not create NINA's production '{type.Name}': {ex.GetBaseException().Message}");
        }
    }

    private static double ReadProductionAdornerDimension(UIElement adorner, string fieldName, string screenshotId) =>
        adorner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(adorner) is double value
            ? value
            : throw new CatalogException($"Screenshot '{screenshotId}' could not read NINA's production drag adorner dimension '{fieldName}'.");

    private static void SpanLayoutGrid(UIElement element, Grid layoutRoot) {
        Grid.SetColumnSpan(element, Math.Max(1, layoutRoot.ColumnDefinitions.Count));
        Grid.SetRowSpan(element, Math.Max(1, layoutRoot.RowDefinitions.Count));
    }

    private static void ResetProductionMenuMode(FrameworkElement fixture) {
        foreach (System.Windows.Interop.HwndSource source in PresentationSource.CurrentSources
            .OfType<System.Windows.Interop.HwndSource>()
            .Where(source => source.RootVisual?.GetType().Name == "PopupRoot")) {
            if (source.RootVisual is not null) {
                foreach (MenuItem item in FindDescendants<MenuItem>(source.RootVisual).Reverse()) {
                    item.IsSubmenuOpen = false;
                }
            }
            _ = SendMessage(source.Handle, 0x001F, IntPtr.Zero, IntPtr.Zero);
        }
        foreach (Button button in FindDescendants<Button>(fixture).Where(button => button.ContextMenu?.IsOpen == true)) {
            button.ContextMenu!.IsOpen = false;
        }
        foreach (FrameworkElement element in FindDescendants<FrameworkElement>(fixture)
            .Where(element => element.ToolTip is ToolTip { IsOpen: true })) {
            ((ToolTip)element.ToolTip).IsOpen = false;
        }
        fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        for (int attempt = 0; attempt < 10 && EnumeratePopupSources().Any(); attempt++) {
            Thread.Sleep(25);
            fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    private static void OpenProductionRootMenu(FrameworkElement fixture, string category, string screenshotId) {
        Button button = FindDescendants<Button>(fixture)
            .FirstOrDefault(control => control.Name == "AddButton"
                && control.ContextMenu is not null
                && control.DataContext is ISequenceRootContainer)
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production root add button.");
        button.ContextMenu!.PlacementTarget = button;
        button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
        Invoke(button, screenshotId);

        MenuItem instructions = button.ContextMenu.Items.OfType<MenuItem>().FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production Instructions menu.");
        Expand(instructions, screenshotId, "Instructions");

        MenuItem categoryItem = instructions.Items.OfType<MenuItem>()
            .FirstOrDefault(item => string.Equals(GetMenuText(item), category, StringComparison.OrdinalIgnoreCase)
                || GetMenuText(item).Contains(category, StringComparison.OrdinalIgnoreCase))
            ?? EnumeratePopupMenuItems()
            .Where(item => item.IsVisible && item.IsEnabled)
            .FirstOrDefault(item => string.Equals(GetMenuText(item), category, StringComparison.OrdinalIgnoreCase)
                || GetMenuText(item).Contains(category, StringComparison.OrdinalIgnoreCase))
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production '{category}' menu category.");
        Expand(categoryItem, screenshotId, category);
    }

    private static void OpenProductionContextMenu(FrameworkElement fixture, string buttonName, string screenshotId) {
        Button? button = FindDescendants<Button>(fixture)
            .Where(control => control.Name == buttonName && control.ContextMenu is not null)
            .FirstOrDefault(control => control.DataContext is ISequenceContainer container && container.Parent is not null)
            ?? FindDescendants<Button>(fixture).FirstOrDefault(control => control.Name == buttonName && control.ContextMenu is not null)
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production '{buttonName}' menu button.");
        button.ContextMenu!.PlacementTarget = button;
        button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
        Invoke(button, screenshotId);
        for (int attempt = 0; attempt < 3 && button.ContextMenu?.IsOpen != true; attempt++) {
            button.ContextMenu!.IsOpen = true;
            button.ContextMenu.UpdateLayout();
            button.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        }
        if (button.ContextMenu.IsOpen != true) {
            throw new CatalogException($"Screenshot '{screenshotId}' invoked NINA's production '{buttonName}' button but its menu did not open.");
        }
        button.ContextMenu.UpdateLayout();
    }

    private static void OpenProductionSaveTargetTooltip(FrameworkElement fixture, string screenshotId) {
        Button button = FindDescendants<Button>(fixture)
            .FirstOrDefault(control => control.Name == "TargetContainerButton"
                && control.DataContext is DeepSkyObjectContainer)
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production save-target button.");
        if (button.ToolTip is not ToolTip tooltip) {
            throw new CatalogException($"Screenshot '{screenshotId}' found NINA's save-target button without its production tooltip.");
        }
        tooltip.PlacementTarget = button;
        tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        tooltip.IsOpen = true;
        tooltip.UpdateLayout();
        fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        if (!tooltip.IsOpen) {
            throw new CatalogException($"Screenshot '{screenshotId}' opened NINA's production save-target tooltip but WPF closed it before capture.");
        }
    }

    private static void OpenProductionValidationTooltip(FrameworkElement fixture, string screenshotId) {
        Border issue = FindDescendants<Border>(fixture)
            .Where(border => border.IsVisible
                && border.DataContext is TakeExposure exposure
                && exposure.Issues.Count > 0
                && border.ToolTip is not null)
            .OrderBy(border => border.ActualWidth * border.ActualHeight)
            .FirstOrDefault()
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production validation indicator.");
        object productionContent = issue.ToolTip;
        ToolTip tooltip = productionContent as ToolTip ?? new ToolTip { Content = productionContent };
        issue.ToolTip = tooltip;
        tooltip.PlacementTarget = issue;
        tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
        tooltip.IsOpen = true;
        tooltip.UpdateLayout();
        fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        if (!tooltip.IsOpen) {
            throw new CatalogException($"Screenshot '{screenshotId}' opened NINA's production validation tooltip but WPF closed it before capture.");
        }
    }

    private static void Invoke(Button button, string screenshotId) {
        ButtonAutomationPeer peer = new(button);
        if (peer.GetPattern(PatternInterface.Invoke) is not IInvokeProvider invoke) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not invoke NINA's production '{button.Name}' button through UI Automation.");
        }
        invoke.Invoke();
        button.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
    }

    private static void Expand(MenuItem item, string screenshotId, string menuName) {
        for (int attempt = 0; attempt < 10 && !item.IsSubmenuOpen; attempt++) {
            if (ItemsControl.ItemsControlFromItemContainer(item) is ContextMenu contextMenu && !contextMenu.IsOpen) {
                contextMenu.IsOpen = true;
                contextMenu.UpdateLayout();
                item.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            }
            item.BringIntoView();
            MenuItemAutomationPeer peer = new(item);
            if (peer.GetPattern(PatternInterface.ExpandCollapse) is IExpandCollapseProvider expand) {
                expand.Expand();
            }
            item.IsSubmenuOpen = true;
            item.UpdateLayout();
            item.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            if (!item.IsSubmenuOpen) {
                Thread.Sleep(25);
            }
        }
        if (!item.IsSubmenuOpen) {
            throw new CatalogException($"Screenshot '{screenshotId}' expanded NINA's production '{menuName}' menu but its submenu did not open.");
        }
    }

    private static IEnumerable<System.Windows.Interop.HwndSource> EnumeratePopupSources() => PresentationSource.CurrentSources
        .OfType<System.Windows.Interop.HwndSource>()
        .Where(source => source.RootVisual is not null && source.RootVisual.GetType().Name == "PopupRoot");

    private static IEnumerable<MenuItem> EnumeratePopupMenuItems() => EnumeratePopupSources()
        .SelectMany(source => FindDescendants<MenuItem>(source.RootVisual!));

    private static string GetMenuText(MenuItem item) {
        if (item.Header is TextBlock header) {
            return header.Text;
        }
        return FindDescendants<TextBlock>(item).Select(text => text.Text).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
            ?? item.Header?.ToString()
            ?? string.Empty;
    }

    private static void ApplyAdvancedSequencerSidebarState(FrameworkElement fixture, string output, string screenshotId) {
        TabControl? sidebar = FindDescendants<TabControl>(fixture).FirstOrDefault(control => control.Items.Count == 5);
        if (sidebar is null) {
            return;
        }

        string requestedTab = GetRequestedSidebarTab(output);
        TabItem? tab = sidebar.Items.OfType<TabItem>().FirstOrDefault(item =>
            FindDescendants<TextBlock>(item).Any(text => string.Equals(text.Text, requestedTab, StringComparison.OrdinalIgnoreCase)));
        if (tab is null) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production '{requestedTab}' sequencer tab.");
        }
        sidebar.SelectedItem = tab;
        sidebar.UpdateLayout();
        fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        fixture.UpdateLayout();

        if (output.EndsWith("/sequencer/sequencer_usertemplate.png", StringComparison.Ordinal)) {
            Expander template = FindDescendants<Expander>(fixture)
                .FirstOrDefault(expander => expander.DataContext is TemplatedSequenceContainer item
                    && item.Container.Name == "RGB Loop")
                ?? throw new CatalogException($"Screenshot '{screenshotId}' could not find NINA's production RGB Loop template preview.");
            template.IsExpanded = true;
            template.UpdateLayout();
            fixture.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            fixture.UpdateLayout();
        }
    }

    private static string GetRequestedSidebarTab(string output) {
        if (ContainsAny(output, "sequencer_templates", "saveastemplate", "usertemplate")) {
            return "Templates";
        }
        if (ContainsAny(output, "targetstab", "addtarget", "droptargettotab", "applytarget")) {
            return "Targets";
        }
        if (ContainsAny(output, "sequencer_symbols", "symbolvalues", "definesymbol", "definevariable", "defineconstant", "symbolexample", "expressionvalue", "undefined", "expressionwarning")) {
            return "Symbols";
        }
        if (ContainsAny(output, "sequencer_functions", "crazeexpression")) {
            return "Functions";
        }
        return "Instructions";
    }

    private static bool ContainsAny(string value, params string[] candidates) => candidates.Any(value.Contains);

    private static IEnumerable<T> FindDescendants<T>(DependencyObject root) where T : DependencyObject {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++) {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T result) {
                yield return result;
            }
            foreach (T descendant in FindDescendants<T>(child)) {
                yield return descendant;
            }
        }
    }
}
