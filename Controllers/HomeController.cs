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
                // Türkiye saatini al
                var turkiyeSaati = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("My Golden Food", "fatihozkaya@mygoldenfood.com"));
                emailMessage.To.Add(new MailboxAddress("Admin", "fatihozkaya@mygoldenfood.com"));
                emailMessage.Subject = konu;
                emailMessage.Body = new TextPart("html")
                {
                    Text = $"<strong>Gönderen:</strong> {adsoyad} ({email}) <br><br> " +
                           $"<strong>Tarih:</strong> {turkiyeSaati:dd.MM.yyyy HH:mm:ss} <br><br> " +
                           $"<strong>Mesaj:</strong> {mesaj}"
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // SSL kontrolünü atlamak için

                    client.Connect("mail.mygoldenfood.com", 465, SecureSocketOptions.Auto);
                    client.Authenticate("fatihozkaya@mygoldenfood.com", "MYG1234MYG");
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
