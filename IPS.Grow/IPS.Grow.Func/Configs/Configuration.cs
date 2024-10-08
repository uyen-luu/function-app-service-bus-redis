namespace IPS.Grow.Func.Configs;

public class RedisConfig
{
    public const string Section = "Redis";
    public required string ConnectionString { get; set; }
}

public class ServiceBusConfig
{
    public const string Section = "ServiceBus";
    public required string ConnectionString { get; set; }
    public required ServiceBusQueueNames QueueNames { get; set; }
}

public class ServiceBusQueueNames
{
    public required string Session { get; set; }
    public required string NoSession { get; set; }
    public required string Output { get; set; }
    public required string Single { get; set; }
    public required string Batch { get; set; }
}

