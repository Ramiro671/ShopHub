namespace BuildingBlocks.Events;

public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt);
