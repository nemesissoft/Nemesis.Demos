using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Nemesis.Demos;

public class SyntaxHighlighter(DemosOptions Options)
{
    public static SyntaxNode GetParsedCodeRoot(string code) => CSharpSyntaxTree.ParseText(code).GetRoot();

    public string GetHighlightedMarkup(string code) => GetHighlightedMarkup(GetParsedCodeRoot(code));

    public string GetHighlightedMarkup(SyntaxNode root)
    {
        var sb = new StringBuilder();

        // Include all tokens AND their leading/trailing trivia
        foreach (var token in root.DescendantTokens())
        {
            // Leading trivia (comments, whitespace, newlines)
            foreach (var trivia in token.LeadingTrivia)
                AppendTrivia(sb, trivia, Options.Theme);

            // Token itself
            string color = GetColor(token, Options.Theme);
            sb.Append(EscapeSpectreMarkup(token.Text, color));

            // Trailing trivia
            foreach (var trivia in token.TrailingTrivia)
                AppendTrivia(sb, trivia, Options.Theme);
        }

        return sb.ToString();
    }

    private static void AppendTrivia(StringBuilder sb, SyntaxTrivia trivia, SyntaxTheme theme)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.WhitespaceTrivia:
                sb.Append(trivia.ToFullString());
                break;
            case SyntaxKind.EndOfLineTrivia:
                sb.AppendLine();
                break;
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
                sb.Append($"[{theme.Comment}]{EscapeSpectreMarkup(trivia.ToFullString())}[/]");
                break;
            default:
                sb.Append(trivia.ToFullString());
                break;
        }
    }

    private static string GetColor(SyntaxToken token, SyntaxTheme theme)
    {
        if (token.IsKeyword()) return theme.Keyword;

        if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.CharacterLiteralToken)) return theme.String;

        if (token.IsKind(SyntaxKind.NumericLiteralToken)) return theme.Number;

        if (token.Parent is PredefinedTypeSyntax or IdentifierNameSyntax) return theme.Type;

        return theme.PlainText;
    }

    private static string EscapeSpectreMarkup(string text) => text.Replace("[", "[[").Replace("]", "]]");

    private static string EscapeSpectreMarkup(string text, string color) => $"[{color}]{EscapeSpectreMarkup(text)}[/]";
}
