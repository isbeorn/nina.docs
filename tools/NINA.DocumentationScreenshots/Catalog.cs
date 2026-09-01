#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Text.Json;
using System.Text.Json.Serialization;

namespace NINA.DocumentationScreenshots;

public sealed class ScreenshotCatalog {
    public int SchemaVersion { get; init; }
    public List<ScreenshotAsset> Assets { get; init; } = [];

    public static ScreenshotCatalog Load(string path) {
        JsonSerializerOptions options = new() {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        ScreenshotCatalog? catalog = JsonSerializer.Deserialize<ScreenshotCatalog>(File.ReadAllText(path), options);
        return catalog ?? throw new CatalogException($"The screenshot catalog is empty: {path}");
    }
}

public sealed class ScreenshotAsset {
    public required string Id { get; init; }
    public required ScreenshotClassification Classification { get; init; }
    public required string Output { get; init; }
    public string? Fixture { get; init; }
    public string? ViewType { get; init; }
    public string? State { get; init; }
    public string? SourceIdentifier { get; init; }
    public string? DisplayName { get; init; }
    public string? Icon { get; init; }
    public string? ExclusionReason { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int? RenderWidth { get; init; }
    public int? RenderHeight { get; init; }
    public string? CropTarget { get; init; }
    public ScreenshotCrop? Crop { get; init; }
    public List<ScreenshotCallout> Callouts { get; init; } = [];
}

public sealed class ScreenshotCrop {
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; } = 1;
    public double Height { get; init; } = 1;
}

public sealed class ScreenshotCallout {
    public ScreenshotCalloutKind Kind { get; init; } = ScreenshotCalloutKind.Badge;
    public double X { get; init; }
    public double Y { get; init; }
    public double? Width { get; init; }
    public string? Text { get; init; }
    public List<ScreenshotPoint> Points { get; init; } = [];
}

public sealed class ScreenshotPoint {
    public double X { get; init; }
    public double Y { get; init; }
}

public enum ScreenshotCalloutKind {
    Badge,
    Label,
    Arrow
}

public enum ScreenshotClassification {
    NinaUi,
    NinaGeneratedVisual,
    ExternalUi,
    BrandOrStatic
}

public sealed class CatalogException(string message) : Exception(message);
