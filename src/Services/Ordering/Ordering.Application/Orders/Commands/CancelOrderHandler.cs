using MediatR;
using Ordering.Domain;

namespace Ordering.Application.Orders.Commands;

internal sealed class CancelOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Pedido {request.OrderId} no encontrado.");

        order.Cancel();

        repository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
