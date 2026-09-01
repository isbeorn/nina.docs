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
                Managed("minimum", "docs/images/minimum.png", 16, 16, [new ScreenshotCallout { X = 0, Y = 0, Text = "1" }]),
                Managed("maximum", "docs/images/maximum.png", 8192, 8192, [new ScreenshotCallout { X = 1, Y = 1, Text = "2" }])
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
        ScreenshotCatalog catalog = Catalog(Managed("invalid", "docs/images/invalid.png", width, height));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("invalid dimensions"));
    }

    [Test]
    public void Validate_RejectsDuplicateIdsAndOutputs() {
        ScreenshotCatalog duplicateIds = Catalog(
            Managed("same", "docs/images/first.png"),
            Managed("same", "docs/images/second.png"));
        ScreenshotCatalog duplicateOutputs = Catalog(
            Managed("first", "docs/images/same.png"),
            Managed("second", "docs/images/same.png"));

        Assert.That(() => CatalogValidator.Validate(duplicateIds, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(duplicateOutputs, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
    }

    [TestCase("docs/images/generated.jpg")]
    [TestCase("docs/images/generated.JPEG")]
    public void Validate_RejectsManagedOutputsThatAreNotPng(string output) {
        ScreenshotCatalog catalog = Catalog(Managed("generated", output));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("must use a PNG output path"));
    }

    [Test]
    public void Validate_RejectsCatalogGuessesForProductionSequencerIcons() {
        ScreenshotAsset asset = new() {
            Id = "cool-camera",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/cool-camera.png",
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
            Managed("open-dome", "docs/images/open-dome.png", 200, 35),
            Managed("park-scope", "docs/images/park-scope.png", 200, 35));

        Assert.That(
            () => CatalogValidator.Validate(catalog, root, new FixtureRegistry()),
            Throws.TypeOf<CatalogException>().With.Message.Contains("identical render state"));
    }

    [Test]
    public void Validate_RejectsPathTraversalAndCalloutsOutsideEveryEdge() {
        ScreenshotCatalog traversal = Catalog(Managed("escape", "../escape.png"));
        ScreenshotCatalog left = Catalog(Managed("left", "docs/images/left.png", callouts: [new ScreenshotCallout { X = -0.01, Y = 0.5, Text = "1" }]));
        ScreenshotCatalog top = Catalog(Managed("top", "docs/images/top.png", callouts: [new ScreenshotCallout { X = 0.5, Y = -0.01, Text = "1" }]));
        ScreenshotCatalog right = Catalog(Managed("right", "docs/images/right.png", callouts: [new ScreenshotCallout { X = 1.01, Y = 0.5, Text = "1" }]));
        ScreenshotCatalog bottom = Catalog(Managed("bottom", "docs/images/bottom.png", callouts: [new ScreenshotCallout { X = 0.5, Y = 1.01, Text = "1" }]));

        Assert.That(() => CatalogValidator.Validate(traversal, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(left, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(top, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(right, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
        Assert.That(() => CatalogValidator.Validate(bottom, root, new FixtureRegistry()), Throws.TypeOf<CatalogException>());
    }

    [Test]
    public void Validate_RequiresFixtureForManagedAssetsAndReasonForExcludedAssets() {
        ScreenshotCatalog missingFixture = Catalog(new ScreenshotAsset {
            Id = "managed",
            Classification = ScreenshotClassification.NinaUi,
            Output = "docs/images/managed.png",
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
        ScreenshotAsset asset = Managed("meridian-flip", "docs/images/meridian-flip.png");
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
