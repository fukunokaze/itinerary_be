namespace itinerary_be.Modules.Auth.Interfaces;

using itinerary_be.Core.Domain.Entities;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(string email, string name);
}
