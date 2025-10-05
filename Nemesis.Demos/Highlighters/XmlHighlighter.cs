using System.Text;
using System.Xml.Linq;

namespace Nemesis.Demos.Highlighters;

internal sealed class XmlHighlighter(DemosOptions Options) : MarkupSyntaxHighlighter
{
    public override string GetHighlightedMarkup(string code)
    {
        var doc = XDocument.Parse(code);
        var sb = new StringBuilder();
        AppendElement(sb, doc.Root!, Options.Theme, indent: 0);
        return sb.AppendLine().ToString();
    }

    private static void AppendElement(StringBuilder sb, XElement element, SyntaxTheme theme, int indent)
    {
        string indentStr = new(' ', indent * 2);

        // Opening tag
        sb.Append(indentStr);


        sb.Append($"[{theme.PlainText}]<[/]");
        sb.Append($"[{theme.Keyword}]{Escape(element.Name.LocalName)}[/]");

        // Attributes
        foreach (var attr in element.Attributes())
        {
            sb.Append(' ');
            sb.Append($"[{theme.Name}]{Escape(attr.Name.LocalName)}[/]");
            sb.Append($"[{theme.PlainText}]=[/]");
            sb.Append($"[{theme.String}]\"{Escape(attr.Value)}\"[/]");
        }

        if (!element.HasElements && string.IsNullOrEmpty(element.Value))
        {
            sb.Append($"[{theme.PlainText}] />[/]");
            sb.AppendLine();
            return;
        }

        sb.Append($"[{theme.PlainText}]>[/]");

        // Inner text or children
        if (element.HasElements)
        {
            sb.AppendLine();
            foreach (var child in element.Elements())
                AppendElement(sb, child, theme, indent + 1);
            sb.Append(indentStr);
        }
        else if (!string.IsNullOrEmpty(element.Value))
        {
            sb.Append($"[{theme.String}]{Escape(element.Value)}[/]");
        }

        // Closing tag
        sb.Append($"[{theme.PlainText}]</[/]");
        sb.Append($"[{theme.Keyword}]{Escape(element.Name.LocalName)}[/]");
        sb.Append($"[{theme.PlainText}]>[/]");
        sb.AppendLine();
    }
}