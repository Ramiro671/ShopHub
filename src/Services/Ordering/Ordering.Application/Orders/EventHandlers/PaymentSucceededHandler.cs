using BuildingBlocks.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Orders.EventHandlers;

public sealed class PaymentSucceededConsumer(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentSucceededConsumer> logger) : IConsumer<PaymentSucceededIntegrationEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededIntegrationEvent> context)
    {
        var orderId = context.Message.OrderId;
        logger.LogInformation("Consumiendo PaymentSucceeded para Order {OrderId}", orderId);

        var order = await repository.GetByIdAsync(orderId, context.CancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} no encontrada", orderId);
            return;
        }

        // Idempotencia: si ya está pagada, no hacer nada
        if (order.Status == Domain.Orders.OrderStatus.Paid)
        {
            logger.LogInformation("Order {OrderId} ya estaba pagada (idempotente)", orderId);
            return;
        }

        order.MarkAsPaid();
        repository.Update(order);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Order {OrderId} marcada como pagada", orderId);
    }
}
