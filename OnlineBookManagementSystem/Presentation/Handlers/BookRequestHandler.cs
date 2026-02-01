using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Presentation.Mappers;
using OnlineBookManagementSystem.Presentation.Models;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;

namespace OnlineBookManagementSystem.Presentation.Handlers
{
    /// <summary>
    /// Handles book-related requests by orchestrating services and mapping to ViewModels.
    /// Keeps controllers thin by extracting business orchestration logic.
    /// </summary>
    public class BookRequestHandler
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly ICategoryInterface _categoryService;
        private readonly ILogger<BookRequestHandler> _logger;

        public BookRequestHandler(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            ICategoryInterface categoryService,
            ILogger<BookRequestHandler> logger)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Handles book list request with filters
        /// </summary>
        public async Task<BookListViewModel> HandleBooksListRequest(BookFilterOptions filters)
        {
            var bookListViewModel = await _bookQueryService.GetPaginatedBooksAsync(
                filters.Page,
                filters.PageSize,
                filters.Search,
                filters.CategoryId,
                filters.SortBy,
                inStock: filters.InStock
            );

            return bookListViewModel;
        }

        /// <summary>
        /// Handles create book form request
        /// </summary>
        public async Task<BookFormViewModel> HandleCreateBookRequest()
        {
            var viewModel = await _bookQueryService.GetCreateBookViewModelAsync();
            return viewModel;
        }

        /// <summary>
        /// Handles create book command
        /// </summary>
        public async Task<CommandResult> HandleCreateBookCommand(
            BookFormViewModel model,
            IFormFile? imageFile,
            int userId)
        {
            try
            {
                var success = await _bookCommandService.AddBookAsync(model.Book!, imageFile);
                
                return success
                    ? CommandResult.Success("Book created successfully!")
                    : CommandResult.Failure("Failed to create book. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleCreateBookCommand for user {UserId}", userId);
                return CommandResult.Failure("An error occurred while creating the book.");
            }
        }

        /// <summary>
        /// Handles edit book form request
        /// </summary>
        public async Task<BookFormViewModel?> HandleEditBookRequest(int bookId)
        {
            return await _bookQueryService.GetEditBookViewModelAsync(bookId);
        }

        /// <summary>
        /// Handles update book command
        /// </summary>
        public async Task<CommandResult> HandleUpdateBookCommand(
            BookFormViewModel model,
            IFormFile? imageFile,
            int userId)
        {
            try
            {
                var success = await _bookCommandService.UpdateBookAsync(model.Book!, imageFile);
                
                return success
                    ? CommandResult.Success("Book updated successfully!")
                    : CommandResult.Failure("Failed to update book. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleUpdateBookCommand for user {UserId}", userId);
                return CommandResult.Failure("An error occurred while updating the book.");
            }
        }

        /// <summary>
        /// Handles delete book command
        /// </summary>
        public async Task<CommandResult> HandleDeleteBookCommand(int bookId, int userId)
        {
            try
            {
                var success = await _bookCommandService.SoftDeleteBookAsync(bookId, userId);
                
                return success
                    ? CommandResult.Success("Book deleted successfully!")
                    : CommandResult.Failure("Book not found or already deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleDeleteBookCommand for book {BookId}", bookId);
                return CommandResult.Failure("An error occurred while deleting the book.");
            }
        }

        /// <summary>
        /// Handles book details request
        /// </summary>
        public async Task<BookDetailsViewModel?> HandleBookDetailsRequest(int bookId)
        {
            var book = await _bookQueryService.GetBookByIdAsync(bookId);
            return book?.ToDetailsViewModel();
        }
    }
}
