namespace Nemesis.Demos;

public static class RangeExtensions
{
    public static int[] ToArray(this Range range) =>
        range.Start.IsFromEnd || range.End.IsFromEnd
            ? throw new NotSupportedException("'FromEnd' ranges are not supported ")
            : [.. Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value + 1)];

    public static TResult[] ToArray<TResult>(this Range range, Func<int, TResult> transformer) =>
        range.Start.IsFromEnd || range.End.IsFromEnd
            ? throw new NotSupportedException("'FromEnd' ranges are not supported ")
            : [.. Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value + 1).Select(transformer)];

    public static int[] ToArray(this Range range, int length)
    {
        var (offset, effectiveLength) = range.GetOffsetAndLength(length);
        return [.. Enumerable.Range(offset, effectiveLength)];
    }

    public static TResult[] ToArray<TResult>(this Range range, int length, Func<int, TResult> transformer)
    {
        var (offset, effectiveLength) = range.GetOffsetAndLength(length);
        return [.. Enumerable.Range(offset, effectiveLength).Select(transformer)];
    }

    public static IntEnumerator GetEnumerator(this Range range) => new(range);

    public ref struct IntEnumerator
    {
        private readonly int _end;

        public IntEnumerator(Range range)
        {
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
                throw new NotSupportedException("'FromEnd' ranges are not supported");

            Current = range.Start.Value - 1;
            _end = range.End.Value;
        }

        public int Current { get; private set; }

        public bool MoveNext() => ++Current <= _end;
    }
}