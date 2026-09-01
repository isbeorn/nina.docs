## Contributing

Thank you for considering a contribution to N.I.N.A.'s documentation!

## Prerequisites
The documentation uses MkDocs. An in-depth guide on how to set it up and use MkDocs can be found on the [MkDocs project homepage](//www.mkdocs.org).
In summary you need:  
- [Python](//www.python.org/)
  - Pip `pip install --upgrade pip`  
- Install MkDocs and needed plugins: `pip install mkdocs mkdocs-material mkdocs-with-pdf`  
- A markdown editor of your choice  

## Building the docs
MkDocs offers a neat built-in server feature to build and preview the documentation on your local machine on the fly. Each time a file is saved, the local server is updated automatically.
To run the server simply run the serve command and open your browser on the indicated address.  
```mkdocs serve``` 

## Branching Model
This project is utilizing a standard git flow where it has the following branches  
* master: all officially released code  
* hotfix/<hotfixname>: used to fix issues inside master  
* release/<version>: when preparing a release with new features a temporary release branch is created for that new release  
* bugfix/<bugfixname>: issues that are found during a release will be fixed here  
* develop: a general develop branch that will contain unreleased new features  
* feature/<featurename>: new features that will be developed and merged to the develop branch  

[A more in-depth guide about this model can be found here](//nvie.com/posts/a-successful-git-branching-model/)

The most relevant branches are master and develop. These branches all have a separate page for documentation on the homepage. 
This enables users that will use for example the released version of N.I.N.A. to have a separate documentation, compared the ones that use the nightly builds and already want to see new features described.

## Pull Requests
* For contributing to this documentation you should fork the repository
* Inside your fork you can make your changes
* Once you are finished with your planned changes it is time to put up a pull request from your fork to the master repository
* Make sure that only relevant changes are inside the pull request  
* Check that the documentation builds properly using the serve command
* Try to create **one pull request per feature**
* Create your pull requests for new features only against the **develop** branch  

## Updating the homepage
The documentation on the homepage is updated automatically via github actions. Each time a pull request is complete, the pipeline will build the docs and upload the changes to the homepage.
There is no action required by a contributor for this.

## On-demand documentation inventory

Maintainers can compare a N.I.N.A. source checkout with the documentation when performing a catch-up audit. The report extracts sequencer instructions, conditions and triggers, profile settings, documentable views and image file formats. It is intentionally heuristic and is not a completeness test.

From the documentation repository, run:

```powershell
.\scripts\report-doc-coverage.ps1 -NinaSource C:\path\to\nina -OutputPath .artifacts\doc-coverage.md
```

The report includes the source identifier, display name, category, likely documentation pages and implementation path. An optional mapping file can override heuristic matches. Pass `-MappingPath` with JSON in this form:

```json
{
  "items": [
    {
      "identifier": "SetUSBLimit",
      "documentation": ["sequencer/advanced/instructions.md"]
    }
  ]
}
```

Review entries marked for attention against the implementation. A match only means that a name occurs on a page and does not prove that the description is accurate.

## Regenerating N.I.N.A. screenshots

Application screenshots are cataloged in `screenshots/manifest.json` and rendered by the non-shipping `tools/NINA.DocumentationScreenshots` project in this documentation repository. `-NinaSource` points the project to the N.I.N.A. solution directory containing `NINA.sln`. The renderer uses compiled production views, the US English (`en-US`) locale, N.I.N.A.'s production Slate color schema, isolated in-memory profile settings and no real devices or network access. Install the .NET SDK used by the source checkout before running it.

Preview all managed screenshots without replacing checked-in files:

```powershell
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Preview -Restore
```

`-Restore` is needed only for the first run or when the source project's dependencies have changed. Later runs can omit it. To preview one documentation area or one stable catalog ID:

```powershell
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Area tabs -Preview
.\scripts\generate-screenshots.ps1 -NinaSource C:\path\to\nina -Id docs-images-sequencer-instructions-utility-waituntil-png -Preview
```

Remove `-Preview` only after reviewing the summary. The renderer completes and validates the whole requested set in a temporary directory before replacing any PNG. All generated assets must have a `.png` output path, including replacements for legacy JPEG screenshots. Existing JPEG files may remain cataloged as static compatibility assets but the renderer never creates or overwrites them. If one fixture fails, all checked-in images are left unchanged. A successful run reports added, changed, unchanged and failed counts. After a non-preview run, review the resulting image diffs before committing them.

The versioned catalog classifies every source image below `docs` as `nina-ui`, `nina-generated-visual`, `external-ui` or `brand-or-static`. The renderer owns the first two classifications and writes them below `docs/images/generated`, preserving the documentation-area hierarchy under that directory. External application screenshots, branding, diagrams, annotated examples and selective states that do not yet have a faithful fixture remain in their existing source-image directories with an exclusion reason. Full regeneration leaves all excluded files untouched.

Catalog entries can select a fixture, a named state, fixed dimensions, a normalized crop and normalized callouts. Keep crop and callout coordinates between 0 and 1. Add a fixture inside the developer tool when a real compiled view needs deterministic sample data. Sequencer fixtures select an exact production type and obtain their name, category and icon from its production export metadata. Do not infer views or icons from filenames and do not add screenshot-only APIs to shipping N.I.N.A. code.

`scripts/initialize-screenshot-catalog.ps1` recreates the one-time baseline from the current image tree. It is not part of normal regeneration. A forced rebuild carries forward stable IDs, reviewed mappings and exclusions from the existing catalog. A newly discovered image is kept as static until a maintainer verifies its real production view and deterministic state, so the initializer never guesses a fixture or icon from its filename.

Screenshot generation and documentation inventory are deliberate local maintenance operations. They do not run in CI, schedule updates or block documentation builds.
