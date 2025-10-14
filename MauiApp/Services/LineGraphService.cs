using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Line graph service that uses GraphDataTransformer and LineGraphGenerator
/// to provide clean methods for generating graphs from MoodEntry data.
/// Supports Impact, Average, and Raw graph modes.
/// </summary>
public class LineGraphService(IGraphDataTransformer graphDataTransformer, ILineGraphGenerator lineGraphGenerator) : ILineGraphService
{
    /// <summary>
    /// Initializes a new instance with default implementations.
    /// </summary>
    public LineGraphService() : this(new GraphDataTransformer(), new LineGraphGenerator()) { }

    // Consolidated Graph Methods

    /// <summary>
    /// Generates a line graph PNG image with white background for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        return graphMode switch
        {
            GraphMode.Impact => await GenerateImpactGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height),
            GraphMode.Average => await GenerateAverageGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height),
            GraphMode.RawData => await GenerateRawGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height),
            _ => throw new ArgumentException($"Unsupported graph mode: {graphMode}")
        };
    }

    /// <summary>
    /// Generates a line graph PNG image with custom background for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        return graphMode switch
        {
            GraphMode.Impact => await GenerateImpactGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height),
            GraphMode.Average => await GenerateAverageGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height),
            GraphMode.RawData => await GenerateRawGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height),
            _ => throw new ArgumentException($"Unsupported graph mode: {graphMode}")
        };
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with white background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        switch (graphMode)
        {
            case GraphMode.Impact:
                await SaveImpactGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
                break;
            case GraphMode.Average:
                await SaveAverageGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
                break;
            case GraphMode.RawData:
                await SaveRawGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
                break;
            default:
                throw new ArgumentException($"Unsupported graph mode: {graphMode}");
        }
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        switch (graphMode)
        {
            case GraphMode.Impact:
                await SaveImpactGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
                break;
            case GraphMode.Average:
                await SaveAverageGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
                break;
            case GraphMode.RawData:
                await SaveRawGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
                break;
            default:
                throw new ArgumentException($"Unsupported graph mode: {graphMode}");
        }
    }

    // Impact Graph Methods

    /// <summary>
    /// Generates an Impact mode line graph PNG image with white background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    private async Task<byte[]> GenerateImpactGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
    }

    /// <summary>
    /// Generates an Impact mode line graph PNG image with custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task<byte[]> GenerateImpactGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves an Impact mode line graph PNG image to the specified file path with white background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveImpactGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves an Impact mode line graph PNG image to the specified file path with custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveImpactGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
    }

    // Average Graph Methods

    /// <summary>
    /// Generates an Average mode line graph PNG image with white background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    private async Task<byte[]> GenerateAverageGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
    }

    /// <summary>
    /// Generates an Average mode line graph PNG image with custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task<byte[]> GenerateAverageGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves an Average mode line graph PNG image to the specified file path with white background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveAverageGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves an Average mode line graph PNG image to the specified file path with custom background
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveAverageGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
    }

    // Raw Graph Methods

    /// <summary>
    /// Generates a Raw mode line graph PNG image with white background.
    /// Shows start and end of work mood values from MoodEntry data.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The date range for the graph</param>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns>PNG image data as byte array</returns>
    private async Task<byte[]> GenerateRawGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.RawData, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
    }

    /// <summary>
    /// Generates a Raw mode line graph PNG image with custom background.
    /// Shows start and end of work mood values from MoodEntry data.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task<byte[]> GenerateRawGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.RawData, dateRange);
        return await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves a Raw mode line graph PNG image to the specified file path with white background.
    /// Shows start and end of work mood values from MoodEntry data.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveRawGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.RawData, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves a Raw mode line graph PNG image to the specified file path with custom background.
    /// Shows start and end of work mood values from MoodEntry data.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
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
    private async Task SaveRawGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, GraphMode.RawData, dateRange);
        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
    }
}
