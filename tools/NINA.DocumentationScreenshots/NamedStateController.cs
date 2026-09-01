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
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using NINA.Sequencer.Container;

namespace NINA.DocumentationScreenshots;

/// <summary>
/// Applies catalog states by manipulating controls from NINA's compiled production visual tree.
/// </summary>
public static class NamedStateController {
    public static void Apply(FrameworkElement fixture, ScreenshotAsset asset) {
        ResetProductionMenuMode(fixture);
        string output = asset.Output.Replace('\\', '/').ToLowerInvariant();
        ApplyAdvancedSequencerSidebarState(fixture, output, asset.Id);

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

        if (output.EndsWith("/sequencer/conditions/loopuntiltime.png", StringComparison.Ordinal)) {
            ComboBox comboBox = FindDescendants<ComboBox>(fixture).FirstOrDefault()
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find the production time-provider ComboBox.");
            comboBox.IsDropDownOpen = true;
        }
        if (output.EndsWith("/advanced/framing/imagesources.png", StringComparison.Ordinal)) {
            ComboBox source = FindDescendants<ComboBox>(fixture)
                .FirstOrDefault(control => control.Name == "PART_FramingAssistantSource")
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find the production image-source selector.");
            source.IsDropDownOpen = true;
        }
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
    }

    private static string GetRequestedSidebarTab(string output) {
        if (ContainsAny(output, "sequencer_templates", "saveastemplate", "usertemplate")) {
            return "Templates";
        }
        if (ContainsAny(output, "targetstab", "addtargettotargettab", "droptargettotab", "applytarget")) {
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
