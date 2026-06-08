using BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Payment.Worker.Consumers;
using Xunit;

namespace Payment.Worker.Tests;

public class OrderCreatedConsumerTests
{
    [Fact]
    public async Task Consume_ConMontoMenorALimite_PublicaPaymentSucceeded()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<OrderCreatedConsumer>();
            })
            .AddLogging()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new OrderCreatedIntegrationEvent(
            orderId, "test@mail.com", 500m, "USD", DateTime.UtcNow));

        (await harness.Consumed.Any<OrderCreatedIntegrationEvent>()).Should().BeTrue();
        (await harness.Published.Any<PaymentSucceededIntegrationEvent>(
            x => x.Context.Message.OrderId == orderId)).Should().BeTrue();
    }

    [Fact]
    public async Task Consume_ConMontoMayorALimite_PublicaPaymentFailed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<OrderCreatedConsumer>();
            })
            .AddLogging()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new OrderCreatedIntegrationEvent(
            orderId, "test@mail.com", 15_000m, "USD", DateTime.UtcNow));

        (await harness.Consumed.Any<OrderCreatedIntegrationEvent>()).Should().BeTrue();
        (await harness.Published.Any<PaymentFailedIntegrationEvent>(
            x => x.Context.Message.OrderId == orderId)).Should().BeTrue();
    }
}
