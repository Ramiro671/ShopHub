namespace Ordering.Application.Orders;

public record OrderDto(
    Guid Id,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    AddressDto ShippingAddress,
    IReadOnlyList<OrderItemDto> Items);

public record AddressDto(string Street, string City, string State, string Country, string ZipCode);

public record OrderItemDto(Guid Id, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);
