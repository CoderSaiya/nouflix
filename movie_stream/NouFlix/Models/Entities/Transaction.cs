using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}

public class Transaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    public Guid? PlanId { get; set; }
    [ForeignKey(nameof(PlanId))]
    public SubscriptionPlan? Plan { get; set; }
    
    public decimal Amount { get; set; }
    
    public int DurationDays { get; set; }
    
    public TransactionStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? Note { get; set; }
}
