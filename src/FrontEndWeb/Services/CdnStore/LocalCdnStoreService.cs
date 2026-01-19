using System;
using System.IO;
using System.Threading.Tasks;
using Common.Config;
using FrontEndWeb.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.CdnStore
{
    public class LocalCdnStoreService : ICdnStoreService
    {
        private readonly ILogger<LocalCdnStoreService> _logger;

        private readonly string _fileSavePath;
        private readonly string _baseUrl;

        public LocalCdnStoreService(
            ILogger<LocalCdnStoreService> logger,
            IHostEnvironment environment,
            ChangeableSettings<FrontEndAppSettings> appSettings
            )
        {
            _logger = logger;

            // wwwroot/../cdnfiles/
            _fileSavePath = Path.Combine(environment.ContentRootPath, "cdnfiles/");
            Directory.CreateDirectory(_fileSavePath);

            _baseUrl = appSettings.Value.CdnUrl + "/cdnfiles/";
        }

        public async Task<string> UploadFileAsync(string path, byte[] bytes)
        {
            var filePath = Path.Combine(_fileSavePath, path);
            await File.WriteAllBytesAsync(filePath, bytes);

            return _baseUrl + path;
        }

        public Task RemoveFileAsync(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // ignored
            }

            return Task.CompletedTask;
        }
    }
}
