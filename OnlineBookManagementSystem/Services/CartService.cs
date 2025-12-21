using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class CartService : ICartService
    {
        private readonly BookManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CartService> _logger;
        private readonly IActivityLogger _activityLogger;
        private readonly IEmailSender _emailSender;  // For confirmation

        public CartService(
            BookManagementContext context,
            IMemoryCache cache,
            ILogger<CartService> logger,
            IActivityLogger activityLogger,
            IEmailSender emailSender = null)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _activityLogger = activityLogger;
            _emailSender = emailSender;
        }

        public async Task<List<ShoppingCartViewModel>> GetUserCartAsync(int userId)
        {
            var cacheKey = $"cart_{userId}";
            if (!_cache.TryGetValue(cacheKey, out List<ShoppingCartViewModel>? cartItems))
            {
                cartItems = await _context.ShoppingCarts
                    .Where(sc => sc.UserId == userId && !sc.IsDeleted && !sc.Book.IsDeleted)
                    .Include(sc => sc.Book)
                        .ThenInclude(b => b.Category)
                    .Include(sc => sc.User)
                    .Select(sc => new ShoppingCartViewModel
                    {
                        Id = sc.Id,
                        BookId = sc.BookId,
                        BookTitle = sc.Book.Title,
                        BookAuthor = sc.Book.Author,
                        BookPrice = sc.Book.Price,
                        BookImage = sc.Book.ImageUrl,
                        Quantity = sc.Quantity,
                        CategoryName = sc.Book.Category.Name,
                        Subtotal = sc.Book.Price * sc.Quantity
                    })
                    .OrderBy(sc => sc.BookTitle)
                    .ToListAsync();

                _cache.Set(cacheKey, cartItems, TimeSpan.FromMinutes(5));
            }
            return cartItems ?? new List<ShoppingCartViewModel>();
        }

        public async Task<CartSummaryViewModel> GetCartSummaryAsync(int userId)
        {
            var cart = await GetUserCartAsync(userId);
            var subtotal = cart.Sum(item => item.Subtotal);
            var tax = subtotal * 0.10m;  // 10% GST
            var shipping = subtotal > 1000 ? 0 : 50;
            var grandTotal = subtotal + tax + shipping;

            return new CartSummaryViewModel
            {
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                GrandTotal = grandTotal,
                ItemCount = cart.Count
            };
        }

        public async Task<bool> AddOrUpdateCartAsync(int userId, int bookId, int quantity = 1)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);
                if (book == null || book.StockQuantity < quantity)
                {
                    _logger.LogWarning("Stock insufficient for book {BookId}, requested {Quantity}, available {Stock}", bookId, quantity, book?.StockQuantity);
                    return false;
                }

                var existingCart = await _context.ShoppingCarts
                    .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.BookId == bookId && !sc.IsDeleted);

                if (existingCart != null)
                {
                    existingCart.Quantity += quantity;
                }
                else
                {
                    existingCart = new ShoppingCart
                    {
                        UserId = userId,
                        BookId = bookId,
                        Quantity = quantity,
                        IsDeleted = false,
                        AddedAt = DateTime.UtcNow
                    };
                    await _context.ShoppingCarts.AddAsync(existingCart);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _cache.Remove($"cart_{userId}");  // Invalidate
                _logger.LogInformation("Cart updated: User {UserId}, Book {BookId}, Qty {Quantity}", userId, bookId, quantity);
                await _activityLogger.LogAsync("CartAdd", $"Added {quantity} of '{book.Title}' to cart.", userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to add/update cart: User {UserId}, Book {BookId}", userId, bookId);
                return false;
            }
        }

        public async Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity)
        {
            if (quantity <= 0) return await RemoveCartItemAsync(userId, bookId);

            var cartItem = await _context.ShoppingCarts
                .Include(sc => sc.Book)
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.BookId == bookId && !sc.IsDeleted);

            if (cartItem == null || cartItem.Book.StockQuantity < quantity)
                return false;

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            _cache.Remove($"cart_{userId}");
            _logger.LogInformation("Cart quantity updated: User {UserId}, Book {BookId}, Qty {Quantity}", userId, bookId, quantity);
            await _activityLogger.LogAsync("CartUpdate", $"Updated quantity of '{cartItem.Book.Title}' to {quantity}.", userId);
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int bookId)
        {
            var cartItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.BookId == bookId && !sc.IsDeleted);

            if (cartItem == null) return false;

            cartItem.IsDeleted = true;  // Soft delete
            await _context.SaveChangesAsync();
            _cache.Remove($"cart_{userId}");
            _logger.LogInformation("Cart item removed: User {UserId}, Book {BookId}", userId, bookId);
            await _activityLogger.LogAsync("CartRemove", $"Removed '{cartItem.Book.Title}' from cart.", userId);
            return true;
        }

        public async Task<List<AdminCartViewModel>> GetAllCartsAsync(int? adminUserId = null)
        {
            return await _context.ShoppingCarts
                .Where(sc => !sc.IsDeleted && !sc.Book.IsDeleted)
                .Include(sc => sc.User)
                .Include(sc => sc.Book)
                .Select(sc => new AdminCartViewModel
                {
                    Id = sc.Id,
                    UserName = sc.User.Name,
                    UserEmail = sc.User.Email,
                    BookTitle = sc.Book.Title,
                    Quantity = sc.Quantity,
                    Subtotal = sc.Book.Price * sc.Quantity,
                    AddedAt = sc.AddedAt
                })
                .OrderByDescending(sc => sc.AddedAt)
                .ToListAsync();
        }

        public async Task<CheckOutViewModel> CheckoutDetailsAsync(int userId)
        {
            var cart = await GetUserCartAsync(userId);
            var summary = await GetCartSummaryAsync(userId);

            return new CheckOutViewModel
            {
                CartItems = cart,
                Subtotal = summary.Subtotal,
                Tax = summary.Tax,
                Shipping = summary.Shipping,
                GrandTotal = summary.GrandTotal,
                UserId = userId
            };
        }

        public async Task<bool> ProcessCheckoutAsync(int userId, CheckOutRequestViewModel request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItems = await _context.ShoppingCarts
                    .Include(sc => sc.Book)
                    .Where(sc => sc.UserId == userId && !sc.IsDeleted)
                    .ToListAsync();

                if (!cartItems.Any()) return false;

                var grandTotal = cartItems.Sum(item => item.Book.Price * item.Quantity);
                var tax = grandTotal * 0.10m;
                var shipping = grandTotal > 1000 ? 0 : 50;
                var total = grandTotal + tax + shipping;

                // Create Order
                var order = new Order
                {
                    UserId = userId,
                    FullName = request.Name,
                    Address = request.Address,
                    PaymentMethod = request.PaymentMethod,
                    TotalAmount = total,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    PaymentStatus = "Unpaid",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();  // For OrderId

                // Create OrderDetails
                foreach (var cartItem in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        BookId = cartItem.BookId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Book.Price,
                        Subtotal = cartItem.Book.Price * cartItem.Quantity
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Deduct stock
                    cartItem.Book.StockQuantity -= cartItem.Quantity;
                }

                // Soft delete cart items
                foreach (var cartItem in cartItems)
                {
                    cartItem.IsDeleted = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Email confirmation
                if (_emailSender != null)
                {
                    await _emailSender.SendEmailAsync(
                        _context.Users.First(u => u.Id == userId).Email,
                        "Order Confirmation",
                        $"Thank you! Order #{order.Id} placed for ₹{total}. Details: {request.Name} at {request.Address}."
                    );
                }

                _cache.Remove($"cart_{userId}");
                _logger.LogInformation("Checkout processed: Order {OrderId} for User {UserId}", order.Id, userId);
                await _activityLogger.LogAsync("OrderPlaced", $"New order #{order.Id} for ₹{total}.", userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Checkout failed for User {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeductInventoryAsync(int orderId)
        {
            // Called post-payment if async (e.g., Stripe webhook)
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Book)
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            foreach (var detail in orderDetails)
            {
                detail.Book.StockQuantity -= detail.Quantity;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Inventory deducted for Order {OrderId}", orderId);
            return true;
        }
    }
}