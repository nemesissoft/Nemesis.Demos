using System.Text.RegularExpressions;

namespace Nemesis.Demos.Highlighters;

internal partial class MsilHighlighter(DemoOptions Options) : MarkupSyntaxHighlighter
{
    public override string GetHighlightedMarkup(string code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        var lines = code.Split(["\r\n", "\n"], StringSplitOptions.None);
        var result = new System.Text.StringBuilder();

        var theme = Options.Theme;

        // Common MSIL keywords
        string[] keywords = ["ldarg\\.\\d+", "ldloc\\.\\d+", "stloc\\.\\d+", "call", "callvirt", "ret", "newobj", "br", "brtrue", "brfalse", "ldstr", "ldc\\.(i4|r8)?", "nop"];

        var keywordRegex = new Regex(@"\b(" + string.Join("|", keywords) + @")\b");

        foreach (var line in lines)
        {
            string processed = line;

            // Escape brackets for Spectre.Console markup
            processed = Escape(processed);

            // Separate comment part (// ...) from code
            string commentPart = "";
            int commentIndex = processed.IndexOf("//");
            if (commentIndex >= 0)
            {
                commentPart = processed.Substring(commentIndex);
                processed = processed.Substring(0, commentIndex);
            }

            // Highlight strings first to avoid conflicts
            processed = StringPattern.Replace(processed, m => $"[{theme.String}]{m.Value}[/]");

            // Highlight keywords
            processed = keywordRegex.Replace(processed, m => $"[{theme.Keyword}]{m.Value}[/]");

            // Highlight numbers
            processed = NumberPattern.Replace(processed, m => $"[{theme.Number}]{m.Value}[/]");

            // Highlight type names (words starting with uppercase)
            processed = TypePattern.Replace(processed, m => $"[{theme.Type}]{m.Value}[/]");

            // Add back comment, highlighted
            if (!string.IsNullOrEmpty(commentPart))
                processed += $"[{theme.Comment}]{commentPart}[/]";

            result.AppendLine(processed);
        }

        return result.ToString();
    }

    [GeneratedRegex(@"\b\d+(\.\d+)?\b")]
    private static partial Regex NumberPattern { get; }

    [GeneratedRegex("\"[^\"]*\"")]
    private static partial Regex StringPattern { get; }

    [GeneratedRegex(@"\b[A-Z][a-zA-Z0-9_\.]*\b")]
    private static partial Regex TypePattern { get; }
}
