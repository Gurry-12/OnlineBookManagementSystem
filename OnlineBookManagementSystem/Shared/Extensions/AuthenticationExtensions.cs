using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using System.Security.Claims;
using System.Text;

namespace OnlineBookManagementSystem.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Identity - Using AddIdentity for full role-based management
            services.AddIdentity<User, IdentityRole<int>>(options =>
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
            .AddEntityFrameworkStores<BookManagementContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication with enhanced error handling
            var jwtKey = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new InvalidOperationException("JWT Key not configured or too short. Minimum 32 characters required.");
            }

            var jwtIssuer = configuration["Jwt:Issuer"] ?? "WhisperingPages";
            var jwtAudience = configuration["Jwt:Audience"] ?? jwtIssuer;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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

            // Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("AdminOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin"));
                options.AddPolicy("UserOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin", "User"));
                options.AddPolicy("PublicOrHigher", policy => policy.RequireRole("SuperAdmin", "Admin", "User", "Public"));
                options.AddPolicy("AuthenticatedUsers", policy => policy.RequireRole("SuperAdmin", "Admin", "User", "Public", "Guest"));
            });

            return services;
        }
    }
}