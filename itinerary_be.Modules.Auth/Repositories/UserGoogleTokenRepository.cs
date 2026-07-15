namespace itinerary_be.Modules.Auth.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Auth.Interfaces;

public class UserGoogleTokenRepository : IUserGoogleTokenRepository
{
    private readonly ItineraryDbContext _context;

    public UserGoogleTokenRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    public async Task<UserGoogleToken?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserGoogleTokens.FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task UpsertAsync(UserGoogleToken token)
    {
        var existing = await GetByUserIdAsync(token.UserId);
        if (existing == null)
        {
            _context.UserGoogleTokens.Add(token);
        }
        else
        {
            existing.AccessToken = token.AccessToken;
            existing.RefreshToken = token.RefreshToken;
            existing.ExpiresAt = token.ExpiresAt;
            existing.Scope = token.Scope;
            existing.UpdatedAt = token.UpdatedAt;
        }

        await _context.SaveChangesAsync();
    }
}
