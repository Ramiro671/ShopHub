using MediatR;
using Ordering.Domain;

namespace Ordering.Application.Orders.Commands;

internal sealed class PayOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<PayOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Pedido {request.OrderId} no encontrado.");

        order.MarkAsPaid();

        repository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
