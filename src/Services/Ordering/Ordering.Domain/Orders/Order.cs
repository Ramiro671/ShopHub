using Ordering.Domain.Events;

namespace Ordering.Domain.Orders;

public class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public string CustomerEmail { get; private set; }
    public Address ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructor sin parámetros requerido por EF Core para materialización
    #pragma warning disable CS8618
    private Order() { }
    #pragma warning restore CS8618

    private Order(Guid id, string customerEmail, Address shippingAddress, OrderStatus status, DateTime createdAt)
    {
        Id = id;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Order Create(string customerEmail, Address shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new DomainException("El email del cliente es obligatorio.");

        var order = new Order(Guid.NewGuid(), customerEmail.Trim(), shippingAddress, OrderStatus.Pending, DateTime.UtcNow);
        order._domainEvents.Add(new OrderCreatedDomainEvent(order.Id, order.CustomerEmail));
        return order;
    }

    public static Order Restore(Guid id, string customerEmail, Address shippingAddress, OrderStatus status, DateTime createdAt)
    {
        return new Order(id, customerEmail, shippingAddress, status, createdAt);
    }

    public void RestoreItems(IEnumerable<OrderItem> items) => _items.AddRange(items);

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Solo se pueden agregar items a un pedido pendiente.");

        var item = OrderItem.Create(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }

    public Money TotalAmount
    {
        get
        {
            if (_items.Count == 0)
                return Money.Zero();

            return _items
                .Select(i => i.Subtotal)
                .Aggregate((a, b) => a.Add(b));
        }
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Solo se puede pagar un pedido pendiente.");

        Status = OrderStatus.Paid;
        _domainEvents.Add(new OrderPaidDomainEvent(Id));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new DomainException("No se puede cancelar un pedido ya pagado.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("El pedido ya está cancelado.");

        Status = OrderStatus.Cancelled;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
