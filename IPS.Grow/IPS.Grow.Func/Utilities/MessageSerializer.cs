using IPS.Grow.Func.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPS.Grow.Func.Utilities;

internal static class MessageSerializer
{
    private static JsonSerializerOptions JsonSerializerOptions
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

    public static BrokerMessage<TData> ToBrokerMessage<TData>(this BinaryData input) where TData : class
       => input.ToObjectFromJson<BrokerMessage<TData>>(JsonSerializerOptions)!;
}
