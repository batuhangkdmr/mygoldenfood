using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    
    public class FaydalariController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public FaydalariController(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Tarif kategorilerini listeleme
            var categories = await _context.Benefits.ToListAsync();
            return View(categories); // categories değişkenini view'e gönderiyoruz
        }

        // Fayda Listeleme
        [HttpGet]
        public async Task<IActionResult> BenefitList()
        {
            var benefits = await _context.Benefits.ToListAsync();
            return PartialView("_BenefitListPartial", benefits);
        }

        // Yeni Fayda Ekle - GET
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateBenefitPartial");
        }

        // Yeni Fayda Ekle - POST
        [HttpPost]
        public async Task<IActionResult> Create(Benefit model, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefits");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                _context.Benefits.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Fayda başarıyla eklendi!" });
            }

            return PartialView("_CreateBenefitPartial", model);
        }

        // Fayda Silme
        [HttpPost]
        public async Task<IActionResult> DeleteBenefit(int id)
        {
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null)
            {
                return Json(new { success = false, message = "Fayda bulunamadı!" });
            }

            if (!string.IsNullOrEmpty(benefit.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(benefit.ImagePath);
            }

            _context.Benefits.Remove(benefit);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fayda başarıyla silindi!" });
        }
    }
}
