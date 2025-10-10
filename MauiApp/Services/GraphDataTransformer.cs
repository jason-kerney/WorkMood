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

        // Special handling for RawData mode - each entry produces two data points
        if (graphMode == GraphMode.RawData)
        {
            return filteredEntries.SelectMany(entry =>
            {
                var startOfWork = entry.StartOfWork.GetValueOrDefault();
                return new[]
                {
                    new GraphDataPoint(startOfWork, entry.CreatedAt),
                    new GraphDataPoint(entry.EndOfWork.GetValueOrDefault(startOfWork), entry.LastModified)
                };
            }).OrderBy(point => point.Timestamp);
        }

        // Standard handling for Impact and Average modes - one data point per entry
        return filteredEntries
            .Select(entry => new GraphDataPoint(
                GetValueForMode(entry, graphMode),
                entry.Date.ToDateTime(TimeOnly.MinValue)
            ))
            .OrderBy(point => point.Timestamp);
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
            GraphMode.RawData => moodEntries.Where(e => e.StartOfWork.HasValue || e.EndOfWork.HasValue),
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
            GraphMode.RawData => entry.StartOfWork ?? 0, // For RawData, this method isn't used in the main transform but kept for compatibility
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