using Microsoft.Extensions.Localization;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System.Globalization;

namespace MyGoldenFood.Services
{
    public class TranslationService
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TranslationService(AppDbContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        public string GetTranslation(string fieldName, string tableName, int referenceId, string lang)
        {
            if (lang == "tr")
            {
                // Eğer dil Türkçe ise doğrudan orijinal veriyi döndür
                var originalText = _context.Products
                    .Where(p => p.Id == referenceId)
                    .Select(p => fieldName == "Name" ? p.Name : p.Description)
                    .FirstOrDefault();

                return originalText ?? "Çeviri bulunamadı";
            }

            // Veritabanından çeviriyi çek
            var translation = _context.Translations
                .Where(t => t.ReferenceId == referenceId && t.TableName == tableName && t.FieldName == fieldName && t.Language == lang)
                .Select(t => t.TranslatedValue)
                .FirstOrDefault();

            return translation ?? "Çeviri bulunamadı";
        }


    }
}
