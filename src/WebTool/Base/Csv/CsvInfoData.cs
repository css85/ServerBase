using System;

namespace WebTool.Base.Csv
{
    public class CsvInfoData
    {
        private readonly string[][] _csvContents;
        private readonly Type[] _types;
        private readonly string[][] _typeNames;

        public CsvInfoData(string csvContent)
        {
            var rows = csvContent.Split("\r\n");

            _csvContents = new string[rows.Length][];

            for (var i = 0; i < rows.Length; i++)
            {
                var columns = rows[i].Split(',');
                _csvContents[i] = new string[columns.Length];
                for (var j = 0; j < columns.Length; j++)
                {
                    _csvContents[i][j] = columns[j];
                }
            }

            var typeRow = _csvContents[1];
            _types = new Type[typeRow.Length];
            for (var i = 0; i < typeRow.Length; i++)
            {
                _types[i] = Type.GetType(typeRow[i]);
            }

            _typeNames = new string[_types.Length][];
            for (var i = 0; i < _types.Length; i++)
            {
                var type = _types[i];
                if (type.IsEnum)
                {
                    _typeNames[i] = Enum.GetNames(type);
                }
                else
                {
                    _typeNames[i] = new string[0];
                }
            }
        }

        public string[] GetColumns(int row)
        {
            return _csvContents[row];
        }

        public string[] GetNames()
        {
            return _csvContents[0];
        }

        public Type[] GetTypes()
        {
            return _types;
        }

        public string[][] GetTypeNames()
        {
            return _typeNames;
        }
    }
}
