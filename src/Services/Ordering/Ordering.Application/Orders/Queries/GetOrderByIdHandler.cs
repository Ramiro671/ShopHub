using MediatR;

namespace Ordering.Application.Orders.Queries;

internal sealed class GetOrderByIdHandler(IOrderRepository repository)
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        return order is null ? null : OrderMapper.ToDto(order);
    }
}
