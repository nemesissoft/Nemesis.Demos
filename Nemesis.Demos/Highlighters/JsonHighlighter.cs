using System.Text;
using System.Text.Json;
using Spectre.Console;

namespace Nemesis.Demos.Highlighters;

internal sealed class JsonHighlighter(DemoOptions Options) : IMarkupSyntaxHighlighter
{
    public string GetHighlightedMarkup(string code)
    {
        using var doc = JsonDocument.Parse(code);
        var sb = new StringBuilder();
        AppendValue(sb, doc.RootElement, Options.Theme, indent: 0);
        return sb.AppendLine().ToString();
    }

    private static void AppendValue(StringBuilder sb, JsonElement element, SyntaxTheme theme, int indent)
    {
        string indentStr = new(' ', indent * 2);

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                sb.AppendLine($"[{theme.PlainText}]{{[/]");
                foreach (var prop in element.EnumerateObject())
                {
                    sb.Append(indentStr + "  ");
                    sb.Append($"[{theme.Type}]\"{Markup.Escape(prop.Name)}\"[/]");
                    sb.Append($"[{theme.PlainText}]:[/] ");
                    AppendValue(sb, prop.Value, theme, indent + 1);
                    sb.AppendLine();
                }
                sb.Append(indentStr);
                sb.Append($"[{theme.PlainText}]}}[/]");
                break;

            case JsonValueKind.Array:
                sb.AppendLine($"[{theme.PlainText}][[[/]");
                foreach (var item in element.EnumerateArray())
                {
                    sb.Append(indentStr + "  ");
                    AppendValue(sb, item, theme, indent + 1);
                    sb.AppendLine();
                }
                sb.Append($"{indentStr}[{theme.PlainText}]]][/]");
                break;

            case JsonValueKind.String:
                sb.Append($"[{theme.String}]\"{Markup.Escape(element.GetString()!)}\"[/]");
                break;

            case JsonValueKind.Number:
                sb.Append($"[{theme.Number}]{element.GetRawText()}[/]");
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                sb.Append($"[{theme.Keyword}]{element.GetRawText()}[/]");
                break;

            case JsonValueKind.Null:
                sb.Append($"[{theme.Keyword}]null[/]");
                break;
        }
    }
}