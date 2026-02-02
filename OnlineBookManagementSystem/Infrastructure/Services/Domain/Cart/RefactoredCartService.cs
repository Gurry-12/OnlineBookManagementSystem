using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Cart;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Cart
{
    /// <summary>
    /// Refactored Cart Service following SRP.
    /// Only handles cart business logic, delegates data access to repository.
    /// </summary>
    public class RefactoredCartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderCommandService _orderCommandService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RefactoredCartService> _logger;
        private readonly IActivityLogger _activityLogger;

        public RefactoredCartService(
            ICartRepository cartRepository,
            IBookRepository bookRepository,
            IOrderRepository orderRepository,
            IOrderCommandService orderCommandService,
            IMemoryCache cache,
            ILogger<RefactoredCartService> logger,
            IActivityLogger activityLogger)
        {
            _cartRepository = cartRepository;
            _bookRepository = bookRepository;
            _orderRepository = orderRepository;
            _orderCommandService = orderCommandService;
            _cache = cache;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public async Task<List<ShoppingCartViewModel>> GetUserCartAsync(int userId)
        {
            var cacheKey = $"cart_{userId}";
            if (!_cache.TryGetValue(cacheKey, out List<ShoppingCartViewModel>? cartItems))
            {
                var cartEntities = await _cartRepository.GetCartItemsWithBooksAsync(userId);

                cartItems = cartEntities.Select(sc => new ShoppingCartViewModel
                {
                    Id = sc.Id,
                    BookId = sc.BookId,
                    BookTitle = sc.Book.Title,
                    Author = sc.Book.Author,
                    Price = sc.Book.Price.Amount,
                    Quantity = sc.Quantity,
                    ImageUrl = sc.Book.ImageUrl ?? "/images/default-book.png",
                    CategoryName = sc.Book.Category?.Name ?? "Uncategorized",
                    StockQuantity = sc.Book.StockQuantity,
                    IsAvailable = sc.Book.StockQuantity > 0 && !sc.Book.IsDeleted
                }).ToList();

                _cache.Set(cacheKey, cartItems, TimeSpan.FromMinutes(5));
            }

            return cartItems ?? new List<ShoppingCartViewModel>();
        }

        public async Task<CartSummaryViewModel> GetCartSummaryAsync(int userId)
        {
            try
            {
                var itemCount = await _cartRepository.GetCartItemsCountAsync(userId);
                var total = await _cartRepository.GetCartTotalAsync(userId);

                return new CartSummaryViewModel
                {
                    ItemCount = itemCount,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddOrUpdateCartAsync(int userId, int bookId, int quantity = 1)
        {
            try
            {
                // Validate book availability
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null || book.IsDeleted)
                {
                    _logger.LogWarning("Attempt to add non-existent book {BookId} to cart for user {UserId}", bookId, userId);
                    return false;
                }

                if (book.StockQuantity < quantity)
                {
                    _logger.LogWarning("Insufficient stock for book {BookId}. Available: {Stock}, Requested: {Quantity}",
                        bookId, book.StockQuantity, quantity);
                    return false;
                }

                // Check if item already exists in cart
                var existingItem = await _cartRepository.GetCartItemAsync(userId, bookId);
                if (existingItem != null)
                {
                    // Update quantity
                    var newQuantity = existingItem.Quantity + quantity;
                    if (newQuantity > book.StockQuantity)
                    {
                        _logger.LogWarning("Cannot add more items. Total would exceed stock. Book: {BookId}, Stock: {Stock}, Requested Total: {Total}",
                            bookId, book.StockQuantity, newQuantity);
                        return false;
                    }

                    existingItem.Quantity = newQuantity;
                    existingItem.UpdateTimestamp();
                    await _cartRepository.UpdateAsync(existingItem);
                }
                else
                {
                    // Create new cart item
                    var cartItem = new ShoppingCart
                    {
                        UserId = userId,
                        BookId = bookId,
                        Quantity = quantity
                    };

                    await _cartRepository.AddAsync(cartItem);
                }

                // Clear cache
                _cache.Remove($"cart_{userId}");

                // Log activity
                await _activityLogger.LogAsync(
                    "Cart Item Added",
                    $"Added {quantity} x '{book.Title}' to cart",
                    userId);

                _logger.LogInformation("Added {Quantity} x book {BookId} to cart for user {UserId}", quantity, bookId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book {BookId} to cart for user {UserId}", bookId, userId);
                throw;
            }
        }

        public async Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity)
        {
            try
            {
                var cartItem = await _cartRepository.GetCartItemAsync(userId, bookId);
                if (cartItem == null)
                {
                    return false;
                }

                if (quantity <= 0)
                {
                    return await RemoveCartItemAsync(userId, bookId);
                }

                // Validate stock availability
                var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                if (book == null || quantity > book.StockQuantity)
                {
                    _logger.LogWarning("Cannot update cart item. Insufficient stock. Available: {Stock}, Requested: {Quantity}",
                        book?.StockQuantity ?? 0, quantity);
                    return false;
                }

                cartItem.Quantity = quantity;
                cartItem.UpdateTimestamp();
                await _cartRepository.UpdateAsync(cartItem);

                // Clear cache
                _cache.Remove($"cart_{userId}");

                _logger.LogInformation("Updated cart item quantity to {Quantity} for user {UserId}", quantity, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int bookId)
        {
            try
            {
                var cartItem = await _cartRepository.GetCartItemAsync(userId, bookId);
                if (cartItem == null)
                {
                    return false;
                }

                cartItem.MarkAsDeleted();
                await _cartRepository.UpdateAsync(cartItem);

                // Clear cache
                _cache.Remove($"cart_{userId}");

                // Log activity
                await _activityLogger.LogAsync(
                    "Cart Item Removed",
                    $"Removed item from cart",
                    userId);

                _logger.LogInformation("Removed cart item for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<AdminCartViewModel>> GetAllCartsAsync(int? adminUserId = null)
        {
            try
            {
                var allCartItems = await _cartRepository.GetAllCartsAsync();

                var adminCarts = allCartItems
                    .GroupBy(sc => sc.UserId)
                    .Select(group => new AdminCartViewModel
                    {
                        UserId = group.Key,
                        UserName = group.First().User?.Name ?? "Unknown User",
                        UserEmail = group.First().User?.Email ?? "Unknown Email",
                        ItemCount = group.Sum(sc => sc.Quantity),
                        TotalValue = group.Sum(sc => sc.Quantity * sc.Book.Price.Amount),
                        LastUpdated = group.Max(sc => sc.UpdatedAt),
                        Items = group.Select(sc => new CartItemViewModel
                        {
                            BookId = sc.BookId,
                            BookTitle = sc.Book.Title,
                            Author = sc.Book.Author,
                            Quantity = sc.Quantity,
                            Price = sc.Book.Price.Amount,
                            Subtotal = sc.Quantity * sc.Book.Price.Amount
                        }).ToList()
                    })
                    .OrderByDescending(ac => ac.LastUpdated)
                    .ToList();

                return adminCarts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all carts for admin {AdminUserId}", adminUserId);
                return new List<AdminCartViewModel>();
            }
        }

        public async Task<CheckOutViewModel> CheckoutDetailsAsync(int userId)
        {
            try
            {
                var cartItems = await GetUserCartAsync(userId);
                var summary = await GetCartSummaryAsync(userId);

                return new CheckOutViewModel
                {
                    CartItems = cartItems,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout details for user {UserId}", userId);
                throw;
            }
        }

        public async Task<(bool Success, int OrderId, string Message)> ProcessCheckoutAsync(int userId, CheckOutRequestViewModel request)
        {
            try
            {
                // Get cart items first
                var cartItems = await _cartRepository.GetCartItemsWithBooksAsync(userId);
                if (!cartItems.Any())
                {
                    return (false, 0, "Cart is empty");
                }

                // Validate stock availability for all items
                foreach (var cartItem in cartItems)
                {
                    var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                    if (book == null || book.IsDeleted)
                    {
                        return (false, 0, $"Book '{cartItem.Book?.Title}' is no longer available");
                    }

                    if (book.StockQuantity < cartItem.Quantity)
                    {
                        return (false, 0, $"Insufficient stock for '{book.Title}'. Available: {book.StockQuantity}, Requested: {cartItem.Quantity}");
                    }
                }

                // Create order request from cart items
                var createOrderRequest = new CreateOrderRequest
                {
                    UserId = userId,
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes,
                    Items = cartItems.Select(ci => new OrderItemRequest
                    {
                        BookId = ci.BookId,
                        Quantity = ci.Quantity,
                        Price = ci.Book.Price.Amount
                    }).ToList()
                };

                // Create the order using the order command service
                var orderResult = await _orderCommandService.CreateOrderAsync(createOrderRequest);

                if (orderResult.Success)
                {
                    // Clear the cart only after successful order creation
                    await _cartRepository.ClearUserCartAsync(userId);
                    _cache.Remove($"cart_{userId}");

                    await _activityLogger.LogAsync(
                        "Checkout Processed",
                        $"Order #{orderResult.OrderId} created from cart",
                        userId);

                    _logger.LogInformation("Checkout processed successfully for user {UserId}, Order ID: {OrderId}", userId, orderResult.OrderId);
                    return (true, orderResult.OrderId, "Order created successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to create order during checkout for user {UserId}: {Message}", userId, orderResult.Message);
                    return (false, 0, orderResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout for user {UserId}", userId);
                return (false, 0, "An error occurred while processing your order");
            }
        }

        public async Task<bool> DeductInventoryAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (order == null) return false;

                foreach (var detail in order.OrderDetails)
                {
                    var book = await _bookRepository.GetByIdAsync(detail.BookId);
                    if (book != null)
                    {
                        book.StockQuantity -= detail.Quantity;
                        book.UpdateTimestamp();
                        await _bookRepository.UpdateAsync(book);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting inventory for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await _cartRepository.GetCartItemsCountAsync(userId);
        }

        public async Task<(bool Success, string Message, int CartCount)> AddToCartAsync(int userId, int bookId, int quantity)
        {
            try
            {
                var success = await AddOrUpdateCartAsync(userId, bookId, quantity);
                var cartCount = await GetCartItemCountAsync(userId);

                if (success)
                {
                    return (true, "Item added to cart successfully", cartCount);
                }
                else
                {
                    return (false, "Failed to add item to cart", cartCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart for user {UserId}", userId);
                var cartCount = await GetCartItemCountAsync(userId);
                return (false, "An error occurred while adding to cart", cartCount);
            }
        }
    }
}