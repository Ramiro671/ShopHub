using Catalog.Application.Products;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using MediatR;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetProductsQuery(), ct)));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var product = await mediator.Send(new GetProductByIdQuery(id), ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        group.MapPost("/", async (CreateProductRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var created = await mediator.Send(
                new CreateProductCommand(request.Name, request.Description, request.Price), ct);
            return Results.Created($"/products/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var updated = await mediator.Send(
                new UpdateProductCommand(id, request.Name, request.Description, request.Price), ct);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteProductCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
