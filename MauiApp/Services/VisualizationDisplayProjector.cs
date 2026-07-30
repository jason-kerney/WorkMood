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
}