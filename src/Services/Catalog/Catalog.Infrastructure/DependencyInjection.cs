using Catalog.Application.Products;
using Catalog.Infrastructure.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB.Driver 3.x ya no asume una representación global de Guid:
        // la fijamos explícitamente para guardarlos en formato estándar (UUID).
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        var connectionString = configuration["MongoDb:ConnectionString"]
            ?? throw new InvalidOperationException("Falta 'MongoDb:ConnectionString' en la configuración.");
        var databaseName = configuration["MongoDb:DatabaseName"]
            ?? throw new InvalidOperationException("Falta 'MongoDb:DatabaseName' en la configuración.");

        // El cliente de Mongo es thread-safe y costoso de crear: una sola instancia (Singleton).
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        // El repositorio ya no guarda datos en sí mismo, así que Scoped es lo correcto.
        // (El de memoria era Singleton porque ÉL era el almacén; ahora el almacén es Mongo.)
        services.AddScoped<IProductRepository, MongoProductRepository>();

        return services;
    }
}