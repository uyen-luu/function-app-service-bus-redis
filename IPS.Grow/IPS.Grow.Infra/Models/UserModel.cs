using IPS.Grow.Infra.AccessControl;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IPS.Grow.Infra.Models;

public class LoginModel
{
    /// <summary>
    /// User identifier - email address
    /// </summary>
    /// <example>uyen.luu@enlabsoftware.com</example>
    public required string Username { get; init; }
    /// <summary>
    /// Your password
    /// </summary>
    /// <example>my-password</example>
    public required string Password { get; init; }
    /// <summary>
    /// Remember for 7 days if true, otherwise one day
    /// </summary>
    [DefaultValue(false)]
    public bool RememberMe { get; set; }
};

public record UserViewModel(string Username,
                            string Email,
                            string? FirstName,
                            string? LastName,
                            string? MobileNumber)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool RememberMe { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    public string? JwtToken { get; set; }
    public IList<int> Roles { get; set; } = [];
    public string? DefaultJobDocumentsPath { get; set; }
    public Claim[] GetClaims()
    {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Email, Email),
            new Claim(JwtRegisteredClaimNames.NameId, Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, Username),
            new Claim("RememberMe", RememberMe.ToString()),
        };
        if (!string.IsNullOrEmpty(MobileNumber))
        {
            claims = [.. claims, new Claim(ClaimTypes.MobilePhone, MobileNumber)];
        }
        if (!string.IsNullOrEmpty(FullName))
        {
            claims = [.. claims, new Claim(JwtRegisteredClaimNames.GivenName, FullName)];
        }
        if (Roles.Any())
        {
            claims = [.. claims, new Claim("Roles", JsonFactory.Serialize(Roles))];
        }

        return claims;
    }
}

public record UserProfileModel(string EmployeeEmailSignature);

public class ChangePasswordModel
{
    public required string NewPassword { get; set; }

    [Compare(nameof(NewPassword))]
    public required string ConfirmationNewPassword { get; set; }
}
public record ResetPasswordModel
{
    public required string Email { get; set; }
}