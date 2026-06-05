using Catalog.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application;

// Cada capa registra sus propios servicios. Así Program.cs queda limpio
// y la capa es autónoma (sabe qué necesita inyectar).
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}