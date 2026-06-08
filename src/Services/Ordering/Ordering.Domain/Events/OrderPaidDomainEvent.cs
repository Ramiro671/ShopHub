namespace Ordering.Domain.Events;

public sealed record OrderPaidDomainEvent(Guid OrderId) : IDomainEvent;
