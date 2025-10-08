namespace MoviePortal.Models.Entities;

public class SubtitleAsset
{
    public int Id { get; set; }
    public int? MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int? EpisodeId { get; set; }
    public Episode? Episode { get; set; }
    
    public string Kind { get; set; } = "subtitles";
    public string Language { get; set; } = "vi";
    public string Label { get; set; } = "Tiếng Việt";

    public string Bucket { get; set; } = "";
    public string ObjectKey { get; set; } = "";
    public string? Endpoint { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}