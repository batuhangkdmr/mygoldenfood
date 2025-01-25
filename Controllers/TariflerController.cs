using Microsoft.AspNetCore.Authorization;
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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Tarif kategorilerini listeleme
            var categories = await _context.RecipeCategories.ToListAsync();
            return View(categories); // categories değişkenini view'e gönderiyoruz
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
                .Select(r => new Recipe
                {
                    Id = r.Id,
                    Name = r.Name,
                    Content = r.Content,
                    ImagePath = r.ImagePath,
                    RecipeCategoryId = r.RecipeCategoryId,
                    CategoryName = r.RecipeCategory != null ? r.RecipeCategory.Name : null // CategoryName'i dolduruyoruz
                })
                .ToListAsync();

            return PartialView("_RecipeListPartial", recipes);
        }

        // Yeni Tarif Ekle - GET
        [HttpGet]
        public IActionResult CreateRecipe()
        {
            return PartialView("_CreateRecipePartial");
        }

        // Yeni Tarif Ekle - POST
        [HttpPost]
        public async Task<IActionResult> CreateRecipe(Recipe model, IFormFile ImageFile)
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

            return PartialView("_CreateRecipePartial", model);
        }

        // Tarif Silme
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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var recipes = await _context.Recipes
                .Where(r => r.RecipeCategoryId == id) // Seçilen kategoriye ait tarifleri getir
                .ToListAsync();

            if (!recipes.Any())
            {
                return NotFound(); // Eğer tarif bulunamazsa 404 döndür
            }

            ViewBag.CategoryName = _context.RecipeCategories
                .Where(c => c.Id == id)
                .Select(c => c.Name)
                .FirstOrDefault();

            return View(recipes); // Tarifleri view'e gönder
        }

    }
}
