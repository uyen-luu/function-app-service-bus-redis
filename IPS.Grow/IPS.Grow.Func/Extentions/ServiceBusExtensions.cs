using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Models;
using IPS.Grow.Func.Utilities;

namespace IPS.Grow.Func.Extentions;

internal static class ServiceBusExtensions
{
    public static Task SendMessageAsync(this ServiceBusClient client,
                                        string queueName,
                                        CancellationToken ct = default,
                                        params ServiceBusMessage[] messages)
    {
        var sender = client.CreateSender(queueName);
        if (messages.Length == 0)
        {
            return Task.CompletedTask;
        }
        //
        return messages.Length == 1 ? sender.SendMessageAsync(messages.First(), ct) : sender.SendMessagesAsync(messages, ct);
    }

    public static ServiceBusMessage[] ToServiceBusMessages(this (BrokerMessage<ProductMessage>[] Products, BrokerMessage<ProductCategoryMessage>[] Categories) input)
    {
        var productMessages = input.Products
            .Select(p => new ServiceBusMessage(MessageSerializer.Serialize(p)));
        var categoryMessages = input.Categories
            .Select(p => new ServiceBusMessage(MessageSerializer.Serialize(p)));

        return productMessages.Union(categoryMessages).ToArray();
    }
}
