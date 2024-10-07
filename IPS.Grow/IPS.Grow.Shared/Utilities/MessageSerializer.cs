using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPS.Grow.Shared.Utilities;

public static class MessageSerializer
{
    public static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            {
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                return options;
            }
        }
    }
    public static string Serialize<TData>(TData input) where TData : class
        => JsonSerializer.Serialize(input, JsonSerializerOptions)!;

    public static TData? Deserialize<TData>(string input) where TData : class
        => JsonSerializer.Deserialize<TData>(input, JsonSerializerOptions);
}
