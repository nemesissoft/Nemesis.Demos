using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using Nemesis.Demos.Highlighters;
using Spectre.Console;

namespace Nemesis.Demos;

public partial class Runnable
{
    public void HighlightDecompiledCSharp(string methodName, LanguageVersion[]? languageVersions = null, object? instanceOrType = null,
        Action<DecompilerSettings>? decompilerSettingsBuilder = null, Action<CSharpFormattingOptions>? formattingOptionsBuilder = null)
        => HighlightDecompiledCSharp(GetMethodInfo(methodName, instanceOrType), languageVersions, decompilerSettingsBuilder, formattingOptionsBuilder);




    public void HighlightDecompiledCSharp(MethodInfo method, LanguageVersion[]? languageVersions = null,
        Action<DecompilerSettings>? decompilerSettingsBuilder = null, Action<CSharpFormattingOptions>? formattingOptionsBuilder = null)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions is null || languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + Decompiler.DecompileAsCSharp(method, defaultVersion, decompilerSettingsBuilder, formattingOptionsBuilder));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + Decompiler.DecompileAsCSharp(method, version, decompilerSettingsBuilder, formattingOptionsBuilder));
    }



    public void HighlightDecompiledCSharp(Type type, LanguageVersion[]? languageVersions = null,
        Action<DecompilerSettings>? decompilerSettingsBuilder = null, Action<CSharpFormattingOptions>? formattingOptionsBuilder = null)
    {
        var defaultVersion = demo.DemoOptions.DefaultDecompilerLanguageVersion;
        if (languageVersions is null || languageVersions.Length == 0)
            HighlightCode(GetComment(defaultVersion) + Decompiler.DecompileAsCSharp(type, defaultVersion, decompilerSettingsBuilder, formattingOptionsBuilder));
        else
            foreach (var version in languageVersions)
                HighlightCode(GetComment(version) + Decompiler.DecompileAsCSharp(type, version, decompilerSettingsBuilder, formattingOptionsBuilder));
    }



    public void HighlightDecompiledMsil(string methodName, object? instanceOrType = null) =>
        HighlightDecompiledMsil(GetMethodInfo(methodName, instanceOrType));

    public void HighlightDecompiledMsil(MethodInfo method)
    {
        try
        {
            var msil = demo.Decompiler.DecompileAsMsil(method);
            HighlightCode(msil, Language.Msil);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine($"Method '{method.Name}' cannot be disassembled to MSIL");
        }
    }

    public void HighlightDecompiledMsil(Type type)
    {
        try
        {
            var msil = demo.Decompiler.DecompileAsMsil(type);
            HighlightCode(msil, Language.Msil);
        }
        catch (Exception)
        {
            AnsiConsole.WriteLine($"Type '{type.Name}' cannot be disassembled to MSIL");
        }
    }

    private static string GetComment(LanguageVersion version) => $"//Decompiled using C# version {version}{Environment.NewLine}";

    private static MethodInfo GetMethodInfo(string methodName, object? instanceOrType)
    {
        Type type = instanceOrType switch
        {
            Type t => t,
            { } obj => obj.GetType(),
            null => GetTypeFromStackTrace() ?? throw new InvalidOperationException($"Cannot determine declaring type for {methodName}")
        };

        var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        return methodInfo ?? throw new ArgumentException($"Method '{methodName}' not found in type '{type.AssemblyQualifiedName}'", nameof(methodName));

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
}