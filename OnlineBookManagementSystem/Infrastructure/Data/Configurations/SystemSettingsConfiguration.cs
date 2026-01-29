using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
    {
        public void Configure(EntityTypeBuilder<SystemSettings> builder)
        {
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.SmtpHost)
                .IsRequired();

            builder.Property(e => e.SmtpPort)
                .HasDefaultValue(587);

            builder.Property(e => e.SmtpUsername)
                .IsRequired();

            builder.Property(e => e.SmtpPassword)
                .IsRequired();

            builder.Property(e => e.EnableSsl)
                .HasDefaultValue(true);

            builder.Property(e => e.SenderName)
                .HasDefaultValue("Whispering Pages");

            builder.Property(e => e.SenderEmail)
                .IsRequired();

            builder.Property(e => e.SiteName)
                .HasDefaultValue("Whispering Pages");

            builder.Property(e => e.ContactEmail)
                .IsRequired();

            builder.Property(e => e.MaintenanceMode)
                .HasDefaultValue(false);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}