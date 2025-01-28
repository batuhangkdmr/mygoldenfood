using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using System.Diagnostics;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
namespace MyGoldenFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        // Ürünler Sayfasý
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }
        [HttpGet]
        public IActionResult Iletisim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Iletisim(string adsoyad, string email, string konu, string mesaj)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("My Golden Food", "info@mygoldenfood.com"));
                emailMessage.To.Add(new MailboxAddress("Admin", "info@mygoldenfood.com"));
                emailMessage.Subject = konu;
                emailMessage.Body = new TextPart("html")
                {
                    Text = $"<strong>Gönderen:</strong> {adsoyad} ({email}) <br><br> <strong>Mesaj:</strong> {mesaj}"
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // (Opsiyonel) SSL kontrolünü atlamak için

                    client.Connect("mail.mygoldenfood.com", 465, SecureSocketOptions.Auto); // Sunucu adresi düzeltildi
                    client.Authenticate("info@mygoldenfood.com", "MYG1234myg");
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                ViewBag.Uyari = "Mesajýnýz baþarýyla gönderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Uyari = "Mesaj gönderilirken hata oluþtu: " + ex.Message;
            }

            return View();
        }
    }
}
