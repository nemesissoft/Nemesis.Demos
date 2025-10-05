using Spectre.Console;

namespace Nemesis.Demos;

public abstract class Runnable
{
    public abstract void Run();

    public virtual string Description => GetType().Name;

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
}

public abstract class RunnableAsync : Runnable
{
    public abstract Task RunAsync();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int value) : Attribute { public int Value => value; }