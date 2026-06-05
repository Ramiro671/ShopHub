using System.Collections.Concurrent;
using Catalog.Application.Products;
using Catalog.Domain.Products;

namespace Catalog.Infrastructure.Products;

public class InMemoryProductRepository : IProductRepository
{
    // ConcurrentDictionary (no un List normal): una API web atiende muchas
    // peticiones en paralelo, así que el almacén debe ser thread-safe.
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    // Estos métodos NO usan 'async' porque no hay nada que esperar de verdad
    // (es memoria). Devolver un Task ya completado es lo correcto y más eficiente.
    // Cuando sea MongoDB sí serán async reales.
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Product>>(_products.Values.ToList());

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult<Product?>(product);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _products.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}