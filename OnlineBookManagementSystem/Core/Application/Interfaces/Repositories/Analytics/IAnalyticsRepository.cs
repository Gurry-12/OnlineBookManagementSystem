using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Analytics
{
    public interface IAnalyticsRepository
    {
        Task<Dictionary<string, int>> GetMonthlyBookUploadsAsync(int year);
        Task<Dictionary<string, int>> GetCategoryDistributionAsync();
        Task<Dictionary<string, int>> GetAuthorBookCountAsync();
        Task<Dictionary<string, int>> GetFavoriteStatsAsync();
        Task<Dictionary<string, decimal>> GetMonthlyRevenueAsync(int year);
        Task<Dictionary<string, int>> GetOrderStatusDistributionAsync();
        Task<int> GetTotalBooksCountAsync();
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<List<Book>> GetTopRatedBooksAsync(int count);
        Task<List<Book>> GetMostFavoritedBooksAsync(int count);
        Task<List<User>> GetMostActiveUsersAsync(int count);
        
        // Additional methods for SuperAdmin analytics
        Task<Dictionary<DateTime, (int NewUsers, int TotalUsers, int ActiveUsers)>> GetUserGrowthDataAsync();
        Task<Dictionary<DateTime, (decimal Revenue, int OrderCount, decimal AverageOrderValue)>> GetRevenueGrowthDataAsync();
        Task<List<(int BookId, string BookTitle, string Author, int ViewCount, int OrderCount, int FavoriteCount, double AverageRating, int ReviewCount)>> GetBookPopularityDataAsync();
        Task<List<(int CategoryId, string CategoryName, int BookCount, int OrderCount, decimal Revenue, double Percentage)>> GetCategoryDistributionDataAsync();
    }
}