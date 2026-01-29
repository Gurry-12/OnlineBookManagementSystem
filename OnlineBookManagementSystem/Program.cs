using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Presentation.Middleware;
using OnlineBookManagementSystem.Shared.Extensions;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

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

// Add All Project Services via Extension
builder.Services.AddProjectServices(builder.Configuration);

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
    // The error handling will be refined in the next steps of the plan
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Health Checks
if (builder.Configuration.GetValue<bool>("Features:EnableHealthChecks"))
{
    app.MapHealthChecks("/health");
}

// --- Database WAL mode fix for SQLite ---
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
        // ignore if cannot set WAL
    }
}

// Seed database with default data
if (builder.Configuration.GetValue<bool>("Features:EnableDatabaseSeeding"))
{
    await app.SeedDatabaseAsync();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Presentation", "wwwroot")),
    RequestPath = ""
});

app.UseRouting();

app.UseRateLimiter();

// Middleware order
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseSession();
app.UseAuthentication();
app.UseMiddleware<RoleSwitchingMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
