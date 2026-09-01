# NINA.DocumentationScreenshots

This non-shipping Windows WPF tool renders documentation images from compiled N.I.N.A. views. It lives in the documentation repository, is invoked by `scripts/generate-screenshots.ps1` and is intentionally excluded from the N.I.N.A. solution and release artifacts.

The renderer loads production resource dictionaries on an STA thread, creates an isolated in-memory profile and renders a versioned screenshot catalog with fixed dimensions at 96 DPI. It does not load the user's profile, connect equipment or make network requests.

Fixtures live in `FixtureRegistry.cs`. Use production views and deterministic view models or data. Do not add screenshot-specific APIs to shipping projects.

The command stages every requested image before copying any output. An unknown fixture or view, invalid path or dimension, binding failure, dispatcher timeout or blank capture fails the operation without replacing existing documentation images.

The project resolves production code through the `NinaSource` MSBuild property. It must point to the N.I.N.A. solution directory that contains `NINA.sln`. The screenshot script supplies this property from its `-NinaSource` parameter.

Run the focused test suite from the documentation root:

```powershell
dotnet test tools\NINA.DocumentationScreenshots.Tests\NINA.DocumentationScreenshots.Tests.csproj -c Debug -m:1 -p:NinaSource=C:\path\to\nina
```
