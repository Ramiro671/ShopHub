using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Infrastructure.Products;

// Modelo de PERSISTENCIA: un POCO simple que MongoDB sabe serializar.
// NO es la entidad de dominio. Aquí sí usamos atributos de Mongo,
// porque esta es la capa de infraestructura. El Product del dominio sigue limpio.
public class ProductDocument
{
    [BsonId]                                    // este campo es el _id del documento
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]   // guarda el precio sin perder precisión
    public decimal Price { get; set; }
}