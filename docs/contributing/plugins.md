Plugins are intended to add functionality that is useful for a more specialized workflow without having to add it to the core application.

## Getting Started

The usual starting point for a new plugin is the [Plugin Template Repository](https://github.com/isbeorn/nina.plugin.template). It provides the base structure and the setup expected by the plugin system.

## Making a Plugin Available in N.I.N.A.

To make a plugin available from the **Available** plugins tab, submit a manifest to the main [Plugin Manifest Repository](https://github.com/isbeorn/nina.plugin.manifests).

The **Available** tab reads plugin repositories and shows the metadata supplied there, so the manifest is what users will see before they install your plugin.

## What Users See

The plugin pages inside N.I.N.A. can display:

* plugin name, author, and version
* homepage and changelog links
* short and long descriptions
* tags
* installation method and download link
* source repository
* license information
* screenshots

If your plugin exposes user-configurable options, they appear on the **Installed** tab after the plugin has been loaded.

For manifest format details and submission instructions, use the README in the plugin manifest repository.
