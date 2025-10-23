using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NouFlix.Models.ValueObject;

namespace NouFlix.Models.Entities;

public class Profile
{
    [Key]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    public int? AvatarId { get; set; }
    [ForeignKey("AvatarId")]
    public ImageAsset? Avatar { get; set; }

    public string? AvatarUrl { get; set; }
    
    public Name? Name { get; set; }
    [Column(TypeName = "date")]
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}