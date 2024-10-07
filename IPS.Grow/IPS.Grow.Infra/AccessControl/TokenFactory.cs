using IPS.Grow.Infra.Entities.Cosmos;
using IPS.Grow.Infra.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IPS.Grow.Infra.AccessControl;
public class TokenFactory(JwtSettings settings)
{
    public readonly JwtSecurityTokenHandler TokenHandler = new();
    public short RefreshExpiryDays => settings.RefreshExpiryDays;
    public TokenValidationParameters GetTokenValidationParameters()
     => new()
     {
         ValidateIssuerSigningKey = true,
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidIssuer = settings.Issuer,
         ValidAudience = settings.Audience,
         IssuerSigningKey = SymmetricSecurityKey,
     };

    private SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.UTF8.GetBytes(settings.SecretKey));
    public string WriteToken(params Claim[] claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(settings.Issuer,
                                                   settings.Audience,
                                                   claims,
                                                   expires: DateTime.UtcNow.AddHours(settings.ExpiryDurationHours),
                                                   signingCredentials: credentials);

        return TokenHandler.WriteToken(tokenDescriptor);
    }

    public ClaimsPrincipal ValidateToken(string token, out SecurityToken validatedToken)
        => TokenHandler.ValidateToken(token,
                                       GetTokenValidationParameters(),
                                       out validatedToken);

    public RefreshTokenEntity BuildRefreshToken(string? ipAddress, string userName, bool rememberMe)
    {
        return new RefreshTokenEntity
        {
            Id = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            // Remembered: forever; else within than one day.
            ExpiryDate = rememberMe ? DateTime.UtcNow.AddDays(settings.RefreshExpiryDays) : DateTime.UtcNow.AddHours(12),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            Pk = userName
        };
    }

    public RefreshTokenEntity RenewRefreshToken(RefreshTokenEntity oldtoken)
        => BuildRefreshToken(oldtoken.CreatedByIp, oldtoken.Pk, (oldtoken.Created - oldtoken.ExpiryDate).Days <= settings.RefreshExpiryDays);
}