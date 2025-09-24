using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Command that handles morning mood reminder alerts based on schedule configuration
/// Reminds users to record their morning mood if they haven't done so within 10 minutes of the scheduled time
/// </summary>
public class MorningReminderCommand : IDispatcherCommand
{
    private readonly MoodDataService _moodDataService;
    private readonly ScheduleConfigService _scheduleConfigService;
    private int _callCount = 0;
    private DateOnly _lastReminderDate = DateOnly.MinValue;

    public MorningReminderCommand(MoodDataService moodDataService, ScheduleConfigService scheduleConfigService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));
    }

    /// <summary>
    /// Processes a timer tick to check if a morning mood reminder should be sent
    /// </summary>
    public async Task<CommandResult> ProcessTickAsync(DateOnly oldDate, DateOnly newDate, MoodEntry? currentRecord = null)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = DateTime.Now;
            
            // Get the schedule configuration
            var config = await _scheduleConfigService.LoadScheduleConfigAsync();
            
            // Get the effective morning time (considering overrides)
            var effectiveMorningTime = config.GetEffectiveMorningTimeToday();
            
            // Create today's morning time from the effective configuration
            var morningTime = DateTime.Today.Add(effectiveMorningTime);
            
            Log($"MorningReminderCommand: Current time: {now:HH:mm:ss}, Morning time: {morningTime:HH:mm:ss}");
            
            // Check if current time is after the morning time
            if (now <= morningTime)
            {
                return CommandResult.NoAction("Current time is before morning time");
            }
            
            // Check if time is within 10 minutes of the morning time
            var timeSinceMorning = now - morningTime;
            if (timeSinceMorning.TotalMinutes > 10)
            {
                return CommandResult.NoAction("Current time is more than 10 minutes past morning time");
            }
            
            // Get today's mood record to check if morning mood has been saved
            var todayRecord = currentRecord ?? await _moodDataService.GetMoodEntryAsync(today);
            
            // Check if morning mood has already been saved
            if (todayRecord?.MorningMood.HasValue == true)
            {
                return CommandResult.NoAction("Morning mood has already been recorded for today");
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
                Log($"MorningReminderCommand: Skipping reminder - call count: {_callCount} (odd)");
                return CommandResult.NoAction($"Skipping reminder - call count: {_callCount}");
            }
            
            Log($"MorningReminderCommand: Sending morning mood reminder - call count: {_callCount}");
            
            // Send the reminder
            var reminderData = new MorningReminderData
            {
                MorningTime = morningTime,
                TimeSinceMorning = timeSinceMorning,
                CallCount = _callCount
            };
            
            return CommandResult.Succeeded("Morning mood reminder sent", reminderData);
        }
        catch (Exception ex)
        {
            Log($"MorningReminderCommand: Error processing tick: {ex.Message}");
            return CommandResult.Failed($"Error processing morning reminder: {ex.Message}");
        }
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
/// Data class for morning reminder information
/// </summary>
public class MorningReminderData
{
    public DateTime MorningTime { get; set; }
    public TimeSpan TimeSinceMorning { get; set; }
    public int CallCount { get; set; }
}