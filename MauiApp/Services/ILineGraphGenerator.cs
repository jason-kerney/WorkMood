using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service responsible for rendering line graphs from unified data points
/// </summary>
public interface ILineGraphGenerator
{
    /// <summary>
    /// Generates a line graph PNG image from unified graph data points
    /// </summary>
    /// <param name="dataPoints">The data points to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="yAxisRange">The Y-axis range to use for the graph</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, AxisRange yAxisRange, int width = 800, int height = 600);

    /// <summary>
    /// Generates a line graph PNG image from unified graph data points with custom background
    /// </summary>
    /// <param name="dataPoints">The data points to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="yAxisRange">The Y-axis range to use for the graph</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    Task<byte[]> GenerateLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, AxisRange yAxisRange, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path
    /// </summary>
    /// <param name="dataPoints">The data points to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="yAxisRange">The Y-axis range to use for the graph</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, AxisRange yAxisRange, int width = 800, int height = 600);

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background
    /// </summary>
    /// <param name="dataPoints">The data points to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="yAxisRange">The Y-axis range to use for the graph</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>Task representing the async operation</returns>
    Task SaveLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, AxisRange yAxisRange, int width = 800, int height = 600);
}