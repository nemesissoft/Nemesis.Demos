using Spectre.Console;

namespace Nemesis.Demos.Internals;

internal static class ConsoleColors
{
    internal readonly struct ForeColorStruct : IDisposable
    {
        private readonly Color _previousColor;

        internal ForeColorStruct(Color foreColor)
        {
            _previousColor = AnsiConsole.Foreground;
            AnsiConsole.Foreground = foreColor;
        }

        public void Dispose() => AnsiConsole.Foreground = _previousColor;
    }

    internal readonly struct BackColorStruct : IDisposable
    {
        private readonly Color _previousColor;

        internal BackColorStruct(Color foreColor)
        {
            _previousColor = AnsiConsole.Background;
            AnsiConsole.Background = foreColor;
        }

        public void Dispose() => AnsiConsole.Background = _previousColor;
    }
}