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
    public GraphData TransformMoodEntries(
        IEnumerable<MoodEntry> moodEntries,
        GraphMode graphMode,
        DateRangeInfo dateRangeInfo,
        GapDisplayMode gapDisplayMode = GapDisplayMode.ShowGaps)
    {
        var orderedEntries = moodEntries
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.CreatedAt)
            .ToList();

        var entriesInRange = orderedEntries
            .Where(entry => entry.Date >= dateRangeInfo.StartDate && entry.Date <= dateRangeInfo.EndDate)
            .ToList();

        var dataPoints = CreateDataPointsForMode(entriesInRange, graphMode);
        var fullHistoryDataPoints = CreateDataPointsForMode(orderedEntries, graphMode);

        var normalizedDataPoints = gapDisplayMode switch
        {
            GapDisplayMode.ShowGaps => dataPoints,
            GapDisplayMode.GapsAsMin => ApplyGapDisplayMode(dataPoints, dateRangeInfo, GetGapDisplayModeMinValue(graphMode)),
            GapDisplayMode.GapsAsMax => ApplyGapDisplayMode(dataPoints, dateRangeInfo, GetGapDisplayModeMaxValue(graphMode)),
            GapDisplayMode.GapsAsAverage => ApplyAverageGapDisplayMode(dataPoints, dateRangeInfo),
            GapDisplayMode.GapsAsSurroundingAverage => ApplySurroundingAverageGapDisplayMode(dataPoints, dateRangeInfo),
            GapDisplayMode.GapsAsMatchPreviousValue => ApplyDirectionalGapDisplayMode(dataPoints, fullHistoryDataPoints, dateRangeInfo, usePreviousValue: true),
            GapDisplayMode.GapsAsMatchFollowingValue => ApplyDirectionalGapDisplayMode(dataPoints, fullHistoryDataPoints, dateRangeInfo, usePreviousValue: false),
            _ => throw new ArgumentOutOfRangeException(nameof(gapDisplayMode), gapDisplayMode, "Unsupported gap display mode")
        };

        return CreateGraphData(normalizedDataPoints, graphMode, dateRangeInfo);
    }

    private static IEnumerable<FilledGraphDataPoint> ApplyGapDisplayMode(
        IEnumerable<FilledGraphDataPoint> dataPoints,
        DateRangeInfo dateRangeInfo,
        float gapFillValue)
    {
        var orderedPoints = dataPoints
            .OrderBy(point => point.Timestamp)
            .ToList();

        var pointsByDate = orderedPoints
            .GroupBy(point => DateOnly.FromDateTime(point.Timestamp))
            .ToDictionary(group => group.Key, group => group.OrderBy(point => point.Timestamp).ToList());

        var result = new List<FilledGraphDataPoint>();

        for (var date = dateRangeInfo.StartDate; date <= dateRangeInfo.EndDate; date = date.AddDays(1))
        {
            if (pointsByDate.TryGetValue(date, out var existingPoints))
            {
                result.AddRange(existingPoints);
                continue;
            }

            if (CalendarGapPolicy.IsWeekend(date.DayOfWeek))
            {
                continue;
            }

            result.Add(new FilledGraphDataPoint(date.ToDateTime(TimeOnly.MinValue), gapFillValue, IsSyntheticGapFill: true));
        }

        return result;
    }

    private static IEnumerable<FilledGraphDataPoint> ApplyAverageGapDisplayMode(
        IEnumerable<FilledGraphDataPoint> dataPoints,
        DateRangeInfo dateRangeInfo)
    {
        var visiblePoints = dataPoints
            .Where(point => !point.IsSyntheticGapFill && point.Value.HasValue)
            .ToList();

        if (visiblePoints.Count == 0)
        {
            return visiblePoints;
        }

        var averageFillValue = (float)visiblePoints.Average(point => point.Value!.Value);
        return ApplyGapDisplayMode(visiblePoints, dateRangeInfo, averageFillValue);
    }

    private static float GetGapDisplayModeMinValue(GraphMode graphMode)
    {
        return GetGapDisplayModeAxisRange(graphMode).Min;
    }

    private static float GetGapDisplayModeMaxValue(GraphMode graphMode)
    {
        return GetGapDisplayModeAxisRange(graphMode).Max;
    }

    private static AxisRange GetGapDisplayModeAxisRange(GraphMode graphMode)
    {
        return graphMode switch
        {
            GraphMode.Impact => AxisRange.Impact,
            GraphMode.GeneralImpact => AxisRange.Impact,
            GraphMode.Average => AxisRange.Average,
            GraphMode.StartOfDay => AxisRange.RawData,
            GraphMode.EndOfDay => AxisRange.RawData,
            GraphMode.RawData => AxisRange.RawData,
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, "Unsupported graph mode")
        };
    }

    private static IEnumerable<FilledGraphDataPoint> ApplySurroundingAverageGapDisplayMode(
        IEnumerable<FilledGraphDataPoint> dataPoints,
        DateRangeInfo dateRangeInfo)
    {
        var visiblePoints = dataPoints
            .Where(point => !point.IsSyntheticGapFill && point.Value.HasValue)
            .OrderBy(point => point.Timestamp)
            .ToList();

        var pointsByDate = visiblePoints
            .GroupBy(point => DateOnly.FromDateTime(point.Timestamp))
            .ToDictionary(group => group.Key, group => group.OrderBy(point => point.Timestamp).ToList());

        var result = new List<FilledGraphDataPoint>();

        for (var date = dateRangeInfo.StartDate; date <= dateRangeInfo.EndDate; date = date.AddDays(1))
        {
            if (pointsByDate.TryGetValue(date, out var existingPoints))
            {
                result.AddRange(existingPoints);
                continue;
            }

            if (CalendarGapPolicy.IsWeekend(date.DayOfWeek))
            {
                continue;
            }

            var previousPoint = visiblePoints.LastOrDefault(point => DateOnly.FromDateTime(point.Timestamp) < date);
            var nextPoint = visiblePoints.FirstOrDefault(point => DateOnly.FromDateTime(point.Timestamp) > date);

            if (previousPoint?.Value is not float previousValue || nextPoint?.Value is not float nextValue)
            {
                continue;
            }

            var averageValue = (previousValue + nextValue) / 2f;
            result.Add(new FilledGraphDataPoint(date.ToDateTime(TimeOnly.MinValue), averageValue, IsSyntheticGapFill: true));
        }

        return result;
    }

    private static IEnumerable<FilledGraphDataPoint> ApplyDirectionalGapDisplayMode(
        IEnumerable<FilledGraphDataPoint> dataPoints,
        IEnumerable<FilledGraphDataPoint> fullHistoryDataPoints,
        DateRangeInfo dateRangeInfo,
        bool usePreviousValue)
    {
        var visiblePoints = dataPoints
            .Where(point => !point.IsSyntheticGapFill && point.Value.HasValue)
            .OrderBy(point => point.Timestamp)
            .ToList();

        var fullHistoryVisiblePoints = fullHistoryDataPoints
            .Where(point => !point.IsSyntheticGapFill && point.Value.HasValue)
            .OrderBy(point => point.Timestamp)
            .ToList();

        var pointsByDate = visiblePoints
            .GroupBy(point => DateOnly.FromDateTime(point.Timestamp))
            .ToDictionary(group => group.Key, group => group.OrderBy(point => point.Timestamp).ToList());

        var result = new List<FilledGraphDataPoint>();

        for (var date = dateRangeInfo.StartDate; date <= dateRangeInfo.EndDate; date = date.AddDays(1))
        {
            if (pointsByDate.TryGetValue(date, out var existingPoints))
            {
                result.AddRange(existingPoints);
                continue;
            }

            if (CalendarGapPolicy.IsWeekend(date.DayOfWeek))
            {
                continue;
            }

            var sourcePoint = usePreviousValue
                ? fullHistoryVisiblePoints.LastOrDefault(point => DateOnly.FromDateTime(point.Timestamp) < date)
                : fullHistoryVisiblePoints.FirstOrDefault(point => DateOnly.FromDateTime(point.Timestamp) > date);

            if (sourcePoint?.Value is not float sourceValue)
            {
                continue;
            }

            result.Add(new FilledGraphDataPoint(date.ToDateTime(TimeOnly.MinValue), sourceValue, IsSyntheticGapFill: true));
        }

        return result;
    }

    private static IEnumerable<FilledGraphDataPoint> CreateDataPointsForMode(
        IEnumerable<MoodEntry> moodEntries,
        GraphMode graphMode)
    {
        return graphMode switch
        {
            GraphMode.Impact => CreateSinglePointPerEntryData(moodEntries, GraphMode.Impact),
            GraphMode.GeneralImpact => CreateGeneralImpactData(moodEntries),
            GraphMode.Average => CreateSinglePointPerEntryData(moodEntries, GraphMode.Average),
            GraphMode.StartOfDay => CreateSinglePointPerEntryData(moodEntries, GraphMode.StartOfDay),
            GraphMode.EndOfDay => CreateSinglePointPerEntryData(moodEntries, GraphMode.EndOfDay),
            GraphMode.RawData => CreateRawData(moodEntries),
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, "Unsupported graph mode")
        };
    }

    private static IEnumerable<FilledGraphDataPoint> CreateSinglePointPerEntryData(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode)
    {
        return moodEntries
            .Where(entry => HasValueForMode(entry, graphMode))
            .Select(entry => new FilledGraphDataPoint(
                entry.Date.ToDateTime(TimeOnly.MinValue),
                GetValueForMode(entry, graphMode)))
            .OrderBy(point => point.Timestamp);
    }

    private static IEnumerable<FilledGraphDataPoint> CreateGeneralImpactData(IEnumerable<MoodEntry> moodEntries)
    {
        var validEntries = moodEntries
            .Where(entry => entry.StartOfWork.HasValue)
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.CreatedAt)
            .ToList();

        for (int index = 1; index < validEntries.Count; index++)
        {
            var previousEntry = validEntries[index - 1];
            var currentEntry = validEntries[index];
            var previousMood = previousEntry.EndOfWork ?? previousEntry.StartOfWork;

            if (!previousMood.HasValue || !currentEntry.StartOfWork.HasValue)
            {
                continue;
            }

            yield return new FilledGraphDataPoint(
                currentEntry.Date.ToDateTime(TimeOnly.MinValue),
                currentEntry.StartOfWork.Value - previousMood.Value);
        }
    }

    private static IEnumerable<FilledGraphDataPoint> CreateRawData(IEnumerable<MoodEntry> moodEntries)
    {
        return moodEntries
            .Where(entry => entry.StartOfWork.HasValue || entry.EndOfWork.HasValue)
            .SelectMany(entry =>
            {
                var startOfWork = entry.StartOfWork.GetValueOrDefault();
                return new[]
                {
                    new FilledGraphDataPoint(entry.CreatedAt, startOfWork),
                    new FilledGraphDataPoint(entry.LastModified, entry.EndOfWork.GetValueOrDefault(startOfWork))
                };
            })
            .OrderBy(point => point.Timestamp);
    }

    private static bool HasValueForMode(MoodEntry entry, GraphMode graphMode)
    {
        return graphMode switch
        {
            GraphMode.Impact => entry.Value.HasValue,
            GraphMode.GeneralImpact => entry.StartOfWork.HasValue,
            GraphMode.Average => entry.GetAdjustedAverageMood().HasValue,
            GraphMode.StartOfDay => entry.StartOfWork.HasValue,
            GraphMode.EndOfDay => entry.EndOfWork.HasValue || entry.StartOfWork.HasValue,
            GraphMode.RawData => entry.StartOfWork.HasValue || entry.EndOfWork.HasValue,
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, null)
        };
    }

    private static float GetValueForMode(MoodEntry entry, GraphMode graphMode)
    {
        return (float)(graphMode switch
        {
            GraphMode.Impact => entry.Value ?? 0,
            GraphMode.GeneralImpact => 0,
            GraphMode.Average => entry.GetAdjustedAverageMood() ?? 0,
            GraphMode.StartOfDay => entry.StartOfWork ?? 0,
            GraphMode.EndOfDay => entry.EndOfWork ?? entry.StartOfWork ?? 0,
            GraphMode.RawData => entry.StartOfWork ?? 0,
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
    private static GraphData CreateGraphData(IEnumerable<FilledGraphDataPoint> dataPoints, GraphMode graphMode, DateRangeInfo? dateRangeInfo)
    {
        var baseTitle = graphMode switch
        {
            GraphMode.Impact => "Mood Change Over Time",
            GraphMode.GeneralImpact => "General Impact Over Time",
            GraphMode.Average => "Average Mood Over Time",
            GraphMode.StartOfDay => "Opening Mood Over Time",
            GraphMode.EndOfDay => "Closing Mood Over Time",
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
            GraphMode.GeneralImpact => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.Impact,
                CenterLineValue = 0,
                YAxisLabel = "Impact",
                XAxisLabel = "Date",
                IsRawData = false,
                YAxisLabelStep = 3,
                Description = "Shows how mood changes between recorded work periods by comparing the current start-of-work mood to the previous available end-of-work mood, or previous start-of-work mood when no end-of-work mood was recorded."
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
            GraphMode.StartOfDay => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.RawData,
                CenterLineValue = 5.5f,
                YAxisLabel = "Mood Level",
                XAxisLabel = "Date",
                IsRawData = false,
                YAxisLabelStep = 2,
                Description = "Shows one opening mood value per day using start-of-work mood."
            },
            GraphMode.EndOfDay => new GraphData
            {
                DataPoints = dataPoints,
                Title = title,
                YAxisRange = AxisRange.RawData,
                CenterLineValue = 5.5f,
                YAxisLabel = "Mood Level",
                XAxisLabel = "Date",
                IsRawData = false,
                YAxisLabelStep = 2,
                Description = "Shows one closing mood value per day using end-of-work mood, or start-of-work mood when no end-of-work mood is available."
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