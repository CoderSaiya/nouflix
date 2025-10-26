using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;

namespace NouFlix.Persistence.Repositories.Interfaces;

public class HistoryRepository(AppDbContext ctx) : Repository<History>(ctx), IHistoryRepository
{
    
}