using IPS.Grow.Func.Models;
using IPS.Grow.Func.Utilities;
using StackExchange.Redis;

namespace IPS.Grow.Func.Services;

public enum RedisDbType
{
    Default,
    Category
}

public interface ICacheService
{
    Task<bool> RemoveApiCacheAsync(BusinessId bid, CancellationToken ct = default);
    Task<TResult?> TryReadApiCacheAsync<TResult>(BusinessId bid,
                                                 Func<Task<TResult?>> getValueAsync,
                                                 CancellationToken ct = default)
        where TResult : class;

    Task<TResult?> TryReadAsync<TResult>(string key,
                                         Func<Task<TResult?>> getValueAsync,
                                         RedisDbType dbType = RedisDbType.Default,
                                         CancellationToken ct = default)
        where TResult : class;
}
internal class CacheService(ConnectionMultiplexer redisServer) : ICacheService
{
    public Task<bool> RemoveApiCacheAsync(BusinessId bid, CancellationToken ct = default)
    {
        var redisDb = GetRedisDb(RedisDbType.Default);
        var key = bid.GetRedisKey();
        return redisDb.KeyDeleteAsync(key);
    }

    public Task<TResult?> TryReadApiCacheAsync<TResult>(BusinessId bid,
                                                     Func<Task<TResult?>> getValueAsync,
                                                     CancellationToken ct = default)
        where TResult : class
    {
        var key = bid.GetRedisKey();
        return TryReadAsync(key, getValueAsync, RedisDbType.Default, ct);
    }

    public Task<TResult?> TryReadAsync<TResult>(string key,
                                                Func<Task<TResult?>> getValueAsync,
                                                RedisDbType dbType = RedisDbType.Default,
                                                CancellationToken ct = default) where TResult : class
    {
        return TryReadAsync(new RedisKey(key), getValueAsync, dbType, ct);
    }

    #region Privates
    private async Task<TResult?> TryReadAsync<TResult>(RedisKey key,
                                                       Func<Task<TResult?>> getValueAsync,
                                                       RedisDbType dbType = RedisDbType.Default,
                                                       CancellationToken ct = default) where TResult : class
    {
        var redisDb = GetRedisDb(dbType);
        var json = await redisDb.StringGetAsync(key);
        TResult? result;
        if (json.HasValue)
        {
            result = MessageSerializer.Deserialize<TResult>(json!);
        }
        else
        {
            result = await getValueAsync().ConfigureAwait(false);

            if (result != null)
            {
                json = MessageSerializer.Serialize(result);
                await redisDb.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
            }
        }

        return result;
    }

    private IDatabase GetRedisDb(RedisDbType type) => redisServer.GetDatabase((int)type);
    #endregion
}
