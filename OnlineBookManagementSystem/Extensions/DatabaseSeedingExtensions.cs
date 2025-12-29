using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Models;

namespace OnlineBookManagementSystem.Extensions;

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
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
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
        var roles = new[] { "SuperAdmin", "Admin", "User", "Guest" };
        
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
        // Seed SuperAdmin
        var superAdminConfig = configuration.GetSection("SuperAdmin");
        await CreateUserIfNotExistsAsync(userManager, logger, 
            superAdminConfig["Email"]!, 
            superAdminConfig["Password"]!, 
            "SuperAdmin", 
            "Super Administrator");
        
        // Seed Admin
        var adminConfig = configuration.GetSection("Admin");
        await CreateUserIfNotExistsAsync(userManager, logger, 
            adminConfig["Email"]!, 
            adminConfig["Password"]!, 
            "Admin", 
            "Administrator");
        
        // Seed User
        var userConfig = configuration.GetSection("User");
        await CreateUserIfNotExistsAsync(userManager, logger, 
            userConfig["Email"]!, 
            userConfig["Password"]!, 
            "User", 
            "Regular User");
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
            IsEmailConfirmed = true,
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
        
        var sampleBooks = new List<Book>();
        
        if (fictionCategory != null)
        {
            sampleBooks.Add(new Book 
            { 
                Title = "The Great Gatsby", 
                Author = "F. Scott Fitzgerald", 
                ISBN = "978-0-7432-7356-5",
                Description = "A classic American novel set in the Jazz Age",
                Price = 12.99m,
                StockQuantity = 50,
                CategoryId = fictionCategory.Id,
                ImageUrl = "/images/books/great-gatsby.jpg",
                ImgUrl = "/images/books/great-gatsby.jpg"
            });
        }
        
        if (technologyCategory != null)
        {
            sampleBooks.Add(new Book 
            { 
                Title = "Clean Code", 
                Author = "Robert C. Martin", 
                ISBN = "978-0-13-235088-4",
                Description = "A handbook of agile software craftsmanship",
                Price = 45.99m,
                StockQuantity = 30,
                CategoryId = technologyCategory.Id,
                ImageUrl = "/images/books/clean-code.jpg",
                ImgUrl = "/images/books/clean-code.jpg"
            });
        }
        
        if (historyCategory != null)
        {
            sampleBooks.Add(new Book 
            { 
                Title = "Sapiens", 
                Author = "Yuval Noah Harari", 
                ISBN = "978-0-06-231609-7",
                Description = "A brief history of humankind",
                Price = 16.99m,
                StockQuantity = 25,
                CategoryId = historyCategory.Id,
                ImageUrl = "/images/books/sapiens.jpg",
                ImgUrl = "/images/books/sapiens.jpg"
            });
        }
        
        if (fantasyCategory != null)
        {
            sampleBooks.Add(new Book 
            { 
                Title = "The Hobbit", 
                Author = "J.R.R. Tolkien", 
                ISBN = "978-0-547-92822-7",
                Description = "A fantasy adventure novel",
                Price = 14.99m,
                StockQuantity = 40,
                CategoryId = fantasyCategory.Id,
                ImageUrl = "/images/books/hobbit.jpg",
                ImgUrl = "/images/books/hobbit.jpg"
            });
        }
        
        if (selfHelpCategory != null)
        {
            sampleBooks.Add(new Book 
            { 
                Title = "Atomic Habits", 
                Author = "James Clear", 
                ISBN = "978-0-7352-1129-2",
                Description = "An easy and proven way to build good habits and break bad ones",
                Price = 18.99m,
                StockQuantity = 35,
                CategoryId = selfHelpCategory.Id,
                ImageUrl = "/images/books/atomic-habits.jpg",
                ImgUrl = "/images/books/atomic-habits.jpg"
            });
        }
        
        if (sampleBooks.Any())
        {
            context.Books.AddRange(sampleBooks);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} sample books", sampleBooks.Count);
        }
        else
        {
            logger.LogWarning("No sample books were seeded due to missing categories");
        }
    }
}