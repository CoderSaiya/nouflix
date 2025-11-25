using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
}

public interface IUserSubscriptionRepository : IRepository<UserSubscription>
{
    Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken ct = default);
}

public interface ITransactionRepository : IRepository<Transaction>
{
}
