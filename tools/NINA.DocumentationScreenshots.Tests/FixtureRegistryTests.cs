#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Threading;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NUnit.Framework;

namespace NINA.DocumentationScreenshots.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class FixtureRegistryTests {
    [OneTimeSetUp]
    public void SetUp() {
        WpfBootstrap.Initialize();
    }

    [Test]
    public void WpfBootstrap_IsolatesNinaApplicationSettingsFromTheUserProfile() {
        Type settingsType = Type.GetType("NINA.Properties.Settings, NINA", throwOnError: true)!;
        ApplicationSettingsBase settings = (ApplicationSettingsBase)settingsType
            .GetProperty("Default", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!
            .GetValue(null)!;

        Assert.That(settings.Providers.Cast<SettingsProvider>(),
            Is.All.Matches<SettingsProvider>(provider => provider.GetType().Assembly == typeof(WpfBootstrap).Assembly),
            "NINA settings must use the documentation tool's isolated provider, never LocalFileSettingsProvider.");
    }

    [Test]
    public void WpfBootstrap_UsesFixedDocumentationPathsAndPngCapableImageSettings() {
        NINA.Profile.Profile profile = (NINA.Profile.Profile)Application.Current.Resources["ActiveProfile"];

        Assert.Multiple(() => {
            Assert.That(profile.ImageFileSettings.FilePath, Is.EqualTo(@"C:\NINA\Images"));
            Assert.That(profile.ImageFileSettings.FileType, Is.EqualTo(NINA.Core.Enum.FileTypeEnum.XISF));
            Assert.That(profile.SequenceSettings.DefaultSequenceFolder, Is.EqualTo(@"C:\NINA\Sequences"));
            Assert.That(profile.SequenceSettings.SequencerTemplatesFolder, Is.EqualTo(@"C:\NINA\Sequences\Templates"));
            Assert.That(profile.SequenceSettings.SequencerTargetsFolder, Is.EqualTo(@"C:\NINA\Sequences\Targets"));
        });
    }

    [Test]
    public void WpfBootstrap_ProvidesDeterministicProfileSelectionAndCameraDevice() {
        NINA.Profile.Profile profile = (NINA.Profile.Profile)Application.Current.Resources["ActiveProfile"];
        NINA.Profile.Interfaces.IProfileService profileService =
            (NINA.Profile.Interfaces.IProfileService)Application.Current.Resources["ProfileService"];

        Assert.Multiple(() => {
            Assert.That(profile.Id, Is.EqualTo(new Guid("1207d37d-076a-4e4f-b25c-d50989fdcc71")));
            Assert.That(profile.ApplicationSettings.SkySurveyCacheDirectory, Is.EqualTo(@"C:\NINA\Cache"));
            Assert.That(profile.CameraSettings.Id, Is.EqualTo("Documentation Camera"));
            Assert.That(profile.CameraSettings.Gain, Is.EqualTo(50));
            Assert.That(profile.CameraSettings.Offset, Is.EqualTo(25));
            Assert.That(profile.CameraSettings.USBLimit, Is.EqualTo(50));
            Assert.That(profile.CameraSettings.PixelSize, Is.EqualTo(3.76d));
            Assert.That(profile.TelescopeSettings.FocalLength, Is.EqualTo(800));
            Assert.That(profile.GuiderSettings.PHD2HistorySize, Is.EqualTo(100));
            Assert.That(profileService.Profiles.Select(item => item.Id), Does.Contain(profile.Id));
            Assert.That(profileService.Profiles.Single(item => item.Id == profile.Id).Name,
                Is.EqualTo("Documentation screenshots"));
            Assert.That(profileService.Profiles.Single(item => item.Id == profile.Id).Description,
                Is.EqualTo("Deterministic documentation profile"));
        });
    }

    [Test]
    public void WpfBootstrap_ProvidesACompleteDocumentationFilterSet() {
        NINA.Profile.Profile profile = (NINA.Profile.Profile)Application.Current.Resources["ActiveProfile"];

        Assert.That(
            profile.FilterWheelSettings.FilterWheelFilters.Select(filter => filter.Name),
            Is.EqualTo(new[] { "L", "R", "G", "B", "Ha", "OIII", "SII" }));
    }

    [Test]
    public void WpfBootstrap_UsesProductionSlateColorSchema() {
        NINA.Profile.Profile profile = (NINA.Profile.Profile)Application.Current.Resources["ActiveProfile"];
        SolidColorBrush background = (SolidColorBrush)Application.Current.Resources["BackgroundBrush"];

        Assert.Multiple(() => {
            Assert.That(profile.ColorSchemaSettings.ColorSchema.Name, Is.EqualTo("Slate"));
            Assert.That(background.Color, Is.EqualTo(Color.FromArgb(0xff, 0x1e, 0x21, 0x29)));
        });
    }

    [Test]
    public void ScreenshotChrome_UsesProductionSlateBackgroundBrush() {
        Grid chrome = (Grid)ScreenshotChrome.Wrap(new Grid(), 320, 240, []);
        SolidColorBrush productionBackground = (SolidColorBrush)Application.Current.Resources["BackgroundBrush"];

        Assert.Multiple(() => {
            Assert.That(chrome.Background, Is.SameAs(productionBackground));
            Assert.That(((SolidColorBrush)chrome.Background).Color, Is.EqualTo(Color.FromArgb(0xff, 0x1e, 0x21, 0x29)));
        });
    }

    [Test]
    public void SequencerEntity_UsesTheProductionDataTemplate() {
        ScreenshotAsset asset = new() {
            Id = "open-dome",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/open-dome.png",
            Fixture = "sequencer-entity",
            State = "open-dome",
            SourceIdentifier = "NINA.Sequencer.SequenceItem.Dome.OpenDomeShutter",
            DisplayName = "Open Dome Shutter",
            Width = 300,
            Height = 60
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Assert.That(((NINA.Sequencer.ISequenceEntity)fixture.DataContext).Name, Is.EqualTo("Open Dome Shutter"));
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();

        Assert.That(fixture.GetType().FullName == "NINA.View.Sequencer.SequenceBlockView"
            || FindDescendants(fixture).Any(element => element.GetType().FullName == "NINA.View.Sequencer.SequenceBlockView"), Is.True,
            "The fixture must render NINA's production SequenceBlockView instead of a screenshot-specific layout.");
        Assert.That(FindDescendants(fixture).OfType<TextBlock>().Any(text => text.Text == "Open Dome Shutter"), Is.True,
            "The production binding must display the fixture entity name.");
    }

    [Test]
    public void SequencerEntity_UsesProductionSequenceBlockViewForTriggerWithoutDedicatedTemplate() {
        ScreenshotAsset asset = new() {
            Id = "reconnect-on-download-failure",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/trigger/reconnectcamera.png",
            Fixture = "sequencer-entity",
            State = "reconnect-on-download-failure",
            SourceIdentifier = "sequencer:ReconnectOnDownloadFailure",
            DisplayName = "Reconnect Camera On Download Failure",
            Width = 720,
            Height = 35
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.View.Sequencer.SequenceBlockView>());
            Assert.That(fixture.DataContext, Is.TypeOf<NINA.Sequencer.Trigger.Connect.ReconnectOnDownloadFailure>());
            Assert.That(FindDescendants(fixture).OfType<TextBlock>()
                .Any(text => text.Text == "Reconnect Camera On Download Failure"), Is.True);
        });
    }

    [Test]
    public void SequencerEntity_RendersSaveSequenceProductionControls() {
        ScreenshotAsset asset = new() {
            Id = "save-sequence",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/instructions/utility_savesequence.png",
            Fixture = "sequencer-entity",
            State = "save-sequence",
            SourceIdentifier = "sequencer:SaveSequence",
            DisplayName = "Save Sequence",
            Width = 900,
            Height = 35
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();

        TextBlock fileLabel = FindDescendants(fixture).OfType<TextBlock>().Single(text => text.Text == "File");
        Point fileLabelOrigin = fileLabel.TransformToAncestor(fixture).Transform(new Point());
        NINA.Sequencer.SequenceItem.Utility.SaveSequence entity =
            (NINA.Sequencer.SequenceItem.Utility.SaveSequence)fixture.DataContext;
        Assert.Multiple(() => {
            Assert.That(entity.Status, Is.Not.EqualTo(NINA.Core.Enum.SequenceEntityStatus.DISABLED));
            Assert.That(fileLabel.ActualWidth, Is.GreaterThan(1));
            Assert.That(fileLabelOrigin.X, Is.InRange(0, asset.Width - 1));
            Assert.That(fileLabelOrigin.Y, Is.InRange(0, asset.Height - 1));
        });
    }

    [Test]
    public void SequencerEntity_RendersReconnectEquipmentProductionNameAndValidState() {
        ScreenshotAsset asset = new() {
            Id = "reconnect-equipment",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/trigger/reconnectequipment.png",
            Fixture = "sequencer-entity",
            State = "reconnect-equipment",
            SourceIdentifier = "sequencer:ReconnectTrigger",
            DisplayName = "Reconnect Equipment",
            Width = 720,
            Height = 35
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();
        NINA.Sequencer.Trigger.Connect.ReconnectTrigger entity =
            (NINA.Sequencer.Trigger.Connect.ReconnectTrigger)fixture.DataContext;

        Assert.Multiple(() => {
            Assert.That(entity.Name, Is.EqualTo("Reconnect Equipment"));
            Assert.That(entity.Validate(), Is.True);
            Assert.That(FindDescendants(fixture).OfType<TextBlock>()
                .Any(text => text.Text == "Reconnect Equipment"), Is.True);
        });
    }

    [Test]
    public void SequencerEntity_PopulatesNestedProductionEntities() {
        ScreenshotAsset asset = new() {
            Id = "take-many-exposures",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/instructions/camera_manyexposures.png",
            Width = 766,
            Height = 34,
            Fixture = "sequencer-entity",
            State = "take-many-exposures",
            DisplayName = "Take Many Exposures",
            SourceIdentifier = "sequencer:TakeManyExposures",
            Icon = "CameraSVG"
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        NINA.Sequencer.SequenceItem.Imaging.TakeManyExposures entity =
            (NINA.Sequencer.SequenceItem.Imaging.TakeManyExposures)fixture.DataContext;

        Assert.Multiple(() => {
            Assert.That(entity.Iterations, Is.EqualTo(2));
            Assert.That(entity.GetTakeExposure().ExposureTime, Is.EqualTo(180));
            Assert.That(entity.GetTakeExposure().Gain, Is.EqualTo(50));
            Assert.That(entity.GetTakeExposure().Offset, Is.EqualTo(25));
            Assert.That(entity.GetTakeExposure().ImageType, Is.EqualTo("LIGHT"));
        });
    }

    [Test]
    public void SequencerEntity_UsesIsolatedProfileFilterForNestedFlatExposure() {
        ScreenshotAsset asset = new() {
            Id = "trained-dark-flat",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/instructions/flat_traineddark.png",
            Width = 1000,
            Height = 35,
            Fixture = "sequencer-entity",
            State = "trained-dark-flat",
            DisplayName = "Trained Dark Exposure",
            SourceIdentifier = "sequencer:TrainedDarkFlatExposure",
            Icon = "BrainBulbSVG"
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        NINA.Sequencer.SequenceItem.FlatDevice.TrainedDarkFlatExposure entity =
            (NINA.Sequencer.SequenceItem.FlatDevice.TrainedDarkFlatExposure)fixture.DataContext;

        Assert.Multiple(() => {
            Assert.That(entity.GetSwitchFilterItem().ComboBoxText, Is.EqualTo("L"));
            Assert.That(entity.GetExposureItem().ExposureTime, Is.EqualTo(180));
            Assert.That(entity.GetIterations().Iterations, Is.EqualTo(2));
        });
    }

    [Test]
    public void AutoFocusChart_UsesProductionViewAndPopulatedProductionViewModel() {
        ScreenshotAsset asset = new() {
            Id = "autofocus-curve",
            Classification = ScreenshotClassification.NinaGeneratedVisual,
            Output = "docs/images/generated/advanced/autofocuscurve1.png",
            Fixture = "autofocus-chart",
            State = "autofocus-curve",
            ViewType = "NINA.View.AutoFocusChart",
            Width = 800,
            Height = 600
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);

        Assert.That(fixture, Is.TypeOf<NINA.View.AutoFocusChart>());
        Assert.That(fixture.DataContext, Is.TypeOf<NINA.WPF.Base.ViewModel.AutoFocus.AutoFocusVM>());
        NINA.WPF.Base.ViewModel.AutoFocus.AutoFocusVM viewModel =
            (NINA.WPF.Base.ViewModel.AutoFocus.AutoFocusVM)fixture.DataContext;
        Assert.That(viewModel.FocusPoints, Has.Count.GreaterThan(8));
        Assert.That(viewModel.HyperbolicFitting, Is.Not.Null);
    }

    [Test]
    public void HfrHistory_UsesProductionViewAndPopulatedProductionViewModel() {
        ScreenshotAsset asset = new() {
            Id = "hfr-history",
            Classification = ScreenshotClassification.NinaGeneratedVisual,
            Output = "docs/images/tabs/imaging_HFRhistory.png",
            Fixture = "autofocus-chart",
            State = "hfr-history",
            ViewType = "NINA.View.AutoFocusChart",
            Width = 410,
            Height = 430
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);

        Assert.That(fixture, Is.TypeOf<NINA.View.AnchorableImageHistoryView>());
        Assert.That(fixture.DataContext, Is.TypeOf<NINA.ViewModel.ImageHistory.ImageHistoryVM>());
        NINA.ViewModel.ImageHistory.ImageHistoryVM viewModel =
            (NINA.ViewModel.ImageHistory.ImageHistoryVM)fixture.DataContext;
        Assert.Multiple(() => {
            Assert.That(viewModel.ObservableImageHistoryView, Has.Count.EqualTo(72));
            Assert.That(viewModel.AutoFocusPointsView, Has.Count.EqualTo(4));
        });
    }

    [Test]
    public void SkyAtlasView_UsesProductionViewModelWithDeterministicSearchResults() {
        ScreenshotAsset asset = new() {
            Id = "sky-atlas",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/skyAtlas1.png",
            Fixture = "view",
            State = "sky-atlas",
            ViewType = "NINA.View.SkyAtlasView",
            Width = 1704,
            Height = 1020
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        object viewModel = fixture.DataContext;
        object searchResult = viewModel.GetType().GetProperty("SearchResult")!.GetValue(viewModel)!;

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.View.SkyAtlasView>());
            Assert.That(viewModel.GetType().FullName, Is.EqualTo("NINA.ViewModel.SkyAtlasVM"));
            Assert.That(searchResult.GetType().GetProperty("Count")!.GetValue(searchResult), Is.EqualTo(3));
            object selectedItem = searchResult.GetType().GetProperty("SelectedItem")!.GetValue(searchResult)!;
            Assert.That(selectedItem.GetType().GetProperty("Name")!.GetValue(selectedItem), Is.EqualTo("Whirlpool Galaxy"));
        });
    }

    [Test]
    public void SimpleSequenceView_UsesInitializedProductionViewModelWithFlatExposures() {
        ScreenshotAsset asset = new() {
            Id = "simple-flat-sequence",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/equipment_flats1.PNG",
            Fixture = "view",
            State = "simple-flat-sequence",
            ViewType = "NINA.View.SimpleSequencer.SimpleSequenceView",
            Width = 2432,
            Height = 809
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        NINA.ViewModel.Interfaces.ISimpleSequenceVM viewModel =
            (NINA.ViewModel.Interfaces.ISimpleSequenceVM)fixture.DataContext;
        NINA.Sequencer.Container.ISequenceContainer selectedTarget =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.SelectedTarget;

        Assert.Multiple(() => {
            Assert.That(fixture.GetType().FullName, Is.EqualTo("NINA.View.SimpleSequencer.SimpleSequenceView"));
            Assert.That(viewModel.Sequencer, Is.Not.Null);
            Assert.That(viewModel.SelectedTarget, Is.Not.Null);
            Assert.That(selectedTarget.Items, Has.Count.EqualTo(4));
            Assert.That(selectedTarget.Items.All(item => item.GetType().Name == "SimpleExposure"), Is.True);
        });
    }

    [Test]
    public void FramingAssistantView_UsesProductionViewModelWithLoadedDeterministicMosaic() {
        ScreenshotAsset asset = new() {
            Id = "framing-mosaic",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/advanced/framing/Mosaic.png",
            Fixture = "view",
            State = "framing-mosaic",
            ViewType = "NINA.View.FramingAssistantView",
            Width = 1920,
            Height = 1080
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() {
            Width = asset.Width,
            Height = asset.Height,
            Content = fixture,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };
        try {
            host.Show();
            fixture.UpdateLayout();
            fixture.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));
            object viewModel = fixture.DataContext;
            Type viewModelType = viewModel.GetType();
            object imageParameter = viewModelType.GetProperty("ImageParameter")!.GetValue(viewModel)!;
            BitmapSource image = (BitmapSource)imageParameter.GetType().GetProperty("Image")!.GetValue(imageParameter)!;
            object dso = viewModelType.GetProperty("DSO")!.GetValue(viewModel)!;
            System.Collections.ICollection rectangles =
                (System.Collections.ICollection)viewModelType.GetProperty("ProjectedCameraRectangles")!.GetValue(viewModel)!;

            Assert.Multiple(() => {
                Assert.That(fixture, Is.TypeOf<NINA.View.FramingAssistantView>());
                Assert.That(viewModelType.FullName, Is.EqualTo("NINA.ViewModel.FramingAssistant.FramingAssistantVM"));
                Assert.That(image.PixelWidth, Is.GreaterThan(500));
                Assert.That(image.PixelHeight, Is.GreaterThan(300));
                Assert.That(dso.GetType().GetProperty("Name")!.GetValue(dso), Is.EqualTo("M 31 Andromeda Galaxy"));
                Assert.That(viewModelType.GetProperty("CameraWidth")!.GetValue(viewModel), Is.EqualTo(4656));
                Assert.That(viewModelType.GetProperty("CameraHeight")!.GetValue(viewModel), Is.EqualTo(3520));
                Assert.That(viewModelType.GetProperty("RectangleCalculated")!.GetValue(viewModel), Is.True);
                Assert.That(rectangles, Has.Count.EqualTo(4));
                Assert.That(FindDescendants(fixture).OfType<System.Windows.Shapes.Rectangle>()
                    .Count(rectangle => rectangle.DataContext?.GetType().Name == "SkyMapCameraRectanglePlacement"),
                    Is.EqualTo(4),
                    "The compiled view must render all four production mosaic overlays.");
            });
        } finally {
            host.Content = null;
            host.Close();
        }
    }

    [Test]
    public void MeridianFlipSettings_ExtractsTheCompiledProductionGroupAndRealSettingsModel() {
        ScreenshotAsset asset = new() {
            Id = "meridian-flip-settings",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/advanced/meridianflip.PNG",
            Fixture = "settings-group",
            State = "meridian-flip-settings",
            Width = 315,
            Height = 237
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);

        Assert.That(fixture, Is.TypeOf<GroupBox>());
        Assert.That(fixture.DataContext, Is.InstanceOf<NINA.Profile.Interfaces.IMeridianFlipSettings>());
        Assert.That(((GroupBox)fixture).Header, Is.TypeOf<Grid>(),
            "The fixture must be the GroupBox compiled from NINA.View.Options.ImagingView.");
    }

    [TestCase("NINA.View.AnchorableImageHistoryView", "NINA.ViewModel.ImageHistory.ImageHistoryVM")]
    [TestCase("NINA.View.AnchorableAutoFocusView", "NINA.Imaging.ViewModel.Imaging.AutoFocusToolVM")]
    [TestCase("NINA.View.AnchorableGuiderView", "NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM")]
    [TestCase("NINA.View.AnchorableImageStatisticsView", "NINA.ViewModel.ImageStatisticsVM")]
    [TestCase("NINA.View.AnchorableFocusTargetsView", "NINA.ViewModel.FocusTargetsVM")]
    [TestCase("NINA.View.AnchorableRotatorView", "NINA.WPF.Base.ViewModel.Equipment.Rotator.RotatorVM")]
    [TestCase("NINA.View.AnchorableSwitchHubView", "NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM")]
    [TestCase("NINA.View.AnchorableTelescopeView", "NINA.WPF.Base.ViewModel.Equipment.Telescope.TelescopeVM")]
    [TestCase("NINA.View.AnchorableWeatherDataView", "NINA.WPF.Base.ViewModel.Equipment.WeatherData.WeatherDataVM")]
    [TestCase("NINA.View.Equipment.CameraView", "NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM")]
    [TestCase("NINA.View.Equipment.TelescopeView", "NINA.WPF.Base.ViewModel.Equipment.Telescope.TelescopeVM")]
    [TestCase("NINA.View.Equipment.WeatherDataView", "NINA.WPF.Base.ViewModel.Equipment.WeatherData.WeatherDataVM")]
    [TestCase("NINA.View.Equipment.Guider.GuiderView", "NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM")]
    [TestCase("NINA.View.Equipment.DomeView", "NINA.WPF.Base.ViewModel.Equipment.Dome.DomeVM")]
    [TestCase("NINA.View.Equipment.RotatorView", "NINA.WPF.Base.ViewModel.Equipment.Rotator.RotatorVM")]
    [TestCase("NINA.View.Equipment.SwitchHubView", "NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM")]
    [TestCase("NINA.View.Equipment.TabPage", "NINA.ViewModel.EquipmentVM")]
    [TestCase("NINA.View.ThumbnailListView", "NINA.ViewModel.ThumbnailVM")]
    [TestCase("NINA.View.ImageControlView", "NINA.ViewModel.ImageControlVM")]
    [TestCase("NINA.View.AnchorableCameraControlView", "NINA.ViewModel.Imaging.AnchorableSnapshotVM")]
    [TestCase("NINA.View.AnchorableSequenceView", "NINA.ViewModel.SimpleSequenceVM")]
    [TestCase("NINA.View.FramingAssistantView", "NINA.ViewModel.FramingAssistant.FramingAssistantVM")]
    [TestCase("NINA.View.FramingPlateSolvePromptView", "NINA.ViewModel.FramingAssistant.FramingPlateSolveParameter")]
    [TestCase("NINA.View.ManualRotatorView", "NINA.Equipment.Equipment.MyRotator.ManualRotator")]
    [TestCase("NINA.View.Options.ImagingView", "NINA.ViewModel.OptionsVM")]
    [TestCase("NINA.View.Options.PlateSolverView", "NINA.ViewModel.OptionsVM")]
    [TestCase("NINA.View.FlatWizardView", "NINA.ViewModel.FlatWizard.FlatWizardVM")]
    public void ViewFixture_UsesTheRequestedCompiledViewAndItsProductionViewModel(
            string viewType,
            string expectedViewModelType) {
        ScreenshotAsset asset = new() {
            Id = "production-view-" + viewType.Split('.').Last(),
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/test.png",
            Fixture = "view",
            State = "production-view",
            ViewType = viewType,
            Width = 800,
            Height = 600
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() {
            Width = asset.Width,
            Height = asset.Height,
            Content = fixture,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };
        try {
            host.Show();
            fixture.UpdateLayout();
            fixture.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));

            Assert.Multiple(() => {
                Assert.That(fixture.GetType().FullName, Is.EqualTo(viewType));
                Assert.That(fixture.DataContext?.GetType().FullName, Is.EqualTo(expectedViewModelType));
                Assert.That(fixture.DataContext, Is.Not.InstanceOf<NINA.Profile.Interfaces.IProfileService>());
            });
        } finally {
            host.Content = null;
            host.Close();
        }
    }

    [Test]
    public void OptionsView_UsesProductionOptionsModelWithBuiltInImagePatterns() {
        ScreenshotAsset asset = new() {
            Id = "imaging-options",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/Options-Imaging10.png",
            Fixture = "view",
            State = "imaging-options",
            ViewType = "NINA.View.Options.ImagingView",
            Width = 2796,
            Height = 1449
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        object viewModel = fixture.DataContext;
        object imagePatterns = viewModel.GetType().GetProperty("ImagePatterns")!.GetValue(viewModel)!;
        object items = imagePatterns.GetType().GetProperty("Items")!.GetValue(imagePatterns)!;

        Assert.Multiple(() => {
            Assert.That(viewModel.GetType().FullName, Is.EqualTo("NINA.ViewModel.OptionsVM"));
            Assert.That((System.Collections.ICollection)items, Has.Count.GreaterThan(20));
        });
    }

    [Test]
    public void ApplicationFixture_UsesCompiledMainWindowAndOptionsNavigation() {
        ScreenshotAsset asset = new() {
            Id = "plate-solving-options",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/OptionsPlateSolving10.png",
            Fixture = "application",
            State = "options-plate-solving",
            Width = 2367,
            Height = 1318
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        Window host = new() {
            Width = asset.Width,
            Height = asset.Height,
            Content = fixture,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };
        try {
            host.Show();
            fixture.UpdateLayout();
            fixture.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { }));

            TabControl mainTabs = FindDescendants(fixture).OfType<TabControl>()
                .Single(control => control.Name == "MainTabControl");
            NINA.View.Options.TabPage optionsPage = FindDescendants(fixture)
                .OfType<NINA.View.Options.TabPage>()
                .Single();
            TabControl optionsTabs = FindDescendants(optionsPage).OfType<TabControl>().Single();

            Assert.Multiple(() => {
                Assert.That(mainTabs.SelectedIndex, Is.EqualTo((int)NINA.Core.Enum.ApplicationTab.OPTIONS));
                Assert.That(optionsTabs.SelectedIndex, Is.EqualTo(5));
                Assert.That(optionsPage.DataContext?.GetType().FullName, Is.EqualTo("NINA.ViewModel.OptionsVM"));
                Assert.That(FindDescendants(optionsPage).Any(element => element is NINA.View.Options.PlateSolverView), Is.True);
            });
        } finally {
            host.Content = null;
            host.Close();
        }
    }

    [Test]
    public void ImagingOverview_UsesInitializedProductionDockManagerAndPanels() {
        ScreenshotAsset asset = new() {
            Id = "imaging-overview",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/imaging_topbar.png",
            Fixture = "view",
            State = "imaging-overview",
            ViewType = "NINA.View.OverView",
            Width = 1562,
            Height = 921
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        object viewModel = fixture.DataContext;
        System.Collections.ICollection anchorables = (System.Collections.ICollection)viewModel.GetType()
            .GetProperty("Anchorables")!.GetValue(viewModel)!;
        System.Collections.ICollection infoPanels = (System.Collections.ICollection)viewModel.GetType()
            .GetProperty("AnchorableInfoPanels")!.GetValue(viewModel)!;
        System.Collections.ICollection tools = (System.Collections.ICollection)viewModel.GetType()
            .GetProperty("AnchorableTools")!.GetValue(viewModel)!;

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.View.OverView>());
            Assert.That(viewModel.GetType().FullName, Is.EqualTo("NINA.ViewModel.DockManagerVM"));
            Assert.That((bool)viewModel.GetType().GetProperty("Initialized")!.GetValue(viewModel)!, Is.True);
            Assert.That(anchorables, Has.Count.GreaterThanOrEqualTo(20));
            Assert.That(infoPanels, Has.Count.GreaterThanOrEqualTo(15));
            Assert.That(tools, Has.Count.GreaterThanOrEqualTo(5));
        });
    }

    [Test]
    public void ImagingSequencePanel_UsesCompiledMiniSequencerAndProductionSequenceModel() {
        ScreenshotAsset asset = new() {
            Id = "imaging-sequence",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/imaging_sequence2.png",
            Fixture = "view",
            State = "imaging-sequence",
            ViewType = "NINA.View.AnchorableSequence2View",
            Width = 412,
            Height = 100
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        object viewModel = fixture.DataContext;

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.View.AnchorableSequence2View>());
            Assert.That(viewModel.GetType().FullName, Is.EqualTo("NINA.ViewModel.Sequencer.Sequence2VM"));
            Assert.That(viewModel.GetType().GetProperty("Sequencer")!.GetValue(viewModel), Is.Not.Null);
            Assert.That(viewModel.GetType().GetProperty("StartSequenceCommand")!.GetValue(viewModel), Is.Not.Null);
        });
    }

    [Test]
    public void ImageStatisticsView_UsesDeterministicProductionStatistics() {
        ScreenshotAsset asset = new() {
            Id = "image-statistics",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/imaging_statistics.png",
            Fixture = "view",
            State = "statistics",
            ViewType = "NINA.View.AnchorableImageStatisticsView",
            Width = 341,
            Height = 422
        };

        NINA.ViewModel.ImageStatisticsVM viewModel = (NINA.ViewModel.ImageStatisticsVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.Statistics, Is.Not.Null);
            Assert.That(viewModel.Statistics.ImageProperties.Width, Is.EqualTo(4656));
            Assert.That(viewModel.Statistics.ImageStatistics.Result.Mean, Is.EqualTo(3857.52));
            Assert.That(viewModel.Statistics.ImageStatistics.Result.Histogram[0].X, Is.EqualTo(0));
            Assert.That(viewModel.Statistics.ImageStatistics.Result.Histogram[^1].X, Is.EqualTo(100));
            Assert.That(viewModel.Statistics.StarDetectionAnalysis.DetectedStars, Is.EqualTo(129));
        });
    }

    [Test]
    public void ThumbnailView_UsesProductionThumbnailModelsAndBundledSampleImage() {
        ScreenshotAsset asset = new() {
            Id = "thumbnail-history",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/imaging_history.png",
            Fixture = "view",
            State = "thumbnail-history",
            ViewType = "NINA.View.ThumbnailListView",
            Width = 590,
            Height = 404
        };

        NINA.WPF.Base.Interfaces.ViewModel.IThumbnailVM viewModel =
            (NINA.WPF.Base.Interfaces.ViewModel.IThumbnailVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.Thumbnails, Has.Count.EqualTo(15));
            Assert.That(viewModel.Thumbnails.All(thumbnail => thumbnail.ThumbnailImage is not null), Is.True);
            Assert.That(viewModel.Thumbnails.All(thumbnail => thumbnail.Date.Date == new DateTime(2026, 8, 31)), Is.True);
            Assert.That(viewModel.SelectedThumbnail, Is.Null, "The fixture must not auto-scroll the production thumbnail list.");
        });
    }

    [Test]
    public void ImageControlView_UsesProductionRenderedImageLoadedFromBundledXisf() {
        ScreenshotAsset asset = new() {
            Id = "image-control",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/imaging_image.png",
            Fixture = "view",
            State = "image-control",
            ViewType = "NINA.View.ImageControlView",
            Width = 665,
            Height = 665
        };

        NINA.Equipment.Interfaces.ViewModel.IImageControlVM viewModel =
            (NINA.Equipment.Interfaces.ViewModel.IImageControlVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.RenderedImage, Is.Not.Null);
            Assert.That(viewModel.Image, Is.Not.Null);
            Assert.That(viewModel.RenderedImage.Image.PixelWidth, Is.GreaterThan(100));
            Assert.That(viewModel.RenderedImage.Image.PixelHeight, Is.GreaterThan(100));
        });
    }

    [Test]
    public void SnapshotView_UsesConnectedProductionCameraAndFilterState() {
        ScreenshotAsset asset = new() {
            Id = "snapshot",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/tabs/imaging_image1.png",
            Fixture = "view",
            State = "snapshot",
            ViewType = "NINA.View.AnchorableCameraControlView",
            Width = 386,
            Height = 410
        };

        NINA.WPF.Base.Interfaces.ViewModel.IAnchorableSnapshotVM viewModel =
            (NINA.WPF.Base.Interfaces.ViewModel.IAnchorableSnapshotVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.CameraInfo.Connected, Is.True);
            Assert.That(viewModel.CameraInfo.CanShowLiveView, Is.True);
            Assert.That(viewModel.FilterWheelInfo.Connected, Is.True);
            Assert.That(viewModel.SnapExposureDuration, Is.EqualTo(2.5));
            Assert.That(viewModel.SnapFilter.Name, Is.EqualTo("L"));
            Assert.That(viewModel.SnapGain, Is.EqualTo(50));
        });
    }

    [Test]
    public void CameraView_UsesCoolingCapabilitiesAndARealisticFixedHistory() {
        ScreenshotAsset asset = new() {
            Id = "equipment-camera",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/equipment_camera.png",
            Fixture = "view",
            State = "equipment-camera",
            ViewType = "NINA.View.Equipment.CameraView",
            Width = 1849,
            Height = 1089
        };

        NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM viewModel =
            (NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.Cam, Is.Not.Null);
            Assert.That(viewModel.Cam.CanSetTemperature, Is.True);
            Assert.That(viewModel.Cam.GainMin, Is.EqualTo(0));
            Assert.That(viewModel.Cam.GainMax, Is.EqualTo(300));
            Assert.That(viewModel.Cam.OffsetMin, Is.EqualTo(0));
            Assert.That(viewModel.Cam.OffsetMax, Is.EqualTo(100));
            Assert.That(viewModel.Cam.USBLimitMin, Is.EqualTo(0));
            Assert.That(viewModel.Cam.USBLimitMax, Is.EqualTo(100));
            Assert.That(viewModel.CameraInfo.CanSetTemperature, Is.True);
            Assert.That(viewModel.CoolerHistory, Has.Count.GreaterThanOrEqualTo(20));
            Assert.That(viewModel.CoolerHistory.Select(step => step.Date).Distinct().Count(), Is.GreaterThan(1));
            Assert.That(viewModel.CoolerHistoryMin, Is.LessThan(-10));
            Assert.That(viewModel.CoolerHistoryMax, Is.GreaterThan(0));
        });
    }

    [TestCase("camera-simulator-random", NINA.Core.Enum.CameraType.RANDOM, "Random Image Generation")]
    [TestCase("camera-simulator-image", NINA.Core.Enum.CameraType.IMAGE, "Load Image")]
    [TestCase("camera-simulator-sky-survey", NINA.Core.Enum.CameraType.SKYSURVEY, "Sky Survey")]
    [TestCase("camera-simulator-directory", NINA.Core.Enum.CameraType.DIRECTORY, "Load Directory")]
    public void CameraSimulatorSetup_UsesTheRealProductionViewAndSelectedSource(
            string state,
            NINA.Core.Enum.CameraType expectedType,
            string expectedPanel) {
        ScreenshotAsset asset = new() {
            Id = state,
            Classification = ScreenshotClassification.NinaUi,
            Output = $"docs/images/generated/advanced/camerasimulator/{state}.png",
            Fixture = "view",
            State = state,
            ViewType = "NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SetupView",
            Width = 800,
            Height = 450
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();

        NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera simulator =
            (NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera)fixture.DataContext;
        ComboBox sourceSelector = FindDescendants(fixture).OfType<ComboBox>()
            .Single(comboBox => comboBox.Name == "PART_Type");
        GroupBox selectedPanel = FindDescendants(fixture).OfType<GroupBox>()
            .Single(groupBox => Equals(groupBox.Header, expectedPanel));
        FrameworkElement selectedPanelContainer = (FrameworkElement)VisualTreeHelper.GetParent(selectedPanel);

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SetupView>());
            Assert.That(simulator.Settings.Type, Is.EqualTo(expectedType));
            Assert.That(sourceSelector.SelectedItem, Is.EqualTo(expectedType));
            Assert.That(selectedPanelContainer.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(FindDescendants(fixture).OfType<GroupBox>()
                .Where(groupBox => !Equals(groupBox.Header, expectedPanel))
                .Select(groupBox => ((FrameworkElement)VisualTreeHelper.GetParent(groupBox)).Visibility),
                Is.All.EqualTo(Visibility.Collapsed));
            Assert.That(simulator.Settings.ImageSettings.ImagePath, Is.EqualTo(@"C:\NINA\Simulator\M42.xisf"));
            Assert.That(simulator.Settings.DirectorySettings.DirectoryPath, Is.EqualTo(@"C:\NINA\Simulator\Images"));
        });
    }

    [Test]
    public void CameraSimulatorSelection_UsesTheRealCameraViewAndSimulatorDevice() {
        ScreenshotAsset asset = new() {
            Id = "camera-simulator-selection",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/advanced/camerasimulator/selection.png",
            Fixture = "view",
            State = "camera-simulator-selection",
            ViewType = "NINA.View.Equipment.CameraView",
            Width = 800,
            Height = 600
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);
        fixture.Measure(new Size(asset.Width, asset.Height));
        fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
        fixture.UpdateLayout();

        NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM camera =
            (NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM)fixture.DataContext;
        NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera simulator =
            (NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera)camera.DeviceChooserVM.SelectedDevice;
        NINA.View.Equipment.Connector connector = FindDescendants(fixture)
            .OfType<NINA.View.Equipment.Connector>()
            .Single();
        ComboBox sourceSelector = FindDescendants(connector).OfType<ComboBox>().Single();

        Assert.Multiple(() => {
            Assert.That(fixture, Is.TypeOf<NINA.View.Equipment.CameraView>());
            Assert.That(simulator.Name, Is.EqualTo("N.I.N.A. Simulator Camera"));
            Assert.That(simulator.HasSetupDialog, Is.True);
            Assert.That(camera.DeviceChooserVM.Devices, Does.Contain(simulator));
            Assert.That(sourceSelector.SelectedItem, Is.SameAs(simulator));
        });
    }

    [Test]
    public void GuiderView_UsesTheProductionPhd2DeviceAndNonRepeatingGuideData() {
        ScreenshotAsset asset = new() {
            Id = "equipment-guider",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/equipment_guider.png",
            Fixture = "view",
            State = "equipment-guider",
            ViewType = "NINA.View.Equipment.Guider.GuiderView",
            Width = 1861,
            Height = 1093
        };

        NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM viewModel =
            (NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.Guider.GetType().FullName,
                Is.EqualTo("NINA.Equipment.Equipment.MyGuider.PHD2.PHD2Guider"));
            Assert.That(viewModel.GuiderInfo.Connected, Is.True);
            Assert.That(viewModel.Guider.Connected, Is.True);
            Assert.That(viewModel.Guider.State, Is.EqualTo("Guiding"));
            Assert.That(viewModel.SettingsVisible, Is.True);
            Assert.That(viewModel.Guider.PixelScale, Is.EqualTo(1.42d));
            Assert.That(viewModel.MainCameraDitherPixels, Is.Not.NaN);
            Assert.That(viewModel.MainCameraDitherPixels, Is.GreaterThan(0));
            NINA.Equipment.Equipment.MyGuider.PHD2.PHD2Guider phd2 =
                (NINA.Equipment.Equipment.MyGuider.PHD2.PHD2Guider)viewModel.Guider;
            Assert.That(phd2.AvailableProfiles.Select(profile => profile.Name),
                Does.Contain("Documentation Equipment"));
            Assert.That(phd2.SelectedProfile?.Name, Is.EqualTo("Documentation Equipment"));
            Assert.That(viewModel.GuideStepsHistory.GuideSteps.Count(), Is.GreaterThanOrEqualTo(100));
        });
    }

    [Test]
    public void FlatWizardView_UsesConnectedProductionEquipmentState() {
        ScreenshotAsset asset = new() {
            Id = "flat-wizard",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/flatwizard1.png",
            Fixture = "view",
            State = "flat-wizard",
            ViewType = "NINA.View.FlatWizardView",
            Width = 1435,
            Height = 837
        };

        object viewModel = new FixtureRegistry().Create(asset).DataContext;
        Type viewModelType = viewModel.GetType();

        Assert.Multiple(() => {
            Assert.That(viewModelType.FullName, Is.EqualTo("NINA.ViewModel.FlatWizard.FlatWizardVM"));
            Assert.That(viewModelType.GetProperty("TargetName")!.GetValue(viewModel), Is.EqualTo("FlatWizard"));
            Assert.That(viewModelType.GetProperty("CameraConnected")!.GetValue(viewModel), Is.True);
            Assert.That(viewModelType.GetProperty("CalculatedExposureTime")!.GetValue(viewModel), Is.EqualTo(1.25d));
            Assert.That(viewModelType.GetProperty("CalculatedHistogramMean")!.GetValue(viewModel), Is.EqualTo(26214d));
            object? activeInstruction = viewModelType.GetProperty("ActiveFlatInstruction")!.GetValue(viewModel);
            Assert.That(activeInstruction, Is.InstanceOf<NINA.Sequencer.SequenceItem.FlatDevice.AutoExposureFlat>());
            NINA.Sequencer.SequenceItem.FlatDevice.AutoExposureFlat autoExposure =
                (NINA.Sequencer.SequenceItem.FlatDevice.AutoExposureFlat)activeInstruction!;
            Assert.That(autoExposure.GetExposureItem().ExposureTime, Is.EqualTo(1.25d));
            Assert.That(autoExposure.DeterminedHistogramADU, Is.EqualTo(26214d));
        });
    }

    [Test]
    public void LoopConditionsExample_UsesTimeConditionAndRgbInstructions() {
        ScreenshotAsset asset = new() {
            Id = "loop-conditions",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/sequencer/Sequencer_LoopConditions.png",
            Fixture = "sequencer",
            State = "sequencer-loop-conditions",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1451,
            Height = 454
        };

        NINA.ViewModel.Sequencer.ISequence2VM viewModel =
            (NINA.ViewModel.Sequencer.ISequence2VM)new FixtureRegistry().Create(asset).DataContext;
        NINA.Sequencer.Container.ISequenceContainer targetArea =
            (NINA.Sequencer.Container.ISequenceContainer)viewModel.Sequencer.MainContainer.Items[1];
        NINA.Sequencer.Container.ISequenceContainer example =
            (NINA.Sequencer.Container.ISequenceContainer)targetArea.Items.Single();

        Assert.Multiple(() => {
            Assert.That(((NINA.Sequencer.Conditions.IConditionable)example).Conditions,
                Has.Exactly(1).TypeOf<NINA.Sequencer.Conditions.TimeCondition>());
            Assert.That(example.Items, Has.Count.EqualTo(6));
            Assert.That(example.Items.OfType<NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter>()
                .Select(item => item.ComboBoxText), Is.EqualTo(new[] { "R", "G", "B" }));
            Assert.That(example.Items.OfType<NINA.Sequencer.SequenceItem.Imaging.TakeExposure>().Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public void SwitchView_UsesDeterministicProductionSwitchInterfaces() {
        ScreenshotAsset asset = new() {
            Id = "switches",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/tabs/equipment_switches.png",
            Fixture = "view",
            State = "switches",
            ViewType = "NINA.View.Equipment.SwitchHubView",
            Width = 1441,
            Height = 1003
        };

        NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM viewModel =
            (NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM)new FixtureRegistry().Create(asset).DataContext;

        Assert.Multiple(() => {
            Assert.That(viewModel.ReadonlySwitches, Has.Count.EqualTo(8));
            Assert.That(viewModel.WritableSwitches, Has.Count.EqualTo(8));
            Assert.That(viewModel.SwitchInfo.Connected, Is.True);
            Assert.That(viewModel.ReadonlySwitches, Has.Count.GreaterThanOrEqualTo(4));
            Assert.That(viewModel.WritableSwitches, Has.Count.GreaterThanOrEqualTo(4));
            Assert.That(viewModel.WritableSwitches.Select(item => item.Name), Does.Contain("Flat Panel"));
        });
    }

    [Test]
    public void EquipmentTabPage_UsesConcreteProductionChildViewModelsWithoutBindingErrors() {
        ScreenshotAsset asset = new() {
            Id = "equipment-tab",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/quickstart/uioverview1.png",
            Fixture = "view",
            State = "equipment-tab",
            ViewType = "NINA.View.Equipment.TabPage",
            Width = 1729,
            Height = 1022
        };

        FrameworkElement fixture = new FixtureRegistry().Create(asset);

        Assert.DoesNotThrow(() => {
            fixture.Measure(new Size(asset.Width, asset.Height));
            fixture.Arrange(new Rect(0, 0, asset.Width, asset.Height));
            fixture.UpdateLayout();
        });
        object equipmentViewModel = fixture.DataContext;
        foreach (System.Reflection.PropertyInfo property in equipmentViewModel.GetType().GetProperties()
            .Where(property => property.Name.EndsWith("VM", StringComparison.Ordinal))) {
            Assert.That(property.GetValue(equipmentViewModel)?.GetType().Name, Is.EqualTo(property.Name),
                $"{property.Name} must be its production implementation, not a dispatch proxy.");
        }
    }

    private static IEnumerable<DependencyObject> FindDescendants(DependencyObject root) {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++) {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (DependencyObject descendant in FindDescendants(child)) {
                yield return descendant;
            }
        }
    }
}
