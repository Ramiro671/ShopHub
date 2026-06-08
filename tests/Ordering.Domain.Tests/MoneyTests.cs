using FluentAssertions;
using Ordering.Domain;
using Ordering.Domain.Orders;
using Xunit;

namespace Ordering.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Create_ConValoresValidos_CreaInstancia()
    {
        var money = Money.Create(100m, "usd");

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ConMontoNegativo_LanzaDomainException()
    {
        var act = () => Money.Create(-1m, "USD");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConMonedaVacia_LanzaDomainException()
    {
        var act = () => Money.Create(10m, "");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Add_MismaMoneda_SumaCorrectamente()
    {
        var a = Money.Create(50m, "USD");
        var b = Money.Create(30m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(80m);
    }

    [Fact]
    public void Add_MonedaDistinta_LanzaDomainException()
    {
        var usd = Money.Create(50m, "USD");
        var eur = Money.Create(30m, "EUR");

        var act = () => usd.Add(eur);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Multiply_CantidadPositiva_MultiplicaCorrectamente()
    {
        var money = Money.Create(25m, "USD");

        var result = money.Multiply(3);

        result.Amount.Should().Be(75m);
    }
}
