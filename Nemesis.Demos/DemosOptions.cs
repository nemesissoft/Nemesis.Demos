using Nemesis.Demos.Highlighters;

namespace Nemesis.Demos;

public class DemosOptions
{
    public ICSharpCode.Decompiler.CSharp.LanguageVersion DefaultDecompilerLanguageVersion { get; set; } = ICSharpCode.Decompiler.CSharp.LanguageVersion.Latest;
    public SyntaxTheme Theme { get; set; } = SyntaxTheme.VisualStudioDark;
}
