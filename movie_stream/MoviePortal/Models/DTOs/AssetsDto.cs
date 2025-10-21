namespace MoviePortal.Models.DTOs;

public class AssetsDto
{
    public class Image
    {
        public int Id { get; set; }
        public string Alt { get; set; }
        public string Endpoint { get; set; }
        public string Bucket { get; set; }
        public string ObjectKey { get; set; }
        public string Url { get; set; }
    }
    
    public sealed class SubtitleUploadRes
    {
        public long Id { get; set; }
        public int MovieId { get; set; }
        public int? EpisodeId { get; set; }
        public string Language { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Bucket { get; set; } = null!;
        public string ObjectKey { get; set; } = null!;
        public string? Endpoint { get; set; }
        public string PublicUrl { get; set; } = null!;
    }
}