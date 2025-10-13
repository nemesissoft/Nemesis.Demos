using ICSharpCode.Decompiler.CSharp;
using Nemesis.Demos.Highlighters;

namespace Nemesis.Demos;

public class DemoOptions
{
    public LanguageVersion DefaultDecompilerLanguageVersion { get; set; } = LanguageVersion.Latest;
    public DemoDecompilerSettings DecompilerSettings { get; set; } = new();
    public SyntaxTheme Theme { get; set; } = SyntaxTheme.VisualStudioDark;
}

public class DemoDecompilerSettings
{
    /// <summary>
    /// Show sequence points if debug information is loaded in Cecil.
    /// </summary>
    public bool MsilShowSequencePoints { get; set; } = true;

    /// <summary>
    /// Show metadata tokens for instructions with token operands.
    /// </summary>
    public bool MsilShowMetadataTokens { get; set; } = false;

    /// <summary>
    /// Show metadata tokens for instructions with token operands in base 10.
    /// </summary>
    public bool MsilShowMetadataTokensInBase10 { get; set; } = false;

    public bool MsilExpandMemberDefinitions { get; set; } = true;
}


