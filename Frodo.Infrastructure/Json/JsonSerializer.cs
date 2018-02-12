using System;
using Newtonsoft.Json;

namespace Frodo.Infrastructure.Json
{
    public sealed class JsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, CreateSettings());
        }

        public object Deserialize(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type, CreateSettings());
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, CreateSettings());
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };
        }
    }
}
