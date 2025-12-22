using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Models.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookManagementSystem.Services.Common
{
    public class BookManager : IBookManager
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BookManager> _logger;

        public BookManager(IWebHostEnvironment env, ILogger<BookManager> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> SaveImageAsync(IFormFile image, string bookId)
        {
            if (image == null || image.Length == 0) return null;

            if (image.Length > 5 * 1024 * 1024 || !image.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning("Invalid image upload: {ContentType}, Size: {Length}", image.ContentType, image.Length);
                return null;
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "images/books");
            Directory.CreateDirectory(uploadsDir);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(image.FileName + DateTime.UtcNow.Ticks.ToString()));
            var filename = $"{bookId}_{Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-")}.{image.ContentType.Split('/')[1]}";
            var filepath = Path.Combine(uploadsDir, filename);

            using var inputStream = image.OpenReadStream();
            using var imageSharp = await Image.LoadAsync(inputStream);
            imageSharp.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(400, 600) }));
            await imageSharp.SaveAsJpegAsync(filepath);

            return filename;
        }

        public void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var path = Path.Combine(_env.WebRootPath, "images/books", imageUrl);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                ISBN = book.ISBN,
                ImageUrl = book.ImageUrl,
                StockQuantity = book.StockQuantity,
                Description = book.Description,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.Name,
                IsFavorite = book.IsFavorite,
                CreatedAt = book.CreatedAt
            };
        }

        public Book MapToEntity(CreateBookDto dto)
        {
            return new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Price = dto.Price,
                ISBN = dto.ISBN,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        public void UpdateEntity(Book book, UpdateBookDto dto)
        {
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Price = dto.Price;
            book.ISBN = dto.ISBN;
            book.StockQuantity = dto.StockQuantity;
            book.Description = dto.Description;
            book.CategoryId = dto.CategoryId;
            book.UpdatedAt = DateTime.UtcNow;
        }
    }
}
