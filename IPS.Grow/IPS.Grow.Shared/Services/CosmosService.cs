using IPS.Grow.Shared.Configs;
using IPS.Grow.Shared.Extensions;
using IPS.Grow.Shared.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using static System.ArgumentException;

namespace IPS.Grow.Shared.Services;

public interface ICosmosService
{
    Task<Container> GetContainerAsync(string containerKey);
}

internal class CosmosService : ICosmosService
{
    private readonly ConcurrentDictionary<string, CustomAsyncLazy<Container>> _lazyContainers;
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosConfiguration _cosmosConfiguration;
    private readonly CustomAsyncLazy<Database> _lazyDatabase;
    public CosmosService(CosmosClient cosmosClient, IOptions<CosmosConfiguration> options)
    {
        _cosmosClient = cosmosClient;
        _cosmosConfiguration = options.GetValue(nameof(CosmosConfiguration.DatabaseName), nameof(CosmosConfiguration.ContainerNames));
        _lazyDatabase = new(() => InitDatabaseAsync(_cosmosConfiguration.DatabaseName!));
        _lazyContainers = new ConcurrentDictionary<string, CustomAsyncLazy<Container>>();
    }

    public async Task<Container> GetContainerAsync(string containerKey)
    {
        var lazyContainer = GetLazyContainer(containerKey);
        return await lazyContainer.GetValue();
    }

    private CustomAsyncLazy<Container> GetLazyContainer(string containerKey)
    {
        var lazyContainer = _lazyContainers.GetOrAdd(containerKey, (key) =>
        {
            var (_, _, containerName) = _cosmosConfiguration.ContainerNames.GetPropertyValue(key);
            ThrowIfNullOrEmpty(containerName?.ToString(), key);
            return new CustomAsyncLazy<Container>(async () =>
            {
                var dbClient = await _lazyDatabase.GetValue();
                var container = await dbClient.CreateContainerIfNotExistsAsync(containerName!.ToString(), "/pk");
                return container;
            });
        });
        return lazyContainer;
    }

    private async Task<Database> InitDatabaseAsync(string databaseName)
    {
        var res = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).ConfigureAwait(false);
        return res.Database;
    }
}