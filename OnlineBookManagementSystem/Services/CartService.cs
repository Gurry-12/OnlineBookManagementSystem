using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class CartService : ICartService
    {
        private readonly BookManagementContext _context;

        public CartService(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<List<ShoppingCart>> GetUserCartAsync(int userId)
        {
            return await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && (sc.Book.IsDeleted == false) && (sc.IsDeleted == false)) // Ensure items are not deleted
                .Include(sc => sc.Book)
                .Include(sc => sc.User)
                .ToListAsync();
        }

        public async Task<bool> AddOrUpdateCartAsync(int userId, int bookId)
        {
            var existingCart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId && c.IsDeleted == false);

            if (existingCart != null)
            {
                existingCart.Quantity = (existingCart.Quantity ?? 0) + 1;
                _context.ShoppingCarts.Update(existingCart);
            }
            else
            {
                var cart = new ShoppingCart
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = 1
                };
                await _context.ShoppingCarts.AddAsync(cart);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartQuantityAsync(int userId, int bookId, int quantity)
        {
            var cartItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId && c.IsDeleted == false);

            if (cartItem == null)
                return false;

            cartItem.Quantity = quantity;

            if (quantity <= 0)
            {

                cartItem.IsDeleted = true; // Soft delete

            }
            else
                _context.ShoppingCarts.Update(cartItem);

            await _context.SaveChangesAsync();
            return true;
        }

        public List<object> GetAllCartItems(int userId)
        {
            return _context.ShoppingCarts
                .Where(c => c.UserId == userId && (c.Book.IsDeleted == false) && (c.IsDeleted == false)) // Ensure items are not deleted
                .Select(c => new
                {
                    bookId = c.BookId,
                    quantity = c.Quantity ?? 0
                })
                .ToList<object>();
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int bookId)
        {
            var item = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

            if (item == null)
                return false;

            item.IsDeleted = true; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CheckOutViewModel> CheckoutDetails(int userId)
        {
            // Retrieve the cart items for the user
            var cartItems = await _context.ShoppingCarts
                .Where(sc => sc.UserId == userId && (sc.Book.IsDeleted == false) && (sc.IsDeleted == false)) // Ensure items are not deleted
                .Include(sc => sc.Book) // Include book details
                .ToListAsync();

            // Calculate the total amount
            decimal totalAmount = cartItems.Sum(item => (item.Quantity ?? 0) * (item.Book.Price ?? 0));

            // Return the CheckoutViewModel with cart items and total amount
            return new CheckOutViewModel
            {
                CartItems = cartItems,
                TotalAmount = totalAmount
            };
        }

        public async Task<bool> ProcessCheckoutAsync(int userId, string name, string address, string paymentMethod)
        {
            // 1. Get user's cart items
            var cartItems = await _context.ShoppingCarts
                .Where(c => c.UserId == userId && c.IsDeleted == false)
                .Include(c => c.Book)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
                return false; // Cart empty

            // 2. Calculate totals
            var subTotal = cartItems.Sum(item => item.Book.Price * item.Quantity);
            var tax = (subTotal * 10) / 100;
            var grandTotal = subTotal + tax;

            // 3. Create new Order
            var order = new Order
            {
                UserId = userId,
                FullName = name,
                Address = address,
                PaymentMethod = paymentMethod,
                TotalAmount = grandTotal,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to generate Order ID

            // 4. Create OrderDetails
            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    BookId = cartItem.BookId,
                    Quantity = (int)cartItem.Quantity,
                    Price = cartItem.Book.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            // 5. Remove cart items (hard delete)
            foreach (var cartItem in cartItems)
            {
                cartItem.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return true; // Successful
        }


    }
}
