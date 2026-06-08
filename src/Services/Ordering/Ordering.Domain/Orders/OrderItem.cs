namespace Ordering.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    #pragma warning disable CS8618
    private OrderItem() { }
    #pragma warning restore CS8618

    private OrderItem(Guid id, Guid productId, string productName, Money unitPrice, int quantity)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    internal static OrderItem Create(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("El nombre del producto es obligatorio.");
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        return new OrderItem(Guid.NewGuid(), productId, productName, unitPrice, quantity);
    }

    internal static OrderItem Restore(Guid id, Guid productId, string productName, Money unitPrice, int quantity)
        => new(id, productId, productName, unitPrice, quantity);

    public Money Subtotal => UnitPrice.Multiply(Quantity);
}
