using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly BookManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly IActivityLogger _activityLogger;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            BookManagementContext context,
            IMemoryCache cache,
            IActivityLogger activityLogger,
            ILogger<ReviewService> logger)
        {
            _context = context;
            _cache = cache;
            _activityLogger = activityLogger;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> SubmitReviewAsync(int userId, int bookId, int rating, string reviewText)
        {
            try
            {
                // Validate book exists and is not deleted
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && !b.IsDeleted);
                if (book == null)
                {
                    return (false, "Book not found or has been removed.");
                }

                // Validate user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                // Check for existing review
                var existingReview = await _context.BookReviews
                    .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId && !r.IsDeleted);

                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.Rating = rating;
                    existingReview.ReviewText = reviewText;
                    existingReview.UpdatedAt = DateTime.UtcNow;
                    existingReview.Status = ReviewStatus.Pending; // Reset to pending for re-moderation

                    await _context.SaveChangesAsync();
                    await InvalidateRatingCacheAsync(bookId);



                    return (true, "Review updated successfully and is pending moderation.");
                }
                else
                {
                    // Create new review
                    var newReview = new BookReview
                    {
                        BookId = bookId,
                        UserId = userId,
                        Rating = rating,
                        ReviewText = reviewText,
                        Status = ReviewStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.BookReviews.Add(newReview);
                    await _context.SaveChangesAsync();
                    await InvalidateRatingCacheAsync(bookId);


                    return (true, "Review submitted successfully and is pending moderation.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review for user {UserId} and book {BookId}", userId, bookId);
                return (false, "An error occurred while submitting your review. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateReviewAsync(int reviewId, int userId, int rating, string reviewText)
        {
            try
            {
                var review = await _context.BookReviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

                if (review == null)
                {
                    return (false, "Review not found or you don't have permission to edit it.");
                }

                review.Rating = rating;
                review.ReviewText = reviewText;
                review.UpdatedAt = DateTime.UtcNow;
                review.Status = ReviewStatus.Pending; // Reset to pending for re-moderation

                await _context.SaveChangesAsync();
                await InvalidateRatingCacheAsync(review.BookId);


                return (true, "Review updated successfully and is pending moderation.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId} for user {UserId}", reviewId, userId);
                return (false, "An error occurred while updating your review. Please try again.");
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            try
            {
                var review = await _context.BookReviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

                if (review == null)
                {
                    return false;
                }

                review.IsDeleted = true;
                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await InvalidateRatingCacheAsync(review.BookId);


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} for user {UserId}", reviewId, userId);
                return false;
            }
        }

        public async Task<BookReview?> GetUserReviewForBookAsync(int userId, int bookId)
        {
            return await _context.BookReviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId && !r.IsDeleted);
        }
        public async Task<PaginatedResult<ReviewDisplayViewModel>> GetBookReviewsAsync(int bookId, int page, int pageSize, ReviewSortOrder sortOrder, int? ratingFilter)
        {
            var query = _context.BookReviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId && r.Status == ReviewStatus.Approved && !r.IsDeleted);

            // Apply rating filter
            if (ratingFilter.HasValue)
            {
                query = query.Where(r => r.Rating == ratingFilter.Value);
            }

            // Apply sorting
            query = sortOrder switch
            {
                ReviewSortOrder.NewestFirst => query.OrderByDescending(r => r.CreatedAt),
                ReviewSortOrder.OldestFirst => query.OrderBy(r => r.CreatedAt),
                ReviewSortOrder.HighestRating => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                ReviewSortOrder.LowestRating => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDisplayViewModel
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CanEdit = false, // Will be set by controller based on current user
                    IsEdited = r.UpdatedAt > r.CreatedAt.AddMinutes(1) // Allow 1 minute grace period
                })
                .ToListAsync();

            return new PaginatedResult<ReviewDisplayViewModel>
            {
                Items = reviews,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ReviewDisplayViewModel?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.BookReviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.Id == reviewId && !r.IsDeleted)
                .Select(r => new ReviewDisplayViewModel
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CanEdit = false,
                    IsEdited = r.UpdatedAt > r.CreatedAt.AddMinutes(1)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BookRatingViewModel> GetBookRatingAsync(int bookId)
        {
            var cacheKey = $"book_rating_{bookId}";

            if (_cache.TryGetValue(cacheKey, out BookRatingViewModel? cachedRating) && cachedRating != null)
            {
                return cachedRating;
            }

            var approvedReviews = await _context.BookReviews
                .Where(r => r.BookId == bookId && r.Status == ReviewStatus.Approved && !r.IsDeleted)
                .ToListAsync();

            var ratingViewModel = new BookRatingViewModel
            {
                BookId = bookId,
                TotalReviews = approvedReviews.Count
            };

            if (approvedReviews.Any())
            {
                ratingViewModel.AverageRating = approvedReviews.Average(r => r.Rating);
                
                // Set individual rating counts
                var ratingGroups = approvedReviews.GroupBy(r => r.Rating).ToDictionary(g => g.Key, g => g.Count());
                ratingViewModel.FiveStarCount = ratingGroups.GetValueOrDefault(5, 0);
                ratingViewModel.FourStarCount = ratingGroups.GetValueOrDefault(4, 0);
                ratingViewModel.ThreeStarCount = ratingGroups.GetValueOrDefault(3, 0);
                ratingViewModel.TwoStarCount = ratingGroups.GetValueOrDefault(2, 0);
                ratingViewModel.OneStarCount = ratingGroups.GetValueOrDefault(1, 0);
            }

            // Cache for 30 minutes
            _cache.Set(cacheKey, ratingViewModel, TimeSpan.FromMinutes(30));

            return ratingViewModel;
        }

        public async Task RecalculateBookRatingAsync(int bookId)
        {
            try
            {
                var approvedReviews = await _context.BookReviews
                    .Where(r => r.BookId == bookId && r.Status == ReviewStatus.Approved && !r.IsDeleted)
                    .ToListAsync();

                var existingCache = await _context.BookRatingCache
                    .FirstOrDefaultAsync(c => c.BookId == bookId);

                if (approvedReviews.Any())
                {
                    var averageRating = approvedReviews.Average(r => r.Rating);
                    var totalReviews = approvedReviews.Count;

                    if (existingCache != null)
                    {
                        existingCache.AverageRating = averageRating;
                        existingCache.TotalReviews = totalReviews;
                        existingCache.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.BookRatingCache.Add(new BookRatingCache
                        {
                            BookId = bookId,
                            AverageRating = averageRating,
                            TotalReviews = totalReviews,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }
                else if (existingCache != null)
                {
                    // No approved reviews, remove cache entry
                    _context.BookRatingCache.Remove(existingCache);
                }

                await _context.SaveChangesAsync();
                await InvalidateRatingCacheAsync(bookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating rating for book {BookId}", bookId);
            }
        }

        public async Task InvalidateRatingCacheAsync(int bookId)
        {
            var cacheKey = $"book_rating_{bookId}";
            _cache.Remove(cacheKey);

            // Also invalidate related cache keys
            _cache.Remove($"book_details_{bookId}");
            _cache.Remove("top_rated_books");
            _cache.Remove("lowest_rated_books");
        }
        public async Task<PaginatedResult<ReviewModerationViewModel>> GetPendingReviewsAsync(int page, int pageSize)
        {
            var query = _context.BookReviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.Status == ReviewStatus.Pending && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewModerationViewModel
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book.Title,
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    RejectionReason = r.RejectionReason
                })
                .ToListAsync();

            return new PaginatedResult<ReviewModerationViewModel>
            {
                Items = reviews,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ApproveReviewAsync(int reviewId, int moderatorId)
        {
            try
            {
                var review = await _context.BookReviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

                if (review == null || review.Status != ReviewStatus.Pending)
                {
                    return false;
                }

                review.Status = ReviewStatus.Approved;
                review.ModeratedBy = moderatorId;
                review.ModeratedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await RecalculateBookRatingAsync(review.BookId);


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review {ReviewId} by moderator {ModeratorId}", reviewId, moderatorId);
                return false;
            }
        }

        public async Task<bool> RejectReviewAsync(int reviewId, int moderatorId, string reason)
        {
            try
            {
                var review = await _context.BookReviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

                if (review == null || review.Status != ReviewStatus.Pending)
                {
                    return false;
                }

                review.Status = ReviewStatus.Rejected;
                review.ModeratedBy = moderatorId;
                review.ModeratedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;
                review.RejectionReason = reason;

                await _context.SaveChangesAsync();


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review {ReviewId} by moderator {ModeratorId}", reviewId, moderatorId);
                return false;
            }
        }

        public async Task<bool> FlagReviewAsync(int reviewId, int moderatorId, string reason)
        {
            try
            {
                var review = await _context.BookReviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

                if (review == null)
                {
                    return false;
                }

                review.Status = ReviewStatus.Flagged;
                review.ModeratedBy = moderatorId;
                review.ModeratedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;
                review.RejectionReason = reason;

                await _context.SaveChangesAsync();


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging review {ReviewId} by moderator {ModeratorId}", reviewId, moderatorId);
                return false;
            }
        }

        public async Task<ReviewAnalyticsViewModel> GetReviewAnalyticsAsync()
        {
            var cacheKey = "review_analytics";

            if (_cache.TryGetValue(cacheKey, out ReviewAnalyticsViewModel? cachedAnalytics) && cachedAnalytics != null)
            {
                return cachedAnalytics;
            }

            var allReviews = await _context.BookReviews
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            var approvedReviews = allReviews.Where(r => r.Status == ReviewStatus.Approved).ToList();

            var analytics = new ReviewAnalyticsViewModel
            {
                TotalReviews = allReviews.Count,
                PendingReviews = allReviews.Count(r => r.Status == ReviewStatus.Pending),
                ApprovedReviews = allReviews.Count(r => r.Status == ReviewStatus.Approved),
                RejectedReviews = allReviews.Count(r => r.Status == ReviewStatus.Rejected),
                FlaggedReviews = allReviews.Count(r => r.Status == ReviewStatus.Flagged),
                AverageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0,
                RatingDistribution = approvedReviews
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ReviewTrends = await GetReviewTrendsAsync()
            };

            // Cache for 15 minutes
            _cache.Set(cacheKey, analytics, TimeSpan.FromMinutes(15));

            return analytics;
        }

        public async Task<List<BookRatingStatsViewModel>> GetTopRatedBooksAsync(int count)
        {
            var cacheKey = $"top_rated_books_{count}";

            if (_cache.TryGetValue(cacheKey, out List<BookRatingStatsViewModel>? cachedBooks) && cachedBooks != null)
            {
                return cachedBooks;
            }

            var topBooks = await _context.BookRatingCache
                .Include(c => c.Book)
                .Where(c => c.TotalReviews >= 3) // Minimum 3 reviews for ranking
                .OrderByDescending(c => c.AverageRating)
                .ThenByDescending(c => c.TotalReviews)
                .Take(count)
                .Select(c => new BookRatingStatsViewModel
                {
                    BookId = c.BookId,
                    BookTitle = c.Book.Title,
                    Author = c.Book.Author,
                    AverageRating = c.AverageRating,
                    TotalReviews = c.TotalReviews
                })
                .ToListAsync();

            // Cache for 1 hour
            _cache.Set(cacheKey, topBooks, TimeSpan.FromHours(1));

            return topBooks;
        }

        public async Task<List<BookRatingStatsViewModel>> GetLowestRatedBooksAsync(int count)
        {
            var cacheKey = $"lowest_rated_books_{count}";

            if (_cache.TryGetValue(cacheKey, out List<BookRatingStatsViewModel>? cachedBooks) && cachedBooks != null)
            {
                return cachedBooks;
            }

            var lowestBooks = await _context.BookRatingCache
                .Include(c => c.Book)
                .Where(c => c.TotalReviews >= 3) // Minimum 3 reviews for ranking
                .OrderBy(c => c.AverageRating)
                .ThenByDescending(c => c.TotalReviews)
                .Take(count)
                .Select(c => new BookRatingStatsViewModel
                {
                    BookId = c.BookId,
                    BookTitle = c.Book.Title,
                    Author = c.Book.Author,
                    AverageRating = c.AverageRating,
                    TotalReviews = c.TotalReviews
                })
                .ToListAsync();

            // Cache for 1 hour
            _cache.Set(cacheKey, lowestBooks, TimeSpan.FromHours(1));

            return lowestBooks;
        }

        private async Task<List<ReviewTrendViewModel>> GetReviewTrendsAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var trends = await _context.BookReviews
                .Where(r => r.CreatedAt >= thirtyDaysAgo && r.Status == ReviewStatus.Approved && !r.IsDeleted)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new ReviewTrendViewModel
                {
                    Date = g.Key,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return trends;
        }
    }
}
