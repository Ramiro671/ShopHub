using MediatR;

namespace Catalog.Application.Products.Commands;

internal sealed class UpdateProductHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return false;

        product.Update(request.Name, request.Description, request.Price);
        await repository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
