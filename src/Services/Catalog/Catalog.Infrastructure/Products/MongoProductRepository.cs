using Catalog.Application.Products;
using Catalog.Domain.Products;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Products;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductDocument> _collection;

    public MongoProductRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ProductDocument>("products");
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        var docs = await _collection.Find(_ => true).ToListAsync(cancellationToken);
        return docs.Select(ToDomain).ToList();
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doc = await _collection.Find(d => d.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc is null ? null : ToDomain(doc);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken) =>
        _collection.InsertOneAsync(ToDocument(product), cancellationToken: cancellationToken);

    public Task UpdateAsync(Product product, CancellationToken cancellationToken) =>
        _collection.ReplaceOneAsync(d => d.Id == product.Id, ToDocument(product), cancellationToken: cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        _collection.DeleteOneAsync(d => d.Id == id, cancellationToken);

    // --- Traducción entre las dos representaciones ---
    private static ProductDocument ToDocument(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price
    };

    private static Product ToDomain(ProductDocument d) =>
        Product.Restore(d.Id, d.Name, d.Description, d.Price);
}