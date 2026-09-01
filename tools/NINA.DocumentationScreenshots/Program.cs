#region "copyright"

/*
    Copyright (c) 2016 - 2026 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Security.Cryptography;

namespace NINA.DocumentationScreenshots;

public static class Program {
    [STAThread]
    public static int Main(string[] args) {
        try {
            Options options = Options.Parse(args);
            WpfBootstrap.Initialize();
            FixtureRegistry fixtures = new();
            ScreenshotCatalog catalog = ScreenshotCatalog.Load(options.Catalog);
            IReadOnlyDictionary<ScreenshotAsset, string> paths = CatalogValidator.Validate(catalog, options.DocumentationRoot, fixtures);

            List<ScreenshotAsset> selected = catalog.Assets
                .Where(asset => asset.Classification is ScreenshotClassification.NinaUi or ScreenshotClassification.NinaGeneratedVisual)
                .Where(asset => options.Id is null || string.Equals(asset.Id, options.Id, StringComparison.OrdinalIgnoreCase))
                .Where(asset => options.Area is null || IsInArea(asset, options.Area))
                .ToList();
            if (selected.Count == 0) {
                throw new CatalogException("No managed screenshots matched the requested filters.");
            }

            string stagingRoot = Path.Combine(Path.GetTempPath(), $"nina-doc-screenshots-{Guid.NewGuid():N}");
            Directory.CreateDirectory(stagingRoot);
            try {
                ScreenshotRenderer renderer = new(fixtures);
                Dictionary<ScreenshotAsset, string> stagedPaths = [];
                foreach (ScreenshotAsset asset in selected) {
                    string stagedPath = Path.Combine(stagingRoot, asset.Id + ".png");
                    Console.WriteLine($"Rendering {asset.Id}...");
                    renderer.Render(asset, stagedPath);
                    stagedPaths.Add(asset, stagedPath);
                }

                int added = 0;
                int changed = 0;
                int unchanged = 0;
                List<string> qualityFailures = [];
                List<(string Staged, string Target)> replacements = [];
                foreach (ScreenshotAsset asset in selected) {
                    string target = paths[asset];
                    string staged = stagedPaths[asset];
                    bool exists = File.Exists(target);
                    bool equal = exists && FilesEqual(target, staged);
                    if (exists && !equal) {
                        try {
                            ImageQualityValidator.ValidateVisualParity(asset.Id, target, staged);
                        } catch (CatalogException ex) {
                            qualityFailures.Add(ex.Message);
                        }
                    }
                    if (equal) {
                        unchanged++;
                        continue;
                    }
                    if (exists) {
                        changed++;
                        Console.WriteLine($"Changed: {asset.Id} -> {asset.Output.Replace('\\', '/')}");
                    } else {
                        added++;
                        Console.WriteLine($"Added: {asset.Id} -> {asset.Output.Replace('\\', '/')}");
                    }
                    replacements.Add((staged, target));
                }

                if (qualityFailures.Count > 0) {
                    throw new CatalogException(
                        $"Visual parity failed for {qualityFailures.Count} screenshot(s):{Environment.NewLine}" +
                        string.Join(Environment.NewLine, qualityFailures.Select(failure => $"- {failure}")));
                }

                if (!options.Preview) {
                    foreach ((string staged, string target) in replacements) {
                        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                        File.Copy(staged, target, overwrite: true);
                    }
                }

                Console.WriteLine($"Summary: added={added}, changed={changed}, unchanged={unchanged}, failed=0, preview={options.Preview.ToString().ToLowerInvariant()}");
                return 0;
            } finally {
                Directory.Delete(stagingRoot, recursive: true);
            }
        } catch (Exception ex) {
            Console.Error.WriteLine($"Screenshot generation failed: {ex.GetBaseException().Message}");
            return 1;
        }
    }

    private static bool IsInArea(ScreenshotAsset asset, string area) {
        string normalized = asset.Output.Replace('\\', '/');
        return normalized.Contains($"/images/{area}/", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith($"docs/images/{area}/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool FilesEqual(string left, string right) {
        using SHA256 sha = SHA256.Create();
        using FileStream leftStream = File.OpenRead(left);
        using FileStream rightStream = File.OpenRead(right);
        return sha.ComputeHash(leftStream).SequenceEqual(sha.ComputeHash(rightStream));
    }

    private sealed class Options {
        public required string Catalog { get; init; }
        public required string DocumentationRoot { get; init; }
        public string? Id { get; init; }
        public string? Area { get; init; }
        public bool Preview { get; init; }

        public static Options Parse(string[] args) {
            Dictionary<string, string?> values = new(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < args.Length; index++) {
                string argument = args[index];
                if (argument == "--preview") {
                    values[argument] = "true";
                    continue;
                }
                if (!argument.StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length) {
                    throw new CatalogException($"Invalid command-line argument: {argument}");
                }
                values[argument] = args[++index];
            }

            if (!values.TryGetValue("--catalog", out string? catalog) || string.IsNullOrWhiteSpace(catalog) ||
                !values.TryGetValue("--docs-root", out string? docsRoot) || string.IsNullOrWhiteSpace(docsRoot)) {
                throw new CatalogException("Required arguments: --catalog <path> --docs-root <path> [--id <id>] [--area <area>] [--preview]");
            }

            return new Options {
                Catalog = Path.GetFullPath(catalog),
                DocumentationRoot = Path.GetFullPath(docsRoot),
                Id = values.GetValueOrDefault("--id"),
                Area = values.GetValueOrDefault("--area"),
                Preview = values.ContainsKey("--preview")
            };
        }
    }
}
