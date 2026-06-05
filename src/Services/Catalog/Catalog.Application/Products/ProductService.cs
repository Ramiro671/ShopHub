using Catalog.Domain.Products;

namespace Catalog.Application.Products;

// Orquesta los casos de uso. Recibe el repositorio por constructor (DIP de SOLID):
// depende de la INTERFAZ IProductRepository, no de una implementación concreta.
public class ProductService(IProductRepository repository) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        // La validación ocurre dentro de Product.Create (en el dominio).
        var product = Product.Create(request.Name, request.Description, request.Price);
        await repository.AddAsync(product, cancellationToken);
        return ToDto(product);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return false;

        product.Update(request.Name, request.Description, request.Price);
        await repository.UpdateAsync(product, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return false;

        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static ProductDto ToDto(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price);
}