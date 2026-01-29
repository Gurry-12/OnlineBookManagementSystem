using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;
using Image = SixLabors.ImageSharp.Image;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Books
{
    public class BookCommandService : IBookCommandService
    {
        private readonly BookManagementContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BookCommandService> _logger;
        private readonly IActivityLogger _activityLogger;

        public BookCommandService(
            BookManagementContext context,
            IWebHostEnvironment env,
            ILogger<BookCommandService> logger,
            IActivityLogger activityLogger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        public async Task<bool> AddBookAsync(Book bookData, IFormFile? imageFile = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            string? tempImagePath = null;

            try
            {
                // Validate input
                if (bookData == null)
                {
                    _logger.LogWarning("Attempted to add null book data");
                    return false;
                }

                // Set timestamps and defaults
                bookData.CreatedAt = DateTime.UtcNow;
                bookData.UpdatedAt = DateTime.UtcNow;
                bookData.IsDeleted = false;

                // Save book first to get ID
                await _context.Books.AddAsync(bookData);
                await _context.SaveChangesAsync(); // This generates the ID

                // Now save image with correct ID
                if (imageFile != null)
                {
                    var imageUrl = await SaveImageAsync(imageFile, bookData.Id.ToString());
                    if (imageUrl != null)
                    {
                        bookData.ImageUrl = imageUrl;
                        tempImagePath = Path.Combine(_env.WebRootPath, "images/books", imageUrl);
                        await _context.SaveChangesAsync(); // Update with image URL
                    }
                    else
                    {
                        _logger.LogWarning("Failed to save image for book {BookId}, continuing without image", bookData.Id);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Book added successfully: {Title} (ID: {BookId})", bookData.Title, bookData.Id);
                await _activityLogger.LogAsync("BookAdded", $"New book '{bookData.Title}' created.", bookData.Id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Clean up orphaned image file
                if (!string.IsNullOrEmpty(tempImagePath) && File.Exists(tempImagePath))
                {
                    try
                    {
                        File.Delete(tempImagePath);
                        _logger.LogInformation("Cleaned up orphaned image file: {ImagePath}", tempImagePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up orphaned image file: {ImagePath}", tempImagePath);
                    }
                }

                _logger.LogError(ex, "Failed to add book: {Title}", bookData?.Title ?? "Unknown");
                return false;
            }
        }

        public async Task<bool> UpdateBookAsync(Book bookData, IFormFile? imageFile = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            string? tempImagePath = null;
            string? oldImagePath = null;

            try
            {
                if (bookData == null)
                {
                    _logger.LogWarning("Attempted to update with null book data");
                    return false;
                }

                var existing = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookData.Id && b.IsDeleted == false);
                if (existing == null)
                {
                    _logger.LogWarning("Book not found for update: {BookId}", bookData.Id);
                    return false;
                }

                // Handle image update
                if (imageFile != null)
                {
                    var newImageUrl = await SaveImageAsync(imageFile, bookData.Id.ToString());
                    if (newImageUrl != null)
                    {
                        // Store old image path for cleanup after successful transaction
                        if (!string.IsNullOrEmpty(existing.ImageUrl))
                        {
                            oldImagePath = Path.Combine(_env.WebRootPath, "images/books", existing.ImageUrl);
                        }

                        existing.ImageUrl = newImageUrl;
                        tempImagePath = Path.Combine(_env.WebRootPath, "images/books", newImageUrl);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to save new image for book {BookId}, keeping existing image", bookData.Id);
                    }
                }

                // Update book properties
                existing.Title = bookData.Title ?? existing.Title;
                existing.Author = bookData.Author ?? existing.Author;
                existing.ISBN = bookData.ISBN ?? existing.ISBN;
                existing.Price = bookData.Price;
                existing.Description = bookData.Description ?? existing.Description;
                existing.CategoryId = bookData.CategoryId != 0 ? bookData.CategoryId : existing.CategoryId;
                existing.StockQuantity = bookData.StockQuantity;
                existing.LowStockThreshold = bookData.LowStockThreshold;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clean up old image after successful transaction
                if (!string.IsNullOrEmpty(oldImagePath) && File.Exists(oldImagePath))
                {
                    try
                    {
                        File.Delete(oldImagePath);
                        _logger.LogInformation("Deleted old image file: {ImagePath}", oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image file: {ImagePath}", oldImagePath);
                    }
                }

                _logger.LogInformation("Book updated successfully: {BookId} - {Title}", bookData.Id, existing.Title);
                await _activityLogger.LogAsync("BookUpdated", $"Book '{existing.Title}' updated.", bookData.Id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Clean up new image file on rollback
                if (!string.IsNullOrEmpty(tempImagePath) && File.Exists(tempImagePath))
                {
                    try
                    {
                        File.Delete(tempImagePath);
                        _logger.LogInformation("Cleaned up new image file after rollback: {ImagePath}", tempImagePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up new image file: {ImagePath}", tempImagePath);
                    }
                }

                _logger.LogError(ex, "Failed to update book: {BookId}", bookData?.Id ?? 0);
                return false;
            }
        }

        public async Task<bool> SoftDeleteBookAsync(int id, int userId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);
            if (book == null) return false;

            book.IsDeleted = true;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Book soft-deleted: {Id} by User {UserId}", id, userId);
            await _activityLogger.LogAsync("BookDeleted", $"Book '{book.Title}' soft-deleted.", userId);
            return true;
        }

        public async Task<string?> SaveImageAsync(IFormFile image, string bookId)
        {
            if (image?.Length == 0) return null;

            try
            {
                // Validate file
                if (!IsValidImage(image))
                    return null;

                var uploadsDir = Path.Combine(_env.WebRootPath, "images/books");
                Directory.CreateDirectory(uploadsDir);

                var filename = GenerateUniqueFileName(image, bookId);
                var filepath = Path.Combine(uploadsDir, filename);

                // Process and save image
                using var inputStream = image.OpenReadStream();
                using var imageSharp = await Image.LoadAsync(inputStream);

                imageSharp.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 600)
                }));

                await imageSharp.SaveAsJpegAsync(filepath, new JpegEncoder { Quality = 85 });

                _logger.LogInformation("Image saved for book {BookId}: {Filename}", bookId, filename);
                return filename;
            }
            catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
            {
                _logger.LogWarning(ex, "Invalid image for book {BookId}: {FileName}", bookId, image.FileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image for book {BookId}", bookId);
                return null;
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int bookId, int userId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsDeleted == false);
            if (book == null) return false;

            book.IsFavorite = !book.IsFavorite;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Favorite toggled for book {BookId} by {UserId}", bookId, userId);
            await _activityLogger.LogAsync("FavoriteToggled", $"Book '{book.Title}' favorited/unfavorited.", userId);
            return true;
        }

        public async Task<(bool Success, string Message, bool IsFavorite)> ToggleUserFavoriteAsync(int bookId, int userId)
        {
            try
            {
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);
                if (book == null)
                    return (false, "Book not found", false);

                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.BookId == bookId);

                bool isFavorite;
                if (existingFavorite != null)
                {
                    _context.UserFavorites.Remove(existingFavorite);
                    isFavorite = false;
                    await _activityLogger.LogAsync("FavoriteRemoved", $"Removed '{book.Title}' from favorites", userId);
                }
                else
                {
                    _context.UserFavorites.Add(new UserFavorite
                    {
                        UserId = userId,
                        BookId = bookId,
                        CreatedAt = DateTime.UtcNow
                    });
                    isFavorite = true;
                    await _activityLogger.LogAsync("FavoriteAdded", $"Added '{book.Title}' to favorites", userId);
                }

                await _context.SaveChangesAsync();
                return (true, isFavorite ? "Added to favorites" : "Removed from favorites", isFavorite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite for book {BookId} and user {UserId}", bookId, userId);
                return (false, "An error occurred", false);
            }
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileViewModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);
                if (user == null) return false;

                user.Name = model.Name;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("ProfileUpdated", "User profile updated", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user profile for user {UserId}", userId);
                return false;
            }
        }

        private bool IsValidImage(IFormFile image)
        {
            if (image.Length > 5 * 1024 * 1024)
            {
                _logger.LogWarning("Image too large: {Size} bytes", image.Length);
                return false;
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
            {
                _logger.LogWarning("Unsupported image type: {ContentType}", image.ContentType);
                return false;
            }

            return true;
        }

        private static string GenerateUniqueFileName(IFormFile image, string bookId)
        {
            var extension = Path.GetExtension(image.FileName)?.ToLower();
            if (string.IsNullOrEmpty(extension) || !new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
                extension = ".jpg";

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{image.FileName}_{bookId}_{DateTime.UtcNow.Ticks}"));
            return $"{bookId}_{Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-")[..16]}{extension}";
        }
    }
}