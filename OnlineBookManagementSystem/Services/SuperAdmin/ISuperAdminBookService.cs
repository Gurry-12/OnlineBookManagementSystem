using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Models.DTOs;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Services.SuperAdmin
{
    public interface ISuperAdminBookService
    {
        Task<BookDto?> CreateBookAsync(CreateBookDto dto, IFormFile? imageFile);
        Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto dto, IFormFile? imageFile);
        Task<bool> DeleteBookAsync(int id, int userId);
        Task<AdminViewModel> GetQuickStatsAsync(int userId);
        Task<BookListViewModel> GetBooksAsync(int page, int pageSize, string? search, int? categoryId, string? sortBy);
        Task<BookFormViewModel?> GetCreateViewModelAsync();
        Task<BookFormViewModel?> GetEditViewModelAsync(int id);
        Task<List<SelectListItem>> GetCategoriesAsync();

        IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload(DateTime? startDate = null, DateTime? endDate = null);
        IEnumerable<CategoryBookCountViewModel> BooksByCategory();
        IEnumerable<AuthorBookCountViewModel> BooksByAuthor();
        FavoriteStatsViewModel FavoriteStats();
    }
}
