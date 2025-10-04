using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.TypeSystem;
using Spectre.Console;

namespace Nemesis.Demos;

internal class Decompiler(DemosOptions Options)
{
    public string DecompileAsCSharp(string methodName, string? fullTypeName = null)
    {
        if (string.IsNullOrEmpty(fullTypeName))
            fullTypeName = new StackFrame(1).GetMethod()?.DeclaringType?.AssemblyQualifiedName;

        if (string.IsNullOrEmpty(fullTypeName))
            throw new ArgumentException("Cannot determine declaring type", nameof(fullTypeName));

        var type = Type.GetType(fullTypeName, false) ?? throw new ArgumentException($"Type '{fullTypeName}' not found", nameof(fullTypeName));
        var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        return methodInfo == null
            ? throw new ArgumentException($"Method '{methodName}' not found in type '{fullTypeName}'", nameof(methodName))
            : DecompileAsCSharp(methodInfo);
    }

    public string DecompileAsCSharp(MethodInfo method)
    {
        var path = method.DeclaringType!.Assembly.Location;
        var fullTypeName = new FullTypeName(method.DeclaringType!.FullName);

        var decompiler = new ICSharpCode.Decompiler.CSharp.CSharpDecompiler(path, new DecompilerSettings(Options.LanguageVersion));

        var typeInfo = decompiler.TypeSystem.FindType(fullTypeName).GetDefinition()!;
        var @params = method.GetParameters();

        var methodToken = typeInfo.Methods.First(m =>
            m.Name == method.Name &&
            m.ReturnType.FullName == method.ReturnType.FullName &&
            m.Parameters.Count == @params.Length &&
            m.Parameters.Zip(@params)
                .Select(t => t.First.Type.FullName == t.Second.ParameterType.FullName)
                .All(b => b == true)
        ).MetadataToken;

        return decompiler.DecompileAsString(methodToken);
    }

    public string DecompileAsCSharp(Type type)
    {
        var path = type.Assembly.Location;
        var fullTypeName = new FullTypeName(type.FullName);

        var decompiler = new ICSharpCode.Decompiler.CSharp.CSharpDecompiler(path, new DecompilerSettings(Options.LanguageVersion));

        return decompiler.DecompileTypeAsString(fullTypeName);
    }
}
