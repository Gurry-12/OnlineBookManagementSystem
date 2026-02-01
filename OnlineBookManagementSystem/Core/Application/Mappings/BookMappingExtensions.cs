using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.ChartViewModel;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;

namespace OnlineBookManagementSystem.Core.Application.Mappings
{
    /// <summary>
    /// Extension methods for mapping Book entities to DTOs and ViewModels
    /// </summary>
    public static class BookMappingExtensions
    {
        /// <summary>
        /// Maps Book entity to BookDto
        /// </summary>
        public static BookDto ToDto(this Book book)
        {
            if (book == null) return null;

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book?.ISBN,
                Price = book?.Price,
                CategoryId = book?.CategoryId,
                CategoryName = book.Category?.Name,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                StockQuantity = book.StockQuantity,
                LowStockThreshold = book.LowStockThreshold,
                PublicationDate = book.PublicationDate,
                AverageRating = book.AverageRating,
                TotalReviews = book.TotalReviews,
                IsFavorite = book.IsFavorite,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }

        /// <summary>
        /// Maps collection of Book entities to BookDto collection
        /// </summary>
        public static IEnumerable<BookDto> ToDto(this IEnumerable<Book> books)
        {
            return books?.Select(book => book.ToDto()) ?? Enumerable.Empty<BookDto>();
        }

        /// <summary>
        /// Maps BookDto to Book entity (for create operations)
        /// </summary>
        public static Book ToEntity(this BookDto dto)
        {
            if (dto == null) return null;

            return new Book
            {
                Id = dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto?.ISBN,
                Price = dto?.Price,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                StockQuantity = dto.StockQuantity,
                LowStockThreshold = dto.LowStockThreshold,
                PublicationDate = dto.PublicationDate,
                AverageRating = dto.AverageRating,
                TotalReviews = dto.TotalReviews,
                IsFavorite = dto.IsFavorite
            };
        }

        /// <summary>
        /// Updates existing Book entity with values from BookDto
        /// </summary>
        public static void UpdateFromDto(this Book book, BookDto dto)
        {
            if (book == null || dto == null) return;

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Price = dto.Price;
            book.CategoryId = dto.CategoryId;
            book.Description = dto.Description;
            book.ImageUrl = dto.ImageUrl;
            book.StockQuantity = dto.StockQuantity;
            book.LowStockThreshold = dto.LowStockThreshold;
            book.PublicationDate = dto.PublicationDate;
            book.IsFavorite = dto.IsFavorite;
            book.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps Book entity to BookDetailsViewModel
        /// </summary>
        public static BookDetailsViewModel ToDetailsViewModel(this Book book, bool canReview = false, int? userId = null)
        {
            if (book == null) return null;

            return new BookDetailsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                Price = book.Price.Amount,
                StockQuantity = book.StockQuantity,
                ISBN = book.ISBN?.Value,
                CategoryName = book.Category?.Name,
                PublicationDate = book.PublicationDate,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                IsDeleted = book.IsDeleted,
                IsFavorite = false, // This should be set by the calling service based on userId
                Rating = new BookRatingViewModel
                {
                    AverageRating = book.AverageRating,
                    TotalReviews = book.TotalReviews,
                    HasUserReview = false // This should be set by the calling service
                },
                CanReview = canReview,
                ReviewForm = new ReviewSubmissionViewModel
                {
                    BookId = book.Id
                }
            };
        }

        /// <summary>
        /// Maps collection of Books to BookListViewModel
        /// </summary>
        public static BookListViewModel ToListViewModel(this IEnumerable<Book> books,
            int currentPage, int totalPages, int totalBooks,
            string searchTerm = null, int? categoryId = null, string sortBy = null)
        {
            return new BookListViewModel
            {
                Books = books?.ToList() ?? new List<Book>(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                SortBy = sortBy
            };
        }

        /// <summary>
        /// Maps Book entity to BookFormViewModel for editing
        /// </summary>
        public static BookFormViewModel ToFormViewModel(this Book book, IEnumerable<SelectListItem> categories = null)
        {
            return new BookFormViewModel
            {
                Book = book,
                Categories = categories?.ToList() ?? new List<SelectListItem>()
            };
        }

        /// <summary>
        /// Creates a new BookFormViewModel for creating books
        /// </summary>
        public static BookFormViewModel CreateFormViewModel(IEnumerable<SelectListItem> categories = null)
        {
            return new BookFormViewModel
            {
                Categories = categories?.ToList() ?? new List<SelectListItem>()
            };
        }

        /// <summary>
        /// Maps grouped book data to CategoryBookCountViewModel
        /// </summary>
        public static CategoryBookCountViewModel ToCategoryBookCountViewModel(this IGrouping<string, Book> group)
        {
            return new CategoryBookCountViewModel
            {
                CategoryName = group.Key,
                Count = group.Count()
            };
        }

        /// <summary>
        /// Maps grouped book data to AuthorBookCountViewModel
        /// </summary>
        public static AuthorBookCountViewModel ToAuthorBookCountViewModel(this IGrouping<string, Book> group)
        {
            return new AuthorBookCountViewModel
            {
                AuthorName = group.Key,
                Count = group.Count()
            };
        }

        /// <summary>
        /// Maps Book entity to BookDetailsViewModel for the unified service
        /// </summary>
        public static BookDetailsViewModel ToDetailsViewModel(this Book book)
        {
            if (book == null) return null;

            return new BookDetailsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                Price = book.Price?.Amount ?? 0,
                StockQuantity = book.StockQuantity,
                ISBN = book.ISBN,
                CategoryName = book.Category?.Name,
                PublicationDate = book.PublicationDate,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                IsDeleted = book.IsDeleted,
                Rating = new BookRatingViewModel
                {
                    AverageRating = book.AverageRating,
                    TotalReviews = book.TotalReviews
                }
            };
        }

        /// <summary>
        /// Maps Book entity to BookListViewModel for the unified service
        /// </summary>
        public static BookDto ToListViewModel(this Book book)
        {
            return book.ToDto();
        }

        /// <summary>
        /// Maps CreateBookDto to Book entity
        /// </summary>
        public static Book ToEntity(this CreateBookDto dto)
        {
            if (dto == null) return null;

            return new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Price = new Money(dto.Price),
                StockQuantity = dto.StockQuantity,
                ISBN = dto.ISBN,
                CategoryId = dto.CategoryId,
                PublicationDate = dto.PublicationDate,
                LowStockThreshold = dto.LowStockThreshold
            };
        }

        /// <summary>
        /// Updates Book entity from CreateBookDto
        /// </summary>
        public static void UpdateEntity(this CreateBookDto dto, Book book)
        {
            if (dto == null || book == null) return;

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Description = dto.Description;
            book.ImageUrl = dto.ImageUrl;
            book.Price = new Money(dto.Price);
            book.StockQuantity = dto.StockQuantity;
            book.ISBN = dto.ISBN;
            book.CategoryId = dto.CategoryId;
            book.PublicationDate = dto.PublicationDate;
            book.LowStockThreshold = dto.LowStockThreshold;
        }
    }
}