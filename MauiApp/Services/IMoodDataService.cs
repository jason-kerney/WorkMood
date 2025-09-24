using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for mood data operations following the Dependency Inversion Principle
/// </summary>
public interface IMoodDataService
{
    /// <summary>
    /// Loads the complete mood data collection
    /// </summary>
    /// <returns>The mood collection</returns>
    Task<MoodCollection> LoadMoodDataAsync();

    /// <summary>
    /// Saves the complete mood data collection
    /// </summary>
    /// <param name="collection">The collection to save</param>
    Task SaveMoodDataAsync(MoodCollection collection);

    /// <summary>
    /// Saves a single mood entry
    /// </summary>
    /// <param name="entry">The entry to save</param>
    Task SaveMoodEntryAsync(MoodEntry entry);

    /// <summary>
    /// Saves a single mood entry with auto-save configuration
    /// </summary>
    /// <param name="entry">The entry to save</param>
    /// <param name="useAutoSaveDefaults">Whether to use auto-save defaults</param>
    Task SaveMoodEntryAsync(MoodEntry entry, bool useAutoSaveDefaults);

    /// <summary>
    /// Gets a mood entry for a specific date
    /// </summary>
    /// <param name="date">The date to search for</param>
    /// <returns>The mood entry if found, null otherwise</returns>
    Task<MoodEntry?> GetMoodEntryAsync(DateOnly date);

    /// <summary>
    /// Gets recent mood entries
    /// </summary>
    /// <param name="count">Number of entries to retrieve</param>
    /// <returns>Collection of recent mood entries</returns>
    Task<IEnumerable<MoodEntry>> GetRecentMoodEntriesAsync(int count = 7);

    /// <summary>
    /// Gets mood statistics
    /// </summary>
    /// <returns>Mood statistics</returns>
    Task<MoodStatistics> GetMoodStatisticsAsync();

    /// <summary>
    /// Gets two-week visualization data
    /// </summary>
    /// <returns>Visualization data</returns>
    Task<MoodVisualizationData> GetTwoWeekVisualizationAsync();

    /// <summary>
    /// Clears the internal cache
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Gets the data file path
    /// </summary>
    /// <returns>The data file path</returns>
    string GetDataFilePath();
}