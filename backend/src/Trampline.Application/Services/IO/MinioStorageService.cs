using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Trampline.Core.Storage;

namespace Trampline.Application.Services.IO;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration config, ILogger<MinioStorageService> logger)
    {
        _logger = logger;
        var endpoint = config["Minio:Endpoint"] ?? "minio:9000";
        var accessKey = config["Minio:AccessKey"] ?? "minioadmin";
        var secretKey = config["Minio:SecretKey"] ?? "minioadmin";
        _bucket = config["Minio:Bucket"] ?? "trampline";
        var useSsl = config.GetValue<bool>("Minio:UseSsl");

        var builder = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey);

        if (useSsl) builder = builder.WithSSL();

        _client = builder.Build();
    }

    public async Task EnsureBucketAsync(CancellationToken ct = default)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
            _logger.LogInformation("MinIO: created bucket {Bucket}", _bucket);
        }

        var policy = $$"""
        {
            "Version": "2012-10-17",
            "Statement": [
                {
                    "Effect": "Allow",
                    "Principal": {"AWS": ["*"]},
                    "Action": ["s3:GetObject"],
                    "Resource": [
                        "arn:aws:s3:::{{_bucket}}/photos/*",
                        "arn:aws:s3:::{{_bucket}}/videos/*"
                    ]
                }
            ]
        }
        """;

        await _client.SetPolicyAsync(new SetPolicyArgs()
            .WithBucket(_bucket)
            .WithPolicy(policy), ct);

        _logger.LogInformation("MinIO: set public read policy for photos/ and videos/");
    }

    public async Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken ct = default)
    {
        var objectName = NormalizeKey(path);
        stream.Position = 0;

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType), ct);

        _logger.LogInformation("MinIO: uploaded {Key} ({Size} bytes)", objectName, stream.Length);
        return path;
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var objectName = NormalizeKey(path);
        try
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName), ct);
            _logger.LogInformation("MinIO: deleted {Key}", objectName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MinIO: failed to delete {Key}", objectName);
        }
    }

    public async Task<Stream?> GetAsync(string path, CancellationToken ct = default)
    {
        var objectName = NormalizeKey(path);
        var ms = new MemoryStream();
        try
        {
            await _client.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms)), ct);
            ms.Position = 0;
            return ms;
        }
        catch
        {
            await ms.DisposeAsync();
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var objectName = NormalizeKey(path);
        try
        {
            await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName), ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeKey(string path)
    {
        var key = path.TrimStart('/');
        return key.StartsWith("files/") ? key[6..] : key;
    }
}
