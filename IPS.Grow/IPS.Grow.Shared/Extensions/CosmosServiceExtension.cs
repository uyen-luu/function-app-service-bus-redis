using IPS.Grow.Shared.Configs;
using IPS.Grow.Shared.Entities.Cosmos;
using IPS.Grow.Shared.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPS.Grow.Shared.Extensions;

public static class CosmosServiceExtension
{
    public static async Task<T?> FindAsync<T>(this Container container, string id, string pk, CancellationToken ct = default) where T : BaseEntity
    {
        try
        {
            var entity = await container.ReadItemAsync<T>(id, new PartitionKey(pk), cancellationToken: ct).ConfigureAwait(false);
            return entity;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        };
    }

    public static async Task<bool> DeleteAsync<T>(this Container container, string id, string pk, CancellationToken ct = default) where T : BaseEntity
    {
        try
        {
            _ = await container.DeleteItemAsync<T>(id, new PartitionKey(pk), cancellationToken: ct).ConfigureAwait(false);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public static async Task<List<T>> FetchAsync<T>(
       this Container container,
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        var options = new QueryRequestOptions() { MaxItemCount = -1, MaxConcurrency = -1, MaxBufferedItemCount = -1 };
        if (!string.IsNullOrWhiteSpace(partitionKey))
        {
            options.PartitionKey = new PartitionKey(partitionKey);
        }
        var feed = container.GetItemQueryIterator<T>(
            query,
            null,
            options);

        while (feed.HasMoreResults)
        {
            var values = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            result.AddRange(values);
        }

        return result;
    }

    public static async Task<T?> FetchSingleAsync<T>(
        this Container container,
        QueryDefinition query,
        string? partitonKey = null,
        CancellationToken cancellationToken = default)
    {
        var result = await container.FetchAsync<T>(query, partitonKey, cancellationToken);

        if (result.Count != 0)
        {
            return result.Single();
        }

        return default;
    }

    public static Task<List<TResult>> FetchAllAsync<T, TResult>(this Container container,
                                                                Expression<Func<T, TResult>> resultSelector,
                                                                string partitionKey,
                                                                CancellationToken cancellationToken = default) where T : BaseEntity
        => container.FetchAsync(i => i.Pk == partitionKey, resultSelector, partitionKey, cancellationToken);

    public static async Task<TResult?> FetchSingleAsync<T, TResult>(this Container container,
                                                                Expression<Func<T, bool>> whereExpression,
                                                                Expression<Func<T, TResult>> resultSelector,
                                                                string partitionKey,
                                                                CancellationToken cancellationToken = default) where T : BaseEntity where TResult : class
    {
        var result = await container.FetchAsync(whereExpression, resultSelector, partitionKey, cancellationToken);
        return result.Count == 0 ? null : result.Single();
    }

    public static async Task<List<TResult>> FetchAsync<T, TResult>(this Container container,
                                                                   Expression<Func<T, bool>> whereExpression,
                                                                   Expression<Func<T, TResult>> resultSelector,
                                                                   string? partitionKey = null,
                                                                   CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var result = new List<TResult>();
        var feed = container.GetItemLinqQueryable<T>(
            requestOptions: new QueryRequestOptions()
            {
                MaxItemCount = -1,
                MaxConcurrency = -1,
                MaxBufferedItemCount = -1,
                PartitionKey = partitionKey != null ? new PartitionKey(partitionKey) : null
            })
            .Where(whereExpression)
            .Select(resultSelector)
            .ToFeedIterator();

        while (feed.HasMoreResults)
        {
            var values = await feed.ReadNextAsync(cancellationToken);
            result.AddRange(values);
        }

        return result;
    }

    public static async Task<Container> InitContainerAsync(this Database dbClient, string containerName, string partitionKeyPath)
    {
        var containerBuilder = dbClient!.DefineContainer(containerName, partitionKeyPath);
        var containerResponse = await containerBuilder!.CreateIfNotExistsAsync();
        return containerResponse!.Container;
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
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                },
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromMinutes(1), // increased from 30 seconds to 60 seconds
                AllowBulkExecution = allowBulkExecution,
            };

            return new CosmosClient(config.ConnectionString, options);
        });
        return services.AddSingleton<ICosmosService, CosmosService>();
    }

}