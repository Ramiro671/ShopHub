namespace Catalog.Domain.Products;

public class Product
{
    // Propiedades con 'private set': el estado SOLO cambia a través de los
    // métodos de la clase (Create/Update). Así nadie puede crear un producto
    // en un estado inválido. Esto es encapsulación, base del diseño orientado a dominio.
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }

    // Constructor privado: obliga a usar la fábrica Create.
    private Product(Guid id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }

    // Método de fábrica: única forma de crear un producto válido.
    public static Product Create(string name, string? description, decimal price)
    {
        Validate(name, price);
        return new Product(Guid.NewGuid(), name.Trim(), Clean(description), price);
    }

    // Reconstruye un producto que YA existe en la base de datos (conserva su Id).
// Uso exclusivo de la capa de infraestructura al leer de persistencia.
public static Product Restore(Guid id, string name, string description, decimal price) =>
    new(id, name, description, price);

    public void Update(string name, string? description, decimal price)
    {
        Validate(name, price);
        Name = name.Trim();
        Description = Clean(description);
        Price = price;
    }

    public void SetImageUrl(string url) => ImageUrl = url;

    private static void Validate(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");
        if (price < 0)
            throw new DomainException("El precio no puede ser negativo.");
    }

    private static string Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}