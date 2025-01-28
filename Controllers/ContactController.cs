using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Text;
using MimeKit;
using MailService = MyGoldenFood.Services.MailService;
using MailKit.Security;


namespace MyGoldenFood.Controllers
{

    public class ContactController : Controller
    {
        private readonly MailService _mailService;

        public ContactController(MailService mailService)
        {
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string name, string email, string message)
        {
            // Form validasyon kontrolü
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                return Json(new { success = false, message = "Lütfen tüm alanları doldurun!" });
            }

            try
            {
                // E-posta gönderme işlemi
                var isSuccess = await _mailService.SendEmailAsync(name, email, message);

                if (isSuccess)
                {
                    return Json(new { success = true, message = "Mesajınız başarıyla gönderildi!" });
                }
                else
                {
                    Console.WriteLine("E-Posta gönderme başarısız! (MailService.SendEmailAsync false döndü.)");
                    return Json(new { success = false, message = "Mesaj gönderilirken hata oluştu!" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage Hatası: {ex.Message}");
                return Json(new { success = false, message = "Sunucu hatası oluştu!" });
            }
        }

    }
}