using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class History
{
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    public int MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
    
    public int? EpisodeId { get; set; }
    [ForeignKey("EpisodeId")]
    public Episode? Episode { get; set; }

    public int PositionSecond { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    [NotMapped] public bool IsFinished => Episode is null ?
        PositionSecond >= Movie.TotalDurationSeconds :
        PositionSecond >= Episode.TotalDurationSeconds;
    public DateTime WatchedDate { get; set; } = DateTime.UtcNow;
}