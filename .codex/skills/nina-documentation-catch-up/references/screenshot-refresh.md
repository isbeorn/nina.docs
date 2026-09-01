# Screenshot Refresh

Use this reference from the documentation repository when cataloging images, adding fixtures, regenerating NINA captures or removing obsolete image assets.

## Automation Boundary

The versioned catalog is `screenshots/manifest.json`. Every source raster image below `docs` must have one stable catalog entry and one classification:

- `nina-ui`: a NINA application view that a verified fixture can render.
- `nina-generated-visual`: a NINA chart, curve or generated visual with deterministic input data.
- `external-ui`: Windows, PHD2, Visual Studio, Crowdin, browser or vendor software that the NINA renderer must not imitate.
- `brand-or-static`: logos, icons, diagrams, annotations and selective states that should remain static.

An external, static or selectively excluded image must have a concise reason. Do not reinterpret it from its filename. Automate a selective state only after a real fixture reproduces it faithfully.

Managed output paths must be below `docs/images/generated` and end in `.png`. Preserve the documentation-area hierarchy and stable catalog ID when moving or replacing a managed output. Keep external and static assets in their existing source-image directories. When replacing a generated JPEG, choose the corresponding PNG path and update all documentation references. Never make the renderer write JPEGs.

## Real-View Fixture Rules

The non-shipping renderer lives in `tools/NINA.DocumentationScreenshots` in the documentation repository. The generation script passes its `-NinaSource` solution directory to the project through the `NinaSource` MSBuild property. Its fixtures must:

- Instantiate compiled production XAML on an STA thread with production resource dictionaries and DataTemplates.
- Use the production Slate schema, icons, export metadata, names and layout. For sequencer entities, select an exact `sourceIdentifier`; never infer a type or icon from the output filename.
- Use production `SequenceBlockView` or the actual entity DataTemplate when a sequencer trigger or instruction has no dedicated view.
- Provide deterministic in-memory profiles, simulator equipment, astronomical data, timestamps and sample images.
- Avoid real devices, user profiles and network access.
- Use the US English (`en-US`) locale, fixed dimensions, 96 DPI, software rendering and disabled animations.
- Represent crops, expanded panels, validation states and before or after containers as named fixture states.
- Express callouts with normalized coordinates rather than editing PNG pixels manually.

Keep screenshot-only setup out of shipping projects and APIs. A fixture may adapt constructors and inert services inside the developer tool, but the rendered control must remain the real production view.

## Visual Quality Rules

Generated output must be at least as useful as the image it replaces. Review more than the absence of a blank bitmap:

- Confirm the production icon, display name, enabled state and validation state.
- Confirm controls are not clipped and no required row or panel is missing.
- Use a common height for comparable sequencer instruction, condition and trigger rows. Increase height only when the real control needs another row or an expanded container.
- Confirm the Slate background and foreground colors, not just the accent color.
- Ensure synthetic full-workspace fixtures include all documentation-relevant areas.
- Render popup menus without transient hover or keyboard-selection state.
- Preserve external and static images instead of rebuilding approximations.

Use local image inspection and image diffs for every changed fixture class. For a large refresh, review representative simple controls, multi-row controls, expanded workflows, menus, full workspaces, crops and generated visuals before accepting the full set.

## Commands

From the documentation root:

```powershell
# Preview everything without replacement
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Preview

# Preview one area or one stable ID
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Area sequencer -Preview
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Id <catalog-id> -Preview

# Transactionally refresh an accepted area
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Area sequencer
```

Use `-Restore` only for the first run or when dependencies changed. A non-preview invocation must render and validate the complete requested set in a temporary directory before replacing checked-in files. A failure must leave existing assets untouched.

After adding or changing a fixture, run:

```powershell
dotnet test tools\NINA.DocumentationScreenshots.Tests\NINA.DocumentationScreenshots.Tests.csproj --configuration Debug --no-restore --verbosity minimal -p:NinaSource=C:\path\to\nina
```

The suite must cover catalog validation, PNG format and dimensions, 96 DPI, nonblank output, deterministic repeats, real runtime view construction, binding failures, dispatcher timeouts, crops and callout boundaries and transactional failure behavior. Reproduce renderer bugs with a failing test before changing implementation.

## Acceptance Checks

1. Validate catalog schema, unique IDs, unique outputs, valid dimensions, allowed roots and existing outputs.
2. Confirm every source image below `docs` is cataloged exactly once.
3. Confirm every catalog output exists and every managed output is PNG.
4. Confirm every source image is referenced after resolving relative Markdown paths and MkDocs logo or favicon paths.
5. Render the affected ID or area twice. The second preview must report no differences.
6. Run a complete preview and require zero failures. Investigate any changed output rather than accepting bulk replacement blindly.
7. Visually review all newly added fixtures and representative refreshed fixtures at original resolution.
8. Build standard and PDF documentation and run link checks.
9. Run `git diff --check` in both repositories and inspect image deletions and additions explicitly.

Keep generation on demand. Do not wire these commands into CI, schedules or bots.
