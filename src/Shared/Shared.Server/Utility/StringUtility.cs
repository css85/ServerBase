using System;
using System.Linq;

namespace SampleGame.Shared.Utility
{
    public static class StringUtility
    {
        private static readonly Random _ran = new();
        public static string RandomString(int length)
        {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_ran.Next(s.Length)]).ToArray());
        }
    }
}