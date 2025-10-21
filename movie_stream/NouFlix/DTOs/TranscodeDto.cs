namespace NouFlix.DTOs;

public class TranscodeDto
{
    public class TranscodeJob
    {
        public string JobId { get; init; } = Guid.NewGuid().ToString("N");
        public int MovieId { get; init; }
        public int? EpisodeId { get; init; }
        public int? EpisodeNumber { get; init; }
        public int? SeasonId { get; init; }
        public int? SeasonNumber { get; init; }
        public string SourceBucket { get; init; } = null!;
        public string SourceKey { get; init; } = null!;
        public string[] Profiles { get; init; } = [];
        public string Language { get; init; } = "vi";
    }
    
    public class TranscodeStatus
    {
        public string JobId { get; set; } = null!;
        public string State { get; set; } = "Queued"; // Queued|Running|Done|Failed
        public int Progress { get; set; }
        public string? Error { get; set; }
        public string? MasterKey { get; set; }
    }
}