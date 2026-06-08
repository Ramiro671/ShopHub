using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Orders;

namespace Ordering.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasColumnName("ShippingStreet").HasMaxLength(256).IsRequired();
            a.Property(p => p.City).HasColumnName("ShippingCity").HasMaxLength(128).IsRequired();
            a.Property(p => p.State).HasColumnName("ShippingState").HasMaxLength(128);
            a.Property(p => p.Country).HasColumnName("ShippingCountry").HasMaxLength(128).IsRequired();
            a.Property(p => p.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
        });

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar la colección de domain events y TotalAmount (calculado)
        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.TotalAmount);
    }
}
