using System.Text.Json;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for archiving old mood data to separate files when data exceeds age threshold
/// Follows Single Responsibility Principle - only handles data archiving operations
/// </summary>
public class DataArchiveService : IDataArchiveService
{
    private readonly string _archiveDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IFolderShim folderShim;
    private readonly IDateShim dateShim;
    private readonly IFileShim fileShim;
    private readonly IJsonSerializerShim jsonSerializerShim;

    /// <summary>
    /// Creates a new data archive service
    /// </summary>
    public DataArchiveService(IFolderShim folderShim, IDateShim dateShim, IFileShim fileShim, IJsonSerializerShim jsonSerializerShim)
    {
        // Store archives in the same directory as the main data file
        _archiveDirectory = folderShim.GetArchiveFolder();

        // Ensure archive directory exists
        folderShim.CreateDirectory(_archiveDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new DateOnlyJsonConverter() }
        };
        this.folderShim = folderShim;
        this.dateShim = dateShim;
        this.fileShim = fileShim;
        this.jsonSerializerShim = jsonSerializerShim;
    }

    public DataArchiveService() : this(new FolderShim(), new DateShim(), new FileShim(), new JsonSerializerShim()) { }

    /// <summary>
    /// Checks if archiving is needed and performs archiving if required.
    /// Archives data older than the specified threshold to a separate file.
    /// </summary>
    /// <param name="collection">The current mood collection</param>
    /// <param name="thresholdYears">Number of years after which data should be archived (default: 3)</param>
    /// <returns>A new collection with archived data removed, or the original collection if no archiving was needed</returns>
    public async Task<MoodCollection> ArchiveOldDataAsync(MoodCollection collection, int thresholdYears = 3)
    {
        Log($"ArchiveOldDataAsync: Starting archive check with threshold of {thresholdYears} years");

        if (!ShouldArchive(collection, thresholdYears))
        {
            Log("ArchiveOldDataAsync: No archiving needed");
            return collection;
        }

        try
        {
            var cutoffDate = dateShim.GetDate(-thresholdYears);
            Log($"ArchiveOldDataAsync: Cutoff date is {cutoffDate}");

            // Separate entries into archive and current collections
            var entriesToArchive = collection.Entries.Where(e => e.Date < cutoffDate).ToList();
            var currentEntries = collection.Entries.Where(e => e.Date >= cutoffDate).ToList();

            Log($"ArchiveOldDataAsync: Found {entriesToArchive.Count} entries to archive, {currentEntries.Count} entries to keep");

            if (entriesToArchive.Any())
            {
                // Create archive file
                var oldestDate = entriesToArchive.Min(e => e.Date);
                var newestDate = entriesToArchive.Max(e => e.Date);
                var archiveFileName = CreateArchiveFileName(oldestDate, newestDate);
                var archiveFilePath = folderShim.CombinePaths(_archiveDirectory, archiveFileName);

                Log($"ArchiveOldDataAsync: Creating archive file {archiveFilePath}");

                // Save archived data
                var archiveJson = jsonSerializerShim.Serialize(entriesToArchive, _jsonOptions);
                await fileShim.WriteAllTextAsync(archiveFilePath, archiveJson);

                Log($"ArchiveOldDataAsync: Successfully archived {entriesToArchive.Count} entries to {archiveFileName}");

                // Return new collection with only current entries
                return new MoodCollection(currentEntries);
            }

            Log("ArchiveOldDataAsync: No entries found to archive");
            return collection;
        }
        catch (Exception ex)
        {
            Log($"ArchiveOldDataAsync: Error during archiving: {ex.Message}");
            // If archiving fails, return original collection to prevent data loss
            return collection;
        }
    }

    /// <summary>
    /// Determines if archiving is needed based on the oldest entry in the collection
    /// </summary>
    /// <param name="collection">The mood collection to check</param>
    /// <param name="thresholdYears">Number of years after which data should be archived (default: 3)</param>
    /// <returns>True if archiving is needed</returns>
    public bool ShouldArchive(MoodCollection collection, int thresholdYears = 3)
    {
        var oldestAge = GetOldestEntryAge(collection);
        var shouldArchive = oldestAge.HasValue && oldestAge.Value >= thresholdYears;

        Log($"ShouldArchive: Oldest entry age: {oldestAge?.ToString("F2") ?? "N/A"} years, threshold: {thresholdYears} years, should archive: {shouldArchive}");

        return shouldArchive;
    }

    /// <summary>
    /// Gets the age in years of the oldest entry in the collection
    /// </summary>
    /// <param name="collection">The mood collection to analyze</param>
    /// <returns>Age in years of the oldest entry, or null if collection is empty</returns>
    public double? GetOldestEntryAge(MoodCollection collection)
    {
        if (!collection.Entries.Any())
        {
            return null;
        }

        var oldestDate = collection.Entries.Min(e => e.Date);
        var today = dateShim.GetTodayDate();
        var ageInDays = today.DayNumber - oldestDate.DayNumber;
        var ageInYears = ageInDays / 365.25; // Account for leap years

        Log($"GetOldestEntryAge: Oldest date: {oldestDate}, today: {today}, age: {ageInYears:F2} years");

        return ageInYears;
    }

    /// <summary>
    /// Creates an archive file name based on the date range of the archived data
    /// </summary>
    /// <param name="oldestDate">The oldest date in the archived data</param>
    /// <param name="newestDate">The newest date in the archived data</param>
    /// <returns>Archive file name</returns>
    public string CreateArchiveFileName(DateOnly oldestDate, DateOnly newestDate)
    {
        var timestamp = dateShim.Now().ToString("yyyyMMdd_HHmmss");
        var fileName = $"mood_data_archive_{oldestDate:yyyy-MM-dd}_to_{newestDate:yyyy-MM-dd}_{timestamp}.json";

        Log($"CreateArchiveFileName: Generated archive file name: {fileName}");

        return fileName;
    }

    private void Log(string message)
    {
        try
        {
            var timestamp = dateShim.Now().ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [DataArchiveService] {message}";

            var desktopPath = folderShim.GetDesktopFolder();
            var logPath = folderShim.CombinePaths(desktopPath, "WorkMood_Debug.log");

            fileShim.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
        catch { } // Ignore logging errors
    }

    /// <summary>
    /// Determines if the current date is within the specified number of days of a year transition
    /// (either end of current year or beginning of current year)
    /// </summary>
    /// <param name="daysFromTransition">Number of days from year boundary to consider (default: 14 for 2 weeks)</param>
    /// <returns>True if within the specified days of a year transition</returns>
    public bool IsNearYearTransition(int daysFromTransition = 14)
    {
        var today = dateShim.GetToday();
        var startOfYear = new DateTime(today.Year, 1, 1);
        var endOfYear = new DateTime(today.Year, 12, 31);

        // Check if we're within the specified days of the beginning of the year
        var daysSinceStartOfYear = (today - startOfYear).Days;
        var nearStartOfYear = daysSinceStartOfYear <= daysFromTransition;

        // Check if we're within the specified days of the end of the year
        var daysUntilEndOfYear = (endOfYear - today).Days;
        var nearEndOfYear = daysUntilEndOfYear <= daysFromTransition;

        var result = nearStartOfYear || nearEndOfYear;
        Log($"IsNearYearTransition: Today={today:yyyy-MM-dd}, Days since start={daysSinceStartOfYear}, Days until end={daysUntilEndOfYear}, Near transition={result}");

        return result;
    }

    /// <summary>
    /// Gets archived mood entries from archive files that might contain relevant data
    /// for the current time period (e.g., when near year transitions)
    /// </summary>
    /// <param name="startDate">Start date for the desired data range</param>
    /// <param name="endDate">End date for the desired data range</param>
    /// <returns>Collection of archived mood entries within the date range</returns>
    public async Task<IEnumerable<MoodEntry>> GetArchivedEntriesInRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        Log($"GetArchivedEntriesInRangeAsync: Searching for entries between {startDate} and {endDate}");

        var allArchivedEntries = new List<MoodEntry>();

        try
        {
            var archiveFiles = GetArchiveFiles();

            foreach (var archiveFile in archiveFiles)
            {
                Log($"GetArchivedEntriesInRangeAsync: Checking archive file {folderShim.GetFileName(archiveFile)}");

                var entriesFromFile = await LoadFromArchiveFileAsync(archiveFile);
                var entriesInRange = entriesFromFile.Where(e => e.Date >= startDate && e.Date <= endDate);

                allArchivedEntries.AddRange(entriesInRange);

                Log($"GetArchivedEntriesInRangeAsync: Added {entriesInRange.Count()} entries from {folderShim.GetFileName(archiveFile)}");
            }

            // Sort by date for consistency
            allArchivedEntries.Sort((e1, e2) => e1.Date.CompareTo(e2.Date));

            Log($"GetArchivedEntriesInRangeAsync: Found {allArchivedEntries.Count} total archived entries in date range");

            return allArchivedEntries;
        }
        catch (Exception ex)
        {
            Log($"GetArchivedEntriesInRangeAsync: Error loading archived entries: {ex.Message}");
            return new List<MoodEntry>();
        }
    }

    /// <summary>
    /// Gets a list of all available archive files in the archive directory
    /// </summary>
    /// <returns>List of archive file paths</returns>
    public IEnumerable<string> GetArchiveFiles()
    {
        try
        {
            if (!folderShim.DirectoryExists(_archiveDirectory))
            {
                Log("GetArchiveFiles: Archive directory does not exist");
                return new List<string>();
            }

            var archiveFiles = folderShim.GetFiles(_archiveDirectory, "mood_data_archive_*.json");
            Log($"GetArchiveFiles: Found {archiveFiles.Length} archive files");

            return archiveFiles;
        }
        catch (Exception ex)
        {
            Log($"GetArchiveFiles: Error getting archive files: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Loads mood entries from a specific archive file
    /// </summary>
    /// <param name="archiveFilePath">Path to the archive file</param>
    /// <returns>Collection of mood entries from the archive file</returns>
    public async Task<IEnumerable<MoodEntry>> LoadFromArchiveFileAsync(string archiveFilePath)
    {
        try
        {
            if (!File.Exists(archiveFilePath))
            {
                Log($"LoadFromArchiveFileAsync: Archive file does not exist: {archiveFilePath}");
                return new List<MoodEntry>();
            }

            Log($"LoadFromArchiveFileAsync: Loading from {folderShim.GetFileName(archiveFilePath)}");

            var json = await File.ReadAllTextAsync(archiveFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Log($"LoadFromArchiveFileAsync: Archive file is empty: {folderShim.GetFileName(archiveFilePath)}");
                return new List<MoodEntry>();
            }

            var entries = jsonSerializerShim.Deserialize<List<MoodEntry>>(json, _jsonOptions);
            var result = entries ?? new List<MoodEntry>();

            Log($"LoadFromArchiveFileAsync: Loaded {result.Count} entries from {folderShim.GetFileName(archiveFilePath)}");

            return result;
        }
        catch (Exception ex)
        {
            Log($"LoadFromArchiveFileAsync: Error loading from {folderShim.GetFileName(archiveFilePath)}: {ex.Message}");
            return new List<MoodEntry>();
        }
    }
}