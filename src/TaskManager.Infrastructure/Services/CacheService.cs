using Microsoft.Extensions.Caching.Memory;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Servicio de caché en memoria.
    /// </summary>
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
        {
            return _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration;
                return factory();
            })!;
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cacheEntry = _cache.CreateEntry(key);
            cacheEntry.AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration;

            var result = factory();
            cacheEntry.Value = result;

            return result;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
            // Implementación simple - en producción usar MemoryCache con ICollection
            // Por ahora, esta es una operación de limpieza limitada
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            return _cache.TryGetValue(key, out value) && value != null;
        }
    }
}
