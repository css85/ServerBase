using System;
using System.Collections.Generic;
using System.Text.Json;
using StackExchange.Redis;

namespace SampleGame.Shared.Common
{
    public static class JsonTextSerializer
    {
        public static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IncludeFields = true,
            Converters = {new BigIntegerConverter()}

        };

        public static string Serialize<T>(T data)
        {
            return JsonSerializer.Serialize(data, jsonOptions);
//            return JsonSerializer.Serialize(data, SystemTextJsonSerializationOptions.Default);
        }
        public static string Serialize(object data, Type type)
        {
            return JsonSerializer.Serialize(data, type, jsonOptions);
//            return JsonSerializer.Serialize(data, type, SystemTextJsonSerializationOptions.Default);
        }
        public static byte[] SerializeUtf8Bytes(object data, Type type)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data, type, jsonOptions);
//            return JsonSerializer.SerializeToUtf8Bytes(data, type,SystemTextJsonSerializationOptions.Default);
        }
        
        public static object Deserialize(string text, Type type)
        {
            return JsonSerializer.Deserialize(text, type, jsonOptions);
//            return JsonSerializer.Deserialize(text, type, SystemTextJsonSerializationOptions.Default);
        }
        
        public static object DeserializeUtf8Bytes(byte[] bytes, Type type)
        {
            return JsonSerializer.Deserialize(bytes, type, jsonOptions);
            //            return JsonSerializer.Deserialize(bytes, type, SystemTextJsonSerializationOptions.Default);
        }
        public static T Deserialize<T>(string text)
        {
            return JsonSerializer.Deserialize<T>(text, jsonOptions);
//            return JsonSerializer.Deserialize<T>(text, SystemTextJsonSerializationOptions.Default);
        }

        public static object Deserialize(ReadOnlySpan<byte> text,Type type)
        {
            return JsonSerializer.Deserialize(text, type, jsonOptions);
//            return JsonSerializer.Deserialize(text, type,SystemTextJsonSerializationOptions.Default);
        }

        public static List<T> DeserializeArray<T>(HashEntry[] array)
        {
            if (array.Length < 1)
                return default;
            
            var items = new List<T>(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                if(array[i].Value.IsNullOrEmpty)
                    continue;

                items.Add(JsonSerializer.Deserialize<T>(array[i].Value.ToString(), jsonOptions));
//                items.Add(JsonSerializer.Deserialize<T>(array[i].Value,SystemTextJsonSerializationOptions.Default));
            }

            return items;
        }

        public static List<T> DeserializeArray<T>(RedisValue[] array)
        {
            if (array.Length < 1)
                return default;
            
            var items = new List<T>(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                if(array[i].IsNullOrEmpty)
                    continue;

                items.Add(JsonSerializer.Deserialize<T>(array[i].ToString(), jsonOptions));
//                items.Add(JsonSerializer.Deserialize<T>(array[i], SystemTextJsonSerializationOptions.Default));
            }

            return items;
        }

}
}
