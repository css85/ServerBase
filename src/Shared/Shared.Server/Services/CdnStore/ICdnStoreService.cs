using System.Threading.Tasks;

namespace Shared.CdnStore
{
    public enum CdnStoreType
    {
        Local,
        Aws,
    }

    public interface ICdnStoreService
    {
        Task<string> UploadFileAsync(string path, byte[] bytes);
        Task RemoveFileAsync(string path);
    }
}
