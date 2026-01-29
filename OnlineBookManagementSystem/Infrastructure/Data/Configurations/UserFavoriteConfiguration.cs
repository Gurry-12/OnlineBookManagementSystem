using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
    {
        public void Configure(EntityTypeBuilder<UserFavorite> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uf => uf.Book)
                .WithMany()
                .HasForeignKey(uf => uf.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => new { e.UserId, e.BookId })
                .IsUnique();

            // Query filter to match Book's soft delete filter
            builder.HasQueryFilter(uf => !EF.Property<bool>(uf.Book, "IsDeleted") && !uf.IsDeleted);
        }
    }
}