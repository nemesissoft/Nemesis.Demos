using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Spectre.Console;

namespace Nemesis.Demos;

public class Decompiler(DemoOptions Options)
{
    public static string DecompileAsCSharp(MethodInfo method, LanguageVersion languageVersion, Action<DecompilerSettings>? decompilerSettingsBuilder = null)
    {
        var path = method.DeclaringType!.Assembly.Location;
        var fullTypeName = new FullTypeName(method.DeclaringType!.FullName);

        var decompiler = GetCSharpDecompiler(path, languageVersion, decompilerSettingsBuilder);

        var typeInfo = decompiler.TypeSystem.FindType(fullTypeName).GetDefinition()!;
        var @params = method.GetParameters();

        var methodToken = typeInfo.Methods.First(m =>
            m.Name == method.Name &&
            m.ReturnType.FullName == method.ReturnType.FullName &&
            m.Parameters.Count == @params.Length &&
            m.Parameters.Zip(@params)
                .Select(t => t.First.Type.FullName == t.Second.ParameterType.FullName)
                .All(b => b)
        ).MetadataToken;

        return decompiler.DecompileAsString(methodToken);
    }

    public static string DecompileAsCSharp(Type type, LanguageVersion languageVersion, Action<DecompilerSettings>? decompilerSettingsBuilder = null)
    {
        var path = type.Assembly.Location;
        var fullTypeName = new FullTypeName(type.FullName);

        var decompiler = GetCSharpDecompiler(path, languageVersion, decompilerSettingsBuilder);

        return decompiler.DecompileTypeAsString(fullTypeName);
    }

    private static CSharpDecompiler GetCSharpDecompiler(string path, LanguageVersion languageVersion, Action<DecompilerSettings>? decompilerSettingsBuilder = null)
    {
        var decompilerSettings = new DecompilerSettings(languageVersion)
        {
            ExtensionMethods = true,
            LockStatement = false,
            UsePrimaryConstructorSyntaxForNonRecordTypes = true,
            ForEachWithGetEnumeratorExtension = true,
        };

        decompilerSettingsBuilder?.Invoke(decompilerSettings);

        return new(path, decompilerSettings);
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

        var methodHandle = (TypeDefinitionHandle)MetadataTokens.Handle(type.MetadataToken);

        var disassembler = GetMsilDisassembler(writer, Options.DecompilerSettings);

        disassembler.DisassembleType(file, methodHandle);

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
