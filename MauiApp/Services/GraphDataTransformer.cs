using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of data transformer that converts mood data into unified graph data points
/// </summary>
public class GraphDataTransformer : IGraphDataTransformer
{
    /// <summary>
    /// Transforms mood entries into graph data points based on the specified graph mode
    /// Results are sorted by timestamp for proper graph rendering
    /// </summary>
    /// <param name="moodEntries">The mood entries to transform</param>
    /// <param name="graphMode">The mode determining how to extract values (Impact or Average)</param>
    /// <returns>Collection of graph data points sorted by timestamp</returns>
    public IEnumerable<GraphDataPoint> TransformMoodEntries(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode)
    {
        var filteredEntries = FilterEntriesForGraphMode(moodEntries, graphMode);
        
        return filteredEntries
            .Select(entry => new GraphDataPoint(
                GetValueForMode(entry, graphMode), 
                entry.Date.ToDateTime(TimeOnly.MinValue)
            ))
            .OrderBy(point => point.Timestamp);
    }

    /// <summary>
    /// Transforms raw mood data points into graph data points
    /// Results are sorted by timestamp for proper graph rendering
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to transform</param>
    /// <returns>Collection of graph data points sorted by timestamp</returns>
    public IEnumerable<GraphDataPoint> TransformRawDataPoints(IEnumerable<RawMoodDataPoint> rawDataPoints)
    {
        return rawDataPoints
            .Select(point => new GraphDataPoint(point.MoodValue, point.Timestamp))
            .OrderBy(point => point.Timestamp);
    }

    /// <summary>
    /// Transforms mood entries into raw graph data points (equivalent to RawMoodDataPoint conversion)
    /// Each MoodEntry produces two GraphDataPoints: one for StartOfWork and one for EndOfWork
    /// Results are sorted by timestamp for proper graph rendering
    /// </summary>
    /// <param name="moodEntries">The mood entries to transform</param>
    /// <returns>Collection of graph data points representing raw mood data, sorted by timestamp</returns>
    public IEnumerable<GraphDataPoint> TransformRawDataPoints(IEnumerable<MoodEntry> moodEntries)
    {
        return moodEntries.SelectMany(entry =>
        {
            var startOfWork = entry.StartOfWork.GetValueOrDefault();
            return new[]
            {
                new GraphDataPoint(startOfWork, entry.CreatedAt),
                new GraphDataPoint(entry.EndOfWork.GetValueOrDefault(startOfWork), entry.LastModified)
            };
        }).OrderBy(point => point.Timestamp);
    }

    /// <summary>
    /// Filters mood entries based on the selected graph mode
    /// </summary>
    private static IEnumerable<MoodEntry> FilterEntriesForGraphMode(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode)
    {
        return graphMode switch
        {
            GraphMode.Impact => moodEntries.Where(e => e.Value.HasValue),
            GraphMode.Average => moodEntries.Where(e => e.GetAdjustedAverageMood().HasValue),
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, null)
        };
    }

    /// <summary>
    /// Gets the value for a mood entry based on the graph mode
    /// </summary>
    private static float GetValueForMode(MoodEntry entry, GraphMode graphMode)
    {
        return (float)(graphMode switch
        {
            GraphMode.Impact => entry.Value ?? 0,
            GraphMode.Average => entry.GetAdjustedAverageMood() ?? 0,
            _ => entry.Value ?? 0
        });
    }

    /// <summary>
    /// Gets the value for a single mood entry based on the graph mode (public method for LineGraphService)
    /// </summary>
    public float GetValueForMoodEntry(MoodEntry entry, GraphMode graphMode)
    {
        return GetValueForMode(entry, graphMode);
    }
}