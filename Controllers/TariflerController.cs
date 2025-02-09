using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using MyGoldenFood.Services;
using System.Globalization;

namespace MyGoldenFood.Controllers
{
    public class TariflerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly DeepLTranslationService _translationService;

        public TariflerController(AppDbContext context, CloudinaryService cloudinaryService, DeepLTranslationService translationService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _translationService = translationService;
        }

        // 📌 Tarif Kategorilerini Listele
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string selectedLanguage = GetSelectedLanguage();

            var categories = await _context.RecipeCategories
                .Include(c => c.Translations)
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations
                    .FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                }
            }

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> RecipeList()
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory) // Tarifin kategorisini de çekiyoruz
                .ToListAsync();

            return PartialView("_RecipeListPartial", recipes);
        }


        // 📌 Yeni Tarif Ekle - GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_CreateRecipePartial");
        }

        // 📌 Yeni Tarif Ekle - POST (DeepL Çeviri Dahil)
        [HttpPost]
        public async Task<IActionResult> Create(Recipe model, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipes");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                _context.Recipes.Add(model);
                await _context.SaveChangesAsync();

                // 🌍 7 dilde çeviri yap ve ekle
                await AddRecipeTranslations(model);

                return Json(new { success = true, message = "Tarif başarıyla eklendi!" });
            }

            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_CreateRecipePartial", model);
        }

        // 📌 Tarif Detayları - Çevrilmiş İçerik Dahil
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            string selectedLanguage = GetSelectedLanguage();

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

            ViewBag.CategoryName = _context.RecipeCategories
                .Where(c => c.Id == id)
                .Select(c => c.Name)
                .FirstOrDefault();

            if (!recipes.Any())
            {
                ViewBag.Message = "Bu kategoriye ait tarif bulunmamaktadır.";
            }

            return View(recipes);
        }

        // 📌 Tarif Düzenleme (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.RecipeCategories.ToListAsync();
            return PartialView("_EditRecipePartial", recipe);
        }

        // 📌 Tarif Düzenleme (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Recipe model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.RecipeCategories.ToListAsync();
                return PartialView("_EditRecipePartial", model);
            }

            var existingRecipe = await _context.Recipes.FindAsync(model.Id);
            if (existingRecipe == null)
            {
                return NotFound();
            }

            existingRecipe.Name = model.Name;
            existingRecipe.Content = model.Content;
            existingRecipe.RecipeCategoryId = model.RecipeCategoryId;

            await _context.SaveChangesAsync();

            // 🌍 Çeviri güncelle veya ekle
            await UpdateRecipeTranslations(model);

            return Json(new { success = true, message = "Tarif başarıyla güncellendi!" });
        }

        // 📌 Tarif Silme (Bağlı Çevirileri de Sil)
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

            // 🔥 Tarif çevirilerini de sil
            var translations = _context.RecipeTranslations.Where(t => t.RecipeId == id);
            _context.RecipeTranslations.RemoveRange(translations);

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Tarif başarıyla silindi!" });
        }

        // 🔄 **Helper Fonksiyonlar**
        private async Task AddRecipeTranslations(Recipe model)
        {
            string[] languages = { "en", "fr", "de", "ru", "ja", "ko", "es" };

            foreach (var lang in languages)
            {
                var translatedName = await _translationService.TranslateText(model.Name, lang);
                var translatedContent = await _translationService.TranslateText(model.Content, lang);

                var translation = new RecipeTranslation
                {
                    RecipeId = model.Id,
                    LanguageCode = lang,
                    Name = translatedName,
                    Content = translatedContent
                };

                _context.RecipeTranslations.Add(translation);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateRecipeTranslations(Recipe model)
        {
            string[] languages = { "en", "fr", "de", "ru", "ja", "ko", "es" };

            foreach (var lang in languages)
            {
                var translation = await _context.RecipeTranslations
                    .FirstOrDefaultAsync(t => t.RecipeId == model.Id && t.LanguageCode == lang);

                if (translation != null)
                {
                    translation.Name = await _translationService.TranslateText(model.Name, lang);
                    translation.Content = await _translationService.TranslateText(model.Content, lang);
                }
                else
                {
                    var newTranslation = new RecipeTranslation
                    {
                        RecipeId = model.Id,
                        LanguageCode = lang,
                        Name = await _translationService.TranslateText(model.Name, lang),
                        Content = await _translationService.TranslateText(model.Content, lang)
                    };

                    _context.RecipeTranslations.Add(newTranslation);
                }
            }

            await _context.SaveChangesAsync();
        }

        private string GetSelectedLanguage()
        {
            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            return string.IsNullOrEmpty(userCulture) ? "tr" : userCulture.Split('|')[0].Replace("c=", "");
        }
    }
}
