using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Processors;

/// <summary>
/// Provides formatting and transformation utilities for mood visualization data
/// </summary>
public static class MoodVisualizationFormatter
{
    /// <summary>
    /// Gets 2-week mood visualization data from the mood data service
    /// </summary>
    /// <param name="moodDataService">The mood data service instance</param>
    /// <returns>Visualization data that can be used for displaying charts or graphics</returns>
    public static async Task<MoodVisualizationData> GetTwoWeekMoodVisualizationAsync(MoodDataService moodDataService)
    {
        // Get the visualization data for the last 2 weeks
        var visualizationData = await moodDataService.GetTwoWeekVisualizationAsync();
        
        // The returned data contains:
        // - DailyValues: Array of 14 daily mood values with colors
        // - StartDate/EndDate: Date range covered
        // - Width/Height: Suggested dimensions for display
        
        return visualizationData;
    }
    
    /// <summary>
    /// Generates a human-readable summary of mood visualization data
    /// </summary>
    /// <param name="visualizationData">The visualization data to summarize</param>
    /// <returns>Summary information about the visualization</returns>
    public static string GetVisualizationSummary(MoodVisualizationData visualizationData)
    {
        var daysWithData = visualizationData.DailyValues.Count(d => d.HasData);
        var totalDays = visualizationData.DailyValues.Length;
        
        // Handle case where there's no data
        var averageValue = visualizationData.DailyValues
            .Where(d => d.HasData)
            .Select(d => d.Value!.Value)
            .DefaultIfEmpty(0)
            .Average();
        
        if (daysWithData == 0)
        {
            return $"No mood data available for the period {visualizationData.StartDate:yyyy-MM-dd} to {visualizationData.EndDate:yyyy-MM-dd}. " +
                   "Start recording your daily moods to see trends here!";
        }
        
        return $"Showing {daysWithData} days of data out of {totalDays} days. " +
               $"Average mood change value: {averageValue:F2}. " +
               $"Date range: {visualizationData.StartDate:yyyy-MM-dd} to {visualizationData.EndDate:yyyy-MM-dd}";
    }
    
    /// <summary>
    /// Transforms raw mood visualization data into UI-friendly format for data binding
    /// </summary>
    /// <param name="moodDataService">The mood data service instance</param>
    /// <returns>List of mood day information objects suitable for UI binding</returns>
    public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(MoodDataService moodDataService)
    {
        var visualizationData = await moodDataService.GetTwoWeekVisualizationAsync();
        
        var moodDayInfos = new List<MoodDayInfo>();
        
        foreach (var dailyValue in visualizationData.DailyValues)
        {
            moodDayInfos.Add(new MoodDayInfo
            {
                Date = dailyValue.Date,
                Value = dailyValue.Value,
                HasData = dailyValue.HasData,
                ColorHex = ColorToHex(dailyValue.Color),
                DayOfWeek = dailyValue.Date.DayOfWeek.ToString(),
                ValueDescription = GetValueDescription(dailyValue.Value)
            });
        }
        
        return moodDayInfos;
    }
    
    /// <summary>
    /// Converts a Color to hex string format for UI binding
    /// </summary>
    /// <param name="color">The color to convert</param>
    /// <returns>Hex color string in format #RRGGBB</returns>
    private static string ColorToHex(Microsoft.Maui.Graphics.Color color)
    {
        var r = (int)(color.Red * 255);
        var g = (int)(color.Green * 255);
        var b = (int)(color.Blue * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
    
    /// <summary>
    /// Converts numeric mood change value to human-readable description.
    /// Change values represent the difference between start and end of work mood.
    /// Positive = improved, Zero = no change, Negative = declined
    /// </summary>
    /// <param name="value">The mood change value to describe</param>
    /// <returns>Human-readable description of the mood change value</returns>
    private static string GetValueDescription(double? value)
    {
        if (!value.HasValue)
            return "No data";
            
        return value.Value switch
        {
            >= 2 => "Significantly improved",
            >= 1 => "Moderately improved", 
            > 0 => "Slightly improved",
            0 => "No change",
            > -1 => "Slightly declined",
            >= -2 => "Moderately declined",
            _ => "Significantly declined"
        };
    }
}

/// <summary>
/// Data structure for UI binding of mood visualization information
/// </summary>
public class MoodDayInfo
{
    public DateOnly Date { get; set; }
    public double? Value { get; set; }
    public bool HasData { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string ValueDescription { get; set; } = string.Empty;
}