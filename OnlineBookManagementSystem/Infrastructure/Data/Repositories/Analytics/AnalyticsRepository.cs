using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Analytics;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories.Analytics
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly BookManagementContext _context;

        public AnalyticsRepository(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetMonthlyBookUploadsAsync(int year)
        {
            return await _context.Books
                .Where(b => !b.IsDeleted && b.CreatedAt.Year == year)
                .GroupBy(b => b.CreatedAt.Month)
                .ToDictionaryAsync(
                    g => new DateTime(year, g.Key, 1).ToString("MMM"),
                    g => g.Count()
                );
        }

        public async Task<Dictionary<string, int>> GetCategoryDistributionAsync()
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Category)
                .GroupBy(b => b.Category.Name)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetAuthorBookCountAsync()
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .GroupBy(b => b.Author)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetFavoriteStatsAsync()
        {
            return await _context.UserFavorites
                .Where(uf => !uf.IsDeleted)
                .Include(uf => uf.Book)
                .GroupBy(uf => uf.Book.Title)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenueAsync(int year)
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && 
                           o.PaymentStatus == PaymentStatus.Paid && 
                           o.OrderDate.HasValue &&
                           o.OrderDate.Value.Year == year)
                .GroupBy(o => o.OrderDate.Value.Month)
                .ToDictionaryAsync(
                    g => new DateTime(year, g.Key, 1).ToString("MMM"),
                    g => g.Sum(o => o.TotalAmount.Amount)
                );
        }

        public async Task<Dictionary<string, int>> GetOrderStatusDistributionAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted)
                .GroupBy(o => o.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.CountAsync(b => !b.IsDeleted);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsDeleted == null || !(bool)u.IsDeleted);
        }

        public async Task<int> GetTotalOrdersCountAsync()
        {
            return await _context.Orders.CountAsync(o => !o.IsDeleted);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount.Amount);
        }

        public async Task<List<Book>> GetTopRatedBooksAsync(int count)
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Book>> GetMostFavoritedBooksAsync(int count)
        {
            return await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.UserFavorites)
                .OrderByDescending(b => b.UserFavorites.Count(uf => !uf.IsDeleted))
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<User>> GetMostActiveUsersAsync(int count)
        {
            return await _context.Users
                .Where(u => u.IsDeleted == null || !(bool)u.IsDeleted)
                .Include(u => u.Orders)
                .OrderByDescending(u => u.Orders.Count(o => !o.IsDeleted))
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, (int NewUsers, int TotalUsers, int ActiveUsers)>> GetUserGrowthDataAsync()
        {
            var result = new Dictionary<DateTime, (int NewUsers, int TotalUsers, int ActiveUsers)>();
            var startDate = DateTime.UtcNow.AddMonths(-12).Date;
            
            for (int i = 0; i < 12; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);
                
                var newUsers = await _context.Users
                    .CountAsync(u => (u.IsDeleted == null || !(bool)u.IsDeleted) && 
                                    u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                
                var totalUsers = await _context.Users
                    .CountAsync(u => (u.IsDeleted == null || !(bool)u.IsDeleted) && 
                                    u.CreatedAt < monthEnd);
                
                var activeUsers = await _context.Users
                    .CountAsync(u => (u.IsDeleted == null || !(bool)u.IsDeleted) && 
                                    u.LastLoginDate >= monthStart && u.LastLoginDate < monthEnd);
                
                result[monthStart] = (newUsers, totalUsers, activeUsers);
            }
            
            return result;
        }

        public async Task<Dictionary<DateTime, (decimal Revenue, int OrderCount, decimal AverageOrderValue)>> GetRevenueGrowthDataAsync()
        {
            var result = new Dictionary<DateTime, (decimal Revenue, int OrderCount, decimal AverageOrderValue)>();
            var startDate = DateTime.UtcNow.AddMonths(-12).Date;
            
            for (int i = 0; i < 12; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);
                
                var monthlyOrders = await _context.Orders
                    .Where(o => !o.IsDeleted && 
                               o.PaymentStatus == PaymentStatus.Paid &&
                               o.OrderDate >= monthStart && o.OrderDate < monthEnd)
                    .ToListAsync();
                
                var revenue = monthlyOrders.Sum(o => o.TotalAmount.Amount);
                var orderCount = monthlyOrders.Count;
                var averageOrderValue = orderCount > 0 ? revenue / orderCount : 0;
                
                result[monthStart] = (revenue, orderCount, averageOrderValue);
            }
            
            return result;
        }

        public async Task<List<(int BookId, string BookTitle, string Author, int ViewCount, int OrderCount, int FavoriteCount, double AverageRating, int ReviewCount)>> GetBookPopularityDataAsync()
        {
            var books = await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.UserFavorites)
                .Include(b => b.BookReviews)
                .ToListAsync();
            
            var result = new List<(int BookId, string BookTitle, string Author, int ViewCount, int OrderCount, int FavoriteCount, double AverageRating, int ReviewCount)>();
            
            foreach (var book in books)
            {
                var orderCount = await _context.OrderDetails
                    .Where(od => !od.IsDeleted && od.BookId == book.Id)
                    .SumAsync(od => od.Quantity);
                
                var favoriteCount = book.UserFavorites.Count(uf => !uf.IsDeleted);
                var reviewCount = book.BookReviews.Count(br => !br.IsDeleted);
                
                result.Add((
                    book.Id,
                    book.Title,
                    book.Author,
                    0, // ViewCount - would need to implement view tracking
                    orderCount,
                    favoriteCount,
                    book.AverageRating,
                    reviewCount
                ));
            }
            
            return result.OrderByDescending(x => x.OrderCount + x.FavoriteCount).ToList();
        }

        public async Task<List<(int CategoryId, string CategoryName, int BookCount, int OrderCount, decimal Revenue, double Percentage)>> GetCategoryDistributionDataAsync()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Books)
                .ToListAsync();
            
            var totalRevenue = await GetTotalRevenueAsync();
            var result = new List<(int CategoryId, string CategoryName, int BookCount, int OrderCount, decimal Revenue, double Percentage)>();
            
            foreach (var category in categories)
            {
                var bookCount = category.Books.Count(b => !b.IsDeleted);
                
                var categoryRevenue = await _context.OrderDetails
                    .Where(od => !od.IsDeleted && 
                                category.Books.Any(b => b.Id == od.BookId && !b.IsDeleted))
                    .Include(od => od.Order)
                    .Where(od => od.Order.PaymentStatus == PaymentStatus.Paid)
                    .SumAsync(od => od.Subtotal.Amount);
                
                var orderCount = await _context.OrderDetails
                    .Where(od => !od.IsDeleted && 
                                category.Books.Any(b => b.Id == od.BookId && !b.IsDeleted))
                    .SumAsync(od => od.Quantity);
                
                var percentage = totalRevenue > 0 ? (double)(categoryRevenue / totalRevenue) * 100 : 0;
                
                result.Add((
                    category.Id,
                    category.Name,
                    bookCount,
                    orderCount,
                    categoryRevenue,
                    percentage
                ));
            }
            
            return result.OrderByDescending(x => x.Revenue).ToList();
        }
    }
}