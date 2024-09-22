using System.Text.Json.Serialization;
using static System.ArgumentException;
namespace IPS.Grow.Func.Models;

public enum BrokerOperation
{
    Upsert,
    Delete
}
public class BrokerMessage<TData> where TData : class
{
    public Guid MessageId { get; set; }
    public DateTime Timestamp { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BrokerOperation Operation { get; set; }
    public BusinessId Bid { get; set; }
    public TData? Data { get; set; }
}

public enum BusinessObjectType
{
    Unknown = default,
    Product,
    ProductCategories
}

public struct BusinessId(string idetifier, BusinessObjectType type)
{
    private const string _separator = "|";
    public string Idetifier { get; set; } = idetifier;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BusinessObjectType Type { get; set; } = type;

    public static BusinessId Parse(string value)
    {
        ThrowIfNullOrWhiteSpace(value);
        var values = value.Split(_separator);
        return values.Length switch
        {
            1 => new(values[0], BusinessObjectType.Unknown),
            2 => new(values[0], Enum.TryParse<BusinessObjectType>(values[1], out var objectType) ? objectType : BusinessObjectType.Unknown),
            _ => throw new NotSupportedException()
        };
    }
}