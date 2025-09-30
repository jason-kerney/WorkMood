using System.Text.Json;

namespace WorkMood.MauiApp.Shims;

public class JsonSerializerShim : IJsonSerializerShim
{
    public string Serialize<T>(T value, JsonSerializerOptions options) => JsonSerializer.Serialize(value, options);
    public T Deserialize<T>(string json, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(json, options);
}
