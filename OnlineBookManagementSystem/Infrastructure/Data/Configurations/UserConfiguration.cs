using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name for Identity
            builder.ToTable("AspNetUsers");

            // Properties
            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(e => e.IsPendingApproval)
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            // Indexes
            builder.HasIndex(e => new { e.Email, e.IsDeleted })
                .IsUnique();

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Navigation properties
            builder.HasMany(u => u.ActivityLogs)
                .WithOne(al => al.User)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.ShoppingCarts)
                .WithOne(sc => sc.User)
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.BookReviews)
                .WithOne(br => br.User)
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.ModeratedReviews)
                .WithOne(br => br.Moderator)
                .HasForeignKey(br => br.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}