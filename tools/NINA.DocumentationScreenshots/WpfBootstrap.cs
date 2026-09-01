#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Globalization;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NINA.Core.Locale;
using NINA.Core.Model.Equipment;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Profile;

namespace NINA.DocumentationScreenshots;

public static class WpfBootstrap {
    private const string ScreenshotCultureName = "en-US";
    private static readonly Guid ScreenshotProfileId = new("1207d37d-076a-4e4f-b25c-d50989fdcc71");
    private static readonly string isolatedRoot = Path.Combine(Path.GetTempPath(), $"nina-documentation-host-{Environment.ProcessId}");

    public static void Initialize() {
        CultureInfo culture = CultureInfo.GetCultureInfo(ScreenshotCultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Loc.Instance.ReloadLocale(culture.Name);
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

        CoreUtil.APPLICATIONTEMPPATH = isolatedRoot;
        CoreUtil.APPLICATIONDIRECTORY = isolatedRoot;
        ProfileService.PROFILEFOLDER = Path.Combine(isolatedRoot, "Profiles");
        InstallIsolatedSettingsProvider("NINA.Properties.Settings, NINA");
        InstallIsolatedSettingsProvider("NINA.Core.Properties.Settings, NINA.Core");
        InstallIsolatedSettingsProvider("NINA.CustomControlLibrary.Properties.Settings, NINA.CustomControlLibrary");

        Application app = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        if (app.MainWindow is null) {
            Grid rootGrid = new() { Name = "RootGrid" };
            Window shell = new() { Content = rootGrid, ShowInTaskbar = false };
            NameScope.SetNameScope(shell, new NameScope());
            shell.RegisterName(rootGrid.Name, rootGrid);
            app.MainWindow = shell;
        }
        if (app.Resources["ActiveProfile"] is NINA.Profile.Profile existingProfile) {
            existingProfile.ApplicationSettings.Culture = culture.Name;
        }
        if (app.Resources.Contains("NINA.DocumentationScreenshots.ResourcesLoaded")) {
            return;
        }

        ProfileService profileService = new();
        NINA.Profile.Profile profile = new("Documentation screenshots") {
            Id = ScreenshotProfileId
        };
        profile.ApplicationSettings.Culture = culture.Name;
        profile.ApplicationSettings.SkySurveyCacheDirectory = @"C:\NINA\Cache";
        profile.CameraSettings.Id = "Documentation Camera";
        profile.CameraSettings.Gain = 50;
        profile.CameraSettings.Offset = 25;
        profile.CameraSettings.USBLimit = 50;
        profile.CameraSettings.PixelSize = 3.76;
        profile.TelescopeSettings.FocalLength = 800;
        profile.AstrometrySettings.Latitude = DocumentationAstronomy.Latitude;
        profile.AstrometrySettings.Longitude = DocumentationAstronomy.Longitude;
        profile.AstrometrySettings.Elevation = DocumentationAstronomy.Elevation;
        profile.ColorSchemaSettings.ColorSchema = profile.ColorSchemaSettings.ColorSchemas.Items
            .Single(schema => string.Equals(schema.Name, "Slate", StringComparison.Ordinal));
        FilterInfo[] filters = [
            new FilterInfo("L", 0, 0, 4, new BinningMode(1, 1), 50, 25) { AutoFocusFilter = true },
            new FilterInfo("R", -15, 1, 4, new BinningMode(1, 1), 50, 25),
            new FilterInfo("G", -8, 2, 4, new BinningMode(1, 1), 50, 25),
            new FilterInfo("B", 12, 3, 4, new BinningMode(1, 1), 50, 25),
            new FilterInfo("Ha", 24, 4, 6, new BinningMode(1, 1), 70, 25),
            new FilterInfo("OIII", 18, 5, 6, new BinningMode(1, 1), 70, 25),
            new FilterInfo("SII", 30, 6, 6, new BinningMode(1, 1), 70, 25)
        ];
        foreach (FilterInfo filter in filters) {
            profile.FilterWheelSettings.FilterWheelFilters.Add(filter);
        }
        profile.FocuserSettings.UseFilterWheelOffsets = true;
        profile.FocuserSettings.AutoFocusStepSize = 100;
        profile.FocuserSettings.AutoFocusInitialOffsetSteps = 4;
        profile.FocuserSettings.AutoFocusExposureTime = 4;
        profile.FocuserSettings.AutoFocusDisableGuiding = true;
        profile.FocuserSettings.FocuserSettleTime = 1;
        profile.GuiderSettings.DitherPixels = 1.5;
        profile.GuiderSettings.SettlePixels = 1.5;
        profile.GuiderSettings.SettleTime = 12;
        profile.GuiderSettings.SettleTimeout = 40;
        profile.GuiderSettings.AutoRetryStartGuidingTimeoutSeconds = 60;
        profile.GuiderSettings.PHD2HistorySize = 100;
        profile.GuiderSettings.PHD2GuiderScale = GuiderScaleEnum.PIXELS;
        profile.MeridianFlipSettings.MinutesAfterMeridian = 5;
        profile.MeridianFlipSettings.MaxMinutesAfterMeridian = 10;
        profile.MeridianFlipSettings.PauseTimeBeforeMeridian = 0;
        profile.MeridianFlipSettings.UseSideOfPier = true;
        profile.MeridianFlipSettings.Recenter = true;
        profile.MeridianFlipSettings.SettleTime = 5;
        profile.MeridianFlipSettings.AutoFocusAfterFlip = false;
        profile.MeridianFlipSettings.RotateImageAfterFlip = true;
        profile.ImageFileSettings.FilePath = @"C:\NINA\Images";
        profile.ImageFileSettings.FileType = FileTypeEnum.XISF;
        profile.ImageFileSettings.FilePattern = @"$$DATETIME$$\$$IMAGETYPE$$\$$TARGETNAME$$_$$DATETIME$$_$$FILTER$$_$$EXPOSURETIME$$s_$$FRAMENR$$";
        profile.ImageFileSettings.FilePatternDARK = @"DARK\$$SENSORTEMP$$\$$EXPOSURETIME$$s_$$FRAMENR$$";
        profile.ImageFileSettings.FilePatternFLAT = @"FLAT\$$FILTER$$\$$EXPOSURETIME$$s_$$FRAMENR$$";
        profile.ImageFileSettings.FilePatternBIAS = @"BIAS\$$FRAMENR$$";
        profile.ImageFileSettings.XISFCompressionType = XISFCompressionTypeEnum.LZ4;
        profile.ImageFileSettings.XISFChecksumType = XISFChecksumTypeEnum.SHA256;
        profile.ImageFileSettings.XISFByteShuffling = true;
        profile.SequenceSettings.DefaultSequenceFolder = @"C:\NINA\Sequences";
        profile.SequenceSettings.SequencerTemplatesFolder = @"C:\NINA\Sequences\Templates";
        profile.SequenceSettings.SequencerTargetsFolder = @"C:\NINA\Sequences\Targets";
        profile.SequenceSettings.StartupSequenceTemplate = string.Empty;
        FieldInfo activeProfile = typeof(ProfileService).GetField("activeProfile", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ProfileService.activeProfile was not found.");
        activeProfile.SetValue(profileService, profile);
        profileService.Profiles.Add(new ProfileMeta {
            Id = profile.Id,
            Name = profile.Name,
            Description = "Deterministic documentation profile",
            IsActive = true,
            LastUsed = new DateTime(2026, 8, 31, 20, 0, 0),
            Location = @"C:\NINA\Profiles\documentation.profile"
        });
        app.Resources["ProfileService"] = profileService;
        app.Resources["ActiveProfile"] = profile;

        string[] resourceSources = [
            "/NINA.WPF.Base;component/Resources/StaticResources/SVGDictionary.xaml",
            "/NINA.WPF.Base;component/Resources/StaticResources/Brushes.xaml",
            "/NINA.WPF.Base;component/Resources/StaticResources/Converters.xaml",
            "/NINA;component/Resources/StaticResources/DataTemplates.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Button.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Path.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/TextBlock.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/TextBox.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/TabControl.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/CheckBox.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/DataGrid.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ListView.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/GroupBox.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/RepeatButton.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ToggleButton.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Slider.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Expander.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ScrollViewer.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ComboBox.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/GridSplitter.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ProgressBar.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Tooltip.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/CancellableButton.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/DatePicker.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/StepperControl.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ContextMenu.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/Hyperlink.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/SplitButton.xaml",
            "/NINA.WPF.Base;component/Resources/Styles/ColorPicker.xaml",
            "/NINA.Sequencer;component/Logic/Datatemplates.xaml",
            "/NINA.Sequencer;component/Container/Datatemplates.xaml",
            "/NINA.Sequencer;component/Conditions/Datatemplates.xaml",
            "/NINA.Sequencer;component/Trigger/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Autofocus/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Camera/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Connect/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Dome/DataTemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Expressions/DataTemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/FilterWheel/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/FlatDevice/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Focuser/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Guider/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Imaging/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Platesolving/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Rotator/DataTemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Switch/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Telescope/Datatemplates.xaml",
            "/NINA.Sequencer;component/SequenceItem/Utility/Datatemplates.xaml",
            "/NINA;component/Resources/Styles/Window.xaml",
            "/NINA;component/Resources/Styles/AvalonDock.xaml",
            "/NINA;component/Resources/Styles/Oxyplot.xaml",
            "/NINA;component/Resources/Styles/Markdown.xaml"
        ];

        foreach (string resourceSource in resourceSources) {
            app.Resources.MergedDictionaries.Add(new ResourceDictionary {
                Source = new Uri(resourceSource, UriKind.Relative)
            });
        }
        app.Resources["NINA.DocumentationScreenshots.ResourcesLoaded"] = true;
    }

    private static void InstallIsolatedSettingsProvider(string assemblyQualifiedTypeName) {
        Type? settingsType = Type.GetType(assemblyQualifiedTypeName, throwOnError: false);
        if (settingsType?.GetProperty("Default", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
                is not ApplicationSettingsBase settings) {
            return;
        }

        IsolatedSettingsProvider provider = new();
        provider.Initialize(nameof(IsolatedSettingsProvider), new NameValueCollection());
        settings.Providers.Clear();
        settings.Providers.Add(provider);
        foreach (SettingsProperty property in settings.Properties) {
            property.Provider = provider;
        }
        settings.Reload();
    }
}

internal sealed class IsolatedSettingsProvider : SettingsProvider {
    private readonly Dictionary<string, object?> values = new(StringComparer.Ordinal);

    public override string ApplicationName { get; set; } = "NINA.DocumentationScreenshots";

    public override void Initialize(string? name, NameValueCollection? config) =>
        base.Initialize(name ?? nameof(IsolatedSettingsProvider), config ?? new NameValueCollection());

    public override SettingsPropertyValueCollection GetPropertyValues(
            SettingsContext context,
            SettingsPropertyCollection properties) {
        SettingsPropertyValueCollection result = new();
        foreach (SettingsProperty property in properties) {
            SettingsPropertyValue value = new(property);
            if (values.TryGetValue(property.Name, out object? stored)) {
                value.PropertyValue = stored;
            } else {
                value.SerializedValue = property.DefaultValue;
            }
            value.IsDirty = false;
            result.Add(value);
        }
        return result;
    }

    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection) {
        foreach (SettingsPropertyValue value in collection) {
            values[value.Name] = value.PropertyValue;
            value.IsDirty = false;
        }
    }
}
