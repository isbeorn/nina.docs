---
name: nina-documentation-catch-up
description: Audit N.I.N.A. documentation against the current NINA source and refresh maintainable application screenshots. Use for documentation catch-up work, source-to-doc accuracy reviews, undocumented feature discovery, screenshot catalog maintenance or real-view screenshot regeneration in the NINA repositories. Do not use for ordinary product code changes that have only a small documentation note.
---

# NINA Documentation Catch-Up

Treat the selected NINA source checkout as the implementation authority and the selected documentation checkout as the update target. This is an explicitly invoked maintenance workflow, not a freshness guarantee.

## Route the Work

1. Locate the solution root through `NINA.sln` and the documentation root through `mkdocs.yml` plus `docs/`.
2. Read the applicable `AGENTS.md`, the NINA source checkout's `.codex/skills/nina-repository/SKILL.md` and the documentation `CONTRIBUTING.md` before editing.
3. Record the source and documentation revisions being compared. Do not switch branches, pull or update a submodule unless the user asked for it.
4. Convert the request into an explicit checklist before editing and recheck it before reporting completion.

For documentation accuracy, inventory work, troubleshooting research or link repair, read [references/documentation-audit.md](references/documentation-audit.md).

For screenshot generation, migration, catalog changes or image cleanup, read [references/screenshot-refresh.md](references/screenshot-refresh.md).

Read both references for a full catch-up.

## Non-Negotiable Boundaries

- Prefer behavior verified in source, compiled XAML and tests over names, old screenshots, forum recollections or inferred layouts.
- Use forum and issue research only to discover recurring documentation gaps. Confirm every technical conclusion against the current source before writing it as fact.
- Preserve documentation URLs where practical. Remove obsolete unreferenced source images instead of retaining them as historical clutter.
- Keep external application screenshots, branding, diagrams and selective states unchanged unless the task specifically covers them.
- Generate NINA UI images only from real production views, templates, resources, icons and metadata. Never substitute hand-built lookalike controls or guessed icons.
- Use the production "Slate" color schema for generated captures.
- Generated raster screenshots are PNG files. When replacing a generated JPEG, add a PNG path and update references rather than generating another JPEG.
- Do not add screenshot-only APIs to shipping NINA code. Keep fixtures and deterministic state inside `tools/NINA.DocumentationScreenshots`.
- Do not add CI enforcement, scheduled automation, update bots or drift warnings. The inventory and renderer are deliberate maintainer tools.
- A selective screenshot that cannot yet be reproduced faithfully may remain cataloged and preserved with an explicit exclusion reason. Bulk automation is the goal, not low-quality total automation.

## Completion Standard

Finish only after the relevant source-backed edits, catalog updates and checked-in images agree. Run the renderer tests after the final source edit, render the affected area twice, visually review changed images and then run the complete preview when the scope warrants it. Build strict standard and PDF documentation after the final documentation edit. Check internal links, authored external-link style, image references, catalog coverage, duplicate IDs and outputs, generated PNG constraints, orphaned images and `git diff --check` in both repositories.

In the final handoff, distinguish generated assets from deliberately preserved assets, report exact verification results and include a proposed pull request title.
