using MediatR;

namespace Ordering.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
