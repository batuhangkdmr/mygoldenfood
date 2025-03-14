using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace MyGoldenFood.Services
{
    public class DeepLTranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DeepLTranslationService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["DeepL:ApiKey"]; // appsettings.json’dan API key al
        }

        public async Task<string> TranslateText(string text, string targetLanguage, string sourceLanguage = "tr")
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            try
            {
                const int maxChunkSize = 4500; // DeepL 5000 sınırı var, biraz boşluk bırakıyoruz
                var translatedChunks = new List<string>();
                var chunks = SplitTextIntoChunks(text, maxChunkSize);

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {_apiKey}");

                foreach (var chunk in chunks)
                {
                    var requestData = new
                    {
                        text = new string[] { chunk }, // DeepL diziler ile çalışıyor
                        target_lang = targetLanguage.ToUpper(),
                        source_lang = sourceLanguage.ToUpper()
                    };

                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("https://api-free.deepl.com/v2/translate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(jsonResponse);
                        translatedChunks.Add(jsonDoc.RootElement.GetProperty("translations")[0].GetProperty("text").GetString());
                    }
                    else
                    {
                        Console.WriteLine($"❌ DeepL API hatası: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        return string.Empty;
                    }
                }

                return string.Join(" ", translatedChunks); // Parçaları birleştir
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 DeepL API çağrısında hata: {ex.Message}");
                return string.Empty;
            }
        }

        // Metni belirli uzunluklarda bölme fonksiyonu
        private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += maxChunkSize)
            {
                chunks.Add(text.Substring(i, Math.Min(maxChunkSize, text.Length - i)));
            }
            return chunks;
        }


    }
}
