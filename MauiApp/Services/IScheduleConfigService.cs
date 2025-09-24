using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for schedule configuration management following the Interface Segregation Principle
/// </summary>
public interface IScheduleConfigService
{
    /// <summary>
    /// Loads the current schedule configuration
    /// </summary>
    /// <returns>The current schedule configuration</returns>
    Task<ScheduleConfig> LoadScheduleConfigAsync();

    /// <summary>
    /// Saves the schedule configuration
    /// </summary>
    /// <param name="config">The configuration to save</param>
    Task SaveScheduleConfigAsync(ScheduleConfig config);

    /// <summary>
    /// Updates the schedule configuration with new times and optional override
    /// </summary>
    /// <param name="morningTime">New morning time</param>
    /// <param name="eveningTime">New evening time</param>
    /// <param name="newOverride">Optional new override to add</param>
    /// <returns>The updated configuration</returns>
    Task<ScheduleConfig> UpdateScheduleConfigAsync(TimeSpan morningTime, TimeSpan eveningTime, ScheduleOverride? newOverride = null);
}