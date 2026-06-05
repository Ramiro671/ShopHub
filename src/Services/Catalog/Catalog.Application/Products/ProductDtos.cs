namespace Catalog.Application.Products;

// DTOs: lo que entra y sale por la API. NUNCA expongas la entidad de dominio
// directamente al exterior; podrías filtrar datos o atarte a su estructura interna.
public record ProductDto(Guid Id, string Name, string Description, decimal Price);

public record CreateProductRequest(string Name, string Description, decimal Price);

public record UpdateProductRequest(string Name, string Description, decimal Price);