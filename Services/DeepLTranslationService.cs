using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyGoldenFood.Services
{
    public class DeepLTranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://api-free.deepl.com/v2/translate";
        private readonly ILogger<DeepLTranslationService> _logger;

        public DeepLTranslationService(IConfiguration configuration, ILogger<DeepLTranslationService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["DeepL:ApiKey"];
            _logger = logger;
        }

        public async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Çeviri için boş metin gönderildi!");
                return string.Empty;
            }

            var requestBody = new
            {
                text = new[] { text },
                source_lang = "TR",  // 🔥 Türkçe olduğunu API'ye bildiriyoruz
                target_lang = targetLanguage.ToUpper()
            };


            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            request.Headers.Add("Authorization", $"DeepL-Auth-Key {_apiKey}");
            request.Content = requestContent;

            _logger.LogInformation($"DeepL API'ye istek gönderiliyor: {JsonSerializer.Serialize(requestBody)}");

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError($"DeepL API hatası: {errorMessage}");
                return string.Empty;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"DeepL API Yanıtı: {responseString}");

            // JSON dönüşümünde hata alıyor olabiliriz, bu yüzden Deserialize ayarını düzenleyelim
            var responseJson = JsonSerializer.Deserialize<DeepLResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // JSON key'lerin büyük/küçük harf duyarlılığını kaldır
            });

            return responseJson?.Translations?[0]?.Text ?? string.Empty;
        }
    }

    public class DeepLResponse
    {
        public TranslationItem[] Translations { get; set; }
    }

    public class TranslationItem
    {
        public string Text { get; set; }
    }
}
