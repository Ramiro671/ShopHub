namespace Ordering.Domain.Orders;

public sealed record Money
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }

    #pragma warning disable CS8618
    private Money() { }
    #pragma warning restore CS8618

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La moneda es obligatoria.");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency) => new(amount, currency);

    public static Money Zero(string currency = "USD") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"No se pueden sumar monedas distintas: {Currency} y {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("La cantidad no puede ser negativa.");
        return new Money(Amount * quantity, Currency);
    }
}
