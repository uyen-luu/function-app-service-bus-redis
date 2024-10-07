using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Configs;
using IPS.Grow.Func.Extentions;
using IPS.Grow.Func.Models;
using IPS.Grow.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace IPS.Grow.Func.Triggers.Http;

public class Broker(ILogger<Broker> logger, ServiceBusClient sbClient, IOptions<ServiceBusConfig> options)
{
    private readonly ServiceBusConfig _config = options.GetValue();

    [Function(nameof(CreateSingleMessages))]
    [OpenApiOperation(operationId: nameof(CreateSingleMessages), tags: ["Broker"], Description = "Create product messages to sb-single")]
    [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, MediaTypeNames.Text.Plain, typeof(string))]
    public async Task<IActionResult> CreateSingleMessages(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "messages/single")] HttpRequest req)
    {
        var brokerMsg = GenerateProductUpsertMessages();
        await sbClient.SendMessageAsync(_config.QueueNames.Single, brokerMsg.ToArray()).ConfigureAwait(false);
        //
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Completed");
    }

    [Function(nameof(CreateBatchMessages))]
    [OpenApiOperation(operationId: nameof(CreateBatchMessages), tags: ["Broker"], Description = "Create product messages to sb-batch")]
    [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, MediaTypeNames.Text.Plain, typeof(string))]
    public async Task<IActionResult> CreateBatchMessages(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "messages/batch")] HttpRequest req)
    {
        var brokerMsg = GenerateProductUpsertMessages();
        await sbClient.SendMessageAsync(_config.QueueNames.Batch, brokerMsg.ToArray()).ConfigureAwait(false);
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Completed");
    }

    private static BrokerMessage<ProductMessage>[] GenerateProductUpsertMessages()
    {
        var random = new Random();
        return Enumerable.Range(1, 10)
            .Select(i =>
            {
                var product = new ProductMessage
                {
                    Name = $"Product {i}",
                    Price = random.Next(0, 100)
                };
                return new BrokerMessage<ProductMessage>
                {
                    MessageId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Bid = new BusinessId(i.ToString(), BusinessObjectType.Product),
                    Data = product,
                    Operation = BrokerOperation.Upsert
                };
            }).ToArray();
    }
}
