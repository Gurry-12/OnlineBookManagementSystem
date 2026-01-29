using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Analytics;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Analytics
{
    /// <summary>
    /// Refactored Analytics Service following SRP.
    /// Only handles analytics business logic, delegates data access to repository.
    /// </summary>
    public class RefactoredAnalyticsService : IBookAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly ILogger<RefactoredAnalyticsService> _logger;

        public RefactoredAnalyticsService(
            IAnalyticsRepository analyticsRepository,
            ILogger<RefactoredAnalyticsService> logger)
        {
            _analyticsRepository = analyticsRepository;
            _logger = logger;
        }

        public async Task<List<MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var data = await _analyticsRepository.GetMonthlyBookUploadsAsync(currentYear);
                
                return data.Select(kvp => new MonthlyBookUploadViewModel
                {
                    Month = kvp.Key,
                    Count = kvp.Value,
                    Year = currentYear
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly book uploads");
                throw;
            }
        }

        public async Task<List<CategoryBookCountViewModel>> GetBooksByCategoryAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetCategoryDistributionAsync();
                
                return data.Select(kvp => new CategoryBookCountViewModel
                {
                    CategoryName = kvp.Key,
                    BookCount = kvp.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by category");
                throw;
            }
        }

        public async Task<List<AuthorBookCountViewModel>> GetBooksByAuthorAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetAuthorBookCountAsync();
                
                return data.Select(kvp => new AuthorBookCountViewModel
                {
                    AuthorName = kvp.Key,
                    BookCount = kvp.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by author");
                throw;
            }
        }

        public async Task<FavoriteStatsViewModel> GetFavoriteStatsAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetFavoriteStatsAsync();
                
                return new FavoriteStatsViewModel
                {
                    FavoriteData = data,
                    TotalFavorites = data.Values.Sum(),
                    MostFavoritedBook = data.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite stats");
                throw;
            }
        }

        public async Task<AdminMonthlyStatsViewModel> GetMonthlyStatsAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var monthlyRevenue = await _analyticsRepository.GetMonthlyRevenueAsync(currentYear);
                var monthlyBooks = await _analyticsRepository.GetMonthlyBookUploadsAsync(currentYear);
                
                return new AdminMonthlyStatsViewModel
                {
                    Year = currentYear,
                    MonthlyRevenue = monthlyRevenue,
                    MonthlyBookUploads = monthlyBooks,
                    TotalRevenue = monthlyRevenue.Values.Sum(),
                    TotalBooks = monthlyBooks.Values.Sum()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly stats");
                throw;
            }
        }

        public async Task<List<UserGrowthDataViewModel>> GetUserGrowthDataAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetUserGrowthDataAsync();
                return data.Select(kvp => new UserGrowthDataViewModel
                {
                    Date = kvp.Key,
                    NewUsers = kvp.Value.NewUsers,
                    TotalUsers = kvp.Value.TotalUsers,
                    ActiveUsers = kvp.Value.ActiveUsers
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user growth data");
                throw;
            }
        }

        public async Task<List<RevenueGrowthDataViewModel>> GetRevenueGrowthDataAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetRevenueGrowthDataAsync();
                return data.Select(kvp => new RevenueGrowthDataViewModel
                {
                    Date = kvp.Key,
                    Revenue = kvp.Value.Revenue,
                    OrderCount = kvp.Value.OrderCount,
                    AverageOrderValue = kvp.Value.AverageOrderValue
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue growth data");
                throw;
            }
        }

        public async Task<List<BookPopularityDataViewModel>> GetBookPopularityDataAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetBookPopularityDataAsync();
                return data.Select(book => new BookPopularityDataViewModel
                {
                    BookId = book.BookId,
                    BookTitle = book.BookTitle,
                    Author = book.Author,
                    ViewCount = book.ViewCount,
                    OrderCount = book.OrderCount,
                    FavoriteCount = book.FavoriteCount,
                    AverageRating = book.AverageRating,
                    ReviewCount = book.ReviewCount
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book popularity data");
                throw;
            }
        }

        public async Task<List<CategoryDistributionViewModel>> GetCategoryDistributionAsync()
        {
            try
            {
                var data = await _analyticsRepository.GetCategoryDistributionDataAsync();
                return data.Select(category => new CategoryDistributionViewModel
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    BookCount = category.BookCount,
                    OrderCount = category.OrderCount,
                    Revenue = category.Revenue,
                    Percentage = category.Percentage
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category distribution data");
                throw;
            }
        }
    }
}