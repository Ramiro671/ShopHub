using Catalog.Domain.Products;

namespace Catalog.Application.Products;

// "Puerto": la Application dice QUÉ necesita (guardar, leer productos),
// pero NO sabe cómo. La implementación vive en Infrastructure.
// Fíjate: CancellationToken en TODOS los métodos de I/O.
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}