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
    private MoodCollection? _cachedCollection;

    /// <summary>
    /// Creates a new mood data service
    /// </summary>
    public MoodDataService()
    {
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

            var entries = JsonSerializer.Deserialize<List<MoodEntry>>(json, _jsonOptions);
            _cachedCollection = new MoodCollection(entries ?? new List<MoodEntry>());
            
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
            var json = JsonSerializer.Serialize(collection.Entries, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
            
            _cachedCollection = collection;
        }
        catch (Exception ex)
        {
            // Log error in production app
            System.Diagnostics.Debug.WriteLine($"Error saving mood data: {ex.Message}");
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