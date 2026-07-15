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
        services.AddOptions<GoogleAuthOptions>()
            .Bind(configuration.GetSection("Google"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId), "Google:ClientId is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ClientSecret), "Google:ClientSecret is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.RedirectUri), "Google:RedirectUri is required.")
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Secret), "Jwt:Secret is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
            .ValidateOnStart();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpClient<IGoogleOAuthClient, GoogleOAuthClient>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
