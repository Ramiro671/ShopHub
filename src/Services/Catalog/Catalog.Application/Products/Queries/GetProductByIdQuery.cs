using MediatR;

namespace Catalog.Application.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
