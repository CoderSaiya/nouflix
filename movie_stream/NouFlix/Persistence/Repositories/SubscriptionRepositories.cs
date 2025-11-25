using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class SubscriptionPlanRepository(AppDbContext db) : Repository<SubscriptionPlan>(db), ISubscriptionPlanRepository
{
}

public class UserSubscriptionRepository(AppDbContext db) : Repository<UserSubscription>(db), IUserSubscriptionRepository
{
    public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await db.UserSubscriptions
            .Where(x => x.UserId == userId && x.Status == SubscriptionStatus.Active && x.EndDate > now)
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefaultAsync(ct);
    }
}

public class TransactionRepository(AppDbContext db) : Repository<Transaction>(db), ITransactionRepository
{
}
