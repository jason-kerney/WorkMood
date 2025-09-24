using System.Text.Json.Serialization;

namespace WorkMood.MauiApp.Models;

public class ScheduleConfig
{
    [JsonPropertyName("morningTime")]
    public TimeSpan MorningTime { get; set; }

    [JsonPropertyName("eveningTime")]
    public TimeSpan EveningTime { get; set; }

    [JsonPropertyName("overrides")]
    public List<ScheduleOverride> Overrides { get; set; }

    public ScheduleConfig()
    {
        // Default values
        MorningTime = new TimeSpan(8, 20, 0);  // 08:20:00
        EveningTime = new TimeSpan(17, 20, 0); // 17:20:00
        Overrides = new List<ScheduleOverride>();
    }

    public ScheduleConfig(TimeSpan morningTime, TimeSpan eveningTime)
    {
        MorningTime = morningTime;
        EveningTime = eveningTime;
        Overrides = new List<ScheduleOverride>();
    }

    /// <summary>
    /// Gets the effective morning time for a specific date, considering overrides
    /// </summary>
    public TimeSpan GetEffectiveMorningTime(DateOnly date)
    {
        var override_ = Overrides.FirstOrDefault(o => o.AppliesToDate(date));
        return override_?.MorningTime ?? MorningTime;
    }

    /// <summary>
    /// Gets the effective evening time for a specific date, considering overrides
    /// </summary>
    public TimeSpan GetEffectiveEveningTime(DateOnly date)
    {
        var override_ = Overrides.FirstOrDefault(o => o.AppliesToDate(date));
        return override_?.EveningTime ?? EveningTime;
    }

    /// <summary>
    /// Gets the effective morning time for today, considering overrides
    /// </summary>
    public TimeSpan GetEffectiveMorningTimeToday()
    {
        return GetEffectiveMorningTime(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Gets the effective evening time for today, considering overrides
    /// </summary>
    public TimeSpan GetEffectiveEveningTimeToday()
    {
        return GetEffectiveEveningTime(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Adds or updates an override for a specific date
    /// </summary>
    public void SetOverride(DateOnly date, TimeSpan? morningTime = null, TimeSpan? eveningTime = null)
    {
        // Remove existing override for this date
        Overrides.RemoveAll(o => o.Date == date);
        
        // Add new override if at least one time is specified
        if (morningTime.HasValue || eveningTime.HasValue)
        {
            Overrides.Add(new ScheduleOverride(date, morningTime, eveningTime));
        }
    }

    /// <summary>
    /// Removes any override for a specific date
    /// </summary>
    public void RemoveOverride(DateOnly date)
    {
        Overrides.RemoveAll(o => o.Date == date);
    }

    /// <summary>
    /// Gets the override for a specific date, if any
    /// </summary>
    public ScheduleOverride? GetOverride(DateOnly date)
    {
        return Overrides.FirstOrDefault(o => o.AppliesToDate(date));
    }

    /// <summary>
    /// Cleans up old overrides (removes overrides older than 30 days)
    /// </summary>
    public void CleanupOldOverrides()
    {
        var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        Overrides.RemoveAll(o => o.Date < cutoffDate);
    }
}