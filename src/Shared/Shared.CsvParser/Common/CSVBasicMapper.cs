
using System;
using Shared.CsvParser;

namespace SampleGame
{
    public class CSVBasicMapper<T> : CSVMapperBase<T> where T : BaseCSVData, new()
    {
        public CSVBasicMapper(string csvFilePath) : base(csvFilePath)
        {
        }
    }

}