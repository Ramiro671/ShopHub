using Catalog.Domain.Products;
using MediatR;

namespace Catalog.Application.Products.Commands;

internal sealed class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price);
        await repository.AddAsync(product, cancellationToken);
        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
