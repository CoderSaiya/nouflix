using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class PlaylistItem
{
    public int Position { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public bool IsPinned { get; set; }

    public int PlaylistId { get; set; }
    [ForeignKey("PlaylistId")]
    public Playlist Playlist { get; set; } = null!;
    public Guid AddedByUserId { get; set; }
    [ForeignKey("AddedByUserId")]
    public User AddedByUser { get; set; } = null!;
    public int MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
}