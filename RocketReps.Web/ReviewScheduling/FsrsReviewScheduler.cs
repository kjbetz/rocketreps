using FSRS.Core.Configurations;
using FSRS.Core.Enums;
using FSRS.Core.Services;
using Microsoft.Extensions.Options;
using RocketReps.Web.Data;

namespace RocketReps.Web.ReviewScheduling;

public sealed class FsrsReviewScheduler(IOptions<SchedulerOptions> baseOptions)
{
    private readonly SchedulerOptions _baseOptions =
        baseOptions.Value ?? throw new ArgumentNullException(nameof(baseOptions));

    public ReviewScheduleResult Review(
        StudentCardProgress progress,
        ReviewRating rating,
        DateTimeOffset reviewedAt)
    {
        var scheduler = new SchedulerFactory(CloneOptions()).CreateScheduler();
        var reviewTime = reviewedAt.UtcDateTime;

        var (updatedCard, _) = scheduler.ReviewCard(
            ToFsrsCard(progress, reviewTime),
            ToFsrsRating(rating),
            reviewTime,
            null);

        var nextDueAt = ToDateTimeOffset(updatedCard.Due);
        var scheduledInterval = nextDueAt - reviewedAt;
        if (scheduledInterval < TimeSpan.Zero)
        {
            scheduledInterval = TimeSpan.Zero;
        }

        return new ReviewScheduleResult(
            nextDueAt,
            scheduledInterval,
            updatedCard.Stability,
            updatedCard.Difficulty,
            updatedCard.State.ToString(),
            updatedCard.Step);
    }

    private FSRS.Core.Models.Card ToFsrsCard(StudentCardProgress progress, DateTime reviewTime)
    {
        var hasMemoryState = progress.LastReviewedAt is not null &&
            progress.Stability is > 0 &&
            progress.Difficulty is > 0;
        var state = Enum.TryParse<State>(progress.FsrsState, ignoreCase: true, out var parsedState)
            ? parsedState
            : State.Learning;

        return new FSRS.Core.Models.Card
        {
            CardId = progress.CardId,
            Due = hasMemoryState ? progress.DueAt.UtcDateTime : reviewTime,
            Stability = hasMemoryState ? progress.Stability : null,
            Difficulty = hasMemoryState ? progress.Difficulty : null,
            State = state,
            Step = hasMemoryState && state is State.Learning or State.Relearning ? progress.FsrsStep : null,
            LastReview = hasMemoryState ? progress.LastReviewedAt!.Value.UtcDateTime : null,
        };
    }

    private SchedulerOptions CloneOptions()
    {
        var parameters = _baseOptions.Parameters is { Length: > 0 }
            ? _baseOptions.Parameters
            : FsrsDefaults.DefaultParameters();
        var learningSteps = _baseOptions.LearningSteps is { Length: > 0 }
            ? _baseOptions.LearningSteps
            : FsrsDefaults.DefaultLearningSteps();
        var relearningSteps = _baseOptions.RelearningSteps is { Length: > 0 }
            ? _baseOptions.RelearningSteps
            : FsrsDefaults.DefaultRelearningSteps();

        return new SchedulerOptions
        {
            DesiredRetention = _baseOptions.DesiredRetention > 0
                ? _baseOptions.DesiredRetention
                : FsrsDefaults.DefaultDesiredRetention,
            Parameters = [.. parameters],
            LearningSteps = [.. learningSteps],
            RelearningSteps = [.. relearningSteps],
            MaximumInterval = _baseOptions.MaximumInterval > 0
                ? _baseOptions.MaximumInterval
                : FsrsDefaults.MaximumIntervalDays,
            EnableFuzzing = _baseOptions.EnableFuzzing,
        };
    }

    private static Rating ToFsrsRating(ReviewRating rating) =>
        rating switch
        {
            ReviewRating.Again => Rating.Again,
            ReviewRating.Good => Rating.Good,
            _ => Rating.Good,
        };

    private static DateTimeOffset ToDateTimeOffset(DateTime dateTime) =>
        dateTime.Kind switch
        {
            DateTimeKind.Local => new DateTimeOffset(dateTime).ToUniversalTime(),
            DateTimeKind.Utc => new DateTimeOffset(dateTime),
            _ => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
        };
}

public sealed record ReviewScheduleResult(
    DateTimeOffset NextDueAt,
    TimeSpan ScheduledInterval,
    double? Stability,
    double? Difficulty,
    string FsrsState,
    int? FsrsStep);
