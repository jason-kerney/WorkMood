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
}