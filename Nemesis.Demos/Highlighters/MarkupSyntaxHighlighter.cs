namespace Nemesis.Demos.Highlighters;

public abstract class MarkupSyntaxHighlighter
{
    public abstract string GetHighlightedMarkup(string code);

    protected static string Escape(string text) => text.Replace("[", "[[").Replace("]", "]]");
}

public class MarkupSyntaxHighlighterFactory(DemoOptions Options)
{
    public MarkupSyntaxHighlighter GetSyntaxHighlighter(Language language)
    {
        return language switch
        {
            Language.CSharp => new CSharpHighlighter(Options),
            Language.Xml => new XmlHighlighter(Options),
            Language.Json => new JsonHighlighter(Options),
            Language.Msil => new MsilHighlighter(Options),
            _ => throw new NotSupportedException($"Language {language} is not supported for syntax highlighting")
        };
    }
}


public enum Language { CSharp, Xml, Json, Msil }
