using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler;
using Spectre.Console;

namespace Nemesis.Demos;

public class Decompiler(DemosOptions Options)
{
    public string DecompileAsCSharp(MethodInfo method)
    {
        var path = Assembly.GetCallingAssembly().Location;
        var fullTypeName = new FullTypeName(method.DeclaringType!.FullName);

        var decompiler = new CSharpDecompiler(path, new DecompilerSettings(Options.LanguageVersion));

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

        var source = decompiler.DecompileAsString(methodToken);

        HighlightCode(source);

        return source;
    }

    public string DecompileAsCSharp(Type type)
    {
        var path = Assembly.GetCallingAssembly().Location;
        var fullTypeName = new FullTypeName(type.FullName);

        var decompiler = new CSharpDecompiler(path, new DecompilerSettings(Options.LanguageVersion));

        var source = decompiler.DecompileTypeAsString(fullTypeName);

        HighlightCode(source);

        return source;
    }

    private void HighlightCode(string source)
    {
        try
        {
            AnsiConsole.Markup(new SyntaxHighlighter(Options).GetHighlightedMarkup($"//Decompiled using {Options.Theme.Name} with C# version {Options.LanguageVersion}{Environment.NewLine}{source}"));
        }
        catch (Exception) { Console.WriteLine(source); }
    }
}
