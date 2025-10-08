using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Views;

public class EpisodeCsvPreviewRow
{
    public int RowNumber { get; set; }

    public int MovieId { get; set; }
    public string MovieDisplay { get; set; } = "";

    public int Number { get; set; }
    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public bool? IsVipOnly { get; set; }
    public int? SeasonNumber { get; set; }

    public PublishStatus? Status { get; set; }
    public string StatusText { get; set; } = "";

    public QualityLevel? Quality { get; set; }
    public string QualityText { get; set; } = "";

    public bool Exists { get; set; }
    public bool IsValid { get; set; }
    public string? Error { get; set; }
}