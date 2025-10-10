using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service responsible for transforming various mood data types into a unified format for graph rendering
/// </summary>
public interface IGraphDataTransformer
{
    /// <summary>
    /// Transforms mood entries into complete graph data including metadata based on the specified graph mode,
    /// filtered by the provided date range
    /// </summary>
    /// <param name="moodEntries">The mood entries to transform</param>
    /// <param name="graphMode">The mode determining how to extract values (Impact, Average, or RawData)</param>
    /// <param name="dateRangeInfo">The date range info to filter the mood entries by</param>
    /// <returns>Complete graph data including data points, title, axis information, and rendering metadata</returns>
    GraphData TransformMoodEntries(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRangeInfo);
}