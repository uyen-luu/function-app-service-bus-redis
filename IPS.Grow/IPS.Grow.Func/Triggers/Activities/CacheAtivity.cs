using IPS.Grow.Func.Models;
using IPS.Grow.Func.Services;
using IPS.Grow.Func.Utilities;
using Microsoft.Azure.Functions.Worker;

namespace IPS.Grow.Func.Triggers.Activities;

internal class CacheAtivity(ICacheService cacheService)
{
    [Function(ActivityFunctions.RemoveCacheKey)]
    public async Task RemoveCacheKeyAsync([ActivityTrigger] BusinessId id)
    {
        await cacheService.RemoveApiCacheAsync(id);
    }
}
