using MediatR;

namespace Ordering.Application.Orders.Commands;

public record CreateOrderCommand(
    string CustomerEmail,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    List<CreateOrderItemDto> Items) : IRequest<OrderDto>;

public record CreateOrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity);
