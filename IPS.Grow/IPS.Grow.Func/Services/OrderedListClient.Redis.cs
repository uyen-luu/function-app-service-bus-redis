using StackExchange.Redis;

namespace IPS.Grow.Func.Services;

public class RedisOrderedListClient(ConnectionMultiplexer redis) : IOrderedListClient
{
    public async Task PushData(string key, string value)
    {
        var redisDb = redis.GetDatabase();
        await redisDb.ListRightPushAsync(key, value);
    }
}