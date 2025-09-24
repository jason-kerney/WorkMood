using System.Text.Json.Serialization;

namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents a daily mood entry with morning and evening moods on a scale of 1-10
/// </summary>
public class MoodEntry
{
    /// <summary>
    /// The date of this mood entry (date only, no time component)
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Morning mood rating (1-10). Null if not recorded.
    /// </summary>
    public int? MorningMood { get; set; }

    /// <summary>
    /// Evening mood rating (1-10). Null if not recorded.
    /// If not explicitly set and MorningMood exists, defaults to MorningMood value when saving.
    /// </summary>
    public int? EveningMood { get; set; }

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
            if (!MorningMood.HasValue)
                return null;
            
            var eveningOrMorning = EveningMood ?? MorningMood.Value;
            return eveningOrMorning - MorningMood.Value;
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
        if (MorningMood.HasValue && (MorningMood < 1 || MorningMood > 10))
            return false;
        
        if (EveningMood.HasValue && (EveningMood < 1 || EveningMood > 10))
            return false;
        
        return true;
    }

    /// <summary>
    /// Determines if this entry should be saved based on business rules:
    /// - Must have a morning mood to be saved
    /// - Evening mood is optional and defaults to morning mood if not set
    /// </summary>
    /// <returns>True if this entry should be saved</returns>
    public bool ShouldSave()
    {
        return MorningMood.HasValue && IsValid();
    }

    /// <summary>
    /// Prepares the entry for saving by applying business rules
    /// </summary>
    /// <param name="useAutoSaveDefaults">If true, applies auto-save defaults like setting evening mood to morning mood</param>
    public void PrepareForSave(bool useAutoSaveDefaults = false)
    {
        if (ShouldSave())
        {
            // Apply auto-save defaults only when explicitly requested (e.g., end of day auto-save)
            if (useAutoSaveDefaults && !EveningMood.HasValue && MorningMood.HasValue)
            {
                EveningMood = MorningMood;
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
        if (MorningMood.HasValue && EveningMood.HasValue)
        {
            return (MorningMood.Value + EveningMood.Value) / 2.0;
        }
        
        return null;
    }

    /// <summary>
    /// Gets a display string for the mood entry
    /// </summary>
    /// <returns>Formatted string representation</returns>
    public override string ToString()
    {
        var morning = MorningMood?.ToString() ?? "Not recorded";
        var evening = EveningMood?.ToString() ?? "Not recorded";
        return $"{Date:yyyy-MM-dd}: Morning {morning}, Evening {evening}";
    }
}