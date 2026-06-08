using MediatR;

namespace Ordering.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId) : IRequest<OrderDto>;
