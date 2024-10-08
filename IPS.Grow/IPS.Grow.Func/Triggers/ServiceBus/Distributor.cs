using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Convertors;
using IPS.Grow.Func.Models;
using IPS.Grow.Func.Utilities;
using IPS.Grow.Shared.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace IPS.Grow.Func.Triggers.ServiceBus;

public class Distributor(ILogger<Distributor> logger)
{
    [Function(nameof(ReceiveSingleMessage))]
    [ServiceBusOutput("%ServiceBus:QueueNames:Output%", Connection = "ServiceBus:ConnectionString")]
    public string ReceiveSingleMessage(
            [ServiceBusTrigger("%ServiceBus:QueueNames:Single%", Connection = "ServiceBus:ConnectionString")] ServiceBusReceivedMessage message)
    {
        var brokerMsg = message.Body.ToBrokerMessage<ProductMessage>();
        var outputMessage = $"Output message created at {DateTime.Now}";
        return MessageSerializer.Serialize(brokerMsg);
    }

    [Function(nameof(ReceiveBatchMessages))]
    public async Task ReceiveBatchMessages(
            [ServiceBusTrigger("%ServiceBus:QueueNames:Batch%", Connection = "ServiceBus:ConnectionString", IsBatched = true)] ServiceBusReceivedMessage[] messages,
            [DurableClient] DurableTaskClient starter)
    {
        foreach (ServiceBusReceivedMessage message in messages)
        {
            var brokerMsg = message.Body.ToBrokerMessage<dynamic>();

            var taskName = brokerMsg.Bid.Type switch
            {
                BusinessObjectType.Product => new TaskName(OrchestratorFunctions.ProductOrchestrator),
                BusinessObjectType.ProductCategories => new TaskName(OrchestratorFunctions.ProductCategoryOrchestrator),
                _ => throw new NotImplementedException()
            };
            await starter.ScheduleNewOrchestrationInstanceAsync(taskName, brokerMsg);
        }
    }
}
