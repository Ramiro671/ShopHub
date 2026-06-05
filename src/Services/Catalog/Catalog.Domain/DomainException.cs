namespace Catalog.Domain;

// Excepción para violaciones de reglas de negocio.
// La usaremos en todas las entidades, por eso vive en la raíz del dominio.
public class DomainException(string message) : Exception(message);