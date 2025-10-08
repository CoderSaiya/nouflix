using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Views;

public class BulkForm
{
    public int? MovieId { get; set; }
    public int StartNumber { get; set; } = 1;
    public int Count { get; set; } = 10;
    public string TitlePattern { get; set; } = "Tập {n}";
    public string? Synopsis { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseStartDate { get; set; }
    public int ReleaseIntervalDays { get; set; } = 7;

    public PublishStatus Status { get; set; } = PublishStatus.Published;
    public QualityLevel Quality { get; set; } = QualityLevel.Medium;
    public bool IsVipOnly { get; set; } = false;

    public bool OnlyCreateMissing { get; set; } = true;
    public bool OverwriteExisting { get; set; } = false;
}