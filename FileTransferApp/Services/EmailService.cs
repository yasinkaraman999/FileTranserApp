using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FileTransferApp.Services
{
    public interface IEmailService
    {
        Task SendFileDownloadEmailAsync(string recipientEmail, string senderEmail, string message, string downloadLink);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["Email:SmtpServer"];
            _port = int.Parse(configuration["Email:Port"]);
            _username = configuration["Email:Username"];
            _password = configuration["Email:Password"];
            _enableSsl = bool.Parse(configuration["Email:EnableSsl"]);
        }

        public async Task SendFileDownloadEmailAsync(string recipientEmail, string senderEmail, string message, string downloadLink)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_username, "File Transfer App"),
                Subject = "Sizinle bir dosya paylaşıldı",
                IsBodyHtml = true,
                Body = GetEmailTemplate(senderEmail, message, downloadLink)
            };

            mailMessage.To.Add(recipientEmail);

            using var smtpClient = new SmtpClient(_smtpServer, _port)
            {
                EnableSsl = _enableSsl,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_username, _password)
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string GetEmailTemplate(string senderEmail, string message, string downloadLink)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1'>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .container {{ background-color: #f9f9f9; border-radius: 8px; padding: 20px; }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .header h2 {{ color: #0d6efd; }}
                    .content {{ background-color: white; padding: 20px; border-radius: 8px; }}
                    .button {{ display: inline-block; padding: 12px 24px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 30px; font-size: 0.9em; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Dosya Paylaşımı</h2>
                    </div>
                    <div class='content'>
                        <h3>Merhaba,</h3>
                        <p><strong>{senderEmail}</strong> sizinle bir dosya paylaştı.</p>
                        {(!string.IsNullOrEmpty(message) ? $@"<p><strong>Mesaj:</strong> {message}</p>" : "")}
                        <p>Dosyayı indirmek için aşağıdaki butona tıklayın:</p>
                        <p style='text-align: center;'>
                            <a href='{downloadLink}' class='button'>Dosyayı İndir</a>
                        </p>
                        <p><strong>Not:</strong> Bu bağlantı sınırlı bir süre için geçerlidir.</p>
                    </div>
                    <div class='footer'>
                        <p>Bu e-posta File Transfer App tarafından gönderilmiştir.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
} 