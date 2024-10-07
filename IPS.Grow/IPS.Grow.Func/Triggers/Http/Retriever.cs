using IPS.Grow.Func.Models;
using IPS.Grow.Func.Triggers.Entities;
using IPS.Grow.Shared.Configs;
using IPS.Grow.Shared.Extensions;
using IPS.Grow.Shared.Services;
using IPS.Grow.Shared.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Net.Mime;
using Cosmos = IPS.Grow.Func.Entities.Cosmos;

namespace IPS.Grow.Func.Triggers.Http
{
    internal class Retriever(ILogger<Retriever> logger, ICosmosService cosmosService, ConnectionMultiplexer redis)
    {
        private readonly Task<Container> _container = cosmosService.GetContainerAsync(nameof(ContainerNames.Product));
        private readonly Task<Container> _categoryContainer = cosmosService.GetContainerAsync(nameof(ContainerNames.ProductCategory));
        private readonly IDatabase _redis = redis.GetDatabase();
        //
        [Function(nameof(GetProductAsync))]
        [OpenApiOperation(operationId: nameof(GetProductAsync), tags: ["Product"], Description = "Get product data")]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(ProductResponse))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.NotFound)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Description = "The product Id", Required = true, Type = typeof(int))]
        public async Task<IActionResult> GetProductAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id:int}")] HttpRequest req,
        int id)
        {
            var key = new RedisKey(req.Path);

            var json = await _redis.StringGetAsync(key);
            ProductResponse? result = null;
            if (json.HasValue)
            {
                result = MessageSerializer.Deserialize<ProductResponse>(json!);
            }
            else
            {
                result = await ReadDbAsync(id);

                if (result == null)
                {
                    return new NotFoundResult();
                }

                json = MessageSerializer.Serialize(result);
                await _redis.StringSetAsync(key, json);
            }
            return new OkObjectResult(result);
        }

        private async Task<ProductResponse?> ReadDbAsync(int id)
        {
            var container = await _container;
            var product = await container.FindAsync<Cosmos.ProductEntity>(id.ToString(), ProductEntity.Pk);
            if (product == null)
            {
                return null;
            }
            var result = new ProductResponse
            {
                Id = int.TryParse(product.Id, out var productId) ? productId : 0,
                Name = product.Name,
                Price = product.Price,
                Categories = []
            };
            if (product.Categories.Length > 0)
            {

                var categoryContainer = await _categoryContainer;
                var ids = product!.Categories.Select(i => $"'{i}'");
                var queryText = $@"
                                SELECT VALUE c.name 
                                FROM c
                                WHERE c.id IN ({string.Join(",", ids)})
                            ";
                var query = new QueryDefinition(queryText);
                var categories = await categoryContainer.FetchAsync<string>(query, ProductEntity.Pk);
                result.Categories = [.. categories];
            }

            return result;
        }
    }
}