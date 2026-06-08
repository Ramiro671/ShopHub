using FluentAssertions;
using Ordering.Domain.Orders;
using Ordering.Infrastructure.Persistence;
using Xunit;

namespace Ordering.Infrastructure.Tests;

public class OrderRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public OrderRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    private OrderRepository CreateRepository() => new(_fixture.DbContext);

    [Fact]
    public async Task AddAsync_Y_GetByIdAsync_PersistenYRecuperanOrden()
    {
        var repo = CreateRepository();
        var address = Address.Create("Calle 1", "CDMX", "CDMX", "México", "06600");
        var order = Order.Create("test@mail.com", address);
        order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(1500m, "USD"), 1);
        order.ClearDomainEvents();

        await repo.AddAsync(order, CancellationToken.None);
        await _fixture.DbContext.SaveChangesAsync();

        // Detach para forzar lectura desde DB
        _fixture.DbContext.ChangeTracker.Clear();

        var loaded = await repo.GetByIdAsync(order.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.CustomerEmail.Should().Be("test@mail.com");
        loaded.Items.Should().ContainSingle();
        loaded.Items[0].ProductName.Should().Be("Laptop");
        loaded.ShippingAddress.City.Should().Be("CDMX");
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodasLasOrdenes()
    {
        var repo = CreateRepository();
        var address = Address.Create("Calle 2", "GDL", "Jalisco", "México", "44100");
        var order = Order.Create("otro@mail.com", address);
        order.AddItem(Guid.NewGuid(), "Mouse", Money.Create(50m, "USD"), 3);
        order.ClearDomainEvents();

        await repo.AddAsync(order, CancellationToken.None);
        await _fixture.DbContext.SaveChangesAsync();
        _fixture.DbContext.ChangeTracker.Clear();

        var all = await repo.GetAllAsync(CancellationToken.None);

        all.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Update_CambiaEstadoDeOrden()
    {
        var repo = CreateRepository();
        var address = Address.Create("Calle 3", "MTY", "NL", "México", "64000");
        var order = Order.Create("pay@mail.com", address);
        order.AddItem(Guid.NewGuid(), "Teclado", Money.Create(80m, "USD"), 1);
        order.ClearDomainEvents();

        await repo.AddAsync(order, CancellationToken.None);
        await _fixture.DbContext.SaveChangesAsync();

        order.MarkAsPaid();
        order.ClearDomainEvents();
        repo.Update(order);
        await _fixture.DbContext.SaveChangesAsync();
        _fixture.DbContext.ChangeTracker.Clear();

        var loaded = await repo.GetByIdAsync(order.Id, CancellationToken.None);
        loaded!.Status.Should().Be(OrderStatus.Paid);
    }
}
