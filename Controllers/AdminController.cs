using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MyGoldenFood.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private const string SECRET_KEY = "mygoldenfood22";

        public AdminController(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // Kayıt Sayfası
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kullanıcı Kayıt İşlemi (Master Key kullanarak)
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string masterKey)
        {
            if (masterKey != SECRET_KEY)
            {
                ViewBag.ErrorMessage = "Yetkiniz yok! Geçersiz kayıt anahtarı.";
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Bu kullanıcı adı zaten kullanılıyor.";
                return View();
            }

            var user = new User
            {
                Username = username,
                Password = _passwordHasher.HashPassword(null, password) // Şifreyi hashleyerek kaydetme
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Giriş Sayfası
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Giriş Doğrulama
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(null, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "AdminCookie");
                    await HttpContext.SignInAsync("AdminCookie", new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Dashboard");
                }
            }

            ViewBag.ErrorMessage = "Geçersiz kullanıcı adı veya şifre!";
            return View();
        }

        // Dashboard Sayfası
        [Authorize(AuthenticationSchemes = "AdminCookie")]
        public IActionResult Dashboard()
        {
            return View();
        }

        // Oturum Kapatma
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookie");
            return RedirectToAction("Index");
        }
    }
}
