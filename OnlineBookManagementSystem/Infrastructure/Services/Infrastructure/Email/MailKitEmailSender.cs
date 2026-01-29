using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.RegularExpressions;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email;

namespace OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Email
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly ISystemSettingsService _settingsService;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(
            ISystemSettingsService settingsService,
            ILogger<MailKitEmailSender> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? plainTextMessage = null)
        {
            // 1. Get Settings (From Centralized Service)
            var settings = await _settingsService.GetEmailSettingsAsync();

            // 2. Validate Configuration
            if (string.IsNullOrEmpty(settings.SmtpHost) || string.IsNullOrEmpty(settings.SmtpUsername) || string.IsNullOrEmpty(settings.SmtpPassword))
            {
                _logger.LogError("Email sending failed: Missing SMTP configuration. Host: {Host}, User: {User}", settings.SmtpHost, settings.SmtpUsername);
                throw new InvalidOperationException("SMTP Configuration is missing. Please check System Settings.");
            }

            // 3. Build Message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = plainTextMessage ?? Regex.Replace(htmlMessage, "<.*?>", String.Empty)
            };

            message.Body = bodyBuilder.ToMessageBody();

            // 4. Send via MailKit
            using var client = new SmtpClient();
            try
            {
                SecureSocketOptions socketOptions = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                if (settings.SmtpPort == 465) socketOptions = SecureSocketOptions.SslOnConnect;

                await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, socketOptions);
                await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword);
                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully to {ToEmail} via {Host}", toEmail, settings.SmtpHost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Host: {Host}, Port: {Port}, SSL: {Ssl}", toEmail, settings.SmtpHost, settings.SmtpPort, settings.EnableSsl);
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
