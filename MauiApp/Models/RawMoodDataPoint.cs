namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents a single raw mood data point with timestamp information
/// Used for Raw Data graph mode visualization
/// </summary>
public class RawMoodDataPoint
{
    /// <summary>
    /// The timestamp when this mood reading was taken
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The mood value (1-10)
    /// </summary>
    public int MoodValue { get; set; }

    /// <summary>
    /// Indicates whether this is a start-of-work or end-of-work reading
    /// </summary>
    public MoodType MoodType { get; set; }

    /// <summary>
    /// The original date this mood entry belongs to
    /// </summary>
    public DateOnly OriginalDate { get; set; }

    /// <summary>
    /// Creates a raw mood data point
    /// </summary>
    public RawMoodDataPoint(DateTime timestamp, int moodValue, MoodType moodType, DateOnly originalDate)
    {
        Timestamp = timestamp;
        MoodValue = moodValue;
        MoodType = moodType;
        OriginalDate = originalDate;
    }
}

/// <summary>
/// Indicates the type of mood reading
/// </summary>
public enum MoodType
{
    StartOfWork,
    EndOfWork
}