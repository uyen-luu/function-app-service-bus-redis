using IPS.Grow.Shared.Entities.Cosmos;

namespace IPS.Grow.Infra.Entities.Cosmos;

/// <summary>
/// RK = Token
/// PK = UserName|EmailAddress
/// </summary>
public partial class RefreshTokenEntity : BaseEntity
{
    public DateTime ExpiryDate { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public DateTime? RevokedDate { get; set; }
}