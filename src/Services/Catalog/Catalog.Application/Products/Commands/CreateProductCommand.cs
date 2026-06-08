using MediatR;

namespace Catalog.Application.Products.Commands;

public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<ProductDto>;
