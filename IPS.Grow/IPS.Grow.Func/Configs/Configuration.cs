namespace IPS.Grow.Func.Configs;

internal class RedisConfig
{
    public const string Section = "Redis";
    public required string ConnectionString { get; set; }
}

internal class ServiceBusConfig
{
    public required string ConnectionString { get; set; }
    public required ServiceBusQueueNames QueueNames { get; set; }
}

internal class ServiceBusQueueNames
{
    public required string Session { get; set; }
}


