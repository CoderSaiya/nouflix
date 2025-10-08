using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext ctx) : Repository<RefreshToken>(ctx), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token) => 
        await Db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(t => t.Token == token);
}