---
title: Overview
description: RunPad is a native, fast, cross-platform C# scratchpad for .NET with IntelliSense, live diagnostics, a debugger and rich .Dump() output.
---

# The native C# scratchpad for .NET

RunPad is a free, open-source C# scratchpad for Windows, macOS and Linux.

- Instantly run any C# snippet, statement block or program — press ++f5++
- Full IntelliSense: [completion with automatic imports, signature help, rename and Go to Definition](editor.md)
- [See problems as you type](analyzers.md): compiler errors, Roslyn analyzers and lightbulb fixes
- [Debug](debugging.md) with breakpoints, conditions, stepping, watches and exception breaks
- [Explore results with `.Dump()`](results.md): tables, live updates, progress bars and interactive input
- [Add NuGet packages](dependencies.md) with the built-in package manager, or reference any `.dll`
- [Read decompiled code](assembly-explorer.md) in an ILSpy-style Assembly Explorer
- [Import `.linq` queries](cli.md#importing-linq-queries) and run them with few or no changes
- [Markdown-compatible scripts](scripts.md): keep notes and runnable code together in `.rpad` files,
  and view them as Markdown in Obsidian or repository previews
- [View your scripts in Obsidian](obsidian.md): read and edit the notes and code of `.rpad` files with the RunPad Markdown plugin
- Native and fast: no embedded browser, scripts compile in-process with Roslyn
- Visual Basic scripts run and debug too
- Free and open source, on Windows, macOS and Linux

[Download RunPad](https://github.com/campforge/run-pad/releases){ .md-button }
[Quick start](quick-start.md){ .md-button }

<figure markdown="span">
  [![RunPad main window with a C# script in the editor and a table of results in the Results tab below it](assets/screenshots/overview_light.png#only-light)](assets/screenshots/overview_light.png "Open full-size screenshot")
  [![RunPad main window with a C# script in the editor and a table of results in the Results tab below it](assets/screenshots/overview.png#only-dark)](assets/screenshots/overview.png "Open full-size screenshot")
</figure>

## A scratchpad for C\#

Each script is a `.rpad` file: a Markdown document with a short YAML header and one
runnable code block. Write top-level statements, a `Main` program or a single expression;
RunPad compiles it with the .NET SDK installed on your machine and shows the output and any
`.Dump()` results right under the editor. There is no project to set up and nothing to build
by hand.

## Markdown-compatible files

`.rpad` is a **Markdown-compatible format**: a document containing a YAML metadata header,
Markdown notes and a fenced, runnable code block. Your code and its documentation stay
in one readable file that you can version, share and review.

You can view and edit these files in [Obsidian with the RunPad Markdown plugin](obsidian.md),
or read them as Markdown on repository pages that support rendering the file type.
For a working GitHub example, open [CompareTo.rpad in the examples repository](https://github.com/campforge/runpad-examples/blob/main/CompareTo.rpad)
and select **Preview**. The file keeps its `.rpad` extension; no separate `.md` copy is needed.

GitHub preview uses a Markdown file-type hint. Add this HTML comment at the end of the
file, outside the code block:

```html
<!-- vim: set filetype=markdown: -->
```

For repository highlighting and Markdown diffs, add `*.rpad linguist-language=Markdown diff=markdown`
to `.gitattributes`.

![A .rpad file rendered as Markdown on GitHub, with metadata, a title and highlighted C# code](assets/screenshots/github-rpad-preview.png)

The comment stays invisible in the rendered document. Repository rendering support varies
by host; GitLab's Markdown syntax-highlighting setting alone does not enable a rendered
`.rpad` preview.

## Download

RunPad runs on Windows, macOS (Apple Silicon and Intel) and Linux. Installers and
portable builds for every release are on the
[releases page](https://github.com/campforge/run-pad/releases); the
[installation guide](install.md#download) explains which file to pick, and the
[changelog](changelog.md) lists what changed in each release.

## Start here

1. [Install RunPad](install.md) and let the first-run setup find or install the .NET
   SDK and the `netcoredbg` debugger.
2. Follow the [quick start](quick-start.md) to run your first script and read its
   output.
3. Download the [examples](examples.md) to try the features described in this guide.

## C# and Visual Basic

RunPad is built for C#. Visual Basic scripts compile, run and debug the same way, but
editor intelligence currently exists only for C#.

| Capability | C# | Visual Basic |
| --- | --- | --- |
| Run, kinds (Statements, Program, Expression) | Yes | Yes |
| Debugger (breakpoints, stepping, locals, watches) | Yes | Yes |
| NuGet packages, assembly references, namespaces | Yes | Yes |
| `.Dump()` and the RunPad runtime helpers | Yes | Yes |
| Syntax highlighting, brace matching, folding | Yes | Yes (keyword-based folding) |
| Completion, signature help, quick info | Yes | No |
| Live diagnostics, analyzers, quick fixes | Yes | No (errors appear when you run) |
| Semantic highlighting, inlay hints | Yes | No |
| Rename, Find References, Go to Definition, Format Document | Yes | No |
| Snippets | Yes | No |

## Feedback and source code

RunPad is free and open source under the GPL-3.0. The code is on
[GitHub](https://github.com/campforge/run-pad); report bugs and suggest ideas in its
[issues](https://github.com/campforge/run-pad/issues).
