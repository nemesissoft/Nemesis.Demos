using System.Collections;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using ICSharpCode.Decompiler.CSharp;
using Nemesis.Demos.Highlighters;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Nemesis.Demos;

public abstract class Runnable(DemoRunner demo)
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

    public void HighlightCode(string source, Language language = Language.CSharp)
    {
        try
        {
            AnsiConsole.Markup(demo.HighlighterFactory.GetSyntaxHighlighter(language).GetHighlightedMarkup(source));
        }
        catch (Exception) { AnsiConsole.WriteLine(source); }
    }

    public void HighlightDecompiledCSharp(string methodName, params LanguageVersion[] languageVersions)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + Decompiler.DecompileAsCSharp(methodName, defaultVersion));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + Decompiler.DecompileAsCSharp(methodName, version));
    }

    public void HighlightDecompiledCSharp(MethodInfo method, params LanguageVersion[] languageVersions)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + Decompiler.DecompileAsCSharp(method, defaultVersion));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + Decompiler.DecompileAsCSharp(method, version));
    }

    public void HighlightDecompiledCSharp(Type type, params LanguageVersion[] languageVersions)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + Decompiler.DecompileAsCSharp(type, defaultVersion));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + Decompiler.DecompileAsCSharp(type, version));
    }

    private static string GetComment(LanguageVersion version) => $"//Decompiled using C# version {version}{Environment.NewLine}";


    public T Dump<T>(T source, string? title = null)
    {
        try
        {
            var renderable = ToRenderable(source);
            if (title != null)
                AnsiConsole.MarkupLine($"[bold underline]{Markup.Escape(title)}[/]");

            AnsiConsole.Write(renderable);
            AnsiConsole.WriteLine();
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine(source?.ToString() ?? "");
        }

        return source;
    }

    private IRenderable ToRenderable(object? obj)
    {
        var theme = demo.DemoOptions.Theme;

        if (obj is null)
            return new Markup("[grey italic]null[/]");

        if (obj is JsonObject json)
            return new Markup(demo.HighlighterFactory.GetSyntaxHighlighter(Language.Json).GetHighlightedMarkup(json.ToString()));

        if (obj is XNode xml)
            return new Markup(demo.HighlighterFactory.GetSyntaxHighlighter(Language.Xml).GetHighlightedMarkup(xml.ToString()));

        static Markup GetFormattableMarkup(IFormattable formattable, string? format, string? style = null) =>
            new(
                (style is null ? "" : $"[{style}]") +
                Markup.Escape(formattable.ToString(format, CultureInfo.InvariantCulture)) +
                (style is null ? "" : "[/]")
               );


        // Temporals
        if (obj is DateTime or DateTimeOffset or DateOnly or TimeOnly)
            return GetFormattableMarkup((IFormattable)obj, "O");

        if (obj is TimeSpan ts)
            return GetFormattableMarkup(ts, "c");

        // Primitive / simple

        if (obj is string text)
            return new Markup($"[{theme.String}]\"{Markup.Escape(text)}\"[/]");

        if (obj is char c)
            return new Markup($"[{theme.String}]'{Markup.Escape(c.ToString())}'[/]");

        if (obj is bool b)
            return new Markup($"[{theme.Keyword}]{(b ? "true" : "false")}[/]");

        if (obj is byte or sbyte or
                   short or ushort or
                   int or uint or
                   long or ulong or
                   nint or nuint or
                   Half or float or double or decimal or
                   Int128 or UInt128)
            return GetFormattableMarkup((IFormattable)obj, null, theme.Number);

        if (obj is IFormattable formattable)
            return GetFormattableMarkup(formattable, null);



        var type = obj.GetType();

        // Enums
        if (type.IsEnum)
            return new Markup($"[{theme.Type}]{obj}[/]");

        // 2D arrays (rectangular)
        if (obj is Array arr && arr.Rank == 2)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            var table = new Table { Border = TableBorder.MinimalHeavyHead, ShowHeaders = false };

            for (int x = 0; x < cols; x++)
                table.AddColumn("");

            for (int y = 0; y < rows; y++)
            {
                var cells = new List<IRenderable>(cols);
                for (int x = 0; x < cols; x++)
                    cells.Add(ToRenderable(arr.GetValue(y, x)));
                table.AddRow(cells);
            }

            return table;
        }

        // 2D arrays (jagged)
        if (obj is Array outer && outer.Rank == 1 && outer.Length > 0 && outer.GetValue(0) is Array)
        {
            // Get max column count to make columns consistent
            var innerArrays = outer.Cast<object?>()
                .Select(x => x as Array)
                .Where(x => x is not null)
                .ToArray();

            int rows = innerArrays.Length;
            int cols = innerArrays.Max(a => a!.Length);

            var table = new Table { Border = TableBorder.MinimalHeavyHead, ShowHeaders = false };

            for (int x = 0; x < cols; x++)
                table.AddColumn("");

            for (int y = 0; y < rows; y++)
            {
                var row = new List<IRenderable>(cols);
                var inner = innerArrays[y]!;

                for (int x = 0; x < cols; x++)
                {
                    object? val = x < inner.Length ? inner.GetValue(x) : null;
                    row.Add(ToRenderable(val));
                }

                table.AddRow(row);
            }

            return table;
        }

        // IDictionary
        if (obj is IDictionary dict)
        {
            var table = new Table()
            {
                Border = TableBorder.Rounded,
                Title = new TableTitle($"[{theme.PlainText}]Dictionary<{type.GenericTypeArguments[0].Name}, {type.GenericTypeArguments[1].Name}>[/]")
            }
                .AddColumns("Key", "Value");

            foreach (DictionaryEntry entry in dict)
                table.AddRow(ToRenderable(entry.Key), ToRenderable(entry.Value));

            return table;
        }

        // IEnumerable (but not string)
        if (obj is IEnumerable enumerable)
        {
            var table = new Table()
            {
                Border = TableBorder.Rounded,
                Title = new TableTitle($"[{theme.PlainText}]Enumerable<{type.GetElementType()?.Name ?? type.GenericTypeArguments.FirstOrDefault()?.Name ?? "object"}>[/]")
            }
                .AddColumns("Index", "Value");

            int i = 0;
            foreach (var item in enumerable)
                table.AddRow(new Markup($"[grey]{i++}[/]"), ToRenderable(item));

            return table;
        }

        // Complex object → show public properties/fields
        var members = (
              type.IsGenericType && typeof(ITuple).IsAssignableFrom(type) && type.GetGenericTypeDefinition().FullName!.StartsWith("System.ValueTuple`", StringComparison.Ordinal)
                ? (IEnumerable<MemberInfo>)type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                : type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
            ).ToArray();

        var objectTable = new Table()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle($"[{theme.PlainText}]{type.Name}[/]")
        }
            .AddColumns("Property", "Value");
        foreach (var m in members)
        {
            try
            {
                var value = m is PropertyInfo pi ? pi.GetValue(obj) : ((FieldInfo)m).GetValue(obj);
                objectTable.AddRow(
                    new Markup($"[bold]{m.Name}[/]"),
                    ToRenderable(value)
                );
            }
            catch (Exception ex)
            {
                objectTable.AddRow(
                    new Markup($"[bold]{m.Name}[/]"),
                    new Markup($"[red]{ex.Message}[/]")
                );
            }
        }
        return objectTable;
    }
}

public abstract class RunnableAsync(DemoRunner demo) : Runnable(demo)
{
    public abstract Task RunAsync();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int value) : Attribute { public int Value => value; }