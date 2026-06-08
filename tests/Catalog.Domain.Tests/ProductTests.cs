using Catalog.Domain;
using Catalog.Domain.Products;
using FluentAssertions;
using Xunit;

namespace Catalog.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Create_ConDatosValidos_CreaProducto()
    {
        var product = Product.Create("Laptop", "Gaming laptop", 1299.99m);

        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(1299.99m);
    }

    [Fact]
    public void Create_SinNombre_LanzaDomainException()
    {
        var act = () => Product.Create("", "Desc", 100m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ConPrecioNegativo_LanzaDomainException()
    {
        var act = () => Product.Create("Laptop", "Desc", -1m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ConDatosValidos_ActualizaProducto()
    {
        var product = Product.Create("Laptop", "Desc", 100m);

        product.Update("Laptop Pro", "New desc", 200m);

        product.Name.Should().Be("Laptop Pro");
        product.Price.Should().Be(200m);
    }

    [Fact]
    public void Restore_ConservaId()
    {
        var id = Guid.NewGuid();
        var product = Product.Restore(id, "Test", "Desc", 50m);

        product.Id.Should().Be(id);
    }
}
