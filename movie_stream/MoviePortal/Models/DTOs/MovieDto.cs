using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.DTOs;

public class MovieDto
{
    public record MovieSummary(
        int Id,
        string Slug,
        string Title,
        string PosterUrl,
        string Type,
        string Status,
        float AvgRating,
        int ViewCount,
        DateTime? ReleaseDate);

    public record Movie
    {
        public int Id { get; init; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string AlternateTitle { get; set; }
        public string Overview { get; set; }
        public string PosterUrl { get; set; }
        public string BackdropUrl { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Director { get; set; }
        public int Runtime { get; set; }
        public float AvgRating { get; set; }
        public int VoteCount { get; set; }
        public int Popularity { get; set; }
        public string AgeRating { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Studio> Studios { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public PublishStatus Status { get; set; }
        public MovieType Type { get; set; }
        public QualityLevel Quality { get; set; }
        public bool IsVipOnly { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public record Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    
    public record Studio
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public record VideoAssets
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public int? EpisodeId { get; set; }
        public VideoKind Kind { get; set; }
        public QualityLevel Quality { get; set; }
        public string Bucket { get; set; }
        public string ObjectKey { get; set; }
    }

    public record Season
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
    }

    public record Episode
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int? SeasonId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
        public DateTime ReleaseDate { get; set; }
        public PublishStatus Status { get; set; }
    }
    
    public record UpsertMovieReq(
        string Title, 
        string AlternateTitle,
        string Slug,
        string Synopsis,
        string Director,
        string Country,
        string Language,
        string AgeRating,
        DateTime ReleaseDate,
        MovieType Type,
        PublishStatus Status,
        QualityLevel Quality,
        bool IsVipOnly,
        IEnumerable<int> GenreIds,
        IEnumerable<int> StudioIds
    );

    public record UpsertEpisodeReq(int MovieId, int SeasonId, int EpisodeNumber, string Title, PublishStatus Status, DateTime ReleaseDate);
    
    public record CreateSeasonReq(int Number, string Title, int Year);

    public record UpdateSeasonReq(string Title);
}