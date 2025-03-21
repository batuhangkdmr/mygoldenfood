using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Hubs;
using MyGoldenFood.Models;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    public class FaydalariController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IHubContext<FaydalariHub> _faydalariHubContext;

        public FaydalariController(AppDbContext context, CloudinaryService cloudinaryService, IHubContext<FaydalariHub> faydalariHubContext)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _faydalariHubContext = faydalariHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string selectedLanguage = "tr"; // Varsayılan dil

            // Kullanıcının seçtiği dili al
            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            // Faydaları ve çevirilerini al
            var benefits = await _context.Benefits
                .Include(b => b.Translations)
                .ToListAsync();

            // Seçili dile göre çevirileri uygula
            foreach (var benefit in benefits)
            {
                var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            return View(benefits);
        }

        [HttpGet]
        public async Task<IActionResult> BenefitList()
        {
            var benefits = await _context.Benefits.ToListAsync();
            return PartialView("_BenefitListPartial", benefits);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateBenefitPartial");
        }

        // ✅ Yeni Fayda Ekle - POST
        [HttpPost]
        public async Task<IActionResult> Create(Benefit model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                // 📌 1️⃣ Resmi Cloudinary'ye yükle
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefits");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                // 📌 2️⃣ Veritabanına faydayı ekle
                _context.Benefits.Add(model);
                await _context.SaveChangesAsync(); // ID almak için önce kaydediyoruz
                Console.WriteLine($"✔ Fayda eklendi: {model.Name} - ID: {model.Id}");

                // 🌍 3️⃣ 7 dilde çeviri yap ve kaydet
                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                    Console.WriteLine($"🌍 Çeviri alındı (Name): {translatedName} - {lang}");
                    Console.WriteLine($"🌍 Çeviri alındı (Content): {translatedContent} - {lang}");

                    if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                    {
                        var newTranslation = new BenefitTranslation
                        {
                            BenefitId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.BenefitTranslations.Add(newTranslation);
                        Console.WriteLine($"✅ Çeviri kaydedildi: {lang}");
                    }
                }

                // 📌 4️⃣ Çevirileri veritabanına kaydet
                await _context.SaveChangesAsync();
                await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
                return Json(new { success = true, message = "Fayda başarıyla eklendi ve çevrildi!" });
            }

            return PartialView("_CreateBenefitPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            string selectedLanguage = "tr"; // Varsayılan dil

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var benefit = await _context.Benefits
                .Include(b => b.Translations)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (benefit == null)
            {
                return NotFound();
            }

            // 📌 Seçili dile göre çeviri yükle
            var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
            if (translation != null)
            {
                benefit.Name = translation.Name;
                benefit.Content = translation.Content;
            }

            return PartialView("_EditBenefitPartial", benefit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Benefit model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditBenefitPartial", model);
            }

            var existingBenefit = await _context.Benefits
                .Include(b => b.Translations)
                .FirstOrDefaultAsync(b => b.Id == model.Id);

            if (existingBenefit == null)
            {
                return NotFound();
            }

            // 📌 1️⃣ Bilgileri Güncelle
            existingBenefit.Name = model.Name;
            existingBenefit.Content = model.Content;

            // 📌 2️⃣ Resim Güncellenirse Cloudinary'de Değiştir
            if (ImageFile != null && ImageFile.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(existingBenefit.ImagePath); // Eski resmi sil
                var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefits");
                if (uploadResult != null)
                {
                    existingBenefit.ImagePath = uploadResult;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✔ Fayda güncellendi: {existingBenefit.Name} - ID: {existingBenefit.Id}");

            // 🌍 3️⃣ 7 Dilde Çeviri Yap ve Güncelle/Kaydet
            string[] languages = { "en", "de", "fr", "ru", "ja", "ko","ar" };

            foreach (var lang in languages)
            {
                var existingTranslation = await _context.BenefitTranslations
                    .FirstOrDefaultAsync(t => t.BenefitId == model.Id && t.LanguageCode == lang);

                var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                Console.WriteLine($"🌍 Çeviri alındı (Name): {translatedName} - {lang}");
                Console.WriteLine($"🌍 Çeviri alındı (Content): {translatedContent} - {lang}");

                if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                {
                    if (existingTranslation != null)
                    {
                        // 🟢 Mevcut Çeviriyi Güncelle
                        existingTranslation.Name = translatedName;
                        existingTranslation.Content = translatedContent;
                        Console.WriteLine($"✅ Çeviri güncellendi: {lang}");
                    }
                    else
                    {
                        // 🔵 Yeni Çeviri Ekle
                        var newTranslation = new BenefitTranslation
                        {
                            BenefitId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.BenefitTranslations.Add(newTranslation);
                        Console.WriteLine($"✅ Yeni çeviri eklendi: {lang}");
                    }
                }
            }

            // 📌 4️⃣ Çevirileri Veritabanına Kaydet
            await _context.SaveChangesAsync();
            await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
            return Json(new { success = true, message = "Fayda başarıyla güncellendi ve çeviriler güncellendi!" });
        }


        // ✅ Fayda Silme
        [HttpPost]
        public async Task<IActionResult> DeleteBenefit(int id)
        {
            Console.WriteLine($"🟢 Silme işlemi başladı! ID: {id}");

            // 📌 1️⃣ Faydayı bul
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null)
            {
                Console.WriteLine("❌ Silinecek fayda bulunamadı.");
                return Json(new { success = false, message = "Fayda bulunamadı!" });
            }

            // 📌 2️⃣ BenefitTranslations tablosundaki çevirileri al
            var translations = await _context.BenefitTranslations
                .Where(t => t.BenefitId == id)
                .ToListAsync();

            // 📌 3️⃣ Eğer resim varsa Cloudinary'den sil
            if (!string.IsNullOrEmpty(benefit.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(benefit.ImagePath);
            }

            // 📌 4️⃣ Önce çevirileri sil
            if (translations.Any())
            {
                _context.BenefitTranslations.RemoveRange(translations);
            }

            // 📌 5️⃣ Ana faydayı sil
            _context.Benefits.Remove(benefit);

            // 📌 6️⃣ Veritabanına kaydet
            await _context.SaveChangesAsync();
            await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
            Console.WriteLine($"✅ Silme işlemi tamamlandı! ID: {id}");
            return Json(new { success = true, message = "Fayda ve çevirileri başarıyla silindi!" });
        }

    }
}
