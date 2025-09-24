using System.Text.Json.Serialization;

namespace WorkMood.MauiApp.Models;

public class ScheduleOverride
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("morningTime")]
    public TimeSpan? MorningTime { get; set; }

    [JsonPropertyName("eveningTime")]
    public TimeSpan? EveningTime { get; set; }

    public ScheduleOverride()
    {
    }

    public ScheduleOverride(DateOnly date, TimeSpan? morningTime = null, TimeSpan? eveningTime = null)
    {
        Date = date;
        MorningTime = morningTime;
        EveningTime = eveningTime;
    }

    /// <summary>
    /// Returns true if this override has at least one time specified
    /// </summary>
    public bool HasOverride => MorningTime.HasValue || EveningTime.HasValue;

    /// <summary>
    /// Returns true if this override applies to the specified date
    /// </summary>
    public bool AppliesToDate(DateOnly date) => Date == date;

    /// <summary>
    /// Returns true if this override applies to today
    /// </summary>
    public bool AppliesToToday() => AppliesToDate(DateOnly.FromDateTime(DateTime.Today));
}