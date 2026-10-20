# RunPad documentation

Source of the RunPad user guide, published at <https://campforge.github.io/run-pad-docs/>.

RunPad itself — a native, fast, cross-platform C# scratchpad for .NET — lives in
[`campforge/run-pad`](https://github.com/campforge/run-pad); downloads are on its
[releases page](https://github.com/campforge/run-pad/releases).

- `docs/site/` — the pages (Markdown), screenshots and downloadable examples; navigation is in
  `mkdocs.yml`.
- `docs/GITHUB-PAGES.md` — how the site is built, checked and deployed, and how to preview it locally.
- `scripts/screenshots/` — renders the guide's screenshots from RunPad itself, in dark and light.
- `.github/workflows/docs.yml` — builds every pull request and deploys `main` to GitHub Pages.
