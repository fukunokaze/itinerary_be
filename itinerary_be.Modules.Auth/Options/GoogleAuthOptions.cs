namespace itinerary_be.Modules.Auth.Options;

public class GoogleAuthOptions
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RedirectUri { get; set; }
}
