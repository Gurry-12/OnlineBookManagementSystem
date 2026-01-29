using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class BookReviewConfiguration : IEntityTypeConfiguration<BookReview>
    {
        public void Configure(EntityTypeBuilder<BookReview> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Rating)
                .IsRequired();

            builder.Property(e => e.ReviewText)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasDefaultValue(ReviewStatus.Pending)
                .HasConversion<int>();

            builder.Property(e => e.RejectionReason)
                .HasMaxLength(500);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(br => br.Book)
                .WithMany()
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(br => br.User)
                .WithMany(u => u.BookReviews)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(br => br.Moderator)
                .WithMany(u => u.ModeratedReviews)
                .HasForeignKey(br => br.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(e => new { e.BookId, e.Status });
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => new { e.Status, e.CreatedAt });
            builder.HasIndex(e => e.Rating);

            // Unique constraint: one review per user per book
            builder.HasIndex(e => new { e.BookId, e.UserId })
                .IsUnique();

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_BookReview_Rating", "Rating >= 1 AND Rating <= 5"));
            builder.ToTable(t => t.HasCheckConstraint("CK_BookReview_ReviewText_Length", "LENGTH(ReviewText) >= 10 AND LENGTH(ReviewText) <= 1000"));
        }
    }
}