using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Services
{
    public interface IBookService
    {
        Task<List<Models.Book>> GetAllBooksAsync();
        Task<Models.Book?> GetBookByIdAsync(int id);
        Task<bool> AddBookAsync(Models.Book bookData);
        Task<bool> UpdateBookAsync(Models.Book bookData);
        Task<bool> SoftDeleteBookAsync(int id);
        Task<List<Models.Book>> GetFavoriteBooksAsync();
        Task<bool> ToggleFavoriteAsync(int id);
        Task<List<object>> GetAllUsersAsync();
        Task<BookFormViewModel?> GetCreateBookViewModelAsync();
        Task<BookFormViewModel?> GetEditBookViewModelAsync(int id);
        AdminViewModel GetQuickStats(int id);

        Task<BookListViewModel> GetPaginatedBooksAsync(int page, int pageSize);
        string GetTimeAgo(DateTime time);
        IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload();
        IEnumerable<CategoryBookCountViewModel> BooksByCategory();
        IEnumerable<AuthorBookCountViewModel> BooksByAuthor();
        FavoriteStatsViewModel FavoriteStats();
    }
}
