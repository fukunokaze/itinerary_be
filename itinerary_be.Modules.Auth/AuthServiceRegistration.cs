namespace itinerary_be.Modules.Auth;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using itinerary_be.Modules.Auth.Interfaces;
using itinerary_be.Modules.Auth.Options;
using itinerary_be.Modules.Auth.Repositories;
using itinerary_be.Modules.Auth.Services;

public static class AuthServiceRegistration
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleAuthOptions>(configuration.GetSection("Google"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
