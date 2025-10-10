using System.Collections.Concurrent;
using Spectre.Console;

namespace Nemesis.Demos;

internal static class FigletFontStore
{
    // Thread-safe cache of loaded fonts
    private static readonly ConcurrentDictionary<string, FigletFont> _cache = new();

    // Assembly containing the embedded resources (can be customized)
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets a FigletFont by name, loading it from embedded resources if not already cached.
    /// </summary>
    public static FigletFont GetFont(string name) => _cache.GetOrAdd(name, LoadFont);

    /// <summary>
    /// Loads a FigletFont from embedded resources.
    /// </summary>
    private static FigletFont LoadFont(string name)
    {
        var resourceName = $"{_assembly.GetName().Name}.Fonts.{name}.flf";

        using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded Figlet font resource '{resourceName}' not found.");

        return FigletFont.Load(stream);
    }
}
