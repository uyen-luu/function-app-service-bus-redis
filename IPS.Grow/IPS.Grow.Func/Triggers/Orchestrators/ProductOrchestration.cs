using IPS.Grow.Func.Models;
using IPS.Grow.Func.Triggers.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Net;

namespace IPS.Grow.Func.Triggers.Orchestrators;

internal class ProductOrchestration(ILogger<ProductOrchestration> logger)
{
    [Function(nameof(ProductOrchestration))]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<BrokerMessage<ProductMessage>>()!;

        try
        {
            var (operationName, operationInput) = input.Operation switch
            {
                BrokerOperation.Upsert => (nameof(IProductEntity.UpsertAsync), input.Data),
                BrokerOperation.Delete => (nameof(IProductEntity.DeleteAsync), null),
                _ => throw new NotImplementedException()
            };
            var entityId = ProductEntity.CreateEntityId(input.Bid);
            var res = await context.Entities.CallEntityAsync<HttpStatusCode>(entityId, operationName, operationInput);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());

            throw;
        }
    }
}
