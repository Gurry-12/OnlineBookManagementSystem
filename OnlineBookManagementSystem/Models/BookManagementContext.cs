using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineBookManagementSystem.Models;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  // Identity tables & configs

        // ActivityLog
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Level).HasMaxLength(20).HasDefaultValue("Info");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("DateTime('now')");
            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
        });

        // Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.ISBN).HasMaxLength(20);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.IsFavorite).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.ISBN).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);  // Soft delete
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50).HasDefaultValue("Unpaid");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // OrderDetail
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            entity.HasOne(d => d.Book).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasIndex(e => e.OrderId);
        });

        // ShoppingCart
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.AddedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasOne(d => d.Book).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.User).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasIndex(e => new { e.UserId, e.BookId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // RefreshToken (new)
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(450);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(450);
            entity.Property(e => e.CreatedByIp).HasMaxLength(45);
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.Created).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiryDate });
            entity.HasQueryFilter(rt => !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
        });

        // UserFavorite (new)
        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.BookId }).IsUnique();
        });

        // User (extend Identity)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => new { e.Email, e.IsDeleted }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            entity.HasQueryFilter(e => EF.Property<bool>(e, "IsDeleted") == false);
        });

        // Seed roles (runs on migration)
        modelBuilder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "SuperAdmin", NormalizedName = "SUPERADMIN" },
            new IdentityRole<int> { Id = 2, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 3, Name = "User", NormalizedName = "USER" },
            new IdentityRole<int> { Id = 4, Name = "Guest", NormalizedName = "GUEST" }
        );
        // Make ActivityLog.UserId optional (nullable FK)
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.Property(al => al.UserId)
                  .IsRequired(false); // This makes the column nullable

            // If you have navigation properties:
            entity.HasOne(al => al.User)
                  .WithMany(u => u.ActivityLogs)
                  .HasForeignKey(al => al.UserId)
                  .OnDelete(DeleteBehavior.SetNull); // or Restrict
        });

        // BookReview (Book Review System)
        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.ReviewText).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue(ReviewStatus.Pending);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("DateTime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("DateTime('now')");
            
            // Foreign key relationships
            entity.HasOne(d => d.Book).WithMany(b => b.BookReviews)
                .HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.User).WithMany(u => u.BookReviews)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Moderator).WithMany(u => u.ModeratedReviews)
                .HasForeignKey(d => d.ModeratedBy).OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for performance
            entity.HasIndex(e => new { e.BookId, e.Status });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasIndex(e => e.Rating);
            
            // Unique constraint: one review per user per book
            entity.HasIndex(e => new { e.BookId, e.UserId }).IsUnique();
            
            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // Check constraints for rating
            entity.ToTable(t => t.HasCheckConstraint("CK_BookReview_Rating", "Rating >= 1 AND Rating <= 5"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BookReview_ReviewText_Length", "LENGTH(ReviewText) >= 10 AND LENGTH(ReviewText) <= 1000"));
        });

        // BookRatingCache (Book Review System)
        modelBuilder.Entity<BookRatingCache>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.AverageRating).HasColumnType("real").IsRequired();
            entity.Property(e => e.TotalReviews).IsRequired();
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("DateTime('now')");
            
            // Foreign key relationship
            entity.HasOne(d => d.Book).WithOne()
                .HasForeignKey<BookRatingCache>(d => d.BookId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}