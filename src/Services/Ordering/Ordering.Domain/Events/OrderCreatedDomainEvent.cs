namespace Ordering.Domain.Events;

public sealed record OrderCreatedDomainEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;
