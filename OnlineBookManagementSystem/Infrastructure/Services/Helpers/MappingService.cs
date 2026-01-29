using OnlineBookManagementSystem.Core.Application.DTOs;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;
using OnlineBookManagementSystem.Presentation.ViewModels.Shared;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using OnlineBookManagementSystem.Core.Application.Interfaces.Helpers;

namespace OnlineBookManagementSystem.Infrastructure.Services.Helpers
{
    /// <summary>
    /// Central mapping service implementation using extension methods for consistent object mapping
    /// </summary>
    public class MappingService : IMappingService
    {
        public TDestination Map<TDestination>(object source)
        {
            if (source == null) return default(TDestination);

            return (TDestination)MapInternal(source, typeof(TDestination));
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) return default(TDestination);

            return (TDestination)MapInternal(source, typeof(TDestination));
        }

        public IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        {
            if (source == null) return Enumerable.Empty<TDestination>();

            return source.Select(item => Map<TSource, TDestination>(item));
        }

        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null || destination == null) return;

            MapInternal(source, destination);
        }

        private object MapInternal(object source, Type destinationType)
        {
            if (source == null) return null;

            var sourceType = source.GetType();

            // Book mappings
            if (sourceType == typeof(Book))
            {
                var book = (Book)source;
                if (destinationType == typeof(BookDto))
                    return book.ToDto();
                if (destinationType == typeof(BookDetailsViewModel))
                    return book.ToDetailsViewModel();
                if (destinationType == typeof(BookFormViewModel))
                    return book.ToFormViewModel();
            }

            // BookDto mappings
            if (sourceType == typeof(BookDto))
            {
                var dto = (BookDto)source;
                if (destinationType == typeof(Book))
                    return dto.ToEntity();
            }

            // User mappings
            if (sourceType == typeof(User))
            {
                var user = (User)source;
                if (destinationType == typeof(UserProfileViewModel))
                    return user.ToProfileViewModel();
                if (destinationType == typeof(UserViewModel))
                    return user.ToUserViewModel();
                if (destinationType == typeof(ProfileViewModel))
                    return user.ToEditProfileViewModel();
            }

            // Category mappings
            if (sourceType == typeof(Category))
            {
                var category = (Category)source;
                if (destinationType == typeof(CategoryDto))
                    return category.ToDto();
                if (destinationType == typeof(CategoryViewModel))
                    return category.ToViewModel();
            }

            // CategoryDto mappings
            if (sourceType == typeof(CategoryDto))
            {
                var dto = (CategoryDto)source;
                if (destinationType == typeof(Category))
                    return dto.ToEntity();
            }

            // Review mappings
            if (sourceType == typeof(BookReview))
            {
                var review = (BookReview)source;
                if (destinationType == typeof(ReviewDisplayViewModel))
                    return review.ToDisplayViewModel();
                if (destinationType == typeof(ReviewSubmissionViewModel))
                    return review.ToSubmissionViewModel();
            }

            // ReviewSubmissionViewModel mappings
            if (sourceType == typeof(ReviewSubmissionViewModel))
            {
                var model = (ReviewSubmissionViewModel)source;
                if (destinationType == typeof(BookReview))
                    throw new InvalidOperationException("Use ToEntity(userId) method for ReviewSubmissionViewModel to BookReview mapping");
            }

            throw new NotSupportedException($"Mapping from {sourceType.Name} to {destinationType.Name} is not supported");
        }

        private void MapInternal(object source, object destination)
        {
            if (source == null || destination == null) return;

            var sourceType = source.GetType();
            var destinationType = destination.GetType();

            // Book update mappings
            if (sourceType == typeof(BookDto) && destinationType == typeof(Book))
            {
                ((Book)destination).UpdateFromDto((BookDto)source);
                return;
            }

            // User update mappings
            if (sourceType == typeof(ProfileViewModel) && destinationType == typeof(User))
            {
                ((User)destination).UpdateFromProfileViewModel((ProfileViewModel)source);
                return;
            }

            // Category update mappings
            if (sourceType == typeof(CategoryDto) && destinationType == typeof(Category))
            {
                ((Category)destination).UpdateFromDto((CategoryDto)source);
                return;
            }

            // Review update mappings
            if (sourceType == typeof(ReviewSubmissionViewModel) && destinationType == typeof(BookReview))
            {
                ((BookReview)destination).UpdateFromViewModel((ReviewSubmissionViewModel)source);
                return;
            }

            throw new NotSupportedException($"Update mapping from {sourceType.Name} to {destinationType.Name} is not supported");
        }
    }
}