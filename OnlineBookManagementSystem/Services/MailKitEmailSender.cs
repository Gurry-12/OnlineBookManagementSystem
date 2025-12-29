using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MimeKit;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models.Configuration;

namespace OnlineBookManagementSystem.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly ISystemSettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly EmailSettings _fallbackSettings;

        public MailKitEmailSender(
            ISystemSettingsService settingsService,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<MailKitEmailSender> logger,
            IOptions<EmailSettings> fallbackSettings)
        {
            _settingsService = settingsService;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _fallbackSettings = fallbackSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? plainTextMessage = null)
        {
            var message = new MimeMessage();

            // 1. Get Settings (DB > Cache > AppSettings)
            // We use SystemSettingsService for dynamic settings, but fallback to appsettings/env vars for critical creds if missing
            var settings = await _settingsService.GetSystemSettingsAsync();
            var password = _cache.Get<string>("Email:SmtpPassword") ?? _configuration["Email:SmtpPassword"] ?? _fallbackSettings.SmtpPassword;

            // Normalize inputs
            var host = !string.IsNullOrEmpty(settings.SmtpHost) ? settings.SmtpHost : _fallbackSettings.SmtpHost;
            var port = settings.SmtpPort > 0 ? settings.SmtpPort : _fallbackSettings.SmtpPort;
            var username = !string.IsNullOrEmpty(settings.SmtpUsername) ? settings.SmtpUsername : _fallbackSettings.SmtpUsername;
            var senderName = !string.IsNullOrEmpty(settings.SiteName) ? settings.SiteName : _fallbackSettings.SenderName;
            var senderEmail = !string.IsNullOrEmpty(settings.ContactEmail) ? settings.ContactEmail : _fallbackSettings.SenderEmail;

            // 2. Validate Configuration
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Email sending failed: Missing SMTP configuration. Host: {Host}, User: {User}", host, username);
                // We choose NOT to throw here to prevent crashing the user flow, but strictly logging it.
                // However, in development, it might be better to throw.
                // Given "Proper exception handling (don't swallow exceptions)", we should probably throw or ensure the caller handles false.
                // But the interface is void/Task. We will throw so the controller can handle it (e.g. show "System Error").
                throw new InvalidOperationException("SMTP Configuration is missing.");
            }

            // 3. Build Message
            message.From.Add(new MailboxAddress(senderName, senderEmail ?? username)); // Fallback to username if sender email not set
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = plainTextMessage ?? "Please view this email in an HTML-compatible client."
            };

            message.Body = bodyBuilder.ToMessageBody();

            // 4. Send via MailKit
            using var client = new SmtpClient();
            try
            {
                // Connect
                // SecureSocketOptions.StartTls is best for port 587. Auto is generally safe.
                await client.ConnectAsync(host, port, settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate
                await client.AuthenticateAsync(username, password);

                // Send
                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully to {ToEmail} via {Host}", toEmail, host);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Host: {Host}, Port: {Port}, SSL: {Ssl}", toEmail, host, port, settings.EnableSsl);
                throw; // Rethrow to let caller know
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
