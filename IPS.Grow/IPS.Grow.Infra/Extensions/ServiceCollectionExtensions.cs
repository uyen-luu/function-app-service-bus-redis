using IPS.Grow.Infra.Models;
using IPS.Grow.Infra.Services;
using IPS.Grow.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace IPS.Grow.Infra.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureAuth(this IHostApplicationBuilder builder,
                                                   Action<JwtSettings>? configureOptions = null,
                                                   string configureSection = nameof(JwtSettings))
    {
        builder.AddAccessControl(configureOptions, configureSection);
        return builder.Services.
            AddCosmosService().
            AddSingleton<IAuthService, AuthService>();
    }
}