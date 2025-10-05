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

        static Markup GetFormattableMarkup(IFormattable formattable, string? format) =>
            new(Markup.Escape(formattable.ToString(format, CultureInfo.InvariantCulture)));


        // Temporals
        if (obj is DateTime or DateTimeOffset or DateOnly or TimeOnly)
            return GetFormattableMarkup((IFormattable)obj, "O");

        if (obj is TimeSpan ts)
            return GetFormattableMarkup(ts, "c");

        // Primitive / simple
        if (obj is IFormattable formattable)
            return GetFormattableMarkup(formattable, null);

        if (obj is string text)
            return new Markup($"[{theme.String}]\"{Markup.Escape(text)}\"[/]");

        if (obj is bool b)
            return new Markup($"[{theme.Keyword}]{(b ? "true" : "false")}[/]");

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
                Title = new TableTitle($"[bold]Dictionary<{type.GenericTypeArguments[0].Name}, {type.GenericTypeArguments[1].Name}>[/]")
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
                Title = new TableTitle($"[bold]Enumerable<{type.GetElementType()?.Name ?? type.GenericTypeArguments.FirstOrDefault()?.Name ?? "object"}>[/]")
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
            Title = new TableTitle($"[bold]{type.Name}[/]")
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

    public Span<T> Dump<T>(Span<T> source, string? title = null) => Dump(source.ToArray(), title);

    public ReadOnlySpan<T> Dump<T>(ReadOnlySpan<T> source, string? title = null) => Dump(source.ToArray(), title);

    public Span<TNumber> DumpBinary<TNumber>(Span<TNumber> source, string? title = null) where TNumber : IBinaryInteger<TNumber>, IFormattable
    {
        Dump(
            string.Join(", ", source.ToArray().Select(b => b.ToString("B8", CultureInfo.InvariantCulture))),
            title
            );
        return source;
    }
}
