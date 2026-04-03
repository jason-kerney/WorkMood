using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Line graph service that uses GraphDataTransformer and LineGraphGenerator
/// to provide clean methods for generating graphs from MoodEntry data.
/// Supports any defined graph mode through shared transformation and rendering paths.
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
        return await GenerateGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath: null, lineColor, width, height);
    }

    /// <summary>
    /// Generates a line graph PNG image with custom background for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        return await GenerateGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with white background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        await SaveGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath: null, lineColor, width, height);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        await SaveGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
    }

    private async Task<byte[]> GenerateGraphInternalAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string? backgroundImagePath, Color lineColor, int width, int height)
    {
        ValidateGraphMode(graphMode);

        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, graphMode, dateRange);

        return string.IsNullOrWhiteSpace(backgroundImagePath)
            ? await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height)
            : await lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
    }

    private async Task SaveGraphInternalAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string? backgroundImagePath, Color lineColor, int width, int height)
    {
        ValidateGraphMode(graphMode);

        var graphData = graphDataTransformer.TransformMoodEntries(moodEntries, graphMode, dateRange);

        if (string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
            return;
        }

        await lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
    }

    private static void ValidateGraphMode(GraphMode graphMode)
    {
        if (!Enum.IsDefined(graphMode))
        {
            throw new ArgumentException($"Unsupported graph mode: {graphMode}");
        }
    }
}
