using IPS.Grow.Func.Models;
using IPS.Grow.Func.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net.Mime;

namespace IPS.Grow.Func.Triggers.Http;

internal class Retriever(IProductLookupService productService, ICacheService cacheService)
{
    [Function(nameof(GetProductAsync))]
    [OpenApiOperation(operationId: nameof(GetProductAsync), tags: ["Product"], Description = "Get product data")]
    [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(ProductResponse))]
    [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.NotFound)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Description = "The product Id", Required = true, Type = typeof(int))]
    public async Task<IActionResult> GetProductAsync(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id:int}")] HttpRequest req,
    int id)
    {
        var bid = new BusinessId(id.ToString(), BusinessObjectType.Product);
        var (Source, Result) = await cacheService.TryReadApiCacheAsync(
            bid,
            () => productService.ReadProductAsync(id, req.HttpContext.RequestAborted),
            req.HttpContext.RequestAborted);
        //
        if (Result != null)
        {
            req.HttpContext.Response.Headers.TryAdd("Source", Source);
            return new OkObjectResult(Result);
        }
        //
        return new NotFoundResult();
    }

    [Function(nameof(GetCategoryAsync))]
    [OpenApiOperation(operationId: nameof(GetCategoryAsync), tags: ["Product"], Description = "Get product data")]
    [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(ProductCategoryResponse))]
    [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.NotFound)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Description = "The products of category", Required = true, Type = typeof(int))]
    public async Task<IActionResult> GetCategoryAsync(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "categories/{id:int}/products")] HttpRequest req,
    int id)
    {
        var bid = new BusinessId(id.ToString(), BusinessObjectType.ProductCategories);
        var (Source, Result) = await cacheService.TryReadApiCacheAsync(
            bid,
            () => productService.ReadProductsAsync(id, req.HttpContext.RequestAborted),
            req.HttpContext.RequestAborted);
        //
        if (Result != null)
        {
            req.HttpContext.Response.Headers.TryAdd("Source", Source);
            return new OkObjectResult(Result);
        }
        //
        return new NotFoundResult();
    }
}