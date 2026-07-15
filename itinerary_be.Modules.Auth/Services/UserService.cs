namespace itinerary_be.Modules.Auth.Services;

using Microsoft.Extensions.Logging;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Modules.Auth.Interfaces;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<User> GetOrCreateUserAsync(string email, string name)
    {
        var existing = await _repository.GetByEmailAsync(email);
        if (existing != null)
        {
            _logger.LogInformation("User with email {Email} logged in", email);
            return existing;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _repository.CreateAsync(user);
            _logger.LogInformation("User created with ID {UserId} for email {Email}", user.Id, email);
            return user;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Likely a concurrent create for the same email; return the row that won the race.
            var created = await _repository.GetByEmailAsync(email);
            if (created != null)
            {
                _logger.LogInformation("User with email {Email} logged in (created concurrently)", email);
                return created;
            }

            throw;
        }
    }
}
