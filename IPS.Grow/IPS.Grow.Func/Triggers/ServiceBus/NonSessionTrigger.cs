using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Convertors;
using IPS.Grow.Func.Models;
using IPS.Grow.Func.Triggers.Orchestrators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace IPS.Grow.Func.Triggers.ServiceBus;

public class NonSessionTrigger(ILogger<NonSessionTrigger> logger)
{
    [Function(nameof(ReceiveSingleMessage))]
    [ServiceBusOutput("%ServiceBus:QueueNames:Output%", Connection = "ServiceBus:ConnectionString")]
    public string ReceiveSingleMessage(
            [ServiceBusTrigger("%ServiceBus:QueueNames:Single%", Connection = "ServiceBus:ConnectionString")] ServiceBusReceivedMessage message)
    {
        var brokerMsg = message.Body.ToBrokerMessage<ProductMessage>();
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var outputMessage = $"Output message created at {DateTime.Now}";
        return outputMessage;
    }

    [Function(nameof(ReceiveBatchMessages))]
    public async Task ReceiveBatchMessages(
            [ServiceBusTrigger("%ServiceBus:QueueNames:Batch%", Connection = "ServiceBus:ConnectionString", IsBatched = true)] ServiceBusReceivedMessage[] messages,
            [DurableClient] DurableTaskClient starter)
    {
        foreach (ServiceBusReceivedMessage message in messages)
        {
            var brokerMsg = message.Body.ToBrokerMessage<ProductMessage>();
            var taskName = new TaskName(nameof(ProductOrchestration));
            await starter.ScheduleNewOrchestrationInstanceAsync(taskName, brokerMsg);
            //logger.LogInformation("Message ID: {id}", message.MessageId);
            //logger.LogInformation("Message Body: {body}", message.Body);
            //logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }

    //[Function(nameof(MessageAction))]
    //public async Task MessageAction(
    //        [ServiceBusTrigger("%ServiceBus:QueueNames:NoSession%", Connection = "ServiceBus:ConnectionString", AutoCompleteMessages = false)]
    //        ServiceBusReceivedMessage message,
    //        ServiceBusMessageActions messageActions)
    //{
    //    logger.LogInformation("Message ID: {id}", message.MessageId);
    //    logger.LogInformation("Message Body: {body}", message.Body);
    //    logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

    //    // Complete the message
    //    await messageActions.CompleteMessageAsync(message);
    //}
}
