namespace itinerary_be.Modules.Auth.Repositories;

using Microsoft.EntityFrameworkCore;
using itinerary_be.Core.Domain.Entities;
using itinerary_be.Infrastructure.Data;
using itinerary_be.Modules.Auth.Interfaces;

public class UserRepository : IUserRepository
{
    private readonly ItineraryDbContext _context;

    public UserRepository(ItineraryDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
