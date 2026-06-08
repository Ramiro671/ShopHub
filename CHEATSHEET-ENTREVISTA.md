# Cheatsheet para Entrevista — .NET Senior

Cada respuesta está anclada a un archivo concreto del repositorio ShopHub.

---

## 1. async/await y CancellationToken

**Pregunta**: ¿Cómo manejas operaciones asíncronas y cancelación?

**Respuesta**: Toda operación de I/O es `async Task` y propaga `CancellationToken`. Nunca uso `.Result` ni `.Wait()` (causan deadlocks). El token se recibe desde el endpoint (Minimal API lo inyecta como `CancellationToken ct`) y se pasa hasta el repositorio.

**Archivo**: `src/Services/Ordering/Ordering.Application/Orders/Commands/CreateOrderHandler.cs` — el handler recibe `CancellationToken` del pipeline de MediatR y lo pasa a `repository.AddAsync()` y `unitOfWork.SaveChangesAsync()`.

---

## 2. Ciclos de vida de DI (Singleton / Scoped / Transient)

**Pregunta**: ¿Cuándo usas cada lifetime?

**Respuesta**:
- **Singleton**: servicios thread-safe que son costosos de crear (ej: `IMongoClient`, `BlobServiceClient`).
- **Scoped**: servicios con estado por request (ej: `DbContext`, repositorios que usan el DbContext).
- **Transient**: servicios ligeros sin estado.

**Archivo**: `src/Services/Catalog/Catalog.Infrastructure/DependencyInjection.cs` — `IMongoClient` es Singleton (thread-safe, costoso), `IProductRepository` es Scoped.
**Archivo**: `src/Services/Ordering/Ordering.Infrastructure/DependencyInjection.cs` — `OrderingDbContext` es Scoped (registrado por `AddDbContext`), `IUnitOfWork` se resuelve al mismo scope.

---

## 3. SOLID con ejemplo del proyecto

**Pregunta**: Muéstrame SOLID en tu código.

- **S (Single Responsibility)**: Cada handler hace una sola cosa (`CreateOrderHandler` solo crea pedidos).
- **O (Open/Closed)**: Los pipeline behaviors añaden funcionalidad (logging, validación) sin modificar handlers existentes.
- **L (Liskov)**: `AzureBlobStorage` y `AwsS3Storage` son intercambiables a través de `IObjectStorage`.
- **I (Interface Segregation)**: `IOrderRepository` solo tiene los métodos que necesitan los handlers. `IUnitOfWork` solo tiene `SaveChangesAsync`.
- **D (Dependency Inversion)**: Los handlers dependen de `IOrderRepository` (abstracción), no de `OrderRepository` (implementación). Se ve en todos los constructores de handlers.

**Archivo**: `src/BuildingBlocks/Storage/IObjectStorage.cs` (I + L + D)

---

## 4. Clean Architecture

**Pregunta**: ¿Cómo organizas las capas?

**Respuesta**: 4 capas con dependencias hacia adentro:
- **Domain**: entidades, value objects, domain events. NO referencia a nadie.
- **Application**: casos de uso (commands/queries), interfaces de repositorio. Referencia solo a Domain.
- **Infrastructure**: implementaciones concretas (EF Core, MongoDB, MassTransit). Referencia Application y Domain.
- **Api**: composición (DI, endpoints, middleware). Referencia Application e Infrastructure.

**Archivo**: Ver los `.csproj` de cada capa de Ordering — las `<ProjectReference>` muestran las dependencias.

---

## 5. DDD: Entidad vs Value Object vs Agregado

**Pregunta**: ¿Cuál es la diferencia?

- **Entidad**: tiene identidad (`Id`). Dos entidades con los mismos datos pero distinto Id son diferentes. Ej: `Order`, `OrderItem`.
- **Value Object**: se define por sus valores, no tiene identidad. Inmutable. Ej: `Money(100, "USD")` es igual a otro `Money(100, "USD")`.
- **Agregado**: cluster de entidades con una raíz que protege invariantes. `Order` es la raíz; `OrderItem` solo se modifica a través de `Order.AddItem()`.

**Domain events** son hechos que ocurrieron dentro del agregado (`OrderCreatedDomainEvent`). **Integration events** cruzan bounded contexts (`OrderCreatedIntegrationEvent`).

**Archivo**: `src/Services/Ordering/Ordering.Domain/Orders/Order.cs` (agregado raíz)
**Archivo**: `src/Services/Ordering/Ordering.Domain/Orders/Money.cs` (value object como record)

---

## 6. EF Core: Owned Types, AsNoTracking, N+1

**Pregunta**: ¿Cómo configuras EF Core?

- **Owned Types**: `Money` y `Address` se persisten como columnas de `Order` (no tablas separadas). Configurado con `OwnsOne` en Fluent API.
- **Conversión enum→string**: `HasConversion<string>()` para que el status se guarde legible.
- **AsNoTracking**: en queries de solo lectura (`GetAllAsync`) para evitar overhead del change tracker.
- **N+1**: uso `.Include(o => o.Items)` para cargar items en una sola query (eager loading).

**Archivo**: `src/Services/Ordering/Ordering.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
**Archivo**: `src/Services/Ordering/Ordering.Infrastructure/Persistence/OrderRepository.cs`

---

## 7. CQRS y Pipeline Behaviors de MediatR

**Pregunta**: ¿Qué es CQRS y para qué los behaviors?

**Respuesta**: Separar Commands (escritura) de Queries (lectura). Cada uno tiene su handler con responsabilidad única. Los pipeline behaviors son decoradores que interceptan TODA request:
- **LoggingBehavior**: loguea antes/después de cada handler.
- **ValidationBehavior**: ejecuta validadores de FluentValidation antes del handler; si falla, lanza `ValidationException` sin llegar al handler.

**Archivo**: `src/Services/Ordering/Ordering.Application/Behaviors/ValidationBehavior.cs`
**Archivo**: `src/Services/Ordering/Ordering.Application/DependencyInjection.cs` — registro con `AddOpenBehavior`

---

## 8. EDA: Outbox, Idempotencia, At-Least-Once

**Pregunta**: ¿Cómo garantizas entrega confiable de eventos?

- **Domain events en SaveChanges**: se recolectan antes de persistir y se publican después del commit. MassTransit puede configurar el patrón Outbox transaccional con EF Core.
- **Idempotencia**: el consumer de `PaymentSucceeded` verifica `order.Status == Paid` antes de actuar. Si ya está pagado, no hace nada.
- **At-least-once**: MassTransit garantiza que el mensaje se entrega al menos una vez. Por eso la idempotencia es crítica.

**Archivo**: `src/Services/Ordering/Ordering.Infrastructure/Persistence/OrderingDbContext.cs` (publish en SaveChanges)
**Archivo**: `src/Services/Ordering/Ordering.Application/Orders/EventHandlers/PaymentSucceededHandler.cs` (idempotencia)

---

## 9. NoSQL vs SQL: ¿Por qué Catalog es documental y Ordering relacional?

**Pregunta**: ¿Cómo decidiste la base de datos?

- **Catalog (MongoDB)**: Product es un agregado autónomo sin relaciones complejas. El esquema puede evolucionar fácilmente (agregar campos como ImageUrl). Las lecturas son simples (por Id o listado). No necesita transacciones ACID multi-documento.
- **Ordering (PostgreSQL)**: Order tiene items como entidades hijas con integridad referencial. Las transiciones de estado requieren ACID. Los owned types de EF Core mapean value objects elegantemente.

**Archivo**: `src/Services/Catalog/Catalog.Infrastructure/Products/ProductDocument.cs` (modelo documental)
**Archivo**: `src/Services/Ordering/Ordering.Infrastructure/Persistence/Configurations/OrderConfiguration.cs` (modelo relacional)

---

## 10. Testing: Unitario vs Integración, Testcontainers

**Pregunta**: ¿Cómo pruebas tu código?

- **Unitarios**: prueban reglas de dominio sin dependencias externas. Verifican invariantes (`Order.AddItem` a pedido pagado lanza excepción), value objects, y transiciones de estado.
- **Integración con Testcontainers**: levantan un contenedor PostgreSQL real, aplican migraciones y ejecutan queries. Garantizan que la configuración de EF Core es correcta.
- **MassTransit TestHarness**: verifica el flujo completo de eventos en memoria.

**Archivo**: `tests/Ordering.Domain.Tests/OrderTests.cs` (24 tests unitarios)
**Archivo**: `tests/Ordering.Infrastructure.Tests/PostgresFixture.cs` (Testcontainers)
**Archivo**: `tests/Payment.Worker.Tests/OrderCreatedConsumerTests.cs` (MassTransit harness)

---

## 11. Multi-Cloud

**Pregunta**: ¿Cómo manejas múltiples proveedores cloud?

**Respuesta**: Abstracción `IObjectStorage` con implementaciones `AzureBlobStorage` y `AwsS3Storage`. El proveedor se selecciona por configuración (`Storage:Provider`). El código de negocio solo conoce la interfaz. Para cambiar de proveedor, solo se modifica `appsettings.json`.

**Archivo**: `src/BuildingBlocks/Storage/StorageDependencyInjection.cs`
