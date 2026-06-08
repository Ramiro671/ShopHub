using BuildingBlocks.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;

namespace Ordering.Application.Orders.EventHandlers;

internal sealed class OrderCreatedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IOrderRepository repository,
    ILogger<OrderCreatedDomainEventHandler> logger)
    : INotificationHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain event: OrderCreated {OrderId}", notification.OrderId);

        var order = await repository.GetByIdAsync(notification.OrderId, cancellationToken);
        if (order is null) return;

        await publishEndpoint.Publish(new OrderCreatedIntegrationEvent(
            order.Id,
            order.CustomerEmail,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.CreatedAt), cancellationToken);

        logger.LogInformation("Integration event publicado: OrderCreatedIntegrationEvent {OrderId}", notification.OrderId);
    }
}
