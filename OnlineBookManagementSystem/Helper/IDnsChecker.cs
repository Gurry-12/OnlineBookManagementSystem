using System.Threading.Tasks;

namespace OnlineBookManagementSystem.Helper
{
    public interface IDnsChecker
    {
        Task<bool> DomainHasMxRecordAsync(string email);
    }
}
