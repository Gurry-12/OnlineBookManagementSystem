using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<IEnumerable<Book>> GetActiveBooksAsync();
        Task<IEnumerable<Book>> GetFavoriteBooksAsync();
        Task<bool> SoftDeleteAsync(int id);

        // Changed to return raw data tuple instead of ViewModel to decouple Repo from ViewModel
        Task<(IEnumerable<Book> Books, int TotalPages, int CurrentPage)> GetPaginatedBooksDataAsync(int page, int pageSize);

        IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload();
        IEnumerable<CategoryBookCountViewModel> BooksByCategory();
        IEnumerable<AuthorBookCountViewModel> BooksByAuthor();
        FavoriteStatsViewModel FavoriteStats();
    }
}
