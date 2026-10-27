---
title: Command line and .linq import
description: Run .rpad scripts from a terminal with the runpad CLI and convert .linq queries.
---

# Command line and .linq import

## The `runpad` command-line runner

`runpad` compiles and runs the code block of a `.rpad` file in a terminal, without the
RunPad window and without the debugger. It is useful for automation and CI. It runs C#
and Visual Basic scripts and honors `kind`, `packages`, `references` (including the path
macros) and `namespaces` the same way the RunPad window does.

!!! info "Getting the runner"
    The runner is not included in the release packages. On Linux, the `runpad` command
    that the `.deb` package puts on `PATH` starts the RunPad window, not this runner.

    The runner is the `RunPad.Cli` project in the
    [RunPad source code](https://github.com/campforge/run-pad). Build it from a clone with
    the .NET 10 SDK:

    ```bash
    dotnet build src/main/RunPad.Cli -c Release
    dotnet src/main/RunPad.Cli/bin/Release/net10.0/runpad.dll path/to/script.rpad
    ```

    Or run it straight from source:

    ```bash
    dotnet run --project src/main/RunPad.Cli -- path/to/script.rpad
    ```

    The build also produces a native launcher, `runpad` (`runpad.exe` on Windows), that
    works without the `dotnet` prefix if it can find the .NET runtime. With a
    non-standard .NET install, for example one from Homebrew or Linuxbrew, it may not, so
    use the `dotnet …/runpad.dll` form, which always works.

### Usage

```text
runpad [run] [options] <script.rpad> [--] [script arguments...]
runpad convert <input.linq> [output.rpad]
```

Running with no arguments prints this usage and exits with code 2. `--help` and
`--version` print information and exit with code 0.

- Script output goes to standard output. Text written to `Console.Error` and compiler
  errors go to standard error.
- `.Dump()` output is rendered in the terminal as tables, with nested tables for nested
  values and colors by value type. Colors are used only when the output is a terminal,
  not when it is redirected to a file or pipe.
- `DumpContainer` and `Util.ProgressBar` updates are printed one after another instead of
  updating in place.
- The script runs on the SDK its `targetSdk` names, found by `dotnet --list-sdks` on `PATH`
  (`latest`: the newest stable one). If that version isn't installed, the newest SDK runs it
  and a warning goes to standard error.
- Each run compiles into a new temporary folder that is deleted afterwards. The runner
  does not use the RunPad window's build cache.
- NuGet sources come from RunPad's own `NuGet.Config` if the RunPad window has created
  one; otherwise your normal NuGet settings are used.
- [Interactive input](results.md#interactive-input) is not supported. A script that calls
  `Util.ReadLine` waits until you press ++ctrl+c++.

### Script arguments and options

```bash
runpad report.rpad "April totals" 2026
runpad run report.rpad -- --kind --help
runpad --kind program --sdk 10 report.rpad -- "April totals"
runpad --compile-only report.rpad
```

Arguments following the script replace its front-matter `args:` list. With no script
arguments, the list in the file is used; a bare `--` after the script explicitly selects
an empty list. Use your shell's normal quoting for spaces or quotes inside one argument.

Recognized runner options are accepted before **or after** the script path. Put `--`
after the path to pass everything that follows literally to the script, including
`--help`, `--kind` or `-s`. Without it, those names are interpreted by the runner.
Unknown options after the script are passed through, except recognized unsupported
runner options, which produce an error. Always use `--` when passing option-like values.

| Option | Meaning |
| --- | --- |
| `--kind`, `-k`, `-lang=<kind>` | Override `Statements`, `Program` or `Expression`; `s`, `p`, `e` also work. `VBStatements`, `VBProgram` and `VBExpression` require a Visual Basic file; they do not translate C# code. |
| `--sdk`, `-s` | Override `targetSdk`, for example `10`, `10.0`, `net10.0` or `latest`. The normal installed-SDK selection and warning rules apply. |
| `--compile-only`, `-compileonly` | Check compilation and report diagnostics without executing the script. The temporary compilation output is removed afterwards. |
| `--refresh-connections` | Read every connection's description again before the build, such as a gRPC server's services after they changed. Without it, a description RunPad already read is reused. |
| `--help`, `-h`, `-?` | Show usage. |
| `--version` | Show the runner version. |

Overrides affect this invocation only; the script file is not rewritten. These spellings
support common `lprun`/`npad` command lines, not every option of those tools. `-nunuget`,
`--no-cache`, `--rebuild` and `-b` are accepted with no effect; package restoration is
not disabled. Options such as output format, evaluation, database connections and
optimization are not implemented and are reported as unsupported.

### Exit codes

Once the script has run, the runner exits with the script's own exit code: `0` when it
finishes normally, `42` after `Environment.Exit(42)`, and a non-zero code after an
unhandled exception. Text on standard error alone does not fail the run. Otherwise:

| Code | Meaning |
| --- | --- |
| `2` | Bad run arguments, file not found, an invalid `.rpad` document, no .NET SDK found, a compile error, or the compiled script could not be started. |
| `1` | The `convert` command failed; conversion retains its existing error code. |
| `130` | Interrupted with ++ctrl+c++. |

### Example

```console
$ dotnet runpad.dll debugging-loop.rpad
i = 1, square = 1, running total = 1
i = 2, square = 4, running total = 5
i = 3, square = 9, running total = 14
i = 4, square = 16, running total = 30
i = 5, square = 25, running total = 55
Sum of squares 1..5
55
```

## Importing .linq queries

RunPad converts `.linq` query files to `.rpad` scripts.

- **In RunPad:** choose **File → Import .linq Query...** and pick the `.linq` file.
  RunPad writes a `.rpad` file with the same name **next to the `.linq` file** and opens
  it. If that name is taken, it adds a number (`Query 2.rpad`). The file appears in the
  file tree only if it is inside your RunPad Queries folder.
- **From a terminal:** `runpad convert query.linq` writes `query.rpad` next to the input.
  Give a second argument to choose the output path. The command **overwrites** an
  existing output file.

### What is converted

| `.linq` | `.rpad` |
| --- | --- |
| `Kind="Statements"`, `"Program"`, `"Expression"` | `language: csharp` with the same kind |
| `Kind="VBStatements"`, `"VBProgram"`, `"VBExpression"` | `language: vb` with the same kind |
| `<NuGetReference Version="…">Id</NuGetReference>` | `packages:` entry (`latest` when no version is given) |
| `<Namespace>` | `namespaces:` entry |
| `<Reference>` (or its `Relative` attribute) | `references:` entry |
| The code after `</Query>` | The run block, under a heading with the file name |

!!! note "Unreleased: C# namespace imports"
    C# `.linq` imports now write LINQPad's standard namespace profile into the converted
    file's `namespaces`, together with explicit `<Namespace>` entries, without exact duplicates.
    This includes `System.Text`, `System.Data` and `System.Xml.Linq`. These imports are editable
    script metadata used by both compilation and the editor. Existing `.rpad` files, new ordinary
    scripts, global defaults and Visual Basic imports are unchanged.

    Custom defaults configured only on the original LINQPad machine are not present in the
    `.linq` header and cannot be inferred; add any missing custom imports to the converted file.

Other kinds, such as SQL or F# queries, are rejected with an error that lists the
supported kinds. Database connections are not converted. The converted file
gets `targetSdk: latest`.

Most `.Dump()` calls work unchanged; see [Results and runtime helpers](results.md). The long
positional `Dump(description, depth, collapseTo, toDataGrid, …)` overloads are not provided; use
`Dump(new DumpOptions { … })` instead.

### Try it

Download [`sample-query.linq`](examples/sample-query.linq):

```xml
<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
</Query>

var culture = CultureInfo.InvariantCulture;
var prices = new[] { 9.99m, 24.50m, 3.25m };

prices
    .Select(p => new { Price = p, Text = p.ToString("N2", culture) })
    .Dump("Formatted prices");
```

`runpad convert sample-query.linq` produces (with the unreleased import profile):

````markdown
---
version: 1.0
language: csharp
targetSdk: latest
kind: Statements
namespaces:
  - System
  - System.Collections
  - System.Collections.Generic
  - System.Data
  - System.Diagnostics
  - System.IO
  - System.Linq
  - System.Linq.Expressions
  - System.Reflection
  - System.Text
  - System.Text.RegularExpressions
  - System.Threading
  - System.Transactions
  - System.Xml
  - System.Xml.Linq
  - System.Xml.XPath
  - System.Globalization
---

# sample-query

```csharp run
var culture = CultureInfo.InvariantCulture;
var prices = new[] { 9.99m, 24.50m, 3.25m };

prices
    .Select(p => new { Price = p, Text = p.ToString("N2", culture) })
    .Dump("Formatted prices");
```
````

Running the converted script shows a **Formatted prices** table with the prices and the
texts `"9.99"`, `"24.50"` and `"3.25"`.
