using Spectre.Console;

namespace Nemesis.Demos;

public partial class Runnable
{
    public static IDisposable ForeColor(Color color) => new ForeColorStruct(color);

    public static IDisposable BackColor(Color color) => new BackColorStruct(color);

    readonly struct ForeColorStruct : IDisposable
    {
        private readonly Color _previousColor;

        internal ForeColorStruct(Color foreColor)
        {
            _previousColor = AnsiConsole.Foreground;
            AnsiConsole.Foreground = foreColor;
        }

        public void Dispose() => AnsiConsole.Foreground = _previousColor;
    }

    readonly struct BackColorStruct : IDisposable
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
