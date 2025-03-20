using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Hubs;
using MyGoldenFood.Models;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{

    public class TariflerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IHubContext<TariflerHub> _tariflerHubContext;
        public TariflerController(AppDbContext context, CloudinaryService cloudinaryService, IHubContext<TariflerHub> tariflerHubContext)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _tariflerHubContext = tariflerHubContext;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string selectedLanguage = "tr"; // Varsayılan dil

            // Kullanıcının seçtiği dili çerezlerden al
            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var categories = await _context.RecipeCategories
                .Include(c => c.Translations) // Çevirileri dahil et
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name; // Çeviriyi uygula
                }
            }

            return View(categories);
        }



        // Tarif Kategorileri Listeleme
        [HttpGet]
        public async Task<IActionResult> RecipeCategoryList()
        {
            var categories = await _context.RecipeCategories.ToListAsync();
            return PartialView("_RecipeListPartial", categories);
        }

        // Yeni Tarif Ekle - GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.RecipeCategories.ToList(); // Kategorileri ViewBag ile gönderiyoruz
            return PartialView("_CreateRecipePartial");
        }

        // Yeni Tarif Ekle - POST
        [HttpPost]
        public async Task<IActionResult> Create(Recipe model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                // 📌 1️⃣ Resmi Cloudinary'ye yükle
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipes");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                // 📌 2️⃣ Veritabanına tarif ekleyelim
                _context.Recipes.Add(model);
                await _context.SaveChangesAsync(); // Tarifin ID’sini almak için önceden kaydediyoruz
                Console.WriteLine($"✔ Tarif eklendi: {model.Name} - ID: {model.Id}");

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
                        var newTranslation = new RecipeTranslation
                        {
                            RecipeId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.RecipeTranslations.Add(newTranslation);
                        Console.WriteLine($"✅ Çeviri kaydedildi: {lang}");
                    }
                }

                // 📌 4️⃣ Çevirileri veritabanına kaydet
                await _context.SaveChangesAsync();
                await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
                return Json(new { success = true, message = "Tarif başarıyla eklendi ve çevrildi!" });
            }

            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_CreateRecipePartial", model);
        }



        // Tarif Detayları Listeleme
        [HttpGet]
        public async Task<IActionResult> RecipeList(int categoryId)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory) // RecipeCategory tablosunu dahil ediyoruz
                .Select(r => new
                {
                    Id = r.Id,
                    Name = r.Name,
                    Content = r.Content,
                    ImagePath = r.ImagePath,
                    RecipeCategoryId = r.RecipeCategoryId,
                    RecipeCategoryName = r.RecipeCategory != null ? r.RecipeCategory.Name : "Kategori Yok"
                })
                .ToListAsync();

            // Veriyi View'e gönderirken model dönüşümü yapıyoruz
            var recipeModels = recipes.Select(r => new Recipe
            {
                Id = r.Id,
                Name = r.Name,
                Content = r.Content,
                ImagePath = r.ImagePath,
                RecipeCategoryId = r.RecipeCategoryId,
                RecipeCategory = new RecipeCategory { Name = r.RecipeCategoryName } // Sadece Name bilgisini ekliyoruz
            }).ToList();

            return PartialView("_RecipeListPartial", recipeModels);
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id, [FromServices] IStringLocalizer<SharedResource> localizer)
        {
            string selectedLanguage = "tr"; // Varsayılan dil

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Where(r => r.RecipeCategoryId == id)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                var translation = await _context.RecipeTranslations
                    .Where(t => t.RecipeId == recipe.Id && t.LanguageCode == selectedLanguage)
                    .FirstOrDefaultAsync();

                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            var category = await _context.RecipeCategories
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                var translatedCategory = category.Translations
                    .FirstOrDefault(t => t.Language == selectedLanguage);

                ViewBag.CategoryName = translatedCategory != null ? translatedCategory.Name : category.Name;
            }
            else
            {
                ViewBag.CategoryName = localizer["Tarifler_TariflerDetails_Kategori Bulunamadı"];
            }

            if (!recipes.Any())
            {
                ViewBag.Message = localizer["Tarifler_Bu kategoriye ait tarif bulunmamaktadır."];
            }

            return View(recipes);
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

            var recipe = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Include(r => r.RecipeTranslations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // 📌 Seçili dile göre çeviri yükle
            var translation = recipe.RecipeTranslations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
            if (translation != null)
            {
                recipe.Name = translation.Name;
                recipe.Content = translation.Content;
            }

            ViewBag.Categories = await _context.RecipeCategories.ToListAsync();

            return PartialView("_EditRecipePartial", recipe);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Recipe model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.RecipeCategories.ToListAsync();
                return PartialView("_EditRecipePartial", model);
            }

            var existingRecipe = await _context.Recipes
                .Include(r => r.RecipeTranslations) // Çevirileri de alıyoruz
                .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (existingRecipe == null)
            {
                return NotFound();
            }

            // 📌 1️⃣ Tarif bilgilerini güncelle
            existingRecipe.Name = model.Name;
            existingRecipe.Content = model.Content;
            existingRecipe.RecipeCategoryId = model.RecipeCategoryId;

            // 📌 2️⃣ Resim Güncellenirse Cloudinary'de Değiştir
            if (ImageFile != null && ImageFile.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(existingRecipe.ImagePath); // Eski resmi sil
                var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipes");
                if (uploadResult != null)
                {
                    existingRecipe.ImagePath = uploadResult;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✔ Tarif güncellendi: {existingRecipe.Name} - ID: {existingRecipe.Id}");

            // 🌍 3️⃣ 7 Dilde Çeviri Yap ve Güncelle/Kaydet
            string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

            foreach (var lang in languages)
            {
                var existingTranslation = await _context.RecipeTranslations
                    .FirstOrDefaultAsync(t => t.RecipeId == model.Id && t.LanguageCode == lang);

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
                        var newTranslation = new RecipeTranslation
                        {
                            RecipeId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.RecipeTranslations.Add(newTranslation);
                        Console.WriteLine($"✅ Yeni çeviri eklendi: {lang}");
                    }
                }
            }

            // 📌 4️⃣ Çevirileri Veritabanına Kaydet
            await _context.SaveChangesAsync();
            await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
            return Json(new { success = true, message = "Tarif başarıyla güncellendi ve çeviriler güncellendi!" });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return Json(new { success = false, message = "Tarif bulunamadı!" });
            }

            if (!string.IsNullOrEmpty(recipe.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(recipe.ImagePath);
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
            return Json(new { success = true, message = "Tarif başarıyla silindi!" });
        }


    }
}
