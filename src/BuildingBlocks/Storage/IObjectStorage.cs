namespace BuildingBlocks.Storage;

public interface IObjectStorage
{
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken);
    Task<Stream?> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken);
    Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken);
}
