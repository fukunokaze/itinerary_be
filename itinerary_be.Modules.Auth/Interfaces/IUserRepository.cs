namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
}
