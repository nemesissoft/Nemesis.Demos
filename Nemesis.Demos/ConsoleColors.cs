namespace Nemesis.Demos;

public static class ConsoleColors
{
    public static IDisposable ForeColor(ConsoleColor color) => new ForeColorStruct(color);

    public static IDisposable BackColor(ConsoleColor color) => new BackColorStruct(color);

    private readonly struct ForeColorStruct : IDisposable
    {
        private readonly ConsoleColor _previousColor;

        internal ForeColorStruct(ConsoleColor foreColor)
        {
            _previousColor = Console.ForegroundColor;
            Console.ForegroundColor = foreColor;
        }

        public void Dispose() => Console.ForegroundColor = _previousColor;
    }

    private readonly struct BackColorStruct : IDisposable
    {
        private readonly ConsoleColor _previousColor;

        internal BackColorStruct(ConsoleColor foreColor)
        {
            _previousColor = Console.BackgroundColor;
            Console.BackgroundColor = foreColor;
        }

        public void Dispose() => Console.BackgroundColor = _previousColor;
    }
}