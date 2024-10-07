using System.Text.Json.Serialization;

namespace IPS.Grow.Shared.Entities.Cosmos;

public class BaseEntity
{
    [JsonPropertyOrder(int.MinValue)]
    public required string Id { get; set; }
    [JsonPropertyOrder(int.MinValue)]
    public required string Pk { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
