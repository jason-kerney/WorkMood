using System.Text.Json;

namespace WorkMood.MauiApp.Shims;

public interface IJsonSerializerShim
{
    string Serialize<T>(T value, JsonSerializerOptions options);
    T Deserialize<T>(string json, JsonSerializerOptions options);
}
