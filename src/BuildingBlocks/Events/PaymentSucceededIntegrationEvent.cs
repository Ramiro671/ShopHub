namespace BuildingBlocks.Events;

public record PaymentSucceededIntegrationEvent(Guid OrderId, DateTime PaidAt);
