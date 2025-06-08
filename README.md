# 📚 N.I.N.A. Documentation

[![Website](https://img.shields.io/badge/website-nighttime--imaging.eu-blue)](https://nighttime-imaging.eu/)
[![Discord](https://img.shields.io/discord/436650817295089664)](https://discord.gg/nighttime-imaging)
[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](https://www.mozilla.org/en-US/MPL/2.0/)

This repository contains the source files for the documentation of **N.I.N.A. – Nighttime Imaging 'N' Astronomy**.

📖 The live documentation is available at:  
**https://nighttime-imaging.eu/docs/master/site/**

---

## ✍️ Contributing

We welcome all contributions — from fixing typos to writing new guides or documenting upcoming features.

Please read our [Contributing Guide](CONTRIBUTING.md) for full details on:

- Setting up the environment with MkDocs
- Building and previewing the docs locally
- The branching model used for contributions
- How pull requests are handled and deployed

---

## ⚙️ Quick Setup

To get started locally, follow these steps:

1. Install [Python](https://www.python.org/)
2. Upgrade pip:
    ```bash
    pip install --upgrade pip
    ```
3. Install MkDocs and required plugins:
    ```bash
    pip install mkdocs mkdocs-material mkdocs-with-pdf
    ```
4. Start the local development server:
    ```bash
    mkdocs serve
    ```
5. Open the browser at the address shown in the terminal to view the docs live.

---

## 🌿 Branching Model
- `master`: live documentation for the latest **stable release**
- `develop`: live documentation for the **nightly builds**

Documentation updates are deployed automatically via GitHub Actions when pull requests are merged into `master` or `develop`.

---

## 📌 Guidelines

- Create pull requests from your fork.
- PRs for new features should target the `develop` branch.
- Test your changes locally by running `mkdocs serve`.
- Keep each PR focused on a single topic/feature.
- Ensure your content follows the general tone and structure of existing documentation.

---

## 💬 Need Help?

- 🔧 Main application repo: [github.com/isbeorn/N.I.N.A](https://github.com/isbeorn/nina)
- 🧠 Real-time support: [Discord Community](https://discord.gg/nighttime-imaging)
- 🌐 More resources: [nighttime-imaging.eu](https://nighttime-imaging.eu/)

---

## ⚖️ License

This project is licensed under the **Mozilla Public License 2.0**.  
See the [`LICENSE`](./LICENSE) file for details.
