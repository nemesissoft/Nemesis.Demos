using System.Collections;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Nemesis.Demos.Highlighters;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Nemesis.Demos;
public partial class DemoRunner
{
    public T Dump<T>(T source, string? title = null)
    {
        var renderable = ToRenderable(source);
        if (title != null)
            AnsiConsole.MarkupLine($"[bold underline]{Markup.Escape(title)}[/]");

        AnsiConsole.Write(renderable);
        AnsiConsole.WriteLine();

        return source;
    }

    private IRenderable ToRenderable(object? obj)
    {
        var theme = _demosOptions.Theme;

        if (obj is null)
            return new Markup("[grey italic]null[/]");

        if (obj is JsonObject json)
            return new Markup(_highlighterFactory.GetSyntaxHighlighter(Language.Json).GetHighlightedMarkup(json.ToString()));

        if (obj is XNode xml)
            return new Markup(_highlighterFactory.GetSyntaxHighlighter(Language.Xml).GetHighlightedMarkup(xml.ToString()));

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

        // Complex object → show public properties
        var props = type.GetProperties()
            .Where(p => p.CanRead)
            .ToArray();

        var objectTable = new Table()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle($"[{theme.PlainText}]{type.Name}[/]")
        }
            .AddColumns("Property", "Value");
        foreach (var prop in props)
        {
            try
            {
                var value = prop.GetValue(obj);
                objectTable.AddRow(
                    new Markup($"[bold]{prop.Name}[/]"),
                    ToRenderable(value)
                );
            }
            catch (Exception ex)
            {
                objectTable.AddRow(
                    new Markup($"[bold]{prop.Name}[/]"),
                    new Markup($"[red]{ex.Message}[/]")
                );
            }
        }
        return objectTable;
    }
}
