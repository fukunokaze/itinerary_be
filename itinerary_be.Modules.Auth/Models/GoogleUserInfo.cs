namespace itinerary_be.Modules.Auth.Models;

public record GoogleUserInfo(string Email, bool EmailVerified, string Name);
