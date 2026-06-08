namespace Ordering.Domain.Orders;

public sealed record Address
{
    public string Street { get; private init; }
    public string City { get; private init; }
    public string State { get; private init; }
    public string Country { get; private init; }
    public string ZipCode { get; private init; }

    #pragma warning disable CS8618
    private Address() { }
    #pragma warning restore CS8618

    private Address(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("La calle es obligatoria.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("La ciudad es obligatoria.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("El país es obligatorio.");

        Street = street.Trim();
        City = city.Trim();
        State = (state ?? string.Empty).Trim();
        Country = country.Trim();
        ZipCode = (zipCode ?? string.Empty).Trim();
    }

    public static Address Create(string street, string city, string state, string country, string zipCode)
        => new(street, city, state, country, zipCode);
}
