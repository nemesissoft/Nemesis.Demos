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
    /// Use extension method syntax as opposed to static method call
    /// </summary>
    public bool ExtensionMethods { get; set; } = true;

    /// <summary>
    /// Use lock statement as opposed to Monitor.Enter/Monitor.Exit calls.
    /// </summary>
    public bool LockStatement { get; set; } = false;

    /// <summary>
    /// Use primary constructor syntax with classes and structs.
    /// </summary>
    public bool UsePrimaryConstructorSyntaxForNonRecordTypes { get; set; } = true;

    /// <summary>
    /// Support GetEnumerator extension methods in foreach.
    /// </summary>
    public bool ForEachWithGetEnumeratorExtension { get; set; } = true;


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


