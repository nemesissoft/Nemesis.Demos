using Spectre.Console;
using Microsoft.CodeAnalysis;

namespace Nemesis.Demos;

public record SyntaxTheme(string Name, string Keyword, string Type, string String, string Number, string Comment, string PlainText)
{
    public static readonly List<(string Name, SyntaxTheme Theme)> All;

    static SyntaxTheme()
    {
        All = (from p in typeof(SyntaxTheme).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
               where p.PropertyType == typeof(SyntaxTheme)
               let value = (SyntaxTheme)p.GetValue(null)!
               orderby value.Name
               select (value.Name, value)
            ).ToList();
    }

    //public override string? ToString() => Name.ToString();

    public static SyntaxTheme Dracula => new("Dracula",
      Keyword: "pink1 bold",
      Type: "deepskyblue1",
      String: "yellow1",
      Number: "mediumorchid1",
      Comment: "grey50",
      PlainText: "white");

    public static SyntaxTheme SolarizedDark => new("Solarized Dark",
        Keyword: "green1 bold",
        Type: "cyan1",
        String: "yellow1",
        Number: "magenta1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme SolarizedLight => new("Solarized Light",
        Keyword: "blue bold",
        Type: "red1",
        String: "green1",
        Number: "magenta1",
        Comment: "grey30",
        PlainText: "black");

    public static SyntaxTheme Monokai => new("Monokai",
        Keyword: "red1 bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "mediumorchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme GitHubLight => new("GitHub Light",
        Keyword: "blue bold",
        Type: "darkorange",
        String: "green3",
        Number: "purple",
        Comment: "grey50",
        PlainText: "black");

    public static SyntaxTheme GitHubDark => new("GitHub Dark",
        Keyword: "cyan1 bold",
        Type: "yellow1",
        String: "green3",
        Number: "mediumorchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme VisualStudioDark => new("VisualStudio Dark",
        Keyword: "blue bold",
        Type: "teal",
        String: "lightgreen",
        Number: "lightcoral",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme VisualStudioLight => new("VisualStudio Light",
        Keyword: "blue bold",
        Type: "darkcyan",
        String: "darkgreen",
        Number: "darkmagenta",
        Comment: "grey50",
        PlainText: "black");

    public static SyntaxTheme RiderDark => new("Rider Dark",
        Keyword: "magenta bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "orchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme RiderLight => new("Rider Light",
        Keyword: "darkmagenta bold",
        Type: "blue",
        String: "darkgreen",
        Number: "darkorange",
        Comment: "grey50",
        PlainText: "black");

    public static SyntaxTheme Nord => new("Nord",
        Keyword: "cyan1 bold",
        Type: "lightskyblue1",
        String: "lightgreen",
        Number: "lightpink1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme OneDark => new("One Dark",
        Keyword: "magenta bold",
        Type: "cyan1",
        String: "green3",
        Number: "darkorange",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme GruvboxDark => new("Gruvbox Dark",
        Keyword: "yellow1 bold",
        Type: "deepskyblue1",
        String: "green3",
        Number: "darkorange",
        Comment: "grey58",
        PlainText: "white");

    public static SyntaxTheme TomorrowNight => new("Tomorrow Night",
        Keyword: "cyan1 bold",
        Type: "yellow1",
        String: "lightgreen",
        Number: "orchid1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme Cyberpunk => new("Cyberpunk",
        Keyword: "hotpink bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "magenta1",
        Comment: "grey42",
        PlainText: "white");

    public static SyntaxTheme NightOwl => new("Night Owl",
        Keyword: "blue bold",
        Type: "cyan1",
        String: "lightgreen",
        Number: "orchid1",
        Comment: "grey54",
        PlainText: "white");

    public static SyntaxTheme DraculaPro => new("Dracula Pro",
        Keyword: "pink1 bold",
        Type: "deepskyblue1",
        String: "yellow1",
        Number: "violet",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme AyuDark => new("Ayu Dark",
        Keyword: "orange1 bold",
        Type: "cyan1",
        String: "green3",
        Number: "yellow1",
        Comment: "grey50",
        PlainText: "white");

    public static SyntaxTheme AyuLight => new("Ayu Light",
        Keyword: "blue bold",
        Type: "darkcyan",
        String: "darkgreen",
        Number: "darkmagenta",
        Comment: "grey42",
        PlainText: "black");
}