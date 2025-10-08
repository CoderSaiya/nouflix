using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class SavedList
{
    public DateTime AddedAt { get; set; }
    public string? Source { get; set; }
    public byte? Priority { get; set; }
    public bool NotifyOnAvailable { get; set; } = false;
    
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    public int MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
}