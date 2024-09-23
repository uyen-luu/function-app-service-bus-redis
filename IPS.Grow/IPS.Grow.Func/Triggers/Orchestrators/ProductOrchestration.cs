using IPS.Grow.Func.Models;
using IPS.Grow.Func.Triggers.Entities;
using IPS.Grow.Func.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace IPS.Grow.Func.Triggers.Orchestrators;

internal class ProductOrchestration(ILogger<ProductOrchestration> logger)
{
    [Function(OrchestratorFunctions.ProductOrchestrator)]
    public Task RunProductOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
        => RunOrchestrator<ProductMessage>(context, ProductEntity.CreateEntityId);

    [Function(OrchestratorFunctions.ProductCategoryOrchestrator)]
    public Task RunProductCategoryOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
        => RunOrchestrator<ProductCategoryMessage>(context, ProductCategoryEntity.CreateEntityId);

    private async Task RunOrchestrator<TData>(TaskOrchestrationContext context,
                                              Func<BusinessId, EntityInstanceId> getEntityId) where TData : class
    {
        var input = context.GetInput<BrokerMessage<TData>>()!;
        try
        {
            var (operationName, operationInput) = input.Operation switch
            {
                BrokerOperation.Upsert => (nameof(IBaseEntity<TData>.UpsertAsync), input.Data),
                BrokerOperation.Delete => (nameof(IBaseEntity<TData>.DeleteAsync), null),
                _ => throw new NotImplementedException()
            };
            var entityId = getEntityId(input.Bid);
            var res = await context.Entities.CallEntityAsync<bool>(entityId, operationName, operationInput);
            if (res)
            {
                await context.CallActivityAsync(ActivityFunctions.RemoveCacheKey, input.Bid);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());

            throw;
        }
    }
}
