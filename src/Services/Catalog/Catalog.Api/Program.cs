using BuildingBlocks.Exceptions;
using BuildingBlocks.Storage;
using Catalog.Api.Endpoints;
using Catalog.Application;
using Catalog.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddObjectStorage(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapProductEndpoints();
app.MapHealthChecks("/health");

app.Run();
