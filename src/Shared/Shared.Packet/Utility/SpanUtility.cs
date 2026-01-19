using System;
using System.Runtime.CompilerServices;

namespace Shared.Packet.Client.Utility
{
    public static class SpanUtility
    {
        public static ReadOnlySpan<char> ReadSplitData(string data, char separatorChar, ref int position)
        {
            for (var i = position; i < data.Length; i++)
            {
                if (data[i] == separatorChar)
                {
                    var span = data.AsSpan(position, i - position);
                    position = i + 1;
                    return span;
                }
            }

            if (position >= data.Length)
            {
                position = data.Length + 1;
                return ReadOnlySpan<char>.Empty;
            }

            var span2 = data.AsSpan(position, data.Length - position);
            return span2;
        }

        public static void Skip(string data, char separatorChar, ref int position)
        {
            for (var i = position; i < data.Length; i++)
            {
                if (data[i] == separatorChar)
                {
                    position = i + 1;
                    return;
                }
            }

            position = data.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ReadSplitDataAll(string data, ref int position)
        {
            return data.AsSpan(position, data.Length - position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(string data, char separatorChar, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return byte.Parse(ReadSplitData(data, separatorChar, ref position));
#else
            return byte.Parse(ReadSplitData(data, separatorChar, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByteAll(string data, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return byte.Parse(ReadSplitDataAll(data, ref position));
#else
            return byte.Parse(ReadSplitDataAll(data, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(string data, char separatorChar, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return int.Parse(ReadSplitData(data, separatorChar, ref position));
#else
            return int.Parse(ReadSplitData(data, separatorChar, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadIntAll(string data, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return int.Parse(ReadSplitDataAll(data, ref position));
#else
            return int.Parse(ReadSplitDataAll(data, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(string data, char separatorChar, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return long.Parse(ReadSplitData(data, separatorChar, ref position));
#else
            return long.Parse(ReadSplitData(data, separatorChar, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLongAll(string data, ref int position)
        {
#if NETSTANDARD2_1 || NET5_0
            return long.Parse(ReadSplitDataAll(data, ref position));
#else
            return long.Parse(ReadSplitDataAll(data, ref position).ToString());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(string data, char separatorChar, ref int position)
        {
            return ReadSplitData(data, separatorChar, ref position).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringAll(string data, ref int position)
        {
            return ReadSplitDataAll(data, ref position).ToString();
        }
    }
}