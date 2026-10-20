---
title: Results and runtime helpers
description: Console output, .Dump() tables, DumpOptions, DumpContainer, progress bars, interactive input and other Util helpers.
---

# Results and runtime helpers

Every script is compiled against a small runtime library, `RunPad.Runtime`. It provides
`.Dump()`, `DumpContainer`, `DumpOptions` and the `Util` class. Its namespace is imported
automatically, so none of these need a `using`. Scripts imported from `.linq` files
usually work without changes.

## The output tabs

Under the editor are three tabs:

- **Results** — console output and `.Dump()` blocks, in the order the script produces
  them. Text the script writes to `Console.Error`, compiler errors and unhandled
  exceptions are shown as errors. When the run ends, the status strip below shows the
  elapsed time and *Success*, *Cancelled*, *Compilation failed* or *Exit code N* when the
  script ends with a non-zero exit code.
- **Debugger** — the debugger panels; see [Debugging](debugging.md).
- **IL** — the IL disassembly of the last successful compile, as read-only text.

The button with the panel icon in the toolbar shows or hides the output area.

## `.Dump()`

`.Dump()` renders any value in the Results tab and returns the same value, so you can put
it in the middle of an expression:

```csharp
var evens = Enumerable.Range(1, 10)
    .Where(n => n % 2 == 0)
    .Dump("Even numbers")      // renders the list ...
    .Sum();                    // ... and the chain continues
evens.Dump();
```

How values are shown:

- A collection of objects becomes a table with one column per public property or field.
  A collection of simple values becomes a two-column table (`#`, `Value`).
- Nested objects and collections become nested tables, expanded up to **3 levels** by
  default.
- Collections show at most **1000 rows** by default; the rest is summarized as
  "… N more".
- Strings are shown in quotes and very long strings are shortened. `null` is shown as
  `null`.
- Dictionaries, enums, dates, numbers and booleans have their own formatting.

Without a label, the heading is the source text of the dumped expression. For
`total.Dump()` the heading is `total`.

### Overloads

| Call | Effect |
| --- | --- |
| `x.Dump()` | Heading is the expression text. |
| `x.Dump("label")` | Custom heading. |
| `x.Dump(5)` | Expands nested values 5 levels deep. No heading. |
| `x.Dump(5, 1)` | 5 levels deep; levels after the first start collapsed (click to expand). |
| `x.Dump("label", 5)` | Heading and depth. |
| `x.Dump(true)`, `x.Dump("label", true)` | Accepted for compatibility with imported scripts; the result looks the same as without `true`. |
| `x.Dump(new DumpOptions { … })` | Full control, see below. |

### `DumpOptions` and `Util.DumpDefaults`

```csharp
people.Dump(new DumpOptions
{
    Description = "People",
    MaxDepth = 2,
    MaxRows = 50,
    MembersToExclude = "Password,InternalId",
});
```

| Property | Meaning |
| --- | --- |
| `Description` | Heading text. |
| `MaxDepth` | Nesting levels to expand (default 3). |
| `CollapseTo` | Levels after this one start collapsed. |
| `MaxRows` | Maximum rows per collection (default 1000). |
| `MembersToInclude`, `MembersToExclude` | Comma-separated member names to show or hide. |
| `IncludePrivateMembers` | Also show non-public properties and fields. |
| `AlphabetizeMembers` | Sort columns by name. |
| `NoTotals` | Hide the row-count footer under tables. |
| `RepeatHeadersAt` | Repeat the header row every N rows. |
| `ForceExpansion` | Render expanded even for types that start collapsed by default. |
| `UseInvariantCultureForNumbers` | Format numbers with the invariant culture. |
| `FormatStrings` | Format strings for integers, real numbers, dates and times. |
| `ToDataGrid` | Accepted for compatibility with imported scripts; currently has no visible effect. |

To change the defaults for the rest of a run, set properties on `Util.DumpDefaults`, for
example `Util.DumpDefaults.MaxDepth = 5;`.

A type can control which members `.Dump()` shows by implementing `ICustomMemberProvider`.

## Live-updating output

`DumpContainer` is a placeholder block. Dump it once, then change its content; the same
block updates in place instead of adding a new one.

```csharp
var status = new DumpContainer("Starting...");
status.Dump("Status");
status.Content = "Halfway there";       // replaces the content in place
status.AppendContent("Still working");  // adds to it
status.ClearContent();                  // empties it
```

`UpdateContent`, `AppendContent`, `ClearContent`, `Refresh`, `Style` (inline CSS for the
block) and `DumpDepth` are available.

`Util.ProgressBar` works the same way:

```csharp
var progress = new Util.ProgressBar(0, "Processing").Dump();
for (var i = 1; i <= 5; i++)
{
    Thread.Sleep(200);
    progress.PercentComplete = i * 20;  // the same bar updates
}
progress.Caption = "Done";
```

[`live-output.rpad`](examples/live-output.rpad) combines both. In the
[command-line runner](cli.md), each update is printed again below the previous output
instead of replacing it.

## Interactive input

A script can ask for input in the Results area while it runs:

```csharp
var name = Util.ReadLine("Your name?");
var age = Util.ReadLine<int>("Your age?");
var secret = Util.GetPassword("Password?");   // typed text is masked on screen
$"{name} is {age}".Dump();
```

An input bar appears under the output tabs with the prompt. Type the answer and press
++enter++. The script waits until you answer. `ReadLine<T>` converts the answer with the
invariant culture and works for types such as `int`, `double`, `bool` and `DateTime`.
If the run is cancelled before you answer, the result is `null` (or the default value).

Interactive input works only in a normal run in the RunPad window. It is not available
while debugging, and the command-line runner does not answer input requests, so such a
script waits there until you press ++ctrl+c++.

## Secrets

Keep passwords, tokens and keys out of your scripts: save them once in **Tools → Secret Store**
and read them by name.

```csharp
var token = Util.GetSecret("billing-api");
```

<figure markdown="span">
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store_light.png#only-light)](assets/screenshots/secret-store_light.png "Open full-size screenshot")
  [![The Secret Store window: three secrets by name, the selected one's value hidden](assets/screenshots/secret-store.png#only-dark)](assets/screenshots/secret-store.png "Open full-size screenshot")
</figure>

The Secret Store window lists the names of your secrets. Select one to change its name or value;
the value stays hidden until you tick **Show**. **Add** and **Remove** change the list, and **OK**
saves it. Names must differ, letter case aside, and `Util.GetSecret` finds them the same way.

The secrets belong to you on this computer and live outside every script, in `secrets.json` in
RunPad's data folder (see [Where RunPad stores data](reference.md#where-runpad-stores-data)). On
Windows the values are encrypted for your Windows account; on macOS and Linux the file is
readable only by you, like `~/.pgpass`. A script asking for a secret that isn't there fails with
`KeyNotFoundException`, whose message names the secret and the store. The command-line runner
reads the same store. To use another file, for example on a build server, set the
`RUNPAD_SECRETS_FILE` environment variable to its path.

## Connections

**Tools → Connections** keeps your own connections in one window: for now HTTP APIs, with database
and gRPC connections to follow. **Add** creates one of a type; the form shows that type's settings.
For an HTTP API they are the base address, headers sent with every request and a timeout.

<figure markdown="span">
  [![The Connections window: a billing HTTP connection with its base address and an Authorization header that names a secret](assets/screenshots/connections_light.png#only-light)](assets/screenshots/connections_light.png "Open full-size screenshot")
  [![The Connections window: a billing HTTP connection with its base address and an Authorization header that names a secret](assets/screenshots/connections.png#only-dark)](assets/screenshots/connections.png "Open full-size screenshot")
</figure>

Write a password or token as `${secret:name}` (or pick it from **Insert secret**): the value stays
in the [Secret Store](#secrets) and is filled in only when the connection is used.
`${env:NAME}` takes an environment variable instead, and `$${` writes a literal `${`.
**Test Connection** tries the selected connection with those values. **OK** saves the list in
`connections.json` in RunPad's data folder; secrets are never written there.

### Connections in a script

In a C# script, open **Tools → Dependencies…** and the **Connections** tab, pick one of your
connections and choose **Add to Script**. Once you save, the script's code has a client for it,
named after the connection in PascalCase: `billing` is `Connections.Billing`, `orders-db` is
`Connections.OrdersDb`. For an HTTP connection the client is an `HttpClient` with the base address,
headers and timeout already set:

```csharp
var invoices = await Connections.Billing.GetStringAsync("invoices");
invoices.Dump();
```

The editor completes the client and its members right away, before the script runs.

<figure markdown="span">
  [![The editor completing Connections.Billing.GetS with GetStreamAsync and GetStringAsync](assets/screenshots/connections-completion_light.png#only-light)](assets/screenshots/connections-completion_light.png "Open full-size screenshot")
  [![The editor completing Connections.Billing.GetS with GetStreamAsync and GetStringAsync](assets/screenshots/connections-completion.png#only-dark)](assets/screenshots/connections-completion.png "Open full-size screenshot")
</figure>

The script keeps its own copy of the connection under `connections:` in its front matter (see
[Front matter](scripts.md#front-matter)), so it runs anywhere, also in the
[command-line runner](cli.md). Secrets stay `${secret:name}` placeholders in the file and are read
from the Secret Store of whoever runs the script, when it runs.

A connection that needs libraries, such as a database driver, brings them as ordinary packages of
the script: **Add to Script** also adds them to the **Packages** tab, where you can see and update
them like any other package, and they are restored the same way.

The tab shows, for each of the script's connections, its name in code, whether it is the same as
yours, and what would fail the build: a missing secret, a missing setting, a missing package, or a
connection type this RunPad doesn't have. **Remove from Script** takes it out; **Import to Your Connections** keeps a
script's connection in your own list (replacing one of the same name), for example one that came
with a script someone shared.

<figure markdown="span">
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections_light.png#only-light)](assets/screenshots/dependencies-connections_light.png "Open full-size screenshot")
  [![The Dependencies window's Connections tab: billing in the script as Connections.Billing, github ready to add](assets/screenshots/dependencies-connections.png#only-dark)](assets/screenshots/dependencies-connections.png "Open full-size screenshot")
</figure>

Connections work in C# scripts for now.

## Other `Util` helpers

| Member | What it does |
| --- | --- |
| `Util.CurrentQueryPath` | Full path of the running `.rpad` file, or `null` for an unsaved tab. |
| `Util.GetSecret(name)` | A secret saved in **Tools → Secret Store**; see [Secrets](#secrets). |
| `Util.Cache(func, key, duration)`, `Util.CacheAsync(…)` | Returns a cached result for `key`, or runs the function and stores its result. The cache survives across runs; `duration` is an optional time-to-live. |
| `Util.SaveString`, `Util.LoadString`, `Util.SaveBytes`, `Util.LoadBytes` | Save and load files by name in RunPad's per-user data folder. |
| `Util.Try(action)`, `Util.Try(func)` | Run code and get the exception back instead of having it thrown. |
| `Util.KeepRunning()` | Keeps the script's process alive until the returned object is disposed, for timers or background work. |
| `Util.CancellationToken` | Cancelled when you press **Stop**; see [Stopping a script](#stopping-a-script). |
| `Util.Cmd(…)` | Runs an external program, streams its output into the Results tab and returns its exit code. An overload captures the output into a string instead. |

`Util.Cache` and the `Save…` methods write files under the `UserData` folder inside
RunPad's data folder (see [Where RunPad stores data](reference.md#where-runpad-stores-data)).
Cached entries stay there until they expire or you delete the files.

## Stopping a script

**Stop** (++shift+f5++) ends a running script at once — unless the script listens for it. A script
that uses `Util.CancellationToken`, or a Program script whose `Main` takes a `CancellationToken`,
sees the token cancelled and gets up to 3 seconds to close files, connections and child processes
before RunPad ends it:

```csharp
async Task Main(string[] args, CancellationToken ct)
{
    using var connection = await OpenConnectionAsync();
    try
    {
        await ProcessAsync(connection, ct);
    }
    finally
    {
        Console.WriteLine("Closing the connection");
    }
}
```

The run still counts as cancelled. Under the debugger, Stop always ends the script at once.
