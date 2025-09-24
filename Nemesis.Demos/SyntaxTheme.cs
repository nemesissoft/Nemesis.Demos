using Spectre.Console;
using Microsoft.CodeAnalysis;

namespace Nemesis.Demos;

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

    public static SyntaxTheme Nord => new(
        Keyword: "cyan1 bold",
        Type: "lightskyblue1",
        String: "lightgreen",
        Number: "lightpink1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme OneDark => new(
        Keyword: "magenta bold",
        Type: "cyan1",
        String: "green3",
        Number: "darkorange",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme GruvboxDark => new(
        Keyword: "yellow1 bold",
        Type: "deepskyblue1",
        String: "green3",
        Number: "darkorange",
        Comment: "grey58",
        PlainText: "white");

    public static SyntaxTheme TomorrowNight => new(
        Keyword: "cyan1 bold",
        Type: "yellow1",
        String: "lightgreen",
        Number: "orchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme Cyberpunk => new(
        Keyword: "hotpink bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "magenta1",
        Comment: "grey42",
        PlainText: "white");

    public static SyntaxTheme NightOwl => new(
        Keyword: "blue bold",
        Type: "cyan1",
        String: "lightgreen",
        Number: "orchid1",
        Comment: "grey54",
        PlainText: "white");

    public static SyntaxTheme DraculaPro => new(
        Keyword: "pink1 bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "violet",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme AyuDark => new(
        Keyword: "orange1 bold",
        Type: "cyan1",
        String: "green3",
        Number: "yellow1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme AyuLight => new(
        Keyword: "blue bold",
        Type: "darkcyan",
        String: "darkgreen",
        Number: "darkmagenta",
        Comment: "grey42",
        PlainText: "black");
}