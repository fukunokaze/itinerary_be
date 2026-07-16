namespace itinerary_be.Modules.Utility;

using Microsoft.Extensions.DependencyInjection;
using itinerary_be.Modules.Utility.Interfaces;
using itinerary_be.Modules.Utility.Services;

/// <summary>
/// Extension methods for registering Utility services in the dependency injection container
/// </summary>
public static class UtilityServiceRegistration
{
    /// <summary>
    /// Add Utility services (cross-cutting services that depend on both Auth and Itinerary modules) to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        services.AddScoped<ICalendarService, CalendarService>();

        return services;
    }
}
