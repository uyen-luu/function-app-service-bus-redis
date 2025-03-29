using IPS.Grow.Infra.AccessControl;
using IPS.Grow.Shared.Entities.Cosmos;

namespace IPS.Grow.Infra.Entities.Cosmos;

/// <summary>
/// RK = UserName
/// PK = UserName
/// </summary>
public class UserEntity : BaseEntity
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required Guid Salt { get; set; }
    public required string EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRoleType[] Roles { get; set; } = [];
}
