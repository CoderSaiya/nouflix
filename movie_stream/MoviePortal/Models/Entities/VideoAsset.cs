using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Entities;

public class VideoAsset
{
    public int Id { get; set; }
    public int? MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int? EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    
    public VideoKind Kind { get; set; } = VideoKind.Primary;
    public QualityLevel Quality { get; set; }
    public string Language { get; set; } = "vi";
    public string? Subtitles { get; set; }

    // Azure Blob
    public string Bucket { get; set; } = "";
    public string ObjectKey { get; set; } = "";
    public string? Endpoint { get; set; }
    public string? CdnBase { get; set; }
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? ETag { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    public PublishStatus Status { get; set; } = PublishStatus.Published;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}