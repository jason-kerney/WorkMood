using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Line graph service that uses GraphDataTransformer and LineGraphGenerator
/// to provide clean methods for generating graphs from MoodEntry data.
/// Supports any defined graph mode through shared transformation and rendering paths.
/// </summary>
public class LineGraphService : ILineGraphService
{
    private readonly IGraphDataTransformer _graphDataTransformer;
    private readonly ILineGraphGenerator _lineGraphGenerator;

    public LineGraphService(
        IGraphDataTransformer graphDataTransformer,
        ILineGraphGenerator lineGraphGenerator)
    {
        _graphDataTransformer = graphDataTransformer ?? throw new ArgumentNullException(nameof(graphDataTransformer));
        _lineGraphGenerator = lineGraphGenerator ?? throw new ArgumentNullException(nameof(lineGraphGenerator));
    }

    /// <summary>
    /// Initializes a new instance with default implementations.
    /// </summary>
    public LineGraphService() : this(new GraphDataTransformer(), new LineGraphGenerator()) { }

    // Consolidated Graph Methods

    /// <summary>
    /// Generates a line graph PNG image with white background for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        return await GenerateGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath: null, lineColor, backgroundColor: null, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    /// <summary>
    /// Generates a line graph PNG image with a solid background color for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, Color backgroundColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        return await GenerateGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath: null, lineColor, backgroundColor, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    /// <summary>
    /// Generates a line graph PNG image with custom background for the specified graph mode
    /// </summary>
    public async Task<byte[]> GenerateGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        return await GenerateGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, backgroundColor: null, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with white background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        await SaveGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath: null, lineColor, backgroundColor: null, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with a solid background color for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, Color backgroundColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        await SaveGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath: null, lineColor, backgroundColor, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background for the specified graph mode
    /// </summary>
    public async Task SaveGraphAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600, GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode = null)
    {
        await SaveGraphInternalAsync(moodEntries, graphMode, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, backgroundColor: null, width, height, gapDisplayMode, gapSegmentSecondaryPenMode);
    }

    private async Task<byte[]> GenerateGraphInternalAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string? backgroundImagePath, Color lineColor, Color? backgroundColor, int width, int height, GapDisplayMode gapDisplayMode, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode)
    {
        ValidateGraphMode(graphMode);

        var graphData = _graphDataTransformer.TransformMoodEntries(moodEntries, graphMode, dateRange, gapDisplayMode);
        graphData.GapSegmentSecondaryPenMode = gapDisplayMode == GapDisplayMode.ShowGaps ? null : gapSegmentSecondaryPenMode;

        if (!string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            return await _lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
        }

        return backgroundColor is not null
            ? await _lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, backgroundColor, width, height)
            : await _lineGraphGenerator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
    }

    private async Task SaveGraphInternalAsync(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string? backgroundImagePath, Color lineColor, Color? backgroundColor, int width, int height, GapDisplayMode gapDisplayMode, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode)
    {
        ValidateGraphMode(graphMode);

        var graphData = _graphDataTransformer.TransformMoodEntries(moodEntries, graphMode, dateRange, gapDisplayMode);
        graphData.GapSegmentSecondaryPenMode = gapDisplayMode == GapDisplayMode.ShowGaps ? null : gapSegmentSecondaryPenMode;

        if (!string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            await _lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, backgroundImagePath, lineColor, width, height);
            return;
        }

        if (backgroundColor is not null)
        {
            await _lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, backgroundColor, width, height);
            return;
        }

        await _lineGraphGenerator.SaveLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, filePath, lineColor, width, height);
    }

    private static void ValidateGraphMode(GraphMode graphMode)
    {
        if (!Enum.IsDefined(graphMode))
        {
            throw new ArgumentException($"Unsupported graph mode: {graphMode}");
        }
    }
}
