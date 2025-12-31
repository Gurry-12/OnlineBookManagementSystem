using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Extensions;
using OnlineBookManagementSystem.Middleware;
using OnlineBookManagementSystem.Models;
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

// Make Program class accessible for testing
public partial class Program { }
