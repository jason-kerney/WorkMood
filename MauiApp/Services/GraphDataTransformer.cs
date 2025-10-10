using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of data transformer that converts mood data into unified graph data points
/// </summary>
public class GraphDataTransformer : IGraphDataTransformer
{
    /// <summary>
    /// Transforms mood entries into complete graph data including metadata based on the specified graph mode,
    /// filtered by the provided date range
    /// Results include data points, title, axis information, and rendering metadata
    /// </summary>
    /// <param name="moodEntries">The mood entries to transform</param>
    /// <param name="graphMode">The mode determining how to extract values (Impact, Average, or RawData)</param>
    /// <param name="dateRangeInfo">The date range info to filter the mood entries by</param>
    /// <returns>Complete graph data including data points, title, axis information, and rendering metadata</returns>
    public GraphData TransformMoodEntries(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode, DateRangeInfo dateRangeInfo)
    {
        // Apply date range filtering based on the provided date range info
        var entriesInRange = moodEntries.Where(entry => entry.Date >= dateRangeInfo.StartDate && entry.Date <= dateRangeInfo.EndDate);

        var filteredEntries = FilterEntriesForGraphMode(entriesInRange, graphMode);

        IEnumerable<GraphDataPoint> dataPoints;

        // Special handling for RawData mode - each entry produces two data points
        if (graphMode == GraphMode.RawData)
        {
            dataPoints = filteredEntries.SelectMany(entry =>
            {
                var startOfWork = entry.StartOfWork.GetValueOrDefault();
                return new[]
                {
                    new GraphDataPoint(startOfWork, entry.CreatedAt),
                    new GraphDataPoint(entry.EndOfWork.GetValueOrDefault(startOfWork), entry.LastModified)
                };
            }).OrderBy(point => point.Timestamp);
        }
        else
        {
            // Standard handling for Impact and Average modes - one data point per entry
            dataPoints = filteredEntries
                .Select(entry => new GraphDataPoint(
                    GetValueForMode(entry, graphMode),
                    entry.Date.ToDateTime(TimeOnly.MinValue)
                ))
                .OrderBy(point => point.Timestamp);
        }

        return CreateGraphData(dataPoints, graphMode, dateRangeInfo);
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



    /// <summary>
    /// Creates a GraphData object with appropriate metadata based on the graph mode and date range info
    /// </summary>
    private static GraphData CreateGraphData(IEnumerable<GraphDataPoint> dataPoints, GraphMode graphMode, DateRangeInfo? dateRangeInfo)
    {
        var baseTitle = graphMode switch
        {
            GraphMode.Impact => "Mood Change Over Time",
            GraphMode.Average => "Average Mood Over Time",
            GraphMode.RawData => "Raw Mood Data Over Time",
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, "Unsupported graph mode")
        };

        var title = baseTitle;

        return graphMode switch
        {
            GraphMode.Impact => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.Impact,
                CenterLineValue = 0,
                YAxisLabel = "Impact",
                XAxisLabel = "Date",
                IsRawData = false,
                YAxisLabelStep = 3,
                Description = "Shows the daily impact on mood relative to neutral (0). Positive values indicate mood improvement, negative values indicate mood decline."
            },
            GraphMode.Average => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.Average,
                CenterLineValue = 0,
                YAxisLabel = "Average Mood",
                XAxisLabel = "Date",
                IsRawData = false,
                YAxisLabelStep = 3,
                Description = "Shows the average mood levels over time. Values represent the overall emotional state adjusted for context."
            },
            GraphMode.RawData => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.RawData,
                CenterLineValue = 5.5f,
                YAxisLabel = "Mood Level",
                XAxisLabel = "Time",
                IsRawData = true,
                YAxisLabelStep = 2,
                Description = "Shows raw mood values at the start and end of work periods. Scale ranges from 1 (lowest) to 10 (highest)."
            },
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, "Unsupported graph mode")
        };
    }
}