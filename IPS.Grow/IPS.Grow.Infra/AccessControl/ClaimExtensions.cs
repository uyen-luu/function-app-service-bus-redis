using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IPS.Grow.Infra.AccessControl;

public static class ClaimExtensions
{
    public static string? GetString(this ClaimsPrincipal principal, string claimName)
    {
        claimName = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.TryGetValue(claimName, out string? claimType) ? claimType : claimName;
        return principal.FindFirstValue(claimName);
    }

    public static TValue? GetValue<TValue>(this ClaimsPrincipal principal, string claimName) where TValue : struct
    {
        var claimValue = principal.GetString(claimName);
        return claimValue != null && TryParse(claimValue!, out TValue value) ? value : default;
    }

    private static bool TryParse<T>(string input, out T value) where T : struct
    {
        var type = typeof(T);
        var tryParseMethod = type.GetMethod("TryParse", [typeof(string), type.MakeByRefType()]);

        if (tryParseMethod != null)
        {
            var parameters = new object?[] { input, null };
            var success = (bool)tryParseMethod.Invoke(null, parameters)!;
            if (parameters[1] is T outVal)
            {
                value = outVal;
            }
            else value = default;
            return success;
        }

        value = default;
        return false;
    }
}
