# Teoría Completa de Desarrollo de Software — Aplicada a ShopHub

> Desde los fundamentos de la programación hasta arquitectura cloud.
> Cada concepto tiene su **definición teórica** y su **aplicación concreta** en el proyecto.

---

## Tabla de Contenido

1. [Programación Orientada a Objetos (POO)](#1-programación-orientada-a-objetos-poo)
2. [Tipos de Datos y Sistema de Tipos de C#](#2-tipos-de-datos-y-sistema-de-tipos-de-c)
3. [Principios SOLID](#3-principios-solid)
4. [Patrones de Diseño (Design Patterns)](#4-patrones-de-diseño-design-patterns)
5. [Patrones Arquitectónicos](#5-patrones-arquitectónicos)
6. [Domain-Driven Design (DDD)](#6-domain-driven-design-ddd)
7. [Programación Asíncrona](#7-programación-asíncrona)
8. [Inyección de Dependencias (Dependency Injection)](#8-inyección-de-dependencias-dependency-injection)
9. [Bases de Datos Relacionales](#9-bases-de-datos-relacionales)
10. [Bases de Datos No Relacionales (NoSQL)](#10-bases-de-datos-no-relacionales-nosql)
11. [ORM — Object-Relational Mapping](#11-orm--object-relational-mapping)
12. [APIs y Protocolos HTTP](#12-apis-y-protocolos-http)
13. [Arquitectura de Microservicios](#13-arquitectura-de-microservicios)
14. [Mensajería y Arquitectura Dirigida por Eventos](#14-mensajería-y-arquitectura-dirigida-por-eventos)
15. [Testing de Software](#15-testing-de-software)
16. [Contenedores y Docker](#16-contenedores-y-docker)
17. [CI/CD — Integración y Entrega Continua](#17-cicd--integración-y-entrega-continua)
18. [Computación en la Nube (Cloud Computing)](#18-computación-en-la-nube-cloud-computing)
19. [Logging y Observabilidad](#19-logging-y-observabilidad)
20. [Seguridad en Aplicaciones Web](#20-seguridad-en-aplicaciones-web)
21. [Control de Versiones con Git](#21-control-de-versiones-con-git)
22. [Conceptos de Rendimiento](#22-conceptos-de-rendimiento)

---

## 1. Programación Orientada a Objetos (POO)

La POO es un paradigma que organiza el software alrededor de **objetos** — unidades que combinan datos (propiedades) y comportamiento (métodos).

---

### 1.1 Clase

**Teoría**: Una clase es un **molde** o **plantilla** que define la estructura y el comportamiento de un tipo de objeto. Define qué propiedades tendrá y qué acciones podrá realizar. Por sí sola no es nada — necesitas crear una **instancia** (un objeto) a partir de ella.

**En ShopHub** — `Order` es una clase. Define que todo pedido tiene `Id`, `CustomerEmail`, `Status`, `Items`, etc. Pero hasta que alguien llame `Order.Create(...)` no existe un pedido real en memoria.

```
Archivo: src/Services/Ordering/Ordering.Domain/Orders/Order.cs

public class Order          ← La clase (el molde)
{
    public Guid Id { get; private set; }
    public string CustomerEmail { get; private set; }
    public OrderStatus Status { get; private set; }
    ...
}

var pedido = Order.Create("cliente@mail.com", address);  ← El objeto (una instancia)
```

---

### 1.2 Objeto / Instancia

**Teoría**: Un objeto es una **instancia concreta** de una clase. Ocupa memoria, tiene valores específicos en sus propiedades y puede ejecutar sus métodos. Puedes tener muchos objetos de la misma clase, cada uno con valores distintos.

**En ShopHub** — Cuando un cliente hace POST /orders, el handler crea un **objeto** Order con un Id único, email del cliente y estado Pending. Ese objeto vive en memoria hasta que se persiste en PostgreSQL.

---

### 1.3 Encapsulamiento

**Teoría**: Ocultar los detalles internos de un objeto y exponer solo lo necesario. El mundo exterior interactúa con el objeto a través de una **interfaz pública** controlada, no accediendo directamente a sus datos internos. Esto protege la integridad del estado.

**En ShopHub** — Las propiedades de `Order` tienen `private set`. Nadie puede hacer `order.Status = OrderStatus.Paid` directamente. Para cambiar el estado, **debes** pasar por `order.MarkAsPaid()`, que valida que la transición sea legal.

```
❌  order.Status = OrderStatus.Paid;       // No compila: set es private
✅  order.MarkAsPaid();                     // Pasa por validación
```

Los constructores son `private` — no puedes hacer `new Order(...)` desde fuera de la clase. Debes usar `Order.Create(...)` que aplica validación.

---

### 1.4 Herencia

**Teoría**: Una clase puede **heredar** propiedades y métodos de otra clase (la clase base o padre). La clase hija extiende o especializa el comportamiento de la padre. Establece una relación "es un" (is-a).

**En ShopHub** — `OrderingDbContext` hereda de `DbContext` (clase base de Entity Framework Core). Hereda toda la maquinaria de EF Core (Change Tracking, generación de SQL, migraciones) y la extiende con su propia lógica de domain events.

```
public class OrderingDbContext : DbContext, IUnitOfWork
                                 ↑ hereda de DbContext
```

También `DomainException` hereda de `Exception`:
```
public class DomainException(string message) : Exception(message);
                                                ↑ hereda de Exception
```

---

### 1.5 Polimorfismo

**Teoría**: La capacidad de que diferentes objetos respondan al mismo mensaje de maneras distintas. Si tienes una interfaz `IObjectStorage` con el método `UploadAsync`, diferentes implementaciones (Azure, AWS) ejecutan código distinto, pero el código que las usa no sabe ni le importa cuál es.

Hay dos tipos principales:
- **Polimorfismo de subtipo** (interfaces/herencia): diferentes clases implementan la misma interfaz.
- **Polimorfismo paramétrico** (genéricos): una clase o método funciona con diferentes tipos.

**En ShopHub** — `IObjectStorage` es implementado por `AzureBlobStorage` y `AwsS3Storage`. El código de Catalog solo conoce `IObjectStorage`; no sabe si está subiendo a Azure o AWS.

```
IObjectStorage storage = ...; // Puede ser Azure o AWS
await storage.UploadAsync("products", "img.jpg", stream, "image/jpeg", ct);
// ↑ La misma llamada, pero ejecuta código diferente según la implementación
```

Polimorfismo paramétrico con genéricos:
```
public sealed class LoggingBehavior<TRequest, TResponse>  ← funciona con CUALQUIER request/response
    : IPipelineBehavior<TRequest, TResponse>
```

---

### 1.6 Abstracción

**Teoría**: Mostrar solo los detalles relevantes y ocultar la complejidad. Una interfaz es la forma más pura de abstracción: define **qué** se puede hacer, sin decir **cómo**.

**En ShopHub** — `IOrderRepository` define 4 operaciones (GetById, GetAll, Add, Update). El handler no sabe si los datos están en PostgreSQL, en memoria o en un archivo. Solo sabe que puede llamar esos 4 métodos.

```
// La abstracción (en Application):
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    void Update(Order order);
}

// La implementación concreta (en Infrastructure):
public sealed class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    // Aquí está el "cómo": usa EF Core contra PostgreSQL
}
```

---

### 1.7 Interfaces

**Teoría**: Un **contrato** que define un conjunto de métodos que una clase debe implementar. No tiene implementación, solo firmas. Permite que diferentes clases sean intercambiables si cumplen el mismo contrato.

**En ShopHub** — Interfaces principales:

| Interfaz | Qué define | Quién la implementa |
|----------|-----------|-------------------|
| `IOrderRepository` | Operaciones de persistencia de pedidos | `OrderRepository` (EF Core + PostgreSQL) |
| `IProductRepository` | Operaciones de persistencia de productos | `MongoProductRepository` (MongoDB) |
| `IUnitOfWork` | Guardar cambios transaccionalmente | `OrderingDbContext` |
| `IObjectStorage` | Subir/descargar/eliminar archivos | `AzureBlobStorage`, `AwsS3Storage` |
| `IExceptionHandler` | Manejar excepciones HTTP | `GlobalExceptionHandler` |
| `IConsumer<T>` | Consumir mensajes de un bus | `PaymentSucceededConsumer`, `OrderCreatedConsumer` |
| `IRequestHandler<TReq, TRes>` | Manejar un command/query de MediatR | Todos los handlers |

---

### 1.8 Clases Abstractas vs Interfaces

**Teoría**:
- **Clase abstracta**: puede tener implementación parcial (métodos con código + métodos abstractos sin código). Una clase solo puede heredar de **una** clase abstracta.
- **Interfaz**: solo define el contrato (sin implementación). Una clase puede implementar **muchas** interfaces.

**En ShopHub** — Se usan interfaces (`IOrderRepository`, `IObjectStorage`) porque:
1. Permiten implementación múltiple (una clase puede implementar varias interfaces).
2. No imponen una jerarquía de herencia.
3. Son más fáciles de mockear en tests.

`OrderingDbContext` implementa **dos interfaces** a la vez: `DbContext` (herencia de clase) + `IUnitOfWork` (interfaz).

---

## 2. Tipos de Datos y Sistema de Tipos de C#

### 2.1 Tipos por Valor vs Tipos por Referencia

**Teoría**:
- **Tipo por valor** (`struct`, `enum`, `int`, `decimal`, `bool`, `Guid`): almacena el dato directamente en la variable. Al asignar a otra variable, se **copia** el valor.
- **Tipo por referencia** (`class`, `record`, `string`, arrays): la variable almacena una **referencia** (puntero) al objeto en el heap. Al asignar a otra variable, ambas apuntan al **mismo** objeto.

**En ShopHub**:
- `OrderStatus` es un `enum` (tipo por valor): `Pending`, `Paid`, `Cancelled`.
- `Guid` es un `struct` (tipo por valor): `Order.Id`, `OrderItem.ProductId`.
- `Order` es una `class` (tipo por referencia): al pasar un Order a un método, se pasa la referencia, no una copia.
- `Money` es un `record` (tipo por referencia con semántica de valor).

---

### 2.2 Records

**Teoría**: Un `record` es un tipo por referencia que el compilador trata como si fuera por valor en cuanto a **igualdad**. Dos records con los mismos valores son iguales (`==` devuelve `true`). Son ideales para objetos **inmutables** que representan datos.

**En ShopHub** — Los value objects y DTOs son records:
```
// Value Object inmutable:
public sealed record Money { ... }

// DTO (Data Transfer Object):
public record OrderDto(Guid Id, string CustomerEmail, string Status, ...);

// Command (inmutable por naturaleza):
public record CreateOrderCommand(...) : IRequest<OrderDto>;

// Integration Event:
public record OrderCreatedIntegrationEvent(Guid OrderId, string CustomerEmail, ...);
```

¿Por qué `record` y no `class`?
- Igualdad automática por valores (no necesitas override de `Equals`/`GetHashCode`).
- Inmutabilidad natural con `init` setters.
- Sintaxis concisa con constructores posicionales: `record OrderDto(Guid Id, string Name);`.

---

### 2.3 Enums

**Teoría**: Un tipo que define un conjunto fijo de constantes con nombre. Internamente son enteros, pero en el código se usan como nombres legibles.

**En ShopHub**:
```
public enum OrderStatus
{
    Pending,     // = 0
    Paid,        // = 1
    Cancelled    // = 2
}
```

En la base de datos se guarda como **string** ("Pending", "Paid") gracias a la configuración de EF Core: `HasConversion<string>()`. Esto hace que los datos en la tabla sean legibles sin necesidad de una tabla de lookup.

---

### 2.4 Generics (Genéricos)

**Teoría**: Permiten definir clases, interfaces y métodos que funcionan con **cualquier tipo**, sin perder seguridad de tipos. El tipo concreto se especifica al usar la clase.

**En ShopHub** — Los pipeline behaviors son genéricos:
```
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
```

`TRequest` y `TResponse` son **parámetros de tipo**. Cuando MediatR procesa `CreateOrderCommand`, los reemplaza:
- `TRequest` = `CreateOrderCommand`
- `TResponse` = `OrderDto`

El mismo `LoggingBehavior` funciona para **todos** los commands y queries sin escribir uno por cada tipo.

---

### 2.5 Nullable Reference Types

**Teoría**: Desde C# 8, puedes marcar que un tipo por referencia puede ser `null` con `?`. El compilador emite warnings si usas un valor potencialmente null sin verificar.

**En ShopHub** — Todos los proyectos tienen `<Nullable>enable</Nullable>`:
```
public string? ImageUrl { get; private set; }   // ← Puede ser null
public string Name { get; private set; }         // ← NO puede ser null (el compilador advierte si no se inicializa)

Task<Order?> GetByIdAsync(Guid id, ...);         // ← Puede devolver null (pedido no encontrado)
```

---

### 2.6 Collections (Colecciones)

**Teoría**: Estructuras de datos que almacenan múltiples elementos.

| Tipo | Descripción | Mutable |
|------|------------|---------|
| `List<T>` | Lista dinámica indexada | Sí |
| `IReadOnlyList<T>` | Vista de solo lectura de una lista | No |
| `IEnumerable<T>` | Secuencia iterable (la más básica) | No |
| `Dictionary<TKey, TValue>` | Pares clave-valor | Sí |

**En ShopHub**:
```
// Lista privada mutable (solo Order la modifica):
private readonly List<OrderItem> _items = [];

// Exposición pública como solo lectura:
public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

// Repositorio devuelve lista inmutable:
Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct);
```

El patrón `_items` (privado mutable) + `Items` (público inmutable) protege la encapsulación: el mundo exterior puede **leer** los items pero no puede agregar ni quitar directamente.

---

## 3. Principios SOLID

SOLID es un acrónimo de 5 principios de diseño orientado a objetos que producen código mantenible, extensible y testeable.

---

### 3.1 S — Single Responsibility Principle (SRP)

**Teoría**: Una clase debe tener **una sola razón para cambiar**. Si una clase hace dos cosas, un cambio en una puede romper la otra.

**En ShopHub** — Cada handler tiene una sola responsabilidad:

| Clase | Única responsabilidad |
|-------|---------------------|
| `CreateOrderHandler` | Crear un pedido |
| `PayOrderHandler` | Marcar un pedido como pagado |
| `LoggingBehavior` | Loguear requests |
| `ValidationBehavior` | Validar requests |
| `OrderRepository` | Acceder a datos de pedidos en PostgreSQL |
| `GlobalExceptionHandler` | Convertir excepciones en respuestas HTTP |

Si mañana cambia la lógica de validación, solo tocas `ValidationBehavior`. Los handlers no se enteran.

---

### 3.2 O — Open/Closed Principle (OCP)

**Teoría**: Las entidades de software deben estar **abiertas para extensión** pero **cerradas para modificación**. Debes poder agregar funcionalidad nueva sin cambiar el código existente.

**En ShopHub** — Los pipeline behaviors de MediatR son el ejemplo perfecto:

```
// Para agregar caching a TODOS los queries, solo creas:
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> { ... }

// Y lo registras:
cfg.AddOpenBehavior(typeof(CachingBehavior<,>));

// ❌ NO modificas ningún handler existente
// ✅ El nuevo behavior se intercala automáticamente en la cadena
```

---

### 3.3 L — Liskov Substitution Principle (LSP)

**Teoría**: Si `S` es un subtipo de `T`, entonces objetos de tipo `T` pueden ser reemplazados por objetos de tipo `S` sin alterar el comportamiento correcto del programa.

**En ShopHub** — `AzureBlobStorage` y `AwsS3Storage` son subtipos de `IObjectStorage`. Puedes intercambiar uno por otro y el programa funciona igual:

```
// En StorageDependencyInjection.cs:
switch (provider)
{
    case "Azure":
        services.AddSingleton<IObjectStorage, AzureBlobStorage>();  // ← Usa Azure
        break;
    case "Aws":
        services.AddSingleton<IObjectStorage, AwsS3Storage>();      // ← Usa AWS
        break;
}
// El código que consume IObjectStorage NO cambia
```

---

### 3.4 I — Interface Segregation Principle (ISP)

**Teoría**: Los clientes no deben depender de interfaces que no usan. Es mejor tener muchas interfaces pequeñas que una grande.

**En ShopHub** — En vez de una sola interfaz `IDatabase` con 20 métodos, hay interfaces específicas:

| Interfaz | Métodos | Quién la usa |
|----------|---------|-------------|
| `IOrderRepository` | `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update` | Command/Query handlers |
| `IUnitOfWork` | `SaveChangesAsync` | Command handlers (solo escritura) |
| `IObjectStorage` | `UploadAsync`, `DownloadAsync`, `DeleteAsync` | Catalog (para imágenes) |

Los query handlers solo necesitan `IOrderRepository` (lectura). Los command handlers necesitan `IOrderRepository` + `IUnitOfWork` (lectura + escritura). Nadie depende de métodos que no usa.

---

### 3.5 D — Dependency Inversion Principle (DIP)

**Teoría**: Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de **abstracciones**. Las abstracciones no deben depender de detalles; los detalles deben depender de abstracciones.

**En ShopHub** — La Application Layer (alto nivel) define las interfaces. La Infrastructure Layer (bajo nivel) las implementa:

```
Application (alto nivel):
  define → IOrderRepository (abstracción)
  usa    → IOrderRepository (no sabe que es PostgreSQL)

Infrastructure (bajo nivel):
  implementa → OrderRepository : IOrderRepository (detalle: usa EF Core + PostgreSQL)
  depende de → IOrderRepository (la abstracción definida en Application)
```

Las flechas de dependencia van **hacia adentro** (Infrastructure → Application → Domain), nunca al revés.

---

## 4. Patrones de Diseño (Design Patterns)

Los patrones de diseño son soluciones probadas a problemas recurrentes en el desarrollo de software. Se clasifican en creacionales, estructurales y de comportamiento.

---

### 4.1 Factory Method (Creacional)

**Teoría**: Define una interfaz para crear objetos, pero deja que las subclases (o la propia clase) decidan qué instancia crear. Encapsula la lógica de construcción.

**En ShopHub** — Los métodos `Create()` y `Restore()` son factory methods:
```
// Factory: crea un Order NUEVO con validación
public static Order Create(string customerEmail, Address shippingAddress)
{
    if (string.IsNullOrWhiteSpace(customerEmail))
        throw new DomainException("El email del cliente es obligatorio.");

    var order = new Order(Guid.NewGuid(), customerEmail.Trim(), shippingAddress, OrderStatus.Pending, DateTime.UtcNow);
    order._domainEvents.Add(new OrderCreatedDomainEvent(order.Id, order.CustomerEmail));
    return order;
}

// Factory: reconstruye un Order EXISTENTE desde la base de datos (sin validación)
public static Order Restore(Guid id, string customerEmail, Address shippingAddress, OrderStatus status, DateTime createdAt)
{
    return new Order(id, customerEmail, shippingAddress, status, createdAt);
}
```

¿Por qué dos métodos? Porque la lógica es diferente:
- `Create` genera un nuevo `Id`, valida datos, y emite un domain event.
- `Restore` reconstruye un objeto que ya fue validado cuando se creó. No genera events.

---

### 4.2 Repository (Estructural)

**Teoría**: Abstrae el acceso a datos detrás de una interfaz que simula una colección en memoria. El código de negocio opera como si los objetos estuvieran en una lista local, sin saber de SQL, conexiones ni transacciones.

**En ShopHub**:
```
// El handler piensa que trabaja con una colección:
await repository.AddAsync(order, ct);        // "Agregar a la colección"
var order = await repository.GetByIdAsync(id, ct);  // "Buscar en la colección"
repository.Update(order);                    // "Actualizar en la colección"

// Pero detrás, OrderRepository traduce a EF Core:
public async Task AddAsync(Order order, CancellationToken ct)
{
    await context.Orders.AddAsync(order, ct);  // ← Realmente es SQL: INSERT INTO Orders...
}
```

---

### 4.3 Unit of Work

**Teoría**: Mantiene una lista de objetos afectados por una transacción de negocio y coordina la escritura de cambios y la resolución de problemas de concurrencia. Agrupa múltiples operaciones en una **sola transacción**.

**En ShopHub** — `OrderingDbContext` implementa `IUnitOfWork`:
```
// El handler hace varias operaciones:
await repository.AddAsync(order, ct);       // ← Solo marca como "pendiente de insertar"
repository.Update(anotherOrder);            // ← Solo marca como "pendiente de actualizar"

// Un solo SaveChangesAsync() persiste TODO en una transacción:
await unitOfWork.SaveChangesAsync(ct);      // ← BEGIN TRANSACTION; INSERT...; UPDATE...; COMMIT;
```

Si algo falla, **nada** se persiste. Es todo o nada (atomicidad).

---

### 4.4 Mediator (Comportamiento)

**Teoría**: Define un objeto que encapsula cómo interactúan un conjunto de objetos. Promueve el acoplamiento débil evitando que los objetos se refieran entre sí explícitamente.

**En ShopHub** — MediatR es el mediador:
```
// El endpoint NO conoce al handler directamente:
var order = await mediator.Send(new CreateOrderCommand(...), ct);
//                ↑ mediator busca el handler correcto automáticamente

// Sin mediator, el endpoint tendría que conocer y crear el handler:
// ❌ var handler = new CreateOrderHandler(repo, uow);
// ❌ var order = await handler.Handle(command, ct);
```

Beneficios:
- El endpoint no depende del handler.
- Puedes agregar behaviors (logging, validación) sin tocar ni endpoint ni handler.
- El handler se descubre automáticamente por convención.

---

### 4.5 Strategy (Comportamiento)

**Teoría**: Define una familia de algoritmos, los encapsula en clases separadas y los hace intercambiables. El cliente elige qué estrategia usar en runtime.

**En ShopHub** — `IObjectStorage` con selección por configuración:
```
// Dos estrategias:
AzureBlobStorage : IObjectStorage  → usa Azure Blob Storage
AwsS3Storage     : IObjectStorage  → usa Amazon S3

// La selección se hace por configuración en runtime:
var provider = configuration["Storage:Provider"];  // "Azure" o "Aws"
// ↑ Cambias de proveedor cloud SIN cambiar código
```

---

### 4.6 Observer (Comportamiento)

**Teoría**: Define una dependencia uno-a-muchos: cuando un objeto cambia de estado, todos sus dependientes son notificados automáticamente.

**En ShopHub** — Los domain events son una implementación del patrón Observer:
```
// Order (el sujeto) registra un evento:
order._domainEvents.Add(new OrderCreatedDomainEvent(order.Id, order.CustomerEmail));

// OrderingDbContext (el publicador) notifica:
foreach (var domainEvent in domainEvents)
    await mediator.Publish(domainEvent, ct);

// OrderCreatedDomainEventHandler (el observador) reacciona:
public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken ct)
{
    await publishEndpoint.Publish(new OrderCreatedIntegrationEvent(...));
}
```

El `Order` no sabe quién escucha sus eventos. Puede haber cero, uno o muchos handlers.

---

### 4.7 Decorator / Chain of Responsibility (Comportamiento)

**Teoría**: Permite pasar una solicitud a lo largo de una cadena de handlers. Cada handler decide si procesa la solicitud o la pasa al siguiente.

**En ShopHub** — Los pipeline behaviors de MediatR forman una cadena:
```
Request
  → LoggingBehavior     (loguea y pasa al siguiente)
    → ValidationBehavior  (valida; si falla, corta; si pasa, sigue)
      → Handler           (ejecuta la lógica de negocio)
    ← ValidationBehavior
  ← LoggingBehavior
Response
```

Cada behavior recibe un `RequestHandlerDelegate<TResponse> next` que representa al siguiente en la cadena. Llamar `await next(ct)` pasa al siguiente; no llamarlo corta la cadena.

---

## 5. Patrones Arquitectónicos

---

### 5.1 Clean Architecture

**Teoría**: Organiza el código en **capas concéntricas** con una regla fundamental: las dependencias solo apuntan **hacia adentro**. Las capas externas conocen a las internas, pero nunca al revés. El dominio está en el centro, protegido de frameworks y detalles de infraestructura.

```
        ┌───────────────────────────┐
        │         API Layer         │  Frameworks, HTTP, configuración
        │  ┌───────────────────┐    │
        │  │  Infrastructure   │    │  Bases de datos, SDKs, servicios externos
        │  │  ┌────────────┐   │    │
        │  │  │ Application│   │    │  Casos de uso, orquestación
        │  │  │  ┌──────┐  │   │    │
        │  │  │  │Domain│  │   │    │  Entidades, reglas de negocio puras
        │  │  │  └──────┘  │   │    │
        │  │  └────────────┘   │    │
        │  └───────────────────┘    │
        └───────────────────────────┘
```

**En ShopHub** — Cada microservicio sigue esta estructura. Las `<ProjectReference>` en los `.csproj` materializan las reglas de dependencia:
- `Ordering.Domain.csproj` → no referencia a nadie
- `Ordering.Application.csproj` → referencia solo a Domain
- `Ordering.Infrastructure.csproj` → referencia a Application (y transitivamente a Domain)
- `Ordering.Api.csproj` → referencia a Application e Infrastructure

---

### 5.2 CQRS (Command Query Responsibility Segregation)

**Teoría**: Separa las operaciones de **escritura** (Commands) de las de **lectura** (Queries) en modelos distintos. Un Command modifica el estado y puede o no devolver un resultado. Una Query solo lee, sin efectos secundarios.

| Aspecto | Command | Query |
|---------|---------|-------|
| Propósito | Modificar estado | Leer estado |
| Ejemplo | `CreateOrderCommand` | `GetOrderByIdQuery` |
| Efectos secundarios | Sí (INSERT, UPDATE) | No (SELECT) |
| Puede fallar por validación | Sí | No (típicamente) |

**En ShopHub**:
```
// COMMAND (modifica): crea un pedido en la BD
await mediator.Send(new CreateOrderCommand(...), ct);

// QUERY (lee): busca un pedido sin modificar nada
await mediator.Send(new GetOrderByIdQuery(orderId), ct);
```

**¿Por qué separar?** En sistemas complejos, el modelo de lectura puede optimizarse de forma diferente al de escritura (diferentes tablas, cachés, proyecciones). Aunque en ShopHub ambos usan el mismo repositorio, la separación conceptual ya está.

---

### 5.3 Arquitectura Hexagonal (Ports & Adapters)

**Teoría**: El dominio está en el centro. Define **puertos** (interfaces) que la infraestructura conecta con **adaptadores** (implementaciones). Los puertos de entrada reciben solicitudes; los puertos de salida acceden a recursos externos.

**En ShopHub**:
```
Puerto de salida (interfaz en Application):   IOrderRepository
Adaptador (implementación en Infrastructure): OrderRepository (usa EF Core)

Puerto de salida:   IObjectStorage
Adaptador Azure:    AzureBlobStorage
Adaptador AWS:      AwsS3Storage
```

---

## 6. Domain-Driven Design (DDD)

**Teoría**: DDD es una metodología de diseño de software que centra el desarrollo en el **dominio del negocio**. El código refleja el lenguaje de los expertos del negocio (Ubiquitous Language). El software se organiza en **Bounded Contexts** — fronteras claras donde un modelo tiene un significado específico.

---

### 6.1 Entity (Entidad)

**Teoría**: Un objeto que tiene una **identidad** persistente a lo largo del tiempo. Aunque todos sus atributos cambien, sigue siendo el mismo objeto si tiene el mismo `Id`. Dos entidades con los mismos datos pero distinto Id son **diferentes**.

**En ShopHub**: `Order` y `OrderItem` son entidades. Cada uno tiene un `Guid Id` que lo identifica de forma única.

---

### 6.2 Value Object (Objeto de Valor)

**Teoría**: Un objeto que se define completamente por sus **atributos**, sin identidad. Es **inmutable** — una vez creado, no cambia. Dos value objects con los mismos valores son iguales. Si necesitas cambiar un valor, creas uno nuevo.

**En ShopHub**: `Money` y `Address` son value objects:
```
Money.Create(100, "USD") == Money.Create(100, "USD")  // TRUE
Money.Create(100, "USD") == Money.Create(200, "USD")  // FALSE

// Inmutable: Add devuelve un NUEVO Money:
var total = price.Add(tax);  // price no cambia; total es un nuevo objeto
```

---

### 6.3 Aggregate (Agregado)

**Teoría**: Un cluster de entidades y value objects con una **raíz** que actúa como puerta de entrada. Las reglas de negocio que involucran a todo el cluster se protegen a través de la raíz. Los objetos externos solo interactúan con la raíz, nunca con los hijos directamente.

**En ShopHub**: `Order` es la raíz del agregado. `OrderItem` es un hijo. Nunca creas un `OrderItem` solo — siempre a través de `order.AddItem(...)`:
```
// La raíz protege invariantes:
public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
{
    if (Status != OrderStatus.Pending)
        throw new DomainException("Solo se pueden agregar items a un pedido pendiente.");
    // ↑ Invariante protegida por la raíz
    var item = OrderItem.Create(productId, productName, unitPrice, quantity);
    _items.Add(item);
}
```

---

### 6.4 Domain Event

**Teoría**: Un registro de que algo **ya ocurrió** en el dominio. Es un hecho pasado, no una solicitud. Se usa para desencadenar efectos secundarios sin acoplar las partes del sistema.

**En ShopHub**: Cuando se crea un pedido, `Order.Create()` registra `OrderCreatedDomainEvent`. No lo publica — solo lo registra. El `DbContext` lo publica después de persistir exitosamente.

---

### 6.5 Integration Event

**Teoría**: Un evento que cruza las fronteras de un bounded context (microservicio) para notificar a otros servicios que algo ocurrió. Viaja por un bus de mensajes.

**En ShopHub**: `OrderCreatedIntegrationEvent` viaja de Ordering a Payment.Worker. `PaymentSucceededIntegrationEvent` viaja de Payment.Worker de vuelta a Ordering.

---

### 6.6 Bounded Context

**Teoría**: Una frontera explícita dentro de la cual un modelo de dominio es consistente y tiene significado. El mismo concepto puede tener significados diferentes en distintos bounded contexts.

**En ShopHub**: Catalog y Ordering son bounded contexts separados. "Product" en Catalog tiene nombre, descripción, precio e imagen. "Product" en Ordering es solo un `ProductId` y `ProductName` desnormalizado en el `OrderItem` — no necesita la imagen ni la descripción completa.

---

### 6.7 Invariante

**Teoría**: Una regla de negocio que **siempre** debe ser verdadera. El agregado es responsable de proteger sus invariantes. Si una operación violaría una invariante, se rechaza con una excepción.

**En ShopHub**, invariantes del agregado `Order`:
- No se pueden agregar items si el pedido no está en estado `Pending`.
- La cantidad de un item debe ser mayor a cero.
- Solo se puede pagar un pedido que está en estado `Pending`.
- No se puede cancelar un pedido que ya fue pagado.
- No se puede cancelar un pedido que ya está cancelado.

---

## 7. Programación Asíncrona

---

### 7.1 ¿Qué es y por qué importa?

**Teoría**: La programación asíncrona permite que un thread **no se bloquee** mientras espera una operación de I/O (lectura de disco, consulta a base de datos, llamada HTTP). Mientras espera, el thread queda libre para atender **otros requests**. En un servidor web, esto significa atender miles de requests concurrentes con pocos threads.

---

### 7.2 async / await

**Teoría**: `async` marca un método como asíncrono. `await` suspende la ejecución del método **sin bloquear el thread** hasta que la operación termine. Cuando termina, la ejecución continúa donde se dejó.

**En ShopHub** — **toda** operación de I/O es async/await:
```
public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
{
    // await: "espera a que se inserte en la BD, pero libera el thread mientras tanto"
    await repository.AddAsync(order, ct);

    // await: "espera a que se persista, pero libera el thread"
    await unitOfWork.SaveChangesAsync(ct);

    return OrderMapper.ToDto(order);
}
```

**Regla de oro**: si un método llama a algo asíncrono, él mismo debe ser `async Task` o `async Task<T>`. Nunca `async void` (excepto event handlers de UI).

---

### 7.3 Task y Task<T>

**Teoría**: `Task` representa una operación asíncrona que puede o no haber terminado. `Task<T>` es lo mismo pero devuelve un resultado de tipo `T`. Son la "promesa" de que un resultado estará disponible eventualmente.

```
Task AddAsync(Order order, CancellationToken ct);           // No devuelve nada (void asíncrono)
Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);   // Devuelve un Order (o null)
Task<int> SaveChangesAsync(CancellationToken ct);            // Devuelve el número de registros afectados
```

---

### 7.4 CancellationToken

**Teoría**: Un mecanismo de cooperación para cancelar operaciones en progreso. El llamador crea un `CancellationTokenSource`, obtiene un `CancellationToken` y lo pasa al método. Si el token se cancela (por ejemplo, porque el cliente cerró la conexión), la operación debe detenerse lo antes posible.

**En ShopHub** — CancellationToken se propaga en **toda la cadena**:
```
// 1. ASP.NET Core inyecta el token del request HTTP:
group.MapPost("/", async (CreateOrderCommand cmd, IMediator mediator, CancellationToken ct) =>
    await mediator.Send(cmd, ct));

// 2. MediatR lo pasa al handler:
public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)

// 3. El handler lo pasa al repositorio:
await repository.AddAsync(order, ct);

// 4. El repositorio lo pasa a EF Core:
await context.Orders.AddAsync(order, ct);

// 5. EF Core lo pasa al driver de PostgreSQL:
// (internamente cancela la query SQL si el token se cancela)
```

Si el usuario cierra el navegador a mitad de la petición, el token se cancela y toda la cadena se detiene.

---

### 7.5 Antipatrones: .Result y .Wait()

**Teoría**: Llamar `.Result` o `.Wait()` en un `Task` **bloquea** el thread actual hasta que la operación termine. Esto anula los beneficios de async y puede causar **deadlocks** en contextos con `SynchronizationContext` (como ASP.NET clásico).

```
// ❌ NUNCA hacer esto:
var order = repository.GetByIdAsync(id, ct).Result;   // Bloquea el thread
repository.AddAsync(order, ct).Wait();                 // Bloquea el thread

// ✅ SIEMPRE usar await:
var order = await repository.GetByIdAsync(id, ct);
await repository.AddAsync(order, ct);
```

En ShopHub, `.Result` y `.Wait()` no se usan en **ningún** lugar del código.

---

## 8. Inyección de Dependencias (Dependency Injection)

---

### 8.1 Concepto

**Teoría**: En vez de que una clase cree sus dependencias internamente (`new OrderRepository()`), las recibe desde afuera (inyectadas por el contenedor de DI). Esto permite desacoplar la clase de sus implementaciones concretas y facilita el testing.

```
// ❌ Sin DI: la clase CREA su dependencia
public class CreateOrderHandler
{
    private readonly OrderRepository _repo = new OrderRepository(new OrderingDbContext(...));
    // Problemas: acoplamiento fuerte, imposible de testear, no puede cambiar de BD
}

// ✅ Con DI: la clase RECIBE su dependencia
public class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
{
    // No sabe si es PostgreSQL, SQLite o un mock de test
}
```

---

### 8.2 Lifetimes (Ciclos de Vida)

**Teoría**: El contenedor de DI gestiona cuándo crear y destruir las instancias.

| Lifetime | Comportamiento | Cuándo usar |
|----------|---------------|-------------|
| **Singleton** | Una sola instancia para toda la aplicación | Servicios thread-safe, costosos de crear: clientes HTTP, clientes de BD, configuración |
| **Scoped** | Una instancia por scope (en web = una por request HTTP) | DbContext, repositorios, cualquier cosa con estado por request |
| **Transient** | Una nueva instancia cada vez que se solicita | Servicios ligeros sin estado, helpers |

**En ShopHub**:
```
// SINGLETON: IMongoClient es costoso de crear y thread-safe
services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

// SCOPED: DbContext tiene estado (Change Tracker) que es por request
services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(connectionString));
// AddDbContext registra como Scoped por defecto

// SCOPED: IUnitOfWork ES la misma instancia de OrderingDbContext
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderingDbContext>());
```

---

### 8.3 Constructor Injection (Primary Constructors de C# 12)

**Teoría**: Las dependencias se pasan a través del constructor. El contenedor de DI las resuelve automáticamente al crear la instancia.

**En ShopHub** — Se usan **primary constructors** (C# 12), una sintaxis que declara el constructor en la misma línea de la clase:
```
// Sintaxis tradicional (C# < 12):
public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
}

// Primary constructor (C# 12) — equivalente pero más conciso:
public class CreateOrderHandler(IOrderRepository repository, IUnitOfWork unitOfWork)
{
    // repository y unitOfWork están disponibles como campos
}
```

---

### 8.4 Extension Methods para Registro

**Teoría**: Un extension method añade métodos a un tipo existente sin modificarlo. Se usa `this` como primer parámetro.

**En ShopHub** — Cada capa expone un método de extensión para registrar sus servicios:
```
// Extension method: añade AddInfrastructure() a IServiceCollection
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    //                                                  ↑ this = lo convierte en extension method
    {
        services.AddDbContext<OrderingDbContext>(...);
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}

// Uso (parece un método nativo de IServiceCollection):
builder.Services.AddInfrastructure(builder.Configuration);
```

---

## 9. Bases de Datos Relacionales

---

### 9.1 Modelo Relacional

**Teoría**: Organiza los datos en **tablas** (relaciones) con filas (registros) y columnas (atributos). Las tablas se conectan entre sí mediante **foreign keys**. Garantiza **ACID**: Atomicidad, Consistencia, Aislamiento, Durabilidad.

**En ShopHub** — El servicio Ordering usa PostgreSQL con dos tablas:
```
Orders (tabla padre)
├── Id (PK)
├── CustomerEmail
├── Status
├── ShippingStreet, ShippingCity, ... (owned type Address)
└── CreatedAt

OrderItems (tabla hija)
├── Id (PK)
├── OrderId (FK → Orders.Id)   ← Relación 1:N
├── ProductId
├── ProductName
├── UnitPriceAmount, UnitPriceCurrency (owned type Money)
└── Quantity
```

---

### 9.2 Primary Key (PK)

**Teoría**: Columna (o conjunto de columnas) que identifica de forma única cada fila de una tabla. No puede ser `NULL` ni repetirse.

**En ShopHub**: `Orders.Id` y `OrderItems.Id` son primary keys de tipo `UUID` (`Guid` en C#).

---

### 9.3 Foreign Key (FK)

**Teoría**: Columna que referencia la primary key de otra tabla, estableciendo una relación entre ambas. Garantiza **integridad referencial** — no puedes tener un `OrderItem` apuntando a un `Order` que no existe.

**En ShopHub**:
```csharp
builder.HasMany(o => o.Items)
    .WithOne()
    .HasForeignKey("OrderId")           // ← FK de OrderItems hacia Orders
    .OnDelete(DeleteBehavior.Cascade);  // ← Si borras un Order, se borran sus Items
```

`"OrderId"` es una **shadow property**: existe en la tabla SQL pero no como propiedad en la clase C#.

---

### 9.4 Transacciones y ACID

**Teoría**:
- **A**tomicidad: una transacción es todo o nada. Si algo falla, todo se revierte.
- **C**onsistencia: la BD pasa de un estado válido a otro estado válido.
- **I**solamiento: transacciones concurrentes no interfieren entre sí.
- **D**urabilidad: una vez confirmada, la transacción sobrevive a fallos.

**En ShopHub** — `SaveChangesAsync()` de EF Core ejecuta todas las operaciones pendientes en una transacción:
```
await repository.AddAsync(order, ct);     // Marca como pendiente
await unitOfWork.SaveChangesAsync(ct);    // BEGIN TRAN → INSERT → COMMIT
// Si falla en cualquier punto → ROLLBACK
```

---

### 9.5 Migraciones

**Teoría**: Scripts que modifican el esquema de la base de datos de forma incremental y versionada. Cada migración describe los cambios (crear tabla, agregar columna, etc.) y cómo revertirlos. Permiten evolucionar el esquema sin perder datos.

**En ShopHub**: EF Core genera migraciones como código C# en `Persistence/Migrations/`. En desarrollo, se aplican automáticamente al iniciar:
```csharp
await db.Database.MigrateAsync();  // Aplica migraciones pendientes
```

---

### 9.6 Normalización vs Desnormalización

**Teoría**:
- **Normalización**: eliminar redundancia distribuyendo datos en múltiples tablas relacionadas. Minimiza inconsistencias.
- **Desnormalización**: duplicar datos a propósito para mejorar el rendimiento de lectura. Acepta redundancia.

**En ShopHub** — `OrderItem.ProductName` es una **desnormalización deliberada**. El nombre del producto se copia al momento de crear el pedido. Si el producto cambia de nombre después, el pedido conserva el nombre original (que es el correcto para el historial).

---

## 10. Bases de Datos No Relacionales (NoSQL)

---

### 10.1 Concepto

**Teoría**: Bases de datos que no usan el modelo tabular relacional. Existen varios tipos: documentales (MongoDB), clave-valor (Redis), columnares (Cassandra), grafos (Neo4j). Priorizan flexibilidad de esquema, escalabilidad horizontal y rendimiento en ciertos patrones de acceso.

---

### 10.2 Bases de Datos Documentales

**Teoría**: Almacenan datos como **documentos** (típicamente JSON/BSON). Cada documento puede tener una estructura diferente. No requieren un esquema fijo. Son ideales para datos con estructura jerárquica o variable.

**En ShopHub** — Catalog usa MongoDB porque:
- `Product` es un agregado simple sin relaciones complejas.
- El esquema puede evolucionar fácilmente (agregar `ImageUrl` no requiere migración).
- Las queries son simples (por Id o listado completo).
- No necesita joins ni transacciones multi-documento.

---

### 10.3 Documento vs Fila

| Relacional (PostgreSQL) | Documental (MongoDB) |
|-------------------------|---------------------|
| Tabla | Colección |
| Fila | Documento |
| Columna | Campo |
| Schema fijo obligatorio | Schema flexible |
| SQL | Operaciones BSON / API del driver |

---

### 10.4 Cuándo Usar Cada Uno

| Escenario | Mejor opción | Por qué |
|-----------|-------------|---------|
| Relaciones complejas (Order → Items) | Relacional | Joins, FK, integridad referencial |
| Transacciones ACID | Relacional | Soporte nativo |
| Datos flexibles / variables | Documental | Sin migraciones, schema flexible |
| Lectura/escritura masiva de documentos simples | Documental | Mejor rendimiento para patrones simples |
| Agregados autónomos | Documental | El documento completo se lee/escribe de una vez |

---

## 11. ORM — Object-Relational Mapping

---

### 11.1 Concepto

**Teoría**: Un ORM mapea **objetos** del lenguaje de programación a **tablas** de una base de datos relacional. En vez de escribir SQL manualmente, operas con objetos de C# y el ORM genera el SQL automáticamente.

**En ShopHub** — Entity Framework Core es el ORM:
```
// C# (lo que escribes):
var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);

// SQL (lo que EF Core genera):
// SELECT * FROM "Orders" WHERE "Id" = @id LIMIT 1
```

---

### 11.2 Change Tracking

**Teoría**: El DbContext mantiene un registro de todas las entidades que ha leído. Cuando llamas `SaveChanges`, compara el estado original con el actual y genera las sentencias SQL necesarias (INSERT, UPDATE, DELETE).

```
var order = await context.Orders.FirstAsync(o => o.Id == id);
// EF Core guarda una "foto" del estado original

order.MarkAsPaid();
// order.Status cambió de Pending a Paid

await context.SaveChangesAsync();
// EF Core detecta que Status cambió → genera: UPDATE Orders SET Status = 'Paid' WHERE Id = @id
```

---

### 11.3 Fluent API vs Data Annotations

**Teoría**: Dos formas de configurar el mapeo entre clases y tablas:
- **Data Annotations**: atributos en las propiedades (`[Required]`, `[MaxLength(256)]`). Simple pero contamina el dominio.
- **Fluent API**: configuración en clases separadas (`IEntityTypeConfiguration<T>`). Más potente y mantiene el dominio limpio.

**En ShopHub** — Se usa Fluent API exclusivamente para que el dominio no tenga atributos de EF Core:
```csharp
// Configuración SEPARADA del dominio:
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(256);
    }
}
```

---

### 11.4 Owned Types

**Teoría**: Un tipo que no tiene identidad propia y se almacena como parte de la tabla del dueño. Es la forma de mapear value objects a la base de datos sin crear tablas adicionales.

**En ShopHub** — `Address` y `Money` son owned types:
```
// Address se persiste como columnas de la tabla Orders:
// ShippingStreet, ShippingCity, ShippingState, ShippingCountry, ShippingZipCode
builder.OwnsOne(o => o.ShippingAddress, a =>
{
    a.Property(p => p.Street).HasColumnName("ShippingStreet");
    // ...
});
```

---

### 11.5 Eager Loading vs Lazy Loading vs Explicit Loading

**Teoría**:
- **Eager Loading** (`Include`): carga las entidades relacionadas en la misma query con JOIN.
- **Lazy Loading**: carga las relacionadas la primera vez que accedes a la propiedad de navegación (genera queries adicionales).
- **Explicit Loading**: cargas las relaciones manualmente cuando lo necesitas.

**En ShopHub** — Se usa **Eager Loading** con `Include`:
```csharp
return await context.Orders
    .Include(o => o.Items)   // ← JOIN con OrderItems en la misma query
    .FirstOrDefaultAsync(o => o.Id == id, ct);
```

Esto evita el **problema N+1**: sin `Include`, cargar 10 orders y sus items generaría 11 queries (1 para orders + 10 para items de cada order). Con `Include`, es 1 sola query.

---

## 12. APIs y Protocolos HTTP

---

### 12.1 REST (Representational State Transfer)

**Teoría**: Estilo arquitectónico para APIs web basado en recursos. Cada recurso tiene una URL y se manipula con verbos HTTP estándar.

| Verbo | Acción | Idempotente | Ejemplo |
|-------|--------|------------|---------|
| `GET` | Leer recurso(s) | Sí | `GET /orders` — listar pedidos |
| `POST` | Crear recurso | No | `POST /orders` — crear pedido |
| `PUT` | Reemplazar recurso | Sí | `PUT /products/123` — actualizar producto |
| `DELETE` | Eliminar recurso | Sí | `DELETE /products/123` — eliminar |
| `PATCH` | Modificar parcialmente | No | No usado en ShopHub |

---

### 12.2 Códigos de Estado HTTP

**Teoría**: Códigos numéricos que indican el resultado de la solicitud.

| Código | Significado | Cuándo se usa en ShopHub |
|--------|-----------|-------------------------|
| `200 OK` | Éxito | GET que encuentra el recurso, POST /orders/{id}/pay |
| `201 Created` | Recurso creado | POST /orders, POST /products |
| `204 No Content` | Éxito sin cuerpo | PUT /products/{id}, DELETE /products/{id} |
| `400 Bad Request` | Error del cliente | Validación fallida, regla de dominio violada |
| `404 Not Found` | Recurso no existe | GET /orders/{id} con id inexistente |
| `500 Internal Server Error` | Error del servidor | Excepción no manejada |

---

### 12.3 Minimal APIs

**Teoría**: Forma simplificada de definir endpoints en ASP.NET Core sin necesidad de controllers, modelos ni convenciones MVC. Ideal para microservicios donde la simplicidad importa.

**En ShopHub**:
```csharp
var group = app.MapGroup("/orders").WithTags("Orders");

group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var order = await mediator.Send(new GetOrderByIdQuery(id), ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
});
```

Elementos clave:
- `MapGroup("/orders")` — agrupa rutas bajo un prefijo.
- `{id:guid}` — route constraint: el parámetro debe ser un GUID válido.
- ASP.NET Core inyecta `IMediator` y `CancellationToken` automáticamente.
- `Results.Ok()`, `Results.NotFound()` — generan respuestas HTTP tipadas.

---

### 12.4 Serialización JSON

**Teoría**: Proceso de convertir un objeto de C# en texto JSON para transmitirlo por HTTP, y viceversa (deserialización). ASP.NET Core usa `System.Text.Json` por defecto.

```
// C# → JSON (serialización, al enviar respuesta):
OrderDto { Id = "abc-123", Status = "Pending", TotalAmount = 1500 }
→ {"id":"abc-123","status":"Pending","totalAmount":1500}

// JSON → C# (deserialización, al recibir request):
{"customerEmail":"cliente@mail.com","street":"Calle 1",...}
→ CreateOrderCommand { CustomerEmail = "cliente@mail.com", Street = "Calle 1", ... }
```

---

## 13. Arquitectura de Microservicios

---

### 13.1 Concepto

**Teoría**: Estilo arquitectónico donde una aplicación se compone de **servicios pequeños e independientes**, cada uno ejecutándose en su propio proceso, comunicándose por mecanismos ligeros (HTTP, mensajes). Cada servicio es desplegable, escalable y actualizable de forma independiente.

**En ShopHub**: 3 microservicios independientes:
- **Catalog.Api** — gestión de productos (MongoDB)
- **Ordering.Api** — gestión de pedidos (PostgreSQL)
- **Payment.Worker** — procesamiento de pagos (stateless)

---

### 13.2 Database per Service

**Teoría**: Cada microservicio tiene su **propia** base de datos. Nunca accede directamente a la BD de otro servicio. Esto garantiza autonomía y permite elegir la tecnología de BD más adecuada para cada caso.

**En ShopHub**:
- Catalog → MongoDB (documental, flexible)
- Ordering → PostgreSQL (relacional, ACID)
- Payment → sin BD (stateless, todo via eventos)

---

### 13.3 Comunicación entre Servicios

**Teoría**: Dos estilos principales:
- **Síncrona** (HTTP/gRPC): el servicio A llama al servicio B y **espera** la respuesta. Simple pero acopla temporalmente ambos servicios.
- **Asíncrona** (mensajes/eventos): el servicio A publica un evento y continúa. El servicio B lo procesa cuando puede. Desacopla temporalmente pero añade complejidad.

**En ShopHub** — Se usa comunicación **asíncrona** via eventos de MassTransit. Ordering publica `OrderCreatedIntegrationEvent` y NO espera respuesta de Payment.Worker.

---

### 13.4 Eventual Consistency (Consistencia Eventual)

**Teoría**: En un sistema distribuido, los datos pueden no estar sincronizados en todo momento, pero eventualmente convergerán al mismo estado. Es el trade-off de la comunicación asíncrona.

**En ShopHub**: Cuando se crea un pedido, Ordering lo guarda como `Pending`. Payment.Worker lo procesa **después** (con delay). El pedido cambia a `Paid` eventualmente, no instantáneamente.

---

## 14. Mensajería y Arquitectura Dirigida por Eventos

---

### 14.1 Event-Driven Architecture (EDA)

**Teoría**: Patrón donde los componentes se comunican a través de **eventos**. Un productor publica un evento sin saber quién lo consume. Uno o más consumidores reaccionan a ese evento. Desacopla completamente productor y consumidor.

---

### 14.2 Message Broker

**Teoría**: Infraestructura intermedia que recibe mensajes de productores y los entrega a consumidores. Ejemplos: RabbitMQ, Azure Service Bus, Amazon SQS, Apache Kafka.

**En ShopHub** — MassTransit abstrae el broker. En desarrollo usa **in-memory** (sin infraestructura externa). Para producción, se cambia a Azure Service Bus o RabbitMQ **sin cambiar el código de negocio**.

---

### 14.3 Publish/Subscribe (Pub/Sub)

**Teoría**: Patrón donde un publicador envía mensajes a un **topic/exchange**, y todos los suscriptores interesados reciben una copia. El publicador no conoce a los suscriptores.

**En ShopHub**:
```
Ordering publica:  OrderCreatedIntegrationEvent
    └── Payment.Worker lo consume (suscriptor)

Payment publica:   PaymentSucceededIntegrationEvent
    └── Ordering lo consume (suscriptor)
```

---

### 14.4 At-Least-Once Delivery

**Teoría**: El broker garantiza que un mensaje se entregará **al menos una vez**. Puede que se entregue más de una vez (por reintentos, fallos de red). El consumidor debe ser **idempotente**.

---

### 14.5 Idempotencia

**Teoría**: Una operación es idempotente si ejecutarla múltiples veces produce el mismo resultado que ejecutarla una sola vez. Es esencial en sistemas con at-least-once delivery.

**En ShopHub**:
```csharp
// PaymentSucceededConsumer verifica antes de actuar:
if (order.Status == OrderStatus.Paid)
{
    logger.LogInformation("Order ya estaba pagada (idempotente)");
    return;  // ← No hace nada si ya está pagada
}
order.MarkAsPaid();
```

---

### 14.6 Outbox Pattern

**Teoría**: Guarda los eventos a publicar en una tabla de la misma base de datos del servicio, **dentro de la misma transacción** que los cambios de dominio. Un proceso aparte lee esa tabla y publica los eventos al broker. Esto garantiza que si los datos se persisten, el evento también se persiste.

**En ShopHub** — MassTransit tiene soporte para Outbox con EF Core. El proyecto está preparado para activarlo (la infraestructura de domain events en `SaveChangesAsync` ya sigue este principio conceptualmente).

---

## 15. Testing de Software

---

### 15.1 ¿Por qué testear?

**Teoría**: Los tests son **especificaciones ejecutables** que verifican que el código funciona correctamente. Detectan bugs antes de que lleguen a producción, documentan el comportamiento esperado y permiten refactorizar con confianza.

---

### 15.2 Pirámide de Testing

```
         ╱  E2E  ╲        ← Pocos: lentos, frágiles, costosos
        ╱─────────╲
       ╱Integration╲      ← Algunos: verifican interacción entre componentes
      ╱─────────────╲
     ╱    Unit Tests  ╲    ← Muchos: rápidos, aislados, baratos
    ╱───────────────────╲
```

**En ShopHub**:
- **Unit tests** (29): prueban reglas de dominio sin dependencias → rápidos (~200ms).
- **Integration tests** (5): prueban persistencia real con Testcontainers → lentos (~3s).
- **E2E tests**: no implementados (se harían con un `WebApplicationFactory`).

---

### 15.3 Test Unitario

**Teoría**: Prueba una **unidad** de código (clase, método) en aislamiento total. Sin bases de datos, sin red, sin filesystem. Las dependencias se reemplazan por mocks/stubs.

**En ShopHub**:
```csharp
[Fact]
public void MarkAsPaid_DesdeCancelled_LanzaDomainException()
{
    var order = Order.Create("test@mail.com", DefaultAddress);
    order.Cancel();

    var act = () => order.MarkAsPaid();  // ← Actúa

    act.Should().Throw<DomainException>();  // ← Verifica
}
```

No hay base de datos, no hay HTTP, no hay contenedores. Solo la lógica de dominio pura.

---

### 15.4 Test de Integración

**Teoría**: Prueba la **interacción** entre componentes reales. Por ejemplo, tu repositorio contra una base de datos real. Verifica que la configuración, el mapeo y las queries funcionan correctamente.

**En ShopHub** — Testcontainers levanta un PostgreSQL real en Docker:
```csharp
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();  // ← Levanta un PostgreSQL real en un contenedor
        // ... configura DbContext con la connection string del contenedor
        await DbContext.Database.MigrateAsync();  // ← Aplica migraciones reales
    }
}
```

---

### 15.5 Arrange-Act-Assert (AAA)

**Teoría**: Patrón para estructurar tests:
1. **Arrange**: prepara el escenario (crea objetos, configura mocks).
2. **Act**: ejecuta la acción que quieres probar.
3. **Assert**: verifica que el resultado es el esperado.

```csharp
[Fact]
public void TotalAmount_ConVariosItems_SumaCorrectamente()
{
    // ARRANGE
    var order = Order.Create("test@mail.com", DefaultAddress);
    order.AddItem(Guid.NewGuid(), "Laptop", Money.Create(1000m, "USD"), 1);
    order.AddItem(Guid.NewGuid(), "Mouse", Money.Create(50m, "USD"), 2);

    // ACT (implícito: acceder a la propiedad calculada)
    var total = order.TotalAmount;

    // ASSERT
    total.Amount.Should().Be(1100m);  // 1000 + (50 × 2)
}
```

---

### 15.6 Mocking

**Teoría**: Reemplazar una dependencia real por una versión falsa controlada. Un **mock** verifica que se llamaron ciertos métodos. Un **stub** solo devuelve valores predefinidos.

**En ShopHub** — NSubstitute crea mocks:
```csharp
var mediator = Substitute.For<IMediator>();  // ← Mock de IMediator
// El DbContext no sabe que el mediator es falso
```

---

### 15.7 FluentAssertions

**Teoría**: Librería que hace las aserciones más legibles que las nativas de xUnit.

```csharp
// xUnit nativo:
Assert.Equal("Pending", order.Status.ToString());
Assert.Throws<DomainException>(() => order.MarkAsPaid());

// FluentAssertions (más legible):
order.Status.Should().Be(OrderStatus.Pending);
act.Should().Throw<DomainException>();
order.Items.Should().ContainSingle();
order.TotalAmount.Amount.Should().Be(1100m);
```

---

## 16. Contenedores y Docker

---

### 16.1 ¿Qué es un Contenedor?

**Teoría**: Un paquete ligero que incluye la aplicación y todas sus dependencias (runtime, librerías, configuración). Se ejecuta de forma aislada del sistema operativo host. A diferencia de una máquina virtual, comparte el kernel del host, haciéndolo mucho más ligero.

---

### 16.2 Docker Image vs Docker Container

**Teoría**:
- **Image**: plantilla inmutable de solo lectura. Es como una clase.
- **Container**: instancia en ejecución de una image. Es como un objeto.

Puedes crear muchos containers a partir de la misma image.

---

### 16.3 Dockerfile

**Teoría**: Archivo de texto con instrucciones para construir una Docker image. Cada instrucción crea una **capa** en la imagen.

**En ShopHub** — Multi-stage build:
```dockerfile
# Stage 1: SDK completo para compilar (~800MB)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Solo runtime para ejecutar (~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
```

La imagen final solo contiene el runtime + los DLLs compilados. El SDK (compilador, herramientas) no se incluye.

---

### 16.4 Docker Compose

**Teoría**: Herramienta para definir y ejecutar aplicaciones multi-contenedor. Un archivo YAML describe todos los servicios, redes y volúmenes.

**En ShopHub**:
```yaml
services:
  mongo:
    image: mongo:7
    container_name: shophub-mongo
    ports: ["27017:27017"]       # hostPort:containerPort
    volumes: [mongo-data:/data/db]  # Persistencia de datos

  postgres:
    image: postgres:15
    container_name: shophub-postgres
    ports: ["5433:5432"]
    environment:
      POSTGRES_DB: ShopHubOrdering
```

`docker compose up -d` levanta todo con un solo comando.

---

### 16.5 Volúmenes

**Teoría**: Mecanismo para persistir datos más allá del ciclo de vida de un contenedor. Sin volúmenes, al destruir un contenedor se pierden todos sus datos.

**En ShopHub**: `mongo-data` y `postgres-data` son volúmenes que sobreviven a reinicios de contenedores.

---

## 17. CI/CD — Integración y Entrega Continua

---

### 17.1 Continuous Integration (CI)

**Teoría**: Práctica de integrar código frecuentemente (varias veces al día). Cada integración se verifica con un **build automatizado** y **tests automatizados** para detectar errores temprano.

---

### 17.2 Continuous Delivery/Deployment (CD)

**Teoría**:
- **Continuous Delivery**: el código siempre está en estado desplegable. El deploy a producción es manual.
- **Continuous Deployment**: cada cambio que pasa los tests se despliega automáticamente a producción.

---

### 17.3 Pipeline

**Teoría**: Secuencia automatizada de pasos que llevan el código desde el commit hasta el deploy.

**En ShopHub** — `azure-pipelines.yml`:
```
Stage 1: Build_Test
  ├── Instalar .NET SDK
  ├── dotnet restore
  ├── dotnet build --configuration Release
  ├── dotnet test con cobertura de código
  ├── Publicar resultados de tests
  └── Publicar cobertura

Stage 2: Containerize (solo si Stage 1 pasó)
  ├── Docker build Catalog.Api
  ├── Docker build Ordering.Api
  └── Docker build Payment.Worker
```

---

## 18. Computación en la Nube (Cloud Computing)

---

### 18.1 Concepto

**Teoría**: Provisión de recursos computacionales (servidores, almacenamiento, bases de datos, redes) bajo demanda a través de internet. Los tres modelos principales son:

| Modelo | Qué gestionas tú | Qué gestiona el cloud | Ejemplo |
|--------|------------------|-----------------------|---------|
| **IaaS** | Apps, datos, runtime, OS | Virtualización, servidores, red | VMs de Azure/AWS |
| **PaaS** | Apps y datos | Todo lo demás | Azure App Service, Azure SQL |
| **SaaS** | Nada (solo usas) | Todo | Gmail, Office 365 |

---

### 18.2 Multi-Cloud

**Teoría**: Estrategia de usar servicios de múltiples proveedores cloud para evitar vendor lock-in, aprovechar las fortalezas de cada proveedor y mejorar la resiliencia.

**En ShopHub** — `IObjectStorage` abstrae el almacenamiento para que el código funcione con Azure o AWS:
```csharp
// Configuración en appsettings.json:
// "Storage:Provider": "Azure"  → usa Azure Blob Storage
// "Storage:Provider": "Aws"    → usa Amazon S3
// El código de negocio NO cambia
```

---

### 18.3 Azure Service Bus

**Teoría**: Servicio de mensajería empresarial de Azure que soporta colas (point-to-point) y topics/subscriptions (pub/sub). Garantiza entrega at-least-once, ordenamiento y transacciones.

**En ShopHub** — MassTransit está listo para Azure Service Bus. Solo cambiando una línea de configuración:
```csharp
// Desarrollo (actual):
x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });

// Producción (Azure):
x.UsingAzureServiceBus((context, cfg) =>
{
    cfg.Host("Endpoint=sb://...");
    cfg.ConfigureEndpoints(context);
});
```

---

### 18.4 Blob Storage / Object Storage

**Teoría**: Almacenamiento de objetos binarios (imágenes, videos, documentos) en la nube. Cada objeto tiene una URL única, no hay jerarquía de directorios (es plano). Altamente escalable y económico.

**En ShopHub** — `AzureBlobStorage` usa el SDK `Azure.Storage.Blobs`:
```csharp
var container = client.GetBlobContainerClient(containerName);
await container.CreateIfNotExistsAsync(cancellationToken: ct);
var blob = container.GetBlobClient(blobName);
await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
return blob.Uri.ToString();  // ← URL pública del archivo
```

---

## 19. Logging y Observabilidad

---

### 19.1 Logging Estructurado

**Teoría**: En vez de guardar logs como texto plano, los logs tienen **propiedades tipadas** que se pueden filtrar y buscar. En vez de `"Error procesando order 123"`, generas `{Level: "Error", OrderId: 123, Event: "ProcessingFailed"}`.

**En ShopHub** — Serilog con propiedades:
```csharp
logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);
//                              ↑ propiedad tipada   ↑ serialización del objeto completo
```

El `{RequestName}` no es un string format — Serilog lo guarda como una propiedad estructurada que puedes buscar en herramientas como Seq, Elasticsearch o Application Insights.

---

### 19.2 Niveles de Log

| Nivel | Cuándo usarlo |
|-------|--------------|
| `Trace` | Detalle extremo (valores de variables) |
| `Debug` | Información de diagnóstico |
| `Information` | Flujo normal de la aplicación |
| `Warning` | Algo inesperado pero no fatal |
| `Error` | Un error que impide completar una operación |
| `Critical` | La aplicación no puede continuar |

---

### 19.3 Health Checks

**Teoría**: Endpoints que reportan si la aplicación y sus dependencias están saludables. Útiles para load balancers (quitar instancias enfermas) y alertas de monitoreo.

**En ShopHub**:
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderingDbContext>();  // ← Verifica que PostgreSQL responde

app.MapHealthChecks("/health");
// GET /health → 200 "Healthy" o 503 "Unhealthy"
```

---

## 20. Seguridad en Aplicaciones Web

---

### 20.1 Validación de Entrada

**Teoría**: **Nunca** confíes en los datos que vienen del usuario. Toda entrada debe validarse antes de procesarse. La validación previene inyecciones, datos corruptos y violaciones de reglas.

**En ShopHub** — Dos niveles de validación:
1. **FluentValidation** (Application): valida el formato del command antes de ejecutar el handler.
2. **Validación de dominio**: las entidades validan las reglas de negocio en sus factory methods.

```csharp
// Nivel 1 (FluentValidation):
RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
RuleFor(x => x.Items).NotEmpty().WithMessage("El pedido debe tener al menos un item.");

// Nivel 2 (Dominio):
if (string.IsNullOrWhiteSpace(customerEmail))
    throw new DomainException("El email del cliente es obligatorio.");
```

---

### 20.2 Manejo Centralizado de Errores

**Teoría**: En vez de poner try/catch en cada endpoint, un middleware global captura todas las excepciones y las convierte en respuestas HTTP apropiadas. Esto evita que detalles internos (stack traces, connection strings) se filtren al cliente.

**En ShopHub** — `GlobalExceptionHandler`:
```csharp
// DomainException → 400 con el mensaje de negocio
// ValidationException → 400 con los errores de validación
// KeyNotFoundException → 404
// Cualquier otra cosa → 500 con mensaje genérico (SIN stack trace)
```

---

### 20.3 Principio de Mínimo Privilegio

**Teoría**: Cada componente debe tener solo los permisos que necesita, ni más ni menos. Clases `internal` y `private` limitan la visibilidad.

**En ShopHub**:
- Los handlers son `internal sealed` — solo el assembly de Application los conoce.
- `OrderItem.Create()` es `internal` — solo `Order` (en el mismo assembly) puede crear items.
- Los constructores son `private` — solo los factory methods pueden instanciar.

---

## 21. Control de Versiones con Git

---

### 21.1 Commits

**Teoría**: Un commit es una **instantánea** del estado del código en un momento dado. Cada commit tiene un hash único, un mensaje descriptivo, autor y fecha. Los commits forman un historial lineal (o con ramas) que permite viajar en el tiempo.

**En ShopHub** — Commits incrementales por bloque de trabajo:
```
cb9bacb Guía técnica completa del proyecto ShopHub
b7de6cc P5: Documentación completa para entrevista
d43f38f P4: Cloud/DevOps demostradores
ffe5834 P3: Event-Driven Architecture con MassTransit in-memory
6bcccb3 P2: Refactor de Catalog a CQRS con MediatR
56bf3d9 P1: Microservicio Ordering con DDD rico, EF Core, CQRS y MediatR
d41f9bc Fase 3: persistencia en MongoDB
d369843 Estructura inicial
```

---

### 21.2 Ramas (Branches)

**Teoría**: Líneas de desarrollo paralelas. La rama `main` contiene el código estable. Las ramas de feature permiten trabajar en aislamiento sin afectar a otros.

---

## 22. Conceptos de Rendimiento

---

### 22.1 AsNoTracking

**Teoría**: Desactiva el Change Tracking de EF Core para queries de solo lectura. Reduce el consumo de memoria y mejora el rendimiento porque EF Core no necesita guardar una copia del estado original de cada entidad.

**En ShopHub**:
```csharp
// Con tracking (para escritura): EF Core guarda estado → más lento, más memoria
var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);

// Sin tracking (para lectura): EF Core NO guarda estado → más rápido, menos memoria
var orders = await context.Orders.AsNoTracking().ToListAsync();
```

---

### 22.2 Problema N+1

**Teoría**: Anti-patrón donde cargar N entidades padre genera N queries adicionales para cargar sus hijos. Si tienes 100 pedidos, se ejecutan 101 queries (1 para pedidos + 100 para items de cada uno).

**Solución**: Eager loading con `Include`:
```csharp
// ❌ N+1: 1 query para orders + N queries para items (una por order)
var orders = await context.Orders.ToListAsync();
foreach (var order in orders)
    var items = order.Items;  // ← Cada acceso genera un SELECT

// ✅ 1 query con JOIN:
var orders = await context.Orders.Include(o => o.Items).ToListAsync();
```

---

### 22.3 Singleton vs Scoped — Impacto en Rendimiento

**Teoría**: Crear objetos tiene un costo. Los Singletons se crean una vez; los Scoped se crean una vez por request; los Transient se crean cada vez que se piden.

```
IMongoClient → Singleton: 1 instancia para toda la app (crear un cliente Mongo es costoso)
OrderingDbContext → Scoped: 1 instancia por request HTTP (el Change Tracker es por request)
```

Si un `DbContext` fuera Singleton, todos los requests compartirían el mismo Change Tracker, causando condiciones de carrera y corrupción de datos.

---

> **Este documento cubre los fundamentos teóricos necesarios para comprender, mantener y extender el proyecto ShopHub. Cada concepto está anclado a código real del repositorio.**
