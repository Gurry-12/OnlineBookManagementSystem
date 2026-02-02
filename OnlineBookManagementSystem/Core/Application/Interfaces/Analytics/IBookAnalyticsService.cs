using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;

public interface IBookAnalyticsService
{
    Task<List<MonthlyBookUploadViewModel>> GetMonthlyBookUploadsAsync();
    Task<List<CategoryBookCountViewModel>> GetBooksByCategoryAsync();
    Task<List<AuthorBookCountViewModel>> GetBooksByAuthorAsync();
    Task<FavoriteStatsViewModel> GetFavoriteStatsAsync();
    Task<AdminMonthlyStatsViewModel> GetMonthlyStatsAsync();

    // Additional methods for SuperAdmin analytics
    Task<List<UserGrowthDataViewModel>> GetUserGrowthDataAsync();
    Task<List<RevenueGrowthDataViewModel>> GetRevenueGrowthDataAsync();
    Task<List<BookPopularityDataViewModel>> GetBookPopularityDataAsync();
    Task<List<CategoryDistributionViewModel>> GetCategoryDistributionAsync();
}
