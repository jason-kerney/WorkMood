using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for mood visualization services following the Interface Segregation Principle
/// </summary>
public interface IMoodVisualizationService
{
    /// <summary>
    /// Creates visualization data from mood data for a specified period
    /// </summary>
    /// <param name="moodCollection">The mood data collection</param>
    /// <returns>Visualization data that can be rendered</returns>
    MoodVisualizationData CreateTwoWeekValueVisualization(MoodCollection moodCollection);
}

/// <summary>
/// Interface for color calculation strategies
/// </summary>
public interface IMoodColorStrategy
{
    /// <summary>
    /// Gets the color representation for a mood value
    /// </summary>
    /// <param name="value">The mood value</param>
    /// <param name="maxAbsValue">The maximum absolute value for scaling</param>
    /// <returns>Color representation</returns>
    Color GetColorForValue(double value, double maxAbsValue);
}

/// <summary>
/// Interface for visualization data processing
/// </summary>
public interface IVisualizationDataProcessor
{
    /// <summary>
    /// Processes raw mood data into visualization-ready format
    /// </summary>
    /// <param name="entries">Raw mood entries</param>
    /// <param name="startDate">Start date for the visualization period</param>
    /// <param name="endDate">End date for the visualization period</param>
    /// <returns>Processed daily mood values</returns>
    DailyMoodValue[] ProcessMoodEntries(IEnumerable<MoodEntry> entries, DateOnly startDate, DateOnly endDate);
}