using StackExchange.Redis;
using System.Text.Json;

namespace LibraryAPI.Services.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer redis;
        private readonly ILogger<RedisCacheService> logger;
        private readonly JsonSerializerOptions jsonOptions = new(){
            PropertyNameCaseInsensitive = true
        };
        private DateTime cacheDisabledUntil = DateTime.MinValue;
        private readonly TimeSpan DisablePeriod = TimeSpan.FromSeconds(30);

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> Logger)
        {
            redis = connectionMultiplexer;
            logger = Logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (DateTime.UtcNow < cacheDisabledUntil) { logger.LogInformation("Cache skipped (disabled until {Until:HH:mm:ss})", cacheDisabledUntil); return default; }

                var db = redis.GetDatabase();
                var value = await db.StringGetAsync(key);
                if (value.IsNullOrEmpty) return default;
                return JsonSerializer.Deserialize<T>(value.ToString(), jsonOptions);
            }
            catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
            {
                cacheDisabledUntil = DateTime.UtcNow + DisablePeriod;
                logger.LogWarning(ex, "Redis unavailable, cache disabled for {Seconds}s", DisablePeriod.TotalSeconds);
                return default;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache error for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            try
            {
                if (DateTime.UtcNow < cacheDisabledUntil) { logger.LogInformation("Cache skipped (disabled until {Until:HH:mm:ss})", cacheDisabledUntil); return; }

                var db = redis.GetDatabase();
                var json = JsonSerializer.Serialize(value, jsonOptions);
                await db.StringSetAsync(key, json, ttl ?? TimeSpan.FromMinutes(5));
            }
            catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
            {
                cacheDisabledUntil = DateTime.UtcNow + DisablePeriod;
                logger.LogWarning(ex, "Redis unavailable, cache disabled for {Seconds}s", DisablePeriod.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache error for key {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                if (DateTime.UtcNow < cacheDisabledUntil) { logger.LogInformation("Cache skipped (disabled until {Until:HH:mm:ss})", cacheDisabledUntil); return; }

                var endpoints = redis.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    var server = redis.GetServer(endpoint);
                    // SCAN по префиксу — не блокирует Redis (в отличие от KEYS)
                    var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                    if (keys.Length > 0)
                    {
                        var db = redis.GetDatabase();
                        var deletedKeysValue = await db.KeyDeleteAsync(keys);
                        logger.LogInformation("Cache: removed {Count} keys with prefix '{Prefix}'", deletedKeysValue, prefix);
                    }
                }
            }
            catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
            {
                cacheDisabledUntil = DateTime.UtcNow + DisablePeriod;
                logger.LogWarning(ex, "Redis unavailable, cache disabled for {Seconds}s", DisablePeriod.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cache error for key prefix {Prefix}", prefix);
            }
        }
    }
}
