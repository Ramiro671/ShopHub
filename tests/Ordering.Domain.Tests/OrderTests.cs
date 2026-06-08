using FluentAssertions;
using Ordering.Domain;
using Ordering.Domain.Events;
using Ordering.Domain.Orders;
using Xunit;

namespace Ordering.Domain.Tests;

public class OrderTests
{
    private static Address DefaultAddress =>
        Address.Create("Calle 1", "CDMX", "CDMX", "México", "06600");

    [Fact]
    public void Create_ConDatosValidos_CreaOrdenPendiente()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);

        order.Id.Should().NotBeEmpty();
        order.CustomerEmail.Should().Be("test@mail.com");
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_SinEmail_LanzaDomainException()
    {
        var act = () => Order.Create("", DefaultAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_GeneraOrderCreatedDomainEvent()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();
    }

    [Fact]
    public void AddItem_APedidoPendiente_AgregaItem()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        var price = Money.Create(100m, "USD");

        order.AddItem(Guid.NewGuid(), "Laptop", price, 2);

        order.Items.Should().ContainSingle();
        order.TotalAmount.Amount.Should().Be(200m);
    }

    [Fact]
    public void AddItem_APedidoPagado_LanzaDomainException()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(100m, "USD"), 1);
        order.MarkAsPaid();

        var act = () => order.AddItem(Guid.NewGuid(), "Mouse", Money.Create(50m, "USD"), 1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_ConCantidadCero_LanzaDomainException()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);

        var act = () => order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(100m, "USD"), 0);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_DesdePending_CambiaEstado()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(100m, "USD"), 1);

        order.MarkAsPaid();

        order.Status.Should().Be(OrderStatus.Paid);
        order.DomainEvents.Should().Contain(e => e is OrderPaidDomainEvent);
    }

    [Fact]
    public void MarkAsPaid_DesdeCancelled_LanzaDomainException()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.Cancel();

        var act = () => order.MarkAsPaid();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_DesdePending_CambiaEstado()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_DesdePagado_LanzaDomainException()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(100m, "USD"), 1);
        order.MarkAsPaid();

        var act = () => order.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_YaCancelado_LanzaDomainException()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.Cancel();

        var act = () => order.Cancel();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TotalAmount_SinItems_RetornaZero()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);

        order.TotalAmount.Amount.Should().Be(0);
    }

    [Fact]
    public void TotalAmount_ConVariosItems_SumaCorrectamente()
    {
        var order = Order.Create("test@mail.com", DefaultAddress);
        order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(1000m, "USD"), 1);
        order.AddItem(Guid.NewGuid(), "Mouse", Money.Create(50m, "USD"), 2);

        order.TotalAmount.Amount.Should().Be(1100m);
    }
}
