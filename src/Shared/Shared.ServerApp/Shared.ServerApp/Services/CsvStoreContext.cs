using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Config;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Config;
using Shared.Session.Base;
using SampleGame.Shared.Common;

namespace Shared.ServerApp.Services
{
    public class CsvStoreContext
    {
        private readonly ILogger<CsvStoreContext> _logger;
        private readonly AppContextServiceBase _appContext;
        private CsvStoreContextData _csvStoreContextData;

        private readonly string _csvRootPath;
        public string CsvRootPath => _csvRootPath;

        private long _csvVersion;

        private readonly Dictionary<object, Func<Task>> _csvDataChangedCallbackMap = new();

        public CsvStoreContext(
            ILogger<CsvStoreContext> logger,
            AppContextServiceBase appContext,
            ChangeableSettings<CsvSettings> csvSettings
            )
        {
            _logger = logger;
            _appContext = appContext;
            var csvRootPath = Path.Combine(Directory.GetCurrentDirectory(), csvSettings.Value.CsvPath);
            if (Directory.Exists(csvRootPath) == false)
            {
                csvRootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, csvSettings.Value.CsvPath);
            }
            _csvRootPath = Path.GetFullPath(csvRootPath);

            _csvStoreContextData = new CsvStoreContextData(_appContext.IsUnitTest, _csvRootPath);
            _csvStoreContextData.LoadCsvDataAll();
            _csvVersion = DateTime.MinValue.Ticks;

        }

        public CsvStoreContext(string csvRootPath)
        {
            _csvRootPath = Path.GetFullPath(csvRootPath);

            _csvStoreContextData = new CsvStoreContextData(false, _csvRootPath);
            _csvStoreContextData.LoadCsvDataAll();
            _csvVersion = DateTime.MinValue.Ticks;
        }

        public void RegisterChangedCallback(object owner, Func<Task> action)
        {
            _csvDataChangedCallbackMap.Add(owner, action);
        }

        public void UnRegisterChangedCallback(object owner)
        {
            _csvDataChangedCallbackMap.Remove(owner);
        }

        public async Task<bool> RefreshCsvDataAsync(DateTime updateTime, string[] fileNames, string[] dataStrings)
        {
            if (fileNames == null || fileNames.Length == 0)
                return false;
            if (dataStrings == null || dataStrings.Length == 0)
                return false;
            if (fileNames.Length != dataStrings.Length)
                return false;
            if (fileNames.Any(string.IsNullOrEmpty))
                return false;
            if (dataStrings.Any(string.IsNullOrEmpty))
                return false;

            CsvStoreContextData csvStoreContextData;
            try
            {
                var alreadyFiles = Directory.GetFiles(_csvRootPath);
                var oldCsvRootPath = Path.Combine(_csvRootPath, "_backup_" + _appContext.AppId);
                
                if (Directory.Exists(oldCsvRootPath))
                    Directory.Delete(oldCsvRootPath, true);

                Directory.CreateDirectory(oldCsvRootPath);

                foreach (var alreadyFile in alreadyFiles)
                {
                    var alreadyFileName = Path.GetFileName(alreadyFile);
                    for (var i = 0; i < fileNames.Length; i++)
                    {
                        if (alreadyFileName.Contains(fileNames[i]))
                        {
                            File.Copy(alreadyFile, Path.Combine(oldCsvRootPath, alreadyFileName), true);
                        }
                    }
                }

                for (var i = 0; i < fileNames.Length; i++)
                {
                    await File.WriteAllTextAsync(Path.Combine(_csvRootPath, fileNames[i]), dataStrings[i], System.Text.Encoding.UTF8);
                }

                csvStoreContextData = new CsvStoreContextData(_appContext.IsUnitTest, _csvRootPath);
                csvStoreContextData.LoadCsvDataAll();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Load CsvData failed");
                return false;
            }

            _csvStoreContextData = csvStoreContextData;
            _csvVersion = updateTime.Ticks;

            foreach (var item in _csvDataChangedCallbackMap)
            {
                await item.Value.Invoke();
            }

            return true;
        }

        public CsvStoreContextData GetData()
        {
            return _csvStoreContextData;
        }
    }
}
