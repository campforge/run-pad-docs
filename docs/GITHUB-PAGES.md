# GitHub Pages user documentation — maintainer notes

The user guide is built with [MkDocs](https://www.mkdocs.org/) and
[Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) and prepared for publication to
<https://campforge.github.io/run-pad-docs/> by `.github/workflows/docs.yml`. The site lives in its own
repository, `campforge/run-pad-docs`. The code it documents (examples run with `RunPad.Cli`, icon
sources) is in the code repository, `campforge/run-pad`; the commands below expect it cloned next to
this one as `../run-pad`.

This file is not part of the published site.

## What is published

- Source: `mkdocs.yml` and `docs/site/**` only (`docs_dir: docs/site`). MkDocs reads
  nothing else, so this file, the workflow, the scripts and the root `README.md` are never copied into
  the site.
- Changelog: `docs/site/changelog.md` is generated from `CHANGELOG.md` in the code repository by
  `scripts/docs/sync_changelog.py`. Never edit it by hand; after a release, run the script and
  commit the result.
- Downloadable examples: `docs/site/examples/*.rpad` and `*.linq` are copied as static
  files.
- Icons: `docs/site/assets/app-icon.svg` (also used on the Campforge home page) and
  `docs/site/assets/favicon.png` (copy of `src/main/RunPad/Assets/runpad.png` in the code
  repository).
- The theme uses no web fonts (`theme.font: false`) and no repository widget, so the
  pages make no third-party requests. `scripts/docs/check_site_links.py` fails the build if
  a page loads a stylesheet, script, image or font from another origin.

## Workflow

`.github/workflows/docs.yml` runs only when `docs/site/**`, `mkdocs.yml`,
`requirements-docs.txt`, `scripts/docs/**` or the workflow itself change, or when started
manually.

| Event | Build (`mkdocs build --strict` + link check) | Deploy |
| --- | --- | --- |
| Pull request | Yes, with `contents: read` only | No |
| Push to `main` | Yes, and uploads the Pages artifact | Yes |
| Manual run (*Run workflow*) on `main` | Yes, and uploads the Pages artifact | Yes |
| Manual run on another branch | Yes | No |

The deploy job alone gets `pages: write` and `id-token: write`, runs in the
`github-pages` environment, and uses `actions/configure-pages@v5` and
`actions/deploy-pages@v4`; the build job uploads with `actions/upload-pages-artifact@v4`.

## One-time setup (repository admin)

1. **Settings → Pages → Build and deployment → Source: GitHub Actions.** Do not choose
   "Deploy from a branch"; no `gh-pages` branch is used.
2. The first deployment creates the `github-pages` environment. In
   **Settings → Environments → github-pages**, keep the deployment branch rule limited to
   `main` (the default for Pages).
3. Push a change under `docs/site/` to `main`, or run the **Docs** workflow manually on
   `main`. The site appears at <https://campforge.github.io/run-pad-docs/>.

`site_url` in `mkdocs.yml` and `SITE_URL` in the workflow must match the Pages URL. If the
repository is renamed or a custom domain is configured, update both.

### Private repositories

- GitHub Pages is available for **public** repositories on every plan, but for
  **private** repositories only with GitHub Pro, GitHub Team, GitHub Enterprise Cloud or
  GitHub Enterprise Server. On GitHub Free, a private repository cannot publish Pages; the
  deploy job then fails at *Configure Pages* or *Deploy*, while PR builds keep working.
- A Pages site is **public on the internet even when the repository is private**, unless
  the repository belongs to an organization on GitHub Enterprise Cloud that restricts
  Pages access. Only the built `docs/site` content is published; internal documents in
  the repository stay private.
- The guide links to the releases page and the source code in `campforge/run-pad`. While
  that repository is private, anonymous readers cannot open those links.

## Building locally

Use an isolated virtual environment, not a global install:

```bash
python3 -m venv /tmp/runpad-docs-venv
/tmp/runpad-docs-venv/bin/pip install -r requirements-docs.txt
/tmp/runpad-docs-venv/bin/mkdocs serve              # preview at http://127.0.0.1:8000/run-pad-docs/
/tmp/runpad-docs-venv/bin/mkdocs build --strict     # output in ./site (git-ignored)
/tmp/runpad-docs-venv/bin/python scripts/docs/check_site_links.py site --site-url https://campforge.github.io/run-pad-docs/
```

`requirements-docs.txt` pins `mkdocs`, `mkdocs-material`, `pymdown-extensions` and
`Pygments`. When updating, check the latest stable versions on PyPI, rebuild with
`--strict` and rerun the link check. Material for MkDocs prints a notice about MkDocs 2.0
during builds; the pinned MkDocs 1.6 line is what this site uses.

## Checking the examples

Every `.rpad` example except `analyzer-diagnostics.rpad` should run cleanly with the
command-line runner (no network, files or packages needed):

```bash
dotnet build ../run-pad/src/main/RunPad.Cli -c Release
for f in docs/site/examples/*.rpad; do dotnet ../run-pad/src/main/RunPad.Cli/bin/Release/net10.0/runpad.dll "$f"; done
```

`analyzer-diagnostics.rpad` is expected to stop with `error CS0103` (exit code 1). The
expected output of each example is described in `docs/site/examples.md`; update it when
an example changes.

## Screenshots

Screenshots live in `docs/site/assets/screenshots/`. Each RunPad screenshot comes in two
files, `name.png` (dark) and `name_light.png` (light), shown with `#only-dark` and
`#only-light` so a page follows the reader's theme. `mkdocs build --strict` fails if an
image link is broken.

They are rendered from RunPad itself, not drawn: `scripts/screenshots` is a small program
that opens RunPad's real main window and dialogs headless (Avalonia's headless platform
with Skia), drives them through the same internal hooks as RunPad's UI tests and saves
each window at 2× scale. Scripts really run, the debugger really stops and the package
search really queries nuget.org. Every scenario starts a fresh RunPad with default
settings kept in memory and a temporary RunPad Queries folder filled from
`docs/site/examples`, so the captures never show your own settings, scripts or file paths.

To regenerate them after a release (Windows, with `../run-pad` checked out at that
release, a .NET 10 SDK, the `netcoredbg` that RunPad's first-run setup installs or
`RUNPAD_NETCOREDBG` pointing at one, and network access):

```bash
cd scripts/screenshots
dotnet run                        # every scenario, dark and light (about 5 minutes)
dotnet run -- debugging editor-light   # only these
```

Look through the results before committing: a scenario fails loudly when something it
waits for never happens, but a changed layout only shows in the picture. When a window
or a feature changes, adjust its scenario in `Scenarios.cs` and the image's alt text.

| Scenario | Files | Page | Captured state |
| --- | --- | --- | --- |
| `overview` | `overview` | Overview | `products-overview.rpad` after a run: code, Results tables, the sidebar's Connections and Scripts sections |
| `editor` | `editor-highlighting` | Editor | A LINQ query with the completion list open after `.Ta` and Take's documentation |
| `search` | `editor-search` | Editor | The find and replace strip searching `word`, 10 matches highlighted |
| `diagnostics` | `diagnostics` | Diagnostics and analyzers | `analyzer-diagnostics.rpad`: CS0103 on `JsonSerializer` with the lightbulb menu open; RunPad analyzers on |
| `debugging` | `debugging` | Debugging | `debugging-loop.rpad` paused on line 5 in the third iteration: Threads, Call Stack, Locals |
| `preview` | `markdown-preview` | Scripts and .rpad files | `products-overview.rpad` with the Markdown preview to the side |
| `preferences` | `preferences` | Reference | Preferences → Interface |
| `dependencies` | `dependencies` | Packages and references | Online search for Humanizer, Humanizer.Core selected; the local cache filtered to Humanizer |
| `explorer` | `assembly-explorer` | Assembly Explorer | System.Linq.dll searched for `Enumerable.Where`, the first result decompiled |

The interface uses RunPad's own themes, RunPad Midnight and RunPad Warm Light, with the
editor's default Dark+ and Light+ colors. The window chrome is RunPad's own Windows title bar; dialogs are captured without the
system's title bar. Numbers are formatted with the invariant culture. The GitHub and
Obsidian screenshots (`github-rpad-preview.png`, `obsidian*.png`) show other programs and
are captured by hand.
