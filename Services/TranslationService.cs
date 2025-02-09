using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System.Globalization;

namespace MyGoldenFood.Services
{
    public class TranslationService
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMemoryCache _cache;

        public TranslationService(AppDbContext context, IStringLocalizer<SharedResource> localizer, IMemoryCache cache)
        {
            _context = context;
            _localizer = localizer;
            _cache = cache;
        }

        public string GetTranslation(string fieldName, string tableName, int referenceId, string lang)
        {
            // 🔹 Önbellekten veri kontrol edelim (CacheKey: "Translation_Product_5_en")
            string cacheKey = $"Translation_{tableName}_{referenceId}_{fieldName}_{lang}";
            if (_cache.TryGetValue(cacheKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            if (lang == "tr")
            {
                // Eğer dil Türkçe ise doğrudan veritabanındaki orijinal veriyi döndür
                var originalText = _context.Products
                    .Where(p => p.Id == referenceId)
                    .Select(p => fieldName == "Name" ? p.Name : p.Description)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(originalText))
                    return "Çeviri bulunamadı";

                // 🔹 Önbelleğe ekleyelim (10 dakika süreyle sakla)
                _cache.Set(cacheKey, originalText, TimeSpan.FromMinutes(10));

                return originalText;
            }

            // 🔹 Veritabanından çeviriyi çekelim
            var translation = _context.Translations
                .Where(t => t.ReferenceId == referenceId && t.TableName == tableName && t.FieldName == fieldName && t.Language == lang)
                .Select(t => t.TranslatedValue)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(translation))
                return "Çeviri bulunamadı";

            // 🔹 Önbelleğe ekleyelim (10 dakika süreyle sakla)
            _cache.Set(cacheKey, translation, TimeSpan.FromMinutes(10));

            return translation;
        }
    }
}
