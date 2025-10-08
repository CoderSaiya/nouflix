using NouFlix.Models.Specification;

namespace MoviePortal.Models.Specification;

public class StorageOptions
{
    public S3Settings S3 { get; set; } = new();
    public BucketSettings Buckets { get; set; } = new();
}