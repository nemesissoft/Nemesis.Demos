using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace Nemesis.Demos;

public class Decompiler(DemoOptions Options)
{
    public static string DecompileAsCSharp(MethodInfo method, LanguageVersion languageVersion, Action<DecompilerSettings>? decompilerSettingsBuilder, Action<CSharpFormattingOptions>? formattingOptionsBuilder) =>
        Decompile(
            path: method.DeclaringType!.Assembly.Location,
            languageVersion,
            decompiler => decompiler.Decompile(MetadataTokens.EntityHandle(method.MetadataToken)),
            decompilerSettingsBuilder,
            formattingOptionsBuilder);

    public static string DecompileAsCSharp(Type type, LanguageVersion languageVersion, Action<DecompilerSettings>? decompilerSettingsBuilder, Action<CSharpFormattingOptions>? formattingOptionsBuilder) =>
        Decompile(
            path: type.Assembly.Location,
            languageVersion,
            d => d.DecompileType(new FullTypeName(type.FullName)),
            decompilerSettingsBuilder,
            formattingOptionsBuilder);

    private static string Decompile(string path, LanguageVersion languageVersion, Func<CSharpDecompiler, SyntaxTree> decompileFunction,
        Action<DecompilerSettings>? decompilerSettingsBuilder, Action<CSharpFormattingOptions>? formattingOptionsBuilder)
    {
        var decompilerSettings = new DecompilerSettings(languageVersion)
        {
            AlwaysUseBraces = false,
            ExtensionMethods = true,
            LockStatement = false,
            UsePrimaryConstructorSyntaxForNonRecordTypes = true,
            ForEachWithGetEnumeratorExtension = true,
        };

        decompilerSettingsBuilder?.Invoke(decompilerSettings);

        CSharpFormattingOptions formattingOptions = FormattingOptionsFactory.CreateAllman();
        formattingOptions.IndentationString = "    ";
        formattingOptions.IndentSwitchBody = false;
        formattingOptions.ArrayInitializerWrapping = Wrapping.WrapIfTooLong;
        formattingOptionsBuilder?.Invoke(formattingOptions);


        var decompiler = new CSharpDecompiler(path, decompilerSettings);
        var syntaxTree = decompileFunction(decompiler);

        using var sw = new StringWriter();
        syntaxTree.AcceptVisitor(new CSharpOutputVisitor(sw, formattingOptions));
        return sw.ToString();
    }

    public string DecompileAsMsil(MethodInfo method)
    {
        using var file = new PEFile(method.ReflectedType?.Assembly.Location ?? throw new ArgumentException($"Method '{method.Name}' is not contained in proper type", nameof(method)));
        using var sw = new StringWriter();
        var writer = new PlainTextOutput(sw);

        var methodHandle = (MethodDefinitionHandle)MetadataTokens.Handle(method.MetadataToken);

        var disassembler = GetMsilDisassembler(writer, Options.DecompilerSettings);

        disassembler.DisassembleMethod(file, methodHandle);

        return sw.ToString();
    }

    public string DecompileAsMsil(Type type)
    {
        using var file = new PEFile(type.Assembly.Location);
        using var sw = new StringWriter();
        var writer = new PlainTextOutput(sw);

        TypeDefinitionHandle typeHandle = (TypeDefinitionHandle)MetadataTokens.Handle(type.MetadataToken);

        var disassembler = GetMsilDisassembler(writer, Options.DecompilerSettings);

        disassembler.DisassembleType(file, typeHandle);

        return sw.ToString();
    }

    private static ReflectionDisassembler GetMsilDisassembler(PlainTextOutput writer, DemoDecompilerSettings settings)
        => new(writer, CancellationToken.None)
        {
            ShowSequencePoints = settings.MsilShowSequencePoints,
            ShowMetadataTokens = settings.MsilShowMetadataTokens,
            ShowMetadataTokensInBase10 = settings.MsilShowMetadataTokensInBase10,
            ExpandMemberDefinitions = settings.MsilExpandMemberDefinitions
        };
}
