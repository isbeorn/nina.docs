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

namespace NINA.DocumentationScreenshots;

public static class ImageQualityValidator {
    public static void ValidateVisualParity(string screenshotId, string baselinePath, string generatedPath) {
        ImageMetrics baseline = Measure(baselinePath);
        ImageMetrics generated = Measure(generatedPath);

        // A deliberately conservative guardrail. It catches placeholders, blank views and
        // other severe regressions without assuming that two versions of a real NINA view
        // must have identical colours or pixel geometry.
        if (baseline.DetailScore >= 8 &&
            generated.DetailScore < baseline.DetailScore * 0.45 &&
            generated.EdgeDensity < baseline.EdgeDensity * 0.40) {
            throw new CatalogException(
                $"Screenshot '{screenshotId}' has materially less visual detail than the checked-in image " +
                $"(detail {generated.DetailScore:F1} versus {baseline.DetailScore:F1}, " +
                $"edge density {generated.EdgeDensity:P1} versus {baseline.EdgeDensity:P1}).");
        }
    }

    private static ImageMetrics Measure(string path) {
        BitmapFrame frame;
        using (FileStream input = File.OpenRead(path)) {
            PngBitmapDecoder decoder = new(input, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            frame = decoder.Frames[0];
        }

        FormatConvertedBitmap bitmap = new(frame, PixelFormats.Bgra32, null, 0);
        int stride = bitmap.PixelWidth * 4;
        byte[] pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);

        const int sampleWidth = 128;
        const int sampleHeight = 96;
        double[] luminance = new double[sampleWidth * sampleHeight];
        double sum = 0;
        for (int y = 0; y < sampleHeight; y++) {
            int sourceY = Math.Min(bitmap.PixelHeight - 1, y * bitmap.PixelHeight / sampleHeight);
            for (int x = 0; x < sampleWidth; x++) {
                int sourceX = Math.Min(bitmap.PixelWidth - 1, x * bitmap.PixelWidth / sampleWidth);
                int offset = sourceY * stride + sourceX * 4;
                double alpha = pixels[offset + 3] / 255d;
                double value = alpha * (
                    0.0722 * pixels[offset] +
                    0.7152 * pixels[offset + 1] +
                    0.2126 * pixels[offset + 2]);
                luminance[y * sampleWidth + x] = value;
                sum += value;
            }
        }

        double mean = sum / luminance.Length;
        double variance = luminance.Sum(value => Math.Pow(value - mean, 2)) / luminance.Length;
        double edgeMagnitude = 0;
        int edgeCount = 0;
        int comparisons = 0;
        for (int y = 0; y < sampleHeight; y++) {
            for (int x = 0; x < sampleWidth; x++) {
                int index = y * sampleWidth + x;
                if (x + 1 < sampleWidth) {
                    AccumulateEdge(Math.Abs(luminance[index] - luminance[index + 1]));
                }
                if (y + 1 < sampleHeight) {
                    AccumulateEdge(Math.Abs(luminance[index] - luminance[index + sampleWidth]));
                }
            }
        }

        double edgeDensity = comparisons == 0 ? 0 : (double)edgeCount / comparisons;
        double meanEdgeMagnitude = comparisons == 0 ? 0 : edgeMagnitude / comparisons;
        return new ImageMetrics(Math.Sqrt(variance) + meanEdgeMagnitude, edgeDensity);

        void AccumulateEdge(double magnitude) {
            comparisons++;
            edgeMagnitude += magnitude;
            if (magnitude >= 18) {
                edgeCount++;
            }
        }
    }

    private sealed record ImageMetrics(double DetailScore, double EdgeDensity);
}
