using System.ComponentModel.DataAnnotations;

namespace NouFlix.Models.Entities;

public enum PlanType
{
    Free = 0,
    VIP = 1,
    SVIP = 2
}

public class SubscriptionPlan
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public PlanType Type { get; set; }
    
    [Obsolete("Use PriceMonthly or PriceYearly instead")]
    public decimal Price { get; set; }
    
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    
    [Obsolete("Duration is now determined by the subscription type (Monthly/Yearly)")]
    public int DurationDays { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public List<string> Features { get; set; } = new();
}
