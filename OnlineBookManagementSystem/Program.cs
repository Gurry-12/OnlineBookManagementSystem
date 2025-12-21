using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Helper;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Clear default claim mapping
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add DbContext (SQLite by default here; change if needed)
builder.Services.AddDbContext<BookManagementContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

// Switch to AddIdentityCore — removes the unwanted cookie scheme that causes /Account/Login redirects
// You still get UserManager, RoleManager, password hashing, etc.
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // dev-friendly
})
.AddRoles<IdentityRole<int>>()  // If you use roles
.AddEntityFrameworkStores<BookManagementContext>()
.AddDefaultTokenProviders();

// Read JWT settings from configuration (appsettings.json / env)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSecretKeyMustBeLongEnough12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "WhisperingPages";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;

// Make JWT the default AND challenge scheme
// Add custom OnChallenge to redirect MVC/HTML requests to your login page
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set true in production
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5),
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            // Custom challenge: redirect to your Auth/Login for browser/MVC requests
            OnChallenge = context =>
            {
                // Suppress default 401 behavior for HTML requests
                context.HandleResponse();

                // Only redirect if it's a browser request (accepts text/html)
                if (context.Request.Headers.Accept.ToString().Contains("text/html"))
                {
                    var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                    context.Response.Redirect($"/Auth/Login?ReturnUrl={returnUrl}");
                }
                else
                {
                    // For API/JSON requests, return plain 401
                    context.Response.StatusCode = 401;
                }

                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                // Prefer Authorization header
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    return Task.CompletedTask;
                }

                // Fallback to your "accessToken" cookie
                if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("AdminOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("UserOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin", "User"));
});

// Your custom services
builder.Services.AddScoped<ICategoryInterface, CategoryServices>();
builder.Services.AddScoped<IBookService, BookServices>();
builder.Services.AddScoped<IAuthInterface, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IActivityLogger, ActivityLogger>();
builder.Services.AddScoped<IDnsChecker, DNSCheckerHelper>();
builder.Services.AddHostedService<LogCleanupService>();

// Session & Cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Database WAL mode fix for SQLite (prevents "database is locked") ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookManagementContext>();
    try
    {
        dbContext.Database.OpenConnection();
        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();
        }
    }
    catch
    {
        // ignore if cannot set WAL (hosted environments may not support)
    }
}

// --- Seeding (roles + superadmin) ---
// (Your existing seeding code — unchanged, UserManager still works)

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BookManagementContext>();

        var userManager = services.GetRequiredService<UserManager<User>>();
        var authService = services.GetRequiredService<IAuthInterface>();

        await authService.SeedRolesAsync();

        var adminEmail = builder.Configuration["SuperAdmin:Email"] ?? "superadmin@example.com";
        var adminPassword = builder.Configuration["SuperAdmin:Password"] ?? "Admin@123";

        var existingUser = await userManager.FindByEmailAsync(adminEmail);
        if (existingUser == null)
        {
            var superAdmin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Super Admin",
                IsEmailConfirmed = true,
                EmailConfirmed = true,
                IsDeleted = false
            };
            var result = await userManager.CreateAsync(superAdmin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }
        }
        else
        {
            existingUser.EmailConfirmed = true;
            existingUser.IsEmailConfirmed = true;

            if (string.IsNullOrEmpty(existingUser.SecurityStamp))
            {
                existingUser.SecurityStamp = Guid.NewGuid().ToString();
            }

            var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, adminPassword);

            await userManager.UpdateAsync(existingUser);

            if (!await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(existingUser, "SuperAdmin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Error / HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware order
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();