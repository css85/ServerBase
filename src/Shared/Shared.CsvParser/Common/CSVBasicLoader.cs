//#if NETSTANDARD //ServerShare
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace SampleGame
{
    public class CSVBasicLoader : CSVLoaderBase ,ICSVLoader
    {
        protected CSVBasicLoader(Type type, string csvFilePath)
        {
            CsvMapperType = type;
            FilePath = csvFilePath;
        }
        protected virtual string ExceptionMessage(string message) => $"{message}\n{CsvMapperType} | {FilePath}";
        public void LoadPath(string path, string bundleName, bool async = true, bool resource = true, Action loadedCallback = null, bool isBundle = false)
        {
            if (!File.Exists(path))
                throw new DataParserException(ExceptionMessage("File Not Found"));

            if (!async)
            {
                byte[] numArray;
                using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    numArray = new byte[fileStream.Length];
                    fileStream.Read(numArray, 0, numArray.Length);
                }
                Load(System.Text.Encoding.UTF8.GetString(numArray), loadedCallback);
            }
            else
            {
                Task.Run(() => LoadPathAsync(path, true ,loadedCallback));
            }
        }

        private async Task<int> LoadPathAsync(string path, bool async, Action loadedCallback)
        {
            byte[] numArray;
            int result=0;
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                numArray = new byte[fileStream.Length];
                result = await fileStream.ReadAsync(numArray, 0, numArray.Length);
            }
            Load(System.Text.Encoding.UTF8.GetString(numArray), loadedCallback);

            return result;
        }

        IEnumerator ICSVLoader.LoadPathAsync(string path, bool async, Action loadedCallback)
        {
            throw new NotSupportedException();
        }
    }
}

//#endif