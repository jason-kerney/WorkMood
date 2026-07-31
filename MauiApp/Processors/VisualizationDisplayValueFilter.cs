using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Processors;

/// <summary>
/// Applies data-level display filtering rules for two-week visualization values.
/// This filter decides which days are included in the display sequence by delegating
/// to <see cref="CalendarGapPolicy"/>, the single source of truth for the
/// weekend/gap-inclusion rule.
/// </summary>
/// <remarks>
/// Precondition: <c>dailyValues</c> must contain at most one entry per <see cref="DateOnly"/>.
/// Production callers (see VisualizationDataProcessor.ProcessMoodEntries) always satisfy this.
/// If violated, inclusion for a given date is decided by whether ANY entry sharing that date
/// is recorded (HasData &amp; Value.HasValue), not by each entry's own fields independently.
/// </remarks>
public static class VisualizationDisplayValueFilter
{
    public static DailyMoodValue[] BuildDisplayValues(IEnumerable<DailyMoodValue> dailyValues)
    {
        ArgumentNullException.ThrowIfNull(dailyValues);

        var values = dailyValues.ToList();

        var recordedDates = values
            .Select(ValidateNotNull)
            .Where(value => value.HasData && value.Value.HasValue)
            .Select(value => value.Date)
            .ToHashSet();

        return values
            .Where(value => CalendarGapPolicy.ShouldConsumeCompressedDaySlot(value.Date, recordedDates))
            .ToArray();
    }

    private static DailyMoodValue ValidateNotNull(DailyMoodValue dailyValue)
    {
        ArgumentNullException.ThrowIfNull(dailyValue);

        return dailyValue;
    }
}
