using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Action)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Message)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500);

            builder.Property(e => e.Level)
                .HasMaxLength(20)
                .HasDefaultValue("Info");

            builder.Property(e => e.Timestamp)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(e => e.Timestamp);
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Level);

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}