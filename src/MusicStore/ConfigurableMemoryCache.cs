using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MusicStore
{
    public class ConfigurableMemoryCache
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _memoryCache;

        public ConfigurableMemoryCache(IMemoryCache memoryCache, IOptions<AppSettings> appSettings)
        {
            _memoryCache = memoryCache;
            _appSettings = appSettings.Value;
        }

        public TItem Get<TItem>(object key)
        {
            if (_appSettings.StoreInCache)
            {
                return _memoryCache.Get<TItem>(key);
            }

            return default(TItem);
        }

        public object Set<T>(object key, T value, MemoryCacheEntryOptions options)
        {
            if (_appSettings.StoreInCache)
            {
                return _memoryCache.Set(key, value, options);
            }

            return value;
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            if (_appSettings.StoreInCache)
            {
                return _memoryCache.TryGetValue(key, out value);
            }

            value = default(T);
            return false;
        }

        public void Remove(object key)
        {
            if (_appSettings.StoreInCache)
            {
                _memoryCache.Remove(key);
            }
        }
    }
}
