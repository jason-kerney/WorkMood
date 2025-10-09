using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service responsible for transforming various mood data types into a unified format for graph rendering
/// </summary>
public interface IGraphDataTransformer
{
    /// <summary>
    /// Transforms mood entries into graph data points based on the specified graph mode
    /// </summary>
    /// <param name="moodEntries">The mood entries to transform</param>
    /// <param name="graphMode">The mode determining how to extract values (Impact or Average)</param>
    /// <returns>Collection of graph data points</returns>
    IEnumerable<GraphDataPoint> TransformMoodEntries(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode);

    /// <summary>
    /// Transforms raw mood data points into graph data points
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to transform</param>
    /// <returns>Collection of graph data points</returns>
    IEnumerable<GraphDataPoint> TransformRawDataPoints(IEnumerable<RawMoodDataPoint> rawDataPoints);

    /// <summary>
    /// Gets the value for a single mood entry based on the graph mode
    /// </summary>
    /// <param name="entry">The mood entry</param>
    /// <param name="graphMode">The graph mode</param>
    /// <returns>The value for the entry in the specified mode</returns>
    float GetValueForMoodEntry(MoodEntry entry, GraphMode graphMode);
}