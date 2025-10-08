using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class Playlist
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    [NotMapped]
    public int Count => Items?.Count() ?? 0;
    [NotMapped] 
    public int TotalDuration => Items?.Sum(m => m.Movie.TotalDurationMinutes) ?? 0;
    
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}