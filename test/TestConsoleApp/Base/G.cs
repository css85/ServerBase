using System;
using System.IO;
using Shared.ServerApp.Services;

namespace TestConsoleApp.Base
{
    public static class G
    {
        private const string CsvPathName = "csvdata";

        private static readonly CsvStoreContext CsvContext;
        public static CsvStoreContextData CsvData => CsvContext.GetData();

        static G()
        {
            // 기본 경로
            var csvRootPath = Path.Combine(Environment.CurrentDirectory, CsvPathName);

            if (Directory.Exists(csvRootPath) == false)
            {
                // 실행파일이 있는 폴더
                var exeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
                var exeDirectoryPath = Path.GetDirectoryName(exeFilePath);
                csvRootPath = Path.Combine(exeDirectoryPath!, CsvPathName);

                if (Directory.Exists(csvRootPath) == false)
                {
                    // 소스폴더
                    csvRootPath = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\..\\src\\" + CsvPathName);
                }
            }

            CsvContext = new CsvStoreContext(csvRootPath);
        }
    }
}