using MediatR;

namespace Catalog.Application.Products.Queries;

public record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
