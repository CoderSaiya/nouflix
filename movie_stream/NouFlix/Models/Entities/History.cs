using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class History
{
    public Guid UserId { get; set; }
    [ForeignKey("MovieId")]
    public User User { get; set; } = null!;
    public int MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
    
    public int Duration { get; set; }
    public DateTime WatchedDate { get; set; }
}