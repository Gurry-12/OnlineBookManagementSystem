using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Interfaces
{
    public interface ICartService
    {
        Task<List<ShoppingCartViewModel>> GetUserCartAsync(int userId);  // Projected VM
        Task<CartSummaryViewModel> GetCartSummaryAsync(int userId);  // Totals
        Task<bool> AddOrUpdateCartAsync(int userId, int bookId, int quantity = 1);  // Validate stock
        Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity);
        Task<bool> RemoveCartItemAsync(int userId, int bookId);
        Task<List<AdminCartViewModel>> GetAllCartsAsync(int? adminUserId = null);  // For Admin
        Task<CheckOutViewModel> CheckoutDetailsAsync(int userId);
        Task<bool> ProcessCheckoutAsync(int userId, CheckOutRequestViewModel request);  // Enriched
        Task<bool> DeductInventoryAsync(int orderId);  // Post-checkout
    }
}