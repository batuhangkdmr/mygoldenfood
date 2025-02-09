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
            if (string.IsNullOrWhiteSpace(text)) return text; // Boş metinleri çevirmiyoruz

            try
            {
                using (var httpClient = new HttpClient()) // Her istekte yeni HttpClient açalım
                {
                    var requestContent = new Dictionary<string, string>
            {
                { "text", text },
                { "target_lang", targetLanguage.ToUpper() },
                { "source_lang", sourceLanguage.ToUpper() } // Kaynak dil Türkçe
            };

                    var content = new FormUrlEncodedContent(requestContent);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {_apiKey}");

                    var response = await httpClient.PostAsync("https://api-free.deepl.com/v2/translate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(jsonResponse);
                        return jsonDoc.RootElement.GetProperty("translations")[0].GetProperty("text").GetString();
                    }
                    else
                    {
                        Console.WriteLine($"❌ DeepL API hatası: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 DeepL API çağrısında hata: {ex.Message}");
                return string.Empty;
            }
        }


    }
}
