namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for migrating app data from LocalApplicationData to Documents folder
/// Enables OneDrive/iCloud sync and multi-machine support
/// </summary>
public interface IDataMigrationService
{
    /// <summary>
    /// Checks if migration is needed and performs it if necessary
    /// Safe to call multiple times - will only migrate once
    /// </summary>
    Task MigrateIfNeededAsync();
}
