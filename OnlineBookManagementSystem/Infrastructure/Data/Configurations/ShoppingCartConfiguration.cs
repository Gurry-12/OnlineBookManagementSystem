using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Quantity)
                .HasDefaultValue(1);

            builder.Property(e => e.AddedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(sc => sc.Book)
                .WithMany()
                .HasForeignKey(sc => sc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sc => sc.User)
                .WithMany(u => u.ShoppingCarts)
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => new { e.UserId, e.BookId })
                .IsUnique();

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}