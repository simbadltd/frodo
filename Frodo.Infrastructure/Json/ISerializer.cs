using System;

namespace Frodo.Infrastructure.Json
{
    public interface ISerializer
    {
        T Deserialize<T>(string data);

        object Deserialize(string data, Type type);

        string Serialize(object obj);
    }
}