using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using NouFlix.DTOs;
using NouFlix.Models.Specification;

namespace NouFlix.Services;

public class MinioObjectStorage
{
    private readonly IMinioClient _client;
    private readonly IMinioClient _publicSigner;
    private readonly S3Settings _cfg;

    public MinioObjectStorage(IOptions<StorageOptions> options)
    {
        _cfg = options.Value.S3;

        _client = BuildClient(NormalizeEndpoint(_cfg.Endpoint), useSsl: _cfg.UseSSL);

        // Nếu có PublicEndpoint thì dùng nó cho việc ký URL, ngược lại fallback về Endpoint
        var (pubEp, pubSsl) = NormalizePublic(_cfg.PublicEndpoint ?? _cfg.Endpoint, _cfg.UseSSL);
        _publicSigner = BuildClient(pubEp, useSsl: pubSsl);
    }

    public async Task<Uri> GetReadSignedUrlAsync(string bucket, string objectKey, TimeSpan ttl, int audience = 1, CancellationToken ct = default)
    {
        var cli = audience == 1 ? _publicSigner : _client;
        
        var url = await cli.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry((int)ttl.TotalSeconds));
        return new Uri(url);
    }
    
    public async Task<Uri> GetWriteSignedUrlAsync(string bucket, string objectKey, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var seconds = (int)(ttl?.TotalSeconds ?? _cfg.DefaultPresignSeconds);
        var url = await _client.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry(seconds));
        return new Uri(url);
    }
    
    public async Task<UploadResult> UploadAsync(string bucket, Stream stream, string objectName, string? contentType, CancellationToken ct = default)
    {
        // ensure bucket
        bool found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), ct);
        if (!found) await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);

        // upload
        var put = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType ?? "application/octet-stream");

        await _client.PutObjectAsync(put, ct);

        // ETag chưa có API trả trực tiếp → nếu cần, gọi StatObject
        var stat = await _client.StatObjectAsync(new StatObjectArgs().WithBucket(bucket).WithObject(objectName), ct);

        return new UploadResult(bucket, objectName, contentType, stat.Size, stat.ETag);
    }
    
    private static string NormalizeEndpoint(string ep)
        => ep.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(ep).Authority : ep;

    // Nếu PublicEndpoint có kèm scheme (https://), suy ra SSL cho signer; nếu không thì dùng cờ UseSSL
    private static (string endpoint, bool useSsl) NormalizePublic(string? publicEp, bool fallbackSsl)
    {
        if (string.IsNullOrWhiteSpace(publicEp))
            return (publicEp ?? string.Empty, fallbackSsl);

        if (publicEp.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var u = new Uri(publicEp);
            var host = u.IsDefaultPort ? u.Host : $"{u.Host}:{u.Port}";
            return (host, u.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase));
        }
        return (publicEp, fallbackSsl);
    }

    private IMinioClient BuildClient(string endpoint, bool useSsl)
    {
        var b = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(_cfg.AccessKey, _cfg.SecretKey);

        if (useSsl) b = b.WithSSL();
        if (!string.IsNullOrWhiteSpace(_cfg.Region)) b = b.WithRegion(_cfg.Region);

        return b.Build();
    }
}