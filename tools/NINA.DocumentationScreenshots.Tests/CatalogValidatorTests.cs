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

namespace NINA.DocumentationScreenshots.Tests;

[TestFixture]
public class CatalogValidatorTests {
    private string root = null!;

    [SetUp]
    public void SetUp() {
        root = Path.Combine(Path.GetTempPath(), $"nina-screenshot-catalog-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
    }

    [TearDown]
    public void TearDown() {
        Directory.Delete(root, recursive: true);
    }

    [Test]
    public void Validate_AcceptsMinimumAndMaximumDimensionsAndCalloutEdges() {
        ScreenshotCatalog catalog = new() {
            SchemaVersion = 1,
            Assets = [
                Managed("minimum", "docs/images/generated/minimum.png", 16, 16, [new ScreenshotCallout { X = 0, Y = 0, Text = "1" }]),
                Managed("maximum", "docs/images/generated/maximum.png", 8192, 8192, [new ScreenshotCallout { X = 1, Y = 1, Text = "2" }])
            ]
        };

        IReadOnlyDictionary<ScreenshotAsset, string> result = CatalogValidator.Validate(catalog, root, new FixtureRegistry());

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [TestCase(15, 16)]
    [TestCase(16, 15)]
    [TestCase(8193, 8192)]
    [TestCase(8192, 8193)]
    public void Validate_RejectsDimensionsOutsideBothBoundaries(int width, int height) {
        ScreenshotCatalog catalog = Catalog(Managed("invalid", "docs/images/generated/invalid.png", width, height));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("invalid dimensions"));
    }

    [Test]
    public void Validate_RejectsDuplicateIdsAndOutputs() {
        ScreenshotCatalog duplicateIds = Catalog(
            Managed("same", "docs/images/generated/first.png"),
            Managed("same", "docs/images/generated/second.png"));
        ScreenshotCatalog duplicateOutputs = Catalog(
            Managed("first", "docs/images/generated/same.png"),
            Managed("second", "docs/images/generated/same.png"));

        Assert.That(() => CatalogValidator.Validate(duplicateIds, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(duplicateOutputs, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
    }

    [TestCase("docs/images/generated/generated.jpg")]
    [TestCase("docs/images/generated/generated.JPEG")]
    public void Validate_RejectsManagedOutputsThatAreNotPng(string output) {
        ScreenshotCatalog catalog = Catalog(Managed("generated", output));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("must use a PNG output path"));
    }

    [Test]
    public void Validate_RequiresManagedOutputsInsideGeneratedImageRoot() {
        ScreenshotCatalog catalog = Catalog(Managed("legacy-location", "docs/images/legacy-location.png"));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("docs/images/generated"));
    }

    [Test]
    public void Validate_RejectsCatalogGuessesForProductionSequencerIcons() {
        ScreenshotAsset asset = new() {
            Id = "cool-camera",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/cool-camera.png",
            Fixture = "sequencer-entity",
            State = "cool-camera",
            SourceIdentifier = "sequencer:CoolCamera",
            Icon = "CameraSVG",
            Width = 720,
            Height = 35
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("production metadata"));
    }

    [Test]
    public void Validate_RejectsDifferentScreenshotsThatWouldRenderIdentically() {
        ScreenshotCatalog catalog = Catalog(
            Managed("open-dome", "docs/images/generated/open-dome.png", 200, 35),
            Managed("park-scope", "docs/images/generated/park-scope.png", 200, 35));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("identical render state"));
    }

    [Test]
    public void Validate_RejectsPathTraversalAndCalloutsOutsideEveryEdge() {
        ScreenshotCatalog traversal = Catalog(Managed("escape", "../escape.png"));
        ScreenshotCatalog left = Catalog(Managed("left", "docs/images/generated/left.png", callouts: [new ScreenshotCallout { X = -0.01, Y = 0.5, Text = "1" }]));
        ScreenshotCatalog top = Catalog(Managed("top", "docs/images/generated/top.png", callouts: [new ScreenshotCallout { X = 0.5, Y = -0.01, Text = "1" }]));
        ScreenshotCatalog right = Catalog(Managed("right", "docs/images/generated/right.png", callouts: [new ScreenshotCallout { X = 1.01, Y = 0.5, Text = "1" }]));
        ScreenshotCatalog bottom = Catalog(Managed("bottom", "docs/images/generated/bottom.png", callouts: [new ScreenshotCallout { X = 0.5, Y = 1.01, Text = "1" }]));

        Assert.That(() => CatalogValidator.Validate(traversal, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(left, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(top, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(right, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(bottom, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
    }

    [Test]
    public void Validate_AcceptsArrowCalloutAtMinimumAndMaximumCoordinates() {
        ScreenshotAsset asset = Managed(
            "edge-arrow",
            "docs/images/generated/edge-arrow.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Arrow,
                    Points = [
                        new ScreenshotPoint { X = 0, Y = 0 },
                        new ScreenshotPoint { X = 1, Y = 1 }
                    ]
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase(-0.01, 0.5)]
    [TestCase(0.5, -0.01)]
    [TestCase(1.01, 0.5)]
    [TestCase(0.5, 1.01)]
    public void Validate_RejectsArrowCalloutPointsOutsideEveryEdge(double x, double y) {
        ScreenshotAsset asset = Managed(
            "invalid-arrow",
            "docs/images/generated/invalid-arrow.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Arrow,
                    Points = [
                        new ScreenshotPoint { X = 0.5, Y = 0.5 },
                        new ScreenshotPoint { X = x, Y = y }
                    ]
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("invalid arrow callout"));
    }

    [Test]
    public void Validate_AcceptsLabelCalloutsAtMinimumAndMaximumCoordinates() {
        ScreenshotAsset asset = Managed(
            "edge-labels",
            "docs/images/generated/edge-labels.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Label,
                    X = 0,
                    Y = 0,
                    Width = 0.5,
                    Text = "Minimum"
                },
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Label,
                    X = 0.5,
                    Y = 1,
                    Width = 0.5,
                    Text = "Maximum"
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase(-0.01, 0.5, 0.5)]
    [TestCase(0.5, -0.01, 0.5)]
    [TestCase(0.8, 0.5, 0.3)]
    [TestCase(0.5, 1.01, 0.5)]
    public void Validate_RejectsLabelCalloutsOutsideEveryEdge(double x, double y, double width) {
        ScreenshotAsset asset = Managed(
            "invalid-label",
            "docs/images/generated/invalid-label.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Label,
                    X = x,
                    Y = y,
                    Width = width,
                    Text = "Invalid"
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("invalid label callout"));
    }

    [Test]
    public void Validate_AcceptsBoxCalloutsAtMinimumAndMaximumCoordinates() {
        ScreenshotAsset asset = Managed(
            "edge-boxes",
            "docs/images/generated/edge-boxes.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Box,
                    X = 0,
                    Y = 0,
                    Width = 0.5,
                    Height = 0.5
                },
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Box,
                    X = 0.5,
                    Y = 0.5,
                    Width = 0.5,
                    Height = 0.5
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase(-0.01, 0.5, 0.25, 0.25)]
    [TestCase(0.5, -0.01, 0.25, 0.25)]
    [TestCase(0.8, 0.5, 0.3, 0.25)]
    [TestCase(0.5, 0.8, 0.25, 0.3)]
    [TestCase(0.5, 0.5, 0, 0.25)]
    [TestCase(0.5, 0.5, 0.25, 0)]
    public void Validate_RejectsBoxCalloutsOutsideEveryEdge(
            double x,
            double y,
            double width,
            double height) {
        ScreenshotAsset asset = Managed(
            "invalid-box",
            "docs/images/generated/invalid-box.png",
            callouts: [
                new ScreenshotCallout {
                    Kind = ScreenshotCalloutKind.Box,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                }
            ]);

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("invalid box callout"));
    }

    [Test]
    public void Validate_RequiresFixtureForManagedAssetsAndReasonForExcludedAssets() {
        ScreenshotCatalog missingFixture = Catalog(new ScreenshotAsset {
            Id = "managed",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/managed.png",
            Width = 100,
            Height = 100
        });
        ScreenshotCatalog missingReason = Catalog(new ScreenshotAsset {
            Id = "external",
            Classification = ScreenshotClassification.ExternalUi,
            Output = "docs/images/external.png"
        });

        Assert.That(() => CatalogValidator.Validate(missingFixture, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(missingReason, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
    }

    [Test]
    public void Validate_AcceptsProductionSettingsCropForARealViewFixture() {
        ScreenshotAsset asset = Managed("meridian-flip", "docs/images/generated/meridian-flip.png");
        asset = new ScreenshotAsset {
            Id = asset.Id,
            Classification = asset.Classification,
            Output = asset.Output,
            Fixture = asset.Fixture,
            State = "meridian-flip-settings",
            ViewType = "NINA.View.Options.ImagingView",
            Width = asset.Width,
            Height = asset.Height,
            CropTarget = "settings:meridian-flip"
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase("simple:set-options")]
    [TestCase("simple:target-tabs")]
    [TestCase("simple:target-general")]
    [TestCase("simple:target-information")]
    [TestCase("simple:target-options")]
    [TestCase("simple:autofocus")]
    [TestCase("simple:imaging-details")]
    [TestCase("simple:transform-button")]
    public void Validate_AcceptsProductionSimpleSequencerCrops(string cropTarget) {
        ScreenshotAsset asset = new() {
            Id = cropTarget.Replace(':', '-'),
            Classification = ScreenshotClassification.NinaUi,
            Output = $"docs/images/generated/sequencer/simple/{cropTarget.Replace(':', '-')}.png",
            Fixture = "view",
            State = "legacy-simple-documentation",
            ViewType = "NINA.View.SimpleSequencer.SimpleSequenceView",
            Width = 900,
            Height = 200,
            RenderWidth = 1920,
            RenderHeight = 1080,
            CropTarget = cropTarget
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [Test]
    public void Validate_AcceptsAllTargetAreaItemsCropForSequencerFixture() {
        ScreenshotAsset asset = new() {
            Id = "sequencer-flow",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/Sequencer_Flow.png",
            Fixture = "sequencer",
            State = "sequencer-flow",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1450,
            Height = 900,
            CropTarget = "target-area:all-items"
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase("start-area:all-items")]
    [TestCase("end-area:all-items")]
    public void Validate_AcceptsOuterAreaItemsCropForSequencerFixture(string cropTarget) {
        ScreenshotAsset asset = new() {
            Id = "sequencer-outer-area",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/simpletoadvanced/outer-area.png",
            Fixture = "sequencer",
            State = "simple-to-advanced-start-area",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 1458,
            Height = 260,
            CropTarget = cropTarget
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase("sidebar:filtered-item")]
    [TestCase("target-area:first-expression:exposure-time")]
    [TestCase("target-area:first-expression:gain")]
    public void Validate_AcceptsProductionSequencerDetailCrops(string cropTarget) {
        ScreenshotAsset asset = new() {
            Id = "sequencer-detail-crop",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/generated/sequencer/detail.png",
            Fixture = "sequencer",
            State = "sequencer-expression-example",
            ViewType = "NINA.View.Sequencer.AdvancedSequencer.AdvancedSequencerView",
            Width = 900,
            Height = 100,
            CropTarget = cropTarget
        };

        Assert.That(
            () => CatalogValidator.Validate(Catalog(asset), root, new FixtureRegistry()),
            Throws.Nothing);
    }

    [TestCase("docs-images-sequencer-instructions-camera-smartexposure-png", 1031, 60)]
    [TestCase("docs-images-sequencer-conditions-loopuntilaltitude2-png", 1170, 35)]
    [TestCase("docs-images-sequencer-conditions-loopwhilehorizon-png", 630, 35)]
    [TestCase("docs-images-sequencer-conditions-loopwhilehorizon2-png", 1150, 35)]
    [TestCase("docs-images-sequencer-instructions-telescope-slewaltaz-png", 1050, 35)]
    [TestCase("docs-images-sequencer-instructions-utility-waitforaltitude-png", 1220, 35)]
    [TestCase("docs-images-sequencer-instructions-utility-waituntilabovehorizon-png", 1170, 35)]
    public void Manifest_ProblemSequencerRowsAllocateSpaceForProductionFields(
            string id,
            int minimumWidth,
            int minimumHeight) {
        ScreenshotCatalog catalog = ScreenshotCatalog.Load(FindRepositoryFile("screenshots/manifest.json"));
        ScreenshotAsset asset = catalog.Assets.Single(candidate => candidate.Id == id);

        Assert.Multiple(() => {
            Assert.That(asset.Width, Is.GreaterThanOrEqualTo(minimumWidth));
            Assert.That(asset.Height, Is.GreaterThanOrEqualTo(minimumHeight));
        });
    }

    private static string FindRepositoryFile(string relativePath) {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null) {
            string candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) {
                return candidate;
            }
            directory = directory.Parent;
        }
        throw new AssertionException($"Could not find repository file '{relativePath}'.");
    }

    private static ScreenshotCatalog Catalog(params ScreenshotAsset[] assets) => new() { SchemaVersion = 1, Assets = [.. assets] };

    private static ScreenshotAsset Managed(
        string id,
        string output,
        int width = 640,
        int height = 480,
        List<ScreenshotCallout>? callouts = null) => new() {
            Id = id,
            Classification = ScreenshotClassification.NinaUi,
            Output = output,
            Fixture = "view",
            State = "NINA.View.Options.GeneralView",
            Width = width,
            Height = height,
            Callouts = callouts ?? []
        };
}
