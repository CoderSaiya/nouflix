namespace NouFlix.Models.Common;

public class AssetBase
{
    public string Bucket { get; set; } = "";
    public string ObjectKey { get; set; } = "";
    public string? Endpoint { get; set; }
}