namespace MoviePortal.Models.DTOs;

public class TranscodeDto
{
    public sealed class TranscodeStatus
    {
        public string JobId { get; set; } = default!;
        public string State { get; set; } = ""; // Queued|Running|Done|Failed
        public int Progress { get; set; }
        public string? Error { get; set; }
        public string? MasterKey { get; set; }
    }

    public sealed class TranscodeEnqueueResponse
    {
        public string JobId { get; set; } = default!;
    }
}