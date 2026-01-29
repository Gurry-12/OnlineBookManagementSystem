using System.Threading.Tasks;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? plainTextMessage = null);
    }
}
