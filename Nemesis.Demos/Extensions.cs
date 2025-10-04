using System.Text;
using System.Text.Json.Nodes;
using Nemesis.TextParsers;
using Spectre.Console;

namespace Nemesis.Demos;

public static class Extensions
{
    public static T Dump<T>(this T source, string? prepend = null)
    {
        var store = TextTransformer.Default;
        string text = source switch
        {
            null => "<null>",
            var _ when store.IsSupportedForTransformation(typeof(T))
                => store.GetTransformer<T>().Format(source),
            _ => GetString(store, source)
        };

        AnsiConsole.WriteLine($"{prepend}{text}");
        return source;
    }

    private static string GetString(ITransformerStore store, object? obj)
    {
        if (obj is null) return "<null>";

        var type = obj.GetType();
        if (store.IsSupportedForTransformation(type))
            return store.GetTransformer(type).FormatObject(obj);
        else if (obj is JsonObject jo)
            return jo.ToString();
        else
        {
            if (type.GetProperties() is { } props && props.Length > 0)
            {
                var sb = new StringBuilder();
                foreach (var p in props)
                {
                    var value = p.GetValue(obj);
                    var @break = store.IsSupportedForTransformation(p.PropertyType) ? " " : Environment.NewLine;
                    var text = $"{p.Name}:{@break}{GetString(store, value)}";
                    text = string.Join(Environment.NewLine,
                        text.Split([Environment.NewLine, "\n", "\r"], StringSplitOptions.None)
                        .Select(line => $"\t{line}")
                    );
                    sb.AppendLine(text);
                }
                return sb.ToString();
            }
            else return obj.ToString() ?? "<null>";
        }
    }

    public static Span<T> Dump<T>(this Span<T> source, string? prepend = null)
        => Dump(source.ToArray(), prepend);

    public static ReadOnlySpan<T> Dump<T>(this ReadOnlySpan<T> source, string? prepend = null)
        => Dump(source.ToArray(), prepend);

    public static Span<TNumber> DumpBinary<TNumber>(this Span<TNumber> source, string? prepend = null)
            where TNumber : IBinaryInteger<TNumber>, IFormattable
    {
        Dump(
            string.Join(", ", source.ToArray().Select(b => b.ToString("B8", CultureInfo.InvariantCulture))),
            prepend
            );
        return source;
    }
}