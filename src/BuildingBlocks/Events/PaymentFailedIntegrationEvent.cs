namespace BuildingBlocks.Events;

public record PaymentFailedIntegrationEvent(Guid OrderId, string Reason, DateTime FailedAt);
