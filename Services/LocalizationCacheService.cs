using Microsoft.Extensions.Caching.Memory;

namespace MyGoldenFood.Services
{
    public class LocalizationCacheService
    {
        private readonly IMemoryCache _cache;

        public LocalizationCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Çeviri önbelleğini temizler.
        /// </summary>
        public void ClearCache()
        {
            _cache.Remove("TranslationsCache");
        }
    }
}
