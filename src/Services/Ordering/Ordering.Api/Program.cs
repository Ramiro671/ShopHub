using Microsoft.EntityFrameworkCore;
using Ordering.Api.Endpoints;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderingDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Aplica migraciones pendientes automáticamente en desarrollo
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapOrderEndpoints();
app.MapHealthChecks("/health");

app.Run();
