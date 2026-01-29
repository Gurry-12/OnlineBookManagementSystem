using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(e => e.Id);
            
            // Value object configuration
            builder.OwnsOne(b => b.Price, price =>
            {
                price.Property(p => p.Amount)
                    .HasColumnName("Price")
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);
                    
                price.Property(p => p.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            builder.OwnsOne(b => b.ISBN, isbn =>
            {
                isbn.Property(i => i.Value)
                    .HasColumnName("ISBN")
                    .HasMaxLength(20);
            });

            // Properties
            builder.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(e => e.Author)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(e => e.ImageUrl)
                .HasMaxLength(500);
                
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
                
            builder.Property(e => e.StockQuantity)
                .HasDefaultValue(0);
                
            builder.Property(e => e.LowStockThreshold)
                .HasDefaultValue(5);
                
            builder.Property(e => e.IsFeatured)
                .HasDefaultValue(false);
                
            builder.Property(e => e.AverageRating)
                .HasDefaultValue(0.0);
                
            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
                
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");
                
            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')")
                .IsConcurrencyToken();

            // Relationships
            builder.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            // Note: ISBN index temporarily removed due to EF Core configuration conflict
            // builder.HasIndex("ISBN")
            //     .IsUnique()
            //     .HasFilter("ISBN IS NOT NULL");
                
            builder.HasIndex(e => e.Title);
            builder.HasIndex(e => e.Author);
            builder.HasIndex(e => e.CategoryId);

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Ignore computed/non-persisted properties
            builder.Ignore(e => e.IsAvailable);
            builder.Ignore(e => e.IsLowStock);
            builder.Ignore(e => e.IsOutOfStock);
            builder.Ignore(e => e.IsFavorite);
        }
    }
}