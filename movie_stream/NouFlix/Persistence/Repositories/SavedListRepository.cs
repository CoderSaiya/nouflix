using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class SavedListRepository(AppDbContext ctx) : Repository<SavedList>(ctx), ISavedListRepository
{
    
}