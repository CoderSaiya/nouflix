using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class UserRepository(AppDbContext ctx) : Repository<User>(ctx), IUserRepository
{
    public async Task<IEnumerable<User>> SearchAsync(string keyword, int page = 1, int pageSize = 50)
    {
        var query = Db.Users
            .Include(u => u.Profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = $"%{keyword.Trim()}%";
            query = query.Where(u =>
                EF.Functions.Like(u.Email.Address, term) ||
                (u.Profile.Name != null && EF.Functions.Like(u.Profile.Name.FirstName, term)) ||
                (u.Profile.Name != null && EF.Functions.Like(u.Profile.Name.LastName, term))
            );
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<User?> GetByEmailAsync(string email) =>
        await Db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email.Address == email);

    public async Task<User?> GetByIdWithProfileAsync(Guid id) =>
        await Db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id.Equals(id)); 

    public async Task<bool> EmailExistsAsync(string email) =>
        await Db.Users
            .AnyAsync(u => u.Email.Address == email);
}