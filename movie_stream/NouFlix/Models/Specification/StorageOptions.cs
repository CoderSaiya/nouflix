namespace NouFlix.Models.Specification;

public class StorageOptions
{
    public S3Settings S3 { get; set; } = new();
    public BucketSettings Buckets { get; set; } = new();
    
    public class S3Settings
    {
        public string Endpoint { get; set; } = "";
        public string? PublicEndpoint { get; set; }
        public bool UseSSL { get; set; } = false;
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string? Region { get; set; }
        public int DefaultPresignSeconds { get; set; } = 900;
    }
    
    public class BucketSettings
    {
        public string Images { get; set; } = "images";
        public string Videos { get; set; } = "videos";
        public string Temps { get; set; } = "temps";
    }
}