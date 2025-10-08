namespace NouFlix.Models.Specification;

public class S3Settings
{
    public string Endpoint { get; set; } = "";
    public bool UseSSL { get; set; } = false;
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string? Region { get; set; }
    public int DefaultPresignSeconds { get; set; } = 900;
}