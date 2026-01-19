using System.Linq;

namespace Shared.Packet.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] _byteStringArray;

        static StringExtensions()
        {
            _byteStringArray = Enumerable.Range(0, 256).Select(p => p.ToString()).ToArray();
        }

        public static string ToString2(this byte value)
        {
            return _byteStringArray[value];
        }
    }
}