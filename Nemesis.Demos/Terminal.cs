namespace Nemesis.Demos;

public static class Terminal
{
    public static IDisposable ForeColor(ConsoleColor color) =>
        new ForeColorStruct(color);

    public static IDisposable BackColor(ConsoleColor color) =>
        new BackColorStruct(color);

    readonly struct ForeColorStruct : IDisposable
    {
        private readonly ConsoleColor _previousColor;

        public ForeColorStruct(ConsoleColor foreColor)
        {
            _previousColor = Console.ForegroundColor;
            Console.ForegroundColor = foreColor;
        }

        public void Dispose() => Console.ForegroundColor = _previousColor;
    }

    readonly struct BackColorStruct : IDisposable
    {
        private readonly ConsoleColor _previousColor;

        public BackColorStruct(ConsoleColor foreColor)
        {
            _previousColor = Console.BackgroundColor;
            Console.BackgroundColor = foreColor;
        }

        public void Dispose() => Console.BackgroundColor = _previousColor;
    }
}