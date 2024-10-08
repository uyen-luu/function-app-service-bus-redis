namespace IPS.Grow.Func.Utilities;

public class CosmosProperty
{
    public const string Id = "id";
    public const string Pk = "pk";
    public const string LastModified = "lastModified";
    public const string Created = "created";
    public const string Status = "status";
}


internal class ActivityFunctions
{
    public const string RemoveCacheKey = "RemoveCacheKey";
}

internal class OrchestratorFunctions
{
    public const string ProductOrchestrator = "ProductOrchestrator";
    public const string ProductCategoryOrchestrator = "ProductCategoryOrchestrator";
}

internal class ServiceBusFunctions
{
    public const string ReceiveSingleMessage = "ReceiveSingleMessage";
    public const string ReceiveBatchMessages = "ReceiveBatchMessages";
}