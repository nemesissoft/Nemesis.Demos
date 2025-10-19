using Nemesis.Demos.Highlighters;
using Nemesis.Demos.Internals;
using Spectre.Console;

namespace Nemesis.Demos;

public abstract partial class Runnable(DemoRunner demo, string? group = null, int? order = null, string? description = null)
{
    public string Group { get; } = group ?? "Demos";
    public int Order { get; } = order ?? int.MaxValue;
    public string Description => description ?? GetType().Name;

    public virtual void Run() { }

    public virtual Task RunAsync() { return Task.CompletedTask; }

    public static IDisposable ForeColor(Color color) => new ConsoleColors.ForeColorStruct(color);

    public static IDisposable BackColor(Color color) => new ConsoleColors.BackColorStruct(color);

    /// <summary>
    /// Adds a horizontal rule line.
    /// </summary>
    /// <param name="text">Optional text to display inside the rule.</param>
    /// <param name="color">Optional color for the rule (default: white).</param>
    public static void DrawLine(string? text = null, Color? color = null)
    {
        var rule = text is null ? new Rule() : new Rule(text);
        rule.Style = new Style(color ?? Color.White);
        AnsiConsole.Write(rule);
    }

    public static void Section(string title) =>
            AnsiConsole.Write(new FigletText(
                FigletFontStore.GetFont("Basic"), title)
                .Color(Color.Orange1)
                .Centered()
    );

    public static void Subsection(string title) =>
            AnsiConsole.Write(new FigletText(
                FigletFontStore.GetFont("Ansi_Regular"), title)
                .Color(Color.DarkGreen)
                .LeftJustified()
    );

    public static void ExpectFailure<TException>(Action action, string? errorMessagePart = null,
        [CallerArgumentExpression(nameof(action))] string? actionText = null) where TException : Exception
    {
        try
        {
            action();
            AnsiConsole.MarkupLineInterpolated($"[bold maroon]Expected exception '{typeof(TException)}' not captured[/]");
        }
        //p⇒q ⟺ ¬(p ∧ ¬q)
        catch (TException e) when (!string.IsNullOrEmpty(errorMessagePart) && e.ToString().Contains(errorMessagePart, StringComparison.OrdinalIgnoreCase))
        {
            var lines = e.Message.Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.None)
                .Select(s => $"    {s}");
            AnsiConsole.MarkupLineInterpolated($"[bold fuchsia]EXPECTED with message for {actionText}:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}[/]");
        }
        catch (TException e)
        {
            var lines = e.Message.Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.None)
                .Select(s => $"    {s}");
            AnsiConsole.MarkupLineInterpolated($"[bold green]EXPECTED for {actionText}:{Environment.NewLine}{string.Join(Environment.NewLine, lines)}[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLineInterpolated($"[bold red]Failed to capture error for '{actionText}' containing '{errorMessagePart}' instead error was {e.GetType().FullName}: {e}[/]");
        }
    }

    public void HighlightCode(string source, Language language = Language.CSharp)
    {
        try
        {
            var markup = demo.HighlighterFactory.GetSyntaxHighlighter(language).GetHighlightedMarkup(source);
            AnsiConsole.Markup(markup);
            AnsiConsole.WriteLine();
        }
        catch (Exception) { AnsiConsole.WriteLine(source); AnsiConsole.WriteLine(); }
    }



    public static void RenderBenchmark(string csvText, Action<BenchmarkVisualizerOptions>? optionsBuilder = null)
    {
        using var reader = new StringReader(csvText);
        BenchmarkVisualizer.Render(reader, optionsBuilder);
    }

    public static void RenderBenchmark(TextReader csvReader, Action<BenchmarkVisualizerOptions>? optionsBuilder = null)
        => BenchmarkVisualizer.Render(csvReader, optionsBuilder);
}