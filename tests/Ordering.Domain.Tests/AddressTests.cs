using FluentAssertions;
using Ordering.Domain;
using Ordering.Domain.Orders;
using Xunit;

namespace Ordering.Domain.Tests;

public class AddressTests
{
    [Fact]
    public void Create_ConDatosValidos_CreaInstancia()
    {
        var address = Address.Create("Av. Reforma 123", "CDMX", "CDMX", "México", "06600");

        address.Street.Should().Be("Av. Reforma 123");
        address.City.Should().Be("CDMX");
        address.Country.Should().Be("México");
    }

    [Fact]
    public void Create_SinCalle_LanzaDomainException()
    {
        var act = () => Address.Create("", "CDMX", "CDMX", "México", "06600");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_SinCiudad_LanzaDomainException()
    {
        var act = () => Address.Create("Calle 1", "", "Estado", "México", "06600");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_SinPais_LanzaDomainException()
    {
        var act = () => Address.Create("Calle 1", "CDMX", "CDMX", "", "06600");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Igualdad_DosAddressIguales_SonIguales()
    {
        var a = Address.Create("Calle 1", "CDMX", "CDMX", "México", "06600");
        var b = Address.Create("Calle 1", "CDMX", "CDMX", "México", "06600");

        a.Should().Be(b);
    }
}
