using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Ordering.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ordering.Infrastructure.Tests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .Build();

    public OrderingDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        var mediator = Substitute.For<IMediator>();
        DbContext = new OrderingDbContext(options, mediator);
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
