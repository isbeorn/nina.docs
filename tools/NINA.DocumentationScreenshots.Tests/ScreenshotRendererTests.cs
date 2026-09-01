#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NUnit.Framework;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.InteropServices;

namespace NINA.DocumentationScreenshots.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class ScreenshotRendererTests {
    private string root = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp() => WpfBootstrap.Initialize();

    [SetUp]
    public void SetUp() {
        root = Path.Combine(Path.GetTempPath(), $"nina-screenshot-renderer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(root, recursive: true);

    [Test]
    public void Initialize_UsesUnitedStatesLocaleForThreadsWpfAndProfile() {
        CultureInfo expected = CultureInfo.GetCultureInfo("en-US");
        NINA.Profile.Profile profile = (NINA.Profile.Profile)Application.Current.Resources["ActiveProfile"];
        FrameworkElement element = new();

        Assert.Multiple(() => {
            Assert.That(CultureInfo.CurrentCulture.Name, Is.EqualTo(expected.Name));
            Assert.That(CultureInfo.CurrentUICulture.Name, Is.EqualTo(expected.Name));
            Assert.That(CultureInfo.DefaultThreadCurrentCulture?.Name, Is.EqualTo(expected.Name));
            Assert.That(CultureInfo.DefaultThreadCurrentUICulture?.Name, Is.EqualTo(expected.Name));
            Assert.That(element.Language.GetEquivalentCulture().Name, Is.EqualTo(expected.Name));
            Assert.That(profile.ApplicationSettings.Culture, Is.EqualTo(expected.Name));
        });
    }

    [Test]
    public void Render_UsesRealCompiledViewAndProducesNonBlankPngAtRequestedSize() {
        ScreenshotAsset asset = Managed("general", 640, 480);
        string output = Path.Combine(root, "general.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        BitmapDecoder decoder = BitmapDecoder.Create(new Uri(output), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        Assert.That(decoder.Frames[0].PixelWidth, Is.EqualTo(640));
        Assert.That(decoder.Frames[0].PixelHeight, Is.EqualTo(480));
        Assert.That(decoder.Frames[0].DpiX, Is.EqualTo(96).Within(0.02));
        Assert.That(decoder.Frames[0].DpiY, Is.EqualTo(96).Within(0.02));
        Assert.That(new FileInfo(output).Length, Is.GreaterThan(1000));
    }

    [Test]
    public void Render_IsDeterministicForSameFixture() {
        ScreenshotAsset asset = Managed("deterministic", 480, 320);
        string first = Path.Combine(root, "first.png");
        string second = Path.Combine(root, "second.png");

        ScreenshotRenderer renderer = new(new FixtureRegistry());
        renderer.Render(asset, first);
        renderer.Render(asset, second);

        Assert.That(File.ReadAllBytes(second), Is.EqualTo(File.ReadAllBytes(first)));
    }

    [Test]
    public void Render_NestedConditionsUsesFixedClock() {
        ScreenshotAsset asset = new() {
            Id = "deterministic-nested-conditions",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/Sequencer_NestedConditions.png",
            Fixture = "sequencer",
            State = "docs-images-sequencer-sequencer-nestedconditions-png",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1434,
            Height = 891,
            RenderWidth = 1793,
            RenderHeight = 1341,
            CropTarget = "target-area:first-item"
        };
        string first = Path.Combine(root, "nested-conditions-first.png");
        string second = Path.Combine(root, "nested-conditions-second.png");

        ScreenshotRenderer renderer = new(new FixtureRegistry());
        renderer.Render(asset, first);
        Thread.Sleep(TimeSpan.FromSeconds(1.1));
        renderer.Render(asset, second);

        Assert.That(File.ReadAllBytes(second), Is.EqualTo(File.ReadAllBytes(first)));
    }

    [Test]
    public void Render_ContainerMenuIsDeterministicRegardlessOfDesktopCursorPosition() {
        ScreenshotAsset asset = new() {
            Id = "deterministic-add-condition",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/sequencer/Sequencer_AddLoopCondition.png",
            Fixture = "sequencer",
            State = "deterministic-add-condition",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1623,
            Height = 250,
            RenderWidth = 2029,
            RenderHeight = 1274,
            CropTarget = "target-area:first-item"
        };
        string first = Path.Combine(root, "condition-cursor-away.png");
        string second = Path.Combine(root, "condition-cursor-over-menu.png");
        Assert.That(GetCursorPos(out NativePoint original), Is.True);
        uint uiThreadId = GetCurrentThreadId();

        try {
            Assert.That(SetCursorPos(0, 0), Is.True);
            new ScreenshotRenderer(new FixtureRegistry()).Render(asset, first);
            using CancellationTokenSource cancellation = new();
            Task hoverPopup = Task.Run(() => HoverPopupWindows(uiThreadId, cancellation.Token));
            try {
                new ScreenshotRenderer(new FixtureRegistry()).Render(asset, second);
            } finally {
                cancellation.Cancel();
                hoverPopup.Wait();
            }
        } finally {
            _ = SetCursorPos(original.X, original.Y);
        }

        Assert.That(File.ReadAllBytes(second), Is.EqualTo(File.ReadAllBytes(first)),
            "Desktop hover state must not alter a generated production ContextMenu capture.");
    }

    [Test]
    public void Render_RootAddMenuRejectsACanvasThatWouldClipThePopup() {
        ScreenshotAsset asset = new() {
            Id = "root-add-menu-padding",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/sequencer/Sequencer_AddInstruction.png",
            Fixture = "sequencer",
            State = "docs-images-sequencer-sequencer-addinstruction-png",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 517,
            Height = 306,
            RenderWidth = 1450,
            RenderHeight = 900,
            CropTarget = "root-add-menu"
        };
        string output = Path.Combine(root, "root-add-menu-padding.png");

        Assert.That(
            () => new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output),
            Throws.TypeOf<CatalogException>()
                .With.Message.Contains("Increase renderWidth or renderHeight instead of clipping"));
    }

    [Test]
    public void Render_AppliesCropsAndCalloutsAtBothBoundaryDirections() {
        ScreenshotAsset asset = new() {
            Id = "crop-callouts",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/crop-callouts.png",
            Fixture = "view",
            State = "NINA.View.Options.GeneralView",
            Width = 400,
            Height = 300,
            Crop = new ScreenshotCrop { X = 0, Y = 0, Width = 1, Height = 1 },
            Callouts = [
                new ScreenshotCallout { X = 0, Y = 0, Text = "min" },
                new ScreenshotCallout { X = 1, Y = 1, Text = "max" }
            ]
        };
        string output = Path.Combine(root, "crop-callouts.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        Assert.That(File.Exists(output), Is.True);
    }

    [Test]
    public void Render_UsesFullSizeCanvasBeforeApplyingDocumentationCrop() {
        ScreenshotAsset asset = new() {
            Id = "full-size-crop",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/full-size-crop.png",
            Fixture = "view",
            State = "NINA.View.Options.GeneralView",
            Width = 400,
            Height = 300,
            RenderWidth = 1200,
            RenderHeight = 900,
            Crop = new ScreenshotCrop { X = 0.5, Y = 0, Width = 0.5, Height = 0.5 }
        };
        string output = Path.Combine(root, "full-size-crop.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        BitmapFrame frame = BitmapDecoder.Create(
            new Uri(output),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad).Frames[0];
        Assert.Multiple(() => {
            Assert.That(frame.PixelWidth, Is.EqualTo(400));
            Assert.That(frame.PixelHeight, Is.EqualTo(300));
        });
    }

    [Test]
    public void Render_CropsToTheFirstRealTargetAreaSequenceContainer() {
        ScreenshotAsset asset = new() {
            Id = "sequence-container-crop",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/Sequencer_SequentialInstructions.png",
            Fixture = "sequencer",
            State = "sequencer-sequential-instructions",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1000,
            Height = 320,
            RenderWidth = 1800,
            RenderHeight = 900,
            CropTarget = "target-area:first-item"
        };
        string output = Path.Combine(root, "sequence-container-crop.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        BitmapFrame frame = BitmapDecoder.Create(
            new Uri(output),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad).Frames[0];
        Assert.Multiple(() => {
            Assert.That(frame.PixelWidth, Is.EqualTo(1000));
            Assert.That(frame.PixelHeight, Is.EqualTo(320));
            Assert.That(new FileInfo(output).Length, Is.GreaterThan(5000));
        });
    }

    [Test]
    public void Render_CropsToTheRealMeridianFlipSettingsGroup() {
        ScreenshotAsset asset = new() {
            Id = "meridian-flip-settings",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/advanced/meridian_flip1.png",
            Fixture = "view",
            State = "meridian-flip-settings",
            ViewType = "NINA.View.Options.ImagingView",
            Width = 720,
            Height = 520,
            RenderWidth = 1440,
            RenderHeight = 900,
            CropTarget = "settings:meridian-flip"
        };
        string output = Path.Combine(root, "meridian-flip-settings.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        Assert.That(File.Exists(output), Is.True);
        Assert.That(new FileInfo(output).Length, Is.GreaterThan(5000));
    }

    [Test]
    public void Render_RendersTheExtractedProductionSettingsGroupAtCompactSize() {
        ScreenshotAsset asset = new() {
            Id = "meridian-flip-settings-compact",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/advanced/meridianflip.PNG",
            Fixture = "settings-group",
            State = "meridian-flip-settings-compact",
            Width = 315,
            Height = 237
        };
        string output = Path.Combine(root, "meridian-flip-settings-compact.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        Assert.That(new FileInfo(output).Length, Is.GreaterThan(5000));
    }

    [Test]
    public void DynamicCropBounds_AreExpandedToOutputAspectWithoutDistortion() {
        System.Reflection.MethodInfo method = typeof(ScreenshotRenderer).GetMethod(
            "ExpandBoundsToAspect",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?? throw new AssertionException("The renderer has no aspect-preserving dynamic crop helper.");

        Rect wide = (Rect)method.Invoke(null, [new Rect(100, 100, 800, 100), 2d, new Rect(0, 0, 1200, 900)])!;
        Rect tall = (Rect)method.Invoke(null, [new Rect(100, 100, 100, 600), 2d, new Rect(0, 0, 1200, 900)])!;

        Assert.Multiple(() => {
            Assert.That(wide.Width / wide.Height, Is.EqualTo(2d).Within(0.0001));
            Assert.That(tall.Width / tall.Height, Is.EqualTo(2d).Within(0.0001));
            Assert.That(wide.Contains(new Rect(100, 100, 800, 100)), Is.True);
            Assert.That(tall.Contains(new Rect(100, 100, 100, 600)), Is.True);
        });
    }

    [Test]
    public void DynamicCropBounds_RejectImpossibleAspectInsteadOfClippingTheRealView() {
        System.Reflection.MethodInfo method = typeof(ScreenshotRenderer).GetMethod(
            "ExpandBoundsToAspect",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?? throw new AssertionException("The renderer has no aspect-preserving dynamic crop helper.");

        System.Reflection.TargetInvocationException exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            method.Invoke(null, [new Rect(0, 0, 800, 600), 4d, new Rect(0, 0, 900, 700)]))!;

        Assert.That(exception.InnerException, Is.TypeOf<CatalogException>());
    }

    [Test]
    public void Render_CapturesProductionComboBoxPopupForNamedDropdownState() {
        ScreenshotAsset asset = new() {
            Id = "loop-until-time-dropdown",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/sequencer/conditions/loopuntiltime.png",
            Fixture = "sequencer-entity",
            State = "loop-until-time-dropdown",
            SourceIdentifier = "NINA.Sequencer.Conditions.TimeCondition",
            DisplayName = "Loop Until Time",
            Width = 640,
            Height = 168
        };
        string output = Path.Combine(root, "loop-until-time.png");

        new ScreenshotRenderer(new FixtureRegistry()).Render(asset, output);

        BitmapFrame frame = BitmapDecoder.Create(
            new Uri(output),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad).Frames[0];
        int stride = frame.PixelWidth * 4;
        byte[] pixels = new byte[stride * frame.PixelHeight];
        frame.CopyPixels(pixels, stride, 0);
        HashSet<uint> lowerHalfColors = [];
        for (int y = frame.PixelHeight / 2; y < frame.PixelHeight; y += 4) {
            for (int x = 0; x < frame.PixelWidth; x += 4) {
                lowerHalfColors.Add(BitConverter.ToUInt32(pixels, y * stride + x * 4));
            }
        }

        Assert.That(lowerHalfColors.Count, Is.GreaterThan(8),
            "The named state must include the real ComboBox popup, not only the collapsed production control.");
    }

    [Test]
    public void CreateSequencerEntity_UsesTheSelectedProductionTypeAndItsDeclaredIcon() {
        ScreenshotAsset asset = new() {
            Id = "cool-camera-production-icon",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/instructions/camera_cool.png",
            Fixture = "sequencer-entity",
            State = "cool-camera-production-icon",
            SourceIdentifier = "sequencer:CoolCamera",
            DisplayName = "Cool Camera",
            Icon = "CameraSVG",
            Width = 640,
            Height = 120
        };

        FrameworkElement element = DocumentationApplicationHost.Instance.CreateSequencerEntity(asset);
        object entity = element.DataContext
            ?? throw new AssertionException("The production sequencer element has no entity DataContext.");
        object? renderedIcon = entity.GetType().GetProperty("Icon")?.GetValue(entity);
        object? productionIcon = Application.Current.TryFindResource("SnowflakeSVG");

        Assert.Multiple(() => {
            Assert.That(entity.GetType().Name, Is.EqualTo("CoolCamera"));
            Assert.That(renderedIcon, Is.SameAs(productionIcon),
                "The renderer must use the selected entity type's production ExportMetadata icon, not a catalog guess.");
        });
    }

    [Test]
    public void CreateSequencerEntity_UsesTheExactUnsafeConditionTypeAndProductionIcon() {
        FrameworkElement element = DocumentationApplicationHost.Instance.CreateSequencerEntity(
            SequencerEntity("loop-while-unsafe", "sequencer:LoopWhileUnsafe"));
        object entity = element.DataContext
            ?? throw new AssertionException("The production sequencer element has no entity DataContext.");

        Assert.Multiple(() => {
            Assert.That(entity.GetType().Name, Is.EqualTo("LoopWhileUnsafe"));
            Assert.That(Property(entity, "Icon"), Is.SameAs(Application.Current.TryFindResource("UnshieldSVG")));
        });
    }

    [Test]
    public void CreateSequencerEntity_AppliesExplicitDeterministicExampleValues() {
        object coolCamera = Entity("cool-camera", "sequencer:CoolCamera");
        object warmCamera = Entity("warm-camera", "sequencer:WarmCamera");
        object annotation = Entity("annotation", "sequencer:Annotation");
        object messageBox = Entity("message-box", "sequencer:MessageBox");
        object externalScript = Entity("external-script", "sequencer:ExternalScript");
        object setBrightness = Entity("set-brightness", "sequencer:SetBrightness");
        object moveFocuser = Entity("move-focuser", "sequencer:MoveFocuserAbsolute");
        object waitForTimeSpan = Entity("wait-for-time-span", "sequencer:WaitForTimeSpan");
        object waitForTime = Entity("wait-for-time", "sequencer:WaitForTime");
        object loopWhile = Entity("loop-while", "sequencer:LoopWhile");
        object waitUntil = Entity("wait-until", "sequencer:WaitUntil");
        object moonIllumination = Entity("moon-illumination", "sequencer:MoonIlluminationCondition");
        object waitForAltitude = Entity("wait-for-altitude", "sequencer:WaitForAltitude");

        Assert.Multiple(() => {
            Assert.That(Property(coolCamera, "Duration"), Is.EqualTo(0d));
            Assert.That(Property(warmCamera, "Duration"), Is.EqualTo(0d));
            Assert.That(Property(annotation, "Text"), Is.EqualTo("This is my personal reminder"));
            Assert.That(Property(messageBox, "Text"), Is.EqualTo("Add my message here"));
            Assert.That(Property(externalScript, "Script"), Is.EqualTo(@"C:\NINA\Scripts\after-exposure.cmd"));
            Assert.That(Property(setBrightness, "Brightness"), Is.EqualTo(0));
            Assert.That(Property(moveFocuser, "Position"), Is.EqualTo(0));
            Assert.That(Property(waitForTimeSpan, "Time"), Is.EqualTo(1d));
            Assert.That(Property(waitForTime, "Hours"), Is.EqualTo(20));
            Assert.That(Property(waitForTime, "Minutes"), Is.EqualTo(16));
            Assert.That(Property(waitForTime, "Seconds"), Is.EqualTo(0));
            Assert.That(Property(Property(waitForTime, "SelectedProvider")!, "Name"), Is.EqualTo("Time"));
            Assert.That(Property(Property(loopWhile, "PredicateExpression")!, "Definition"), Is.EqualTo("Camera_Temperature > 0"));
            Assert.That(Property(Property(waitUntil, "PredicateExpression")!, "Definition"), Is.EqualTo("Camera_Temperature > 0"));
            Assert.That(Property(moonIllumination, "CurrentMoonIllumination"), Is.EqualTo(42d));
            Assert.That(Property(Property(waitForAltitude, "Data")!, "CurrentAltitude"), Is.EqualTo(42d));
            Assert.That(Property(Property(waitForAltitude, "Data")!, "ExpectedTime"), Is.EqualTo("12:00 AM"));
        });
    }

    private static object Entity(string id, string sourceIdentifier) {
        return DocumentationApplicationHost.Instance.CreateSequencerEntity(
            SequencerEntity(id, sourceIdentifier)).DataContext
            ?? throw new AssertionException($"The production sequencer element '{id}' has no entity DataContext.");
    }

    private static ScreenshotAsset SequencerEntity(string id, string sourceIdentifier) => new() {
        Id = id,
        Classification = ScreenshotClassification.NinaUi,
        Output = $"docs/images/{id}.png",
        Fixture = "sequencer-entity",
        State = id,
        SourceIdentifier = sourceIdentifier,
        Width = 720,
        Height = 70
    };

    private static object? Property(object target, string name) {
        return target.GetType().GetProperty(name)?.GetValue(target)
            ?? throw new AssertionException($"Production type '{target.GetType().Name}' has no value for '{name}'.");
    }

    [Test]
    public void Render_CanCaptureBothProductionContainerMenusSequentially() {
        ScreenshotRenderer renderer = new(new FixtureRegistry());
        ScreenshotAsset condition = ContainerMenuAsset(
            "add-condition",
            "docs/images/sequencer/Sequencer_AddLoopCondition.png");
        ScreenshotAsset trigger = ContainerMenuAsset(
            "add-trigger",
            "docs/images/sequencer/Sequencer_AddTrigger.png");

        renderer.Render(condition, Path.Combine(root, "condition.png"));
        renderer.Render(trigger, Path.Combine(root, "trigger.png"));

        Assert.Multiple(() => {
            Assert.That(new FileInfo(Path.Combine(root, "condition.png")).Length, Is.GreaterThan(5000));
            Assert.That(new FileInfo(Path.Combine(root, "trigger.png")).Length, Is.GreaterThan(5000));
        });
    }

    [Test]
    public void Main_WhenLaterFixtureFails_DoesNotReplaceEarlierScreenshot() {
        byte[] original = [1, 2, 3, 4];
        string target = CreateTarget("generated/first.png", original);
        string catalog = WriteCatalog("""
            {
              "schemaVersion": 1,
              "assets": [
                { "id": "first", "classification": "nina-ui", "output": "docs/images/generated/first.png", "fixture": "view", "state": "NINA.View.Options.GeneralView", "width": 320, "height": 240 },
                { "id": "failure", "classification": "nina-ui", "output": "docs/images/generated/failure.png", "fixture": "view", "state": "NINA.View.DoesNotExist", "width": 320, "height": 240 }
              ]
            }
            """);

        int exitCode = Program.Main(["--catalog", catalog, "--docs-root", root]);

        Assert.That(exitCode, Is.EqualTo(1));
        Assert.That(File.ReadAllBytes(target), Is.EqualTo(original));
        Assert.That(File.Exists(Path.Combine(root, "docs", "images", "generated", "failure.png")), Is.False);
    }

    [Test]
    public void Main_WhenQualityValidationFails_ReportsEveryFailureAndReplacesNothing() {
        string first = CreatePngTarget("generated/first.png");
        byte[] original = File.ReadAllBytes(first);
        CreateDetailedPngTarget("generated/degraded-one.png", 320, 240);
        CreateDetailedPngTarget("generated/degraded-two.png", 321, 240);
        string catalog = WriteCatalog("""
            {
              "schemaVersion": 1,
              "assets": [
                { "id": "first", "classification": "nina-ui", "output": "docs/images/generated/first.png", "fixture": "view", "state": "NINA.View.Options.GeneralView", "width": 320, "height": 240 },
                { "id": "degraded-one", "classification": "nina-ui", "output": "docs/images/generated/degraded-one.png", "fixture": "view", "state": "NINA.View.MeridianFlipView", "width": 320, "height": 240 },
                { "id": "degraded-two", "classification": "nina-ui", "output": "docs/images/generated/degraded-two.png", "fixture": "view", "state": "NINA.View.MeridianFlipView", "width": 321, "height": 240 }
              ]
            }
            """);
        StringWriter errors = new();
        TextWriter originalError = Console.Error;
        Console.SetError(errors);
        try {
            int exitCode = Program.Main(["--catalog", catalog, "--docs-root", root]);

            Assert.Multiple(() => {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(File.ReadAllBytes(first), Is.EqualTo(original));
                Assert.That(errors.ToString(), Does.Contain("degraded-one"));
                Assert.That(errors.ToString(), Does.Contain("degraded-two"));
            });
        } finally {
            Console.SetError(originalError);
        }
    }

    [Test]
    public void Main_WhenCatalogContainsExcludedAssets_LeavesTheirOriginalFilesUntouched() {
        byte[] original = [7, 6, 5, 4, 3, 2, 1];
        string excluded = CreateTarget("external.png", original);
        string catalog = WriteCatalog("""
            {
              "schemaVersion": 1,
              "assets": [
                { "id": "managed", "classification": "nina-ui", "output": "docs/images/generated/managed.png", "fixture": "view", "state": "NINA.View.Options.GeneralView", "width": 320, "height": 240 },
                { "id": "external", "classification": "external-ui", "output": "docs/images/external.png", "width": 100, "height": 100, "exclusionReason": "This belongs to third-party software." }
              ]
            }
            """);

        int exitCode = Program.Main(["--catalog", catalog, "--docs-root", root]);

        Assert.Multiple(() => {
            Assert.That(exitCode, Is.EqualTo(0));
            Assert.That(File.ReadAllBytes(excluded), Is.EqualTo(original));
            Assert.That(File.Exists(Path.Combine(root, "docs", "images", "generated", "managed.png")), Is.True);
        });
    }

    [Test]
    public void Main_AreaFilterMatchesAreaBelowGeneratedRoot() {
        TextWriter originalOutput = Console.Out;
        StringWriter output = new();
        string catalog = WriteCatalog("""
            {
              "schemaVersion": 1,
              "assets": [
                { "id": "tabs", "classification": "nina-ui", "output": "docs/images/generated/tabs/tabs.png", "fixture": "view", "state": "NINA.View.Options.GeneralView", "width": 320, "height": 240 },
                { "id": "sequencer", "classification": "nina-ui", "output": "docs/images/generated/sequencer/sequencer.png", "fixture": "view", "state": "NINA.View.MeridianFlipView", "width": 320, "height": 240 }
              ]
            }
            """);

        int exitCode;
        try {
            Console.SetOut(output);
            exitCode = Program.Main(["--catalog", catalog, "--docs-root", root, "--area", "tabs", "--preview"]);
        } finally {
            Console.SetOut(originalOutput);
        }

        Assert.Multiple(() => {
            Assert.That(exitCode, Is.EqualTo(0));
            Assert.That(output.ToString(), Does.Contain("Rendering tabs..."));
            Assert.That(output.ToString(), Does.Not.Contain("Rendering sequencer..."));
        });
    }

    [Test]
    public void Main_InPreviewMode_ReportsChangeWithoutReplacingScreenshot() {
        string target = CreatePngTarget("generated/preview.png");
        byte[] original = File.ReadAllBytes(target);
        TextWriter originalOutput = Console.Out;
        StringWriter output = new();
        string catalog = WriteCatalog("""
            {
              "schemaVersion": 1,
              "assets": [
                { "id": "preview", "classification": "nina-ui", "output": "docs/images/generated/preview.png", "fixture": "view", "state": "NINA.View.Options.GeneralView", "width": 320, "height": 240 }
              ]
            }
            """);

        int exitCode;
        try {
            Console.SetOut(output);
            exitCode = Program.Main(["--catalog", catalog, "--docs-root", root, "--preview"]);
        } finally {
            Console.SetOut(originalOutput);
        }

        Assert.That(exitCode, Is.EqualTo(0));
        Assert.That(File.ReadAllBytes(target), Is.EqualTo(original));
        Assert.That(output.ToString(), Does.Contain("Changed: preview -> docs/images/generated/preview.png"));
    }

    private string CreateTarget(string relativePath, byte[] content) {
        string directory = Path.Combine(root, "docs", "images");
        string target = Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllBytes(target, content);
        return target;
    }

    private string CreatePngTarget(string relativePath) {
        string directory = Path.Combine(root, "docs", "images");
        string target = Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        BitmapSource bitmap = BitmapSource.Create(
            320,
            240,
            96,
            96,
            System.Windows.Media.PixelFormats.Bgra32,
            null,
            new byte[320 * 240 * 4],
            320 * 4);
        using FileStream output = File.Create(target);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(output);
        return target;
    }

    private string CreateDetailedPngTarget(string relativePath, int width, int height) {
        string directory = Path.Combine(root, "docs", "images");
        string target = Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        byte[] pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                byte value = (byte)(((x / 6 + y / 6) % 2) * 255);
                int offset = (y * width + x) * 4;
                pixels[offset] = value;
                pixels[offset + 1] = value;
                pixels[offset + 2] = value;
                pixels[offset + 3] = 255;
            }
        }
        BitmapSource bitmap = BitmapSource.Create(
            width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, width * 4);
        using FileStream output = File.Create(target);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(output);
        return target;
    }

    private string WriteCatalog(string content) {
        string path = Path.Combine(root, "catalog.json");
        File.WriteAllText(path, content);
        return path;
    }

    private static ScreenshotAsset Managed(string id, int width, int height) => new() {
        Id = id,
        Classification = ScreenshotClassification.NinaUi,
        Output = $"docs/images/{id}.png",
        Fixture = "view",
        State = "NINA.View.Options.GeneralView",
        Width = width,
        Height = height
    };

    private static ScreenshotAsset ContainerMenuAsset(string id, string output) => new() {
        Id = id,
        Classification = ScreenshotClassification.NinaUi,
        Output = output,
        Fixture = "sequencer",
        State = id,
        ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
        Width = 700,
        Height = 420,
        RenderWidth = 1450,
        RenderHeight = 1274,
        CropTarget = "target-area:first-item"
    };

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint threadId, EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out NativeRect bounds);

    private static void HoverPopupWindows(uint uiThreadId, CancellationToken token) {
        while (!token.IsCancellationRequested) {
            NativeRect? popup = null;
            _ = EnumThreadWindows(uiThreadId, (window, _) => {
                if (!IsWindowVisible(window) || !GetWindowRect(window, out NativeRect bounds)) {
                    return true;
                }
                int width = bounds.Right - bounds.Left;
                int height = bounds.Bottom - bounds.Top;
                if (width is >= 200 and <= 900 && height is >= 50 and <= 1100) {
                    popup = bounds;
                }
                return true;
            }, IntPtr.Zero);
            if (popup is NativeRect bounds) {
                _ = SetCursorPos((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
                keybd_event(0x28, 0, 0, UIntPtr.Zero);
                keybd_event(0x28, 0, 0x0002, UIntPtr.Zero);
            }
            Thread.Sleep(10);
        }
    }

    private delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
