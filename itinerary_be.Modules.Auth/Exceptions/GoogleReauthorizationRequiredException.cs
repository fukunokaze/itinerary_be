namespace itinerary_be.Modules.Auth.Exceptions;

public class GoogleReauthorizationRequiredException : Exception
{
    public GoogleReauthorizationRequiredException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}
