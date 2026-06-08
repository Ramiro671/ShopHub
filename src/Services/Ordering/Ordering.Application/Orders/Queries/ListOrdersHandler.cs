using MediatR;

namespace Ordering.Application.Orders.Queries;

internal sealed class ListOrdersHandler(IOrderRepository repository)
    : IRequestHandler<ListOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.ToDto).ToList();
    }
}
