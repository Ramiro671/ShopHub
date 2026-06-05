using Catalog.Api.Endpoints;
using Catalog.Application;
using Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Cada capa aporta sus servicios. Program.cs solo las conecta.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);   // <-- antes era AddInfrastructure()

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapProductEndpoints();

app.Run();