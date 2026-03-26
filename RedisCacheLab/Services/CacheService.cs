using Microsoft.Extensions.Caching.Distributed;
using RedisCacheLab.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisCacheLab.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly JsonSerializerOptions _jsonOpitons;

    public CacheService(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
        _jsonOpitons = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cached = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cached)) return default;

        return JsonSerializer.Deserialize<T>(cached, _jsonOpitons);
    }
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        var json = JsonSerializer.Serialize(value, _jsonOpitons);
        await _cache.SetStringAsync(key, json, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();

        if (keys.Any())
        {
            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
        }
    }
}
