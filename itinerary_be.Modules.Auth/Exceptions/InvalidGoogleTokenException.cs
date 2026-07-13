namespace itinerary_be.Modules.Auth.Exceptions;

public class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}
