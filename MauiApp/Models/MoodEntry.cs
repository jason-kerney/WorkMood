using System.Text.Json.Serialization;

namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents a daily mood entry with morning and evening moods on a scale of 1-10
/// </summary>
public class MoodEntry
{
    // Backing fields for synchronized properties
    private int? _morningStartOfWork;
    private int? _eveningEndOfWork;

    /// <summary>
    /// The date of this mood entry (date only, no time component)
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Morning mood rating (1-10). Null if not recorded.
    /// Synchronized with StartOfWork - both properties reflect the same value.
    /// </summary>
    public int? MorningMood 
    { 
        get => _morningStartOfWork;
        set 
        {
            if (value.HasValue)
                _morningStartOfWork = value;
        }
    }

    /// <summary>
    /// Evening mood rating (1-10). Null if not recorded.
    /// Synchronized with EndOfWork - both properties reflect the same value.
    /// If not explicitly set and MorningMood exists, defaults to MorningMood value when saving.
    /// </summary>
    public int? EveningMood 
    { 
        get => _eveningEndOfWork;
        set 
        {
            if (value.HasValue)
                _eveningEndOfWork = value;
        }
    }

    /// <summary>
    /// Start of work mood rating (1-10). Null if not recorded.
    /// Synchronized with MorningMood - both properties reflect the same value.
    /// </summary>
    public int? StartOfWork 
    { 
        get => _morningStartOfWork;
        set 
        {
            if (value.HasValue)
                _morningStartOfWork = value;
        }
    }

    /// <summary>
    /// End of work mood rating (1-10). Null if not recorded.
    /// Synchronized with EveningMood - both properties reflect the same value.
    /// </summary>
    public int? EndOfWork 
    { 
        get => _eveningEndOfWork;
        set 
        {
            if (value.HasValue)
                _eveningEndOfWork = value;
        }
    }

    /// <summary>
    /// Timestamp when this entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when this entry was last modified
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Computed value based on mood change throughout the day.
    /// Formula: (evening ?? morning) - morning
    /// Positive = mood improved, Zero = no change, Negative = mood declined
    /// This property is not serialized.
    /// </summary>
    [JsonIgnore]
    public double? Value
    {
        get
        {
            if (!StartOfWork.HasValue)
                return null;
            
            var endOfWorkOrStartOfWork = EndOfWork ?? StartOfWork.Value;
            return endOfWorkOrStartOfWork - StartOfWork.Value;
        }
    }

    /// <summary>
    /// Creates a new mood entry for today
    /// </summary>
    public MoodEntry()
    {
        Date = DateOnly.FromDateTime(DateTime.Today);
        CreatedAt = DateTime.Now;
        LastModified = DateTime.Now;
    }

    /// <summary>
    /// Creates a new mood entry for the specified date
    /// </summary>
    /// <param name="date">The date for this mood entry</param>
    public MoodEntry(DateOnly date)
    {
        Date = date;
        CreatedAt = DateTime.Now;
        LastModified = DateTime.Now;
    }

    /// <summary>
    /// Validates that mood values are within the valid range (1-10)
    /// </summary>
    /// <returns>True if all set mood values are valid</returns>
    public bool IsValid()
    {
        // Since MorningMood and StartOfWork are synchronized, only need to validate the backing field once
        if (_morningStartOfWork.HasValue && (_morningStartOfWork < 1 || _morningStartOfWork > 10))
            return false;
        
        // Since EveningMood and EndOfWork are synchronized, only need to validate the backing field once
        if (_eveningEndOfWork.HasValue && (_eveningEndOfWork < 1 || _eveningEndOfWork > 10))
            return false;
        
        return true;
    }

    /// <summary>
    /// Determines if this entry should be saved based on business rules:
    /// - Must have a start of work mood to be saved
    /// - End of work mood is optional and defaults to start of work mood if not set
    /// </summary>
    /// <returns>True if this entry should be saved</returns>
    public bool ShouldSave()
    {
        return StartOfWork.HasValue && IsValid();
    }

    /// <summary>
    /// Prepares the entry for saving by applying business rules
    /// </summary>
    /// <param name="useAutoSaveDefaults">If true, applies auto-save defaults like setting end of work mood to start of work mood</param>
    public void PrepareForSave(bool useAutoSaveDefaults = false)
    {
        if (ShouldSave())
        {
            // Apply auto-save defaults only when explicitly requested (e.g., end of day auto-save)
            if (useAutoSaveDefaults && !EndOfWork.HasValue && StartOfWork.HasValue)
            {
                EndOfWork = StartOfWork;
            }
            
            LastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// Gets the average mood for the day (if both moods are set)
    /// </summary>
    /// <returns>Average mood or null if insufficient data</returns>
    public double? GetAverageMood()
    {
        if (StartOfWork.HasValue && EndOfWork.HasValue)
        {
            return (StartOfWork.Value + EndOfWork.Value) / 2.0;
        }
        
        return null;
    }

    /// <summary>
    /// Gets a display string for the mood entry
    /// </summary>
    /// <returns>Formatted string representation</returns>
    public override string ToString()
    {
        var startOfWork = StartOfWork?.ToString() ?? "Not recorded";
        var endOfWork = EndOfWork?.ToString() ?? "Not recorded";
        return $"{Date:yyyy-MM-dd}: Start of Work {startOfWork}, End of Work {endOfWork}";
    }
}