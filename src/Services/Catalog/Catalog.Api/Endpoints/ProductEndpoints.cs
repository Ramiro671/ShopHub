using Catalog.Application.Products;
using Catalog.Domain;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // Agrupa todas las rutas bajo /products.
        var group = app.MapGroup("/products").WithTags("Products");

        // Fíjate en el patrón de TODOS los endpoints:
        //  - son async
        //  - reciben IProductService inyectado automáticamente
        //  - reciben y propagan el CancellationToken (ct) del request

        group.MapGet("/", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        group.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            try
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/products/{created.Id}", created);
            }
            catch (DomainException ex)
            {
                // Por ahora atrapamos la excepción aquí. En una fase posterior lo
                // centralizaremos con IExceptionHandler para no repetir este try/catch.
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, request, ct);
                return updated ? Results.NoContent() : Results.NotFound();
            }
            catch (DomainException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}