namespace Nemesis.Demos.Highlighters;

public interface IMarkupSyntaxHighlighter
{
    string GetHighlightedMarkup(string code);
}

public class MarkupSyntaxHighlighterFactory(DemoOptions Options)
{
    public IMarkupSyntaxHighlighter GetSyntaxHighlighter(Language language)
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
