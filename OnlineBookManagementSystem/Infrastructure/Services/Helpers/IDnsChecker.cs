namespace OnlineBookManagementSystem.Infrastructure.Services.Helpers
{
    public interface IDnsChecker
    {
        Task<bool> DomainHasMxRecordAsync(string email);
    }
}
