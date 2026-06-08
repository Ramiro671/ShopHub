using BuildingBlocks.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Payment.Worker.Consumers;

public sealed class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var orderId = context.Message.OrderId;
        logger.LogInformation("Procesando pago para Order {OrderId}, monto {Amount} {Currency}",
            orderId, context.Message.TotalAmount, context.Message.Currency);

        // Simula procesamiento de pago (en producción sería una pasarela real)
        await Task.Delay(500, context.CancellationToken);

        // Simula éxito (en un escenario real podría fallar)
        var succeeded = context.Message.TotalAmount < 10_000m; // falla si > 10k para demostrar el flujo

        if (succeeded)
        {
            await context.Publish(new PaymentSucceededIntegrationEvent(orderId, DateTime.UtcNow));
            logger.LogInformation("Pago exitoso para Order {OrderId}", orderId);
        }
        else
        {
            await context.Publish(new PaymentFailedIntegrationEvent(orderId, "Monto excede el límite", DateTime.UtcNow));
            logger.LogWarning("Pago fallido para Order {OrderId}: monto excede límite", orderId);
        }
    }
}
