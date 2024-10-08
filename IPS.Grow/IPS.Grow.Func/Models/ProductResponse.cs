namespace IPS.Grow.Func.Models
{
    public class ProductResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public string[] Categories { get; set; } = [];
    }

    public class CategoryResponse : ProductCategoryMessage
    {
        public required int Id { get; set; }
    }

    public class ProductCategoryResponse
    {
        public required string Name { get; set; }
        public ProductItemResponse[] Items { get; set; } = [];
    }

    public class ProductItemResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
    }
}
