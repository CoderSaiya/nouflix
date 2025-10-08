namespace MoviePortal.Helpers;

public static class UrlHelper
{
    public static string BuildPublicUrl(string? cdnBase, string? endpoint, string bucket, string objectKey)
    {
        var baseUrl = !string.IsNullOrWhiteSpace(cdnBase)
            ? cdnBase.TrimEnd('/')
            : (endpoint?.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true
                ? endpoint.TrimEnd('/')
                : $"https://{endpoint}".TrimEnd('/'));
        return $"{baseUrl}/{bucket}/{objectKey}";
    }
}