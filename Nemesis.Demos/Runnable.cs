using ICSharpCode.Decompiler.CSharp;
using Nemesis.Demos.Highlighters;
using Spectre.Console;

namespace Nemesis.Demos;

public abstract partial class Runnable(DemoRunner demo, string? group = null, int? order = null)
{
    public string Group { get; } = group ?? "Demos";
    public int Order { get; } = order ?? int.MaxValue;
    public virtual string Description => GetType().Name;

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

    public void HighlightDecompiledCSharp(string methodName, LanguageVersion[]? languageVersions = null, object? instanceOrType = null) =>
        HighlightDecompiledCSharp(GetMethod(methodName, instanceOrType), languageVersions);

    public void HighlightDecompiledCSharp(MethodInfo method, LanguageVersion[]? languageVersions = null)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions is null || languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + demo.Decompiler.DecompileAsCSharp(method, defaultVersion));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + demo.Decompiler.DecompileAsCSharp(method, version));
    }

    public void HighlightDecompiledCSharp(Type type, LanguageVersion[]? languageVersions = null)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions is null || languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + demo.Decompiler.DecompileAsCSharp(type, defaultVersion));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + demo.Decompiler.DecompileAsCSharp(type, version));
    }

    private static string GetComment(LanguageVersion version) => $"//Decompiled using C# version {version}{Environment.NewLine}";

    public void HighlightDecompiledMsil(string methodName, object? instanceOrType = null) =>
        HighlightDecompiledMsil(GetMethod(methodName, instanceOrType));

    public void HighlightDecompiledMsil(MethodInfo method)
    {
        try
        {
            var msil = demo.Decompiler.DecompileAsMsil(method);
            HighlightCode(msil, Language.Msil);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine($"Method '{method.Name}' cannot be disassembled to MSIL");
        }
    }

    public void HighlightDecompiledMsil(Type type)
    {
        try
        {
            var msil = demo.Decompiler.DecompileAsMsil(type);
            HighlightCode(msil, Language.Msil);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine($"Type '{type.Name}' cannot be disassembled to MSIL");
        }
    }

    private static MethodInfo GetMethod(string methodName, object? instanceOrType = null)
    {
        Type type = instanceOrType switch
        {
            Type t => t,
            { } obj => obj.GetType(),
            null => GetTypeFromStackTrace() ?? throw new InvalidOperationException($"Cannot determine declaring type for {methodName}")
        };

        var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        return methodInfo ?? throw new ArgumentException($"Method '{methodName}' not found in type '{type.AssemblyQualifiedName}'", nameof(methodName));

        static Type? GetTypeFromStackTrace()
        {
            var demosNamespace = typeof(Decompiler).Namespace ?? throw new InvalidOperationException("Demos namespace cannot be determined");
            var stack = new StackTrace();
            return stack.GetFrames()?
                .Select(f => f.GetMethod()?.DeclaringType)
                .FirstOrDefault(t => t is not null
                            && t.Namespace is not null
                            && !t.Namespace.StartsWith(demosNamespace));
        }
    }
}