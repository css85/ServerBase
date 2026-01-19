using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Shared;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class TestByteToString
    {
        private string[] _byteStringArray;

        public TestByteToString()
        {
            _byteStringArray = Enumerable.Range(0, byte.MaxValue).Select(p => p.ToString()).ToArray();
        }

        [Benchmark]
        public string TestEnumNormal()
        {
            var value = PacketType.Request;

            return $"_{(byte)value}";
        }

        [Benchmark]
        public string TestEnumArray()
        {
            var value = PacketType.Request;

            return $"_{_byteStringArray[(byte)value]}";
        }

        [Benchmark]
        public string TestEnumConvert()
        {
            var value = PacketType.Request;

            return $"_{Convert.ToByte(value)}";
        }

        [Benchmark]
        public string TestEnumConvertArray()
        {
            var value = PacketType.Request;

            return $"_{_byteStringArray[Convert.ToByte(value)]}";
        }
    }
}