namespace itinerary_be.Modules.Itinerary;

using Microsoft.Extensions.DependencyInjection;
using itinerary_be.Modules.Itinerary.Interfaces;
using itinerary_be.Modules.Itinerary.Repositories;
using itinerary_be.Modules.Itinerary.Services;

/// <summary>
/// Extension methods for registering Trip services in the dependency injection container
/// </summary>
public static class ItineraryServiceRegistration
{
    /// <summary>
    /// Add Trip services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTripServices(this IServiceCollection services)
    {
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<ITripService, TripService>();
        
        services.AddScoped<ITripEventRepository, TripEventRepository>();
        services.AddScoped<ITripEventService, TripEventService>();

        services.AddScoped<ILodgingRepository, LodgingRepository>();
        services.AddScoped<ILodgingService, LodgingService>();

        return services;
    }
}
