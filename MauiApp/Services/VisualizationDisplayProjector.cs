namespace WorkMood.MauiApp.Services;

/// <summary>
/// Provides display-axis helpers for the 2-week visualization while preserving
/// the original 14-day calendar sequence for summaries and detail views.
/// </summary>
public static class VisualizationDisplayProjector
{
    public static IReadOnlyList<DailyMoodValue> GetDisplayValues(MoodVisualizationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.DisplayValues.Length > 0 ? data.DisplayValues : data.DailyValues;
    }

    public static bool ShouldBreakDisplaySegment(DateOnly previousDate, DateOnly currentDate)
    {
        if (currentDate <= previousDate)
        {
            return false;
        }

        for (var date = previousDate.AddDays(1); date < currentDate; date = date.AddDays(1))
        {
            if (!IsWeekend(date.DayOfWeek))
            {
                return true;
            }
        }

        return false;
    }
    private static bool IsWeekend(DayOfWeek dayOfWeek)
    {
        return dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}