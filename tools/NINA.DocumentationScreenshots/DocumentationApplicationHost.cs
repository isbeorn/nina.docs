#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NINA.Core.Enum;
using NINA.Core.Locale;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.Sequencer;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces;
using NINA.Sequencer.Logic;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.SequenceItem.Camera;
using NINA.Sequencer.SequenceItem.FlatDevice;
using NINA.Sequencer.SequenceItem.Focuser;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.SequenceItem.Telescope;
using NINA.Sequencer.SequenceItem.Utility;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Utility.DateTimeProvider;
using NINA.Plugin.Interfaces;
using NINA.View.Sequencer;
using NINA.View.Sequencer.AdvancedSequencer;
using NINA.View;
using NINA.ViewModel.ImageHistory;
using NINA.ViewModel.Sequencer;
using NINA.WPF.Base.Model;
using NINA.WPF.Base.Utility.AutoFocus;
using NINA.WPF.Base.ViewModel.AutoFocus;
using OxyPlot.Series;
using DataPoint = OxyPlot.DataPoint;

namespace NINA.DocumentationScreenshots;

/// <summary>
/// Supplies deterministic data and inert services to NINA's real compiled views.
/// This class deliberately contains no screenshot-specific visual layout.
/// </summary>
public sealed class DocumentationApplicationHost {
    internal static NINA.Core.Utility.ICustomDateTime FixedDateTime => DocumentationFixedDateTime.Instance;

    public static DocumentationApplicationHost Instance { get; } = new();
    private ISymbolBroker? symbolBroker;
    private static NINA.Image.Interfaces.IRenderedImage? documentationSampleRenderedImage;

    private DocumentationApplicationHost() {
    }

    public FrameworkElement CreateApplicationView(ScreenshotAsset asset) {
        Type applicationViewModelType = ResolveProductionType("NINA.ViewModel.ApplicationVM")
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's production ApplicationVM.");
        Type mainWindowViewModelType = ResolveProductionType("NINA.ViewModel.MainWindowVM")
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's production MainWindowVM.");
        Type optionsViewModelType = ResolveProductionType("NINA.ViewModel.OptionsVM")
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's production OptionsVM.");

        object applicationViewModel = CreateWithInertServices(applicationViewModelType);
        applicationViewModelType.GetProperty("TabIndex")!.SetValue(
            applicationViewModel,
            asset.State?.StartsWith("options-", StringComparison.OrdinalIgnoreCase) == true
                ? (int)ApplicationTab.OPTIONS
                : (int)ApplicationTab.EQUIPMENT);
        object optionsViewModel = CreateWithInertServices(optionsViewModelType);
        object mainWindowViewModel = Activator.CreateInstance(mainWindowViewModelType, nonPublic: true)
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not construct NINA's production MainWindowVM.");
        mainWindowViewModelType.GetProperty("AppVM")!.SetValue(mainWindowViewModel, applicationViewModel);
        mainWindowViewModelType.GetProperty("OptionsVM")!.SetValue(mainWindowViewModel, optionsViewModel);

        NINA.MainWindow mainWindow = new() { DataContext = mainWindowViewModel };
        TabControl mainTabs = (TabControl)(mainWindow.FindName("MainTabControl")
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find MainTabControl in NINA's compiled MainWindow."));
        mainTabs.SelectedIndex = (int)applicationViewModelType.GetProperty("TabIndex")!.GetValue(applicationViewModel)!;

        if (mainTabs.SelectedIndex == (int)ApplicationTab.OPTIONS) {
            NINA.View.Options.TabPage optionsPage = ((TabItem)mainTabs.Items[(int)ApplicationTab.OPTIONS]).Content
                as NINA.View.Options.TabPage
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's compiled Options tab page.");
            TabControl optionsTabs = optionsPage.Content as TabControl
                ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's compiled Options navigation.");
            optionsTabs.SelectedIndex = asset.State?.Equals("options-plate-solving", StringComparison.OrdinalIgnoreCase) == true ? 5 : 4;
        }

        FrameworkElement content = mainWindow.Content as FrameworkElement
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not extract NINA's compiled MainWindow content.");
        mainWindow.Content = null;
        content.DataContext = mainWindowViewModel;
        return content;
    }

    public FrameworkElement CreateProductionView(ScreenshotAsset asset) {
        string viewTypeName = asset.ViewType ?? asset.State
            ?? throw new CatalogException($"Screenshot '{asset.Id}' must specify a production view type.");
        if (viewTypeName == typeof(AnchorableImageHistoryView).FullName) {
            return CreateImageHistoryChart();
        }
        if (viewTypeName == "NINA.View.FramingAssistantView") {
            _ = LoadDocumentationSampleImage(asset.Id);
        }

        FrameworkElement view = InstantiateProductionView(viewTypeName, asset.Id);
        if (viewTypeName == "NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SetupView") {
            view.DataContext = CreateCameraSimulator(asset.State, asset.Id);
            return view;
        }
        if (viewTypeName == "NINA.View.Equipment.CameraView"
                && asset.State?.Equals("camera-simulator-selection", StringComparison.OrdinalIgnoreCase) == true) {
            view.DataContext = CreateCameraSimulatorViewModel(asset.Id);
            return view;
        }
        foreach (NINA.WPF.Base.View.BrowserPopupButton helpButton in
            EnumerateLogicalDescendants<NINA.WPF.Base.View.BrowserPopupButton>(view)) {
            ClearBrowserPopupSizingBindings(helpButton);
        }
        string? viewModelTypeName = viewTypeName switch {
            "NINA.View.AnchorableAutoFocusView" => "NINA.Imaging.ViewModel.Imaging.AutoFocusToolVM",
            "NINA.View.AnchorableGuiderView" => "NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM",
            "NINA.View.AnchorableImageStatisticsView" => "NINA.ViewModel.ImageStatisticsVM",
            "NINA.View.AnchorableFocusTargetsView" => "NINA.ViewModel.FocusTargetsVM",
            "NINA.View.AnchorableRotatorView" => "NINA.WPF.Base.ViewModel.Equipment.Rotator.RotatorVM",
            "NINA.View.AnchorableSwitchHubView" => "NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM",
            "NINA.View.AnchorableTelescopeView" => "NINA.WPF.Base.ViewModel.Equipment.Telescope.TelescopeVM",
            "NINA.View.AnchorableWeatherDataView" => "NINA.WPF.Base.ViewModel.Equipment.WeatherData.WeatherDataVM",
            "NINA.View.Equipment.CameraView" => "NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM",
            "NINA.View.Equipment.TelescopeView" => "NINA.WPF.Base.ViewModel.Equipment.Telescope.TelescopeVM",
            "NINA.View.Equipment.WeatherDataView" => "NINA.WPF.Base.ViewModel.Equipment.WeatherData.WeatherDataVM",
            "NINA.View.Equipment.Guider.GuiderView" => "NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM",
            "NINA.View.Equipment.DomeView" => "NINA.WPF.Base.ViewModel.Equipment.Dome.DomeVM",
            "NINA.View.Equipment.RotatorView" => "NINA.WPF.Base.ViewModel.Equipment.Rotator.RotatorVM",
            "NINA.View.Equipment.SwitchHubView" => "NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM",
            "NINA.View.Equipment.TabPage" => "NINA.ViewModel.EquipmentVM",
            "NINA.View.ThumbnailListView" => "NINA.ViewModel.ThumbnailVM",
            "NINA.View.ImageControlView" => "NINA.ViewModel.ImageControlVM",
            "NINA.View.AnchorableCameraControlView" => "NINA.ViewModel.Imaging.AnchorableSnapshotVM",
            "NINA.View.AnchorableSequenceView" => "NINA.ViewModel.SimpleSequenceVM",
            "NINA.View.AnchorableSequence2View" => "NINA.ViewModel.Sequencer.Sequence2VM",
            "NINA.View.SimpleSequencer.SimpleSequenceView" => "NINA.ViewModel.SimpleSequenceVM",
            "NINA.View.FramingAssistantView" => "NINA.ViewModel.FramingAssistant.FramingAssistantVM",
            "NINA.View.FramingPlateSolvePromptView" => "NINA.ViewModel.FramingAssistant.FramingPlateSolveParameter",
            "NINA.View.ManualRotatorView" => "NINA.Equipment.Equipment.MyRotator.ManualRotator",
            "NINA.View.SkyAtlasView" => "NINA.ViewModel.SkyAtlasVM",
            "NINA.View.Options.AutoFocusView" => "NINA.ViewModel.OptionsVM",
            "NINA.View.Options.EquipmentView" => "NINA.ViewModel.OptionsVM",
            "NINA.View.Options.GeneralView" => "NINA.ViewModel.OptionsVM",
            "NINA.View.Options.ImagingView" => "NINA.ViewModel.OptionsVM",
            "NINA.View.Options.PlateSolverView" => "NINA.ViewModel.OptionsVM",
            "NINA.View.FlatWizardView" => "NINA.ViewModel.FlatWizard.FlatWizardVM",
            "NINA.View.OverView" => "NINA.ViewModel.DockManagerVM",
            _ => null
        };
        if (viewModelTypeName is null) {
            view.DataContext = Application.Current?.Resources["ProfileService"];
            return view;
        }

        Type viewModelType = ResolveProductionType(viewModelTypeName)
            ?? throw new CatalogException($"Screenshot '{asset.Id}' refers to unavailable production view model '{viewModelTypeName}'.");
        object viewModel = viewModelTypeName switch {
            "NINA.ViewModel.EquipmentVM" => CreateEquipmentViewModel(viewModelType),
            "NINA.ViewModel.DockManagerVM" => CreateDockManagerViewModel(viewModelType, asset.Id),
            "NINA.ViewModel.Sequencer.Sequence2VM" => CreateAdvancedSequenceViewModel(asset.Id),
            "NINA.ViewModel.SimpleSequenceVM" when viewTypeName == "NINA.View.SimpleSequencer.SimpleSequenceView"
                => CreateSimpleSequenceViewModel(viewModelType, asset.Id),
            "NINA.ViewModel.FramingAssistant.FramingAssistantVM" => CreateFramingAssistantViewModel(viewModelType, asset),
            "NINA.ViewModel.FramingAssistant.FramingPlateSolveParameter" => CreateFramingPlateSolveParameter(),
            "NINA.Equipment.Equipment.MyRotator.ManualRotator" => CreateManualRotator(),
            "NINA.ViewModel.SkyAtlasVM" => CreateSkyAtlasViewModel(viewModelType, asset.Id),
            _ => CreateWithInertServices(viewModelType)
        };
        PopulateProductionViewModel(viewModel);
        view.DataContext = viewModel;
        return view;
    }

    private static NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera CreateCameraSimulator(
            string? state,
            string screenshotId) {
        NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera simulator = new(
            GetProfileService(),
            (NINA.Equipment.Interfaces.Mediator.ITelescopeMediator)InertValue.Create(
                typeof(NINA.Equipment.Interfaces.Mediator.ITelescopeMediator))!,
            (NINA.Image.Interfaces.IExposureDataFactory)InertValue.Create(
                typeof(NINA.Image.Interfaces.IExposureDataFactory))!,
            (NINA.Image.Interfaces.IImageDataFactory)InertValue.Create(
                typeof(NINA.Image.Interfaces.IImageDataFactory))!);

        simulator.Settings.RandomSettings.ImageWidth = 640;
        simulator.Settings.RandomSettings.ImageHeight = 480;
        simulator.Settings.RandomSettings.ImageMean = 5000;
        simulator.Settings.RandomSettings.ImageStdDev = 100;
        simulator.Settings.ImageSettings.IsBayered = true;
        simulator.Settings.ImageSettings.ImagePath = @"C:\NINA\Simulator\M42.xisf";
        simulator.Settings.SkySurveySettings.FieldOfView = 1;
        simulator.Settings.SkySurveySettings.RAError = 45;
        simulator.Settings.SkySurveySettings.DecError = -30;
        simulator.Settings.SkySurveySettings.AzShift = 60;
        simulator.Settings.SkySurveySettings.AltShift = -15;
        simulator.Settings.DirectorySettings.DirectoryPath = @"C:\NINA\Simulator\Images";
        simulator.Settings.Type = state?.ToLowerInvariant() switch {
            "camera-simulator-random" => NINA.Core.Enum.CameraType.RANDOM,
            "camera-simulator-image" => NINA.Core.Enum.CameraType.IMAGE,
            "camera-simulator-sky-survey" => NINA.Core.Enum.CameraType.SKYSURVEY,
            "camera-simulator-directory" => NINA.Core.Enum.CameraType.DIRECTORY,
            _ => throw new CatalogException(
                $"Screenshot '{screenshotId}' has unknown camera simulator state '{state}'.")
        };
        return simulator;
    }

    private static NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM CreateCameraSimulatorViewModel(
            string screenshotId) {
        NINA.WPF.Base.Model.Equipment.MyCamera.Simulator.SimulatorCamera simulator =
            CreateCameraSimulator("camera-simulator-random", screenshotId);
        DocumentationCameraChooserVM chooser = new(
            GetProfileService(),
            simulator,
            (NINA.Equipment.Interfaces.ViewModel.IEquipmentProviders<NINA.Equipment.Interfaces.ICamera>)
                InertValue.Create(typeof(NINA.Equipment.Interfaces.ViewModel.IEquipmentProviders<NINA.Equipment.Interfaces.ICamera>))!);
        NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM camera = new(
            GetProfileService(),
            (NINA.Equipment.Interfaces.Mediator.ICameraMediator)InertValue.Create(
                typeof(NINA.Equipment.Interfaces.Mediator.ICameraMediator))!,
            (NINA.Equipment.Interfaces.Mediator.IFilterWheelMediator)InertValue.Create(
                typeof(NINA.Equipment.Interfaces.Mediator.IFilterWheelMediator))!,
            (NINA.WPF.Base.Interfaces.Mediator.IApplicationStatusMediator)InertValue.Create(
                typeof(NINA.WPF.Base.Interfaces.Mediator.IApplicationStatusMediator))!,
            chooser);
        SetNonPublicProperty(camera, nameof(camera.Cam), simulator);
        camera.CameraInfo = new NINA.Equipment.Equipment.MyCamera.CameraInfo {
            Connected = false,
            Name = simulator.Name,
            Description = simulator.Description,
            DriverVersion = simulator.DriverVersion,
            SensorType = simulator.SensorType
        };
        return camera;
    }

    private object CreateDockManagerViewModel(Type dockManagerViewModelType, string screenshotId) {
        ISymbolBroker productionSymbolBroker = GetSymbolBroker();
        DocumentationPluginLoader pluginLoader = new(
            CreateExported<ISequenceItem>(productionSymbolBroker),
            CreateExported<ISequenceCondition>(productionSymbolBroker),
            CreateExported<ISequenceTrigger>(productionSymbolBroker),
            CreateExported<ISequenceContainer>(productionSymbolBroker));
        ConstructorInfo constructor = dockManagerViewModelType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single();
        object?[] arguments = constructor.GetParameters().Select(parameter => {
            if (parameter.ParameterType == typeof(IProfileService)) {
                return (object?)GetProfileService();
            }
            if (parameter.ParameterType.Name == "IPluginLoader") {
                return pluginLoader;
            }
            Type implementation = FindProductionImplementation(parameter.ParameterType)
                ?? throw new CatalogException(
                    $"Screenshot '{screenshotId}' could not find the production dock panel for '{parameter.ParameterType.FullName}'.");
            object childViewModel = parameter.ParameterType.Name == "ISequenceNavigationVM"
                ? CreateSequenceNavigationViewModel(implementation, pluginLoader, productionSymbolBroker, screenshotId)
                : CreateWithInertServices(implementation);
            PopulateProductionViewModel(childViewModel);
            return childViewModel;
        }).ToArray();
        object viewModel = constructor.Invoke(arguments);
        PropertyInfo initialized = dockManagerViewModelType.GetProperty("Initialized")
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not inspect DockManagerVM initialization.");
        WaitForInitializationWithDispatcher(viewModel, initialized, "DockManagerVM", screenshotId);
        return viewModel;
    }

    private static object CreateSequenceNavigationViewModel(
            Type viewModelType,
            IPluginLoader pluginLoader,
            ISymbolBroker productionSymbolBroker,
            string screenshotId) {
        ConstructorInfo constructor = viewModelType.GetConstructors().Single();
        object?[] arguments = constructor.GetParameters().Select(parameter => {
            if (parameter.ParameterType == typeof(IProfileService)) {
                return (object?)GetProfileService();
            }
            if (parameter.ParameterType == typeof(IPluginLoader)) {
                return pluginLoader;
            }
            if (parameter.ParameterType == typeof(ISymbolBroker)) {
                return productionSymbolBroker;
            }
            return InertValue.Create(parameter.ParameterType);
        }).ToArray();
        object viewModel = constructor.Invoke(arguments);
        PropertyInfo initialized = viewModelType.GetProperty("Initialized")
            ?? throw new CatalogException($"Screenshot '{screenshotId}' could not inspect SequenceNavigationVM initialization.");
        WaitForInitializationWithDispatcher(viewModel, initialized, "SequenceNavigationVM", screenshotId);
        return viewModel;
    }

    private static void WaitForInitializationWithDispatcher(
            object viewModel,
            PropertyInfo initialized,
            string viewModelName,
            string screenshotId) {
        Task wait = Task.Run(async () => {
            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            while (initialized.GetValue(viewModel) is not true) {
                if (DateTime.UtcNow >= deadline) {
                    throw new CatalogException(
                        $"Screenshot '{screenshotId}' timed out while NINA's production {viewModelName} initialized.");
                }
                await Task.Delay(20).ConfigureAwait(false);
            }
        });
        WaitWithDispatcher(wait, screenshotId);
    }

    private static object CreateEquipmentViewModel(Type equipmentViewModelType) {
        ConstructorInfo constructor = equipmentViewModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single();
        object?[] arguments = constructor.GetParameters().Select(parameter => {
            if (parameter.ParameterType == typeof(IProfileService)) {
                return (object?)(Application.Current?.Resources["ProfileService"]
                    ?? throw new CatalogException("NINA's isolated profile service was not initialized."));
            }
            Type implementation = FindProductionImplementation(parameter.ParameterType)
                ?? throw new CatalogException($"No production equipment view model implements '{parameter.ParameterType.FullName}'.");
            object childViewModel = CreateWithInertServices(implementation);
            PopulateProductionViewModel(childViewModel);
            return childViewModel;
        }).ToArray();
        return constructor.Invoke(arguments);
    }

    private static Type? FindProductionImplementation(Type contract) {
        string expectedName = contract.Name.StartsWith('I') ? contract.Name[1..] : contract.Name;
        return new[] {
            typeof(NINA.App).Assembly,
            typeof(NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM).Assembly
        }.SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && contract.IsAssignableFrom(type))
            .FirstOrDefault(type => type.Name == expectedName);
    }

    private static object CreateSimpleSequenceViewModel(Type viewModelType, string screenshotId) {
        IProfileService profileService = GetProfileService();
        SequencerFactory factory = CreateSequencerFactory(profileService, Instance.GetSymbolBroker());
        ConstructorInfo constructor = viewModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single();
        object?[] arguments = constructor.GetParameters().Select(parameter =>
            parameter.ParameterType == typeof(ISequencerFactory)
                ? (object?)factory
                : InertValue.Create(parameter.ParameterType)).ToArray();
        NINA.ViewModel.Interfaces.ISimpleSequenceVM viewModel =
            (NINA.ViewModel.Interfaces.ISimpleSequenceVM)constructor.Invoke(arguments);
        WaitWithDispatcher(viewModel.Initialize(), screenshotId);

        NINA.Astrometry.DeepSkyObject flats = new(
            "Flats",
            new NINA.Astrometry.Coordinates(
                NINA.Astrometry.Angle.ByHours(0),
                NINA.Astrometry.Angle.ByDegree(0),
                NINA.Astrometry.Epoch.J2000),
            profileService.ActiveProfile.AstrometrySettings.Horizon);
        viewModel.AddTarget(flats);
        NINA.Sequencer.Container.SimpleDSOContainer target =
            (NINA.Sequencer.Container.SimpleDSOContainer)viewModel.SelectedTarget;
        target.Name = "Flats";
        target.Target.TargetName = "Flats";
        target.Items.Clear();

        NINA.Core.Model.Equipment.FilterInfo[] filters = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters
            .Take(4)
            .ToArray();
        foreach (NINA.Core.Model.Equipment.FilterInfo filter in filters) {
            NINA.ViewModel.Sequencer.SimpleSequence.SimpleExposure exposure =
                (NINA.ViewModel.Sequencer.SimpleSequence.SimpleExposure)target.AddSimpleExposure();
            ((NINA.Sequencer.SequenceItem.FilterWheel.SwitchFilter)exposure.GetSwitchFilter()).Filter = filter;
            NINA.Sequencer.SequenceItem.Imaging.TakeExposure takeExposure =
                (NINA.Sequencer.SequenceItem.Imaging.TakeExposure)exposure.GetTakeExposure();
            takeExposure.ExposureTime = 2.5;
            takeExposure.ImageType = "FLAT";
            takeExposure.Gain = 50;
            takeExposure.Offset = 20;
            takeExposure.Binning = new NINA.Core.Model.Equipment.BinningMode(1, 1);
            ((NINA.Sequencer.Conditions.LoopCondition)exposure.GetLoopCondition()).Iterations = 15;
            exposure.Dither = false;
        }
        viewModel.SelectedTarget = target;
        return viewModel;
    }

    private static object CreateSkyAtlasViewModel(Type viewModelType, string screenshotId) {
        object viewModel = CreateWithInertServices(viewModelType);
        BitmapSource sample = LoadDocumentationSampleImage(screenshotId);
        Func<NINA.Astrometry.SkyObjectBase, Task<BitmapSource>> imageFactory = _ => Task.FromResult(sample);
        NINA.Profile.Profile profile = GetActiveProfile();
        DateTime referenceDate = new(2026, 8, 31, 12, 0, 0, DateTimeKind.Local);

        NINA.Astrometry.DeepSkyObject[] objects = [
            CreateDocumentationDeepSkyObject("M 51", "Whirlpool Galaxy", 13.497, 47.195, "GALAXY", "CVN", 8.4, 11.2, imageFactory, profile, referenceDate),
            CreateDocumentationDeepSkyObject("M 31", "Andromeda Galaxy", 0.712, 41.269, "GALAXY", "AND", 3.4, 190, imageFactory, profile, referenceDate),
            CreateDocumentationDeepSkyObject("M 42", "Orion Nebula", 5.588, -5.391, "NEBULA", "ORI", 4.0, 85, imageFactory, profile, referenceDate)
        ];
        NINA.Core.Model.PagedList<NINA.Astrometry.DeepSkyObject> result = new(10, objects) {
            SelectedItem = objects[0]
        };
        PropertyInfo nighttimeDataProperty = viewModelType.GetProperty("NighttimeData")!;
        if (!SpinWait.SpinUntil(() => nighttimeDataProperty.GetValue(viewModel) is not null, TimeSpan.FromSeconds(2))) {
            throw new CatalogException($"Screenshot '{screenshotId}' timed out waiting for Sky Atlas initialization.");
        }
        nighttimeDataProperty.SetValue(viewModel, DocumentationNighttimeCalculator.Instance.Calculate(referenceDate));
        viewModelType.GetProperty("FilterDate")!.SetValue(viewModel, referenceDate);
        viewModelType.GetProperty("SearchResult")!.SetValue(viewModel, result);
        return viewModel;
    }

    private static object CreateFramingAssistantViewModel(Type viewModelType, ScreenshotAsset asset) {
        Type deepSkyObjectSearchType = ResolveProductionType("NINA.ViewModel.DeepSkyObjectSearchVM")
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find NINA's production DeepSkyObjectSearchVM.");
        object deepSkyObjectSearch = Activator.CreateInstance(deepSkyObjectSearchType)
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not construct NINA's production DeepSkyObjectSearchVM.");
        ConstructorInfo constructor = viewModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single();
        object?[] arguments = constructor.GetParameters().Select(parameter =>
            parameter.ParameterType.Name == "IDeepSkyObjectSearchVM"
                ? deepSkyObjectSearch
                : InertValue.Create(parameter.ParameterType)).ToArray();
        object viewModel;
        try {
            viewModel = constructor.Invoke(arguments);
        } catch (Exception ex) {
            throw new CatalogException(
                $"Screenshot '{asset.Id}' could not construct NINA's production FramingAssistantVM: {ex.GetBaseException().Message}");
        }

        NINA.Profile.Profile profile = GetActiveProfile();
        NINA.Astrometry.Coordinates coordinates = new(
            NINA.Astrometry.Angle.ByHours(0.712),
            NINA.Astrometry.Angle.ByDegree(41.269),
            NINA.Astrometry.Epoch.J2000);
        NINA.Astrometry.DeepSkyObject target = new(
            "M 31 Andromeda Galaxy",
            coordinates,
            profile.AstrometrySettings.Horizon);
        target.RotationPositionAngle = 30;
        target.SetDateAndPosition(
            new DateTime(2026, 8, 31, 12, 0, 0, DateTimeKind.Local),
            profile.AstrometrySettings.Latitude,
            profile.AstrometrySettings.Longitude);
        viewModelType.GetProperty("DSO")!.SetValue(viewModel, target);
        deepSkyObjectSearchType.GetMethod("SetTargetNameWithoutSearch")!
            .Invoke(deepSkyObjectSearch, [target.Name]);

        BitmapSource image = LoadDocumentationSampleImage(asset.Id);
        const double verticalFieldOfViewDegrees = 4;
        double horizontalFieldOfViewDegrees = verticalFieldOfViewDegrees * image.PixelWidth / image.PixelHeight;
        NINA.WPF.Base.SkySurvey.SkySurveyImage surveyImage = new() {
            Id = new Guid("2d9c72e8-5696-4a82-99bb-a62ed3ee68a0"),
            Source = "Bundled offline documentation image",
            Image = image,
            FoVWidth = NINA.Astrometry.AstroUtil.DegreeToArcmin(horizontalFieldOfViewDegrees),
            FoVHeight = NINA.Astrometry.AstroUtil.DegreeToArcmin(verticalFieldOfViewDegrees),
            Rotation = 0,
            Coordinates = coordinates,
            Name = target.Name
        };

        SetProperty(viewModel, "FramingAssistantSource", SkySurveySource.HIPS2FITS);
        SetProperty(viewModel, "FieldOfView", verticalFieldOfViewDegrees);
        SetProperty(viewModel, "CameraWidth", 4656);
        SetProperty(viewModel, "CameraHeight", 3520);
        SetProperty(viewModel, "CameraPixelSize", 3.76d);
        SetProperty(viewModel, "FocalLength", 600d);
        SetProperty(viewModel, "SelectedOverlapUnit", "%");
        SetProperty(viewModel, "OverlapPercentage", 0.2d);
        bool mosaic = asset.Id.Contains("mosaic", StringComparison.OrdinalIgnoreCase);
        SetProperty(viewModel, "HorizontalPanels", mosaic ? 2 : 1);
        SetProperty(viewModel, "VerticalPanels", mosaic ? 2 : 1);
        SetProperty(viewModel, "Opacity", 0.22d);
        viewModelType.GetProperty("ImageParameter")!.SetValue(viewModel, surveyImage);

        NINA.Astrometry.ViewportFoV viewport = new(
            coordinates,
            verticalFieldOfViewDegrees,
            image.PixelWidth,
            image.PixelHeight,
            surveyImage.Rotation);
        MethodInfo calculateRectangle = viewModelType.GetMethod(
            "CalculateRectangle",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            [typeof(NINA.Astrometry.ViewportFoV), typeof(bool)],
            modifiers: null)
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not find FramingAssistantVM.CalculateRectangle.");
        calculateRectangle.Invoke(viewModel, [viewport, true]);
        SetProperty(viewModel, "RectangleTotalRotation", 30d);
        return viewModel;
    }

    private static NINA.ViewModel.FramingAssistant.FramingPlateSolveParameter CreateFramingPlateSolveParameter() => new(
        new NINA.Astrometry.Coordinates(
            NINA.Astrometry.Angle.ByHours(0.712),
            NINA.Astrometry.Angle.ByDegree(41.269),
            NINA.Astrometry.Epoch.J2000),
        600,
        3.76,
        1);

    private static NINA.Equipment.Equipment.MyRotator.ManualRotator CreateManualRotator() => new(GetProfileService()) {
        Connected = true,
        Position = 0,
        TargetPosition = 40
    };

    private static NINA.Astrometry.DeepSkyObject CreateDocumentationDeepSkyObject(
            string id,
            string name,
            double rightAscensionHours,
            double declinationDegrees,
            string objectType,
            string constellation,
            double magnitude,
            double size,
            Func<NINA.Astrometry.SkyObjectBase, Task<BitmapSource>> imageFactory,
            NINA.Profile.Profile profile,
            DateTime referenceDate) {
        NINA.Astrometry.DeepSkyObject result = new(
            id,
            new NINA.Astrometry.Coordinates(
                NINA.Astrometry.Angle.ByHours(rightAscensionHours),
                NINA.Astrometry.Angle.ByDegree(declinationDegrees),
                NINA.Astrometry.Epoch.J2000),
            imageFactory,
            profile.AstrometrySettings.Horizon) {
            Name = name,
            DSOType = objectType,
            Constellation = constellation,
            Magnitude = magnitude,
            Size = size,
            SizeMin = size * 0.7,
            SurfaceBrightness = 12.9,
            RotationPositionAngle = 0
        };
        result.AlsoKnownAs = [id];
        result.SetDateAndPosition(referenceDate, profile.AstrometrySettings.Latitude, profile.AstrometrySettings.Longitude);
        return result;
    }

    private static BitmapSource LoadDocumentationSampleImage(string screenshotId) {
        BitmapSource image = LoadDocumentationSampleRenderedImage(screenshotId).Image;
        image.Freeze();
        return image;
    }

    private static NINA.Image.Interfaces.IRenderedImage LoadDocumentationSampleRenderedImage(string screenshotId) {
        if (documentationSampleRenderedImage is not null) {
            return documentationSampleRenderedImage;
        }
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "TestImage_Jelly.xisf");
        if (!File.Exists(path)) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not find the bundled deterministic sample image: {path}");
        }
        NINA.Image.Interfaces.IImageDataFactory factory =
            (NINA.Image.Interfaces.IImageDataFactory)CreateWithInertServices(typeof(NINA.Image.ImageData.ImageDataFactory));
        Task<NINA.Image.Interfaces.IImageData> loadTask = NINA.Image.ImageData.BaseImageData.FromFile(
            path,
            16,
            false,
            (NINA.Image.Interfaces.IRawConverter)InertValue.Create(typeof(NINA.Image.Interfaces.IRawConverter))!,
            factory);
        WaitWithDispatcher(loadTask, screenshotId);
        documentationSampleRenderedImage = loadTask.GetAwaiter().GetResult().RenderImage();
        documentationSampleRenderedImage.Image.Freeze();
        return documentationSampleRenderedImage;
    }

    private static IProfileService GetProfileService() =>
        (IProfileService)(Application.Current?.Resources["ProfileService"]
            ?? throw new CatalogException("NINA's isolated profile service was not initialized."));

    private static NINA.Profile.Profile GetActiveProfile() =>
        (NINA.Profile.Profile)(Application.Current?.Resources["ActiveProfile"]
            ?? throw new CatalogException("NINA's isolated profile was not initialized."));

    private static FrameworkElement InstantiateProductionView(string typeName, string screenshotId) {
        Type? type = ResolveProductionType(typeName);
        if (type is null || !typeof(FrameworkElement).IsAssignableFrom(type)) {
            throw new CatalogException($"Screenshot '{screenshotId}' refers to unavailable production view '{typeName}'.");
        }
        try {
            return (FrameworkElement)(Activator.CreateInstance(type)
                ?? throw new InvalidOperationException("The production view constructor returned null."));
        } catch (Exception ex) {
            throw new CatalogException($"Screenshot '{screenshotId}' could not construct '{typeName}': {ex.GetBaseException().Message}");
        }
    }

    private static Type? ResolveProductionType(string typeName) {
        return typeof(NINA.App).Assembly.GetType(typeName, throwOnError: false)
            ?? typeof(NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM).Assembly.GetType(typeName, throwOnError: false)
            ?? typeof(NINA.Equipment.Equipment.MyRotator.ManualRotator).Assembly.GetType(typeName, throwOnError: false);
    }

    private static void PopulateProductionViewModel(object viewModel) {
        switch (viewModel) {
            case NINA.Imaging.ViewModel.Imaging.AutoFocusToolVM autoFocusTool:
                AutoFocusVM autoFocus = CreateAutoFocusViewModel(false, false);
                SetNonPublicProperty(autoFocusTool, nameof(autoFocusTool.AutoFocusVM), autoFocus);
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Guider.GuiderVM guider:
                NINA.Equipment.Equipment.MyGuider.PHD2.PHD2Guider phd2 = new(
                    GetProfileService(),
                    (NINA.Core.Utility.WindowService.IWindowServiceFactory)InertValue.Create(
                        typeof(NINA.Core.Utility.WindowService.IWindowServiceFactory))!);
                phd2.AppState = new NINA.Equipment.Equipment.MyGuider.PHD2.PhdEvents.PhdEventAppState {
                    State = "Guiding"
                };
                SetNonPublicProperty(phd2, nameof(phd2.Connected), true);
                phd2.PixelScale = 1.42;
                NINA.Equipment.Equipment.MyGuider.PHD2.Phd2Profile documentationProfile = new() {
                    Id = 1,
                    Name = "Documentation Equipment"
                };
                phd2.AvailableProfiles.Add(documentationProfile);
                phd2.SelectedProfile = documentationProfile;
                SetNonPublicProperty(guider, nameof(guider.Guider), phd2);
                guider.GuiderInfo.Connected = true;
                guider.SettingsVisible = true;
                guider.GuideStepsHistory.PixelScale = 1.42;
                for (int index = 0; index < 120; index++) {
                    guider.GuideStepsHistory.AddGuideStep(new DocumentationGuideStep(
                        index,
                        0.28 * Math.Sin(index * 0.37) + 0.13 * Math.Sin(index * 1.17),
                        0.24 * Math.Cos(index * 0.31) - 0.11 * Math.Sin(index * 0.83),
                        index is 7 or 24 or 51 or 83 or 108 ? 0.7 + (index % 3) * 0.15 : 0,
                        index is 13 or 36 or 68 or 91 or 116 ? -0.6 - (index % 4) * 0.12 : 0));
                }
                break;
            case NINA.ViewModel.FocusTargetsVM focusTargets:
                NINA.Astrometry.FocusTarget deneb = new("Deneb") {
                    Magnitude = 1.25,
                    Coordinates = new NINA.Astrometry.Coordinates(
                        NINA.Astrometry.Angle.ByHours(20.6905),
                        NINA.Astrometry.Angle.ByDegree(45.2803),
                        NINA.Astrometry.Epoch.J2000),
                    Altitude = 36.86,
                    Azimuth = 332.63
                };
                focusTargets.FocusTargets = new System.Collections.ObjectModel.ObservableCollection<NINA.Astrometry.FocusTarget>([deneb]);
                focusTargets.SelectedFocusTarget = deneb;
                focusTargets.TelescopeConnected = true;
                break;
            case NINA.ViewModel.ImageStatisticsVM statistics:
                statistics.Statistics = CreateDocumentationImageStatistics();
                break;
            case NINA.WPF.Base.Interfaces.ViewModel.IThumbnailVM thumbnails:
                PopulateThumbnails(thumbnails);
                break;
            case NINA.Equipment.Interfaces.ViewModel.IImageControlVM imageControl:
                imageControl.RenderedImage = LoadDocumentationSampleRenderedImage("image-control").ReRender();
                imageControl.Image = imageControl.RenderedImage.Image;
                imageControl.AutoStretch = true;
                break;
            case NINA.WPF.Base.Interfaces.ViewModel.IAnchorableSnapshotVM snapshot:
                NINA.Profile.Profile activeProfile = GetActiveProfile();
                NINA.Core.Model.Equipment.FilterInfo filter = activeProfile.FilterWheelSettings.FilterWheelFilters.First();
                NINA.Equipment.Equipment.MyCamera.CameraInfo snapshotCamera = CreateDocumentationCameraInfo();
                snapshotCamera.CanGetGain = true;
                snapshotCamera.ExposureMin = 0.001;
                snapshotCamera.ExposureMax = 3600;
                snapshotCamera.BinningModes = new NINA.Core.Utility.AsyncObservableCollection<NINA.Core.Model.Equipment.BinningMode>([
                    new NINA.Core.Model.Equipment.BinningMode(1, 1),
                    new NINA.Core.Model.Equipment.BinningMode(2, 2)
                ]);
                snapshot.CameraInfo = snapshotCamera;
                snapshot.FilterWheelInfo = new NINA.Equipment.Equipment.MyFilterWheel.FilterWheelInfo {
                    Connected = true,
                    Name = "N.I.N.A. Filter Wheel Simulator",
                    SelectedFilter = filter
                };
                snapshot.SnapExposureDuration = 2.5;
                snapshot.SnapFilter = filter;
                snapshot.SnapBin = snapshotCamera.BinningModes.First();
                snapshot.SnapGain = 50;
                snapshot.SnapSave = true;
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Camera.CameraVM camera:
                SetNonPublicProperty(camera, nameof(camera.Cam),
                    InertValue.Create(typeof(NINA.Equipment.Interfaces.ICamera))!);
                camera.CameraInfo = CreateDocumentationCameraInfo();
                camera.TargetTemp = -10;
                camera.CoolingDuration = 10;
                camera.WarmingDuration = 10;
                DateTime coolingStart = new(2026, 8, 31, 21, 45, 0, DateTimeKind.Local);
                for (int index = 0; index < 31; index++) {
                    double progress = index / 30d;
                    double temperature = 12 - 22 * (1 - Math.Exp(-4 * progress)) / (1 - Math.Exp(-4));
                    double power = Math.Min(100, 22 + 115 * progress);
                    camera.CoolerHistory.AddLast(new NINA.WPF.Base.ViewModel.Equipment.Camera.CameraCoolingStep(
                        OxyPlot.Axes.DateTimeAxis.ToDouble(coolingStart.AddSeconds(index * 10)),
                        temperature,
                        power));
                }
                camera.CoolerHistoryMin = -12;
                camera.CoolerHistoryMax = 15;
                camera.CoolerHistoryChangeId++;
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Dome.DomeVM dome:
                SetNonPublicProperty(dome, nameof(dome.Dome),
                    InertValue.Create(typeof(NINA.Equipment.Interfaces.IDome))!);
                dome.DomeInfo = new NINA.Equipment.Equipment.MyDome.DomeInfo {
                    Connected = true,
                    Name = "N.I.N.A. Dome Simulator",
                    Description = "Deterministic roll-off roof simulator",
                    DriverInfo = "NINA documentation fixture",
                    DriverVersion = "3.0",
                    Azimuth = 334,
                    Altitude = 90,
                    ShutterStatus = NINA.Equipment.Interfaces.ShutterState.ShutterClosed,
                    CanSetShutter = true,
                    CanSetPark = true,
                    CanSetAzimuth = true,
                    CanSyncAzimuth = true,
                    CanPark = true,
                    CanFindHome = true,
                    AtPark = false,
                    AtHome = false,
                    ApplicationFollowing = true
                };
                dome.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyTelescope.TelescopeInfo {
                    Connected = true,
                    Name = "N.I.N.A. Simulator Telescope",
                    Altitude = 38.92,
                    Azimuth = 180
                });
                dome.FollowEnabled = true;
                dome.TargetAzimuthDegrees = 334;
                dome.RotateDegrees = 10;
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Rotator.RotatorVM rotator:
                SetNonPublicProperty(rotator, nameof(rotator.Rotator),
                    InertValue.Create(typeof(NINA.Equipment.Interfaces.IRotator))!);
                rotator.RotatorInfo = new NINA.Equipment.Equipment.MyRotator.RotatorInfo {
                    Connected = true,
                    Name = "N.I.N.A. Rotator Simulator",
                    Description = "Deterministic documentation rotator",
                    DriverInfo = "NINA documentation fixture",
                    DriverVersion = "3.0",
                    CanReverse = true,
                    Reverse = false,
                    Position = 279.45f,
                    MechanicalPosition = 279.45f,
                    StepSize = 0.01f,
                    Synced = true,
                    IsMoving = false
                };
                rotator.TargetPosition = 279.45f;
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Telescope.TelescopeVM telescope:
                SetNonPublicProperty(telescope, nameof(telescope.Telescope),
                    InertValue.Create(typeof(NINA.Equipment.Interfaces.ITelescope))!);
                NINA.Equipment.Equipment.MyTelescope.TelescopeInfo telescopeInfo = telescope.TelescopeInfo;
                telescopeInfo.Connected = true;
                telescopeInfo.Name = "N.I.N.A. Simulator Telescope";
                telescopeInfo.Description = "Deterministic documentation simulator";
                telescopeInfo.DriverInfo = "NINA documentation fixture";
                telescopeInfo.DriverVersion = "3.0";
                telescopeInfo.Altitude = 38.92;
                telescopeInfo.Azimuth = 180;
                telescopeInfo.RightAscension = 2.065;
                telescopeInfo.Declination = -1.6386;
                telescopeInfo.TrackingEnabled = true;
                telescopeInfo.SiteLatitude = 52.52;
                telescopeInfo.SiteLongitude = 13.405;
                break;
            case NINA.WPF.Base.ViewModel.Equipment.WeatherData.WeatherDataVM weather:
                SetNonPublicProperty(weather, nameof(weather.WeatherData),
                    InertValue.Create(typeof(NINA.Equipment.Interfaces.IWeatherData))!);
                weather.WeatherDataInfo = new NINA.Equipment.Equipment.MyWeatherData.WeatherDataInfo {
                    Connected = true,
                    Name = "N.I.N.A. Weather Simulator",
                    Description = "Deterministic observing conditions",
                    DriverInfo = "NINA documentation fixture",
                    DriverVersion = "3.0",
                    Temperature = 5.57,
                    Humidity = 50.25,
                    Pressure = 1021.1,
                    DewPoint = -3.97,
                    CloudCover = 0.25,
                    RainRate = 0,
                    WindDirection = 178.37,
                    WindGust = 2.5,
                    WindSpeed = 0.39,
                    StarFWHM = 0.88,
                    SkyQuality = 18.1,
                    SkyBrightness = 85.5,
                    SkyTemperature = -27.85
                };
                break;
            case NINA.WPF.Base.ViewModel.Equipment.Switch.SwitchVM switches:
                switches.SwitchHub = (NINA.Equipment.Interfaces.ISwitchHub)InertValue.Create(
                    typeof(NINA.Equipment.Interfaces.ISwitchHub))!;
                switches.ReadonlySwitches = new List<NINA.Equipment.Interfaces.ISwitch> {
                    new DocumentationReadOnlySwitch(0, "Power 1", "Generic power switch", 1),
                    new DocumentationReadOnlySwitch(1, "Power 2", "Generic power switch", 1),
                    new DocumentationReadOnlySwitch(2, "Cloud cover", "Cloud monitor percentage", 0.25),
                    new DocumentationReadOnlySwitch(3, "Temperature", "Ambient temperature in degrees C", 5.57),
                    new DocumentationReadOnlySwitch(4, "Humidity", "Relative humidity percentage", 50.25),
                    new DocumentationReadOnlySwitch(5, "Raining", "Rain monitor, 0 means dry", 0),
                    new DocumentationReadOnlySwitch(6, "Sky quality", "Sky quality in magnitudes per square arcsecond", 18.1),
                    new DocumentationReadOnlySwitch(7, "Dew point", "Calculated dew point in degrees C", -3.97)
                };
                switches.WritableSwitches = new List<NINA.Equipment.Interfaces.IWritableSwitch> {
                    new DocumentationWritableSwitch(8, "Light Box", "Light box brightness", 0, 255, 1, 64),
                    new DocumentationWritableSwitch(9, "Flat Panel", "Flat panel brightness", 0, 255, 1, 128),
                    new DocumentationWritableSwitch(10, "Scope Cover", "Scope cover, on means closed", 0, 1, 1, 1),
                    new DocumentationWritableSwitch(11, "Observatory Power", "Main observatory power", 0, 1, 1, 1),
                    new DocumentationWritableSwitch(12, "Dew Heater", "Dew heater power", 0, 100, 1, 35),
                    new DocumentationWritableSwitch(13, "Camera Power", "Camera power relay", 0, 1, 1, 1),
                    new DocumentationWritableSwitch(14, "Mount Power", "Mount power relay", 0, 1, 1, 1),
                    new DocumentationWritableSwitch(15, "Roof Light", "Observatory roof light", 0, 1, 1, 0)
                };
                switches.SwitchInfo = new NINA.Equipment.Equipment.MySwitch.SwitchInfo {
                    Connected = true,
                    Name = "N.I.N.A. Switch Simulator",
                    Description = "Deterministic documentation switches",
                    DriverInfo = "NINA documentation fixture",
                    DriverVersion = "3.0",
                    WritableSwitches = new System.Collections.ObjectModel.ReadOnlyCollection<NINA.Equipment.Interfaces.IWritableSwitch>(switches.WritableSwitches),
                    ReadonlySwitches = new System.Collections.ObjectModel.ReadOnlyCollection<NINA.Equipment.Interfaces.ISwitch>(switches.ReadonlySwitches)
                };
                break;
            case NINA.ViewModel.Interfaces.IFlatWizardVM flatWizard:
                flatWizard.UpdateDeviceInfo(CreateDocumentationCameraInfo());
                flatWizard.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyFilterWheel.FilterWheelInfo {
                    Connected = true,
                    Name = "N.I.N.A. Filter Wheel Simulator",
                    SelectedFilter = GetActiveProfile().FilterWheelSettings.FilterWheelFilters.First()
                });
                flatWizard.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyTelescope.TelescopeInfo {
                    Connected = true,
                    Name = "N.I.N.A. Simulator Telescope",
                    Altitude = 89,
                    Azimuth = 90
                });
                flatWizard.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyFlatDevice.FlatDeviceInfo {
                    Connected = true,
                    Name = "N.I.N.A. Flat Panel Simulator",
                    SupportsOpenClose = true,
                    SupportsOnOff = true,
                    MinBrightness = 0,
                    MaxBrightness = 255,
                    Brightness = 80
                });
                flatWizard.FlatCount = 6;
                flatWizard.CalculatedExposureTime = 1.25;
                flatWizard.CalculatedHistogramMean = 26214;
                SetProperty(viewModel, "TargetName", "FlatWizard");
                NINA.Sequencer.SequenceItem.FlatDevice.AutoExposureFlat activeInstruction =
                    new(
                        GetProfileService(),
                        (NINA.Equipment.Interfaces.Mediator.ICameraMediator)InertValue.Create(typeof(NINA.Equipment.Interfaces.Mediator.ICameraMediator))!,
                        (NINA.Equipment.Interfaces.Mediator.IImagingMediator)InertValue.Create(typeof(NINA.Equipment.Interfaces.Mediator.IImagingMediator))!,
                        (NINA.WPF.Base.Interfaces.Mediator.IImageSaveMediator)InertValue.Create(typeof(NINA.WPF.Base.Interfaces.Mediator.IImageSaveMediator))!,
                        (NINA.WPF.Base.Interfaces.ViewModel.IImageHistoryVM)InertValue.Create(typeof(NINA.WPF.Base.Interfaces.ViewModel.IImageHistoryVM))!,
                        (NINA.Equipment.Interfaces.Mediator.IFilterWheelMediator)InertValue.Create(typeof(NINA.Equipment.Interfaces.Mediator.IFilterWheelMediator))!,
                        (NINA.Equipment.Interfaces.Mediator.IFlatDeviceMediator)InertValue.Create(typeof(NINA.Equipment.Interfaces.Mediator.IFlatDeviceMediator))!);
                activeInstruction.GetExposureItem().ExposureTime = 1.25;
                activeInstruction.DeterminedHistogramADU = 26214;
                SetProperty(viewModel, "ActiveFlatInstruction", activeInstruction);
                break;
        }
    }

    private static NINA.Equipment.Equipment.MyCamera.CameraInfo CreateDocumentationCameraInfo() => new() {
        Connected = true,
        Name = "N.I.N.A. Simulator Camera",
        Description = "Deterministic monochrome camera",
        DriverInfo = "NINA documentation fixture",
        DriverVersion = "3.0",
        SensorType = NINA.Core.Enum.SensorType.Monochrome,
        XSize = 3000,
        YSize = 2000,
        PixelSize = 3.76,
        BitDepth = 16,
        CanSetTemperature = true,
        CanShowLiveView = true,
        CanSetGain = true,
        GainMin = 0,
        GainMax = 300,
        Gain = 50,
        CanSetOffset = true,
        OffsetMin = 0,
        OffsetMax = 100,
        Offset = 25,
        BinX = 1,
        BinY = 1,
        Temperature = -10,
        CoolerOn = true,
        CoolerPower = 18.5
    };

    private static NINA.Image.ImageData.AllImageStatistics CreateDocumentationImageStatistics() {
        NINA.Image.ImageData.ImageProperties properties = new(4656, 3520, 16, false, 50, 25);
        DocumentationImageStatistics imageStatistics = new();
        DocumentationStarDetectionAnalysis starDetection = new() {
            HFR = 3.12,
            FWHM = 6.85,
            Eccentricity = 0.46,
            HFRStDev = 0.31,
            DetectedStars = 129
        };
        ConstructorInfo constructor = typeof(NINA.Image.ImageData.AllImageStatistics)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        return (NINA.Image.ImageData.AllImageStatistics)constructor.Invoke([
            properties,
            Task.FromResult<NINA.Image.Interfaces.IImageStatistics>(imageStatistics),
            starDetection
        ]);
    }

    private static void PopulateThumbnails(NINA.WPF.Base.Interfaces.ViewModel.IThumbnailVM viewModel) {
        BitmapSource sample = LoadDocumentationSampleImage("thumbnail-history");
        NINA.Image.Interfaces.IImageDataFactory factory =
            (NINA.Image.Interfaces.IImageDataFactory)CreateWithInertServices(typeof(NINA.Image.ImageData.ImageDataFactory));
        DateTime firstExposure = new(2026, 8, 31, 21, 59, 12, DateTimeKind.Local);
        for (int index = 0; index < 15; index++) {
            DocumentationImageStatistics statistics = new(1204 + 18 * Math.Sin(index * 0.55));
            DocumentationStarDetectionAnalysis stars = new() {
                HFR = 3.55 + 0.18 * Math.Sin(index * 0.73),
                FWHM = 6.8,
                Eccentricity = 0.46,
                HFRStDev = 0.3,
                DetectedStars = 120 + index
            };
            NINA.Image.Thumbnail thumbnail = new(factory) {
                ThumbnailImage = sample,
                ImagePath = new Uri($"C:\\NINA\\Images\\M42_{index + 1:000}.xisf"),
                FileType = FileTypeEnum.XISF,
                Duration = 180,
                ImageStatistics = statistics,
                StarDetectionAnalysis = stars,
                Filter = (index % 4) switch { 0 => "L", 1 => "R", 2 => "G", _ => "B" },
                IsBayered = false,
                Date = firstExposure.AddMinutes(index * 3)
            };
            viewModel.Thumbnails.Add(thumbnail);
        }
        viewModel.SelectedThumbnail = null;
    }

    public FrameworkElement CreateSettingsGroup(ScreenshotAsset asset) {
        if (asset.State?.StartsWith("meridian-flip-settings", StringComparison.OrdinalIgnoreCase) != true) {
            throw new CatalogException($"Screenshot '{asset.Id}' refers to unknown production settings group '{asset.State}'.");
        }
        IProfileService profileService = (IProfileService)(Application.Current?.Resources["ProfileService"]
            ?? throw new CatalogException("NINA's isolated profile service was not initialized."));
        NINA.View.Options.ImagingView productionView = new() { DataContext = profileService };
        foreach (NINA.WPF.Base.View.BrowserPopupButton helpButton in
            EnumerateLogicalDescendants<NINA.WPF.Base.View.BrowserPopupButton>(productionView)) {
            ClearBrowserPopupSizingBindings(helpButton);
        }

        UniformGrid layout = productionView.Content as UniformGrid
            ?? throw new CatalogException("NINA's production ImagingView no longer has its expected root layout.");
        ScrollViewer rightColumn = layout.Children.OfType<ScrollViewer>().SingleOrDefault()
            ?? throw new CatalogException("NINA's production ImagingView no longer has its expected settings column.");
        StackPanel settings = rightColumn.Content as StackPanel
            ?? throw new CatalogException("NINA's production ImagingView no longer has its expected settings stack.");
        GroupBox group = settings.Children.OfType<GroupBox>().FirstOrDefault()
            ?? throw new CatalogException("NINA's production ImagingView no longer exposes the meridian flip settings group.");
        settings.Children.Remove(group);
        if (group.Header is Grid header) {
            foreach (NINA.WPF.Base.View.BrowserPopupButton helpButton in header.Children
                .OfType<NINA.WPF.Base.View.BrowserPopupButton>()
                .ToList()) {
                header.Children.Remove(helpButton);
            }
        }
        group.DataContext = profileService.ActiveProfile.MeridianFlipSettings;
        return group;
    }

    private static void ClearBrowserPopupSizingBindings(NINA.WPF.Base.View.BrowserPopupButton helpButton) {
        if (helpButton.FindName("PopupControl") is System.Windows.Controls.Primitives.Popup popup
            && popup.Child is Border popupBorder) {
            System.Windows.Data.BindingOperations.ClearBinding(popupBorder, FrameworkElement.WidthProperty);
            System.Windows.Data.BindingOperations.ClearBinding(popupBorder, FrameworkElement.HeightProperty);
        }
    }

    private static IEnumerable<T> EnumerateLogicalDescendants<T>(DependencyObject root) where T : DependencyObject {
        foreach (object child in LogicalTreeHelper.GetChildren(root)) {
            if (child is T result) {
                yield return result;
            }
            if (child is DependencyObject dependencyObject) {
                foreach (T descendant in EnumerateLogicalDescendants<T>(dependencyObject)) {
                    yield return descendant;
                }
            }
        }
    }

    public FrameworkElement CreateAutoFocusChart(ScreenshotAsset asset) {
        if (asset.Output.Contains("hfr", StringComparison.OrdinalIgnoreCase)) {
            return CreateImageHistoryChart();
        }

        bool contrastCurve = asset.Output.Contains("autofocuscurve2", StringComparison.OrdinalIgnoreCase);
        bool backlashCurve = asset.Output.Contains("backlash", StringComparison.OrdinalIgnoreCase);
        AutoFocusVM viewModel = CreateAutoFocusViewModel(contrastCurve, backlashCurve);
        return new AutoFocusChart { DataContext = viewModel };
    }

    private static AutoFocusVM CreateAutoFocusViewModel(bool contrastCurve, bool backlashCurve) {
        AutoFocusVM viewModel = (AutoFocusVM)CreateWithInertServices(typeof(AutoFocusVM));
        viewModel.AutoFocusChartMethod = contrastCurve ? AFMethodEnum.CONTRASTDETECTION : AFMethodEnum.STARHFR;
        viewModel.AutoFocusChartCurveFitting = AFCurveFittingEnum.TRENDHYPERBOLIC;

        IReadOnlyList<DataPoint> points = contrastCurve
            ? CreateContrastCurve()
            : CreateHfrCurve(backlashCurve);
        foreach (DataPoint point in points) {
            viewModel.FocusPoints.Add(new ScatterErrorPoint(point.X, point.Y, 0, 0.18));
            viewModel.PlotFocusPoints.Add(point);
        }
        viewModel.SetCurveFittings(viewModel.AutoFocusChartMethod.ToString(), viewModel.AutoFocusChartCurveFitting.ToString());
        DataPoint focusPoint = contrastCurve
            ? points.MaxBy(point => point.Y)
            : points.MinBy(point => point.Y);
        viewModel.FinalFocusPoint = focusPoint;
        viewModel.LastAutoFocusPoint = new ReportAutoFocusPoint {
            Focuspoint = focusPoint,
            Timestamp = new DateTime(2026, 8, 31, 22, 15, 0, DateTimeKind.Local),
            Temperature = 11.5,
            Filter = "L"
        };
        viewModel.AutoFocusDuration = TimeSpan.FromMinutes(2.4);
        return viewModel;
    }

    private static IReadOnlyList<DataPoint> CreateHfrCurve(bool backlashCurve) {
        List<DataPoint> points = [];
        for (int index = -6; index <= 6; index++) {
            double hfr = 2.2 + 0.42 * index * index + 0.08 * Math.Abs(index);
            if (backlashCurve && index >= 3) {
                hfr = 5.8 + 0.12 * (index - 3);
            }
            points.Add(new DataPoint(9000 + index * 50, hfr));
        }
        return points;
    }

    private static IReadOnlyList<DataPoint> CreateContrastCurve() {
        List<DataPoint> points = [];
        for (int index = -8; index <= 8; index++) {
            double position = 3200 + index * 18;
            double contrast = 10 + 13 * Math.Exp(-Math.Pow(position - 3200, 2) / (2 * Math.Pow(34, 2)));
            points.Add(new DataPoint(position, contrast));
        }
        return points;
    }

    private static FrameworkElement CreateImageHistoryChart() {
        ImageHistoryVM viewModel = (ImageHistoryVM)CreateWithInertServices(typeof(ImageHistoryVM));
        viewModel.ImageHistoryLeftSelected = ImageHistoryEnum.HFR;
        viewModel.ImageHistoryRightSelected = ImageHistoryEnum.Stars;
        int[] autoFocusIndexes = [16, 32, 48, 64];
        for (int index = 1; index <= 72; index++) {
            ImageHistoryPoint point = new(index, "LIGHT") { Index = index };
            SetNonPublicProperty(point, nameof(ImageHistoryPoint.HFR), 2.55 + 0.12 * Math.Sin(index * 0.43) + (index == 68 ? 0.65 : 0));
            SetNonPublicProperty(point, nameof(ImageHistoryPoint.Stars), 940 + (int)(260 * Math.Sin(index * 0.19)));
            SetNonPublicProperty(point, nameof(ImageHistoryPoint.Filter), "L");
            viewModel.ObservableImageHistory.Add(point);
            if (autoFocusIndexes.Contains(index)) {
                SetNonPublicProperty(point, nameof(ImageHistoryPoint.AutoFocusPoint), new NINA.Core.Model.AutoFocusPoint {
                    OldPosition = 10000 + index * 4,
                    NewPosition = 10012 + index * 4,
                    Temperature = 11.5 - index * 0.02,
                    Time = new DateTime(2026, 8, 31, 21, 0, 0).AddMinutes(index * 3),
                    Filter = "L"
                });
                viewModel.AutoFocusPoints.Add(point);
            }
        }
        viewModel.FilterImageHistoryList();
        return new AnchorableImageHistoryView { DataContext = viewModel };
    }

    private static void SetNonPublicProperty(object target, string name, object value) {
        PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new CatalogException($"Production type '{target.GetType().FullName}' has no property '{name}'.");
        property.SetValue(target, value);
    }

    public FrameworkElement CreateSequencerEntity(ScreenshotAsset asset) {
        Type entityType = FindSequenceEntityType(asset);
        ISequenceEntity entity = (ISequenceEntity)CreateWithInertServices(entityType);
        ApplySequenceMetadata(entity);
        ApplyDeterministicValues(entity);

        DataTemplate? productionTemplate = Application.Current?.TryFindResource(new DataTemplateKey(entityType)) as DataTemplate;
        if (productionTemplate is not null) {
            return WrapProductionElement(new ContentControl {
                Content = entity,
                ContentTemplate = productionTemplate,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            }, entity);
        }
        if (entity is NINA.Sequencer.SequenceItem.ISequenceItem or ISequenceTrigger) {
            return WrapProductionElement(new SequenceBlockView { DataContext = entity }, entity);
        }
        throw new CatalogException($"Screenshot '{asset.Id}' has no production DataTemplate for '{entityType.FullName}'.");
    }

    public FrameworkElement CreateAdvancedSequencer(ScreenshotAsset asset) {
        ISequence2VM viewModel = CreateAdvancedSequenceViewModel(asset.Id);
        SequencerFixtureState.Apply(viewModel, asset);
        return new AdvancedSequencerView { DataContext = viewModel };
    }

    private ISequence2VM CreateAdvancedSequenceViewModel(string screenshotId) {
        IProfileService profileService = GetProfileService();
        ISymbolBroker productionSymbolBroker = GetSymbolBroker();
        SequencerFactory factory = CreateSequencerFactory(profileService, productionSymbolBroker);

        Type viewModelType = typeof(NINA.App).Assembly.GetType("NINA.ViewModel.Sequencer.Sequence2VM", throwOnError: true)!;
        ConstructorInfo constructor = viewModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single();
        object?[] arguments = constructor.GetParameters().Select(parameter => {
            if (parameter.ParameterType == typeof(IProfileService)) {
                return (object?)profileService;
            }
            if (parameter.ParameterType == typeof(ISequencerFactory)) {
                return factory;
            }
            if (parameter.ParameterType == typeof(ISymbolBroker)) {
                return productionSymbolBroker;
            }
            return InertValue.Create(parameter.ParameterType);
        }).ToArray();
        ISequence2VM viewModel = (ISequence2VM)constructor.Invoke(arguments);
        WaitWithDispatcher(viewModel.Initialize(), screenshotId);
        AddDocumentationTemplates(viewModel, profileService);
        AddDocumentationTargets(viewModel, profileService);
        return viewModel;
    }

    private static SequencerFactory CreateSequencerFactory(IProfileService profileService, ISymbolBroker productionSymbolBroker) {
        List<ISequenceItem> items = CreateExported<ISequenceItem>(productionSymbolBroker);
        List<ISequenceCondition> conditions = CreateExported<ISequenceCondition>(productionSymbolBroker);
        List<ISequenceTrigger> triggers = CreateExported<ISequenceTrigger>(productionSymbolBroker);
        List<ISequenceContainer> containers = CreateExported<ISequenceContainer>(productionSymbolBroker);
        return new SequencerFactory(
            profileService,
            items,
            conditions,
            triggers,
            containers,
            [],
            []);
    }

    private static void AddDocumentationTemplates(ISequence2VM viewModel, IProfileService profileService) {
        TemplateController templateController = (TemplateController)(viewModel.GetType()
            .GetProperty("TemplateController", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(viewModel)
            ?? throw new CatalogException("NINA's production sequencer view model has no template controller."));
        if (templateController.Templates.Count > 0) {
            return;
        }

        SequentialContainer startup = viewModel.SequencerFactory.GetContainer<SequentialContainer>();
        startup.Name = "Basic Sequence Startup";
        startup.Add(viewModel.SequencerFactory.GetItem<CoolCamera>());
        startup.Add(viewModel.SequencerFactory.GetItem<UnparkScope>());

        DeepSkyObjectContainer target = viewModel.SequencerFactory.GetContainer<DeepSkyObjectContainer>();
        target.Name = "Basic Sequence Target";
        target.Target.TargetName = "Deep Sky Target";
        target.Add(viewModel.SequencerFactory.GetItem<TakeExposure>());

        SequentialContainer end = viewModel.SequencerFactory.GetContainer<SequentialContainer>();
        end.Name = "Basic Sequence End";
        end.Add(viewModel.SequencerFactory.GetItem<ParkScope>());
        end.Add(viewModel.SequencerFactory.GetItem<WarmCamera>());

        foreach (ISequenceContainer container in new ISequenceContainer[] { end, startup, target }) {
            templateController.Templates.Add(new TemplatedSequenceContainer(
                profileService,
                TemplateController.DefaultTemplatesGroup,
                container));
        }
        templateController.TemplatesView.Refresh();
        templateController.TemplatesMenuView.Refresh();
    }

    private static void AddDocumentationTargets(ISequence2VM viewModel, IProfileService profileService) {
        TargetController targetController = (TargetController)(viewModel.GetType()
            .GetProperty("TargetController", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(viewModel)
            ?? throw new CatalogException("NINA's production sequencer view model has no target controller."));
        if (targetController.Targets.Count > 0) {
            return;
        }

        targetController.Targets.Add(new TargetSequenceContainer(profileService,
            NewDocumentationTarget(viewModel, "Andromeda Galaxy", 0.712, 41.269)));
        targetController.Targets.Add(new TargetSequenceContainer(profileService,
            NewDocumentationTarget(viewModel, "Orion Nebula", 5.588, -5.391)));
        targetController.Targets.Add(new TargetSequenceContainer(profileService,
            NewDocumentationTarget(viewModel, "Triangulum Galaxy", 1.564, 30.660)));
        targetController.TargetsView.SortDescriptions.Clear();
        targetController.TargetsView.SortDescriptions.Add(new SortDescription(
            nameof(TargetSequenceContainer.Name),
            ListSortDirection.Ascending));
        targetController.TargetsView.Refresh();
        targetController.TargetsMenuView.Refresh();
    }

    private static DeepSkyObjectContainer NewDocumentationTarget(
            ISequence2VM viewModel,
            string name,
            double rightAscensionHours,
            double declinationDegrees) {
        DeepSkyObjectContainer target = viewModel.SequencerFactory.GetContainer<DeepSkyObjectContainer>();
        target.Name = name;
        target.Target.TargetName = name;
        target.Target.InputCoordinates.Coordinates = new NINA.Astrometry.Coordinates(
            NINA.Astrometry.Angle.ByHours(rightAscensionHours),
            NINA.Astrometry.Angle.ByDegree(declinationDegrees),
            NINA.Astrometry.Epoch.J2000);
        target.Target.PositionAngle = 0;
        return target;
    }

    private static void WaitWithDispatcher(Task task, string screenshotId) {
        System.Windows.Threading.Dispatcher dispatcher = Application.Current?.Dispatcher
            ?? throw new CatalogException("The WPF dispatcher is unavailable.");
        System.Windows.Threading.DispatcherFrame frame = new();
        bool timedOut = false;
        System.Windows.Threading.DispatcherTimer timeout = new(System.Windows.Threading.DispatcherPriority.Send, dispatcher) {
            Interval = TimeSpan.FromSeconds(10)
        };
        timeout.Tick += (_, _) => {
            timedOut = true;
            frame.Continue = false;
        };
        _ = task.ContinueWith(_ => dispatcher.BeginInvoke(new Action(() => frame.Continue = false)), TaskScheduler.Default);
        timeout.Start();
        System.Windows.Threading.Dispatcher.PushFrame(frame);
        timeout.Stop();
        if (timedOut) {
            throw new CatalogException($"Screenshot '{screenshotId}' timed out while initializing NINA's production sequencer view model.");
        }
        task.GetAwaiter().GetResult();
    }

    private static List<T> CreateExported<T>(ISymbolBroker symbolBroker) where T : class, ISequenceEntity {
        Type contract = typeof(T);
        List<T> entities = [];
        foreach (Type type in typeof(ISequenceEntity).Assembly.GetTypes()
            .Where(candidate => !candidate.IsAbstract && contract.IsAssignableFrom(candidate))
            .Where(candidate => candidate.GetCustomAttributes<ExportAttribute>().Any(attribute => attribute.ContractType == contract))) {
            T entity = (T)CreateWithInertServices(type);
            ApplyExportMetadata(entity, type, symbolBroker);
            ApplyDeterministicValues(entity);
            entities.Add(entity);
        }
        return entities;
    }

    private static void ApplyExportMetadata(ISequenceEntity entity, Type type, ISymbolBroker symbolBroker) {
        Dictionary<string, object?> metadata = type.GetCustomAttributes<ExportMetadataAttribute>()
            .GroupBy(attribute => attribute.Name)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase);
        entity.Name = ResolveMetadataText(metadata.GetValueOrDefault("Name"), SplitWords(type.Name));
        entity.Description = ResolveMetadataText(metadata.GetValueOrDefault("Description"), string.Empty);
        entity.Category = ResolveMetadataText(metadata.GetValueOrDefault("Category"), "Sequencer");
        if (metadata.GetValueOrDefault("Icon") is string iconKey
            && Application.Current?.TryFindResource(iconKey) is GeometryGroup icon) {
            entity.Icon = icon;
        }
        entity.SymbolBroker = symbolBroker;
    }

    private static string ResolveMetadataText(object? value, string fallback) {
        if (value is not string text || string.IsNullOrWhiteSpace(text)) {
            return fallback;
        }
        string localized = Loc.Instance[text];
        if (string.IsNullOrWhiteSpace(localized)) {
            return fallback;
        }
        if (localized.StartsWith("MISSING LABEL", StringComparison.OrdinalIgnoreCase)) {
            return text.StartsWith("Lbl", StringComparison.OrdinalIgnoreCase) ? fallback : text;
        }
        return localized;
    }

    private static FrameworkElement WrapProductionElement(FrameworkElement element, ISequenceEntity entity) {
        element.DataContext = entity;
        element.HorizontalAlignment = HorizontalAlignment.Left;
        element.VerticalAlignment = VerticalAlignment.Top;
        return element;
    }

    private static Type FindSequenceEntityType(ScreenshotAsset asset) {
        Assembly assembly = typeof(ISequenceEntity).Assembly;
        string requested = asset.SourceIdentifier?.Split(':').LastOrDefault() ?? asset.DisplayName ?? string.Empty;
        Type[] candidates = assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ISequenceEntity).IsAssignableFrom(type))
            .ToArray();
        Type? result = assembly.GetType(asset.SourceIdentifier ?? string.Empty, throwOnError: false)
            ?? candidates.FirstOrDefault(type => Normalize(type.Name) == Normalize(requested))
            ?? candidates.FirstOrDefault(type => Normalize(type.Name) == Normalize(asset.DisplayName ?? string.Empty))
            ?? candidates.FirstOrDefault(type => GetLocalizedExportName(type) is string name
                && Normalize(name) == Normalize(asset.DisplayName ?? requested));
        return result ?? throw new CatalogException(
            $"Screenshot '{asset.Id}' could not find the production sequencer entity '{asset.SourceIdentifier ?? asset.DisplayName}'.");
    }

    private static string? GetLocalizedExportName(Type type) {
        ExportMetadataAttribute? metadata = type.GetCustomAttributes<ExportMetadataAttribute>()
            .LastOrDefault(attribute => string.Equals(attribute.Name, "Name", StringComparison.OrdinalIgnoreCase));
        return metadata?.Value is string name ? ResolveMetadataText(name, string.Empty) : null;
    }

    private static object CreateWithInertServices(Type type) {
        ConstructorInfo constructor = type.GetConstructors()
            .OrderByDescending(candidate => candidate.GetCustomAttribute<ImportingConstructorAttribute>() is not null)
            .ThenBy(candidate => candidate.GetParameters().Length)
            .FirstOrDefault() ?? throw new CatalogException($"Production type '{type.FullName}' has no public constructor.");
        try {
            object?[] arguments = constructor.GetParameters().Select(parameter => InertValue.Create(parameter.ParameterType)).ToArray();
            return constructor.Invoke(arguments);
        } catch (Exception ex) {
            throw new CatalogException($"Production type '{type.FullName}' could not be constructed with inert documentation services: {ex.GetBaseException().Message}");
        }
    }

    private void ApplySequenceMetadata(ISequenceEntity entity) {
        ApplyExportMetadata(entity, entity.GetType(), GetSymbolBroker());
    }

    private ISymbolBroker GetSymbolBroker() {
        if (symbolBroker is not null) {
            return symbolBroker;
        }

        SymbolBroker broker = (SymbolBroker)CreateWithInertServices(typeof(SymbolBroker));
        broker.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyCamera.CameraInfo {
            Connected = true,
            Temperature = -10,
            Gain = 50,
            Offset = 25
        });
        broker.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyDome.DomeInfo {
            Connected = true,
            Altitude = 0,
            Azimuth = 180,
            ShutterStatus = NINA.Equipment.Interfaces.ShutterState.ShutterClosed
        });
        broker.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyFocuser.FocuserInfo {
            Connected = true,
            Position = 25000,
            Temperature = 9.56
        });
        broker.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyFlatDevice.FlatDeviceInfo {
            Connected = true,
            Brightness = 0,
            CoverState = NINA.Equipment.Interfaces.CoverState.Closed,
            LightOn = false
        });
        broker.UpdateDeviceInfo(new NINA.Equipment.Equipment.MyTelescope.TelescopeInfo {
            Connected = true,
            Altitude = 45,
            Azimuth = 180,
            RightAscension = 0.712,
            Declination = 41.269,
            Coordinates = new NINA.Astrometry.Coordinates(
                NINA.Astrometry.Angle.ByHours(0.712),
                NINA.Astrometry.Angle.ByDegree(41.269),
                NINA.Astrometry.Epoch.J2000),
            SiteLatitude = 52.52,
            SiteLongitude = 13.405
        });
        symbolBroker = broker;
        return broker;
    }

    private static void ApplyDeterministicValues(ISequenceEntity entity) {
        ApplyDeterministicValues(entity, new HashSet<ISequenceEntity>(ReferenceEqualityComparer.Instance));
    }

    private static void ApplyDeterministicValues(ISequenceEntity entity, ISet<ISequenceEntity> visited) {
        if (!visited.Add(entity)) {
            return;
        }

        SetProperty(entity, "Iterations", 2);
        SetProperty(entity, "ExposureTime", 180d);
        SetProperty(entity, "Gain", 50);
        SetProperty(entity, "Offset", 25);
        SetProperty(entity, "Temperature", -10d);
        SetProperty(entity, "Duration", 10d);
        SetProperty(entity, "ImageType", "LIGHT");
        SetProperty(entity, "ComboBoxText", "L");

        if (entity is TimeCondition timeCondition) {
            timeCondition.SelectedProvider = timeCondition.DateTimeProviders
                .First(provider => provider.Name == "Time");
            timeCondition.DateTime = DocumentationFixedDateTime.Instance;
            timeCondition.Hours = 20;
            timeCondition.Minutes = 16;
            timeCondition.Seconds = 0;
            timeCondition.MinutesOffset = 0;
        }
        if (entity is WaitForTime waitForTime) {
            waitForTime.SelectedProvider = waitForTime.DateTimeProviders
                .First(provider => provider.Name == "Time");
            waitForTime.DateTime = DocumentationFixedDateTime.Instance;
            waitForTime.Hours = 20;
            waitForTime.Minutes = 16;
            waitForTime.Seconds = 0;
            waitForTime.MinutesOffset = 0;
        }
        if (entity is CoolCamera coolCamera) {
            coolCamera.Duration = 0;
        }
        if (entity is WarmCamera warmCamera) {
            warmCamera.Duration = 0;
        }
        if (entity is Annotation annotation) {
            annotation.Text = "This is my personal reminder";
        }
        if (entity is NINA.Sequencer.SequenceItem.Utility.MessageBox messageBox) {
            messageBox.Text = "Add my message here";
        }
        if (entity is ExternalScript externalScript) {
            externalScript.Script = @"C:\NINA\Scripts\after-exposure.cmd";
        }
        if (entity is SetBrightness setBrightness) {
            setBrightness.Brightness = 0;
        }
        if (entity is MoveFocuserAbsolute moveFocuserAbsolute) {
            moveFocuserAbsolute.Position = 0;
        }
        if (entity is WaitForTimeSpan waitForTimeSpan) {
            waitForTimeSpan.Time = 1;
        }
        if (entity is LoopWhile loopWhile) {
            loopWhile.PredicateExpression.Definition = "Camera_Temperature > 0";
        }
        if (entity is WaitUntil waitUntil) {
            waitUntil.PredicateExpression.Definition = "Camera_Temperature > 0";
        }
        if (entity is MoonIlluminationCondition moonIllumination) {
            moonIllumination.CurrentMoonIllumination = 42;
        }

        object? altitudeData = entity.GetType()
            .GetProperty("Data", BindingFlags.Instance | BindingFlags.Public)
            ?.GetValue(entity);
        if (altitudeData is not null) {
            SetProperty(altitudeData, "CurrentAltitude", 42d);
            SetProperty(altitudeData, "ExpectedTime", "12:00 AM");
        }

        PropertyInfo? binning = entity.GetType().GetProperty("Binning", BindingFlags.Instance | BindingFlags.Public);
        ConstructorInfo? binningConstructor = binning?.PropertyType.GetConstructor([typeof(short), typeof(short)])
            ?? binning?.PropertyType.GetConstructor([typeof(int), typeof(int)]);
        if (binning?.CanWrite == true && binningConstructor is not null) {
            Type parameterType = binningConstructor.GetParameters()[0].ParameterType;
            object one = Convert.ChangeType(1, parameterType, System.Globalization.CultureInfo.InvariantCulture);
            binning.SetValue(entity, binningConstructor.Invoke([one, one]));
        }

        if (entity is ISequenceContainer container) {
            foreach (ISequenceItem child in container.Items) {
                ApplyDeterministicValues(child, visited);
            }
        }
        if (entity is IConditionable conditionable) {
            foreach (ISequenceCondition condition in conditionable.Conditions) {
                ApplyDeterministicValues(condition, visited);
            }
        }
        if (entity is ITriggerable triggerable) {
            foreach (ISequenceTrigger trigger in triggerable.Triggers) {
                ApplyDeterministicValues(trigger, visited);
            }
        }
    }

    private static void SetProperty(object target, string name, object value) {
        PropertyInfo? property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (property?.CanWrite != true || !property.PropertyType.IsAssignableFrom(value.GetType())) {
            return;
        }
        property.SetValue(target, value);
    }

    private static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static string SplitWords(string value) => System.Text.RegularExpressions.Regex.Replace(value, "(?<!^)([A-Z])", " $1");

    private class InertDispatchProxy : DispatchProxy {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) {
            if (targetMethod is null || targetMethod.ReturnType == typeof(void)) {
                return null;
            }
            if (targetMethod.Name == "get_SequenceFile"
                && targetMethod.DeclaringType?.Name == "ICommandLineOptions") {
                return null;
            }
            if (targetMethod.Name.StartsWith("get_", StringComparison.Ordinal)
                && DocumentationDeviceValue(targetMethod) is object deviceValue) {
                return deviceValue;
            }
            if (args?.FirstOrDefault() is string resourceKey
                && Application.Current?.TryFindResource(resourceKey) is object resource
                && targetMethod.ReturnType.IsInstanceOfType(resource)) {
                return resource;
            }
            return InertValue.Create(targetMethod.ReturnType);
        }

        private static object? DocumentationDeviceValue(MethodInfo method) {
            string name = method.Name[4..];
            string declaringType = method.DeclaringType?.FullName ?? string.Empty;
            bool camera = declaringType.Contains("Camera", StringComparison.OrdinalIgnoreCase);
            bool telescope = declaringType.Contains("Telescope", StringComparison.OrdinalIgnoreCase);
            bool weather = declaringType.Contains("Weather", StringComparison.OrdinalIgnoreCase);
            return name switch {
                "Connected" => true,
                "CanSetTemperature" => true,
                "CanGetTemperature" => true,
                "CanSetCCDTemperature" => true,
                "CanGetGain" => true,
                "CanSetGain" => true,
                "CanSetOffset" => true,
                "CanSetUSBLimit" => true,
                "Name" => "N.I.N.A. Documentation Simulator",
                "Description" => "Deterministic offline documentation device",
                "DriverInfo" => "NINA documentation fixture",
                "DriverVersion" => "3.0",
                "State" => "Guiding",
                "CameraXSize" => 3000,
                "CameraYSize" => 2000,
                "ExposureMin" => 0.001d,
                "ExposureMax" => 3600d,
                "MaxBinX" => 4,
                "MaxBinY" => 4,
                "PixelSizeX" => 3.76d,
                "PixelSizeY" => 3.76d,
                "Gain" => 50,
                "GainMin" => 0,
                "GainMax" => 300,
                "Offset" => 25,
                "OffsetMin" => 0,
                "OffsetMax" => 100,
                "USBLimit" => 50,
                "USBLimitMin" => 0,
                "USBLimitMax" => 100,
                "USBLimitStep" => 1,
                "CoolerOn" => true,
                "CoolerPower" => 18.5d,
                "Temperature" when camera => -10d,
                "Temperature" when weather => 5.57d,
                "SiteLatitude" => 52.52d,
                "SiteLongitude" => 13.405d,
                "SiteElevation" => 34d,
                "RightAscension" => 2.065d,
                "Declination" => -1.6386d,
                "Altitude" => 38.92d,
                "Azimuth" => 180d,
                "TrackingEnabled" => true,
                "SiderealTime" => 2.065d,
                "HoursToMeridian" => 1.25d,
                "Humidity" => 50.25d,
                "Pressure" => 1021.1d,
                "DewPoint" => -3.97d,
                "CloudCover" => 0.25d,
                "RainRate" => 0d,
                "WindDirection" => 178.37d,
                "WindGust" => 2.5d,
                "WindSpeed" => 0.39d,
                "StarFWHM" => 0.88d,
                "SkyQuality" => 18.1d,
                "SkyBrightness" => 85.5d,
                "SkyTemperature" => -27.85d,
                _ => null
            };
        }
    }

    private static class InertValue {
        public static object? Create(Type type) {
            if (type == typeof(IProfileService)) {
                return Application.Current?.Resources["ProfileService"]
                    ?? throw new CatalogException("NINA's isolated profile service was not initialized.");
            }
            if (type == typeof(NINA.Astrometry.Interfaces.INighttimeCalculator)) {
                return DocumentationNighttimeCalculator.Instance;
            }
            if (type == typeof(NINA.Equipment.Equipment.MyCamera.CameraInfo)) {
                return new NINA.Equipment.Equipment.MyCamera.CameraInfo {
                    Connected = true,
                    CanSetTemperature = true,
                    CanSetGain = true,
                    GainMin = 0,
                    GainMax = 300,
                    Gain = 50,
                    DefaultGain = 50,
                    CanSetOffset = true,
                    OffsetMin = 0,
                    OffsetMax = 100,
                    Offset = 25,
                    DefaultOffset = 25,
                    BinX = 1,
                    BinY = 1,
                    Temperature = -10
                };
            }
            if (type == typeof(NINA.Equipment.Equipment.MyFilterWheel.FilterWheelInfo)) {
                NINA.Profile.Profile profile = (NINA.Profile.Profile)(Application.Current?.Resources["ActiveProfile"]
                    ?? throw new CatalogException("NINA's isolated profile was not initialized."));
                return new NINA.Equipment.Equipment.MyFilterWheel.FilterWheelInfo {
                    Connected = true,
                    SelectedFilter = profile.FilterWheelSettings.FilterWheelFilters.First()
                };
            }
            if (type == typeof(IList<IDateTimeProvider>)) {
                NINA.Sequencer.Utility.DateTimeProvider.TimeProvider timeProvider = new(DocumentationNighttimeCalculator.Instance) {
                    DateTime = DocumentationFixedDateTime.Instance
                };
                return new List<IDateTimeProvider> {
                    timeProvider,
                    new DocumentationDateTimeProvider("Sunset", new DateTime(2026, 8, 31, 19, 52, 0)),
                    new DocumentationDateTimeProvider("Nautical Dusk", new DateTime(2026, 8, 31, 20, 58, 0)),
                    new DocumentationDateTimeProvider("Astronomical Dusk", new DateTime(2026, 8, 31, 21, 36, 0)),
                    new DocumentationDateTimeProvider("Astronomical Dawn", new DateTime(2026, 9, 1, 4, 42, 0)),
                    new DocumentationDateTimeProvider("Nautical Dawn", new DateTime(2026, 9, 1, 5, 20, 0)),
                    new DocumentationDateTimeProvider("Sunrise", new DateTime(2026, 9, 1, 6, 26, 0)),
                    new DocumentationDateTimeProvider("Meridian", new DateTime(2026, 9, 1, 0, 0, 0))
                };
            }
            if (type == typeof(string)) {
                return string.Empty;
            }
            if (type == typeof(Task)) {
                return Task.CompletedTask;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
                Type resultType = type.GetGenericArguments()[0];
                MethodInfo fromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType);
                return fromResult.Invoke(null, [Create(resultType)]);
            }
            if (type.IsArray) {
                return Array.CreateInstance(type.GetElementType()!, 0);
            }
            if (TryCreateEmptyCollection(type, out object? collection)) {
                return collection;
            }
            if (type.IsInterface) {
                return DispatchProxy.Create(type, typeof(InertDispatchProxy));
            }
            if (type.IsValueType) {
                return Activator.CreateInstance(type);
            }
            ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
            return constructor is null ? null : constructor.Invoke(null);
        }

        private static bool TryCreateEmptyCollection(Type type, out object? collection) {
            collection = null;
            if (!type.IsGenericType) {
                return false;
            }
            Type definition = type.GetGenericTypeDefinition();
            if (definition != typeof(IEnumerable<>) && definition != typeof(ICollection<>) && definition != typeof(IList<>)
                && definition != typeof(IReadOnlyCollection<>) && definition != typeof(IReadOnlyList<>)) {
                return false;
            }
            collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
            return true;
        }
    }

    private sealed class DocumentationDateTimeProvider(string name, DateTime value) : IDateTimeProvider {
        public string Name { get; } = name;

        public DateTime GetDateTime(ISequenceEntity context) => value;

        public TimeOnly GetRolloverTime(ISequenceEntity context) => new(12, 0, 0);
    }

    private sealed class DocumentationFixedDateTime : NINA.Core.Utility.ICustomDateTime {
        public static DocumentationFixedDateTime Instance { get; } = new();
        public DateTime Now { get; } = new(2026, 8, 31, 20, 0, 0, DateTimeKind.Local);
        public DateTime UtcNow { get; } = new(2026, 8, 31, 18, 0, 0, DateTimeKind.Utc);
    }

    private sealed class DocumentationNighttimeCalculator : NINA.Astrometry.Interfaces.INighttimeCalculator {
        public static DocumentationNighttimeCalculator Instance { get; } = new();
        public event EventHandler? OnReferenceDayChanged { add { } remove { } }

        public NINA.Astrometry.NighttimeData Calculate(DateTime? selectedDate = null) {
            DateTime date = selectedDate ?? new DateTime(2026, 8, 31, 22, 0, 0, DateTimeKind.Local);
            DateTime referenceDate = new(2026, 8, 31, 12, 0, 0, DateTimeKind.Local);
            const double latitude = 52.52;
            const double longitude = 13.405;
            const double elevation = 34;
            NINA.Astrometry.ObserverInfo observer = new() {
                Latitude = latitude,
                Longitude = longitude,
                Elevation = elevation
            };
            return new NINA.Astrometry.NighttimeData(
                date,
                referenceDate,
                NINA.Astrometry.AstroUtil.GetMoonPhase(referenceDate, observer),
                NINA.Astrometry.AstroUtil.GetMoonIllumination(referenceDate, observer),
                NINA.Astrometry.AstroUtil.GetNightTimes(referenceDate, latitude, longitude, elevation),
                NINA.Astrometry.AstroUtil.GetNauticalNightTimes(referenceDate, latitude, longitude, elevation),
                NINA.Astrometry.AstroUtil.GetSunRiseAndSet(referenceDate, latitude, longitude, elevation),
                NINA.Astrometry.AstroUtil.GetMoonRiseAndSet(referenceDate, latitude, longitude, elevation),
                NINA.Astrometry.AstroUtil.GetCivilNightTimes(referenceDate, latitude, longitude, elevation));
        }
    }

    private sealed class DocumentationPluginLoader(
            IList<ISequenceItem> items,
            IList<ISequenceCondition> conditions,
            IList<ISequenceTrigger> triggers,
            IList<ISequenceContainer> containers) : IPluginLoader {
        public IDictionary<IPluginManifest, bool> Plugins { get; } = new Dictionary<IPluginManifest, bool>();
        public IList<ISequenceCondition> Conditions { get; } = conditions;
        public IList<ISequenceContainer> Container { get; } = containers;
        public IList<ISequenceItem> Items { get; } = items;
        public IList<ISequenceTrigger> Triggers { get; } = triggers;
        public IList<IDateTimeProvider> DateTimeProviders { get; } = [];
        public IList<NINA.Equipment.Interfaces.ViewModel.IDockableVM> DockableVMs { get; } = [];
        public IList<NINA.Core.Interfaces.IPluggableBehavior> PluggableBehaviors { get; } = [];
        public IList<NINA.Equipment.Interfaces.ViewModel.IEquipmentProvider> DeviceProviders { get; } = [];
        public IList<ISequenceEntityUpgrader> Upgraders { get; } = [];
        public Task Load() => Task.CompletedTask;
    }

    private sealed class DocumentationGuideStep(
            double frame,
            double raDistance,
            double decDistance,
            double raDuration,
            double decDuration) : NINA.Core.Interfaces.IGuideStep {
        public string Event => "GuideStep";
        public string TimeStamp => new DateTime(2026, 8, 31, 22, 0, 0).AddSeconds(frame).ToString("O");
        public string Host => "Documentation";
        public int Inst => 1;
        public double Frame => frame;
        public double Time => frame;
        public double RADistanceRaw { get; set; } = raDistance;
        public double DECDistanceRaw { get; set; } = decDistance;
        public double RADuration { get; } = raDuration;
        public double DECDuration { get; } = decDuration;
        public NINA.Core.Interfaces.IGuideStep Clone() => new DocumentationGuideStep(
            Frame, RADistanceRaw, DECDistanceRaw, RADuration, DECDuration);
    }

    private sealed class DocumentationImageStatistics(double mean = 3857.52) : NINA.Image.Interfaces.IImageStatistics {
        public int BitDepth => 16;
        public double StDev => 687.76;
        public double Mean { get; } = mean;
        public double Median => 3840;
        public double MedianAbsoluteDeviation => 192;
        public int Max => 65504;
        public long MaxOccurrences => 944;
        public int Min => 2464;
        public long MinOccurrences => 1;
        public System.Collections.Immutable.ImmutableList<DataPoint> Histogram { get; } =
            System.Collections.Immutable.ImmutableList.CreateRange(
                Enumerable.Range(0, 256).Select(index => new DataPoint(
                    index * 100d / 255d,
                    8000 * Math.Exp(-Math.Pow(index - 16, 2) / 35d))));
    }

    private sealed class DocumentationStarDetectionAnalysis : NINA.Image.Interfaces.IStarDetectionAnalysis {
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
        public double HFR { get; set; }
        public double FWHM { get; set; }
        public double Eccentricity { get; set; }
        public double HFRStDev { get; set; }
        public int DetectedStars { get; set; }
        public List<NINA.Image.ImageAnalysis.DetectedStar> StarList { get; set; } = [];
    }

    private class DocumentationReadOnlySwitch(
            short id,
            string name,
            string description,
            double value) : NINA.Equipment.Interfaces.ISwitch {
        public short Id { get; } = id;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public double Value { get; protected set; } = value;
        public bool Poll() => true;
    }

    private sealed class DocumentationWritableSwitch(
            short id,
            string name,
            string description,
            double minimum,
            double maximum,
            double stepSize,
            double value) : DocumentationReadOnlySwitch(id, name, description, value), NINA.Equipment.Interfaces.IWritableSwitch {
        public double Maximum { get; } = maximum;
        public double Minimum { get; } = minimum;
        public double StepSize { get; } = stepSize;
        public double TargetValue { get; set; } = value;
        public void SetValue() => Value = Math.Clamp(TargetValue, Minimum, Maximum);
    }
}
