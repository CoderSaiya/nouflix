namespace NouFlix.DTOs;

public record UploadResult(
    string Bucket, string ObjectKey,
    string? ContentType, long? SizeBytes, string? ETag);