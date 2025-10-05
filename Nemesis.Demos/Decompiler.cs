using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Spectre.Console;

namespace Nemesis.Demos;

internal static class Decompiler
{
    public static string DecompileAsCSharp(string methodName, LanguageVersion languageVersion)
    {
        var fullTypeName = new StackFrame(1).GetMethod()?.DeclaringType?.AssemblyQualifiedName;

        if (string.IsNullOrEmpty(fullTypeName))
            throw new ArgumentException($"Cannot determine declaring type for {methodName}");

        var type = Type.GetType(fullTypeName, false) ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found");
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

        return methodInfo == null
            ? throw new ArgumentException($"Method '{methodName}' not found in type '{fullTypeName}'", nameof(methodName))
            : DecompileAsCSharp(methodInfo, languageVersion);
    }

    public static string DecompileAsCSharp(MethodInfo method, LanguageVersion languageVersion)
    {
        var path = method.DeclaringType!.Assembly.Location;
        var fullTypeName = new FullTypeName(method.DeclaringType!.FullName);

        var decompiler = new CSharpDecompiler(path, new DecompilerSettings(languageVersion));

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
}
