using MoviePortal.Models.Entities;

namespace MoviePortal.Helpers;

public static class StorageHelper
{
    public static string? BuildUrl(ImageAsset img)
    {
        if (string.IsNullOrWhiteSpace(img.Endpoint)) return null;

        var baseHost = img.Endpoint!.TrimEnd('/'); // ví dụ: "localhost:9009"
        var bucketSeg = Uri.EscapeDataString(img.Bucket ?? ""); // "images"

        var keySegs = (img.ObjectKey ?? "")
            .Split('/', StringSplitOptions.RemoveEmptyEntries) // giữ nguyên dấu '/'
            .Select(Uri.EscapeDataString); // encode từng segment

        return $"{baseHost}/{bucketSeg}/{string.Join("/", keySegs)}";
    }
    
    public static string? BuildUrlWithHttp(ImageAsset img)
    {
        var url = BuildUrl(img);
        if (url is null) return null;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return "http://" + url;
    }
}