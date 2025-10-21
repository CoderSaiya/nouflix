using System.ComponentModel.DataAnnotations.Schema;
using NouFlix.Models.Common;
using NouFlix.Models.ValueObject;

namespace NouFlix.Models.Entities;

public class ImageAsset : AssetBase
{
    public int Id { get; set; }
    public int? MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie? Movie { get; set; }

    public int? EpisodeId { get; set; }
    [ForeignKey("EpisodeId")]
    public Episode? Episode { get; set; }
    public Guid? ProfileId { get; set; }
    [ForeignKey("ProfileId")]
    public Profile? Profile { get; set; }
    
    public ImageKind Kind { get; set; } = ImageKind.Poster;
    public string Alt { get; set; } = "";

    // S3 MinIO
    public string? CdnBase { get; set; } // https://cdn.example.com
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? ETag { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    
    // index n8n
    // public string Status { get; set; } = "pending"; // pending|processing|indexed|failed
    // public int RetryCount { get; set; } = 0;
    // public string? LastError { get; set; }
    // public string ModelVersion { get; set; } = "clip_vitb32_v1";
    // public long? Phash { get; set; }
    // public int? TsSeconds { get; set; }
    // public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}