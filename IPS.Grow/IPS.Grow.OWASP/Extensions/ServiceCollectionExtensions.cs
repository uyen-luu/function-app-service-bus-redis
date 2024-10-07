using IPS.Grow.OWASP.AccessControl;
using IPS.Grow.OWASP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.ArgumentNullException;
namespace IPS.Grow.OWASP.Extensions;

internal static class ServiceCollectionExtensions
{
    public static AuthenticationBuilder AddIpsAuth(this IHostApplicationBuilder builder, Action<JwtSettings>? configureOptions = null, string configureSection = nameof(JwtSettings))
    {
        var jwtSettings = builder.Configuration.Get<JwtSettings>();
        builder.Configuration.GetSection(configureSection).Bind(jwtSettings);
        ThrowIfNull(jwtSettings);
        configureOptions ??= (o) => { };
        configureOptions(jwtSettings);
        var tokenFactory = new TokenFactory(jwtSettings);
        builder.Services.AddSingleton(tokenFactory);
        return builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = tokenFactory.GetTokenValidationParameters();
        });
    }
}
