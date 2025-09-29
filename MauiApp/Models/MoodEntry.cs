using System.Text.Json.Serialization;

namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents a daily mood entry with start and end of work moods on a scale of 1-10
/// </summary>
public class MoodEntry
{
    /// <summary>
    /// The date of this mood entry (date only, no time component)
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Start of work mood rating (1-10). Null if not recorded.
    /// </summary>
    public int? StartOfWork { get; set; }

    /// <summary>
    /// End of work mood rating (1-10). Null if not recorded.
    /// </summary>
    public int? EndOfWork { get; set; }

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
        if (StartOfWork.HasValue && (StartOfWork < 1 || StartOfWork > 10))
            return false;
        
        if (EndOfWork.HasValue && (EndOfWork < 1 || EndOfWork > 10))
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
    /// Gets the adjusted average mood for graphing purposes.
    /// Uses EndOfWork if available, otherwise uses StartOfWork for both values.
    /// Subtracts 5 from the average to shift the range from 1-10 to -4 to +5.
    /// </summary>
    /// <returns>Adjusted average mood or null if no start of work mood is recorded</returns>
    public double? GetAdjustedAverageMood()
    {
        if (!StartOfWork.HasValue)
            return null;
            
        var endOfWorkOrStartOfWork = EndOfWork ?? StartOfWork.Value;
        var average = (StartOfWork.Value + endOfWorkOrStartOfWork) / 2.0;
        return average - 5.0; // Shift from 1-10 range to -4 to +5 range
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