using System.Collections;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Nemesis.Demos.Highlighters;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Nemesis.Demos;

public partial class Runnable
{
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

        if (obj is IValueWrapper valueWrapper)
            return ToRenderable(valueWrapper.Value);

        if (obj is JsonNode json)
            return new Markup(demo.HighlighterFactory.GetSyntaxHighlighter(Language.Json).GetHighlightedMarkup(json.ToString()));

        if (obj is XNode xml)
            return new Markup(demo.HighlighterFactory.GetSyntaxHighlighter(Language.Xml).GetHighlightedMarkup(xml.ToString()));

        if (obj is Exception ex)
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);

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

        //System.IO
        if (obj is FileSystemInfo fsi)
            return new TextPath(fsi.FullName)
            {
                RootStyle =
                    fsi.Exists ? new Style(Color.Green) : new Style(Color.Red, null, Decoration.Strikethrough),
                SeparatorStyle =
                    fsi.Exists ? new Style(Color.GreenYellow) : new Style(Color.Red, null, Decoration.Strikethrough),
                StemStyle =
                    fsi.Exists ? new Style(Color.DarkOrange) : new Style(Color.Red, null, Decoration.Strikethrough),
                LeafStyle =
                    fsi.Exists ? new Style(Color.Orange1) : new Style(Color.Red, null, Decoration.Strikethrough),
            };

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
        if (obj is IEnumerable e && !e.Cast<object>().Any())
            return new Text("[]");
        if (obj.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) is { } @interface &&
            @interface.GetGenericArguments()[0] is { } t &&
            (t == typeof(string) || t == typeof(char) || t == typeof(bool) || typeof(INumber<>).MakeGenericType(t).IsAssignableFrom(t))
        )
        {
            var renderedElements = ((IEnumerable)obj).Cast<object>().Select(o => ToRenderable(o)).ToList();

            List<IRenderable> combined = [new Text("[")];
            for (int i = 0; i < renderedElements.Count; i++)
            {
                combined.Add(renderedElements[i]);
                if (i < renderedElements.Count - 1)
                    combined.Add(new Text(","));
            }
            combined.Add(new Text("]"));

            return new Columns(combined) { Expand = false };
        }
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

public interface IValueWrapper
{
    object Value { get; }
}