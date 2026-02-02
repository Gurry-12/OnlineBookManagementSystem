using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.User;

namespace OnlineBookManagementSystem.Presentation.Mappers
{
    /// <summary>
    /// Maps Book entities to Favorites ViewModels
    /// Prevents entity leakage to views
    /// </summary>
    public static class FavoritesViewModelMapper
    {
        public static FavoritesBooksViewModel MapToFavoritesBooksViewModel(
            IEnumerable<Book> books,
            Dictionary<int, DateTime>? favoriteDates = null)
        {
            var bookList = books.ToList();

            return new FavoritesBooksViewModel
            {
                FavoriteBooks = bookList.Select(book => MapToFavoriteBookItem(book, favoriteDates)).ToList(),
                TotalFavorites = bookList.Count
            };
        }

        private static FavoriteBookItemViewModel MapToFavoriteBookItem(
            Book book,
            Dictionary<int, DateTime>? favoriteDates)
        {
            return new FavoriteBookItemViewModel
            {
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price.Amount,
                ImageUrl = book.ImageUrl,
                CategoryName = book.Category?.Name,
                StockQuantity = book.StockQuantity,
                AddedToFavoritesDate = favoriteDates?.GetValueOrDefault(book.Id) ?? DateTime.Now
            };
        }
    }
}
