using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPS.Grow.Infra.AccessControl;

public class JsonFactory
{
    private readonly static JsonSerializerOptions _jsonOptions = ConfigureOptions(new JsonSerializerOptions());

    public static JsonSerializerOptions ConfigureOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.AllowTrailingCommas = true;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _jsonOptions);
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, _jsonOptions);
}