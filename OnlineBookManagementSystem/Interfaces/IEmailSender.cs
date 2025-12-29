using System.Threading.Tasks;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? plainTextMessage = null);
    }
}
