using MediatR;

namespace Ordering.Application.Orders.Commands;

public record PayOrderCommand(Guid OrderId) : IRequest<OrderDto>;
