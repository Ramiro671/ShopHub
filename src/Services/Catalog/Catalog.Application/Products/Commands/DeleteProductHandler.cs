using MediatR;

namespace Catalog.Application.Products.Commands;

internal sealed class DeleteProductHandler(IProductRepository repository)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return false;

        await repository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
