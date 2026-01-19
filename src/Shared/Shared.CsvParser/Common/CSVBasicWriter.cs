using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace Shared.CsvParser.Common
{
    public static class CSVBasicWriter
    {
        public static async Task<string> WriteAndSave(string fileName, string[][] writeDatas)
        {
            string[] insertData = new string[writeDatas.Length];
            var filePath = $"./StoreCsv/";
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }
            for (var i = 0; i < writeDatas.Length; i++)
            {
                insertData[i] = string.Join(',', writeDatas[i]);
            }
            fileName = $"{filePath}{fileName}";
            await File.WriteAllLinesAsync(fileName, insertData, Encoding.UTF8);
            return fileName;
        }
    }
}
