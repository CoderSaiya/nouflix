namespace NouFlix.DTOs;

public class SubtitleDto
{
    public sealed class SubtitleJob
    {
        public string JobId { get; init; } = Guid.NewGuid().ToString("N");
        public int MovieId { get; init; }
        public int? EpisodeId { get; init; }
        public int? EpisodeNumber { get; init; }
        public string Language { get; init; } = "vi";
        public string Label { get; init; } = "Tiếng Việt";
        public int SegmentSeconds { get; init; } = 6;
        
        public string PresignedUrl { get; init; } = null!;
        public string DestBucket { get; init; } = null!;
        public string DestKey { get; init; } = null!;
    }

    public sealed class SubtitleStatus
    {
        public string JobId { get; set; } = null!;
        public string State { get; set; } = "Queued"; // Queued|Running|Done|Failed
        public int Progress { get; set; }
        public string? Error { get; set; }
        public string? IndexKey { get; set; } // key HLS subtitle đã tạo
    }

    public sealed record SubtitleUploadRes(
        long Id,
        int MovieId,
        int? EpisodeId,
        string Language,
        string Label,
        string Bucket,
        string ObjectKey,
        string? Endpoint,
        string PublicUrl);

    public sealed record UploadReq(string Lang, string Label, IFormFile File);
}