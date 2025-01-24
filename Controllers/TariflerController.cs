using Microsoft.AspNetCore.Mvc;
using MyGoldenFood.ApplicationDbContext;

namespace MyGoldenFood.Controllers
{
    public class TariflerController : Controller
    {
        private readonly AppDbContext _context;

        public TariflerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.RecipeCategories.ToList();
            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var recipes = _context.Recipes.Where(r => r.RecipeCategoryId == id).ToList();
            return View(recipes);
        }
    }
}
