using MediatR;

namespace Ordering.Application.Orders.Commands;

public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity) : IRequest<OrderDto>;
