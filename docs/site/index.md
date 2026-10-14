---
title: Overview
description: RunPad is a native, fast, cross-platform C# scratchpad for .NET with IntelliSense, live diagnostics, a debugger and rich .Dump() output.
---

# The native C# scratchpad for .NET

RunPad is a free, open-source alternative to LINQPad, RoslynPad and NetPad.

- Instantly run any C# snippet, statement block or program — press ++f5++
- Full IntelliSense: [completion with automatic imports, signature help, rename and Go to Definition](editor.md)
- [See problems as you type](analyzers.md): compiler errors, Roslyn analyzers and lightbulb fixes
- [Debug](debugging.md) with breakpoints, conditions, stepping, watches and exception breaks
- [Explore results with `.Dump()`](results.md): tables, live updates, progress bars and interactive input
- [Add NuGet packages](dependencies.md) with the built-in package manager, or reference any `.dll`
- [Read decompiled code](assembly-explorer.md) in an ILSpy-style Assembly Explorer
- Feel at home coming from LINQPad: [import `.linq` queries](cli.md#importing-linqpad-queries), the same `Dump` and `Util` APIs, the [same keyboard shortcuts](reference.md#keyboard-shortcuts)
- [Scripts are Markdown](scripts.md): notes and code in one `.rpad` file that reads well and diffs cleanly
- Native and fast: no embedded browser, scripts compile in-process with Roslyn
- Visual Basic scripts run and debug too
- Free and open source, on Windows, macOS and Linux

[Download RunPad](https://github.com/campforge/run-pad/releases){ .md-button .md-button--primary }
[Quick start](quick-start.md){ .md-button }

<figure markdown="span">
  [![RunPad main window with a C# script in the editor and a table of results in the Results tab below it](assets/screenshots/overview_light.png#only-light)](assets/screenshots/overview_light.png "Open full-size screenshot")
  [![RunPad main window with a C# script in the editor and a table of results in the Results tab below it](assets/screenshots/overview.png#only-dark)](assets/screenshots/overview.png "Open full-size screenshot")
  <figcaption>The main window: file tree, editor, toolbar and Results tab. Click to open it at full size.</figcaption>
</figure>

## A scratchpad for C#

Each script is a `.rpad` file: a Markdown document with a short YAML header and one
runnable code block. Write top-level statements, a `Main` program or a single expression;
RunPad compiles it with the .NET SDK installed on your machine and shows the output and any
`.Dump()` results right under the editor. There is no project to set up and nothing to build
by hand.

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

## What RunPad does not do (yet)

- Scripts cannot reference `.csproj` or `.vbproj` projects; reference a built `.dll`
  instead. See [Packages and references](dependencies.md#project-references-are-not-supported).
- There is no File → Open command. RunPad shows the `.rpad` files in its scripts folder;
  see [Where your scripts live](scripts.md#where-your-scripts-live).
- The macOS build is not signed or notarized with an Apple Developer certificate yet;
  see [Installation](install.md#macos).

## Feedback and source code

RunPad is free and open source under the GPL-3.0. The code is on
[GitHub](https://github.com/campforge/run-pad); report bugs and suggest ideas in its
[issues](https://github.com/campforge/run-pad/issues).
