using System.Text.Json;

namespace Shared.Server.Extensions
{
    public static class SystemTextJsonSerializationOptions
    {
        public static JsonSerializerOptions Default { get; }

        static SystemTextJsonSerializationOptions()
        {
            Default = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IncludeFields = true,
                MaxDepth = 100000, // depth를 전역으로 사용하고있어서 동시에 JsonSerializer를 사용할때 depth가 누적되어서 높게설정
            };
        }
    }
}
