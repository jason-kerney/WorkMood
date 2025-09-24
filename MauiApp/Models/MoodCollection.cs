namespace WorkMood.MauiApp.Models;

/// <summary>
/// Collection of mood entries with helper methods for common operations
/// </summary>
public class MoodCollection
{
    private readonly List<MoodEntry> _entries;

    /// <summary>
    /// All mood entries in chronological order
    /// </summary>
    public IReadOnlyList<MoodEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// Creates a new empty mood collection
    /// </summary>
    public MoodCollection()
    {
        _entries = new List<MoodEntry>();
    }

    /// <summary>
    /// Creates a mood collection with existing entries
    /// </summary>
    /// <param name="entries">Existing mood entries</param>
    public MoodCollection(IEnumerable<MoodEntry> entries)
    {
        _entries = new List<MoodEntry>(entries);
        SortEntries();
    }

    /// <summary>
    /// Adds or updates a mood entry for a specific date
    /// </summary>
    /// <param name="entry">The mood entry to add or update</param>
    /// <param name="useAutoSaveDefaults">If true, applies auto-save defaults like setting evening mood to morning mood</param>
    public void AddOrUpdate(MoodEntry entry, bool useAutoSaveDefaults = false)
    {
        if (!entry.ShouldSave())
            return;

        entry.PrepareForSave(useAutoSaveDefaults);

        var existingEntry = _entries.FirstOrDefault(e => e.Date == entry.Date);
        if (existingEntry != null)
        {
            // Only update the mood values that are actually set in the new entry
            if (entry.MorningMood.HasValue)
                existingEntry.MorningMood = entry.MorningMood;
            if (entry.EveningMood.HasValue)
                existingEntry.EveningMood = entry.EveningMood;
            existingEntry.LastModified = DateTime.Now;
        }
        else
        {
            _entries.Add(entry);
            SortEntries();
        }
    }

    /// <summary>
    /// Gets a mood entry for a specific date
    /// </summary>
    /// <param name="date">The date to search for</param>
    /// <returns>The mood entry or null if not found</returns>
    public MoodEntry? GetEntry(DateOnly date)
    {
        return _entries.FirstOrDefault(e => e.Date == date);
    }

    /// <summary>
    /// Gets mood entries for a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Mood entries within the date range</returns>
    public IEnumerable<MoodEntry> GetEntriesInRange(DateOnly startDate, DateOnly endDate)
    {
        return _entries.Where(e => e.Date >= startDate && e.Date <= endDate);
    }

    /// <summary>
    /// Gets the most recent mood entries
    /// </summary>
    /// <param name="count">Number of entries to return</param>
    /// <returns>Most recent mood entries</returns>
    public IEnumerable<MoodEntry> GetRecentEntries(int count = 7)
    {
        return _entries.OrderByDescending(e => e.Date).Take(count);
    }

    /// <summary>
    /// Calculates average mood for all entries
    /// </summary>
    /// <returns>Average mood or null if no valid entries</returns>
    public double? GetOverallAverageMood()
    {
        var validEntries = _entries.Where(e => e.GetAverageMood().HasValue).ToList();
        if (!validEntries.Any())
            return null;

        return validEntries.Average(e => e.GetAverageMood()!.Value);
    }

    /// <summary>
    /// Calculates average mood for a specific time period
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Average mood for the period or null if no valid entries</returns>
    public double? GetAverageMoodForPeriod(DateOnly startDate, DateOnly endDate)
    {
        var entriesInRange = GetEntriesInRange(startDate, endDate)
            .Where(e => e.GetAverageMood().HasValue)
            .ToList();

        if (!entriesInRange.Any())
            return null;

        return entriesInRange.Average(e => e.GetAverageMood()!.Value);
    }

    /// <summary>
    /// Gets mood trend information (improving, declining, stable)
    /// </summary>
    /// <param name="days">Number of recent days to analyze</param>
    /// <returns>Trend description</returns>
    public string GetMoodTrend(int days = 7)
    {
        var recentEntries = GetRecentEntries(days)
            .Where(e => e.GetAverageMood().HasValue)
            .OrderBy(e => e.Date)
            .ToList();

        if (recentEntries.Count < 2)
            return "Insufficient data";

        var firstHalf = recentEntries.Take(recentEntries.Count / 2);
        var secondHalf = recentEntries.Skip(recentEntries.Count / 2);

        var firstAvg = firstHalf.Average(e => e.GetAverageMood()!.Value);
        var secondAvg = secondHalf.Average(e => e.GetAverageMood()!.Value);

        var difference = secondAvg - firstAvg;

        return difference switch
        {
            > 0.5 => "Improving",
            < -0.5 => "Declining", 
            _ => "Stable"
        };
    }

    /// <summary>
    /// Removes entries older than the specified date
    /// </summary>
    /// <param name="cutoffDate">Date before which entries will be removed</param>
    /// <returns>Number of entries removed</returns>
    public int RemoveEntriesOlderThan(DateOnly cutoffDate)
    {
        var toRemove = _entries.Where(e => e.Date < cutoffDate).ToList();
        foreach (var entry in toRemove)
        {
            _entries.Remove(entry);
        }
        return toRemove.Count;
    }

    /// <summary>
    /// Gets total number of entries
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Sorts entries by date (most recent first)
    /// </summary>
    private void SortEntries()
    {
        _entries.Sort((e1, e2) => e2.Date.CompareTo(e1.Date));
    }
}