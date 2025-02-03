using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Recipe Recipe { get; set; }

    public List<RecipeCategory> Categories { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Recipe = await _context.Recipes.FindAsync(id);
        if (Recipe == null)
        {
            return NotFound();
        }

        Categories = await _context.RecipeCategories.ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _context.RecipeCategories.ToListAsync();
            return Page();
        }

        _context.Attach(Recipe).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return RedirectToPage("/Tarifler/Index");
    }
}
