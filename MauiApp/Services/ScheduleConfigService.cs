using System.Text.Json;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for persisting schedule configuration to local file storage using JSON
/// </summary>
public class ScheduleConfigService : IScheduleConfigService
{
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILoggingService _loggingService;
    private ScheduleConfig? _cachedConfig;

    /// <summary>
    /// Creates a new schedule configuration service
    /// </summary>
    public ScheduleConfigService(ILoggingService? loggingService = null)
    {
        _loggingService = loggingService ?? new LoggingService();
        
        Log("ScheduleConfigService: Constructor starting");
        
        // Store config in the app's local data directory
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "WorkMood");
        Directory.CreateDirectory(appFolder);
        
        Log($"ScheduleConfigService: Created app folder at {appFolder}");
        
        _configFilePath = Path.Combine(appFolder, "schedule_config.json");

        Log($"ScheduleConfigService: Config file path: {_configFilePath}");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        Log("ScheduleConfigService: Constructor completed");
    }

    private void Log(string message)
    {
        _loggingService.LogDebug(message);
    }

    /// <summary>
    /// Loads the schedule configuration from storage
    /// </summary>
    /// <returns>Schedule configuration</returns>
    public async Task<ScheduleConfig> LoadScheduleConfigAsync()
    {
        Log("LoadScheduleConfigAsync: Starting");
        
        if (_cachedConfig != null)
        {
            Log("LoadScheduleConfigAsync: Returning cached configuration");
            return _cachedConfig;
        }

        try
        {
            Log($"LoadScheduleConfigAsync: Checking if file exists: {_configFilePath}");
            
            if (!File.Exists(_configFilePath))
            {
                Log("LoadScheduleConfigAsync: File doesn't exist, creating default configuration");
                _cachedConfig = new ScheduleConfig();
                return _cachedConfig;
            }

            Log("LoadScheduleConfigAsync: Reading file");
            var json = await File.ReadAllTextAsync(_configFilePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Log("LoadScheduleConfigAsync: File is empty, creating default configuration");
                _cachedConfig = new ScheduleConfig();
                return _cachedConfig;
            }

            Log("LoadScheduleConfigAsync: Deserializing JSON");
            var config = JsonSerializer.Deserialize<ScheduleConfig>(json, _jsonOptions);
            _cachedConfig = config ?? new ScheduleConfig();
            
            Log($"LoadScheduleConfigAsync: Loaded - Morning: {_cachedConfig.MorningTime}, Evening: {_cachedConfig.EveningTime}");
            return _cachedConfig;
        }
        catch (Exception ex)
        {
            Log($"LoadScheduleConfigAsync: Error loading schedule config: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error loading schedule config: {ex.Message}");
            
            // Return default configuration if there's an error
            _cachedConfig = new ScheduleConfig();
            return _cachedConfig;
        }
    }

    /// <summary>
    /// Saves the schedule configuration to storage
    /// </summary>
    /// <param name="config">The schedule configuration to save</param>
    public async Task SaveScheduleConfigAsync(ScheduleConfig config)
    {
        try
        {
            Log($"SaveScheduleConfigAsync: Saving - Morning: {config.MorningTime}, Evening: {config.EveningTime}");
            
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
            
            _cachedConfig = config;
            Log("SaveScheduleConfigAsync: Successfully saved configuration");
        }
        catch (Exception ex)
        {
            Log($"SaveScheduleConfigAsync: Error saving schedule config: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error saving schedule config: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the cached configuration without loading from file
    /// </summary>
    /// <returns>Cached configuration or null if not loaded yet</returns>
    public ScheduleConfig? GetCachedConfig()
    {
        return _cachedConfig;
    }

    /// <summary>
    /// Clears the cached configuration, forcing next load to read from file
    /// </summary>
    public void ClearCache()
    {
        Log("ClearCache: Clearing cached configuration");
        _cachedConfig = null;
    }

    /// <summary>
    /// Updates the schedule configuration with new times and optional override, performing automatic cleanup
    /// </summary>
    /// <param name="morningTime">New morning time</param>
    /// <param name="eveningTime">New evening time</param>
    /// <param name="newOverride">Optional new override to add</param>
    /// <returns>The updated configuration</returns>
    public async Task<ScheduleConfig> UpdateScheduleConfigAsync(TimeSpan morningTime, TimeSpan eveningTime, ScheduleOverride? newOverride = null)
    {
        Log($"UpdateScheduleConfigAsync: Updating config - Morning: {morningTime}, Evening: {eveningTime}");
        
        // Load current configuration
        var currentConfig = await LoadScheduleConfigAsync();
        
        // Create updated configuration
        var updatedConfig = new ScheduleConfig(morningTime, eveningTime);
        
        // Copy existing overrides first
        updatedConfig.Overrides.AddRange(currentConfig.Overrides);
        
        // Add new override if provided
        if (newOverride != null && newOverride.HasOverride)
        {
            Log($"UpdateScheduleConfigAsync: Adding new override for {newOverride.Date}");
            updatedConfig.SetOverride(newOverride.Date, newOverride.MorningTime, newOverride.EveningTime);
        }
        
        // Remove all overrides for dates that have already passed
        var today = DateOnly.FromDateTime(DateTime.Today);
        var removedCount = updatedConfig.Overrides.RemoveAll(o => o.Date < today);
        if (removedCount > 0)
        {
            Log($"UpdateScheduleConfigAsync: Removed {removedCount} past overrides");
        }
        
        // Clean up very old overrides (30+ days) as well
        updatedConfig.CleanupOldOverrides();
        
        // Save the configuration
        await SaveScheduleConfigAsync(updatedConfig);
        
        Log("UpdateScheduleConfigAsync: Configuration updated successfully");
        return updatedConfig;
    }
}