using System.Reflection;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RunPad.Avalonia.Themes;
using RunPad.Services;
using RunPad.Services.Scaling;
using RunPad.UserControls;
using RunPad.UserControls.Models;
using RunPad.Windows;
using RunPad.Windows.Models;
using static Shots.Program;

namespace Shots;

public sealed class Ctx : IAsyncDisposable
{
    public required MemoryPrefs Prefs;
    public required ServiceProvider Services;
    public required MainWindow Window;
    public required MainWindowViewModel Vm;
    public required EditorTabsControl Tabs;
    public required string Root;

    public static async Task<Ctx> Create(double width = 1100, double height = 700, Action<MemoryPrefs>? configure = null)
    {
        var temp = Directory.CreateTempSubdirectory("runpad-shots").FullName;
        var root = Path.Combine(temp, "RunPad Queries");
        Directory.CreateDirectory(root);
        var prefs = new MemoryPrefs();
        prefs.Appearance.Mode = Program.Light ? ThemeMode.Light : ThemeMode.Dark;
        // RunPad's own interface themes; the editor keeps its default Dark+ / Light+ colors.
        prefs.Appearance.DarkInterfaceTheme = "RunPad Midnight";
        prefs.Appearance.LightInterfaceTheme = "RunPad Warm Light";
        prefs.Appearance.RestoreUnsavedTabs = false;
        // The debugger the first-run setup installs on Windows, unless RUNPAD_NETCOREDBG names another.
        prefs.Debugger.DebuggerPath = Environment.GetEnvironmentVariable("RUNPAD_NETCOREDBG") is { Length: > 0 } dbg ? dbg
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RunPad", "netcoredbg", "netcoredbg", "netcoredbg.exe");
        prefs.Layout.SidebarWidth = 220;
        configure?.Invoke(prefs);
        var sp = CreateServices(prefs, root);
        sp.GetRequiredService<IThemeService>();
        InterfaceScaling.Attach(ViewScale.For(prefs));
        var window = sp.GetRequiredService<MainWindow>();
        window.Width = width;
        window.Height = height;
        window.Show();
        await Pump(500);
        return new Ctx
        {
            Prefs = prefs, Services = sp, Window = window, Vm = (MainWindowViewModel)window.DataContext!,
            Tabs = sp.GetRequiredService<EditorTabsControl>(), Root = root,
        };
    }

    public string AddScript(string name, string content)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content.ReplaceLineEndings("\n"));
        return path;
    }

    public string AddExample(string file, string? as_ = null)
        => AddScript(as_ ?? file, File.ReadAllText(Path.Combine(Program.ExamplesDir, file)));

    public async Task<(EditorTabItemViewModel Tab, EditorsTabItemControl Ctrl)> Open(string path)
    {
        await Vm.OpenScriptAsync(path);
        var tab = Vm.Tabs.Single(t => t.FilePath == path);
        EditorsTabItemControl? ctrl = null;
        await WaitUntil(() => (ctrl = Tabs.ControlForTests(tab)) is not null, 10000, "tab control");
        await Pump(1500);
        return (tab, ctrl!);
    }

    public async Task RunAndWait(EditorTabItemViewModel tab)
    {
        Tabs.RunOrContinueActive();
        await WaitUntil(() => !tab.EnableRunButton, 10000, "run start");
        await WaitUntil(() => tab.EnableRunButton, 120000, "run end");
        await Pump(1500);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var t in Vm.Tabs) t.IsDirty = false;
        Window.Close();
        await Pump(200);
        Window.Dispose();
        Vm.Dispose();
        InterfaceScaling.Detach();
        await Services.DisposeAsync();
    }
}

public static class Scenarios
{
    public static readonly string[] All =
        ["overview", "editor", "search", "diagnostics", "debugging", "preview", "preferences", "dependencies", "explorer"];

    public static async Task Run(string name)
    {
        Program.Light = name.EndsWith("-light");
        if (Program.Light) name = name[..^"-light".Length];
        switch (name)
        {
            case "overview": await Overview(); break;
            case "editor": await Editor(); break;
            case "search": await Search(); break;
            case "diagnostics": await Diagnostics(); break;
            case "debugging": await Debugging(); break;
            case "preview": await Preview(); break;
            case "preferences": await Preferences(); break;
            case "dependencies": await Dependencies(); break;
            case "explorer": await Explorer(); break;
            default: throw new ArgumentException("Unknown scenario " + name);
        }
    }

    private static void AddTree(Ctx c)
    {
        c.AddExample("quick-start.rpad");
        c.AddExample("live-output.rpad");
        c.AddExample("debugging-loop.rpad");
        c.AddExample("program-kind.rpad", Path.Combine("Samples", "program-kind.rpad"));
        c.AddExample("expression-kind.rpad", Path.Combine("Samples", "expression-kind.rpad"));
    }

    public static void ScrollResultsHome(Ctx c)
    {
        foreach (var panel in c.Window.GetVisualDescendants().OfType<RunPad.Avalonia.HtmlRenderer.HtmlPanel>())
        {
            var bar = (Avalonia.Controls.Primitives.ScrollBar?)typeof(RunPad.Avalonia.HtmlRenderer.HtmlPanel)
                .GetField("_verticalScrollBar", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(panel);
            if (bar is null) continue;
            bar.Value = 0;
            typeof(RunPad.Avalonia.HtmlRenderer.HtmlPanel).GetMethod("UpdateScrollOffsets", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(panel, null);
        }
    }

    public static async Task Caret(EditorsTabItemControl ctrl, int line, int column)
    {
        var editor = ctrl.State.Editor;
        editor.TextArea.Focus();
        editor.TextArea.Caret.Position = new TextViewPosition(line, column);
        editor.TextArea.Caret.BringCaretToView();
        await Pump(800);
    }

    private static async Task Overview()
    {
        await using var c = await Ctx.Create(1100, 720, p => p.Layout.DebugPanelHeight = 330);
        AddTree(c);
        var path = c.AddExample("products-overview.rpad");
        var (tab, ctrl) = await c.Open(path);
        await c.RunAndWait(tab);
        await Caret(ctrl, 10, 1);
        ScrollResultsHome(c);
        await Pump(500);
        Capture(c.Window, "overview.png");
    }

    private const string EditorCode = """
        ---
        version: "1.0"
        language: csharp
        targetSdk: latest
        kind: Statements
        ---

        ```csharp run
        // Word frequencies in a short text
        var text = "the quick brown fox jumps over the lazy dog the end";

        var counts = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .GroupBy(word => word)
            .Select(g => new WordCount(g.Key, g.Count()))
            .OrderByDescending(w => w.Count)
            .ThenBy(w => w.Word);

        foreach (var (word, count) in counts.Take(3))
        {
            Console.WriteLine($"{word,-6} {count}");
        }

        counts.Dump("All words");

        record WordCount(string Word, int Count);
        ```
        """;

    private static async Task Editor()
    {
        // Inlay hints off: while ".Ta" is unfinished, the hint for counts would read "?".
        await using var c = await Ctx.Create(1100, 640, p => p.CSharp.InlayHintsEnabled = false);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddScript("word-counts.rpad", EditorCode));
        // Hide the output panel: this one is about the code.
        c.Tabs.ToggleOutputPanelActive();
        await Pump(3000);
        // Continue the query on a new line to open completion: ".ThenBy(w => w.Word)" + "\n    .Ta;".
        var doc = ctrl.State.Editor.Document;
        var line9 = doc.GetLineByNumber(9);
        doc.Insert(line9.EndOffset - 1, "\n    ");
        await Pump(500);
        await Caret(ctrl, 10, 5);
        c.Window.KeyTextInput(".");
        await Pump(2500);
        c.Window.KeyTextInput("Ta");
        await Pump(3000);
        Capture(c.Window, "editor-highlighting.png");
    }

    private static async Task Search()
    {
        await using var c = await Ctx.Create(1100, 560);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddScript("word-counts.rpad", EditorCode));
        c.Tabs.ToggleOutputPanelActive();
        await Caret(ctrl, 4, 1);
        c.Window.KeyPress(Key.H, RawInputModifiers.Control, PhysicalKey.H, "h");
        await Pump(800);
        c.Window.KeyTextInput("word");
        await Pump(1200);
        Capture(c.Window, "editor-search.png");
    }

    private static async Task Diagnostics()
    {
        await using var c = await Ctx.Create(1100, 640, p => { p.Experimental.RunPadAnalyzers = true; p.CSharp.InlayHintsEnabled = false; });
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddExample("analyzer-diagnostics.rpad"));
        c.Tabs.ToggleOutputPanelActive();
        await Pump(4000);
        // The caret on the unknown JsonSerializer, and its lightbulb menu open.
        await Caret(ctrl, 1, 20);
        await Pump(3000);
        var margin = ctrl.State.Editor.TextArea.LeftMargins.OfType<RunPad.Avalonia.CodeTextEditor.CodeFixMargin>().First();
        margin.ShowFixMenu(margin.Fixes);
        await Pump(1500);
        Capture(c.Window, "diagnostics.png");
    }

    private static async Task Debugging()
    {
        await using var c = await Ctx.Create(1100, 720);
        AddTree(c);
        var (tab, ctrl) = await c.Open(Path.Combine(c.Root, "debugging-loop.rpad"));
        tab.DebugModeEnabled = true;
        ctrl.State.Editor.AddBreakpoint(5);
        await Pump(300);
        c.Tabs.RunOrContinueActive();
        await WaitUntil(() => tab.IsPaused, 120000, "first stop");
        await Pump(1000);
        // Continue to the third iteration, so the values tell a story.
        c.Tabs.RunOrContinueActive();
        await WaitUntil(() => !tab.IsPaused, 20000, "resume");
        await WaitUntil(() => tab.IsPaused, 20000, "second stop");
        c.Tabs.RunOrContinueActive();
        await WaitUntil(() => !tab.IsPaused, 20000, "resume");
        await WaitUntil(() => tab.IsPaused, 20000, "third stop");
        await Pump(1500);
        await Caret(ctrl, 5, 9);
        await Pump(1000);
        Capture(c.Window, "debugging.png");
        c.Tabs.CancelActive();
        await WaitUntil(() => tab.EnableRunButton, 30000, "stop");
    }

    private static async Task Preview()
    {
        await using var c = await Ctx.Create(1280, 720);
        AddTree(c);
        var (tab, ctrl) = await c.Open(c.AddExample("products-overview.rpad"));
        c.Tabs.ToggleOutputPanelActive();
        tab.TogglePreviewToSide();
        await Pump(2500);
        await Caret(ctrl, 10, 1);
        Capture(c.Window, "markdown-preview.png");
    }

    private static async Task Preferences()
    {
        // The default: the recovery store is a temporary folder here anyway.
        await using var c = await Ctx.Create(1100, 700, p => p.Appearance.RestoreUnsavedTabs = true);
        var window = c.Services.GetRequiredService<PreferencesWindow>();
        window.Width = 820;
        window.Height = 560;
        window.Show(c.Window);
        await Pump(1500);
        Capture(window, "preferences.png");
        window.Close();
    }

    private static async Task Dependencies()
    {
        await using var c = await Ctx.Create(1100, 700);
        var (tab, ctrl) = await c.Open(c.AddExample("quick-start.rpad"));
        var window = c.Services.GetRequiredService<DependenciesWindow>();
        window.Width = 1100;
        window.Height = 680;
        window.SetFrontMatter(ctrl.State.RpadDocument?.FrontMatter);
        window.Show(c.Window);
        await Pump(1500);
        var vm = (DependenciesWindowViewModel)window.DataContext!;
        vm.LocalFilter = "Humanizer";
        vm.SearchText = "Humanizer";
        await WaitUntil(() => vm.OnlinePackages.Count > 0, 60000, "online results");
        await Pump(1000);
        vm.SelectedOnlinePackage = vm.OnlinePackages[0];
        await Pump(5000);
        Capture(window, "dependencies.png");
        window.Close();
    }

    private static async Task Explorer()
    {
        await using var c = await Ctx.Create(1100, 700);
        var ws = c.Services.GetRequiredService<IWindowService>();
        var window = new AssemblyExplorerWindow(c.Services.GetRequiredService<IThemeService>(), ws, c.Root)
        {
            Preferences = c.Prefs, Width = 1100, Height = 720,
        };
        window.Show(c.Window);
        await Pump(1000);
        var linq = typeof(Enumerable).Assembly.Location;
        await window.LoadAssemblyAsync(linq);
        await Pump(1000);
        await window.SearchAsync("Enumerable.Where");
        await Pump(1500);
        var first = window.SearchResultList.Items.OfType<AssemblyTreeNode>().First();
        window.SearchResultList.SelectedItem = first;
        await window.OpenSearchResultAsync(first);
        await Pump(3000);
        Capture(window, "assembly-explorer.png");
        window.Close();
    }
}
