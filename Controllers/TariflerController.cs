using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    
    public class TariflerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public TariflerController(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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
                return Json(new { success = true, message = "Tarif başarıyla eklendi!" });
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
        public async Task<IActionResult> Details(int id)
        {
            string selectedLanguage = "tr"; // Varsayılan dil

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory) // Kategori bilgilerini de çekiyoruz
                .ToListAsync();

            ViewBag.CategoryName = _context.RecipeCategories
                .Where(c => c.Id == id)
                .Select(c => c.Name)
                .FirstOrDefault();

            // Çeviriyi uygula
            foreach (var recipe in recipes)
            {
                var translation = _context.Translations
                    .Where(t => t.ReferenceId == recipe.Id && t.TableName == "Recipes" && t.Language == selectedLanguage)
                    .FirstOrDefault();

                if (translation != null)
                {
                    recipe.Name = translation.FieldName == "Name" ? translation.TranslatedValue : recipe.Name;
                    recipe.Content = translation.FieldName == "Content" ? translation.TranslatedValue : recipe.Content;
                }
            }

            if (!recipes.Any())
            {
                ViewBag.Message = "Bu kategoriye ait tarif bulunmamaktadır.";
            }

            return View(recipes);
        }



        // Tarif Düzenleme (GET
        [Route("Tarifler/Edit")]
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

        // Tarif Düzenleme (POST)
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

            return Json(new { success = true, message = "Tarif başarıyla güncellendi!" });
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

            return Json(new { success = true, message = "Tarif başarıyla silindi!" });
        }


    }
}
