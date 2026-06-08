using MediatR;
using Ordering.Domain;
using Ordering.Domain.Orders;

namespace Ordering.Application.Orders.Commands;

internal sealed class AddOrderItemHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<AddOrderItemCommand, OrderDto>
{
    public async Task<OrderDto> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new DomainException($"Pedido {request.OrderId} no encontrado.");

        var unitPrice = Money.Create(request.UnitPrice, request.Currency);
        order.AddItem(request.ProductId, request.ProductName, unitPrice, request.Quantity);

        repository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
