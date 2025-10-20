using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
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


    public static void ExpectFailure<TException>(Action action, string? errorMessagePartOrPattern = null, [CallerArgumentExpression(nameof(action))] string? actionText = null) where TException : Exception
        => ExpectFailureAsync<TException>(() => { action(); return Task.CompletedTask; }, errorMessagePartOrPattern, actionText).GetAwaiter().GetResult();

    public static async Task ExpectFailureAsync<TException>(Func<Task> action, string? errorMessagePartOrPattern = null, [CallerArgumentExpression(nameof(action))] string? actionText = null) where TException : Exception
    {
        actionText ??= "<no action>";

        try
        {
            await action();
            AnsiConsole.MarkupLineInterpolated(
                $"[bold maroon]❌ Expected exception '{Markup.Escape(typeof(TException).Name)}' was not thrown in:[/] [grey]{Markup.Escape(actionText)}[/]");
        }
        catch (TException ex)
        {
            bool matches = string.IsNullOrWhiteSpace(errorMessagePartOrPattern) ||
                ex.Message.Contains(errorMessagePartOrPattern, StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(ex.Message, errorMessagePartOrPattern, RegexOptions.IgnoreCase);

            var lines = ex.Message
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"    [grey]{Markup.Escape(s)}[/]");

            if (matches)
            {
                AnsiConsole.MarkupLineInterpolated($"\n[bold green]✅ Expected {Markup.Escape(typeof(TException).Name)} caught:[/]");
                AnsiConsole.MarkupLineInterpolated($"[yellow]{Markup.Escape(actionText ?? "")}[/]");
                AnsiConsole.MarkupLine(string.Join(Environment.NewLine, lines));
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"\n[bold yellow]⚠️ {Markup.Escape(typeof(TException).Name)} caught, but message did not match expected part:[/]");
                AnsiConsole.MarkupLineInterpolated($"[grey]{Markup.Escape(errorMessagePartOrPattern ?? "(none)")}[/]");
                AnsiConsole.MarkupLine(string.Join(Environment.NewLine, lines));
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"\n[bold red]💥 Unexpected exception in '{Markup.Escape(actionText)}':[/]");
            AnsiConsole.MarkupLineInterpolated($"[bold red]{Markup.Escape(ex.GetType().FullName ?? "")}[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }


    }


    public void HighlightCsharp([StringSyntax("C#")] string source) => HighlightCode(source, Language.CSharp);
    public void HighlightMsil([StringSyntax("MSIL")] string source) => HighlightCode(source, Language.Msil);
    public void HighlightXml([StringSyntax(StringSyntaxAttribute.Xml)] string source) => HighlightCode(source, Language.Xml);
    public void HighlightJson([StringSyntax(StringSyntaxAttribute.Json)] string source) => HighlightCode(source, Language.Json);

    private void HighlightCode(string source, Language language)
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