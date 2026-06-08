using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Orders;

namespace Ordering.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId).IsRequired();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Quantity).IsRequired();

        builder.OwnsOne(i => i.UnitPrice, m =>
        {
            m.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasColumnType("decimal(18,2)").IsRequired();
            m.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3).IsRequired();
        });

        // Subtotal es calculado, no se persiste
        builder.Ignore(i => i.Subtotal);
    }
}
