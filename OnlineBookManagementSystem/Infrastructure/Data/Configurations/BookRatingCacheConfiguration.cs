using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class BookRatingCacheConfiguration : IEntityTypeConfiguration<BookRatingCache>
    {
        public void Configure(EntityTypeBuilder<BookRatingCache> builder)
        {
            builder.HasKey(e => e.BookId);

            // Properties
            builder.Property(e => e.AverageRating)
                .HasColumnType("real")
                .IsRequired();

            builder.Property(e => e.TotalReviews)
                .IsRequired();

            builder.Property(e => e.LastUpdated)
                .HasDefaultValueSql("DateTime('now')");

            // Relationships
            builder.HasOne(brc => brc.Book)
                .WithOne()
                .HasForeignKey<BookRatingCache>(brc => brc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to match Book's soft delete filter
            builder.HasQueryFilter(brc => !EF.Property<bool>(brc.Book, "IsDeleted"));
        }
    }
}