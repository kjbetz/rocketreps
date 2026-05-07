using PostHog;

namespace RocketReps.Web.Analytics;

public sealed class RocketRepsAnalytics(
    IServiceProvider serviceProvider,
    IHostEnvironment hostEnvironment,
    ILogger<RocketRepsAnalytics> logger) : IRocketRepsAnalytics
{
    public void Capture(string? distinctId, string eventName, Dictionary<string, object?>? properties = null)
    {
        if (string.IsNullOrWhiteSpace(distinctId) || string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        var client = serviceProvider.GetService<IPostHogClient>();
        if (client is null)
        {
            return;
        }

        try
        {
            client.Capture(distinctId, eventName, WithDefaults(properties));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "PostHog capture failed for event {EventName}.", eventName);
        }
    }

    public async Task IdentifyAsync(string? distinctId, Dictionary<string, object?> properties)
    {
        if (string.IsNullOrWhiteSpace(distinctId))
        {
            return;
        }

        var client = serviceProvider.GetService<IPostHogClient>();
        if (client is null)
        {
            return;
        }

        try
        {
            await client.IdentifyAsync(distinctId, WithDefaults(properties), null, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "PostHog identify failed for distinct ID {DistinctId}.", distinctId);
        }
    }

    private Dictionary<string, object> WithDefaults(Dictionary<string, object?>? properties)
    {
        var enriched = new Dictionary<string, object>(StringComparer.Ordinal);
        if (properties is not null)
        {
            foreach (var (key, value) in properties)
            {
                if (value is not null)
                {
                    enriched[key] = value;
                }
            }
        }

        enriched["app"] = "rocketreps";
        enriched["environment"] = hostEnvironment.EnvironmentName;
        return enriched;
    }
}
