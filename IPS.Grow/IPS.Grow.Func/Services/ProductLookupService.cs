using IPS.Grow.Func.Models;
using IPS.Grow.Shared.Configs;
using IPS.Grow.Shared.Extensions;
using IPS.Grow.Shared.Services;
using Microsoft.Azure.Cosmos;
using Cosmos = IPS.Grow.Func.Entities.Cosmos;

namespace IPS.Grow.Func.Services;
public interface IProductLookupService
{
    Task<List<CategoryResponse>> ReadCategoriesAsync(CancellationToken ct = default);
    Task<ProductResponse?> ReadProductAsync(int id, CancellationToken ct = default);
    Task<ProductCategoryResponse?> ReadProductsAsync(int categoryId, CancellationToken ct = default);
}
internal class ProductLookupService(ICosmosService cosmosService, ICacheService cacheService) : IProductLookupService
{
    private readonly Task<Container> _categoryContainer = cosmosService.GetContainerAsync(nameof(ContainerNames.ProductCategory));
    private readonly Task<Container> _container = cosmosService.GetContainerAsync(nameof(ContainerNames.Product));

    public const string ProductPk = "product-pk";
    public const string CategoryPk = "category-pk";
    public const string CategoryKey = "product-categories";
    public async Task<List<CategoryResponse>> ReadCategoriesAsync(CancellationToken ct = default)
    {
        var (_, result) = await cacheService.TryReadAsync(CategoryKey, () => ReadAllCategoriesAsync(ct), RedisDbType.Category, ct);
        return result!;
    }

    public async Task<ProductResponse?> ReadProductAsync(int id, CancellationToken ct = default)
    {
        var container = await _container;
        var product = await container.FindAsync<Cosmos.ProductEntity>(id.ToString(), ProductPk, ct);
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
            var catogories = await ReadCategoriesAsync(ct);
            var categoryNames = catogories.Where(c => product.Categories.Contains(c.Id)).OrderBy(c => c.Id).Select(c => c.Name);
            result.Categories = [.. categoryNames];
        }

        return result;
    }

    public async Task<ProductCategoryResponse?> ReadProductsAsync(int categoryId, CancellationToken ct = default)
    {
        var categorycontainer = await _categoryContainer;
        var category = await categorycontainer.FindAsync<Cosmos.ProductCategoryEntity>(categoryId.ToString(), CategoryPk, ct);
        if (category == null)
        {
            return null;
        }

        var result = new ProductCategoryResponse
        {
            Name = category.Name,
        };

        var container = await _container;
        var query = @$"SELECT c.id, c.name, c.price
                    FROM c
                    JOIN ct in c.categories
                    WHERE c.pk = '{ProductPk}' AND ct IN ({categoryId}) AND c.status <> '{Cosmos.ProductStatusType.Obsoleted}'";
        var products = await container.FetchAsync<ProductItemResponse>(new QueryDefinition(query), ProductPk, ct);
        result.Items = [.. products.OrderBy(p => p.Id)];
        return result;
    }

    #region Privates

    private async Task<List<CategoryResponse>> ReadAllCategoriesAsync(CancellationToken ct = default)
    {
        var container = await _categoryContainer;
        var queryText = $@"SELECT c.id, c.name 
                        FROM c
                        WHERE c.pk = '{CategoryPk}' AND c.status <> '{Cosmos.ProductStatusType.Obsoleted}'";
        var query = new QueryDefinition(queryText);
        var result = await container.FetchAsync<CategoryResponse>(query, CategoryPk, ct);
        return [.. result.OrderBy(i => i.Id)];
    }

    #endregion
}
