#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Windows.Media;
using System.Windows.Media.Imaging;
using NUnit.Framework;

namespace NINA.DocumentationScreenshots.Tests;

[TestFixture]
public class ImageQualityValidatorTests {
    private string root = null!;

    [SetUp]
    public void SetUp() {
        root = Path.Combine(Path.GetTempPath(), $"nina-screenshot-quality-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
    }

    [TearDown]
    public void TearDown() {
        Directory.Delete(root, recursive: true);
    }

    [Test]
    public void ValidateVisualParity_RejectsMaterialLossOfVisualDetail() {
        string baseline = WriteImage("baseline.png", (x, y) => (x / 8 + y / 8) % 2 == 0 ? Colors.White : Colors.Black);
        string generated = WriteImage("generated.png", (_, _) => Color.FromRgb(35, 35, 38));

        Assert.That(
            () => ImageQualityValidator.ValidateVisualParity("degraded", baseline, generated),
            Throws.TypeOf<CatalogException>().With.Message.Contains("visual detail"));
    }

    [Test]
    public void ValidateVisualParity_AcceptsEquivalentVisualDetail() {
        string baseline = WriteImage("baseline.png", (x, y) => (x / 8 + y / 8) % 2 == 0 ? Colors.White : Colors.Black);
        string generated = WriteImage("generated.png", (x, y) => (x / 8 + y / 8) % 2 == 0 ? Colors.Cyan : Colors.Black);

        Assert.That(() => ImageQualityValidator.ValidateVisualParity("equivalent", baseline, generated), Throws.Nothing);
    }

    private string WriteImage(string name, Func<int, int, Color> pixel) {
        const int width = 128;
        const int height = 96;
        byte[] pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Color color = pixel(x, y);
                int offset = (y * width + x) * 4;
                pixels[offset] = color.B;
                pixels[offset + 1] = color.G;
                pixels[offset + 2] = color.R;
                pixels[offset + 3] = color.A;
            }
        }

        BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        string path = Path.Combine(root, name);
        using FileStream output = File.Create(path);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(output);
        return path;
    }
}
