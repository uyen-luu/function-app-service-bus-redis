using IPS.Grow.Shared.Entities.Cosmos;
using System.Text.Json.Serialization;

namespace IPS.Grow.Func.Entities.Cosmos;

public class ProductCategoryEntity : BaseEntity
{
    public required string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatusType Status { get; set; }
}
