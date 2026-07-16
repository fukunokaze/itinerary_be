namespace itinerary_be.Modules.Auth.Exceptions;

public class GoogleCalendarApiException : Exception
{
    public GoogleCalendarApiException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}
