using System.Diagnostics.CodeAnalysis;
using System.Timers;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service that monitors for date changes and automatically manages mood records
/// Follows Single Responsibility Principle - only handles timing and coordination
/// </summary>
public class MoodDispatcherService : IDisposable
{
    private readonly IDispatcherCommand[] _commands = null!;
    private readonly ScheduleConfigService _scheduleConfigService = null!;
    private readonly System.Timers.Timer _timer = null!;
    private DateOnly _lastCheckedDate;
    private bool _disposed = false;

    // Events to notify the UI about changes
    public event EventHandler<DateChangeEventArgs>? DateChanged;
    public event EventHandler<AutoSaveEventArgs>? AutoSaveOccurred;
    public event EventHandler<MorningReminderEventArgs>? MorningReminderOccurred;
    public event EventHandler<EveningReminderEventArgs>? EveningReminderOccurred;

    public MoodDispatcherService(ScheduleConfigService scheduleConfigService, [NotNull] params IDispatcherCommand[] commands)
    {
        _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        ArgumentOutOfRangeException.ThrowIfZero(_commands.Length, nameof(commands));
        
        _lastCheckedDate = DateOnly.FromDateTime(DateTime.Today);
        
        // Set up timer to check every 30 seconds
        _timer = new System.Timers.Timer(30000); // 30 seconds
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await CheckForDateChange();
        }
        catch (Exception ex)
        {
            // Log errors for debugging if needed
            Log($"MoodDispatcherService: Error during timer check: {ex.Message}");
        }
    }

    // Field to track current record state for better auto-save decisions
    private MoodEntryOld? _currentRecordState;

    /// <summary>
    /// Called by UI to provide current record state for more accurate auto-save decisions
    /// </summary>
    public void UpdateCurrentRecordState(MoodEntryOld? currentRecord)
    {
        _currentRecordState = currentRecord;
    }

    /// <summary>
    /// Checks if the date has changed and handles auto-save logic
    /// Also processes time-sensitive commands like morning reminders
    /// </summary>
    private async Task CheckForDateChange()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Today);
        
        // Always check for reminders on every tick
        await ProcessReminders(currentDate);
        
        if (currentDate != _lastCheckedDate)
        {
            await HandleDateChange(_lastCheckedDate, currentDate);
            _lastCheckedDate = currentDate;
        }
    }
    
    /// <summary>
    /// Processes morning and evening reminder commands on every timer tick
    /// </summary>
    private async Task ProcessReminders(DateOnly currentDate)
    {
        try
        {
            // Process MorningReminderCommand instances
            var morningReminderCommands = _commands.OfType<MorningReminderCommand>();
            foreach (var command in morningReminderCommands)
            {
                try
                {
                    var result = await command.ProcessTickAsync(currentDate, currentDate, _currentRecordState);
                    
                    // Check for morning reminder results
                    if (result.Success && result.Data is MorningReminderData reminderData)
                    {
                        MorningReminderOccurred?.Invoke(this, new MorningReminderEventArgs
                        {
                            MorningTime = reminderData.MorningTime,
                            TimeSinceMorning = reminderData.TimeSinceMorning,
                            CallCount = reminderData.CallCount,
                            Message = result.Message ?? "Time to record your morning mood!"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log($"MoodDispatcherService: Error executing morning reminder command: {ex.Message}");
                }
            }
            
            // Process EveningReminderCommand instances
            var eveningReminderCommands = _commands.OfType<EveningReminderCommand>();
            foreach (var command in eveningReminderCommands)
            {
                try
                {
                    var result = await command.ProcessTickAsync(currentDate, currentDate, _currentRecordState);
                    
                    // Check for evening reminder results
                    if (result.Success && result.Data is EveningReminderData reminderData)
                    {
                        EveningReminderOccurred?.Invoke(this, new EveningReminderEventArgs
                        {
                            EveningTime = reminderData.EveningTime,
                            TimeSinceEvening = reminderData.TimeSinceEvening,
                            CallCount = reminderData.CallCount,
                            ReminderType = reminderData.ReminderType,
                            MorningMissed = reminderData.MorningMissed,
                            EveningNeeded = reminderData.EveningNeeded,
                            Message = result.Message ?? "Time to record your evening mood!"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log($"MoodDispatcherService: Error executing evening reminder command: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Log($"MoodDispatcherService: Error processing reminders: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the date change by executing all dispatcher commands and notifying UI
    /// </summary>
    private async Task HandleDateChange(DateOnly oldDate, DateOnly newDate)
    {
        try
        {
            // Clean up schedule configuration (remove past overrides) at day transition
            await CleanupScheduleConfig();
            
            // Execute all dispatcher commands for the old date
            var results = new List<CommandResult>();
            
            foreach (var command in _commands)
            {
                try
                {
                    var result = await command.ProcessTickAsync(oldDate, newDate, _currentRecordState);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Log($"MoodDispatcherService: Error executing command {command.GetType().Name}: {ex.Message}");
                    results.Add(CommandResult.Failed($"Command {command.GetType().Name} failed: {ex.Message}"));
                }
            }
            
            // Process results and notify UI (for backward compatibility, use first successful auto-save result)
            var autoSaveResult = results.FirstOrDefault(r => r.Success && r.Data is MoodEntryOld);
            var autoSaveDecision = autoSaveResult != null ? MapResultToDecision(autoSaveResult) : AutoSaveDecision.NoAction;
            
            // Notify UI about date change
            DateChanged?.Invoke(this, new DateChangeEventArgs 
            { 
                OldDate = oldDate, 
                NewDate = newDate, 
                AutoSaveDecision = autoSaveDecision 
            });
            
            // If a record was saved, notify UI
            if (autoSaveResult?.Data is MoodEntryOld savedRecord)
            {
                AutoSaveOccurred?.Invoke(this, new AutoSaveEventArgs 
                { 
                    SavedRecord = savedRecord, 
                    SavedDate = oldDate 
                });
            }
            
            // Check for morning reminder results
            var morningReminderResult = results.FirstOrDefault(r => r.Success && r.Data is MorningReminderData);
            if (morningReminderResult?.Data is MorningReminderData reminderData)
            {
                MorningReminderOccurred?.Invoke(this, new MorningReminderEventArgs
                {
                    MorningTime = reminderData.MorningTime,
                    TimeSinceMorning = reminderData.TimeSinceMorning,
                    CallCount = reminderData.CallCount,
                    Message = morningReminderResult.Message ?? "Time to record your morning mood!"
                });
            }
        }
        catch (Exception ex)
        {
            Log($"MoodDispatcherService: Error handling date change: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps CommandResult to AutoSaveDecision for backward compatibility with UI
    /// </summary>
    private AutoSaveDecision MapResultToDecision(CommandResult result)
    {
        if (!result.Success)
        {
            return AutoSaveDecision.NoAction;
        }

        if (result.Data is MoodEntryOld)
        {
            return AutoSaveDecision.SaveRecord;
        }

        // Check message content to determine the decision
        if (result.Message?.Contains("already complete") == true)
        {
            return AutoSaveDecision.AlreadySaved;
        }

        if (result.Message?.Contains("no valid mood data") == true)
        {
            return AutoSaveDecision.InvalidState;
        }

        return AutoSaveDecision.NoAction;
    }

    public void Start()
    {
        if (!_timer.Enabled)
        {
            _timer.Enabled = true;
            Log("MoodDispatcherService: Timer started");
        }
    }

    public void Stop()
    {
        if (_timer.Enabled)
        {
            _timer.Enabled = false;
            Log("MoodDispatcherService: Timer stopped");
        }
    }

    private void Log(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            
            // Try multiple locations
            var locations = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WorkMood_Debug.log"),
                Path.Combine(Path.GetTempPath(), "WorkMood_Debug.log"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkMood_Debug.log"),
                @"C:\WorkMood_Debug.log"
            };
            
            foreach (var location in locations)
            {
                try
                {
                    File.AppendAllText(location, logEntry + Environment.NewLine);
                    break; // If successful, don't try other locations
                }
                catch
                {
                    continue; // Try next location
                }
            }
        }
        catch (Exception ex)
        {
            // Try to log the exception to a fallback location
            try
            {
                var fallbackPath = Path.Combine(Path.GetTempPath(), "WorkMood_Debug_Fallback.log");
                File.AppendAllText(fallbackPath, $"[{DateTime.Now}] LOGGING ERROR: {ex.Message} | Original message: {message}" + Environment.NewLine);
            }
            catch
            {
                // Ultimate fallback - ignore completely
            }
        }
    }

    /// <summary>
    /// Cleans up schedule configuration by removing past overrides at day transition
    /// </summary>
    private async Task CleanupScheduleConfig()
    {
        try
        {
            Log("MoodDispatcherService: Starting schedule configuration cleanup");
            
            // Load current config
            var currentConfig = await _scheduleConfigService.LoadScheduleConfigAsync();
            
            // Use the centralized update method to clean up past overrides
            // This maintains the current times but removes expired overrides
            await _scheduleConfigService.UpdateScheduleConfigAsync(
                currentConfig.MorningTime, 
                currentConfig.EveningTime, 
                newOverride: null // No new override to add
            );
            
            Log("MoodDispatcherService: Schedule configuration cleanup completed");
        }
        catch (Exception ex)
        {
            Log($"MoodDispatcherService: Error during schedule config cleanup: {ex.Message}");
            // Don't rethrow - this shouldn't stop other day transition processes
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer?.Dispose();
            Log("MoodDispatcherService: Disposed");
            _disposed = true;
        }
    }
}

/// <summary>
/// Event args for when a date change is detected
/// </summary>
public class DateChangeEventArgs : EventArgs
{
    public DateOnly OldDate { get; set; }
    public DateOnly NewDate { get; set; }
    public AutoSaveDecision AutoSaveDecision { get; set; }
}

/// <summary>
/// Event args for when an auto-save occurs
/// </summary>
public class AutoSaveEventArgs : EventArgs
{
    public MoodEntryOld SavedRecord { get; set; } = null!;
    public DateOnly SavedDate { get; set; }
}

/// <summary>
/// Event args for when a morning reminder should be shown
/// </summary>
public class MorningReminderEventArgs : EventArgs
{
    public DateTime MorningTime { get; set; }
    public TimeSpan TimeSinceMorning { get; set; }
    public int CallCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Event args for when an evening reminder should be shown
/// </summary>
public class EveningReminderEventArgs : EventArgs
{
    public DateTime EveningTime { get; set; }
    public TimeSpan TimeSinceEvening { get; set; }
    public int CallCount { get; set; }
    public EveningReminderType ReminderType { get; set; }
    public bool MorningMissed { get; set; }
    public bool EveningNeeded { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Possible decisions for auto-saving when date changes
/// </summary>
public enum AutoSaveDecision
{
    NoAction,        // No record found
    AlreadySaved,    // Record already complete
    SaveRecord,      // Record has valid data - save it
    InvalidState     // Record exists but has no valid data
}