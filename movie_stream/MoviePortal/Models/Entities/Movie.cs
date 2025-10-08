using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.Entities;

public class Movie
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(256)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string AlternateTitle { get; set; } = "";
    [MaxLength(256)] public string Slug { get; set; } = ""; // unique

    public string Synopsis { get; set; } = "";
    public string Director { get; set; } = "";
    public string Country { get; set; } = "";
    public string Language { get; set; } = "";

    public double Rating { get; set; } = 0.0;
    public int TotalRatings { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int Followers { get; set; } = 0;

    public string AgeRating { get; set; } = "";
    public DateTime? ReleaseDate { get; set; }

    public MovieType Type { get; set; } = MovieType.Single;
    public PublishStatus Status { get; set; } = PublishStatus.Draft;

    public QualityLevel Quality { get; set; } = QualityLevel.Low;
    public bool IsVipOnly { get; set; } = false;

    // Timestamps & soft delete & concurrency
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    [Timestamp] public byte[]? RowVersion { get; set; }
    public TimeSpan? Runtime { get; set; }
    [NotMapped]
    public int TotalDurationMinutes =>
        Type == MovieType.Single
            ? (int)Math.Round((Runtime ?? TimeSpan.Zero).TotalMinutes)
            : (int)Math.Round(Episodes?.Sum(e => e.Duration?.TotalMinutes ?? 0) ?? 0);

    // Navigation
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<ImageAsset> Images { get; set; } = new List<ImageAsset>();
    public ICollection<VideoAsset> Videos { get; set; } = new List<VideoAsset>(); // cho phim lẻ / trailer
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieStudio> MovieStudios { get; set; } = new List<MovieStudio>();
    // public ICollection<History> Histories { get; set; } = new List<History>();
}