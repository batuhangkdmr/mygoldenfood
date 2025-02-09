using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly DeepLTranslationService _translationService;

        public ProductController(AppDbContext context, CloudinaryService cloudinaryService, DeepLTranslationService translationService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _translationService = translationService;
        }

        // Ürün Listeleme
        [HttpGet]
        public async Task<IActionResult> ProductList()
        {
            // Kullanıcının seçtiği dili al
            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            string selectedLanguage = "tr"; // Varsayılan olarak Türkçe gösterelim

            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", ""); // Cookie formatını düzenle
            }

            var products = await _context.Products.ToListAsync();

            foreach (var product in products)
            {
                var translation = await _context.Translations
                    .FirstOrDefaultAsync(t => t.ReferenceId == product.Id && t.TableName == "Product" && t.Language == selectedLanguage);

                if (translation != null)
                {
                    product.Name = translation.TranslatedValue; // Çevrilen metni göster
                }
            }

            return PartialView("_ProductListPartial", products);
        }


        // Yeni Ürün Ekle - GET
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateProductPartial");
        }

        // Yeni Ürün Ekle - POST
        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                _context.Products.Add(model);
                await _context.SaveChangesAsync(); // Ürünü kaydedelim ki ID oluşsun
                Console.WriteLine($"✔ Ürün eklendi: {model.Name} - ID: {model.Id}");

                // 🌍 7 dilde çeviri yap ve kaydet
                // 7 dilde çeviri yap ve kaydet
                // Çeviri yapılacak diller (Name ve Description için ayrı tanımlandı)
                string[] languagesForName = { "en", "de", "fr", "ru", "ja", "ko" };
                string[] languagesForDescription = { "en", "de", "fr", "ru", "ja", "ko" };

                // 🟢 1️⃣ Önce Name çevirisini yapalım
                foreach (var lang in languagesForName)
                {
                    var existingNameTranslation = await _context.Translations
                        .FirstOrDefaultAsync(t => t.ReferenceId == model.Id && t.TableName == "Product" && t.FieldName == "Name" && t.Language == lang);

                    if (existingNameTranslation == null)
                    {
                        var translatedName = await translationService.TranslateText(model.Name, lang, "tr"); // Türkçeden çevireceğiz
                        Console.WriteLine($"🌍 Çeviri alındı (Name): {translatedName} - {lang}");

                        if (!string.IsNullOrEmpty(translatedName))
                        {
                            var newTranslation = new Translation
                            {
                                ReferenceId = model.Id,
                                TableName = "Product",
                                FieldName = "Name",
                                Language = lang,
                                TranslatedValue = translatedName
                            };
                            _context.Translations.Add(newTranslation);
                            Console.WriteLine($"✅ Çeviri kaydedildi (Name): {lang}");
                        }
                    }
                }

                // 🔵 2️⃣ Şimdi Description çevirisini yapalım
                foreach (var lang in languagesForDescription)
                {
                    var existingDescriptionTranslation = await _context.Translations
                        .FirstOrDefaultAsync(t => t.ReferenceId == model.Id && t.TableName == "Product" && t.FieldName == "Description" && t.Language == lang);

                    if (existingDescriptionTranslation == null)
                    {
                        var translatedDescription = await translationService.TranslateText(model.Description, lang, "tr"); // Türkçeden çevireceğiz
                        Console.WriteLine($"🌍 Çeviri alındı (Description): {translatedDescription} - {lang}");

                        if (!string.IsNullOrEmpty(translatedDescription))
                        {
                            var newTranslation = new Translation
                            {
                                ReferenceId = model.Id,
                                TableName = "Product",
                                FieldName = "Description",
                                Language = lang,
                                TranslatedValue = translatedDescription
                            };
                            _context.Translations.Add(newTranslation);
                            Console.WriteLine($"✅ Çeviri kaydedildi (Description): {lang}");
                        }
                    }
                }

                // Çevirileri kaydet
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ürün başarıyla eklendi!" });
            }

            return PartialView("_CreateProductPartial", model);
        }





        // Ürün Düzenle - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return PartialView("_EditProductPartial", product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(model.Id);
                if (existingProduct == null) return NotFound();

                existingProduct.Name = model.Name;
                existingProduct.Description = model.Description;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    await _cloudinaryService.DeleteImageAsync(existingProduct.ImagePath);
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "products");
                    if (uploadResult != null)
                    {
                        existingProduct.ImagePath = uploadResult;
                    }
                }

                else if (string.IsNullOrEmpty(existingProduct.ImagePath) && !string.IsNullOrEmpty(model.ImagePath))
                {
                    existingProduct.ImagePath = model.ImagePath;
                }

                // Eski çevirileri kaldır
                var existingTranslations = await _context.Translations
                    .Where(t => t.ReferenceId == model.Id && t.TableName == "Product")
                    .ToListAsync();
                _context.Translations.RemoveRange(existingTranslations);

                // Yeni çevirileri oluştur
                string[] languages = { "en", "de", "fr", "ru", "ja", "ko" };

                foreach (var lang in languages)
                {
                    var translatedName = await _translationService.TranslateText(model.Name, lang, "tr");
                    var translatedDescription = await _translationService.TranslateText(model.Description, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName))
                    {
                        _context.Translations.Add(new Translation
                        {
                            ReferenceId = model.Id,
                            TableName = "Product",
                            FieldName = "Name",
                            Language = lang,
                            TranslatedValue = translatedName
                        });
                    }

                    if (!string.IsNullOrEmpty(translatedDescription))
                    {
                        _context.Translations.Add(new Translation
                        {
                            ReferenceId = model.Id,
                            TableName = "Product",
                            FieldName = "Description",
                            Language = lang,
                            TranslatedValue = translatedDescription
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ürün başarıyla güncellendi!" });
            }

            return PartialView("_EditProductPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı!" });
            }

            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(product.ImagePath);
            }

            _context.Products.Remove(product);

            // Translations tablosundan ilgili çevirileri kaldır
            var translations = await _context.Translations
                .Where(t => t.ReferenceId == id && t.TableName == "Product")
                .ToListAsync();
            _context.Translations.RemoveRange(translations);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ürün ve çevirileri başarıyla silindi!" });
        }
    }
}
