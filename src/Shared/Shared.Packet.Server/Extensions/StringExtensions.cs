using System;

namespace SampleGame.Shared.Extensions
{
    public static class StringExtensions
    {
        // https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm

        public static ReadOnlySpan<char> Split(ref ReadOnlySpan<char> span, char separator)
        {
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == separator)
                {
                    var resultSpan = span.Slice(0, i);
                    span = span.Slice(i + 1);
                    return resultSpan;
                }
            }

            var resultSpan2 = span;
            span = ReadOnlySpan<char>.Empty;
            return resultSpan2;
        }

        public static LineSplitEnumerator SpanSplit(this string str, char separator)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(str.AsSpan(), separator);
        }

        public static LineSplitEnumerator SpanSplit(this ReadOnlySpan<char> span, char separator)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(span, separator);
        }

        // Must be a ref struct as it contains a ReadOnlySpan<char>
        public ref struct LineSplitEnumerator
        {
            private readonly char _separator;
            private ReadOnlySpan<char> _str;

            public LineSplitEnumerator(ReadOnlySpan<char> str, char separator)
            {
                _separator = separator;
                _str = str;
                Current = default;
            }

            // Needed to be compatible with the foreach operator
            public LineSplitEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                var span = _str;
                if (span.Length == 0) // Reach the end of the string
                    return false;

                var index = span.IndexOf(_separator);
                if (index == -1) // The string is composed of only one line
                {
                    _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                    Current = new LineSplitEntry(span, true);
                    return true;
                }

                Current = new LineSplitEntry(span.Slice(0, index), false);
                _str = span.Slice(index + 1);
                return true;
            }

            public LineSplitEntry Current { get; private set; }
        }

        public readonly ref struct LineSplitEntry
        {
            public LineSplitEntry(ReadOnlySpan<char> line, bool isLast)
            {
                Line = line;
                IsLast = isLast;
            }

            public ReadOnlySpan<char> Line { get; }
            public bool IsLast { get; }

            // This method allow to deconstruct the type, so you can write any of the following code
            // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
            // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
            // https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
            public void Deconstruct(out ReadOnlySpan<char> line, out bool isLast)
            {
                line = Line;
                isLast = IsLast;
            }

            // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
            // foreach (ReadOnlySpan<char> entry in str.SplitLines())
            public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
        }
    }
}
