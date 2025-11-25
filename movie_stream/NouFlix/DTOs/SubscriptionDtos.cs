using NouFlix.Models.Entities;

namespace NouFlix.DTOs;

public class SubscriptionDtos
{
    public record PlanDto(
        Guid Id,
        string Name,
        PlanType Type,
        decimal PriceMonthly,
        decimal PriceYearly,
        string? Description,
        List<string> Features
    );

    public record CreatePlanDto(
        string Name,
        PlanType Type,
        decimal PriceMonthly,
        decimal PriceYearly,
        string? Description,
        List<string> Features
    );

    public record UpdatePlanDto(
        string Name,
        PlanType Type,
        decimal PriceMonthly,
        decimal PriceYearly,
        string? Description,
        List<string> Features
    );

    public record SubscribeReq(Guid PlanId, string PaymentProvider, string ReturnUrl, string CancelUrl, string DurationType);
    public record SubscribeRes(string? PaymentUrl, bool IsActivated);
    public record ActivateSubscriptionReq(Guid TransactionId, string SessionId);

    public record SubscriptionRes(
        Guid Id,
        string PlanName,
        PlanType PlanType,
        DateTime StartDate,
        DateTime EndDate,
        SubscriptionStatus Status
    );
}
