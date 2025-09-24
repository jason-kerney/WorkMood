using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Command that handles evening mood reminder alerts based on schedule configuration
/// Reminds users to record their evening mood and also prompts for missed morning mood
/// </summary>
public class EveningReminderCommand : IDispatcherCommand
{
    private readonly MoodDataService _moodDataService;
    private readonly ScheduleConfigService _scheduleConfigService;
    private int _callCount = 0;
    private DateOnly _lastReminderDate = DateOnly.MinValue;

    public EveningReminderCommand(MoodDataService moodDataService, ScheduleConfigService scheduleConfigService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));
    }

    /// <summary>
    /// Processes a timer tick to check if an evening mood reminder should be sent
    /// </summary>
    public async Task<CommandResult> ProcessTickAsync(DateOnly oldDate, DateOnly newDate, MoodEntry? currentRecord = null)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = DateTime.Now;
            
            // Get the schedule configuration
            var config = await _scheduleConfigService.LoadScheduleConfigAsync();
            
            // Get the effective evening time (considering overrides)
            var effectiveEveningTime = config.GetEffectiveEveningTimeToday();
            
            // Create today's evening time from the effective configuration
            var eveningTime = DateTime.Today.Add(effectiveEveningTime);
            
            Log($"EveningReminderCommand: Current time: {now:HH:mm:ss}, Evening time: {eveningTime:HH:mm:ss}");
            
            // Check if current time is after the evening time
            if (now <= eveningTime)
            {
                return CommandResult.NoAction("Current time is before evening time");
            }
            
            // Check if time is within 10 minutes of the evening time
            var timeSinceEvening = now - eveningTime;
            if (timeSinceEvening.TotalMinutes > 10)
            {
                return CommandResult.NoAction("Current time is more than 10 minutes past evening time");
            }
            
            // Get today's mood record to check mood status
            var todayRecord = currentRecord ?? await _moodDataService.GetMoodEntryAsync(today);
            
            // Determine what reminders are needed
            var reminderType = DetermineReminderType(todayRecord);
            
            if (reminderType == EveningReminderType.NoReminder)
            {
                return CommandResult.NoAction("All moods have been recorded for today");
            }
            
            // Reset call count if this is a new day
            if (_lastReminderDate != today)
            {
                _callCount = 0;
                _lastReminderDate = today;
            }
            
            // Increment call count
            _callCount++;
            
            // Only send reminder every other time (on even call counts)
            if (_callCount % 2 != 0)
            {
                Log($"EveningReminderCommand: Skipping reminder - call count: {_callCount} (odd)");
                return CommandResult.NoAction($"Skipping reminder - call count: {_callCount}");
            }
            
            Log($"EveningReminderCommand: Sending evening mood reminder - call count: {_callCount}, type: {reminderType}");
            
            // Create the reminder message based on what needs to be recorded
            var message = CreateReminderMessage(reminderType);
            
            // Send the reminder
            var reminderData = new EveningReminderData
            {
                EveningTime = eveningTime,
                TimeSinceEvening = timeSinceEvening,
                CallCount = _callCount,
                ReminderType = reminderType,
                MorningMissed = reminderType == EveningReminderType.EveningAndMissedMorning || reminderType == EveningReminderType.OnlyMissedMorning,
                EveningNeeded = reminderType == EveningReminderType.EveningOnly || reminderType == EveningReminderType.EveningAndMissedMorning
            };
            
            return CommandResult.Succeeded(message, reminderData);
        }
        catch (Exception ex)
        {
            Log($"EveningReminderCommand: Error processing tick: {ex.Message}");
            return CommandResult.Failed($"Error processing evening reminder: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Determines what type of evening reminder is needed based on current mood record status
    /// </summary>
    private EveningReminderType DetermineReminderType(MoodEntry? todayRecord)
    {
        var hasMorning = todayRecord?.MorningMood.HasValue == true;
        var hasEvening = todayRecord?.EveningMood.HasValue == true;
        
        if (hasMorning && hasEvening)
        {
            // Both moods recorded - no reminder needed
            return EveningReminderType.NoReminder;
        }
        else if (hasMorning && !hasEvening)
        {
            // Only morning recorded - remind for evening
            return EveningReminderType.EveningOnly;
        }
        else if (!hasMorning && hasEvening)
        {
            // Only evening recorded - remind for missed morning
            return EveningReminderType.OnlyMissedMorning;
        }
        else
        {
            // Neither recorded - remind for both
            return EveningReminderType.EveningAndMissedMorning;
        }
    }
    
    /// <summary>
    /// Creates an appropriate reminder message based on the reminder type
    /// </summary>
    private string CreateReminderMessage(EveningReminderType reminderType)
    {
        return reminderType switch
        {
            EveningReminderType.EveningOnly => "Time to record your evening mood!",
            EveningReminderType.OnlyMissedMorning => "Don't forget to record your missed morning mood!",
            EveningReminderType.EveningAndMissedMorning => "Time to record your evening mood! You also missed recording your morning mood today.",
            _ => "Time to record your mood!"
        };
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
}

/// <summary>
/// Types of evening reminders that can be triggered
/// </summary>
public enum EveningReminderType
{
    NoReminder,                 // Both moods recorded
    EveningOnly,               // Morning recorded, evening needed
    OnlyMissedMorning,         // Evening recorded, morning missed
    EveningAndMissedMorning    // Both moods needed
}

/// <summary>
/// Data class for evening reminder information
/// </summary>
public class EveningReminderData
{
    public DateTime EveningTime { get; set; }
    public TimeSpan TimeSinceEvening { get; set; }
    public int CallCount { get; set; }
    public EveningReminderType ReminderType { get; set; }
    public bool MorningMissed { get; set; }
    public bool EveningNeeded { get; set; }
}