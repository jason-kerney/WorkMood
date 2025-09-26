using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Strategies;
using WorkMood.MauiApp.Processors;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for creating visualizations of mood data following SOLID principles
/// </summary>
public class MoodVisualizationService : IMoodVisualizationService
{
    private const int BITMAP_WIDTH = 280; // 14 days * 20 pixels per day
    private const int BITMAP_HEIGHT = 100;
    private const int DAYS_TO_SHOW = 14; // 2 weeks
    
    private readonly IVisualizationDataProcessor _dataProcessor;
    private readonly IMoodColorStrategy _colorStrategy;
    
    /// <summary>
    /// Initializes the service with dependency injection
    /// </summary>
    /// <param name="dataProcessor">Data processor for handling mood entries</param>
    /// <param name="colorStrategy">Strategy for calculating colors</param>
    public MoodVisualizationService(
        IVisualizationDataProcessor? dataProcessor = null,
        IMoodColorStrategy? colorStrategy = null)
    {
        _colorStrategy = colorStrategy ?? new DefaultMoodColorStrategy();
        _dataProcessor = dataProcessor ?? new VisualizationDataProcessor(_colorStrategy);
    }
    
    /// <summary>
    /// Creates visualization data from the last 2 weeks of mood data values.
    /// If there's less than 2 weeks of data, positions the available data proportionally 
    /// from the beginning while keeping the visualization the same size.
    /// Excludes today to prevent anchoring bias during mood entry.
    /// </summary>
    /// <param name="moodCollection">The mood data collection</param>
    /// <returns>Array of mood value data for visualization</returns>
    public MoodVisualizationData CreateTwoWeekValueVisualization(MoodCollection moodCollection)
    {
        // Calculate date range for last 2 weeks, excluding today to prevent anchoring bias
        var endDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1); // End yesterday
        var startDate = endDate.AddDays(-DAYS_TO_SHOW + 1);
        
        // Get entries in the date range
        var entriesInRange = moodCollection.GetEntriesInRange(startDate, endDate);
        
        // Process the data using the processor
        var dailyValues = _dataProcessor.ProcessMoodEntries(entriesInRange, startDate, endDate);
        
        // Calculate the maximum absolute value in the dataset for dynamic scaling
        var maxAbsValue = CalculateMaxAbsoluteValue(entriesInRange);
        
        return new MoodVisualizationData
        {
            DailyValues = dailyValues,
            StartDate = startDate,
            EndDate = endDate,
            Width = BITMAP_WIDTH,
            Height = BITMAP_HEIGHT,
            MaxAbsoluteValue = maxAbsValue
        };
    }
    
    /// <summary>
    /// Calculates the maximum absolute value for scaling purposes
    /// </summary>
    private static double CalculateMaxAbsoluteValue(IEnumerable<MoodEntry> entries)
    {
        return Math.Max(1.0, entries
            .Where(e => e.Value.HasValue)
            .Select(e => Math.Abs(e.Value!.Value))
            .DefaultIfEmpty(1.0)
            .Max());
    }
}

/// <summary>
/// Data structure for mood visualization
/// </summary>
public class MoodVisualizationData
{
    public DailyMoodValue[] DailyValues { get; set; } = Array.Empty<DailyMoodValue>();
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double MaxAbsoluteValue { get; set; } = 1.0;
}

/// <summary>
/// Represents a single day's mood value for visualization
/// </summary>
public class DailyMoodValue
{
    public DateOnly Date { get; set; }
    public double? Value { get; set; }
    public bool HasData { get; set; }
    public Color Color { get; set; } = Colors.Transparent;
}