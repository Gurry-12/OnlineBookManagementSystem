using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Cart;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Infrastructure.Data.Repositories.Cart
{
    public class CartRepository : Repository<ShoppingCart>, ICartRepository
    {
        public CartRepository(BookManagementContext context) : base(context)
        {
        }

        public async Task<List<ShoppingCart>> GetUserCartAsync(int userId)
        {
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                .Include(sc => sc.Book)
                    .ThenInclude(b => b.Category)
                .ToListAsync();
        }

        public async Task<ShoppingCart?> GetCartItemAsync(int userId, int bookId)
        {
            return await _context.ShoppingCarts
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.BookId == bookId && !sc.IsDeleted);
        }

        public async Task<int> GetCartItemsCountAsync(int userId)
        {
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                .SumAsync(sc => sc.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                .Include(sc => sc.Book)
                .SumAsync(sc => sc.Book.Price.Amount * sc.Quantity);
        }

        public async Task ClearUserCartAsync(int userId)
        {
            var cartItems = await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                item.MarkAsDeleted();
            }

            _context.ShoppingCarts.UpdateRange(cartItems);
        }

        public async Task<bool> HasCartItemAsync(int userId, int bookId)
        {
            return await _context.ShoppingCarts
                .AnyAsync(sc => sc.UserId == userId && sc.BookId == bookId && !sc.IsDeleted);
        }

        public async Task<List<ShoppingCart>> GetCartItemsWithBooksAsync(int userId)
        {
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                .Include(sc => sc.Book)
                    .ThenInclude(b => b.Category)
                .ToListAsync();
        }

        public async Task<List<ShoppingCart>> GetAllCartsAsync()
        {
            return await _context.ShoppingCarts
                .Where(sc => !sc.IsDeleted)
                .Include(sc => sc.Book)
                    .ThenInclude(b => b.Category)
                .Include(sc => sc.User)
                .OrderByDescending(sc => sc.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ShoppingCart> UpdateAsync(ShoppingCart entity)
        {
            return await base.UpdateAsync(entity);
        }
    }
}