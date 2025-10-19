using System.Text;
using System.Xml.Linq;

namespace Nemesis.Demos.Highlighters;

internal sealed class XmlHighlighter(DemoOptions Options) : MarkupSyntaxHighlighter
{
    public override string GetHighlightedMarkup(string code)
    {
        var doc = XDocument.Parse(code, LoadOptions.PreserveWhitespace);
        var sb = new StringBuilder();
        AppendElement(sb, doc.Root!, Options.Theme, indent: 0);
        return sb.AppendLine().ToString();
    }

    private static void AppendComment(StringBuilder sb, XComment comment, SyntaxTheme theme, string indentStr)
    {
        sb.AppendLine($"{indentStr}[{theme.Comment}]<!-- {Escape(comment.Value)} -->[/]");
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
            sb.Append($"[{theme.Type}]{Escape(attr.Name.LocalName)}[/]");
            sb.Append($"[{theme.PlainText}]=[/]");
            sb.Append($"[{theme.String}]\"{Escape(attr.Value)}\"[/]");
        }

        if (!element.HasElements && !element.Nodes().OfType<XComment>().Any() && string.IsNullOrEmpty(element.Value))
        {
            // Self-closing tag
            sb.Append($"[{theme.PlainText}] />[/]");
            sb.AppendLine();
            return;
        }

        sb.Append($"[{theme.PlainText}]>[/]");

        // Inner content: Iterate over ALL child nodes (Elements, Comments, Text)
        var innerNodes = element.Nodes().ToList();

        if (innerNodes.Count > 0)
        {
            // If the element contains anything (elements, text, or comments), we add a newline
            sb.AppendLine();

            // The existing indentation logic assumes child elements should be indented.
            // We use the same approach here for children and comments.
            string childIndentStr = new(' ', (indent + 1) * 2);

            foreach (var node in innerNodes)
            {
                if (node is XComment comment)
                {
                    AppendComment(sb, comment, theme, childIndentStr);
                }
                else if (node is XElement childElement)
                {
                    // Recursively call for child elements
                    AppendElement(sb, childElement, theme, indent + 1);
                }
                else if (node is XText textNode)
                {
                    // This handles text content, including text nodes that only contain whitespace.
                    // We only highlight and append non-empty, non-whitespace text nodes.
                    string value = textNode.Value.Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        sb.Append(childIndentStr);
                        sb.Append($"[{theme.String}]{Escape(value)}[/]");
                        sb.AppendLine();
                    }
                }
            }
            sb.Append(indentStr); // Indent for the closing tag
        }
        // If there are no elements or comments, but there is simple text content (e.g., <tag>text</tag>)
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