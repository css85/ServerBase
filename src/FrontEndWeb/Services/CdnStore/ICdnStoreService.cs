using System.Threading.Tasks;

namespace FrontEndWeb.Services.CdnStore
{
    public enum CdnStoreType
    {
        Local,
        Aws,
    }

    public interface ICdnStoreService
    {
        public Task<string> UploadFileAsync(byte[] bytes);
    }
}
