﻿using IPS.Grow.Func.Configs;
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

internal class ProductEntity(ICosmosService cosmosService) : TaskEntity<ProductState?>, IBaseEntity<ProductMessage>
{
    private static readonly string Pk = ProductLookupService.ProductPk;
    internal static EntityInstanceId CreateEntityId(BusinessId bid) => new(nameof(ProductEntity), bid.Idetifier);
    private readonly Task<Container> _container = cosmosService.GetContainerAsync(nameof(ContainerNames.Product));
    [Function(nameof(ProductEntity))]
    public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
    {
        return dispatcher.DispatchAsync<ProductEntity>();
    }

    protected override ProductState? InitializeState(TaskEntityOperation entityOperation)
    {
        var container = _container.GetAwaiter().GetResult();
        var data = container.FindAsync<Cosmos.ProductEntity>(Context.Id.Key, Pk).GetAwaiter().GetResult();
        return data != null ? new ProductState
        {
            Name = data.Name,
            Price = data.Price,
            Categories = data.Categories,
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
        var res = await container.PatchItemAsync<Cosmos.ProductEntity>(Context.Id.Key, new PartitionKey(Pk), operations);
        State = null;
        return res.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> UpsertAsync(ProductMessage input)
    {
        var container = await _container;
        var product = new Cosmos.ProductEntity
        {
            Id = Context.Id.Key,
            Name = input!.Name,
            Price = input!.Price,
            Categories = input!.Categories,
            Pk = Pk,
            Status = ProductStatusType.Active,
            Created = State!.Created.HasValue ? State!.Created : DateTime.UtcNow,
            LastModified = State!.Created.HasValue ? DateTime.UtcNow : null,
        };

        var res = await container.UpsertItemAsync(product, new PartitionKey(Pk));
        if (res.StatusCode == HttpStatusCode.OK)
        {
            State = new ProductState
            {
                Name = res.Resource.Name,
                Price = res.Resource.Price,
                Categories = res.Resource.Categories,
                Created = res.Resource?.Created,
                LastModified = res.Resource?.LastModified,
            };

            return true;
        }

        return false;
    }
}
