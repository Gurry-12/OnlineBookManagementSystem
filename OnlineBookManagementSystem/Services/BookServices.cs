using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.ViewModel;

namespace OnlineBookManagementSystem.Services
{
    public class BookServices : IBookService
    {
        private readonly BookManagementContext _context;

        public BookServices(BookManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Where(b => b.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<Models.Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> AddBookAsync(Models.Book bookData)
        {
            await _context.Books.AddAsync(bookData);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBookAsync(Models.Book bookData)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id);
            if (existingBook == null)
                return false;

            existingBook.Title = bookData.Title;
            existingBook.Author = bookData.Author;
            existingBook.Stock = bookData.Stock;
            existingBook.Isbn = bookData.Isbn;
            existingBook.ImgUrl = bookData.ImgUrl;
            existingBook.Price = bookData.Price;
            existingBook.CategoryId = bookData.CategoryId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteBookAsync(int id)
        {
            var book = await _context.Books
                .Where(b => (bool)!b.IsDeleted)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            book.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Models.Book>> GetFavoriteBooksAsync()
        {
            return await _context.Books
                .Where(b => (bool)!b.IsDeleted && (bool)b.IsFavorite)
                .ToListAsync();
        }

        public async Task<bool> ToggleFavoriteAsync(int id)
        {
            var book = await _context.Books
                .Where(b => (bool)!b.IsDeleted)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            book.IsFavorite = !(book.IsFavorite ?? false);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "User" && u.IsDeleted == false)
                .Select(u => new
                {   u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role,
                    CartItemCount = u.ShoppingCarts.Count(sc => sc.UserId == u.Id && sc.IsDeleted == false)
                })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<BookFormViewModel?> GetCreateBookViewModelAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return new BookFormViewModel
            {
                Book = null,
                CategoryList = categories
            };
        }

        public async Task<BookFormViewModel?> GetEditBookViewModelAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return null;

            var categories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

            return new BookFormViewModel
            {
                Book = book,
                CategoryList = categories
            };
        }

        public AdminViewModel GetQuickStats(int id)
        {
            
            return new AdminViewModel
            {
                TotalBooks = _context.Books.Where(b => b.IsDeleted == false).Count(),
                TotalUsers = _context.Users.Where(u => u.IsDeleted == false && u.Role == "User").Count(),

                TotalOrders = _context.Orders.Count(),
                TotalCategories = _context.Categories.Where(c => c.IsDeleted == false).Count(),
                User = _context.Users.Where(u => u.Id == id).Where(u => u.IsDeleted == false).FirstOrDefault()
            };
        }
    }
}
