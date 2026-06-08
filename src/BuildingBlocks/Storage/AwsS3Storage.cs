using Amazon.S3;
using Amazon.S3.Model;

namespace BuildingBlocks.Storage;

public sealed class AwsS3Storage(IAmazonS3 s3Client) : IObjectStorage
{
    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = containerName,
            Key = blobName,
            InputStream = content,
            ContentType = contentType
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        return $"https://{containerName}.s3.amazonaws.com/{blobName}";
    }

    public async Task<Stream?> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(containerName, blobName, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken)
    {
        await s3Client.DeleteObjectAsync(containerName, blobName, cancellationToken);
    }
}
