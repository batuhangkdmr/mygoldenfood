using Microsoft.AspNetCore.Mvc;
using MyGoldenFood.ApplicationDbContext;

namespace MyGoldenFood.Controllers
{
    public class FaydalariController : Controller
    {
        private readonly AppDbContext _context;

        public FaydalariController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Benefits.ToList();
            return View(products);
        }
    }
}
