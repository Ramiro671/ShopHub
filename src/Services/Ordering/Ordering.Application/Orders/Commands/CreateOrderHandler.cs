using MediatR;
using Ordering.Domain.Orders;

namespace Ordering.Application.Orders.Commands;

internal sealed class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(request.Street, request.City, request.State, request.Country, request.ZipCode);
        var order = Order.Create(request.CustomerEmail, address);

        foreach (var item in request.Items)
        {
            var unitPrice = Money.Create(item.UnitPrice, item.Currency);
            order.AddItem(item.ProductId, item.ProductName, unitPrice, item.Quantity);
        }

        await repository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
