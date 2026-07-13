namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
