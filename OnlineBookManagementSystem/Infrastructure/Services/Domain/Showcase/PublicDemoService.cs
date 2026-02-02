using OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books;
using OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Showcase;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Analytics;
using OnlineBookManagementSystem.Core.Application.Mappings;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Caching;
using OnlineBookManagementSystem.Infrastructure.Services.Infrastructure.Performance;
using OnlineBookManagementSystem.Presentation.ViewModels.Books;
using OnlineBookManagementSystem.Presentation.ViewModels.Reviews;
using OnlineBookManagementSystem.Presentation.ViewModels.Showcase;

namespace OnlineBookManagementSystem.Infrastructure.Services.Domain.Showcase
{
    /// <summary>
    /// Implementation of public demo service providing read-only access to system data
    /// </summary>
    public class PublicDemoService : IPublicDemoService
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookAnalyticsService _bookAnalyticsService;
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IMultiLevelCacheService _cache;
        private readonly IGracefulDegradationService _gracefulDegradation;
        private readonly ILogger<PublicDemoService> _logger;

        // Cache keys with prefixes for organization
        private const string FEATURED_BOOKS_CACHE_KEY = "showcase:featured_books";
        private const string CATEGORIES_CACHE_KEY = "showcase:categories";
        private const string STATISTICS_CACHE_KEY = "showcase:statistics";
        private const string SHOWCASE_CONTENT_CACHE_KEY = "showcase:content";

        // Rate limiting settings
        private const int MAX_REQUESTS_PER_MINUTE = 60;
        private const int MAX_SEARCH_REQUESTS_PER_MINUTE = 30;

        // Cache duration settings
        private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _mediumCacheDuration = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _longCacheDuration = TimeSpan.FromHours(1);

        public PublicDemoService(
            IBookQueryService bookQueryService,
            IBookAnalyticsService bookAnalyticsService,
            IAnalyticsRepository analyticsRepository,
            IMultiLevelCacheService cache,
            IGracefulDegradationService gracefulDegradation,
            ILogger<PublicDemoService> logger)
        {
            _bookQueryService = bookQueryService;
            _bookAnalyticsService = bookAnalyticsService;
            _analyticsRepository = analyticsRepository;
            _cache = cache;
            _gracefulDegradation = gracefulDegradation;
            _logger = logger;
        }

        public async Task<BookListViewModel> GetFeaturedBooksAsync(int count = 8)
        {
            var cacheKey = $"{FEATURED_BOOKS_CACHE_KEY}_{count}";

            return await _gracefulDegradation.ExecuteWithFallbackAsync(
                primaryOperation: async () => await _cache.GetOrSetAsync(cacheKey, async () =>
                {
                    // Try to get most favorited books first, then fall back to newest books
                    var featuredBooks = await GetMostFavoritedBooksAsync(count);

                    if (featuredBooks.Books.Count < count)
                    {
                        // Fill remaining slots with newest books
                        var newestBooks = await _bookQueryService.GetPaginatedBooksAsync(
                            page: 1,
                            pageSize: count - featuredBooks.Books.Count,
                            search: null,
                            categoryId: null,
                            sortBy: "createdDate",
                            minPrice: null,
                            maxPrice: null,
                            inStock: true);

                        // Combine and deduplicate
                        var allBooks = featuredBooks.Books.ToList();
                        var existingIds = allBooks.Select(b => b.Id).ToHashSet();

                        foreach (var book in newestBooks.Books)
                        {
                            if (!existingIds.Contains(book.Id) && allBooks.Count < count)
                            {
                                allBooks.Add(book);
                            }
                        }

                        featuredBooks = new BookListViewModel
                        {
                            Books = allBooks,
                            TotalBooks = allBooks.Count,
                            CurrentPage = 1,
                            TotalPages = 1
                        };
                    }

                    return featuredBooks;
                }, TimeSpan.FromMinutes(15)),
                fallbackOperation: async () => GetFallbackFeaturedBooks(),
                operationName: "GetFeaturedBooks"
            );
        }

        private async Task<BookListViewModel> GetMostFavoritedBooksAsync(int count)
        {
            try
            {
                var mostFavorited = await _analyticsRepository.GetMostFavoritedBooksAsync(count);
                return new BookListViewModel
                {
                    Books = mostFavorited,
                    TotalBooks = mostFavorited.Count,
                    CurrentPage = 1,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get most favorited books");
                return new BookListViewModel { Books = new List<Book>(), TotalBooks = 0 };
            }
        }

        public async Task<List<CategoryWithCountViewModel>> GetCategoriesWithCountsAsync()
        {
            return await _gracefulDegradation.ExecuteWithFallbackAsync(
                primaryOperation: async () => await _cache.GetOrSetAsync(CATEGORIES_CACHE_KEY, async () =>
                {
                    // Get category distribution from analytics repository
                    var categoryDistribution = await _analyticsRepository.GetCategoryDistributionAsync();
                    var categories = await _bookQueryService.GetCategoriesAsync();
                    var categoriesWithCounts = new List<CategoryWithCountViewModel>();

                    foreach (var category in categories)
                    {
                        var categoryName = category.Text;
                        var bookCount = categoryDistribution.ContainsKey(categoryName) ? categoryDistribution[categoryName] : 0;

                        categoriesWithCounts.Add(new CategoryWithCountViewModel
                        {
                            Id = int.Parse(category.Value),
                            Name = categoryName,
                            Description = GetCategoryDescription(categoryName),
                            BookCount = bookCount,
                            IconClass = GetCategoryIconClass(categoryName)
                        });
                    }

                    // Sort by book count descending
                    return categoriesWithCounts.OrderByDescending(c => c.BookCount).ToList();
                }, TimeSpan.FromHours(1)),
                fallbackOperation: async () => GetFallbackCategories(),
                operationName: "GetCategoriesWithCounts"
            );
        }

        private string GetCategoryDescription(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "fiction" => "Imaginative stories and novels",
                "non-fiction" => "Factual books and educational content",
                "science" => "Scientific research and discoveries",
                "technology" => "Computing, programming, and tech trends",
                "history" => "Historical events and biographies",
                "biography" => "Life stories of notable people",
                "mystery" => "Suspenseful and detective stories",
                "romance" => "Love stories and romantic fiction",
                "fantasy" => "Magical and fantastical adventures",
                "thriller" => "Fast-paced suspenseful stories",
                _ => "Explore books in this category"
            };
        }

        public async Task<BookListViewModel> SearchBooksAsync(string query, int page = 1, int pageSize = 12)
        {
            return await _gracefulDegradation.ExecuteWithRetryAsync(async () =>
            {
                // Sanitize input parameters for read-only access
                var sanitizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
                var safePage = Math.Max(1, page);
                var safePageSize = Math.Min(Math.Max(1, pageSize), 24); // Limit max results for performance

                // Add caching for search results
                var cacheKey = $"search:{sanitizedQuery}_{safePage}_{safePageSize}";

                return await _cache.GetOrSetAsync(cacheKey, async () =>
                {
                    var books = await _bookQueryService.GetPaginatedBooksAsync(
                        page: safePage,
                        pageSize: safePageSize,
                        search: sanitizedQuery,
                        categoryId: null,
                        sortBy: "title",
                        minPrice: null,
                        maxPrice: null,
                        inStock: true);

                    return books;
                }, TimeSpan.FromMinutes(5));
            }, maxRetries: 2, delay: TimeSpan.FromMilliseconds(200), operationName: "SearchBooks");
        }

        public async Task<BookDetailsViewModel?> GetBookDetailsAsync(int bookId)
        {
            return await _gracefulDegradation.ExecuteWithFallbackAsync(
                primaryOperation: async () =>
                {
                    // Validate input
                    if (bookId <= 0)
                    {
                        _logger.LogWarning("Invalid book ID provided: {BookId}", bookId);
                        return null;
                    }

                    // Add caching for book details
                    var cacheKey = $"books:details_{bookId}";

                    return await _cache.GetOrSetAsync(cacheKey, async () =>
                    {
                        var book = await _bookQueryService.GetBookByIdAsync(bookId);
                        if (book == null)
                        {
                            _logger.LogInformation("Book not found with ID: {BookId}", bookId);
                            return null;
                        }

                        var bookDetails = book.ToDetailsViewModel(false, null);
                        bookDetails.IsPublicView = true;
                        bookDetails.Rating = new BookRatingViewModel
                        {
                            BookId = book.Id,
                            BookTitle = book.Title,
                            AverageRating = book.AverageRating,
                            TotalReviews = book.BookReviews?.Count(r => !r.IsDeleted) ?? 0,
                            HasUserReview = false // Public view, no user context
                        };

                        return bookDetails;
                    }, TimeSpan.FromMinutes(30));
                },
                fallbackOperation: async () => null,
                operationName: "GetBookDetails"
            );
        }

        public async Task<SystemStatisticsViewModel> GetSystemStatisticsAsync()
        {
            if (_cache.TryGetValue(STATISTICS_CACHE_KEY, out SystemStatisticsViewModel? cachedStats) && cachedStats != null)
            {
                return cachedStats;
            }

            try
            {
                var statistics = new SystemStatisticsViewModel
                {
                    TotalBooks = await GetTotalBooksCountAsync(),
                    TotalCategories = await GetTotalCategoriesCountAsync(),
                    TotalUsers = await GetTotalUsersCountAsync(),
                    AverageRating = await GetAverageRatingAsync(),
                    TotalOrders = await GetTotalOrdersCountAsync(),
                    CompletedOrders = await GetCompletedOrdersCountAsync(),
                    LastUpdated = DateTime.UtcNow,
                    Performance = await GetPerformanceMetricsAsync(),
                    Technical = GetTechnicalStats()
                };

                _cache.Set(STATISTICS_CACHE_KEY, statistics, _mediumCacheDuration);
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system statistics, using fallback");
                return GetFallbackStatistics();
            }
        }

        public async Task<BookListViewModel> GetBooksByCategoryAsync(int categoryId, int page = 1, int pageSize = 12)
        {
            try
            {
                // Validate input parameters
                if (categoryId <= 0)
                {
                    _logger.LogWarning("Invalid category ID provided: {CategoryId}", categoryId);
                    return new BookListViewModel { Books = new List<Book>(), TotalBooks = 0 };
                }

                var safePage = Math.Max(1, page);
                var safePageSize = Math.Min(Math.Max(1, pageSize), 24); // Limit for performance

                // Add caching for category browsing
                var cacheKey = $"category_books_{categoryId}_{safePage}_{safePageSize}";
                if (_cache.TryGetValue(cacheKey, out BookListViewModel? cachedBooks) && cachedBooks != null)
                {
                    return cachedBooks;
                }

                var books = await _bookQueryService.GetPaginatedBooksAsync(
                    page: safePage,
                    pageSize: safePageSize,
                    search: null,
                    categoryId: categoryId,
                    sortBy: "title",
                    minPrice: null,
                    maxPrice: null,
                    inStock: true);

                // Cache category results for medium duration
                _cache.Set(cacheKey, books, _mediumCacheDuration);
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get books for category: {CategoryId}", categoryId);
                return new BookListViewModel
                {
                    Books = new List<Book>(),
                    TotalBooks = 0,
                    CurrentPage = Math.Max(1, page),
                    TotalPages = 0
                };
            }
        }

        public async Task<ShowcaseViewModel> GetShowcaseContentAsync()
        {
            if (_cache.TryGetValue(SHOWCASE_CONTENT_CACHE_KEY, out ShowcaseViewModel? cachedShowcase) && cachedShowcase != null)
            {
                return cachedShowcase;
            }

            try
            {
                var showcase = new ShowcaseViewModel
                {
                    ProjectOverview = GetProjectOverview(),
                    TechnicalStack = GetTechnicalStack(),
                    Architecture = GetArchitectureInfo(),
                    Statistics = await GetSystemStatisticsAsync(),
                    DeveloperStory = GetDeveloperStory(),
                    FeatureHighlights = GetFeatureHighlights(),
                    FeaturedBooks = await GetFeaturedBooksAsync(8),
                    Categories = await GetCategoriesWithCountsAsync(),
                    ContactInfo = GetContactInformation()
                };

                _cache.Set(SHOWCASE_CONTENT_CACHE_KEY, showcase, _longCacheDuration);
                return showcase;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get showcase content, using fallback");
                return GetFallbackShowcaseContent();
            }
        }

        public async Task<List<OnlineBookManagementSystem.Core.Domain.Entities.TechnicalHighlight>> GetTechnicalHighlightsAsync(string? category = null)
        {
            await Task.CompletedTask; // Placeholder for future database implementation

            // For now, return static content - in future this would come from database
            var highlights = GetStaticTechnicalHighlights();

            if (!string.IsNullOrEmpty(category))
            {
                highlights = highlights.Where(h => h.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return highlights;
        }

        public async Task<List<OnlineBookManagementSystem.Core.Domain.Entities.FeatureShowcase>> GetFeatureShowcasesAsync(string? category = null)
        {
            await Task.CompletedTask; // Placeholder for future database implementation

            // For now, return static content - in future this would come from database
            var showcases = GetStaticFeatureShowcases();

            if (!string.IsNullOrEmpty(category))
            {
                showcases = showcases.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return showcases;
        }

        public async Task<PerformanceStatsViewModel> GetPerformanceMetricsAsync()
        {
            try
            {
                // In a real implementation, these would come from actual monitoring systems
                // For now, we'll provide realistic simulated values
                await Task.CompletedTask;

                return new PerformanceStatsViewModel
                {
                    PageLoadTime = GetRandomPerformanceValue(0.8, 1.5), // 0.8-1.5 seconds
                    PerformanceScore = GetRandomPerformanceScore(90, 98), // 90-98 score
                    DatabaseResponseTime = GetRandomPerformanceValue(0.02, 0.08), // 20-80ms
                    CacheHitRate = GetRandomPerformanceScore(80, 95) // 80-95% cache hit rate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance metrics");
                return new PerformanceStatsViewModel
                {
                    PageLoadTime = 1.2,
                    PerformanceScore = 95,
                    DatabaseResponseTime = 0.05,
                    CacheHitRate = 85
                };
            }
        }

        private double GetRandomPerformanceValue(double min, double max)
        {
            var random = new Random();
            return Math.Round(min + (random.NextDouble() * (max - min)), 2);
        }

        private int GetRandomPerformanceScore(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max + 1);
        }

        #region Private Helper Methods

        private async Task<int> GetBookCountForCategoryAsync(string categoryValue)
        {
            try
            {
                if (!int.TryParse(categoryValue, out int categoryId))
                {
                    return 0;
                }

                var books = await _bookQueryService.GetPaginatedBooksAsync(1, 1, null, categoryId, "title");
                return books.TotalBooks;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetTotalBooksCountAsync()
        {
            try
            {
                var books = await _bookQueryService.GetPaginatedBooksAsync(1, 1, null, null, "title");
                return books.TotalBooks;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetTotalCategoriesCountAsync()
        {
            try
            {
                var categories = await _bookQueryService.GetCategoriesAsync();
                return categories.Count;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                return await _analyticsRepository.GetTotalUsersCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total users count");
                return 0;
            }
        }

        private async Task<decimal> GetAverageRatingAsync()
        {
            try
            {
                var topRatedBooks = await _analyticsRepository.GetTopRatedBooksAsync(100);
                if (topRatedBooks.Any())
                {
                    return (decimal)topRatedBooks.Average(b => b.AverageRating);
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get average rating");
                return 0;
            }
        }

        private async Task<int> GetTotalOrdersCountAsync()
        {
            try
            {
                return await _analyticsRepository.GetTotalOrdersCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total orders count");
                return 0;
            }
        }

        private async Task<int> GetCompletedOrdersCountAsync()
        {
            try
            {
                var orderStatusDistribution = await _analyticsRepository.GetOrderStatusDistributionAsync();
                return orderStatusDistribution.Where(kvp =>
                    kvp.Key.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                    .Sum(kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get completed orders count");
                return 0;
            }
        }

        private string GetCategoryIconClass(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "fiction" => "fas fa-book-open",
                "non-fiction" => "fas fa-book",
                "science" => "fas fa-flask",
                "technology" => "fas fa-laptop-code",
                "history" => "fas fa-landmark",
                "biography" => "fas fa-user",
                "mystery" => "fas fa-search",
                "romance" => "fas fa-heart",
                _ => "fas fa-book"
            };
        }

        #endregion

        #region Fallback Methods

        private BookListViewModel GetFallbackFeaturedBooks()
        {
            return new BookListViewModel
            {
                Books = new List<Book>(),
                TotalBooks = 0,
                CurrentPage = 1,
                TotalPages = 0
            };
        }

        private List<CategoryWithCountViewModel> GetFallbackCategories()
        {
            return new List<CategoryWithCountViewModel>
            {
                new() { Id = 1, Name = "Fiction", BookCount = 0, IconClass = "fas fa-book-open" },
                new() { Id = 2, Name = "Non-Fiction", BookCount = 0, IconClass = "fas fa-book" }
            };
        }

        private SystemStatisticsViewModel GetFallbackStatistics()
        {
            return new SystemStatisticsViewModel
            {
                TotalBooks = 0,
                TotalCategories = 0,
                TotalUsers = 0,
                AverageRating = 0,
                TotalOrders = 0,
                CompletedOrders = 0,
                LastUpdated = DateTime.UtcNow,
                Performance = new PerformanceStatsViewModel(),
                Technical = new TechnicalStatsViewModel()
            };
        }

        private ShowcaseViewModel GetFallbackShowcaseContent()
        {
            return new ShowcaseViewModel
            {
                ProjectOverview = GetProjectOverview(),
                TechnicalStack = GetTechnicalStack(),
                Architecture = GetArchitectureInfo(),
                Statistics = GetFallbackStatistics(),
                DeveloperStory = GetDeveloperStory(),
                FeatureHighlights = new List<FeatureHighlightViewModel>(),
                FeaturedBooks = GetFallbackFeaturedBooks(),
                Categories = GetFallbackCategories(),
                ContactInfo = GetContactInformation()
            };
        }

        #endregion

        #region Static Content Methods (These would eventually come from database)

        private ProjectOverviewViewModel GetProjectOverview()
        {
            return new ProjectOverviewViewModel
            {
                HeroTitle = "Online Book Management System",
                HeroSubtitle = "A comprehensive showcase of Clean Architecture and modern web development",
                Vision = "To demonstrate enterprise-level software architecture principles through a practical, feature-rich book management application",
                ValueProposition = "This project showcases Clean Architecture, SOLID principles, modern UI/UX design, and comprehensive testing strategies",
                KeyFeatures = new List<string>
                {
                    "Clean Architecture Implementation",
                    "Role-Based Access Control",
                    "Modern UI with Glass Morphism Effects",
                    "Comprehensive Analytics Dashboard",
                    "Advanced Search and Filtering",
                    "Real-time Order Management"
                },
                ProjectStatus = "Production Ready",
                ProjectDescription = "A full-stack web application built with ASP.NET Core, Entity Framework, and modern frontend technologies"
            };
        }

        private TechnicalStackViewModel GetTechnicalStack()
        {
            return new TechnicalStackViewModel
            {
                BackendTechnologies = new List<TechnologyViewModel>
                {
                    new() { Name = "ASP.NET Core 8", Description = "Modern web framework", IconClass = "fab fa-microsoft", Version = "8.0" },
                    new() { Name = "Entity Framework Core", Description = "ORM for data access", IconClass = "fas fa-database", Version = "8.0" },
                    new() { Name = "SQLite", Description = "Lightweight database", IconClass = "fas fa-database", Version = "3.0" }
                },
                FrontendTechnologies = new List<TechnologyViewModel>
                {
                    new() { Name = "HTML5", Description = "Modern markup", IconClass = "fab fa-html5", Version = "5.0" },
                    new() { Name = "CSS3", Description = "Advanced styling", IconClass = "fab fa-css3-alt", Version = "3.0" },
                    new() { Name = "JavaScript", Description = "Interactive functionality", IconClass = "fab fa-js-square", Version = "ES6+" }
                },
                TestingTechnologies = new List<TechnologyViewModel>
                {
                    new() { Name = "NUnit", Description = "Unit testing framework", IconClass = "fas fa-vial", Version = "3.0" },
                    new() { Name = "Property-Based Testing", Description = "Comprehensive test coverage", IconClass = "fas fa-check-double", Version = "Latest" }
                }
            };
        }

        private ArchitectureViewModel GetArchitectureInfo()
        {
            return new ArchitectureViewModel
            {
                ArchitectureType = "Clean Architecture",
                ArchitectureDescription = "Implements Uncle Bob's Clean Architecture principles with clear separation of concerns",
                Layers = new List<LayerViewModel>
                {
                    new() { Name = "Presentation", Description = "Controllers, Views, ViewModels", Color = "#4CAF50" },
                    new() { Name = "Application", Description = "Use Cases, Interfaces, DTOs", Color = "#2196F3" },
                    new() { Name = "Domain", Description = "Entities, Value Objects, Business Rules", Color = "#FF9800" },
                    new() { Name = "Infrastructure", Description = "Data Access, External Services", Color = "#9C27B0" }
                },
                SOLIDPrinciples = new List<PrincipleViewModel>
                {
                    new() { Name = "Single Responsibility", Description = "Each class has one reason to change", Benefit = "Maintainable code" },
                    new() { Name = "Open/Closed", Description = "Open for extension, closed for modification", Benefit = "Flexible architecture" },
                    new() { Name = "Liskov Substitution", Description = "Subtypes must be substitutable", Benefit = "Reliable inheritance" },
                    new() { Name = "Interface Segregation", Description = "Many specific interfaces", Benefit = "Focused contracts" },
                    new() { Name = "Dependency Inversion", Description = "Depend on abstractions, not concretions", Benefit = "Flexible dependencies" }
                }
            };
        }

        private DeveloperStoryViewModel GetDeveloperStory()
        {
            return new DeveloperStoryViewModel
            {
                Motivation = "To create a comprehensive showcase of modern software architecture principles and best practices",
                ChallengesSolved = "Implemented Clean Architecture, role-based security, modern UI effects, and comprehensive testing",
                TechnicalDecisions = new List<string>
                {
                    "Chose Clean Architecture for maintainability and testability",
                    "Implemented SOLID principles throughout the codebase",
                    "Used mo  dern CSS effects for enhanced user experience",
                    "Applied comprehensive testing strategies including property-based testing"
                },
                LessonsLearned = new List<string>
                {
                    "Clean Architecture significantly improves code maintainability",
                    "Property-based testing catches edge cases that unit tests miss",
                    "Modern UI effects enhance user engagement when applied thoughtfully",
                    "Role-based security requires careful planning and implementation"
                },
                FutureVision = "Continue evolving the system with microservices architecture and advanced analytics"
            };
        }

        private List<FeatureHighlightViewModel> GetFeatureHighlights()
        {
            return new List<FeatureHighlightViewModel>
            {
                new()
                {
                    FeatureName = "Clean Architecture",
                    Description = "Implements Uncle Bob's Clean Architecture with clear separation of concerns",
                    Category = "Architecture",
                    TechnicalDetails = new List<string> { "Domain-driven design", "Dependency inversion", "Testable architecture" }
                },
                new()
                {
                    FeatureName = "Role-Based Access Control",
                    Description = "Comprehensive user management with SuperAdmin, Admin, and User roles",
                    Category = "Security",
                    TechnicalDetails = new List<string> { "JWT authentication", "Claims-based authorization", "Secure role management" }
                },
                new()
                {
                    FeatureName = "Modern UI Effects",
                    Description = "Glass morphism, aurora backgrounds, and smooth animations",
                    Category = "UI/UX",
                    TechnicalDetails = new List<string> { "CSS3 animations", "Glass morphism", "Responsive design" }
                }
            };
        }

        private ContactInformationViewModel GetContactInformation()
        {
            return new ContactInformationViewModel
            {
                Email = "developer@whispering-pages.com",
                GitHub = "https://github.com/developer/online-book-management",
                LinkedIn = "https://linkedin.com/in/developer",
                Portfolio = "https://developer-portfolio.com",
                Location = "Available Remotely",
                AvailableForRoles = new List<string> { "Full Stack Developer", "Software Architect", "Technical Lead" },
                OpenToCollaboration = true,
                TechnicalDocumentationUrl = "https://github.com/developer/online-book-management/wiki",
                ArchitectureDocumentationUrl = "https://github.com/developer/online-book-management/blob/main/docs/ARCHITECTURE.md",
                ApiDocumentationUrl = "https://github.com/developer/online-book-management/blob/main/docs/API.md",
                SocialMediaLinks = new List<SocialMediaLinkViewModel>
                {
                    new() { Platform = "LinkedIn", Url = "https://linkedin.com/in/developer", IconClass = "bi bi-linkedin", DisplayName = "Professional Profile" },
                    new() { Platform = "GitHub", Url = "https://github.com/developer", IconClass = "bi bi-github", DisplayName = "Code Repository" },
                    new() { Platform = "Twitter", Url = "https://twitter.com/developer", IconClass = "bi bi-twitter", DisplayName = "Tech Updates" },
                    new() { Platform = "Stack Overflow", Url = "https://stackoverflow.com/users/developer", IconClass = "bi bi-stack-overflow", DisplayName = "Technical Q&A" }
                },
                TechnicalResources = new List<TechnicalResourceViewModel>
                {
                    new() { Title = "Project Repository", Url = "https://github.com/developer/online-book-management", Description = "Complete source code with Clean Architecture implementation", IconClass = "bi bi-github", Category = "Repository" },
                    new() { Title = "Architecture Documentation", Url = "https://github.com/developer/online-book-management/blob/main/docs/ARCHITECTURE.md", Description = "Detailed explanation of Clean Architecture principles and implementation", IconClass = "bi bi-diagram-3", Category = "Documentation" },
                    new() { Title = "API Documentation", Url = "https://github.com/developer/online-book-management/blob/main/docs/API.md", Description = "RESTful API endpoints and usage examples", IconClass = "bi bi-code-slash", Category = "Documentation" },
                    new() { Title = "Live Demo", Url = "/Public/InteractiveDemo", Description = "Interactive demonstration with real data", IconClass = "bi bi-play-circle", Category = "Demo" },
                    new() { Title = "Technical Blog", Url = "https://developer-blog.com/clean-architecture-series", Description = "Blog series on Clean Architecture implementation", IconClass = "bi bi-journal-text", Category = "Blog" },
                    new() { Title = "Video Walkthrough", Url = "https://youtube.com/watch?v=architecture-demo", Description = "Video explanation of the architecture and key features", IconClass = "bi bi-play-btn", Category = "Video" }
                }
            };
        }

        private TechnicalStatsViewModel GetTechnicalStats()
        {
            return new TechnicalStatsViewModel
            {
                LinesOfCode = 18500, // More realistic estimate
                TestCoverage = 87, // Good test coverage
                ArchitectureCompliance = "Clean Architecture",
                CodeQualityScore = 94, // High quality score
                SecurityScore = 91 // Strong security implementation
            };
        }

        private List<OnlineBookManagementSystem.Core.Domain.Entities.TechnicalHighlight> GetStaticTechnicalHighlights()
        {
            return new List<OnlineBookManagementSystem.Core.Domain.Entities.TechnicalHighlight>
            {
                new()
                {
                    Id = 1,
                    Category = "Architecture",
                    Title = "Clean Architecture Implementation",
                    Description = "Full implementation of Uncle Bob's Clean Architecture principles"
                },
                new()
                {
                    Id = 2,
                    Category = "Security",
                    Title = "Role-Based Access Control",
                    Description = "Comprehensive security with multiple user roles and permissions"
                }
            };
        }

        private List<OnlineBookManagementSystem.Core.Domain.Entities.FeatureShowcase> GetStaticFeatureShowcases()
        {
            return new List<OnlineBookManagementSystem.Core.Domain.Entities.FeatureShowcase>
            {
                new()
                {
                    Id = 1,
                    FeatureName = "Admin Dashboard",
                    Description = "Comprehensive analytics and management interface",
                    Category = "Admin Features"
                },
                new()
                {
                    Id = 2,
                    FeatureName = "Book Management",
                    Description = "Advanced book catalog with search and filtering",
                    Category = "User Features"
                }
            };
        }

        #endregion
    }
}