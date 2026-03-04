using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Infrastructure.Data.Context;

namespace OnlineBookManagementSystem.Shared.Extensions;

public static class DatabaseSeedingExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();

        try
        {
            logger.LogInformation("Starting database seeding process...");

            var context = services.GetRequiredService<BookManagementContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            // Ensure database is created using migrations
            await context.Database.MigrateAsync();

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed users
            await SeedUsersAsync(userManager, configuration, logger);

            // Seed categories
            await SeedCategoriesAsync(context, logger);

            // Seed sample books only in development
            if (environment.IsDevelopment())
            {
                await SeedSampleBooksAsync(context, logger);
                await SeedSystemSettingsAsync(context, logger);
                await SeedSampleOrdersAsync(context, userManager, logger);
                await SeedSampleReviewsAsync(context, userManager, logger);
                await SeedSampleFavoritesAsync(context, userManager, logger);
            }

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager, ILogger logger)
    {
        var roles = new[] { "SuperAdmin", "Admin", "User", "Guest", "Public" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<int> { Name = roleName };
                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager, IConfiguration configuration, ILogger logger)
    {
        // Seed SuperAdmin with multiple roles
        var superAdminConfig = configuration.GetSection("DefaultUsers:SuperAdmin");
        await CreateSuperAdminWithMultipleRolesAsync(userManager, logger,
            superAdminConfig["Email"]!,
            superAdminConfig["Password"]!,
            "Super Administrator");

        // Seed Admin
        var adminConfig = configuration.GetSection("DefaultUsers:Admin");
        await CreateUserIfNotExistsAsync(userManager, logger,
            adminConfig["Email"]!,
            adminConfig["Password"]!,
            "Admin",
            "Administrator");

        // Seed User
        var userConfig = configuration.GetSection("DefaultUsers:User");
        await CreateUserIfNotExistsAsync(userManager, logger,
            userConfig["Email"]!,
            userConfig["Password"]!,
            "User",
            "Regular User");

        // Seed Public User
        var publicConfig = configuration.GetSection("DefaultUsers:Public");
        await CreateUserIfNotExistsAsync(userManager, logger,
            publicConfig["Email"] ?? "public@whisperingpages.com",
            publicConfig["Password"] ?? "Public123!",
            "Public",
            "Public User");

        // --- Seed Extra Sample Users For Testing ---
        string[] sampleNames = { "Emma Watson", "John Doe", "Alice Smith", "Robert Johnson", "Michael Brown" };
        for (int i = 0; i < sampleNames.Length; i++)
        {
            await CreateUserIfNotExistsAsync(userManager, logger,
                $"user{i + 1}@example.com",
                "Testing123!",
                "User",
                sampleNames[i]);
        }
        
        // Seed some pending users to show in the approval queue
        string[] pendingNames = { "David Waiting", "Sarah Pending", "Mike Unapproved" };
        for (int i = 0; i < pendingNames.Length; i++)
        {
            var pendingEmail = $"pending{i + 1}@example.com";
            if (await userManager.FindByEmailAsync(pendingEmail) == null)
            {
                var user = new User
                {
                    UserName = pendingEmail,
                    Email = pendingEmail,
                    Name = pendingNames[i],
                    EmailConfirmed = true,
                    IsPendingApproval = true,
                    RequestDate = DateTime.UtcNow.AddDays(-i - 1),
                    RequestedRole = i % 2 == 0 ? "Admin" : "User",
                    CreatedAt = DateTime.UtcNow.AddDays(-i - 1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-i - 1)
                };
                var res = await userManager.CreateAsync(user, "Testing123!");
                if (res.Succeeded)
                {
                    logger.LogInformation("Created pending user: {Email}", pendingEmail);
                }
            }
        }
    }

    private static async Task CreateSuperAdminWithMultipleRolesAsync(
        UserManager<User> userManager,
        ILogger logger,
        string email,
        string password,
        string name)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            logger.LogInformation("SuperAdmin user {Email} already exists", email);

            // Ensure SuperAdmin has all roles
            var allRoles = new[] { "SuperAdmin", "Admin", "User", "Public" };
            var currentRoles = await userManager.GetRolesAsync(existingUser);

            foreach (var role in allRoles)
            {
                if (!currentRoles.Contains(role))
                {
                    await userManager.AddToRoleAsync(existingUser, role);
                    logger.LogInformation("Added role {Role} to existing SuperAdmin {Email}", role, email);
                }
            }
            return;
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true,
            IsPendingApproval = false,
            RequestDate = DateTime.UtcNow,
            RequestedRole = "SuperAdmin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Add all roles to SuperAdmin
            var allRoles = new[] { "SuperAdmin", "Admin", "User", "Public" };
            foreach (var role in allRoles)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("Added role {Role} to SuperAdmin {Email}", role, email);
            }
            logger.LogInformation("Created SuperAdmin user: {Email} with multiple roles", email);
        }
        else
        {
            logger.LogError("Failed to create SuperAdmin user {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task CreateUserIfNotExistsAsync(
        UserManager<User> userManager,
        ILogger logger,
        string email,
        string password,
        string role,
        string name)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            logger.LogInformation("User {Email} already exists", email);
            return;
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true,
            IsPendingApproval = false,
            RequestDate = DateTime.UtcNow,
            RequestedRole = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Created user: {Email} with role: {Role}", email, role);
        }
        else
        {
            logger.LogError("Failed to create user {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedCategoriesAsync(BookManagementContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Categories already exist, skipping seeding");
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Fiction", Description = "Fictional books and novels" },
            new Category { Name = "Non-Fiction", Description = "Non-fictional books and educational content" },
            new Category { Name = "Science", Description = "Scientific books and research" },
            new Category { Name = "Technology", Description = "Technology and programming books" },
            new Category { Name = "History", Description = "Historical books and biographies" },
            new Category { Name = "Romance", Description = "Romance novels and love stories" },
            new Category { Name = "Mystery", Description = "Mystery and thriller books" },
            new Category { Name = "Fantasy", Description = "Fantasy and magical stories" },
            new Category { Name = "Biography", Description = "Biographies and memoirs" },
            new Category { Name = "Self-Help", Description = "Self-improvement and motivational books" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} categories", categories.Length);
    }

    private static async Task SeedSampleBooksAsync(BookManagementContext context, ILogger logger)
    {
        if (await context.Books.AnyAsync())
        {
            logger.LogInformation("Books already exist, skipping sample book seeding");
            return;
        }

        var categories = await context.Categories.ToListAsync();
        if (!categories.Any())
        {
            logger.LogWarning("No categories found, cannot seed sample books");
            return;
        }

        var fictionCategory = categories.FirstOrDefault(c => c.Name == "Fiction");
        var technologyCategory = categories.FirstOrDefault(c => c.Name == "Technology");
        var historyCategory = categories.FirstOrDefault(c => c.Name == "History");
        var fantasyCategory = categories.FirstOrDefault(c => c.Name == "Fantasy");
        var selfHelpCategory = categories.FirstOrDefault(c => c.Name == "Self-Help");
        var scienceCategory = categories.FirstOrDefault(c => c.Name == "Science");
        var romanceCategory = categories.FirstOrDefault(c => c.Name == "Romance");
        var mysteryCategory = categories.FirstOrDefault(c => c.Name == "Mystery");
        var biographyCategory = categories.FirstOrDefault(c => c.Name == "Biography");
        var nonFictionCategory = categories.FirstOrDefault(c => c.Name == "Non-Fiction");

        var sampleBooks = new List<Book>();

        // Fiction Books
        if (fictionCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    ISBN = new ISBN("9780743273565"),
                    Description = "A classic American novel set in the Jazz Age, exploring themes of wealth, love, and the American Dream.",
                    Price = new Money(19.99m),
                    StockQuantity = 50,
                    CategoryId = fictionCategory.Id,
                    ImageUrl = "/images/books/great-gatsby.jpg",
                    PublicationDate = new DateTime(1925, 4, 10),
                    LowStockThreshold = 10
                },
                new Book
                {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    ISBN = new ISBN("9780061120084"),
                    Description = "A gripping tale of racial injustice and childhood innocence in the American South.",
                    Price = new Money(16.99m),
                    StockQuantity = 35,
                    CategoryId = fictionCategory.Id,
                    ImageUrl = "/images/demo-book-fiction.svg",
                    PublicationDate = new DateTime(1960, 7, 11),
                    LowStockThreshold = 8
                },
                new Book
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = new ISBN("9780452284234"),
                    Description = "A dystopian social science fiction novel about totalitarian control and surveillance.",
                    Price = new Money(18.50m),
                    StockQuantity = 42,
                    CategoryId = fictionCategory.Id,
                    ImageUrl = "/images/demo-book-fiction.svg",
                    PublicationDate = new DateTime(1949, 6, 8),
                    LowStockThreshold = 12
                }
            });
        }

        // Technology Books
        if (technologyCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = new ISBN("9780132350884"),
                    Description = "A handbook of agile software craftsmanship with practical advice for writing maintainable code.",
                    Price = new Money(49.99m),
                    StockQuantity = 30,
                    CategoryId = technologyCategory.Id,
                    ImageUrl = "/images/demo-book-technology.svg",
                    PublicationDate = new DateTime(2008, 8, 1),
                    LowStockThreshold = 5
                },
                new Book
                {
                    Title = "Design Patterns",
                    Author = "Gang of Four",
                    ISBN = new ISBN("9780201633610"),
                    Description = "Elements of reusable object-oriented software design patterns.",
                    Price = new Money(54.99m),
                    StockQuantity = 25,
                    CategoryId = technologyCategory.Id,
                    ImageUrl = "/images/demo-book-technology.svg",
                    PublicationDate = new DateTime(1994, 10, 21),
                    LowStockThreshold = 5
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "David Thomas, Andrew Hunt",
                    ISBN = new ISBN("9780135957059"),
                    Description = "Your journey to mastery in software development and programming craftsmanship.",
                    Price = new Money(45.99m),
                    StockQuantity = 28,
                    CategoryId = technologyCategory.Id,
                    ImageUrl = "/images/demo-book-technology.svg",
                    PublicationDate = new DateTime(1999, 10, 30),
                    LowStockThreshold = 6
                }
            });
        }

        // History Books
        if (historyCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "Sapiens",
                    Author = "Yuval Noah Harari",
                    ISBN = new ISBN("9780062316097"),
                    Description = "A brief history of humankind, exploring how Homo sapiens came to dominate the world.",
                    Price = new Money(19.99m),
                    StockQuantity = 25,
                    CategoryId = historyCategory.Id,
                    ImageUrl = "/images/demo-book-placeholder.svg",
                    PublicationDate = new DateTime(2014, 9, 4),
                    LowStockThreshold = 8
                },
                new Book
                {
                    Title = "The Guns of August",
                    Author = "Barbara Tuchman",
                    ISBN = new ISBN("9780345476098"),
                    Description = "A detailed account of the first month of World War I and the events leading to it.",
                    Price = new Money(22.99m),
                    StockQuantity = 20,
                    CategoryId = historyCategory.Id,
                    ImageUrl = "/images/demo-book-placeholder.svg",
                    PublicationDate = new DateTime(1962, 1, 1),
                    LowStockThreshold = 5
                }
            });
        }

        // Fantasy Books
        if (fantasyCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    ISBN = new ISBN("9780547928227"),
                    Description = "A fantasy adventure novel about Bilbo Baggins' unexpected journey to Lonely Mountain.",
                    Price = new Money(14.99m),
                    StockQuantity = 40,
                    CategoryId = fantasyCategory.Id,
                    ImageUrl = "/images/books/hobbit.jpg",
                    PublicationDate = new DateTime(1937, 9, 21),
                    LowStockThreshold = 10
                },
                new Book
                {
                    Title = "Harry Potter and the Philosopher's Stone",
                    Author = "J.K. Rowling",
                    ISBN = new ISBN("9780439708180"),
                    Description = "The first book in the Harry Potter series about a young wizard's adventures at Hogwarts.",
                    Price = new Money(12.99m),
                    StockQuantity = 60,
                    CategoryId = fantasyCategory.Id,
                    ImageUrl = "/images/demo-book-placeholder.svg",
                    PublicationDate = new DateTime(1997, 6, 26),
                    LowStockThreshold = 15
                }
            });
        }

        // Self-Help Books
        if (selfHelpCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "Atomic Habits",
                    Author = "James Clear",
                    ISBN = new ISBN("9780735211292"),
                    Description = "An easy and proven way to build good habits and break bad ones through small changes.",
                    Price = new Money(18.99m),
                    StockQuantity = 35,
                    CategoryId = selfHelpCategory.Id,
                    ImageUrl = "/images/demo-book-placeholder.svg",
                    PublicationDate = new DateTime(2018, 10, 16),
                    LowStockThreshold = 8
                },
                new Book
                {
                    Title = "The 7 Habits of Highly Effective People",
                    Author = "Stephen R. Covey",
                    ISBN = new ISBN("9781982137274"),
                    Description = "Powerful lessons in personal change and effectiveness for achieving success.",
                    Price = new Money(21.99m),
                    StockQuantity = 30,
                    CategoryId = selfHelpCategory.Id,
                    ImageUrl = "/images/demo-book-placeholder.svg",
                    PublicationDate = new DateTime(1989, 8, 15),
                    LowStockThreshold = 7
                }
            });
        }

        // Science Books
        if (scienceCategory != null)
        {
            sampleBooks.AddRange(new[]
            {
                new Book
                {
                    Title = "A Brief History of Time",
                    Author = "Stephen Hawking",
                    ISBN = new ISBN("9780553380163"),
                    Description = "A landmark volume in science writing that explores the nature of time and the universe.",
                    Price = new Money(17.99m),
                    StockQuantity = 22,
                    CategoryId = scienceCategory.Id,
                    ImageUrl = "/images/demo-book-science.svg",
                    PublicationDate = new DateTime(1988, 4, 1),
                    LowStockThreshold = 6
                },
                new Book
                {
                    Title = "The Origin of Species",
                    Author = "Charles Darwin",
                    ISBN = new ISBN("9780140432053"),
                    Description = "Darwin's groundbreaking work on the theory of evolution by natural selection.",
                    Price = new Money(15.99m),
                    StockQuantity = 18,
                    CategoryId = scienceCategory.Id,
                    ImageUrl = "/images/demo-book-science.svg",
                    PublicationDate = new DateTime(1859, 11, 24),
                    LowStockThreshold = 5
                }
            });
        }

        // Romance Books
        if (romanceCategory != null)
        {
            sampleBooks.Add(new Book
            {
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                ISBN = new ISBN("9780141439518"),
                Description = "A romantic novel about Elizabeth Bennet and Mr. Darcy's complex relationship.",
                Price = new Money(13.99m),
                StockQuantity = 32,
                CategoryId = romanceCategory.Id,
                ImageUrl = "/images/demo-book-fiction.svg",
                PublicationDate = new DateTime(1813, 1, 28),
                LowStockThreshold = 8
            });
        }

        // Mystery Books
        if (mysteryCategory != null)
        {
            sampleBooks.Add(new Book
            {
                Title = "The Murder of Roger Ackroyd",
                Author = "Agatha Christie",
                ISBN = new ISBN("9780062073501"),
                Description = "A classic Hercule Poirot mystery with one of the most famous plot twists in detective fiction.",
                Price = new Money(16.99m),
                StockQuantity = 26,
                CategoryId = mysteryCategory.Id,
                ImageUrl = "/images/demo-book-placeholder.svg",
                PublicationDate = new DateTime(1926, 6, 1),
                LowStockThreshold = 6
            });
        }

        // Biography Books
        if (biographyCategory != null)
        {
            sampleBooks.Add(new Book
            {
                Title = "Steve Jobs",
                Author = "Walter Isaacson",
                ISBN = new ISBN("9781451648539"),
                Description = "The exclusive biography of Apple co-founder Steve Jobs, based on extensive interviews.",
                Price = new Money(24.99m),
                StockQuantity = 20,
                CategoryId = biographyCategory.Id,
                ImageUrl = "/images/demo-book-placeholder.svg",
                PublicationDate = new DateTime(2011, 10, 24),
                LowStockThreshold = 5
            });
        }

        if (sampleBooks.Any())
        {
            context.Books.AddRange(sampleBooks);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} sample books with ISBN values", sampleBooks.Count);
        }
        else
        {
            logger.LogWarning("No sample books were seeded due to missing categories");
        }
    }

    private static async Task SeedSystemSettingsAsync(BookManagementContext context, ILogger logger)
    {
        if (await context.SystemSettings.AnyAsync())
        {
            logger.LogInformation("System settings already exist, skipping seeding");
            return;
        }

        var systemSettings = new SystemSettings(
            smtpHost: "smtp.gmail.com",
            smtpPort: 587,
            smtpUsername: "noreply@whisperingpages.com",
            smtpPassword: "defaultpassword",
            senderName: "Whispering Pages",
            senderEmail: "noreply@whisperingpages.com",
            siteName: "Whispering Pages",
            contactEmail: "contact@whisperingpages.com"
        );

        context.SystemSettings.Add(systemSettings);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded system settings");
    }

    private static async Task SeedSampleOrdersAsync(BookManagementContext context, UserManager<User> userManager, ILogger logger)
    {
        if (await context.Orders.AnyAsync())
        {
            logger.LogInformation("Orders already exist, skipping sample order seeding");
            return;
        }

        var users = await userManager.Users.Where(u => !u.IsDeleted).ToListAsync();
        var books = await context.Books.ToListAsync();

        if (!users.Any() || !books.Any())
        {
            logger.LogWarning("No users or books found, cannot seed sample orders");
            return;
        }

        var sampleOrders = new List<Order>();
        
        // Generate 40-50 random orders over the past 90 days
        int orderCount = Random.Shared.Next(40, 55);
        for (int i = 0; i < orderCount; i++)
        {
            var user = users[Random.Shared.Next(users.Count)];
            var statusArray = new[] { OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled };
            
            // Bias heavily towards completed/delivered orders
            var status = Random.Shared.Next(0, 10) > 7 ? statusArray[Random.Shared.Next(statusArray.Length)] : OrderStatus.Completed;
            var paymentStatus = status == OrderStatus.Cancelled ? PaymentStatus.Refunded : (status == OrderStatus.Pending ? PaymentStatus.Pending : PaymentStatus.Paid);

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 90)).AddHours(-Random.Shared.Next(1, 24)),
                Status = status,
                PaymentStatus = paymentStatus,
                ShippingAddress = new Address(
                    fullName: user.Name,
                    street: $"{Random.Shared.Next(100, 9999)} Bookish Lane",
                    city: "Literary City",
                    state: "Reader State",
                    zipCode: $"{Random.Shared.Next(10000, 99999)}",
                    country: "USA"
                ),
                TotalAmount = new Money(0)
            };

            var orderDetails = new List<OrderDetail>();
            int lines = Random.Shared.Next(1, 5);
            var selectedBooks = books.OrderBy(x => Random.Shared.Next()).Take(lines).ToList();
            decimal totalAmount = 0;

            foreach (var book in selectedBooks)
            {
                var quantity = Random.Shared.Next(1, 3);
                var unitPrice = new Money(book.Price.Amount);
                totalAmount += unitPrice.Amount * quantity;

                var orderDetail = new OrderDetail
                {
                    BookId = book.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Subtotal = unitPrice.Multiply(quantity),
                    Order = order
                };

                orderDetails.Add(orderDetail);
            }

            order.TotalAmount = new Money(totalAmount);
            order.OrderDetails = orderDetails;
            sampleOrders.Add(order);
        }

        context.Orders.AddRange(sampleOrders);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample orders with order details", sampleOrders.Count);
    }

    private static async Task SeedSampleReviewsAsync(BookManagementContext context, UserManager<User> userManager, ILogger logger)
    {
        if (await context.BookReviews.AnyAsync())
        {
            logger.LogInformation("Book reviews already exist, skipping sample review seeding");
            return;
        }

        var users = await userManager.Users.Where(u => !u.IsDeleted).ToListAsync();
        var books = await context.Books.ToListAsync();
    
        if (!users.Any() || !books.Any())
        {
            logger.LogWarning("No users or books found, cannot seed sample reviews");
            return;
        }

        var sampleReviews = new List<BookReview>();
        var reviewTexts = new[]
        {
            "Excellent book! Highly recommend it to anyone interested in this topic.",
            "Good read, but could have been better. The pacing was a bit slow.",
            "Outstanding work! The author's expertise really shows through.",
            "Decent book with some interesting insights. Worth reading.",
            "Fantastic! This book changed my perspective on the subject.",
            "Well-written and engaging. Couldn't put it down!",
            "Informative and well-researched. Great for beginners.",
            "Amazing book! The examples are practical and easy to follow.",
            "Good content but the writing style could be improved.",
            "Brilliant work! Every chapter was enlightening."
        };

        foreach (var book in books)
        {
            // Add 1-8 reviews per book
            var reviewCount = Random.Shared.Next(1, 9);
            var selectedUsers = users.OrderBy(x => Random.Shared.Next()).Take(reviewCount).ToList();

            foreach (var user in selectedUsers)
            {
                var review = new BookReview
                {
                    BookId = book.Id,
                    UserId = user.Id,
                    Rating = Random.Shared.Next(3, 6), // 3-5 star ratings
                    ReviewText = reviewTexts[Random.Shared.Next(reviewTexts.Length)],
                    Status = ReviewStatus.Approved
                };

                sampleReviews.Add(review);
            }
        }

        context.BookReviews.AddRange(sampleReviews);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample book reviews", sampleReviews.Count);
    }

    private static async Task SeedSampleFavoritesAsync(BookManagementContext context, UserManager<User> userManager, ILogger logger)
    {
        if (await context.UserFavorites.AnyAsync())
        {
            logger.LogInformation("User favorites already exist, skipping sample favorites seeding");
            return;
        }

        var users = await userManager.Users.Where(u => !u.IsDeleted).ToListAsync();
        var books = await context.Books.ToListAsync();

        if (!users.Any() || !books.Any())
        {
            logger.LogWarning("No users or books found, cannot seed sample favorites");
            return;
        }

        var sampleFavorites = new List<UserFavorite>();

        foreach (var user in users)
        {
            // Each user favorites 5-15 random books
            var favoriteCount = Random.Shared.Next(5, 16);
            var selectedBooks = books.OrderBy(x => Random.Shared.Next()).Take(favoriteCount).ToList();

            foreach (var book in selectedBooks)
            {
                var favorite = new UserFavorite
                {
                    UserId = user.Id,
                    BookId = book.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90))
                };

                sampleFavorites.Add(favorite);
            }
        }

        context.UserFavorites.AddRange(sampleFavorites);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample user favorites", sampleFavorites.Count);
    }

}