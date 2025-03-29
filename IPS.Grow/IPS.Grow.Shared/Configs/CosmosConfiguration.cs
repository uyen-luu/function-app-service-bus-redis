namespace IPS.Grow.Shared.Configs;

public class CosmosConfiguration
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; } = "IpsGrow";
    public required ContainerNames ContainerNames { get; set; } = new();
}

public class ContainerNames
{
    public string Product { get; set; } = "Product";
    public string ProductCategory { get; set; } = "ProductCategory";
    public string User { get; set; } = "User";
    public string RefreshToken { get; set; } = "RefreshToken";
};