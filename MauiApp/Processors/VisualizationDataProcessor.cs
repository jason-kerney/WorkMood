using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Processors;

/// <summary>
/// Processes mood data for visualization following Single Responsibility Principle
/// </summary>
public class VisualizationDataProcessor : IVisualizationDataProcessor
{
    private readonly IMoodColorStrategy _colorStrategy;
    
    public VisualizationDataProcessor(IMoodColorStrategy colorStrategy)
    {
        _colorStrategy = colorStrategy ?? throw new ArgumentNullException(nameof(colorStrategy));
    }

    public DailyMoodValue[] ProcessMoodEntries(IEnumerable<MoodEntryOld> entries, DateOnly startDate, DateOnly endDate)
    {
        var entriesList = entries.OrderBy(e => e.Date).ToList();
        var totalDays = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
        
        // Calculate the maximum absolute value for proper color scaling
        var maxAbsValue = CalculateMaxAbsoluteValue(entriesList);
        
        // Create array of daily values
        var dailyValues = new DailyMoodValue[totalDays];
        
        for (int dayIndex = 0; dayIndex < totalDays; dayIndex++)
        {
            var currentDate = startDate.AddDays(dayIndex);
            var entry = entriesList.FirstOrDefault(e => e.Date == currentDate);
            
            dailyValues[dayIndex] = CreateDailyMoodValue(currentDate, entry, maxAbsValue);
        }
        
        return dailyValues;
    }

    /// <summary>
    /// Calculates the maximum absolute value from mood entries for proper scaling
    /// </summary>
    private static double CalculateMaxAbsoluteValue(IEnumerable<MoodEntryOld> entries)
    {
        return Math.Max(1.0, entries
            .Where(e => e.Value.HasValue)
            .Select(e => Math.Abs(e.Value!.Value))
            .DefaultIfEmpty(1.0)
            .Max());
    }

    /// <summary>
    /// Creates a DailyMoodValue from a mood entry
    /// </summary>
    private DailyMoodValue CreateDailyMoodValue(DateOnly date, MoodEntryOld? entry, double maxAbsValue)
    {
        var hasData = entry?.Value.HasValue == true;
        var value = entry?.Value;
        
        var color = hasData && value.HasValue 
            ? _colorStrategy.GetColorForValue(value.Value, maxAbsValue)
            : Colors.LightGray;

        return new DailyMoodValue
        {
            Date = date,
            Value = value,
            HasData = hasData,
            Color = color
        };
    }
}