using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineBookManagementSystem.Helper;
using OnlineBookManagementSystem.Interfaces;
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Services;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace OnlineBookManagementSystem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<BookManagementContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);
            });

            // Identity
            services.AddIdentityCore<User>(options =>
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
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<BookManagementContext>()
            .AddDefaultTokenProviders();

            // Authentication & JWT
            var jwtKey = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "WhisperingPages";
            var jwtAudience = configuration["Jwt:Audience"] ?? jwtIssuer;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
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
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if (context.Request.Headers.Accept.ToString().Contains("text/html"))
                            {
                                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                                context.Response.Redirect($"/Auth/Login?ReturnUrl={returnUrl}");
                            }
                            else
                            {
                                context.Response.StatusCode = 401;
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                                return Task.CompletedTask;
                            }
                            if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
                            {
                                context.Token = cookieToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("AdminOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin"));
                options.AddPolicy("UserOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin", "User"));
                options.AddPolicy("PublicOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin", "User", "Public"));
                options.AddPolicy("AuthenticatedUsers", policy => policy.RequireRole("SuperAdmin", "Admin", "User", "Public", "Guest"));
            });

            // Application Services
            services.AddScoped<ICategoryInterface, CategoryServices>();
            services.AddScoped<IBookService, BookServices>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IActivityLogger, ActivityLogger>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IDnsChecker, DNSCheckerHelper>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IErrorViewModelFactory, ErrorViewModelFactory>();

            // Email Configuration
            services.Configure<Models.Configuration.EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register Custom IEmailSender (MailKit)
            // Using AddTransient because MailKit SmtpClient implements IDisposable and is lightweight to create.
            // Using AddScoped would also be acceptable if we want to share connection within a request, but SmtpClient is usually meant to be used and disposed per operation or session.
            services.AddTransient<OnlineBookManagementSystem.Interfaces.IEmailSender, MailKitEmailSender>();

            services.AddHostedService<LogCleanupService>();

            services.AddHttpContextAccessor();

            // Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                            QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 10),
                            Window = TimeSpan.Parse(configuration["RateLimiting:Window"] ?? "00:01:00")
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
            if (configuration.GetValue<bool>("Features:EnableHealthChecks"))
            {
                services.AddHealthChecks()
                    .AddDbContextCheck<BookManagementContext>();
            }

            // Swagger
            if (configuration.GetValue<bool>("Features:EnableSwagger"))
            {
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
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
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // MVC
            services.AddControllersWithViews();

            return services;
        }
    }
}
