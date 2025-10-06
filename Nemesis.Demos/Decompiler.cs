using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Spectre.Console;

namespace Nemesis.Demos;

internal static class Decompiler
{
    public static string DecompileAsCSharp(string methodName, LanguageVersion languageVersion, object? instanceOrType = null)
    {
        Type type = instanceOrType switch
        {
            Type t => t,
            { } obj => obj.GetType(),
            null => GetTypeFromStackTrace() ?? throw new InvalidOperationException($"Cannot determine declaring type for {methodName}")
        };

        var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        return methodInfo == null
            ? throw new ArgumentException($"Method '{methodName}' not found in type '{type.AssemblyQualifiedName}'", nameof(methodName))
            : DecompileAsCSharp(methodInfo, languageVersion);

        static Type? GetTypeFromStackTrace()
        {
            var demosNamespace = typeof(Decompiler).Namespace ?? throw new InvalidOperationException("Demos namespace cannot be determined");
            var stack = new StackTrace();
            return stack.GetFrames()?
                .Select(f => f.GetMethod()?.DeclaringType)
                .FirstOrDefault(t => t is not null
                            && t.Namespace is not null
                            && !t.Namespace.StartsWith(demosNamespace));
        }
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
