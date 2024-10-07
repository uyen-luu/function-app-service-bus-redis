namespace IPS.Grow.OWASP.Entities.Cosmos;

/// <summary>
/// RK = UserName
/// PK = EmailAddress
/// </summary>
public class User : BaseEntity
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required Guid Salt { get; set; }
    public required string EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
