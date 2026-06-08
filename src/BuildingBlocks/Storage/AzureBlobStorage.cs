using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BuildingBlocks.Storage;

public sealed class AzureBlobStorage(BlobServiceClient client) : IObjectStorage
{
    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var container = client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        return blob.Uri.ToString();
    }

    public async Task<Stream?> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        var container = client.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync(cancellationToken))
            return null;

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        var container = client.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
