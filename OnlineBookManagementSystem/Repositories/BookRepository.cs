using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;
using OnlineBookManagementSystem.Models.ViewModel.ChartViewModel;

namespace OnlineBookManagementSystem.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(BookManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetActiveBooksAsync()
        {
            return await _dbSet.Where(b => b.IsDeleted == false).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync()
        {
            return await _dbSet.Where(b => (bool)!b.IsDeleted && (bool)b.IsFavorite).ToListAsync();
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var book = await _dbSet.FirstOrDefaultAsync(b => b.Id == id && (bool)!b.IsDeleted);
            if (book == null)
                return false;

            book.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Book> Books, int TotalPages, int CurrentPage)> GetPaginatedBooksDataAsync(int page, int pageSize)
        {
             int totalBooks = await _dbSet.CountAsync(b => b.IsDeleted == false);
             var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

             var books = await _dbSet.Where(b => b.IsDeleted == false)
                                 .Include(b => b.Category)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

             return (books, totalPages, page);
        }

        public IEnumerable<MonthlyBookUploadViewModel> MonthlyBookUpload()
        {
            var books = _dbSet.Where(b => b.CreatedDate != null && b.IsDeleted == false).ToList();

            return books
                .GroupBy(b => new { b.CreatedDate.Year, b.CreatedDate.Month })
                .Select(g => new MonthlyBookUploadViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();
        }

        public IEnumerable<CategoryBookCountViewModel> BooksByCategory()
        {
            return _dbSet
                .Where(b => b.Category != null && b.IsDeleted == false)
                .Include(b => b.Category)
                .AsEnumerable()
                .GroupBy(b => b.Category!.Name)
                .Select(g => new CategoryBookCountViewModel
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        public IEnumerable<AuthorBookCountViewModel> BooksByAuthor()
        {
            return _dbSet
                .Where(b => !string.IsNullOrEmpty(b.Author) && b.IsDeleted == false)
                .AsEnumerable()
                .GroupBy(b => b.Author!)
                .Select(g => new AuthorBookCountViewModel
                {
                    AuthorName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(a => a.Count)
                .ToList();
        }

        public FavoriteStatsViewModel FavoriteStats()
        {
            var total = _dbSet.Count();
            var favoriteCount = _dbSet.Count(b => b.IsFavorite == true && b.IsDeleted == false);
            return new FavoriteStatsViewModel
            {
                FavoriteCount = favoriteCount,
                NonFavoriteCount = total - favoriteCount
            };
        }
    }
}
