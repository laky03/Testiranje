using System.Net.Mail;

namespace SplitSync.Services
{
    public interface IEmailService
    {
        Task SendEmail(string receptor, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string receptor, string subject, string body)
        {
            var email = _configuration["Smtp:Username"];
            if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out var fromAddress))
                return;

            var password = _configuration["Smtp:Password"];
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(smtpHost))
                return;

            if (!MailAddress.TryCreate(receptor, out var toAddress))
                return;

            var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new System.Net.NetworkCredential(email, password);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            await smtpClient.SendMailAsync(message);
        }
    }
}
