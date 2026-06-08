using MediatR;

namespace Ordering.Application.Orders.Queries;

public record ListOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;
