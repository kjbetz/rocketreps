using FSRS.Core.Constants;

namespace RocketReps.Web.ReviewScheduling;

public static class FsrsDefaults
{
    public const int MaximumIntervalDays = 365;
    public const double DefaultDesiredRetention = 0.9;

    public static double[] DefaultParameters() => [.. FsrsConstants.DefaultParameters];

    public static TimeSpan[] DefaultLearningSteps() =>
        [TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10)];

    public static TimeSpan[] DefaultRelearningSteps() =>
        [TimeSpan.FromMinutes(5)];
}
