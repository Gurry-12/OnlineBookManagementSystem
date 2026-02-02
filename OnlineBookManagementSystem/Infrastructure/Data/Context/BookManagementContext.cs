using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Infrastructure.Data.Configurations;

namespace OnlineBookManagementSystem.Infrastructure.Data.Context;

public partial class BookManagementContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public BookManagementContext()
    {
    }

    public BookManagementContext(DbContextOptions<BookManagementContext> options)
        : base(options)
    {
    }

    // Existing DbSets
    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
    public virtual DbSet<Book> Books { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public new virtual DbSet<User> Users { get; set; }  // Now Identity-backed
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }  // New
    public virtual DbSet<UserFavorite> UserFavorites { get; set; }  // New
    public virtual DbSet<BookReview> BookReviews { get; set; }  // Book Review System
    public virtual DbSet<BookRatingCache> BookRatingCache { get; set; }  // Book Review System
    public virtual DbSet<SystemSettings> SystemSettings { get; set; } // New

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  // Identity tables & configs

        // Configure concurrency tokens for all BaseEntity-derived entities
        ConfigureConcurrencyTokens(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());
        modelBuilder.ApplyConfiguration(new ShoppingCartConfiguration());
        modelBuilder.ApplyConfiguration(new BookReviewConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new BookRatingCacheConfiguration());
        modelBuilder.ApplyConfiguration(new UserFavoriteConfiguration());

        // Category configuration (if not already configured elsewhere)
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.ConcurrencyToken).IsConcurrencyToken();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Order configuration (if not already configured elsewhere)
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure Money value object
            entity.OwnsOne(o => o.TotalAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalAmount")
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            // Configure Address value object
            entity.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(500);
                address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
                address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
                address.Property(a => a.FullName).HasColumnName("ShippingFullName").HasMaxLength(200);
                address.Property(a => a.PhoneNumber).HasColumnName("ShippingPhoneNumber").HasMaxLength(20);
            });

            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.ConcurrencyToken).IsConcurrencyToken();

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Ignore computed properties
            entity.Ignore(e => e.FullName);
            entity.Ignore(e => e.Address);
            entity.Ignore(e => e.Phone);
            entity.Ignore(e => e.City);
            entity.Ignore(e => e.State);
            entity.Ignore(e => e.Country);
            entity.Ignore(e => e.ZipCode);
        });

        // OrderDetail configuration (if not already configured elsewhere)
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure Money value objects
            entity.OwnsOne(od => od.UnitPrice, price =>
            {
                price.Property(p => p.Amount)
                    .HasColumnName("UnitPrice")
                    .HasColumnType("decimal(10,2)");

                price.Property(p => p.Currency)
                    .HasColumnName("UnitPriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            entity.OwnsOne(od => od.Subtotal, subtotal =>
            {
                subtotal.Property(s => s.Amount)
                    .HasColumnName("Subtotal")
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                subtotal.Property(s => s.Currency)
                    .HasColumnName("SubtotalCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.ConcurrencyToken).IsConcurrencyToken();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(od => od.Book)
                .WithMany()
                .HasForeignKey(od => od.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.BookId);

            // Ignore computed properties
            entity.Ignore(e => e.Price);
            entity.Ignore(e => e.TotalPrice);

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Seed roles (runs on migration)
        modelBuilder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
            new IdentityRole<int> { Id = 2, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 3, Name = "User", NormalizedName = "USER" },
            new IdentityRole<int> { Id = 4, Name = "Guest", NormalizedName = "GUEST" }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Configures concurrency tokens for all BaseEntity-derived entities.
    /// SQLite-compatible approach using GUID-based ConcurrencyToken.
    /// </summary>
    private void ConfigureConcurrencyTokens(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("ConcurrencyToken")
                    .IsConcurrencyToken();
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
