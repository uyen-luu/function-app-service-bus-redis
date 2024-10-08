using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Configs;
using IPS.Grow.Func.Services;
using IPS.Grow.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace IPS.Grow.Func.Extentions;

internal static class ServiceCollectionExtension
{
    public static IServiceCollection AddRedisClient(this IServiceCollection services)
    {
        services.AddOptions<RedisConfig>().BindConfiguration(RedisConfig.Section);
        services.AddSingleton((sp) =>
        {
            var config = sp.GetOptionsValue<RedisConfig>(nameof(RedisConfig.ConnectionString));
            var options = ConfigurationOptions.Parse(config.ConnectionString);
            options.ConnectTimeout = 40000;
            var cm = ConnectionMultiplexer.Connect(options);
            return cm;
        });
        return services.AddSingleton<ICacheService, CacheService>();
    }

    public static IServiceCollection AddServiceBus(this IServiceCollection services)
    {
        services.AddOptions<ServiceBusConfig>().BindConfiguration(ServiceBusConfig.Section);
        services.AddSingleton(sp =>
        {
            var config = sp.GetOptionsValue<ServiceBusConfig>(nameof(ServiceBusConfig.ConnectionString));
            return new ServiceBusClient(config.ConnectionString);
        });

        return services;
    }
}
