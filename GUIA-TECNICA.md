# Guía Técnica Completa — ShopHub

> Manual de referencia para desarrolladores. Este documento explica **cada concepto, tecnología, patrón y decisión** del proyecto desde cero. Si no conoces el proyecto, empieza aquí.

---

## Tabla de Contenido

1. [Visión General del Proyecto](#1-visión-general-del-proyecto)
2. [Stack Tecnológico](#2-stack-tecnológico)
3. [Glosario de Conceptos y Patrones](#3-glosario-de-conceptos-y-patrones)
4. [Arquitectura del Sistema](#4-arquitectura-del-sistema)
5. [Clean Architecture — Las 4 Capas](#5-clean-architecture--las-4-capas)
6. [Domain-Driven Design (DDD)](#6-domain-driven-design-ddd)
7. [CQRS y MediatR](#7-cqrs-y-mediatr)
8. [Entity Framework Core — Conceptos Clave](#8-entity-framework-core--conceptos-clave)
9. [MongoDB — Conceptos Clave](#9-mongodb--conceptos-clave)
10. [Event-Driven Architecture y MassTransit](#10-event-driven-architecture-y-masstransit)
11. [Inyección de Dependencias (DI)](#11-inyección-de-dependencias-di)
12. [Diccionario de Datos](#12-diccionario-de-datos)
13. [Mapa de Archivos — Referencia Rápida](#13-mapa-de-archivos--referencia-rápida)
14. [API Reference — Endpoints](#14-api-reference--endpoints)
15. [Flujo de Ejecución Paso a Paso](#15-flujo-de-ejecución-paso-a-paso)
16. [Testing — Estrategia y Herramientas](#16-testing--estrategia-y-herramientas)
17. [Infraestructura y DevOps](#17-infraestructura-y-devops)
18. [Cross-Cutting Concerns](#18-cross-cutting-concerns)
19. [Palabras Reservadas y Sintaxis de C# Usadas](#19-palabras-reservadas-y-sintaxis-de-c-usadas)
20. [Paquetes NuGet — Referencia Completa](#20-paquetes-nuget--referencia-completa)

---

## 1. Visión General del Proyecto

**ShopHub** es una plataforma de e-commerce construida con **microservicios** en **.NET 10**. Cada servicio tiene su propia base de datos y se comunica con los demás a través de **eventos asincrónicos**.

### Servicios

| Servicio | Responsabilidad | Base de datos | Puerto |
|----------|----------------|---------------|--------|
| **Catalog.Api** | Gestión de productos (CRUD) | MongoDB (documento) | 5100 |
| **Ordering.Api** | Gestión de pedidos (crear, pagar, cancelar) | PostgreSQL (relacional) | 5102 |
| **Payment.Worker** | Procesamiento simulado de pagos | Ninguna (stateless) | — |

### Proyectos compartidos

| Proyecto | Responsabilidad |
|----------|----------------|
| **BuildingBlocks** | Eventos de integración, abstracciones de storage (Azure/AWS), manejo global de excepciones |

---

## 2. Stack Tecnológico

### Runtime y Lenguaje

| Tecnología | Versión | Para qué se usa |
|-----------|---------|-----------------|
| **.NET** | 10.0 | Runtime y SDK. El target framework en todos los `.csproj` es `net10.0`. |
| **C#** | 13 (implícito con .NET 10) | Lenguaje de programación. |
| **ASP.NET Core** | 10.0 | Framework web para las APIs (Minimal APIs). |

### Bases de Datos

| Tecnología | Versión | Servicio | Puerto host |
|-----------|---------|----------|-------------|
| **MongoDB** | 7 | Catalog | 27017 |
| **PostgreSQL** | 15 | Ordering | 5433 |

### ORMs y Drivers

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `MongoDB.Driver` | 3.x | Driver oficial de MongoDB para .NET. Conecta C# con MongoDB. |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.x | Proveedor de PostgreSQL para Entity Framework Core. |
| `Microsoft.EntityFrameworkCore` | 10.x | ORM (Object-Relational Mapper) de Microsoft. Mapea clases C# a tablas SQL. |

### Mensajería

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `MassTransit` | 8.x | Framework de mensajería que abstrae el transporte (in-memory, RabbitMQ, Azure Service Bus). |

### Mediador (CQRS)

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `MediatR` | 12.x | Implementa el patrón Mediator. Despacha commands y queries a sus handlers. |
| `MediatR.Contracts` | 2.x | Solo contiene las interfaces (`IRequest`, `INotification`). Se usa en Domain para no depender del paquete completo. |

### Validación

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `FluentValidation` | 12.x | Librería para definir reglas de validación con API fluida. |
| `FluentValidation.DependencyInjectionExtensions` | 12.x | Registro automático de validadores en el contenedor de DI. |

### Logging

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `Serilog.AspNetCore` | 9.x | Logging estructurado. Cada log tiene propiedades tipadas, no solo texto plano. |

### Cloud Storage

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `Azure.Storage.Blobs` | 12.x | SDK oficial de Azure Blob Storage. |
| `AWSSDK.S3` | 4.x | SDK oficial de Amazon S3. |

### Testing

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `xunit` | 2.x | Framework de testing. Define `[Fact]` y `[Theory]`. |
| `FluentAssertions` | 7.x | Aserciones legibles: `result.Should().Be(expected)`. |
| `NSubstitute` | 5.x | Librería de mocking. Crea implementaciones falsas de interfaces. |
| `Testcontainers.PostgreSql` | 4.x | Levanta contenedores Docker reales en tests de integración. |

### DevOps

| Herramienta | Descripción |
|------------|-------------|
| **Docker** | Contenedores para cada servicio y base de datos. |
| **Docker Compose** | Orquestación local de múltiples contenedores. |
| **Azure Pipelines** | CI/CD definido en `azure-pipelines.yml`. |

---

## 3. Glosario de Conceptos y Patrones

### Arquitectura

| Concepto | Definición | Dónde se aplica |
|----------|-----------|-----------------|
| **Microservicio** | Servicio independiente con su propia base de datos, desplegable por separado. | Catalog, Ordering y Payment son microservicios independientes. |
| **Clean Architecture** | Organización en capas concéntricas donde las dependencias apuntan hacia adentro (Domain no depende de nadie). | Cada servicio tiene 4 capas: Domain, Application, Infrastructure, Api. |
| **CQRS** | Command Query Responsibility Segregation. Separa las operaciones de escritura (Commands) de las de lectura (Queries). | Todos los handlers en `Application/Orders/Commands/` y `Application/Orders/Queries/`. |
| **DDD** | Domain-Driven Design. Modelo del software centrado en el dominio del negocio. | Entidades ricas, value objects, agregados, domain events. |
| **EDA** | Event-Driven Architecture. Los servicios se comunican publicando y consumiendo eventos. | MassTransit publica `OrderCreatedIntegrationEvent`, Payment.Worker lo consume. |

### Patrones de Diseño

| Patrón | Definición | Ejemplo en el proyecto |
|--------|-----------|----------------------|
| **Repository** | Abstracción sobre el acceso a datos. El código de negocio no sabe si los datos vienen de SQL, MongoDB o memoria. | `IProductRepository`, `IOrderRepository` (interfaces en Application, implementaciones en Infrastructure). |
| **Unit of Work** | Agrupa múltiples operaciones en una transacción. Llamas `SaveChangesAsync()` una vez al final. | `IUnitOfWork` → `OrderingDbContext` implementa ambos (`DbContext` + `IUnitOfWork`). |
| **Factory Method** | Método estático que crea instancias validadas. El constructor es privado. | `Product.Create(...)`, `Order.Create(...)`, `Money.Create(...)`. |
| **Mediator** | Un objeto central (mediador) despacha mensajes a sus handlers, desacoplando emisor de receptor. | `IMediator.Send(command)` → MediatR busca el handler registrado. |
| **Pipeline / Decorator** | Cadena de responsabilidad que intercepta cada request antes/después del handler. | `LoggingBehavior` y `ValidationBehavior` son pipeline behaviors de MediatR. |
| **Strategy** | Intercambiar algoritmos (implementaciones) sin cambiar el código que los usa. | `IObjectStorage` con `AzureBlobStorage` y `AwsS3Storage`. La configuración decide cuál se usa. |
| **Aggregate** | Cluster de objetos de dominio que se tratan como una unidad. Solo el aggregate root recibe operaciones externas. | `Order` (raíz) contiene `OrderItem` (hijo). No puedes crear un `OrderItem` solo; usas `order.AddItem(...)`. |
| **Value Object** | Objeto definido por sus valores, no por su identidad. Inmutable. Dos instancias con los mismos valores son iguales. | `Money(100, "USD")` == `Money(100, "USD")`. No tienen `Id`. |
| **Domain Event** | Registro de algo que ocurrió en el dominio. Se publica dentro del agregado y se maneja en Application. | `OrderCreatedDomainEvent`, `OrderPaidDomainEvent`. |
| **Integration Event** | Evento que cruza fronteras entre microservicios. Viaja por el bus de mensajes. | `OrderCreatedIntegrationEvent`, `PaymentSucceededIntegrationEvent`. |
| **Idempotencia** | Una operación puede ejecutarse múltiples veces y el resultado es el mismo. | `PaymentSucceededConsumer` verifica `order.Status == Paid` antes de actuar. Si ya está pagado, no hace nada. |

### Principios SOLID

| Principio | Significado | Ejemplo |
|-----------|-----------|---------|
| **S** — Single Responsibility | Cada clase tiene una sola razón para cambiar. | `CreateOrderHandler` solo crea pedidos. `LoggingBehavior` solo loguea. |
| **O** — Open/Closed | Abierto para extensión, cerrado para modificación. | Agregar un nuevo behavior no modifica handlers existentes. |
| **L** — Liskov Substitution | Las subclases/implementaciones son intercambiables. | `AzureBlobStorage` y `AwsS3Storage` son intercambiables vía `IObjectStorage`. |
| **I** — Interface Segregation | Interfaces pequeñas y específicas, no una gigante. | `IOrderRepository` (4 métodos), `IUnitOfWork` (1 método). |
| **D** — Dependency Inversion | Depende de abstracciones, no de implementaciones concretas. | Los handlers dependen de `IOrderRepository`, no de `OrderRepository`. |

---

## 4. Arquitectura del Sistema

### Diagrama de Capas (por servicio)

```
┌─────────────────────────────────────────────┐
│                  API LAYER                  │  ← Program.cs, Endpoints, configuración
│  (Ordering.Api / Catalog.Api)               │
├─────────────────────────────────────────────┤
│             APPLICATION LAYER               │  ← Commands, Queries, Handlers, DTOs
│  (*.Application)                            │     Behaviors, Interfaces de repositorio
├─────────────────────────────────────────────┤
│            INFRASTRUCTURE LAYER             │  ← DbContext, Repositorios concretos
│  (*.Infrastructure)                         │     Configuración de EF Core, MongoDB
├─────────────────────────────────────────────┤
│               DOMAIN LAYER                  │  ← Entidades, Value Objects, Enums
│  (*.Domain)                                 │     Domain Events, Excepciones
└─────────────────────────────────────────────┘
```

**Regla de dependencias**: cada capa solo referencia a las capas interiores.
- Api → Application, Infrastructure
- Infrastructure → Application, Domain
- Application → Domain
- **Domain → NADIE** (es el centro, no depende de nada externo)

### Comunicación entre servicios

```
Ordering.Api ──publish──► [MassTransit Bus] ──consume──► Payment.Worker
                              │
Payment.Worker ──publish──►   │   ──consume──► Ordering.Api
```

Los servicios **nunca se llaman directamente** entre sí. Se comunican solo a través del bus de mensajes (MassTransit).

---

## 5. Clean Architecture — Las 4 Capas

### 5.1. Domain Layer (`*.Domain`)

**Qué contiene**: el modelo de negocio puro. Entidades, value objects, enums, excepciones de dominio, domain events.

**Reglas**:
- NO referencia a ninguna otra capa.
- NO tiene dependencias de frameworks (excepto `MediatR.Contracts` para `INotification`, que es solo una interfaz marker).
- NO tiene atributos de base de datos (`[BsonId]`, `[Table]`, etc.).
- Las propiedades tienen `private set`: solo se modifican a través de métodos de la clase.
- Los constructores son `private`: solo se crean instancias a través de métodos de fábrica (`Create`, `Restore`).

**Archivos clave**:

| Archivo | Contenido |
|---------|-----------|
| `Ordering.Domain/Orders/Order.cs` | Agregado raíz. Contiene `OrderItem`, despacha domain events, protege invariantes. |
| `Ordering.Domain/Orders/OrderItem.cs` | Entidad hija de Order. Constructor `internal` — solo Order puede crearla. |
| `Ordering.Domain/Orders/Money.cs` | Value object (record). Inmutable. Operaciones: `Add`, `Multiply`. |
| `Ordering.Domain/Orders/Address.cs` | Value object (record). Inmutable. Valida campos requeridos. |
| `Ordering.Domain/Orders/OrderStatus.cs` | Enum: `Pending`, `Paid`, `Cancelled`. |
| `Ordering.Domain/Events/OrderCreatedDomainEvent.cs` | Domain event publicado al crear un pedido. |
| `Ordering.Domain/DomainException.cs` | Excepción personalizada para violaciones de reglas de negocio. |
| `Catalog.Domain/Products/Product.cs` | Entidad Product con fábrica `Create`, `Restore`, `Update`. |

### 5.2. Application Layer (`*.Application`)

**Qué contiene**: los casos de uso. Define **qué** hace el sistema, pero no **cómo** accede a los datos.

**Contiene**:
- **Commands** (escritura): `CreateOrderCommand`, `PayOrderCommand`, etc.
- **Queries** (lectura): `GetOrderByIdQuery`, `ListOrdersQuery`, etc.
- **Handlers**: la lógica de cada caso de uso.
- **DTOs** (Data Transfer Objects): objetos que entran y salen por la API.
- **Interfaces de repositorio**: `IOrderRepository`, `IProductRepository` — el "puerto" que Infrastructure implementa.
- **Interfaces de infraestructura**: `IUnitOfWork`.
- **Pipeline Behaviors**: interceptores de MediatR (logging, validación).
- **Validators**: reglas de validación con FluentValidation.
- **Event Handlers**: handlers de domain events y consumers de MassTransit.

**Archivos clave**:

| Archivo | Contenido |
|---------|-----------|
| `Orders/Commands/CreateOrderCommand.cs` | Record que representa la intención de crear un pedido. Implementa `IRequest<OrderDto>`. |
| `Orders/Commands/CreateOrderHandler.cs` | Handler que ejecuta la creación: crea Address, Order, agrega items, persiste. |
| `Orders/IOrderRepository.cs` | Interfaz — define qué operaciones de datos necesita la aplicación. |
| `Orders/IUnitOfWork.cs` | Interfaz con un solo método: `SaveChangesAsync`. |
| `Orders/OrderDto.cs` | Records para la respuesta: `OrderDto`, `AddressDto`, `OrderItemDto`. |
| `Behaviors/LoggingBehavior.cs` | Pipeline behavior genérico que loguea cada request/response. |
| `Behaviors/ValidationBehavior.cs` | Pipeline behavior que ejecuta validadores antes del handler. |
| `DependencyInjection.cs` | Método `AddApplication()` que registra MediatR, validators y behaviors. |

### 5.3. Infrastructure Layer (`*.Infrastructure`)

**Qué contiene**: implementaciones concretas de las interfaces definidas en Application. Aquí vive el "cómo".

**Contiene**:
- **Repositorios concretos**: `OrderRepository` (EF Core), `MongoProductRepository` (MongoDB).
- **DbContext**: `OrderingDbContext` con configuración Fluent API.
- **Configuraciones de entidades**: `IEntityTypeConfiguration<T>` por cada entidad.
- **Migraciones de EF Core**: código generado que crea/modifica tablas.
- **Modelos de persistencia**: `ProductDocument` (separado de la entidad de dominio).

**Archivos clave (Ordering)**:

| Archivo | Contenido |
|---------|-----------|
| `Persistence/OrderingDbContext.cs` | DbContext + IUnitOfWork. Override de `SaveChangesAsync` para publicar domain events. |
| `Persistence/OrderRepository.cs` | Implementación con EF Core. Usa `Include(o => o.Items)` para eager loading. |
| `Persistence/Configurations/OrderConfiguration.cs` | Fluent API: tabla, owned types (Address), relación con Items, enum como string. |
| `Persistence/Configurations/OrderItemConfiguration.cs` | Fluent API: tabla, owned type (Money/UnitPrice). |
| `DependencyInjection.cs` | Método `AddInfrastructure()` que registra DbContext, repositorio y UnitOfWork. |

**Archivos clave (Catalog)**:

| Archivo | Contenido |
|---------|-----------|
| `Products/MongoProductRepository.cs` | Implementación con MongoDB.Driver. Traduce entre `Product` (dominio) y `ProductDocument` (persistencia). |
| `Products/ProductDocument.cs` | POCO con atributos de MongoDB (`[BsonId]`, `[BsonRepresentation]`). El dominio queda limpio. |
| `DependencyInjection.cs` | Registra `IMongoClient` (Singleton), `IMongoDatabase` (Singleton), `IProductRepository` (Scoped). |

### 5.4. API Layer (`*.Api`)

**Qué contiene**: la puerta de entrada. Compone todas las capas, define endpoints y configura middleware.

**Contiene**:
- **Program.cs**: punto de entrada. Configura DI, middleware, inicia la app.
- **Endpoints**: Minimal API endpoints agrupados con `MapGroup`.
- **Archivos de configuración**: `appsettings.json`, `launchSettings.json`.
- **Archivos .http**: para probar endpoints manualmente (en VS Code / Rider).

**Archivos clave**:

| Archivo | Contenido |
|---------|-----------|
| `Ordering.Api/Program.cs` | Configura: Serilog, Application, Infrastructure, MassTransit, health checks, exception handler. Aplica migraciones en Development. |
| `Ordering.Api/Endpoints/OrderEndpoints.cs` | 6 endpoints con `MapGroup("/orders")`. Cada uno llama a `mediator.Send(...)`. |
| `Catalog.Api/Program.cs` | Configura: Serilog, Application, Infrastructure, ObjectStorage, health checks, exception handler. |
| `Catalog.Api/Endpoints/ProductEndpoints.cs` | 5 endpoints CRUD con `MapGroup("/products")`. |

---

## 6. Domain-Driven Design (DDD)

### 6.1. Entidad (Entity)

Una entidad tiene **identidad** (`Id`). Dos entidades con los mismos datos pero distinto `Id` son diferentes.

```csharp
// Archivo: src/Services/Ordering/Ordering.Domain/Orders/Order.cs
public class Order
{
    public Guid Id { get; private set; }           // ← Identidad
    public string CustomerEmail { get; private set; }
    public OrderStatus Status { get; private set; }

    // Constructor PRIVADO: nadie puede hacer `new Order()`
    private Order(Guid id, string customerEmail, ...) { ... }

    // Factory method: la ÚNICA forma de crear un Order válido
    public static Order Create(string customerEmail, Address shippingAddress)
    {
        // Valida ANTES de crear
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new DomainException("El email del cliente es obligatorio.");

        var order = new Order(Guid.NewGuid(), customerEmail.Trim(), ...);
        // Registra que algo pasó en el dominio
        order._domainEvents.Add(new OrderCreatedDomainEvent(order.Id, order.CustomerEmail));
        return order;
    }

    // Rehidratación desde base de datos (NO valida, porque ya fue validado al crear)
    public static Order Restore(Guid id, ...) => new Order(id, ...);
}
```

**¿Por qué `private set`?** Impide que código externo modifique el estado directamente:
```csharp
// ❌ PROHIBIDO: order.Status = OrderStatus.Paid;
// ✅ CORRECTO:  order.MarkAsPaid();  ← Pasa por validación
```

**¿Por qué constructor privado + factory method?** Para que sea **imposible** crear un objeto en estado inválido.

### 6.2. Value Object

Un value object se define por sus **valores**, no tiene identidad. Es **inmutable**.

```csharp
// Archivo: src/Services/Ordering/Ordering.Domain/Orders/Money.cs
public sealed record Money   // ← 'record' da igualdad estructural automática
{
    public decimal Amount { get; private init; }   // ← init: solo se asigna en construcción
    public string Currency { get; private init; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency) => new(amount, currency);

    // Operaciones que devuelven NUEVAS instancias (inmutabilidad)
    public Money Add(Money other) => new(Amount + other.Amount, Currency);
    public Money Multiply(int quantity) => new(Amount * quantity, Currency);
}
```

**¿Cuándo usar record?** Cuando la igualdad es por valores:
```csharp
var a = Money.Create(100, "USD");
var b = Money.Create(100, "USD");
a == b  // ✅ TRUE — porque son records
```

### 6.3. Agregado (Aggregate)

Un agregado es un **cluster de entidades** con una raíz que protege invariantes. El mundo externo solo interactúa con la raíz.

```
Order (Aggregate Root)
  ├── OrderItem  ← solo se crea via order.AddItem(...)
  ├── OrderItem
  └── OrderItem
```

```csharp
// Solo Order puede crear OrderItems
public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
{
    // INVARIANTE: solo se agregan items si el pedido está pendiente
    if (Status != OrderStatus.Pending)
        throw new DomainException("Solo se pueden agregar items a un pedido pendiente.");

    var item = OrderItem.Create(productId, productName, unitPrice, quantity);
    _items.Add(item);
}
```

`OrderItem.Create` es `internal` — solo código dentro del mismo assembly (Ordering.Domain) puede llamarlo.

### 6.4. Domain Events

Hechos que ocurrieron dentro del agregado:

```csharp
// Archivo: src/Services/Ordering/Ordering.Domain/Events/OrderCreatedDomainEvent.cs
public sealed record OrderCreatedDomainEvent(Guid OrderId, string CustomerEmail) : IDomainEvent;
```

Se publican en `SaveChangesAsync()` del `OrderingDbContext`:
```csharp
// 1. Recolecta events de todas las entidades modificadas
var domainEvents = ChangeTracker.Entries<Order>()
    .SelectMany(e => e.Entity.DomainEvents).ToList();
// 2. Limpia events de las entidades
domainEntities.ForEach(e => e.ClearDomainEvents());
// 3. Persiste en BD
var result = await base.SaveChangesAsync(cancellationToken);
// 4. Publica events (MediatR los enruta a los handlers)
foreach (var domainEvent in domainEvents)
    await mediator.Publish(domainEvent, cancellationToken);
```

### 6.5. Transiciones de Estado (State Machine)

```
          ┌──────────┐
    ┌────►│ Pending  │────┐
    │     └──────────┘    │
    │          │          │
    │   MarkAsPaid()  Cancel()
    │          │          │
    │     ┌────▼───┐  ┌───▼──────┐
    │     │  Paid  │  │Cancelled │
    │     └────────┘  └──────────┘
    │
 Create()
```

Transiciones inválidas lanzan `DomainException`:
- `Paid → Cancel()` → "No se puede cancelar un pedido ya pagado."
- `Cancelled → MarkAsPaid()` → "Solo se puede pagar un pedido pendiente."
- `Paid → AddItem()` → "Solo se pueden agregar items a un pedido pendiente."

---

## 7. CQRS y MediatR

### 7.1. ¿Qué es CQRS?

**C**ommand **Q**uery **R**esponsibility **S**egregation: separar las operaciones de escritura (Commands) de las de lectura (Queries). Cada una tiene su propio handler.

```
                  ┌──────────────────────┐
                  │      IMediator       │
                  │    mediator.Send()   │
                  └──────┬───────────────┘
                         │
           ┌─────────────┴─────────────┐
           │                           │
    ┌──────▼──────┐           ┌────────▼───────┐
    │  Command    │           │    Query       │
    │  (escritura)│           │   (lectura)    │
    └──────┬──────┘           └────────┬───────┘
           │                           │
    ┌──────▼──────┐           ┌────────▼───────┐
    │  Handler    │           │    Handler     │
    │  (modifica) │           │  (solo lee)    │
    └─────────────┘           └────────────────┘
```

### 7.2. Anatomía de un Command

```csharp
// 1. DEFINIR el command (qué datos necesita)
// Archivo: Ordering.Application/Orders/Commands/CreateOrderCommand.cs
public record CreateOrderCommand(
    string CustomerEmail,
    string Street, string City, string State, string Country, string ZipCode,
    List<CreateOrderItemDto> Items
) : IRequest<OrderDto>;   // ← IRequest<T> = "devuelve un OrderDto"

// 2. IMPLEMENTAR el handler (qué hace)
// Archivo: Ordering.Application/Orders/Commands/CreateOrderHandler.cs
internal sealed class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(request.Street, request.City, ...);
        var order = Order.Create(request.CustomerEmail, address);

        foreach (var item in request.Items)
            order.AddItem(item.ProductId, item.ProductName, Money.Create(item.UnitPrice, item.Currency), item.Quantity);

        await repository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);  // ← persiste + publica domain events

        return OrderMapper.ToDto(order);
    }
}
```

### 7.3. Anatomía de una Query

```csharp
// Archivo: Ordering.Application/Orders/Queries/GetOrderByIdQuery.cs
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

// Archivo: Ordering.Application/Orders/Queries/GetOrderByIdHandler.cs
internal sealed class GetOrderByIdHandler(IOrderRepository repository)
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        return order is null ? null : OrderMapper.ToDto(order);
    }
}
```

### 7.4. Pipeline Behaviors

Son **interceptores** que se ejecutan **antes y después** de cada handler, para TODOS los requests. Se registran una vez y aplican globalmente.

```
Request → LoggingBehavior → ValidationBehavior → Handler → Response
                                    ↑
                            Si la validación falla,
                            lanza ValidationException
                            y NUNCA llega al Handler
```

```csharp
// Archivo: Ordering.Application/Behaviors/ValidationBehavior.cs
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Ejecuta TODOS los validadores registrados para este tipo de request
        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);  // ← Corta la cadena. No llega al handler.

        return await next(ct);  // ← Continúa al siguiente behavior o al handler
    }
}
```

### 7.5. Registro en DI

```csharp
// Archivo: Ordering.Application/DependencyInjection.cs
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);      // ← Escanea y registra todos los handlers
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); // ← Interceptor de logging
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // ← Interceptor de validación
});
services.AddValidatorsFromAssembly(assembly);  // ← Registra todos los AbstractValidator<T>
```

---

## 8. Entity Framework Core — Conceptos Clave

### 8.1. DbContext

Es la "sesión" con la base de datos. Rastrea cambios (Change Tracking) y genera SQL.

```csharp
// Archivo: Ordering.Infrastructure/Persistence/OrderingDbContext.cs
public class OrderingDbContext(DbContextOptions<OrderingDbContext> options, IMediator mediator)
    : DbContext(options), IUnitOfWork    // ← Implementa dos interfaces: DbContext y UnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();           // ← Tabla "Orders"
    public DbSet<OrderItem> OrderItems => Set<OrderItem>(); // ← Tabla "OrderItems"
}
```

### 8.2. Fluent API (IEntityTypeConfiguration)

Configura el mapeo entre clases C# y tablas SQL **sin atributos**. Mantiene el dominio limpio.

```csharp
// Archivo: Ordering.Infrastructure/Persistence/Configurations/OrderConfiguration.cs
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");                  // ← Nombre de tabla
        builder.HasKey(o => o.Id);                  // ← Primary key

        builder.Property(o => o.CustomerEmail)
            .IsRequired().HasMaxLength(256);         // ← NOT NULL, VARCHAR(256)

        builder.Property(o => o.Status)
            .HasConversion<string>()                 // ← Guarda "Pending"/"Paid" en vez de 0/1
            .HasMaxLength(20);

        // Owned Type: Address se guarda como columnas de la tabla Orders
        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.Street).HasColumnName("ShippingStreet").HasMaxLength(256);
            a.Property(p => p.City).HasColumnName("ShippingCity").HasMaxLength(128);
            // ... más columnas
        });

        // Relación 1:N con OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")                // ← Shadow property (no existe en C#, solo en BD)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignorar propiedades que NO se persisten
        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.TotalAmount);          // ← Calculado en memoria, no en BD
    }
}
```

### 8.3. Owned Types

Un owned type es un value object que se guarda como **columnas** en la tabla del dueño, no en una tabla separada.

```
Tabla "Orders":
┌────┬───────────────┬─────────┬───────────────┬──────────────┬──────────────┬───────────────┐
│ Id │ CustomerEmail │ Status  │ ShippingStreet│ ShippingCity │ShippingState │ShippingCountry│
└────┴───────────────┴─────────┴───────────────┴──────────────┴──────────────┴───────────────┘
                                ↑ ← Estas columnas SON el Address (owned type)

Tabla "OrderItems":
┌────┬─────────┬───────────┬──────────────────┬──────────┬──────────────────┐
│ Id │ OrderId │ ProductName│ UnitPriceAmount │ Quantity │ UnitPriceCurrency│
└────┴─────────┴───────────┴──────────────────┴──────────┴──────────────────┘
                              ↑ ← Money es owned type de OrderItem
```

### 8.4. AsNoTracking

```csharp
// Archivo: Ordering.Infrastructure/Persistence/OrderRepository.cs
public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct)
{
    return await context.Orders
        .Include(o => o.Items)   // ← Eager loading: carga Items en la misma query (evita N+1)
        .AsNoTracking()          // ← NO rastrea cambios. Más rápido para solo lectura.
        .ToListAsync(ct);
}
```

**Sin `AsNoTracking`**: EF Core guarda una referencia de cada entidad leída. Al llamar `SaveChanges`, compara el estado original con el actual y genera UPDATE. Esto consume memoria.

**Con `AsNoTracking`**: EF Core NO guarda referencias. Es más rápido y usa menos memoria. Pero no puedes hacer `SaveChanges` sobre esas entidades.

### 8.5. Migraciones

Las migraciones son **código C# auto-generado** que crea/modifica tablas en la base de datos.

```bash
# Crear migración (genera código en Persistence/Migrations/)
dotnet ef migrations add InitialCreate \
  --project src/Services/Ordering/Ordering.Infrastructure \
  --startup-project src/Services/Ordering/Ordering.Api \
  --output-dir Persistence/Migrations

# Aplicar migración (ejecuta el SQL contra la BD)
dotnet ef database update \
  --project src/Services/Ordering/Ordering.Infrastructure \
  --startup-project src/Services/Ordering/Ordering.Api
```

En desarrollo, se aplican automáticamente en `Program.cs`:
```csharp
await db.Database.MigrateAsync();  // ← Equivale a "ef database update"
```

---

## 9. MongoDB — Conceptos Clave

### 9.1. Modelo de Persistencia Separado

En MongoDB, la entidad de dominio (`Product`) NO tiene atributos de persistencia. Se usa un modelo separado (`ProductDocument`):

```csharp
// Archivo: Catalog.Infrastructure/Products/ProductDocument.cs
public class ProductDocument
{
    [BsonId]                                   // ← Este campo es el _id del documento
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)] // ← Precisión decimal completa
    public decimal Price { get; set; }
}
```

### 9.2. Repositorio con Traducción

```csharp
// Archivo: Catalog.Infrastructure/Products/MongoProductRepository.cs
public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductDocument> _collection;

    // Traducción Dominio → Persistencia
    private static ProductDocument ToDocument(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price
    };

    // Traducción Persistencia → Dominio
    private static Product ToDomain(ProductDocument d) =>
        Product.Restore(d.Id, d.Name, d.Description, d.Price);
}
```

### 9.3. Registro del GuidSerializer

```csharp
// Archivo: Catalog.Infrastructure/DependencyInjection.cs
BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
// ← Sin esto, MongoDB guarda los Guid en formato binario legacy.
// Con Standard, se guardan como UUID estándar RFC 4122.
```

---

## 10. Event-Driven Architecture y MassTransit

### 10.1. Flujo Completo de Eventos

```
1. Cliente → POST /orders
2. CreateOrderHandler → Order.Create() → [OrderCreatedDomainEvent añadido a la lista]
3. unitOfWork.SaveChangesAsync() → persiste en PostgreSQL
4. OrderingDbContext.SaveChangesAsync() → mediator.Publish(OrderCreatedDomainEvent)
5. OrderCreatedDomainEventHandler → publishEndpoint.Publish(OrderCreatedIntegrationEvent)
6. MassTransit Bus → entrega el evento a Payment.Worker
7. OrderCreatedConsumer → simula pago → publishEndpoint.Publish(PaymentSucceededIntegrationEvent)
8. MassTransit Bus → entrega el evento de vuelta a Ordering
9. PaymentSucceededConsumer → order.MarkAsPaid() → SaveChangesAsync()
```

### 10.2. Domain Event vs Integration Event

| Aspecto | Domain Event | Integration Event |
|---------|-------------|-------------------|
| **Alcance** | Dentro de un bounded context | Cruza fronteras entre servicios |
| **Transporte** | MediatR (in-process) | MassTransit (bus de mensajes) |
| **Formato** | `INotification` de MediatR | POCO record compartido en BuildingBlocks |
| **Ejemplo** | `OrderCreatedDomainEvent` | `OrderCreatedIntegrationEvent` |
| **Dónde se define** | `Ordering.Domain/Events/` | `BuildingBlocks/Events/` |

### 10.3. Consumer de MassTransit

```csharp
// Archivo: src/Services/Payment/Payment.Worker/Consumers/OrderCreatedConsumer.cs
public sealed class OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreatedIntegrationEvent>  // ← Interfaz de MassTransit
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var orderId = context.Message.OrderId;  // ← Accede al mensaje

        await Task.Delay(500, context.CancellationToken);  // Simula procesamiento

        // Publica otro evento como resultado
        await context.Publish(new PaymentSucceededIntegrationEvent(orderId, DateTime.UtcNow));
    }
}
```

### 10.4. Configuración In-Memory

```csharp
// Archivo: Ordering.Api/Program.cs
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentSucceededConsumer>();  // ← Registra el consumer

    x.UsingInMemory((context, cfg) =>          // ← Transporte in-memory (sin RabbitMQ/SB)
    {
        cfg.ConfigureEndpoints(context);        // ← Auto-configura endpoints por convención
    });
});
```

**Para cambiar a Azure Service Bus**, solo reemplazas `UsingInMemory` por `UsingAzureServiceBus`:
```csharp
x.UsingAzureServiceBus((context, cfg) =>
{
    cfg.Host("connection-string");
    cfg.ConfigureEndpoints(context);
});
```

### 10.5. Idempotencia

```csharp
// Archivo: Ordering.Application/Orders/EventHandlers/PaymentSucceededHandler.cs
public async Task Consume(ConsumeContext<PaymentSucceededIntegrationEvent> context)
{
    var order = await repository.GetByIdAsync(context.Message.OrderId, ct);

    // ✅ Idempotente: si ya está pagada, no hacer nada
    if (order.Status == OrderStatus.Paid)
    {
        logger.LogInformation("Order ya estaba pagada (idempotente)");
        return;  // ← No lanza excepción, simplemente no hace nada
    }

    order.MarkAsPaid();
    await unitOfWork.SaveChangesAsync(ct);
}
```

**¿Por qué?** En mensajería, un mensaje puede entregarse más de una vez (at-least-once delivery). Sin idempotencia, el pedido se "pagaría" dos veces.

---

## 11. Inyección de Dependencias (DI)

### 11.1. Lifetimes (Ciclos de Vida)

| Lifetime | Cuándo se crea | Cuándo se destruye | Uso |
|----------|---------------|-------------------|-----|
| **Singleton** | Primera vez que se resuelve | Cuando la app se cierra | Servicios thread-safe y costosos: `IMongoClient`, `BlobServiceClient` |
| **Scoped** | Una vez por request HTTP | Al finalizar el request | `DbContext`, repositorios, `IUnitOfWork` |
| **Transient** | Cada vez que se resuelve | Después de usarse | Servicios ligeros sin estado |

### 11.2. Patrón DependencyInjection.cs

Cada capa tiene un `DependencyInjection.cs` con un método de extensión. `Program.cs` solo los llama:

```csharp
// Program.cs (Api) — limpio, solo conecta capas
builder.Services.AddApplication();                     // ← Catalog.Application o Ordering.Application
builder.Services.AddInfrastructure(builder.Configuration); // ← Catalog.Infrastructure o Ordering.Infrastructure
```

```csharp
// Infrastructure/DependencyInjection.cs — sabe qué registrar
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(connectionString));
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderingDbContext>());
    return services;
}
```

`AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderingDbContext>())` — Esto hace que `IUnitOfWork` y `OrderingDbContext` sean **la misma instancia** dentro del mismo scope. Es el mismo objeto detrás de dos interfaces.

---

## 12. Diccionario de Datos

### 12.1. Ordering — Tabla `Orders` (PostgreSQL)

| Columna | Tipo SQL | Nullable | Origen en C# | Descripción |
|---------|---------|----------|---------------|-------------|
| `Id` | `uuid` | NO (PK) | `Order.Id` | Identificador único del pedido |
| `CustomerEmail` | `varchar(256)` | NO | `Order.CustomerEmail` | Email del cliente |
| `Status` | `varchar(20)` | NO | `Order.Status` (enum→string) | Estado: "Pending", "Paid", "Cancelled" |
| `CreatedAt` | `timestamp` | NO | `Order.CreatedAt` | Fecha/hora de creación (UTC) |
| `ShippingStreet` | `varchar(256)` | NO | `Order.ShippingAddress.Street` | Calle (owned type Address) |
| `ShippingCity` | `varchar(128)` | NO | `Order.ShippingAddress.City` | Ciudad |
| `ShippingState` | `varchar(128)` | SI | `Order.ShippingAddress.State` | Estado/provincia |
| `ShippingCountry` | `varchar(128)` | NO | `Order.ShippingAddress.Country` | País |
| `ShippingZipCode` | `varchar(20)` | SI | `Order.ShippingAddress.ZipCode` | Código postal |

### 12.2. Ordering — Tabla `OrderItems` (PostgreSQL)

| Columna | Tipo SQL | Nullable | Origen en C# | Descripción |
|---------|---------|----------|---------------|-------------|
| `Id` | `uuid` | NO (PK) | `OrderItem.Id` | Identificador único del item |
| `OrderId` | `uuid` | NO (FK) | Shadow property | Foreign key a `Orders.Id` |
| `ProductId` | `uuid` | NO | `OrderItem.ProductId` | ID del producto (referencia a Catalog) |
| `ProductName` | `varchar(256)` | NO | `OrderItem.ProductName` | Nombre del producto (desnormalizado) |
| `UnitPriceAmount` | `decimal(18,2)` | NO | `OrderItem.UnitPrice.Amount` | Precio unitario (owned type Money) |
| `UnitPriceCurrency` | `varchar(3)` | NO | `OrderItem.UnitPrice.Currency` | Moneda: "USD", "MXN" |
| `Quantity` | `int` | NO | `OrderItem.Quantity` | Cantidad |

### 12.3. Catalog — Colección `products` (MongoDB)

| Campo | Tipo BSON | Origen en C# | Descripción |
|-------|-----------|---------------|-------------|
| `_id` | `UUID` | `ProductDocument.Id` | Identificador único (GuidRepresentation.Standard) |
| `Name` | `string` | `ProductDocument.Name` | Nombre del producto |
| `Description` | `string` | `ProductDocument.Description` | Descripción |
| `Price` | `Decimal128` | `ProductDocument.Price` | Precio con precisión decimal |

### 12.4. DTOs (Data Transfer Objects)

| DTO | Campos | Usado en |
|-----|--------|----------|
| `ProductDto` | `Id`, `Name`, `Description`, `Price` | Respuesta de GET/POST en Catalog |
| `CreateProductRequest` | `Name`, `Description`, `Price` | Body de POST /products |
| `UpdateProductRequest` | `Name`, `Description`, `Price` | Body de PUT /products/{id} |
| `OrderDto` | `Id`, `CustomerEmail`, `Status`, `TotalAmount`, `Currency`, `CreatedAt`, `ShippingAddress`, `Items` | Respuesta de GET/POST en Ordering |
| `AddressDto` | `Street`, `City`, `State`, `Country`, `ZipCode` | Sub-objeto dentro de OrderDto |
| `OrderItemDto` | `Id`, `ProductId`, `ProductName`, `UnitPrice`, `Quantity`, `Subtotal` | Sub-objeto dentro de OrderDto |
| `CreateOrderItemDto` | `ProductId`, `ProductName`, `UnitPrice`, `Currency`, `Quantity` | Dentro de CreateOrderCommand |
| `AddOrderItemRequest` | `ProductId`, `ProductName`, `UnitPrice`, `Currency`, `Quantity` | Body de POST /orders/{id}/items |

### 12.5. Integration Events

| Evento | Campos | Publicado por | Consumido por |
|--------|--------|--------------|---------------|
| `OrderCreatedIntegrationEvent` | `OrderId`, `CustomerEmail`, `TotalAmount`, `Currency`, `CreatedAt` | Ordering (domain event handler) | Payment.Worker |
| `PaymentSucceededIntegrationEvent` | `OrderId`, `PaidAt` | Payment.Worker | Ordering (PaymentSucceededConsumer) |
| `PaymentFailedIntegrationEvent` | `OrderId`, `Reason`, `FailedAt` | Payment.Worker | (sin consumer aún) |

---

## 13. Mapa de Archivos — Referencia Rápida

```
ShopHub/
├── ShopHub.slnx                          ← Solución (formato XML nuevo de .NET)
├── docker-compose.yml                    ← MongoDB + PostgreSQL
├── azure-pipelines.yml                   ← CI/CD pipeline
├── README.md                             ← Documentación general
├── CHEATSHEET-ENTREVISTA.md              ← Preguntas y respuestas para entrevista
│
├── src/
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.csproj
│   │   ├── Events/
│   │   │   ├── OrderCreatedIntegrationEvent.cs
│   │   │   ├── PaymentSucceededIntegrationEvent.cs
│   │   │   └── PaymentFailedIntegrationEvent.cs
│   │   ├── Storage/
│   │   │   ├── IObjectStorage.cs              ← Abstracción multi-cloud
│   │   │   ├── AzureBlobStorage.cs            ← Implementación Azure
│   │   │   ├── AwsS3Storage.cs                ← Implementación AWS
│   │   │   └── StorageDependencyInjection.cs  ← Factory por configuración
│   │   └── Exceptions/
│   │       └── GlobalExceptionHandler.cs      ← IExceptionHandler centralizado
│   │
│   ├── Services/
│   │   ├── Catalog/
│   │   │   ├── Catalog.Domain/
│   │   │   │   ├── Products/Product.cs        ← Entidad con Create/Restore/Update
│   │   │   │   └── DomainException.cs
│   │   │   ├── Catalog.Application/
│   │   │   │   ├── Products/
│   │   │   │   │   ├── IProductRepository.cs  ← Puerto (interfaz)
│   │   │   │   │   ├── ProductDtos.cs         ← Records para API
│   │   │   │   │   ├── Commands/*.cs          ← Create, Update, Delete
│   │   │   │   │   └── Queries/*.cs           ← GetById, GetAll
│   │   │   │   ├── Behaviors/*.cs             ← Logging, Validation
│   │   │   │   └── DependencyInjection.cs
│   │   │   ├── Catalog.Infrastructure/
│   │   │   │   ├── Products/
│   │   │   │   │   ├── MongoProductRepository.cs  ← Implementación MongoDB
│   │   │   │   │   ├── ProductDocument.cs         ← Modelo de persistencia
│   │   │   │   │   └── InMemoryProductRepository.cs
│   │   │   │   └── DependencyInjection.cs
│   │   │   └── Catalog.Api/
│   │   │       ├── Program.cs                 ← Composición
│   │   │       ├── Endpoints/ProductEndpoints.cs
│   │   │       └── Dockerfile
│   │   │
│   │   ├── Ordering/
│   │   │   ├── Ordering.Domain/
│   │   │   │   ├── Orders/
│   │   │   │   │   ├── Order.cs               ← Agregado raíz
│   │   │   │   │   ├── OrderItem.cs           ← Entidad hija
│   │   │   │   │   ├── Money.cs               ← Value object (record)
│   │   │   │   │   ├── Address.cs             ← Value object (record)
│   │   │   │   │   └── OrderStatus.cs         ← Enum
│   │   │   │   ├── Events/
│   │   │   │   │   ├── OrderCreatedDomainEvent.cs
│   │   │   │   │   └── OrderPaidDomainEvent.cs
│   │   │   │   ├── IDomainEvent.cs            ← Marker interface
│   │   │   │   └── DomainException.cs
│   │   │   ├── Ordering.Application/
│   │   │   │   ├── Orders/
│   │   │   │   │   ├── IOrderRepository.cs
│   │   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   │   ├── OrderDto.cs
│   │   │   │   │   ├── OrderMapper.cs
│   │   │   │   │   ├── Commands/             ← Create, AddItem, Pay, Cancel
│   │   │   │   │   ├── Queries/              ← GetById, List
│   │   │   │   │   └── EventHandlers/
│   │   │   │   │       ├── OrderCreatedDomainEventHandler.cs  ← Publica integration event
│   │   │   │   │       └── PaymentSucceededHandler.cs         ← Consume integration event
│   │   │   │   ├── Behaviors/
│   │   │   │   └── DependencyInjection.cs
│   │   │   ├── Ordering.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── OrderingDbContext.cs    ← DbContext + UnitOfWork + domain events
│   │   │   │   │   ├── OrderRepository.cs
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   ├── OrderConfiguration.cs     ← Fluent API
│   │   │   │   │   │   └── OrderItemConfiguration.cs
│   │   │   │   │   └── Migrations/
│   │   │   │   └── DependencyInjection.cs
│   │   │   └── Ordering.Api/
│   │   │       ├── Program.cs
│   │   │       ├── Endpoints/OrderEndpoints.cs
│   │   │       ├── appsettings.json
│   │   │       ├── orders.http
│   │   │       └── Dockerfile
│   │   │
│   │   └── Payment/
│   │       └── Payment.Worker/
│   │           ├── Program.cs
│   │           ├── Consumers/OrderCreatedConsumer.cs
│   │           └── Dockerfile
│
└── tests/
    ├── Catalog.Domain.Tests/ProductTests.cs        ← 5 tests
    ├── Ordering.Domain.Tests/
    │   ├── OrderTests.cs                            ← 12 tests
    │   ├── MoneyTests.cs                            ← 6 tests
    │   └── AddressTests.cs                          ← 5 tests + 1 de igualdad
    ├── Ordering.Infrastructure.Tests/
    │   ├── PostgresFixture.cs                       ← Testcontainers setup
    │   └── OrderRepositoryTests.cs                  ← 3 tests de integración
    └── Payment.Worker.Tests/
        └── OrderCreatedConsumerTests.cs             ← 2 tests MassTransit harness
```

---

## 14. API Reference — Endpoints

### Catalog API (puerto 5100)

| Método | Ruta | MediatR Request | Respuesta |
|--------|------|-----------------|-----------|
| `GET` | `/products` | `GetProductsQuery` | `200 OK` — `ProductDto[]` |
| `GET` | `/products/{id}` | `GetProductByIdQuery` | `200 OK` / `404 Not Found` |
| `POST` | `/products` | `CreateProductCommand` | `201 Created` — `ProductDto` |
| `PUT` | `/products/{id}` | `UpdateProductCommand` | `204 No Content` / `404` |
| `DELETE` | `/products/{id}` | `DeleteProductCommand` | `204 No Content` / `404` |
| `GET` | `/health` | — | `200 OK` — "Healthy" |

### Ordering API (puerto 5102)

| Método | Ruta | MediatR Request | Respuesta |
|--------|------|-----------------|-----------|
| `GET` | `/orders` | `ListOrdersQuery` | `200 OK` — `OrderDto[]` |
| `GET` | `/orders/{id}` | `GetOrderByIdQuery` | `200 OK` / `404` |
| `POST` | `/orders` | `CreateOrderCommand` | `201 Created` — `OrderDto` |
| `POST` | `/orders/{id}/items` | `AddOrderItemCommand` | `200 OK` — `OrderDto` |
| `POST` | `/orders/{id}/pay` | `PayOrderCommand` | `200 OK` — `OrderDto` |
| `POST` | `/orders/{id}/cancel` | `CancelOrderCommand` | `200 OK` — `OrderDto` |
| `GET` | `/health` | — | `200 OK` — "Healthy" |

### Ejemplo de Minimal API

```csharp
// Archivo: Ordering.Api/Endpoints/OrderEndpoints.cs
group.MapPost("/", async (CreateOrderCommand command, IMediator mediator, CancellationToken ct) =>
{
    var order = await mediator.Send(command, ct);    // ← Envía al handler
    return Results.Created($"/orders/{order.Id}", order);
});
```

**`Results`** es la clase estática de ASP.NET Core que genera respuestas HTTP tipadas:
- `Results.Ok(data)` → 200
- `Results.Created(url, data)` → 201
- `Results.NoContent()` → 204
- `Results.NotFound()` → 404
- `Results.BadRequest(error)` → 400

---

## 15. Flujo de Ejecución Paso a Paso

### Ejemplo: Crear un pedido (`POST /orders`)

```
1. HTTP Request llega a Ordering.Api
   ↓
2. ASP.NET Core deserializa el JSON body a CreateOrderCommand (record)
   ↓
3. El endpoint llama: mediator.Send(command, ct)
   ↓
4. MediatR ejecuta la CADENA de pipeline behaviors:
   a. LoggingBehavior: loguea "Handling CreateOrderCommand: {data}"
   b. ValidationBehavior: ejecuta CreateOrderValidator
      - Si falla → lanza ValidationException → GlobalExceptionHandler → 400
      - Si pasa → continúa
   ↓
5. MediatR despacha al CreateOrderHandler.Handle()
   a. Address.Create(...) → valida campos, crea value object
   b. Order.Create(...) → valida email, crea entidad, genera OrderCreatedDomainEvent
   c. order.AddItem(...) → valida cantidad > 0 y status == Pending
   d. repository.AddAsync(order) → EF Core añade al change tracker
   e. unitOfWork.SaveChangesAsync() → OrderingDbContext:
      i.   Recolecta domain events de Order
      ii.  Limpia domain events
      iii. base.SaveChangesAsync() → INSERT INTO Orders + OrderItems (PostgreSQL)
      iv.  Publica domain events via mediator.Publish()
   ↓
6. OrderCreatedDomainEventHandler se activa:
   a. Lee el Order desde el repositorio
   b. Crea OrderCreatedIntegrationEvent
   c. publishEndpoint.Publish(...) → MassTransit envía al bus
   ↓
7. MassTransit despacha a OrderCreatedConsumer (Payment.Worker):
   a. Simula procesamiento de pago
   b. Publica PaymentSucceededIntegrationEvent
   ↓
8. MassTransit despacha a PaymentSucceededConsumer (Ordering):
   a. Lee Order
   b. Verifica idempotencia (no está ya pagado)
   c. order.MarkAsPaid() → genera OrderPaidDomainEvent
   d. SaveChangesAsync() → UPDATE Orders SET Status = 'Paid'
   ↓
9. El handler original retorna OrderDto
   ↓
10. LoggingBehavior loguea "Handled CreateOrderCommand"
    ↓
11. Endpoint devuelve Results.Created($"/orders/{order.Id}", orderDto)
    ↓
12. ASP.NET Core serializa a JSON y envía HTTP 201
```

---

## 16. Testing — Estrategia y Herramientas

### 16.1. Tests Unitarios (sin dependencias externas)

Prueban reglas de dominio en aislamiento total. No necesitan base de datos, red ni Docker.

```csharp
// Archivo: tests/Ordering.Domain.Tests/OrderTests.cs
[Fact]   // ← Atributo de xUnit que marca un test
public void AddItem_APedidoPagado_LanzaDomainException()
{
    // Arrange
    var order = Order.Create("test@mail.com", DefaultAddress);
    order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(100m, "USD"), 1);
    order.MarkAsPaid();

    // Act
    var act = () => order.AddItem(Guid.NewGuid(), "Mouse", Money.Create(50m, "USD"), 1);

    // Assert (FluentAssertions)
    act.Should().Throw<DomainException>();  // ← Legible: "la acción debería lanzar DomainException"
}
```

**Convenciones de nombres**: `MetodoQueSePrueba_Escenario_ResultadoEsperado`.

### 16.2. Tests de Integración (con Testcontainers)

Prueban la capa de persistencia contra una base de datos real.

```csharp
// Archivo: tests/Ordering.Infrastructure.Tests/PostgresFixture.cs
public class PostgresFixture : IAsyncLifetime  // ← xUnit llama InitializeAsync/DisposeAsync
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();          // ← Levanta un contenedor PostgreSQL real
        // ... configura DbContext con connection string del contenedor
        await DbContext.Database.MigrateAsync(); // ← Aplica migraciones
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();         // ← Destruye el contenedor
    }
}
```

**`IClassFixture<PostgresFixture>`** — xUnit comparte la misma instancia del fixture entre todos los tests de la clase. El contenedor se levanta una vez.

### 16.3. Tests de MassTransit (InMemoryTestHarness)

```csharp
// Archivo: tests/Payment.Worker.Tests/OrderCreatedConsumerTests.cs
[Fact]
public async Task Consume_ConMontoMenorALimite_PublicaPaymentSucceeded()
{
    await using var provider = new ServiceCollection()
        .AddMassTransitTestHarness(cfg => cfg.AddConsumer<OrderCreatedConsumer>())
        .AddLogging()
        .BuildServiceProvider(true);

    var harness = provider.GetRequiredService<ITestHarness>();
    await harness.Start();

    await harness.Bus.Publish(new OrderCreatedIntegrationEvent(...));

    // Verifica que el consumer procesó el mensaje
    (await harness.Consumed.Any<OrderCreatedIntegrationEvent>()).Should().BeTrue();
    // Verifica que publicó el resultado
    (await harness.Published.Any<PaymentSucceededIntegrationEvent>()).Should().BeTrue();
}
```

### 16.4. Resumen de Tests

| Proyecto | # Tests | Tipo | Qué prueba |
|----------|---------|------|-----------|
| Catalog.Domain.Tests | 5 | Unitario | Product: Create, validación, Update, Restore |
| Ordering.Domain.Tests | 24 | Unitario | Order: invariantes, transiciones. Money: operaciones. Address: validación, igualdad |
| Ordering.Infrastructure.Tests | 3 | Integración | OrderRepository contra PostgreSQL real (Testcontainers) |
| Payment.Worker.Tests | 2 | Integración | Consumer de MassTransit con InMemoryTestHarness |
| **Total** | **34** | | |

---

## 17. Infraestructura y DevOps

### 17.1. Docker Compose

```yaml
# Archivo: docker-compose.yml
services:
  mongo:
    image: mongo:7
    container_name: shophub-mongo
    ports: ["27017:27017"]     # ← host:contenedor
    volumes: [mongo-data:/data/db]

  postgres:
    image: postgres:15
    container_name: shophub-postgres
    ports: ["5433:5432"]       # ← puerto host 5433 (5432 ya está ocupado)
    environment:
      POSTGRES_USER: shophub
      POSTGRES_PASSWORD: shophub123
      POSTGRES_DB: ShopHubOrdering
```

### 17.2. Dockerfile Multi-Stage

```dockerfile
# Archivo: src/Services/Ordering/Ordering.Api/Dockerfile

# Stage 1: Base de runtime (imagen ligera, sin SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Compilación (imagen con SDK completo)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Ordering/Ordering.Api/Ordering.Api.csproj", "..."]
RUN dotnet restore    # ← Solo descarga paquetes (cacheable)
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Imagen final (solo runtime + DLLs compilados)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
```

**¿Por qué multi-stage?** La imagen final solo tiene el runtime (~200MB), no el SDK (~800MB).

### 17.3. Azure Pipelines

```yaml
# Archivo: azure-pipelines.yml
stages:
  - stage: Build_Test
    jobs:
      - job: BuildAndTest
        steps:
          - UseDotNet@2         # Instala .NET SDK
          - DotNetCoreCLI@2     # dotnet restore
          - DotNetCoreCLI@2     # dotnet build --configuration Release
          - DotNetCoreCLI@2     # dotnet test --collect:"XPlat Code Coverage"
          - PublishTestResults@2
          - PublishCodeCoverageResults@2

  - stage: Containerize        # Solo si Build_Test pasó
    jobs:
      - job: DockerBuild
        steps:
          - Docker@2           # Build Catalog.Api
          - Docker@2           # Build Ordering.Api
          - Docker@2           # Build Payment.Worker
```

---

## 18. Cross-Cutting Concerns

### 18.1. Manejo Centralizado de Errores

```csharp
// Archivo: src/BuildingBlocks/Exceptions/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, message) = exception switch
        {
            _ when exception.GetType().Name == "DomainException" => (400, exception.Message),
            ValidationException ex => (400, string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),
            KeyNotFoundException  => (404, exception.Message),
            _                     => (500, "Error interno del servidor.")
        };

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { error = message }, ct);
        return true;  // ← true = "yo manejé la excepción, no la propagues más"
    }
}
```

Registrado en Program.cs:
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// ...
app.UseExceptionHandler();  // ← Activa el middleware
```

### 18.2. Logging Estructurado (Serilog)

```csharp
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());
```

En vez de: `"Order 123 creado por user@mail.com"` (texto plano)
Genera: `{"RequestName": "CreateOrderCommand", "OrderId": "123", "Email": "user@mail.com"}` (JSON con propiedades)

### 18.3. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderingDbContext>();  // ← Verifica que PostgreSQL responde

app.MapHealthChecks("/health");              // ← GET /health → 200 "Healthy" o 503 "Unhealthy"
```

---

## 19. Palabras Reservadas y Sintaxis de C# Usadas

### Keywords de C#

| Keyword | Significado | Ejemplo en el proyecto |
|---------|-----------|----------------------|
| `record` | Tipo de referencia con igualdad por valores e inmutabilidad. | `public record Money { ... }`, `public record OrderDto(...)` |
| `sealed` | Impide herencia. La clase no puede tener subclases. | `public sealed record Money`, `internal sealed class CreateOrderHandler` |
| `internal` | Visible solo dentro del mismo assembly (proyecto). | `internal static OrderItem Create(...)` — solo Ordering.Domain puede llamarlo |
| `private set` | El setter solo es accesible desde dentro de la clase. | `public Guid Id { get; private set; }` |
| `private init` | Solo se puede asignar durante la construcción del objeto. | `public decimal Amount { get; private init; }` (en records) |
| `async` / `await` | Programación asíncrona sin bloquear el thread. | Toda operación de I/O: `await repository.GetByIdAsync(id, ct)` |
| `CancellationToken` | Permite cancelar operaciones en progreso. | En TODA firma de I/O. Si el cliente cierra la conexión, el token se cancela. |
| `where TRequest : notnull` | Constraint genérico. `TRequest` no puede ser null. | `IPipelineBehavior<TRequest, TResponse> where TRequest : notnull` |
| `nameof()` | Devuelve el nombre de una variable/tipo como string. | Usado implícitamente por el compilador |
| `is` / `is not` | Pattern matching. | `if (product is null)` |
| `switch expression` | Match de patrones con retorno de valor. | En GlobalExceptionHandler: `exception switch { ... }` |
| `=>` | Expression body. Método/propiedad de una línea. | `public Money Subtotal => UnitPrice.Multiply(Quantity);` |
| `var` | Tipo inferido por el compilador. | `var order = Order.Create(...)` — el tipo es `Order` |
| `using` | Importar namespace / disposable scope. | `using var scope = app.Services.CreateScope();` |
| `this` (en extensión) | Define un método de extensión. | `public static IServiceCollection AddApplication(this IServiceCollection services)` |
| `#pragma warning disable` | Suprime un warning del compilador. | `#pragma warning disable CS8618` — para constructores sin parámetros de EF |

### Operadores y Sintaxis

| Sintaxis | Significado | Ejemplo |
|----------|-----------|---------|
| `??` | Null-coalescing: si la izquierda es null, usa la derecha. | `config["Key"] ?? throw new InvalidOperationException("Falta Key")` |
| `?.` | Null-conditional: solo accede si no es null. | `product?.Name` |
| `!` (postfix) | Null-forgiving: le dice al compilador "confía, no es null". | No usado en este proyecto (buena práctica). |
| `[]` (collection) | Collection expression (C# 12+). | `private readonly List<OrderItem> _items = [];` |
| `( )` primary constructor | Constructor en la declaración de clase. | `public class ProductService(IProductRepository repository)` |
| `required` | La propiedad debe asignarse al crear. | No usado en este proyecto. |
| `file` | Tipo visible solo dentro del archivo. | No usado en este proyecto. |

### Atributos

| Atributo | Paquete | Significado |
|----------|---------|-----------|
| `[Fact]` | xUnit | Marca un método como test sin parámetros. |
| `[Theory]` | xUnit | Test parametrizado (con `[InlineData]`). No usado aún. |
| `[BsonId]` | MongoDB.Driver | Marca la propiedad como `_id` del documento. |
| `[BsonRepresentation]` | MongoDB.Driver | Controla cómo se serializa (ej: decimal como Decimal128). |

---

## 20. Paquetes NuGet — Referencia Completa

### Por proyecto

#### BuildingBlocks

| Paquete | Para qué |
|---------|----------|
| `Azure.Storage.Blobs` | SDK de Azure Blob Storage. Operaciones: upload, download, delete. |
| `AWSSDK.S3` | SDK de Amazon S3. Interfaz equivalente para AWS. |
| `FluentValidation` | Definir reglas de validación para `GlobalExceptionHandler`. |
| `FrameworkReference: Microsoft.AspNetCore.App` | Acceso a `IExceptionHandler`, `HttpContext`, etc. |

#### Catalog.Application

| Paquete | Para qué |
|---------|----------|
| `MediatR` | Despacha commands/queries a handlers. |
| `FluentValidation.DependencyInjectionExtensions` | `AddValidatorsFromAssembly()` — auto-registra validators. |
| `Microsoft.Extensions.Logging.Abstractions` | `ILogger<T>` para pipeline behaviors. |

#### Catalog.Infrastructure

| Paquete | Para qué |
|---------|----------|
| `MongoDB.Driver` | Conexión, queries, CRUD contra MongoDB. |

#### Ordering.Domain

| Paquete | Para qué |
|---------|----------|
| `MediatR.Contracts` | Solo `INotification` — para que `IDomainEvent` no dependa del paquete completo. |

#### Ordering.Application

| Paquete | Para qué |
|---------|----------|
| `MediatR` | CQRS handlers. |
| `MassTransit` | `IPublishEndpoint` para publicar integration events. `IConsumer<T>` para consumir. |
| `FluentValidation.DependencyInjectionExtensions` | Validators. |

#### Ordering.Infrastructure

| Paquete | Para qué |
|---------|----------|
| `Microsoft.EntityFrameworkCore` | ORM: DbContext, DbSet, Change Tracking, migraciones. |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Proveedor PostgreSQL para EF Core. `UseNpgsql()`. |
| `Microsoft.EntityFrameworkCore.Design` | Herramientas de diseño (migraciones). Solo desarrollo. |

#### Ordering.Api / Catalog.Api

| Paquete | Para qué |
|---------|----------|
| `Microsoft.AspNetCore.OpenApi` | `AddOpenApi()` / `MapOpenApi()` — genera documentación de API. |
| `Serilog.AspNetCore` | `UseSerilog()` — logging estructurado. |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | `.AddDbContextCheck<T>()` |

#### Payment.Worker

| Paquete | Para qué |
|---------|----------|
| `MassTransit` | Consumer de mensajes. |
| `Microsoft.Extensions.Hosting` | `Host.CreateApplicationBuilder()` para aplicaciones background. |

#### Tests

| Paquete | Para qué |
|---------|----------|
| `xunit` | Framework de testing. `[Fact]`, `Assert`. |
| `FluentAssertions` | Aserciones legibles: `.Should().Be()`, `.Should().Throw<>()`. |
| `NSubstitute` | Mocking: `Substitute.For<IMediator>()`. |
| `Testcontainers.PostgreSql` | Levanta PostgreSQL en Docker para tests de integración. |
| `Microsoft.NET.Test.Sdk` | Integra con `dotnet test`. |

---

> **Nota**: este documento se generó para la versión del proyecto al 2026-06-08. Si se agregan nuevas features, actualizarlo.
