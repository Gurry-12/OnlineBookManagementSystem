using System.Threading.Tasks;

namespace OnlineBookManagementSystem.Infrastructure.Services.Helpers
{
    public interface IDnsChecker
    {
        Task<bool> DomainHasMxRecordAsync(string email);
    }
}
