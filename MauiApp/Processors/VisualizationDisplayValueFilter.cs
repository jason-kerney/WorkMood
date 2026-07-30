using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Processors;

/// <summary>
/// Applies data-level display filtering rules for two-week visualization values.
/// This filter decides which days are included in the display sequence.
/// </summary>
public static class VisualizationDisplayValueFilter
{
    public static DailyMoodValue[] BuildDisplayValues(IEnumerable<DailyMoodValue> dailyValues)
    {
        ArgumentNullException.ThrowIfNull(dailyValues);

        return dailyValues
            .Where(ShouldIncludeInDisplay)
            .ToArray();
    }

    private static bool ShouldIncludeInDisplay(DailyMoodValue dailyValue)
    {
        ArgumentNullException.ThrowIfNull(dailyValue);

        if (!CalendarGapPolicy.IsWeekend(dailyValue.Date.DayOfWeek))
        {
            return true;
        }

        return dailyValue.HasData && dailyValue.Value.HasValue;
    }
}
