using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Processors;

namespace WorkMood.MauiApp.Adapters;

/// <summary>
/// Adapter to bridge between interface-based services and formatting utilities
/// Following the Adapter pattern to maintain compatibility
/// </summary>
public static class VisualizationDataAdapter
{
    /// <summary>
    /// Adapts the interface-based service to work with existing example methods
    /// </summary>
    public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(IMoodDataService moodDataService)
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
    /// Gets a summary for visualization data using interface-based service
    /// </summary>
    public static async Task<string> GetVisualizationSummaryAsync(IMoodDataService moodDataService)
    {
        var visualizationData = await moodDataService.GetTwoWeekVisualizationAsync();
        return MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    }
    
    /// <summary>
    /// Helper method to convert Color to hex string for UI binding
    /// </summary>
    private static string ColorToHex(Microsoft.Maui.Graphics.Color color)
    {
        var r = (int)(color.Red * 255);
        var g = (int)(color.Green * 255);
        var b = (int)(color.Blue * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
    
    /// <summary>
    /// Helper method to get human-readable description of mood change value.
    /// Change values represent the difference between start and end of work mood.
    /// Positive = improved, Zero = no change, Negative = declined
    /// </summary>
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