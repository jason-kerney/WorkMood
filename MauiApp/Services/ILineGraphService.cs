using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for line graph service that generates graphs from MoodEntry data
/// using GraphDataTransformer and LineGraphGenerator components.
/// Provides methods for generating graphs in different modes with optional background images.
/// </summary>
public interface ILineGraphService
{
    // Consolidated Graph Methods

    /// <summary>
    /// Generates a line graph PNG image with white background for the specified graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="graphMode">The mode of the graph (Impact, Average, or RawData)</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Generates a line graph PNG image with custom background for the specified graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="graphMode">The mode of the graph (Impact, Average, or RawData)</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with white background for the specified graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="graphMode">The mode of the graph (Impact, Average, or RawData)</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background for the specified graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="graphMode">The mode of the graph (Impact, Average, or RawData)</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);
}