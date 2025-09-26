using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for generating line graphs from mood entry data
/// </summary>
public interface ILineGraphService
{
    /// <summary>
    /// Generates a line graph PNG image from mood entry data
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, int width = 800, int height = 600);
    
    /// <summary>
    /// Saves a line graph PNG image to the specified file path
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, string filePath, int width = 800, int height = 600);
}