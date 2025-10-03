using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for generating line graphs from mood entry data
/// </summary>
public interface ILineGraphService
{
    /// <summary>
    /// Generates a line graph PNG image from mood entry data with custom line color
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, Color lineColor, int width = 800, int height = 600);
    

    
    /// <summary>
    /// Generates a line graph PNG image from mood entry data with custom background and line color
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);
    
    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom line color
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, Color lineColor, int width = 800, int height = 600);
    
    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, int width = 800, int height = 600);
    
    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background and line color
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);

    // New overloads with GraphMode support

    /// <summary>
    /// Generates a line graph PNG image from mood entry data with graph mode selection
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="graphMode">The graph mode (Impact or Average)</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, int width = 800, int height = 600);

    /// <summary>
    /// Generates a line graph PNG image from mood entry data with graph mode and custom line color
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="graphMode">The graph mode (Impact or Average)</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Generates a line graph PNG image from mood entry data with graph mode and custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="graphMode">The graph mode (Impact or Average)</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="graphMode">The graph mode (Impact or Average)</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, GraphMode graphMode, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with graph mode and custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="graphMode">The graph mode (Impact or Average)</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);

    // Raw Data overloads for handling individual mood recordings with timestamps

    /// <summary>
    /// Generates a scatter plot PNG image from raw mood data points
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="lineColor">Color for the data points</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Generates a scatter plot PNG image from raw mood data points with custom background
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the data points</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a scatter plot PNG image to the specified file path from raw mood data points
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="lineColor">Color for the data points</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, Color lineColor, int width = 800, int height = 600);

    /// <summary>
    /// Saves a scatter plot PNG image to the specified file path from raw mood data points with custom background
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the data points</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600);
}