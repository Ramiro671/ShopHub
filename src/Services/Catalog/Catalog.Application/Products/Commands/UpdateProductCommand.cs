using MediatR;

namespace Catalog.Application.Products.Commands;

public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price) : IRequest<bool>;
