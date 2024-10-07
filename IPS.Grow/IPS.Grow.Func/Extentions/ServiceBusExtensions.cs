using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Models;
using IPS.Grow.Shared.Utilities;

namespace IPS.Grow.Func.Extentions;

internal static class ServiceBusExtensions
{
    public static Task SendMessageAsync<TData>(this ServiceBusClient client, string queueName,
                                               params BrokerMessage<TData>[] brokerMessages) where TData : class
    {
        var sender = client.CreateSender(queueName);
        var messages = brokerMessages
           .Select(p => new ServiceBusMessage(MessageSerializer.Serialize(p)));
        if (!messages.Any())
        {
            return Task.CompletedTask;
        }
        //
        return messages.Count() == 1 ? sender.SendMessageAsync(messages.First()) : sender.SendMessagesAsync(messages);
    }
}
