namespace RocketReps.Web.Analytics;

public interface IRocketRepsAnalytics
{
    void Capture(string? distinctId, string eventName, Dictionary<string, object?>? properties = null);

    Task IdentifyAsync(string? distinctId, Dictionary<string, object?> properties);
}
