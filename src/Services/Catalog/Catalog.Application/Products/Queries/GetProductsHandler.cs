using MediatR;

namespace Catalog.Application.Products.Queries;

internal sealed class GetProductsHandler(IProductRepository repository)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price)).ToList();
    }
}
