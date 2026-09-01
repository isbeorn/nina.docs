#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

namespace NINA.DocumentationScreenshots;

public static class CatalogValidator {
    public static IReadOnlyDictionary<ScreenshotAsset, string> Validate(
        ScreenshotCatalog catalog,
        string documentationRoot,
        FixtureRegistry fixtures) {
        if (catalog.SchemaVersion is not 1 and not 2) {
            throw new CatalogException($"Unsupported screenshot catalog schema version {catalog.SchemaVersion}. Expected 1 or 2.");
        }

        string root = Path.GetFullPath(documentationRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        Dictionary<ScreenshotAsset, string> paths = [];
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> outputs = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> renderStates = new(StringComparer.OrdinalIgnoreCase);

        foreach (ScreenshotAsset asset in catalog.Assets) {
            if (string.IsNullOrWhiteSpace(asset.Id) || !ids.Add(asset.Id)) {
                throw new CatalogException($"Screenshot ID is empty or duplicated: '{asset.Id}'.");
            }
            if (string.IsNullOrWhiteSpace(asset.Output) || !outputs.Add(asset.Output)) {
                throw new CatalogException($"Screenshot output is empty or duplicated: '{asset.Output}'.");
            }

            string outputPath = Path.GetFullPath(Path.Combine(documentationRoot, asset.Output.Replace('/', Path.DirectorySeparatorChar)));
            if (!outputPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)) {
                throw new CatalogException($"Screenshot '{asset.Id}' writes outside the documentation repository: {asset.Output}");
            }
            paths.Add(asset, outputPath);

            bool managed = asset.Classification is ScreenshotClassification.NinaUi or ScreenshotClassification.NinaGeneratedVisual;
            if (managed) {
                if (!string.Equals(Path.GetExtension(asset.Output), ".png", StringComparison.OrdinalIgnoreCase)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' must use a PNG output path. Generated JPEG and other image formats are not supported.");
                }
                if (asset.Width is < 16 or > 8192 || asset.Height is < 16 or > 8192) {
                    throw new CatalogException($"Screenshot '{asset.Id}' has invalid dimensions {asset.Width}x{asset.Height}.");
                }
                bool hasRenderWidth = asset.RenderWidth.HasValue;
                bool hasRenderHeight = asset.RenderHeight.HasValue;
                if (hasRenderWidth != hasRenderHeight) {
                    throw new CatalogException($"Screenshot '{asset.Id}' must define both renderWidth and renderHeight.");
                }
                if ((asset.RenderWidth ?? asset.Width) is < 16 or > 8192 || (asset.RenderHeight ?? asset.Height) is < 16 or > 8192) {
                    throw new CatalogException($"Screenshot '{asset.Id}' has invalid render dimensions {asset.RenderWidth}x{asset.RenderHeight}.");
                }
                if (hasRenderWidth && asset.Crop is null && string.IsNullOrWhiteSpace(asset.CropTarget)
                    && (asset.RenderWidth != asset.Width || asset.RenderHeight != asset.Height)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' defines separate render dimensions without a crop.");
                }
                if (string.IsNullOrWhiteSpace(asset.Fixture) || !fixtures.Contains(asset.Fixture)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' refers to unknown fixture '{asset.Fixture}'.");
                }
                if (string.Equals(asset.Fixture, "sequencer-entity", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(asset.Icon)) {
                    throw new CatalogException(
                        $"Screenshot '{asset.Id}' must obtain its sequencer icon from production metadata on the selected type, not a catalog icon guess.");
                }
                if (catalog.SchemaVersion >= 2 && string.IsNullOrWhiteSpace(asset.State)) {
                    throw new CatalogException($"Screenshot '{asset.Id}' must define a named fixture state.");
                }
                string renderState = GetRenderState(asset);
                if (renderStates.TryGetValue(renderState, out string? existingId)) {
                    throw new CatalogException($"Screenshots '{existingId}' and '{asset.Id}' have an identical render state.");
                }
                renderStates.Add(renderState, asset.Id);
            } else if (string.IsNullOrWhiteSpace(asset.ExclusionReason)) {
                throw new CatalogException($"Excluded screenshot '{asset.Id}' must explain why it is not generated.");
            }

            ValidateCrop(asset);
            ValidateCropTarget(asset);
            ValidateCallouts(asset);
        }
        return paths;
    }

    private static void ValidateCrop(ScreenshotAsset asset) {
        ScreenshotCrop? crop = asset.Crop;
        if (crop is null) {
            return;
        }
        if (!InUnitRange(crop.X) || !InUnitRange(crop.Y) || crop.Width <= 0 || crop.Height <= 0 ||
            crop.X + crop.Width > 1 || crop.Y + crop.Height > 1) {
            throw new CatalogException($"Screenshot '{asset.Id}' has a crop outside normalized image bounds.");
        }
    }

    private static void ValidateCropTarget(ScreenshotAsset asset) {
        if (string.IsNullOrWhiteSpace(asset.CropTarget)) {
            return;
        }
        if (asset.Crop is not null) {
            throw new CatalogException($"Screenshot '{asset.Id}' cannot define both a relative crop and crop target.");
        }
        if (asset.CropTarget is not "target-area:first-item"
            and not "target-area:first-item-instructions"
            and not "root-add-menu"
            and not "settings:meridian-flip"
            and not "framing:image-source"
            and not "framing:coordinates"
            and not "framing:mosaic-plan") {
            throw new CatalogException($"Screenshot '{asset.Id}' has unknown crop target '{asset.CropTarget}'.");
        }
        bool sequencerCrop = asset.CropTarget is "target-area:first-item"
            or "target-area:first-item-instructions"
            or "root-add-menu";
        if (sequencerCrop && !string.Equals(asset.Fixture, "sequencer", StringComparison.OrdinalIgnoreCase)) {
            throw new CatalogException($"Screenshot '{asset.Id}' uses a sequencer crop target with fixture '{asset.Fixture}'.");
        }
        if (asset.CropTarget == "settings:meridian-flip"
            && !string.Equals(asset.Fixture, "view", StringComparison.OrdinalIgnoreCase)) {
            throw new CatalogException($"Screenshot '{asset.Id}' uses a settings crop target with fixture '{asset.Fixture}'.");
        }
        if (asset.CropTarget.StartsWith("framing:", StringComparison.Ordinal)
            && (!string.Equals(asset.Fixture, "framing-assistant", StringComparison.OrdinalIgnoreCase)
                || asset.ViewType != "NINA.View.FramingAssistantView")) {
            throw new CatalogException($"Screenshot '{asset.Id}' uses a Framing Assistant crop target with fixture '{asset.Fixture}'.");
        }
    }

    private static void ValidateCallouts(ScreenshotAsset asset) {
        foreach (ScreenshotCallout callout in asset.Callouts) {
            if (!InUnitRange(callout.X) || !InUnitRange(callout.Y) || string.IsNullOrWhiteSpace(callout.Text)) {
                throw new CatalogException($"Screenshot '{asset.Id}' has an invalid callout.");
            }
        }
    }

    private static bool InUnitRange(double value) => double.IsFinite(value) && value is >= 0 and <= 1;

    private static string GetRenderState(ScreenshotAsset asset) {
        string crop = asset.Crop is null ? asset.CropTarget ?? string.Empty : $"{asset.Crop.X:R},{asset.Crop.Y:R},{asset.Crop.Width:R},{asset.Crop.Height:R}";
        string callouts = string.Join(";", asset.Callouts.Select(callout => $"{callout.X:R},{callout.Y:R},{callout.Text}"));
        return string.Join("|", asset.Fixture, asset.ViewType, asset.State, asset.Width, asset.Height, asset.RenderWidth, asset.RenderHeight, crop, callouts);
    }
}
