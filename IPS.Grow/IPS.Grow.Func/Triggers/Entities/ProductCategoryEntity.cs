using IPS.Grow.Func.Configs;
using IPS.Grow.Func.Entities.Cosmos;
using IPS.Grow.Func.Extentions;
using IPS.Grow.Func.Models;
using IPS.Grow.Func.Services;
using IPS.Grow.Func.Utilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using System.Net;
using Cosmos = IPS.Grow.Func.Entities.Cosmos;

namespace IPS.Grow.Func.Triggers.Entities;

internal class ProductCategoryEntity(ICosmosService cosmosService) : TaskEntity<ProductCategoryState?>, IBaseEntity<ProductCategoryMessage>
{
    private static readonly string Pk = ProductLookupService.CategoryPk;
    internal static EntityInstanceId CreateEntityId(BusinessId bid) => new(nameof(ProductCategoryEntity), bid.Idetifier);
    private readonly Task<Container> _container = cosmosService.GetContainerAsync(nameof(ContainerNames.ProductCategory));
    [Function(nameof(ProductCategoryEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<ProductCategoryEntity>();
    }

    protected override ProductCategoryState? InitializeState(TaskEntityOperation entityOperation)
    {
        var container = _container.GetAwaiter().GetResult();
        var data = container.FindAsync<Cosmos.ProductCategoryEntity>(Context.Id.Key, Pk).GetAwaiter().GetResult();
        return data != null ? new ProductCategoryState
        {
            Name = data.Name,
            Created = data.Created,
            LastModified = data.LastModified,
        } : base.InitializeState(entityOperation);
    }
    public async Task<bool> DeleteAsync()
    {
        var container = await _container;
        var operations = new List<PatchOperation>()
        {
            PatchOperation.Set($"/{(State!.Created.HasValue ? CosmosProperty.LastModified : CosmosProperty.Created)}",  DateTime.UtcNow),
            PatchOperation.Set($"/{CosmosProperty.Status}",ProductStatusType.Obsoleted.ToString() ),
        };
        var res = await container.PatchItemAsync<Cosmos.ProductCategoryEntity>(Context.Id.Key, new PartitionKey(Pk), operations);
        State = null;
        return res.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> UpsertAsync(ProductCategoryMessage input)
    {
        var container = await _container;
        var product = new Cosmos.ProductCategoryEntity
        {
            Id = Context.Id.Key,
            Name = input!.Name,
            Pk = Pk,
            Status = ProductStatusType.Active,
            Created = State!.Created.HasValue ? State!.Created : DateTime.UtcNow,
            LastModified = State!.Created.HasValue ? DateTime.UtcNow : null,
        };

        var res = await container.UpsertItemAsync(product, new PartitionKey(Pk));
        if (res.StatusCode == HttpStatusCode.OK)
        {
            State = new ProductCategoryState
            {
                Name = res.Resource.Name,
                Created = res.Resource?.Created,
                LastModified = res.Resource?.LastModified,
            };

            return true;
        }

        return false;
    }
}
