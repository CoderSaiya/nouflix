using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NouFlix.Models.ValueObject;

namespace NouFlix.Models.Entities;

public class Episode
{
    [Key] public int Id { get; set; }
    [Required] public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int? SeasonId { get; set; }
    public Season? Season { get; set; }

    [Required] public int Number { get; set; } // số tập (unique theo Movie hoặc theo Season)
    [MaxLength(256)] public string Title { get; set; } = "";
    public string Synopsis { get; set; } = "";
    public TimeSpan? Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Published;
    public QualityLevel Quality { get; set; } = QualityLevel.Low;
    public bool IsVipOnly { get; set; } = false;
    [NotMapped] public int TotalDurationMinutes => (int)Math.Round((Duration ?? TimeSpan.Zero).TotalMinutes);
    [NotMapped] public int TotalDurationSeconds => (int)Math.Round((Duration ?? TimeSpan.Zero).TotalSeconds);

    public ICollection<VideoAsset> Videos { get; set; } = new List<VideoAsset>();
    public ICollection<ImageAsset> Images { get; set; } = new List<ImageAsset>();
}