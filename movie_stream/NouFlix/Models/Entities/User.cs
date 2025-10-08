using System.ComponentModel.DataAnnotations;
using NouFlix.Models.ValueObject;

namespace NouFlix.Models.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Email Email { get; set; } = null!;
    [Required]
    [MaxLength(256)]
    public string Password { get; set; } = null!;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public Profile Profile { get; set; } = new();
    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<History> Histories { get; set; } = new List<History>();
    public ICollection<Notification> SenderNotification { get; set; } = new List<Notification>();
    public ICollection<Notification> RecipientNotification { get; set; } = new List<Notification>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<SavedList> SavedList { get; set; } = new List<SavedList>();
}