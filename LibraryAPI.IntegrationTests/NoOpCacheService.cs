using LibraryAPI.Services.Redis;

namespace LibraryAPI.IntegrationTests;

/// <summary>
/// Кэш отключён в интеграционных тестах сознательно: Redis — внешняя
/// инфраструктура, а тесты здесь проверяют HTTP-конвейер и SQL Server.
/// Без кэша каждый запрос идёт в базу, и тесты не зависят от порядка.
/// </summary>
public sealed class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix) => Task.CompletedTask;
}
