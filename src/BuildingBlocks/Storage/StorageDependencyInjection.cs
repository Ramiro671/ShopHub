using Amazon.S3;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Storage;

public static class StorageDependencyInjection
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"] ?? "Azure";

        switch (provider)
        {
            case "Azure":
                var azureConnStr = configuration["Storage:Azure:ConnectionString"]
                    ?? "UseDevelopmentStorage=true";
                services.AddSingleton(_ => new BlobServiceClient(azureConnStr));
                services.AddSingleton<IObjectStorage, AzureBlobStorage>();
                break;

            case "Aws":
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client());
                services.AddSingleton<IObjectStorage, AwsS3Storage>();
                break;

            default:
                throw new InvalidOperationException($"Proveedor de almacenamiento no soportado: {provider}");
        }

        return services;
    }
}
