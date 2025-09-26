using System.Text.Json;
using System.Text.Json.Serialization;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for persisting mood data to local file storage using JSON
/// </summary>
public class MoodDataService : IMoodDataService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IDataArchiveService _archiveService;
    private MoodCollection? _cachedCollection;

    /// <summary>
    /// Creates a new mood data service
    /// </summary>
    /// <param name="archiveService">Service for handling data archiving</param>
    public MoodDataService(IDataArchiveService archiveService)
    {
        _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
        
        Log("MoodDataService: Constructor starting");
        
        // Store data in the app's local data directory
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "WorkMood");
        Directory.CreateDirectory(appFolder);
        
        Log($"MoodDataService: Created app folder at {appFolder}");
        
        _dataFilePath = Path.Combine(appFolder, "mood_data.json");

        Log($"MoodDataService: Data file path: {_dataFilePath}");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new DateOnlyJsonConverter() }
        };
        
        Log("MoodDataService: Constructor completed");
    }

    /// <summary>
    /// Creates a new mood data service with default archive service (for backwards compatibility)
    /// </summary>
    public MoodDataService() : this(new DataArchiveService())
    {
    }

    private void Log(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var logPath = Path.Combine(desktopPath, "WorkMood_Debug.log");
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
        catch { } // Ignore logging errors
    }

    /// <summary>
    /// Loads all mood entries from storage
    /// </summary>
    /// <returns>Collection of mood entries</returns>
    public async Task<MoodCollection> LoadMoodDataAsync()
    {
        Log("LoadMoodDataAsync: Starting");
        
        if (_cachedCollection != null)
        {
            Log("LoadMoodDataAsync: Returning cached collection");
            return _cachedCollection;
        }

        try
        {
            Log($"LoadMoodDataAsync: Checking if file exists: {_dataFilePath}");
            
            if (!File.Exists(_dataFilePath))
            {
                Log("LoadMoodDataAsync: File doesn't exist, creating new collection");
                _cachedCollection = new MoodCollection();
                return _cachedCollection;
            }

            Log("LoadMoodDataAsync: Reading file");
            var json = await File.ReadAllTextAsync(_dataFilePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Log("LoadMoodDataAsync: File is empty, creating new collection");
                _cachedCollection = new MoodCollection();
                return _cachedCollection;
            }

            var entriesOld = JsonSerializer.Deserialize<List<MoodEntryOld>>(json, _jsonOptions);
            var entries = new List<MoodEntry>();
            
            // Copy each MoodEntryOld to MoodEntry
            if (entriesOld != null)
            {
                foreach (var oldEntry in entriesOld)
                {
                    var newEntry = MoodEntry.FromMoodEntryOld(oldEntry);
                    entries.Add(newEntry);
                }
            }
            _cachedCollection = new MoodCollection(entries);
            
            return _cachedCollection;
        }
        catch (Exception ex)
        {
            // Log error in production app
            System.Diagnostics.Debug.WriteLine($"Error loading mood data: {ex.Message}");
            
            // Return empty collection if there's an error
            _cachedCollection = new MoodCollection();
            return _cachedCollection;
        }
    }

    /// <summary>
    /// Saves all mood entries to storage
    /// </summary>
    /// <param name="collection">The mood collection to save</param>
    public async Task SaveMoodDataAsync(MoodCollection collection)
    {
        try
        {
            Log("SaveMoodDataAsync: Starting save operation");
            
            // Archive old data before saving if needed
            var archivedCollection = await _archiveService.ArchiveOldDataAsync(collection);
            
            // Save the current (non-archived) data
            var json = JsonSerializer.Serialize(archivedCollection.Entries, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
            
            // Update cached collection with the archived collection
            _cachedCollection = archivedCollection;
            
            Log($"SaveMoodDataAsync: Successfully saved {archivedCollection.Entries.Count} entries");
        }
        catch (Exception ex)
        {
            // Log error in production app
            System.Diagnostics.Debug.WriteLine($"Error saving mood data: {ex.Message}");
            Log($"SaveMoodDataAsync: Error saving mood data: {ex.Message}");
            throw new InvalidOperationException("Failed to save mood data", ex);
        }
    }

    /// <summary>
    /// Adds or updates a single mood entry
    /// </summary>
    /// <param name="entry">The mood entry to save</param>
    public async Task SaveMoodEntryAsync(MoodEntry entry)
    {
        var collection = await LoadMoodDataAsync();
        collection.AddOrUpdate(entry);
        await SaveMoodDataAsync(collection);
    }

    /// <summary>
    /// Adds or updates a single mood entry with auto-save options
    /// </summary>
    /// <param name="entry">The mood entry to save</param>
    /// <param name="useAutoSaveDefaults">If true, applies auto-save defaults like setting evening mood to morning mood</param>
    public async Task SaveMoodEntryAsync(MoodEntry entry, bool useAutoSaveDefaults)
    {
        var collection = await LoadMoodDataAsync();
        collection.AddOrUpdate(entry, useAutoSaveDefaults);
        await SaveMoodDataAsync(collection);
    }

    /// <summary>
    /// Gets a mood entry for a specific date
    /// </summary>
    /// <param name="date">The date to search for</param>
    /// <returns>The mood entry or null if not found</returns>
    public async Task<MoodEntry?> GetMoodEntryAsync(DateOnly date)
    {
        var collection = await LoadMoodDataAsync();
        return collection.GetEntry(date);
    }

    /// <summary>
    /// Gets recent mood entries
    /// </summary>
    /// <param name="count">Number of entries to return</param>
    /// <returns>Recent mood entries</returns>
    public async Task<IEnumerable<MoodEntry>> GetRecentMoodEntriesAsync(int count = 7)
    {
        var collection = await LoadMoodDataAsync();
        return collection.GetRecentEntries(count);
    }

    /// <summary>
    /// Gets recent mood entries, including archived data if needed when near year transitions
    /// and there isn't enough recent data in the active file
    /// </summary>
    /// <param name="count">Number of entries to return</param>
    /// <returns>Recent mood entries from active and archived data as needed</returns>
    public async Task<IEnumerable<MoodEntry>> GetRecentMoodEntriesWithArchiveAsync(int count = 7)
    {
        Log($"GetRecentMoodEntriesWithArchiveAsync: Requesting {count} recent entries");
        
        // First, get entries from the active file
        var activeCollection = await LoadMoodDataAsync();
        var activeEntries = activeCollection.GetRecentEntries(count).ToList();
        
        Log($"GetRecentMoodEntriesWithArchiveAsync: Found {activeEntries.Count} active entries");
        
        // If we have enough entries or we're not near a year transition, return active entries
        if (activeEntries.Count >= count || !_archiveService.IsNearYearTransition(14))
        {
            Log($"GetRecentMoodEntriesWithArchiveAsync: Sufficient active data or not near year transition, returning {activeEntries.Count} entries");
            return activeEntries;
        }
        
        // We need more data and we're near a year transition - check archives
        Log("GetRecentMoodEntriesWithArchiveAsync: Need more data and near year transition, checking archives");
        
        try
        {
            // Calculate how many more entries we need
            var needed = count - activeEntries.Count;
            Log($"GetRecentMoodEntriesWithArchiveAsync: Need {needed} more entries from archives");
            
            // Define search range - look back from the oldest active entry or current date
            var today = DateOnly.FromDateTime(DateTime.Today);
            var oldestActiveDate = activeEntries.Any() ? activeEntries.Min(e => e.Date) : today;
            
            // Search in a reasonable range around the year transition
            var searchStartDate = oldestActiveDate.AddDays(-60); // Look back 60 days from oldest active
            var searchEndDate = today.AddDays(14); // Look forward 14 days
            
            Log($"GetRecentMoodEntriesWithArchiveAsync: Searching archives from {searchStartDate} to {searchEndDate}");
            
            // Get archived entries in the search range
            var archivedEntries = await _archiveService.GetArchivedEntriesInRangeAsync(searchStartDate, searchEndDate);
            
            // Combine active and archived entries, remove duplicates, and sort by date (newest first)
            var allEntries = new List<MoodEntry>(activeEntries);
            
            foreach (var archived in archivedEntries)
            {
                // Only add if we don't already have an entry for this date
                if (!allEntries.Any(e => e.Date == archived.Date))
                {
                    allEntries.Add(archived);
                }
            }
            
            // Sort by date (newest first) and take the requested count
            var result = allEntries
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToList();
            
            Log($"GetRecentMoodEntriesWithArchiveAsync: Returning {result.Count} combined entries (active + archived)");
            
            return result;
        }
        catch (Exception ex)
        {
            Log($"GetRecentMoodEntriesWithArchiveAsync: Error accessing archives: {ex.Message}");
            // If there's an error with archives, just return the active entries
            return activeEntries;
        }
    }

    /// <summary>
    /// Gets mood statistics
    /// </summary>
    /// <returns>Basic mood statistics</returns>
    public async Task<MoodStatistics> GetMoodStatisticsAsync()
    {
        var collection = await LoadMoodDataAsync();
        
        var overallAverage = collection.GetOverallAverageMood();
        var last7DaysAverage = collection.GetAverageMoodForPeriod(
            DateOnly.FromDateTime(DateTime.Today.AddDays(-7)), 
            DateOnly.FromDateTime(DateTime.Today));
        var last30DaysAverage = collection.GetAverageMoodForPeriod(
            DateOnly.FromDateTime(DateTime.Today.AddDays(-30)), 
            DateOnly.FromDateTime(DateTime.Today));
        
        return new MoodStatistics
        {
            TotalEntries = collection.Count,
            OverallAverageMood = overallAverage,
            Last7DaysAverageMood = last7DaysAverage,
            Last30DaysAverageMood = last30DaysAverage,
            Trend = collection.GetMoodTrend(7)
        };
    }

    /// <summary>
    /// Gets mood visualization data for the last 2 weeks
    /// </summary>
    /// <returns>Visualization data for charting/display</returns>
    public async Task<MoodVisualizationData> GetTwoWeekVisualizationAsync()
    {
        var collection = await LoadMoodDataAsync();
        var visualizationService = new MoodVisualizationService();
        return visualizationService.CreateTwoWeekValueVisualization(collection);
    }

    /// <summary>
    /// Clears the cache and forces reload on next access
    /// </summary>
    public void ClearCache()
    {
        _cachedCollection = null;
    }

    /// <summary>
    /// Gets the path where mood data is stored
    /// </summary>
    public string GetDataFilePath() => _dataFilePath;
}

/// <summary>
/// Basic mood statistics
/// </summary>
public class MoodStatistics
{
    public int TotalEntries { get; set; }
    public double? OverallAverageMood { get; set; }
    public double? Last7DaysAverageMood { get; set; }
    public double? Last30DaysAverageMood { get; set; }
    public string Trend { get; set; } = string.Empty;
}

/// <summary>
/// JSON converter for DateOnly type
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, "yyyy-MM-dd");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}