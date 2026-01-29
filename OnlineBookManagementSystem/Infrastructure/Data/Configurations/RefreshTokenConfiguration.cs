using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Token)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(e => e.ReplacedByToken)
                .HasMaxLength(450);

            builder.Property(e => e.CreatedByIp)
                .HasMaxLength(45);

            builder.Property(e => e.ExpiryDate)
                .IsRequired();

            builder.Property(e => e.Created)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsRevoked)
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(rt => rt.Token)
                .IsUnique();
            builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiryDate });

            // Note: Removed global query filter to allow refresh token validation logic to work properly
            // The service layer will handle filtering for active tokens when needed
        }
    }
}