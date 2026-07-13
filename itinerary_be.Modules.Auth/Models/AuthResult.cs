namespace itinerary_be.Modules.Auth.Models;

using itinerary_be.Core.Domain.Entities;

public record AuthResult(User User, string AccessToken, DateTime ExpiresAtUtc);
