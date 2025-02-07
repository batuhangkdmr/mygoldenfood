using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyGoldenFood.Services
{
    public class TranslationService
    {
        private readonly AppDbContext _dbContext;
        private readonly DeepLTranslationService _deepLTranslationService;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(AppDbContext dbContext, DeepLTranslationService deepLTranslationService, ILogger<TranslationService> logger)
        {
            _dbContext = dbContext;
            _deepLTranslationService = deepLTranslationService;
            _logger = logger;
        }

        public async Task<string> TranslateAndSaveAsync(string entityType, int entityId, string fieldName, string text, string targetLanguage)
        {
            _logger.LogInformation($"Çeviri işlemi başladı: {text} -> {targetLanguage}");

            // 1️⃣ Veritabanında çeviri kontrolü
            var existingTranslation = await _dbContext.ProductTranslations
                .FirstOrDefaultAsync(t => t.ProductId == entityId && t.LanguageCode == targetLanguage);

            if (existingTranslation != null)
            {
                _logger.LogInformation($"Mevcut çeviri bulundu: {existingTranslation.Name}");
                return existingTranslation.Name;
            }

            // 2️⃣ DeepL API’den çeviri al
            _logger.LogInformation($"DeepL API'ye istek gönderiliyor: {text}");
            var translatedText = await _deepLTranslationService.TranslateTextAsync(text, targetLanguage);

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                _logger.LogWarning("DeepL API'den gelen çeviri boş!");
                return text; // Eğer çeviri başarısız olursa orijinal metni döndür
            }

            // 3️⃣ Yeni çeviriyi veritabanına ekle
            var newTranslation = new ProductTranslation
            {
                ProductId = entityId,
                LanguageCode = targetLanguage,
                Name = translatedText,
                Description = null // Eğer açıklama da çevrilecekse buraya ekle
            };

            _logger.LogInformation($"Veritabanına ekleniyor: ProductId={entityId}, Language={targetLanguage}, Name={translatedText}");

            _dbContext.ProductTranslations.Add(newTranslation);
            await _dbContext.SaveChangesAsync();

            // 4️⃣ ChangeTracker doğrulaması
            var entries = _dbContext.ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                _logger.LogInformation($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
            }

            _logger.LogInformation($"Çeviri kaydedildi: {translatedText}");
            return translatedText;
        }

    }
}
