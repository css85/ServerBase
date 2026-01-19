using System.Text.Json;

namespace Shared.Server.Extensions
{
    public class JsonPacketSerializerEncryptOptions
    {
        public static JsonSerializerOptions Default { get; }

        static JsonPacketSerializerEncryptOptions()
        {
            Default = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                IncludeFields = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                MaxDepth = 100_000, // depth를 전역으로 사용하고있어서 동시에 JsonSerializer를 사용할때 depth가 누적되어서 높게설정
            };
        }
    }
}
