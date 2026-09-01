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
using System.Diagnostics;
using System.Reflection;

namespace NINA.DocumentationScreenshots;

public static class Program {
    [STAThread]
    public static int Main(string[] args) {
        try {
            Options options = Options.Parse(args);
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
            if (options.WorkerOutput is not null) {
                if (selected.Count != 1) {
                    throw new CatalogException("An isolated screenshot worker requires exactly one --id selection.");
                }
                WpfBootstrap.Initialize();
                new ScreenshotRenderer(fixtures).Render(selected[0], options.WorkerOutput);
                return 0;
            }

            string stagingRoot = Path.Combine(Path.GetTempPath(), $"nina-doc-screenshots-{Guid.NewGuid():N}");
            Directory.CreateDirectory(stagingRoot);
            try {
                Dictionary<ScreenshotAsset, string> stagedPaths = [];
                if (UseIsolatedWorkers(selected.Count)) {
                    foreach (ScreenshotAsset asset in selected) {
                        string stagedPath = Path.Combine(stagingRoot, asset.Id + ".png");
                        Console.WriteLine($"Rendering {asset.Id}...");
                        RunIsolatedWorker(options, asset, stagedPath);
                        stagedPaths.Add(asset, stagedPath);
                    }
                } else {
                    WpfBootstrap.Initialize();
                    ScreenshotRenderer renderer = new(fixtures);
                    foreach (ScreenshotAsset asset in selected) {
                        string stagedPath = Path.Combine(stagingRoot, asset.Id + ".png");
                        Console.WriteLine($"Rendering {asset.Id}...");
                        renderer.Render(asset, stagedPath);
                        stagedPaths.Add(asset, stagedPath);
                    }
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

    private static bool UseIsolatedWorkers(int selectedCount) =>
        selectedCount > 1
        && string.Equals(
            Assembly.GetEntryAssembly()?.GetName().Name,
            typeof(Program).Assembly.GetName().Name,
            StringComparison.Ordinal);

    private static void RunIsolatedWorker(
            Options options,
            ScreenshotAsset asset,
            string outputPath) {
        string executable = Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe");
        if (!File.Exists(executable)) {
            throw new CatalogException($"The isolated screenshot renderer executable was not found: {executable}");
        }
        ProcessStartInfo startInfo = new(executable) {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("--catalog");
        startInfo.ArgumentList.Add(options.Catalog);
        startInfo.ArgumentList.Add("--docs-root");
        startInfo.ArgumentList.Add(options.DocumentationRoot);
        startInfo.ArgumentList.Add("--id");
        startInfo.ArgumentList.Add(asset.Id);
        startInfo.ArgumentList.Add("--worker-output");
        startInfo.ArgumentList.Add(outputPath);

        using Process process = Process.Start(startInfo)
            ?? throw new CatalogException($"Screenshot '{asset.Id}' could not start its isolated renderer process.");
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit((int)TimeSpan.FromMinutes(2).TotalMilliseconds)) {
            process.Kill(entireProcessTree: true);
            throw new CatalogException($"Screenshot '{asset.Id}' exceeded the two-minute isolated renderer timeout.");
        }
        Task.WaitAll(standardOutput, standardError);
        if (process.ExitCode != 0) {
            string detail = standardError.Result.Trim();
            if (string.IsNullOrWhiteSpace(detail)) {
                detail = standardOutput.Result.Trim();
            }
            throw new CatalogException($"Screenshot '{asset.Id}' failed in its isolated renderer: {detail}");
        }
    }

    private static bool IsInArea(ScreenshotAsset asset, string area) {
        string normalized = asset.Output.Replace('\\', '/');
        const string generatedPrefix = "docs/images/generated/";
        if (!normalized.StartsWith(generatedPrefix, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        int separator = normalized.IndexOf('/', generatedPrefix.Length);
        string assetArea = separator < 0
            ? normalized[generatedPrefix.Length..]
            : normalized[generatedPrefix.Length..separator];
        return string.Equals(assetArea, area, StringComparison.OrdinalIgnoreCase);
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
        public string? WorkerOutput { get; init; }
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
                WorkerOutput = values.GetValueOrDefault("--worker-output") is string workerOutput
                    ? Path.GetFullPath(workerOutput)
                    : null,
                Preview = values.ContainsKey("--preview")
            };
        }
    }
}
