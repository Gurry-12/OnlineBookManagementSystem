using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Cart
{
    public interface ICartRepository : IRepository<ShoppingCart>
    {
        Task<List<ShoppingCart>> GetUserCartAsync(int userId);
        Task<ShoppingCart?> GetCartItemAsync(int userId, int bookId);
        Task<int> GetCartItemsCountAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
        Task ClearUserCartAsync(int userId);
        Task<bool> HasCartItemAsync(int userId, int bookId);
        Task<List<ShoppingCart>> GetCartItemsWithBooksAsync(int userId);
        Task<ShoppingCart> UpdateAsync(ShoppingCart entity);
    }
}