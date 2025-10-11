using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Spectre.Console;


namespace Nemesis.Demos;

internal static class Decompiler
{
    public static string DecompileAsCSharp(MethodInfo method, LanguageVersion languageVersion)
    {
        var path = method.DeclaringType!.Assembly.Location;
        var fullTypeName = new FullTypeName(method.DeclaringType!.FullName);

        //ExtensionMethods true  ,LockStatement true, UsePrimaryConstructorSyntaxForNonRecordTypes true, ForEachWithGetEnumeratorExtension true
        var decompiler = new CSharpDecompiler(path, new DecompilerSettings(languageVersion) { LockStatement = false, });

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

    public static string DecompileAsCSharp(Type type, LanguageVersion languageVersion)
    {
        var path = type.Assembly.Location;
        var fullTypeName = new FullTypeName(type.FullName);

        var decompiler = new CSharpDecompiler(path, new DecompilerSettings(languageVersion));

        return decompiler.DecompileTypeAsString(fullTypeName);
    }

    public static string DecompileAsMsil(MethodInfo method)
    {
        using var file = new PEFile(method.ReflectedType?.Assembly.Location ?? throw new ArgumentException($"Method '{method.Name}' is not contained in proper type", nameof(method)));
        using var sw = new StringWriter();
        var writer = new PlainTextOutput(sw);

        MethodDefinitionHandle methodHandle = (MethodDefinitionHandle)MetadataTokens.Handle(method.MetadataToken);

        var disassembler = new ReflectionDisassembler(writer, CancellationToken.None) { ShowSequencePoints = true, ExpandMemberDefinitions = true, };

        disassembler.DisassembleMethod(file, methodHandle);

        return sw.ToString();
    }
}
