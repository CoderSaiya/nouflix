using MoviePortal.Models.Common;
using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Entities;

public class ImageAsset : AssetBase
{
    public int Id { get; set; }
    public int? MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int? EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    
    public ImageKind Kind { get; set; } = ImageKind.Poster;
    public string Alt { get; set; } = "";

    // S3 MinIO
    public string? CdnBase { get; set; } // https://cdn.example.com
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? ETag { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}