using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace MyGoldenFood.Services
{
    public class MailService
    {
        private readonly string smtpServer = "mail.mygoldenfood.com"; // SMTP Sunucusu
        private readonly int smtpPort = 465; // SMTP Port (SSL için)
        private readonly string smtpUsername = "info@mygoldenfood.com"; // SMTP Kullanıcı Adı
        private readonly string smtpPassword = "MYG1234myg"; // SMTP Şifresi

        public async Task<bool> SendEmailAsync(string senderName, string senderEmail, string messageContent)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail)); // Gönderen
                email.To.Add(new MailboxAddress("My Golden Food", smtpUsername)); // Alıcı
                email.Subject = "Yeni İletişim Formu Mesajı"; // E-posta Konusu

                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $"<p><strong>Gönderen:</strong> {senderName} ({senderEmail})</p><p>{messageContent}</p>"
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.SslOnConnect); // SSL ile bağlan
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
