using BuildingBlocks.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Endpoints;
using Ordering.Application;
using Ordering.Application.Orders.EventHandlers;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderingDbContext>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentSucceededConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapOrderEndpoints();
app.MapHealthChecks("/health");

app.Run();
