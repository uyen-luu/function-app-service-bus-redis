using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Configs;
using IPS.Grow.Func.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.ArgumentException;

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

    public static IServiceCollection AddCosmosService(this IServiceCollection services, bool allowBulkExecution = false)
    {
        services.AddOptions<CosmosConfiguration>().Configure<IConfiguration>((o, c) => c.GetSection(nameof(CosmosConfiguration)).Bind(o));
        services.AddSingleton(sp =>
        {
            var config = sp.GetOptionsValue<CosmosConfiguration>();
            var options = new CosmosClientOptions()
            {
                UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                },
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromMinutes(1), // increased from 30 seconds to 60 seconds
                AllowBulkExecution = allowBulkExecution,
            };

            return new CosmosClient(config.ConnectionString, options);
        });
        return services.AddSingleton<ICosmosService, CosmosService>()
            .AddSingleton<IProductLookupService, ProductLookupService>();
    }

    public static TData GetValue<TData>(this IOptions<TData> options, params string[] paramNames) where TData : class
    {
        var config = options.Value;
        foreach (var item in paramNames)
        {
            var (PropertyName, _, PropertyValue) = config.GetPropertyValue(item);
            ThrowIfNullOrEmpty(PropertyValue?.ToString(), PropertyName);
        }

        return config;
    }

    public static TData GetOptionsValue<TData>(this IServiceProvider provider, params string[] paramNames) where TData : class
    {
        var configOptions = provider.GetRequiredService<IOptions<TData>>();
        return configOptions.GetValue(paramNames);
    }

    public static (string PropertyName, bool PropertyExists, object? PropertyValue) GetPropertyValue<TData>(this TData? obj, string propertyName) where TData : class
    {
        if (obj == null)
            return (propertyName, false, null);
        //
        var objectType = obj.GetType();
        var prop = objectType.GetProperty(propertyName);
        var propName = $"{objectType.Name}.{propertyName}";
        if (prop != null)
            return (propName, true, prop.GetValue(obj));
        //
        var properties = obj.GetType().GetProperties();
        var nestedProperties = properties.Where(p => p.PropertyType.IsClass && !p.PropertyType.IsValueType);
        foreach (var property in nestedProperties)
        {
            var propValue = property.GetValue(obj);
            if (propValue is not string)
            {
                (var nestedPropName, var propExists, var nestedPropValue) = property.GetValue(obj).GetPropertyValue(propertyName);
                if (propExists)
                {
                    return ($"{objectType.Name}.{nestedPropName}", propExists, nestedPropValue);
                }
            }
        }

        return (propName, false, null);
    }
}
