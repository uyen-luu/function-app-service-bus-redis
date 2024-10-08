using Azure.Messaging.ServiceBus;
using IPS.Grow.Func.Configs;
using IPS.Grow.Func.Extentions;
using IPS.Grow.Func.Models;
using IPS.Grow.Func.Utilities;
using IPS.Grow.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;

namespace IPS.Grow.Func.Triggers.Http;

public class Broker(ILogger<Broker> logger, ServiceBusClient sbClient, IOptions<ServiceBusConfig> options)
{
    private readonly ServiceBusConfig _config = options.GetValue();

    [Function(nameof(CreateSingleMessages))]
    [OpenApiOperation(operationId: nameof(CreateSingleMessages), tags: ["Broker"], Description = "Create product messages to sb-single")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(ApiResponse))]
    public async Task<IActionResult> CreateSingleMessages(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "messages/single")] HttpRequest req)
    {
        var res = await SendMesasgesAsync(_config.QueueNames.Single, req.HttpContext.RequestAborted).ConfigureAwait(false);
        //
        return new OkObjectResult(res);
    }

    [Function(nameof(CreateBatchMessages))]
    [OpenApiOperation(operationId: nameof(CreateBatchMessages), tags: ["Broker"], Description = "Create product messages to sb-batch")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(ApiResponse))]
    public async Task<IActionResult> CreateBatchMessages(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "messages/batch")] HttpRequest req)
    {
        var res = await SendMesasgesAsync(_config.QueueNames.Batch, req.HttpContext.RequestAborted).ConfigureAwait(false);
        //
        return new OkObjectResult(res);
    }


    #region Privates
    private async Task<ApiResponse> SendMesasgesAsync(string queueName, CancellationToken ct = default)
    {
        var brokerMsg = DataFactory.GenerateBrokerMessages();
        var serviceBusMessages = brokerMsg.ToServiceBusMessages();
        try
        {
            await sbClient.SendMessageAsync(queueName, ct, serviceBusMessages).ConfigureAwait(false);
            return new ApiResponse
            {
                Status = ApiResponseStatus.Successed,
                StatusCode = HttpStatusCode.OK,
                Data = brokerMsg,
                Message = $"All messages have been sent to Service Bus Queue \"{queueName}\""
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            return new ApiResponse
            {
                Status = ApiResponseStatus.Failed,
                StatusCode = HttpStatusCode.InternalServerError,
                Data = ex,
                Message = ex.Message
            };
        }
    }
    #endregion
}
