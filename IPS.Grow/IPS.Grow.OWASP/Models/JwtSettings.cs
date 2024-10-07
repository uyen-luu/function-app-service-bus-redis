namespace IPS.Grow.OWASP.Models;

public class JwtSettings
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public short ExpiryDurationHours { get; set; }
    public short RefreshExpiryDays { get; set; }
    public const string RefreshTokenKey = "refreshToken";
}