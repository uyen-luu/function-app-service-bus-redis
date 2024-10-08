using System.Text.Json.Serialization;

namespace IPS.Grow.Func.Entities.Cosmos;

public enum ProductStatusType
{
    Unknown,
    Active,
    Obsoleted
}
public class ProductEntity : BaseEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatusType Status { get; set; }
    public int[] Categories { get; set; } = [];
}