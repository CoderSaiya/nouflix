using System.Text.Json.Serialization;
using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public class SystemDto
{
    public sealed record HealthRes(
        string Name,
        HealthState State,
        string Message,
        long DurationMs);
    
    public sealed record SystemInfo(
        string Environment,
        string MachineName,
        string OSVersion,
        string Framework,
        string ProcessArchitecture,
        DateTime StartedAtUtc,
        TimeSpan Uptime);
    
    public sealed record SystemHealthReport(
        SystemInfo Info,
        IReadOnlyList<HealthRes> Checks);
    
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
    
    public sealed record EpisodeCsvPreviewReq(string CsvText);
    
    public sealed record EpisodeCsvImportReq(
        IEnumerable<SystemDto.EpisodeCsvPreviewRow> Preview,
        bool Overwrite,
        bool AutoCreateSeason);
    
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
    public readonly record struct EpisodeCsvImportResult(int Created, int Updated, int Skipped, int Failed);
    
    public sealed record MoviePick(int Id, string Title, MovieType Type);
    
    public sealed record MovieInfoRes(
        int Id,
        string Title,
        MovieType Type,
        PublishStatus Status,
        QualityLevel Quality,
        bool IsVipOnly,
        DateTime? ReleaseDate,
        int EpisodesCount,
        int MaxNumber
    );
    
    public sealed record BuildPlanReq(
        int MovieId,
        int Start,
        int Count,
        string? TitlePattern,
        DateTime? ReleaseStart,
        int IntervalDays
    );
    
    public sealed record CreateReq(
        int MovieId,
        IEnumerable<PlanRow> Plan,
        bool Overwrite,
        string? Synopsis,
        int? DurationMinutes,
        PublishStatus Status,
        QualityLevel Quality,
        bool IsVipOnly
    );
    
    public sealed record BulkCreateResult(int Created, int Updated, int Skipped);
    
    // Dashboard
    public sealed record TaxonomyCounts(int Genres, int Studios);
    
    public sealed record Kpis(
        int TotalMovies, int PublishedMovies, int DraftMovies,
        int TotalEpisodes, int EpisodesMissingVideo,
        int TotalImages, int TotalVideos, int TotalAssets,
        long ImageBytes, long VideoBytes, long TotalBytes,
        int OrphanAssets);
    
    public sealed record IssueItem(string Title, string Description, string? Link);
    
    public sealed record DashboardRes(
        SystemDto.Kpis Kpis,
        SystemDto.TaxonomyCounts Taxonomy,
        List<MovieRes> RecentMovies,
        List<MovieRes> TopViewedMovies,
        List<SystemDto.IssueItem> Issues,
        List<AssetsDto.ImageAssetRes> OrphanImages
    );

    public record AuditLog
    {
        public string? Id { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Action { get; set; }
        public string? ResourceType { get; set; } 
        public string? ResourceId { get; set; }
        public object? Details { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }
        public string? Route { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
    }
    
    public class LogEntry<T>
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("@timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("audit")]
        public T? Audit { get; set; }
    }
}