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
        private readonly IConfiguration _configuration; // ? appsettings.json'u okumak i�in
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

            // ? Email ayarlar�n� appsettings.json'dan al
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

        // �r�nler Sayfas�
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
                // T�rkiye saatini al
                var turkiyeSaati = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("My Golden Food", _smtpUsername));

                // ? Mailin gidece�i adresleri ekledik
                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail1));
                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail2));

                emailMessage.Subject = konu;

                // E-posta ba�l���ndaki Date alan�n� manuel olarak ayarla
                emailMessage.Date = turkiyeSaati;

                emailMessage.Body = new TextPart("html")
                {
                    Text = $"<strong>G�nderen:</strong> {adsoyad} ({email}) <br><br> " +
                           $"<strong>Mesaj:</strong> {mesaj}"
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // SSL kontrol�n� atlamak i�in

                    client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.Auto);
                    client.Authenticate(_smtpUsername, _smtpPassword);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                ViewBag.Uyari = "Mesaj�n�z ba�ar�yla g�nderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Uyari = "Mesaj g�nderilirken hata olu�tu: " + ex.Message;
            }

            return View();
        }



    }
}
