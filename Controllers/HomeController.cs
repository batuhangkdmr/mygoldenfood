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
        private readonly IConfiguration _configuration; // ? appsettings.json'u okumak için
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _recipientEmail1;
        private readonly string _recipientEmail2;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;

            // ? Email ayarlarýný appsettings.json'dan al
            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpServer = emailSettings["SmtpServer"];
            _smtpPort = int.Parse(emailSettings["Port"]);
            _smtpUsername = emailSettings["Username"];
            _smtpPassword = emailSettings["Password"];
            _recipientEmail1 = emailSettings["RecipientEmail1"];
            _recipientEmail2 = emailSettings["RecipientEmail2"];
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
                emailMessage.From.Add(new MailboxAddress("My Golden Food", _smtpUsername));

                // ? Mailin gideceði adresleri ekledik
                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail1));
                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail2));

                emailMessage.Subject = konu;

                // E-posta baþlýðýndaki Date alanýný manuel olarak ayarla
                emailMessage.Date = turkiyeSaati;

                emailMessage.Body = new TextPart("html")
                {
                    Text = $"<strong>Gönderen:</strong> {adsoyad} ({email}) <br><br> " +
                           $"<strong>Mesaj:</strong> {mesaj}"
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // SSL kontrolünü atlamak için

                    client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.Auto);
                    client.Authenticate(_smtpUsername, _smtpPassword);
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
