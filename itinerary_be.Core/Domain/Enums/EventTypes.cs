namespace itinerary_be.Core.Domain.Enums;

public enum EventTypes
{
    flight,
    lodging,
    activity
}

public static class EventTypesExtensions
{
    public static string ToFriendlyString(this EventTypes eventType)
    {
        return eventType switch
        {
            EventTypes.flight => "flight",
            EventTypes.lodging => "lodging",
            EventTypes.activity => "actrivity",
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
        };
    }
}