using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineBookManagementSystem.Helper;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Middleware;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Services;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Clear default claim mapping
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add DbContext (SQLite by default here; change if needed)
builder.Services.AddDbContext<BookManagementContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

// Switch to AddIdentityCore � removes the unwanted cookie scheme that causes /Account/Login redirects
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
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT Key not configured");
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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IActivityLogger, ActivityLogger>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IDnsChecker, DNSCheckerHelper>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddHostedService<LogCleanupService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10),
                Window = TimeSpan.Parse(builder.Configuration["RateLimiting:Window"] ?? "00:01:00")
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync($"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.", token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
        }
    };
});

// Health Checks
if (builder.Configuration.GetValue<bool>("Features:EnableHealthChecks"))
{
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<BookManagementContext>();
}

// API Documentation
if (builder.Configuration.GetValue<bool>("Features:EnableSwagger"))
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Whispering Pages API", 
            Version = "v1",
            Description = "Online Book Management System API"
        });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

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

// Configure request pipeline
if (app.Environment.IsDevelopment())
{
    if (builder.Configuration.GetValue<bool>("Features:EnableSwagger"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Whispering Pages API V1");
            c.RoutePrefix = "api-docs";
        });
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Health Checks
if (builder.Configuration.GetValue<bool>("Features:EnableHealthChecks"))
{
    app.MapHealthChecks("/health");
}

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
// (Your existing seeding code � unchanged, UserManager still works)

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<BookManagementContext>();

//        var userManager = services.GetRequiredService<UserManager<User>>();
//        var authService = services.GetRequiredService<IAuthService>();

//        await authService.SeedRolesAsync();

//        var adminEmail = builder.Configuration["User:Email"] ?? "admin@example.com";
//        var adminPassword = builder.Configuration["User:Password"] ?? "Admin@123";

//        var existingUser = await userManager.FindByEmailAsync(adminEmail);
//        if (existingUser == null)
//        {
//            var user = new User
//            {
//                UserName = adminEmail,
//                Email = adminEmail,
//                Name = "User",
//                IsEmailConfirmed = true,
//                EmailConfirmed = true,
//                IsDeleted = false
//            };
//            var result = await userManager.CreateAsync(user, adminPassword);
//            if (result.Succeeded)
//            {
//                await userManager.AddToRoleAsync(user, "User");
//            }
//        }
//        else
//        {
//            existingUser.EmailConfirmed = true;
//            existingUser.IsEmailConfirmed = true;

//            if (string.IsNullOrEmpty(existingUser.SecurityStamp))
//            {
//                existingUser.SecurityStamp = Guid.NewGuid().ToString();
//            }

//            var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
//            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, adminPassword);

//            //await userManager.UpdateAsync(existingUser);

//            //if (!await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
//            //{
//            //    await userManager.AddToRoleAsync(existingUser, "SuperAdmin");
//            //}
//        }
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}

// Error / HSTS
if (!app.Environment.IsDevelopment())
{
    // Already handled above
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

// Middleware order
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();