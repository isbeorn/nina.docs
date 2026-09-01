# Documentation Audit

Use this reference from the documentation repository for source-to-documentation comparisons, missing feature discovery, troubleshooting research and link cleanup.

## Establish the Comparison

- Resolve the exact NINA solution root and documentation root. They may be a solution submodule checkout or separate local checkouts.
- Record `git rev-parse HEAD`, branch status and dirty state for both repositories before changing files.
- Treat the user's selected revisions as authoritative. If the request names `develop`, verify the checkout but do not fetch, pull or switch without authorization.
- Preserve unrelated working-tree changes and do not reset either repository.

## Generate and Interpret the Inventory

From the documentation root, run:

```powershell
.\scripts\report-doc-coverage.ps1 -NinaSource C:\path\to\nina -OutputPath .artifacts\doc-coverage.md
```

The report inventories sequencer instructions, conditions and triggers, profile settings, documentable views and image formats. It is heuristic. A name match does not prove that a behavior is explained correctly and a missing name match does not prove that documentation is absent.

Use `-MappingPath` when maintainers have a reviewed identifier-to-page map. Include source identifiers, display names, categories and implementation locations in durable audit artifacts when the script can derive them reliably.

For each item in scope:

1. Read the production implementation, related interface and production XAML or DataTemplate.
2. Check tests for boundary behavior, supported modes, validation rules and recovery paths.
3. Compare the current docs, including neighboring pages that describe the same behavior.
4. Classify the documentation as current, stale, incomplete or missing.
5. Update workflow-level documentation instead of mechanically creating a heading for every internal setting.

Audit paired behavior symmetrically, such as connect and disconnect, park and unpark, safe and unsafe, start and stop or before and after actions. When one omission is found, inspect its neighboring source category rather than patching only the named example.

## Recurring Questions and External Research

When research is in scope, search current official NINA documentation, NINA issue and discussion sources and active astronomy support forums for repeated beginner or operator questions. Use this only as a discovery input. Verify product behavior in the current checkout and prefer source-backed wording in the documentation.

Good troubleshooting entries provide observable symptoms, the relevant setting or dependency, a short isolation sequence and the point where logs or upstream-driver support are needed. Do not present a forum workaround as a product guarantee.

## Writing and Link Rules

- Preserve existing page paths and anchors where practical.
- Use protocol-relative destinations such as `//example.org/path` for authored external Markdown hyperlinks, matching this documentation repository's convention.
- Keep an explicit scheme where it is part of a command, clone URL, API example or MkDocs configuration that rejects protocol-relative values.
- Repair malformed forms such as `//https://...`, empty destinations and obsolete filenames.
- Keep descriptions operator-focused and precise about prerequisites, validation and failure behavior.

## Images and Orphans

Do not inspect generated `site/` copies as source assets. After Markdown changes, enumerate source images below `docs`, resolve every Markdown image destination and include MkDocs logo or favicon references. Remove source images with no remaining reference only after verifying exact resolved paths and catalog entries. Read [screenshot-refresh.md](screenshot-refresh.md) before changing generated NINA captures or the screenshot catalog.

## Documentation Verification

Run after the final edit:

```powershell
mkdocs build --strict --clean -f mkdocs-nopdf.yml

$env:ENABLE_PDF_EXPORT = '1'
try {
    mkdocs build --strict --clean -f mkdocs.yml
} finally {
    Remove-Item Env:ENABLE_PDF_EXPORT -ErrorAction SilentlyContinue
}
```

Check generated local `href` and `src` targets, then audit authored Markdown for accidental absolute link destinations, malformed protocol-relative links and empty destinations. Run `git diff --check` after the exact final edit. Keep the dated catch-up report under `audit/` accurate if counts or automation boundaries change.
