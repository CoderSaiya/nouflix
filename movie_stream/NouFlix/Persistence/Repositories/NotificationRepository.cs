using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class NotificationRepository(AppDbContext ctx) : Repository<Notification>(ctx), INotificationRepository
{
    
}