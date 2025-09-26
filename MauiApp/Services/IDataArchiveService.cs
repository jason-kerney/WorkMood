using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for data archiving operations to manage data partitioning
/// </summary>
public interface IDataArchiveService
{
    /// <summary>
    /// Checks if archiving is needed and performs archiving if required.
    /// Archives data older than the specified threshold to a separate file.
    /// </summary>
    /// <param name="collection">The current mood collection</param>
    /// <param name="thresholdYears">Number of years after which data should be archived (default: 3)</param>
    /// <returns>A new collection with archived data removed, or the original collection if no archiving was needed</returns>
    Task<MoodCollection> ArchiveOldDataAsync(MoodCollection collection, int thresholdYears = 3);

    /// <summary>
    /// Determines if archiving is needed based on the oldest entry in the collection
    /// </summary>
    /// <param name="collection">The mood collection to check</param>
    /// <param name="thresholdYears">Number of years after which data should be archived (default: 3)</param>
    /// <returns>True if archiving is needed</returns>
    bool ShouldArchive(MoodCollection collection, int thresholdYears = 3);

    /// <summary>
    /// Gets the age in years of the oldest entry in the collection
    /// </summary>
    /// <param name="collection">The mood collection to analyze</param>
    /// <returns>Age in years of the oldest entry, or null if collection is empty</returns>
    double? GetOldestEntryAge(MoodCollection collection);

    /// <summary>
    /// Creates an archive file name based on the date range of the archived data
    /// </summary>
    /// <param name="oldestDate">The oldest date in the archived data</param>
    /// <param name="newestDate">The newest date in the archived data</param>
    /// <returns>Archive file name</returns>
    string CreateArchiveFileName(DateOnly oldestDate, DateOnly newestDate);

    /// <summary>
    /// Determines if the current date is within the specified number of days of a year transition
    /// (either end of current year or beginning of current year)
    /// </summary>
    /// <param name="daysFromTransition">Number of days from year boundary to consider (default: 14 for 2 weeks)</param>
    /// <returns>True if within the specified days of a year transition</returns>
    bool IsNearYearTransition(int daysFromTransition = 14);

    /// <summary>
    /// Gets archived mood entries from archive files that might contain relevant data
    /// for the current time period (e.g., when near year transitions)
    /// </summary>
    /// <param name="startDate">Start date for the desired data range</param>
    /// <param name="endDate">End date for the desired data range</param>
    /// <returns>Collection of archived mood entries within the date range</returns>
    Task<IEnumerable<MoodEntryOld>> GetArchivedEntriesInRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Gets a list of all available archive files in the archive directory
    /// </summary>
    /// <returns>List of archive file paths</returns>
    IEnumerable<string> GetArchiveFiles();

    /// <summary>
    /// Loads mood entries from a specific archive file
    /// </summary>
    /// <param name="archiveFilePath">Path to the archive file</param>
    /// <returns>Collection of mood entries from the archive file</returns>
    Task<IEnumerable<MoodEntryOld>> LoadFromArchiveFileAsync(string archiveFilePath);
}