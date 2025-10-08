using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class Review
{
    public int MovieId { get; set; }
    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;
    
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    [Range(1,10)]
    public int Number { get; set; }
    [MaxLength(256)]
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}