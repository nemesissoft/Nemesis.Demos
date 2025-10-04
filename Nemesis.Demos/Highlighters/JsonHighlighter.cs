using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nemesis.Demos.Highlighters;

internal sealed class JsonHighlighter(DemosOptions Options) : MarkupSyntaxHighlighter
{
    public override string GetHighlightedMarkup(string json)
    {
        using var doc = JsonDocument.Parse(json);
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
                sb.AppendLine($"{indentStr}[{theme.PlainText}]{{[/]");
                foreach (var prop in element.EnumerateObject())
                {
                    sb.Append(indentStr + "  ");
                    sb.Append($"[{theme.String}]\"{Escape(prop.Name)}\"[/]");
                    sb.Append($" [{theme.PlainText}]:[/] ");
                    AppendValue(sb, prop.Value, theme, indent + 1);
                    sb.AppendLine();
                }
                sb.Append(indentStr);
                sb.Append($"[{theme.PlainText}]}}[/]");
                break;

            case JsonValueKind.Array:
                sb.AppendLine($"{indentStr}[{theme.PlainText}]\\[[/]");
                foreach (var item in element.EnumerateArray())
                {
                    AppendValue(sb, item, theme, indent + 1);
                    sb.AppendLine();
                }
                sb.Append(indentStr);
                sb.Append($"[{theme.PlainText}]\\][/]");
                break;

            case JsonValueKind.String:
                sb.Append($"[{theme.String}]\"{Escape(element.GetString()!)}\"[/]");
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