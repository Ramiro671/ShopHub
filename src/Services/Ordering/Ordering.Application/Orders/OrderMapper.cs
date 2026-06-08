using Ordering.Domain.Orders;

namespace Ordering.Application.Orders;

internal static class OrderMapper
{
    public static OrderDto ToDto(Order order) => new(
        order.Id,
        order.CustomerEmail,
        order.Status.ToString(),
        order.TotalAmount.Amount,
        order.TotalAmount.Currency,
        order.CreatedAt,
        new AddressDto(
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.Country,
            order.ShippingAddress.ZipCode),
        order.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.ProductName,
            i.UnitPrice.Amount, i.Quantity, i.Subtotal.Amount)).ToList());
}
