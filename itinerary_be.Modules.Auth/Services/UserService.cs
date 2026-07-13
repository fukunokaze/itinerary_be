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
            _logger.LogInformation($"User with email {email} logged in");
            return existing;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(user);
        _logger.LogInformation($"User created with ID: {user.Id} for email {email}");

        return user;
    }
}
