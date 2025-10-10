using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Simplified line graph service interface that uses GraphData pipeline for rendering.
/// Combines related functionality and reduces parameter complexity while maintaining full feature set.
/// </summary>
public interface ISimpleLineGraphService
{
    /// <summary>
    /// Generates a line graph PNG image from mood entries with configurable display options.
    /// Uses GraphDataTransformer to convert mood data to GraphData, then LineGraphGenerator to render.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The date range for filtering and proportional positioning</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact, Average, or RawData)</param>
    /// <param name="options">Display configuration options (data points, axes, title, trend line, colors, dimensions)</param>
    /// <param name="backgroundImagePath">Optional path to custom background image (null/empty for white background)</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, GraphMode graphMode, GraphDisplayOptions options, string? backgroundImagePath = null);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path from mood entries.
    /// Uses GraphDataTransformer to convert mood data to GraphData, then LineGraphGenerator to render.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The date range for filtering and proportional positioning</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact, Average, or RawData)</param>
    /// <param name="options">Display configuration options (data points, axes, title, trend line, colors, dimensions)</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Optional path to custom background image (null/empty for white background)</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, GraphMode graphMode, GraphDisplayOptions options, string filePath, string? backgroundImagePath = null);

    /// <summary>
    /// Generates a scatter plot PNG image from raw mood data points with configurable display options.
    /// Uses GraphDataTransformer to convert raw data to GraphData, then LineGraphGenerator to render.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The date range for filtering and proportional positioning</param>
    /// <param name="options">Display configuration options (data points, axes, title, trend line, colors, dimensions)</param>
    /// <param name="backgroundImagePath">Optional path to custom background image (null/empty for white background)</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, GraphDisplayOptions options, string? backgroundImagePath = null);

    /// <summary>
    /// Saves a scatter plot PNG image to the specified file path from raw mood data points.
    /// Uses GraphDataTransformer to convert raw data to GraphData, then LineGraphGenerator to render.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The date range for filtering and proportional positioning</param>
    /// <param name="options">Display configuration options (data points, axes, title, trend line, colors, dimensions)</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Optional path to custom background image (null/empty for white background)</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, GraphDisplayOptions options, string filePath, string? backgroundImagePath = null);
}