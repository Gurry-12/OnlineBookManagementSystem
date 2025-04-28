using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface ICartService
    {
        Task<List<ShoppingCart>> GetUserCartAsync(int userId);
        Task<bool> AddOrUpdateCartAsync(int userId, int bookId);
        Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity);
        List<object> GetAllCartItems(int userId);
        Task<bool> RemoveCartItemAsync(int userId, int bookId);
        Task<CheckOutViewModel> CheckoutDetails(int userId);
        Task<bool> ProcessCheckoutAsync(int userId, string name, string address, string paymentMethod);
    }
}
