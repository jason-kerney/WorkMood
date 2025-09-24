using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Command that handles auto-save logic when date changes occur
/// Follows Command Pattern and Single Responsibility Principle
/// </summary>
public class AutoSaveCommand : IDispatcherCommand
{
    private readonly MoodDataService _moodDataService;

    public AutoSaveCommand(MoodDataService moodDataService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
    }

    /// <summary>
    /// Processes a timer tick to handle auto-save logic for date changes
    /// </summary>
    public async Task<CommandResult> ProcessTickAsync(DateOnly oldDate, DateOnly newDate, MoodEntry? currentRecord = null)
    {
        try
        {
            // Get the record for the old date to evaluate
            var recordToCheck = currentRecord ?? await _moodDataService.GetMoodEntryAsync(oldDate);
            
            if (recordToCheck == null)
            {
                return CommandResult.NoAction("No record found for the previous date");
            }

            // Determine auto-save action based on record state
            var decision = DetermineAutoSaveAction(recordToCheck);
            
            // Execute the save if needed
            if (decision == AutoSaveDecision.SaveRecord)
            {
                // Use auto-save defaults for end-of-day saves (evening defaults to morning if not set)
                await _moodDataService.SaveMoodEntryAsync(recordToCheck, useAutoSaveDefaults: true);
                return CommandResult.Succeeded($"Auto-saved record for {oldDate:yyyy-MM-dd}", recordToCheck);
            }

            // Return appropriate result based on decision
            return decision switch
            {
                AutoSaveDecision.AlreadySaved => CommandResult.NoAction("Record already complete"),
                AutoSaveDecision.InvalidState => CommandResult.NoAction("Record exists but has no valid mood data"),
                _ => CommandResult.NoAction("No action needed")
            };
        }
        catch (Exception ex)
        {
            return CommandResult.Failed($"Error during auto-save: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines what action should be taken for auto-saving based on record state
    /// </summary>
    private AutoSaveDecision DetermineAutoSaveAction(MoodEntry record)
    {
        // Record is already complete - no action needed
        if (record.MorningMood.HasValue && record.EveningMood.HasValue)
        {
            return AutoSaveDecision.AlreadySaved;
        }

        // Record has at least morning mood (minimum valid state for saving)
        if (record.MorningMood.HasValue)
        {
            return AutoSaveDecision.SaveRecord;
        }

        // Record exists but has no valid data - consider it invalid
        return AutoSaveDecision.InvalidState;
    }
}