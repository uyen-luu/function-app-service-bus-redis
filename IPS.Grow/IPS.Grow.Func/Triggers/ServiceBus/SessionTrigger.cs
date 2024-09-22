using IPS.Grow.Func.Services;
using Microsoft.Extensions.Logging;

namespace IPS.Grow.Func.Triggers.ServiceBus;

public class SessionTrigger(ILogger<SessionTrigger> logger, IOrderedListClient client)
{
    //[Function(nameof(SessionTrigger))]
    //public async Task Run(
    //    [ServiceBusTrigger("%ServiceBus:QueueNames:Session%", Connection = "ServiceBus:ConnectionString", IsSessionsEnabled = true)]
    //    ServiceBusReceivedMessage message,
    //    ServiceBusMessageActions messageActions)
    //{
    //    logger.LogInformation("Message ID: {id}", message.MessageId);
    //    logger.LogInformation("Message Body: {body}", message.Body);
    //    logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

    //    logger.LogInformation($"C# ServiceBus queue trigger function processed message: {Encoding.UTF8.GetString(message.Body)}");
    //    await client.PushData(message.SessionId, Encoding.UTF8.GetString(message.Body));
    //    // Complete the message
    //    await messageActions.CompleteMessageAsync(message);
    //}
}
