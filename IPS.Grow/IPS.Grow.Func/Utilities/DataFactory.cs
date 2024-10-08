using IPS.Grow.Func.Models;

namespace IPS.Grow.Func.Utilities;

internal class DataFactory
{
    private const int MaxCategoryNumber = 10;
    private const int MaxProductNumber = 100;
    private static readonly Random _random = new();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    public static (BrokerMessage<ProductMessage>[] Products, BrokerMessage<ProductCategoryMessage>[] Categories) GenerateBrokerMessages()
    {
        var categories = GenerateProductCategoryUpsertMessages();
        var categoryNos = categories
            .Select(m => int.TryParse(m.Bid.Idetifier, out var id) ? id : (int?)null)
            .Where(id => id.HasValue).Select(id => id!.Value).ToArray();
        var products = GenerateProductUpsertMessages(categoryNos);
        return (products, categories);
    }

    private static BrokerMessage<ProductMessage>[] GenerateProductUpsertMessages(int[] categoryIds)
    {
        return Enumerable.Range(1, MaxProductNumber)
            .Select(i =>
            {
                var product = new ProductMessage
                {
                    Name = $"Product {i} - {RandomString(10)}",
                    Price = (decimal)_random.NextDouble() * 100,
                    Categories = categoryIds.OrderBy(x => _random.Next()).Take(_random.Next(0, 4)).ToArray()
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

    private static BrokerMessage<ProductCategoryMessage>[] GenerateProductCategoryUpsertMessages()
    {
        return Enumerable.Range(1, MaxCategoryNumber)
            .Select(i =>
            {
                var product = new ProductCategoryMessage
                {
                    Name = $"Product Category {i} - {RandomString(10)}",
                };
                return new BrokerMessage<ProductCategoryMessage>
                {
                    MessageId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Bid = new BusinessId(i.ToString(), BusinessObjectType.ProductCategories),
                    Data = product,
                    Operation = BrokerOperation.Upsert
                };
            }).ToArray();
    }
}
