using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;
namespace OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders
{
    public interface ICartService
    {
        Task<List<ShoppingCartViewModel>> GetUserCartAsync(int userId);
        Task<CartSummaryViewModel> GetCartSummaryAsync(int userId);
        Task<bool> AddOrUpdateCartAsync(int userId, int bookId, int quantity = 1);
        Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity);
        Task<bool> RemoveCartItemAsync(int userId, int bookId);
        Task<List<AdminCartViewModel>> GetAllCartsAsync(int? adminUserId = null);
        Task<CheckOutViewModel> CheckoutDetailsAsync(int userId);
        Task<bool> ProcessCheckoutAsync(int userId, CheckOutRequestViewModel request);
        Task<bool> DeductInventoryAsync(int orderId);

        // New methods for enhanced functionality
        Task<int> GetCartItemCountAsync(int userId);
        Task<(bool Success, string Message, int CartCount)> AddToCartAsync(int userId, int bookId, int quantity);
    }
}
