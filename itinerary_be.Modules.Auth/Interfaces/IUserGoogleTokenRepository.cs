namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IUserGoogleTokenRepository
{
    Task<UserGoogleToken?> GetByUserIdAsync(Guid userId);
    Task UpsertAsync(UserGoogleToken token);
}
