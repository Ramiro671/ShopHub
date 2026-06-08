using MediatR;
using Ordering.Application.Orders.Commands;
using Ordering.Application.Orders.Queries;

namespace Ordering.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new ListOrdersQuery(), ct)));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var order = await mediator.Send(new GetOrderByIdQuery(id), ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        group.MapPost("/", async (CreateOrderCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var order = await mediator.Send(command, ct);
            return Results.Created($"/orders/{order.Id}", order);
        });

        group.MapPost("/{id:guid}/items", async (Guid id, AddOrderItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new AddOrderItemCommand(id, request.ProductId, request.ProductName, request.UnitPrice, request.Currency, request.Quantity);
            var order = await mediator.Send(command, ct);
            return Results.Ok(order);
        });

        group.MapPost("/{id:guid}/pay", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var order = await mediator.Send(new PayOrderCommand(id), ct);
            return Results.Ok(order);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var order = await mediator.Send(new CancelOrderCommand(id), ct);
            return Results.Ok(order);
        });
    }
}

public record AddOrderItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity);
