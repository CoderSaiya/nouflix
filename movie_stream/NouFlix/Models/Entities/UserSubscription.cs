using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public enum SubscriptionStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3
}

public class UserSubscription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    public Guid PlanId { get; set; }
    [ForeignKey(nameof(PlanId))]
    public SubscriptionPlan Plan { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public SubscriptionStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
