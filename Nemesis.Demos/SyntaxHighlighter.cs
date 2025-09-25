using Spectre.Console;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nemesis.Demos;

public static class SyntaxHighlighter
{
    public static void Highlight(string code, SyntaxTheme theme)
    {
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
    public static readonly List<(string Name, SyntaxTheme Theme)> All;

    static SyntaxTheme()
    {
        All = typeof(SyntaxTheme).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(SyntaxTheme))
            .Select(p => (
                p.Name,
                (SyntaxTheme)p.GetValue(null)!
            ))
            .ToList();

    }

    public static SyntaxTheme Dracula => new(
      Keyword: "pink1 bold",
      Type: "deepskyblue1",
      String: "yellow1",
      Number: "mediumorchid1",
      Comment: "grey50",
      PlainText: "white");

    public static SyntaxTheme SolarizedDark => new(
        Keyword: "green1 bold",
        Type: "cyan1",
        String: "yellow1",
        Number: "magenta1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme SolarizedLight => new(
        Keyword: "blue bold",
        Type: "red1",
        String: "green1",
        Number: "magenta1",
        Comment: "grey30",
        PlainText: "black");

    public static SyntaxTheme Monokai => new(
        Keyword: "red1 bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "mediumorchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme GitHubLight => new(
        Keyword: "blue bold",
        Type: "darkorange",
        String: "green3",
        Number: "purple",
        Comment: "grey50",
        PlainText: "black");

    public static SyntaxTheme GitHubDark => new(
        Keyword: "cyan1 bold",
        Type: "yellow1",
        String: "green3",
        Number: "mediumorchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme VisualStudioDark => new(
        Keyword: "blue bold",
        Type: "teal",
        String: "lightgreen",
        Number: "lightcoral",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme VisualStudioLight => new(
        Keyword: "blue bold",
        Type: "darkcyan",
        String: "darkgreen",
        Number: "darkmagenta",
        Comment: "grey50",
        PlainText: "black");

    public static SyntaxTheme RiderDark => new(
        Keyword: "magenta bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "orchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme RiderLight => new(
        Keyword: "darkmagenta bold",
        Type: "blue",
        String: "darkgreen",
        Number: "darkorange",
        Comment: "grey50",
        PlainText: "black");
}