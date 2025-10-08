using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Views;

public class EpisodeCsvRawRow
{
    public int? MovieId { get; set; }
    public string? MovieSlug { get; set; }
    public string? MovieTitle { get; set; }

    public int? SeasonNumber { get; set; }
    public int? Number { get; set; }

    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public PublishStatus? Status { get; set; }
    public QualityLevel? Quality { get; set; }
    public bool? IsVipOnly { get; set; }
}