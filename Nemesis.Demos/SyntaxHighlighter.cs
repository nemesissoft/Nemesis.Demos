using Spectre.Console;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nemesis.Demos;

public static class SyntaxHighlighter
{
    public static void Highlight(string code, SyntaxTheme? theme =null)
    {
        theme ??= SyntaxTheme.Dracula;

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        // Include all tokens AND their leading/trailing trivia
        foreach (var token in root.DescendantTokens())
        {
            // Leading trivia (comments, whitespace, newlines)
            foreach (var trivia in token.LeadingTrivia)
                WriteTrivia(trivia, theme);

            // Token itself
            string color = GetColor(token, theme);
            AnsiConsole.Markup(EscapeMarkup(token.Text, color));

            // Trailing trivia
            foreach (var trivia in token.TrailingTrivia)
                WriteTrivia(trivia, theme);
        }
        Console.WriteLine();
    }

    private static void WriteTrivia(SyntaxTrivia trivia, SyntaxTheme theme)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.WhitespaceTrivia:
                Console.Write(trivia.ToFullString());
                break;
            case SyntaxKind.EndOfLineTrivia:
                Console.WriteLine();
                break;
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
                AnsiConsole.Markup($"[{theme.Comment}]{EscapeMarkup(trivia.ToFullString())}[/]");
                break;
            default:
                Console.Write(trivia.ToFullString());
                break;
        }
    }

    private static string GetColor(SyntaxToken token, SyntaxTheme theme)
    {
        if (token.IsKeyword())
            return theme.Keyword;
        if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.CharacterLiteralToken))
            return theme.String;
        if (token.IsKind(SyntaxKind.NumericLiteralToken))
            return theme.Number;
        if (token.Parent is PredefinedTypeSyntax || token.Parent is IdentifierNameSyntax)
            return theme.Type;

        return theme.PlainText;
    }

    private static string EscapeMarkup(string text)
    {
        // Escape Spectre.Console markup characters
        return text.Replace("[", "[[").Replace("]", "]]");
    }

    private static string EscapeMarkup(string text, string color)
    {
        return $"[{color}]{text.Replace("[", "[[").Replace("]", "]]")}[/]";
    }
}

public record SyntaxTheme(string Keyword, string Type, string String, string Number, string Comment, string PlainText)
{

    public static SyntaxTheme Dracula => new(
        "pink1 bold",
        "deepskyblue1",
        "yellow1",
        "mediumorchid1",
        "grey50",
        "white"
    );

    public static SyntaxTheme Solarized => new(
        "green1 bold",
        "yellow1",
        "cyan1",
        "magenta1",
        "grey50",
        "white"
    );

    public static SyntaxTheme Monokai => new(
        "red1 bold",
        "deepskyblue1",
        "yellow1",
        "mediumorchid1",
        "grey50",
        "white"
    );
}