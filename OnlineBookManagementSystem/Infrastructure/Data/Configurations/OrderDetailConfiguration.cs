using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookManagementSystem.Core.Domain.Entities;

namespace OnlineBookManagementSystem.Infrastructure.Data.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.HasKey(e => e.Id);

            // Configure Money value objects
            builder.OwnsOne(od => od.UnitPrice, price =>
            {
                price.Property(p => p.Amount)
                    .HasColumnName("UnitPrice")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                price.Property(p => p.Currency)
                    .HasColumnName("UnitPriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD")
                    .IsRequired();
            });

            builder.OwnsOne(od => od.Subtotal, subtotal =>
            {
                subtotal.Property(s => s.Amount)
                    .HasColumnName("Subtotal")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                subtotal.Property(s => s.Currency)
                    .HasColumnName("SubtotalCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD")
                    .IsRequired();
            });

            // Properties
            builder.Property(e => e.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("DateTime('now')");

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Book)
                .WithMany()
                .HasForeignKey(od => od.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => e.OrderId);
            builder.HasIndex(e => e.BookId);

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);

            // Ignore computed properties
            builder.Ignore(e => e.Price);
            builder.Ignore(e => e.TotalPrice);
        }
    }
}