using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Migrates app data from LocalApplicationData to Documents folder
/// Enables cloud sync (OneDrive, iCloud) and multi-machine support
/// </summary>
public class DataMigrationService : IDataMigrationService
{
    private readonly IFolderShim _folderShim;
    private readonly IFileShim _fileShim;
    private readonly ILoggingService _loggingService;
    private const string MigrationCompleteMarker = ".migration_complete";

    public DataMigrationService(
        IFolderShim folderShim,
        IFileShim fileShim,
        ILoggingService loggingService)
    {
        _folderShim = folderShim ?? throw new ArgumentNullException(nameof(folderShim));
        _fileShim = fileShim ?? throw new ArgumentNullException(nameof(fileShim));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    public async Task MigrateIfNeededAsync()
    {
        Log("MigrateIfNeededAsync: Starting migration check");

        try
        {
            // If new location already has migration marker, skip
            if (MigrationAlreadyCompleted())
            {
                Log("MigrateIfNeededAsync: Migration already completed, skipping");
                return;
            }

            // If old location has no data, nothing to migrate
            if (!OldLocationHasData())
            {
                Log("MigrateIfNeededAsync: No data in old location, marking as complete");
                await CreateMigrationMarkerAsync();
                return;
            }

            // If new location already has data that's newer, keep it
            if (NewLocationHasNewerData())
            {
                Log("MigrateIfNeededAsync: New location has newer data, skipping migration");
                await CreateMigrationMarkerAsync();
                return;
            }

            // Perform the migration
            await PerformMigrationAsync();
            await CreateMigrationMarkerAsync();
            Log("MigrateIfNeededAsync: Migration completed successfully");
        }
        catch (Exception ex)
        {
            Log($"MigrateIfNeededAsync: Migration failed with error: {ex.Message}");
            Log($"MigrateIfNeededAsync: Stack trace: {ex.StackTrace}");
            // Don't throw - allow app to continue. Data services will handle missing files gracefully
        }
    }

    private bool MigrationAlreadyCompleted()
    {
        var newLocation = _folderShim.GetDocumentsFolder();
        var markerPath = _folderShim.CombinePaths(newLocation, MigrationCompleteMarker);
        return _fileShim.Exists(markerPath);
    }

    private bool OldLocationHasData()
    {
        var oldLocation = _folderShim.GetApplicationFolder();
        
        if (!_folderShim.DirectoryExists(oldLocation))
            return false;

        // Check for key data files
        var moodDataPath = _folderShim.CombinePaths(oldLocation, "mood_data.json");
        var scheduleConfigPath = _folderShim.CombinePaths(oldLocation, "schedule_config.json");

        return _fileShim.Exists(moodDataPath) || _fileShim.Exists(scheduleConfigPath);
    }

    private bool NewLocationHasNewerData()
    {
        var newLocation = _folderShim.GetDocumentsFolder();
        var oldLocation = _folderShim.GetApplicationFolder();

        if (!_folderShim.DirectoryExists(newLocation))
            return false;

        // Compare modification times for key files
        var oldMoodDataPath = _folderShim.CombinePaths(oldLocation, "mood_data.json");
        var newMoodDataPath = _folderShim.CombinePaths(newLocation, "mood_data.json");

        if (_fileShim.Exists(oldMoodDataPath) && _fileShim.Exists(newMoodDataPath))
        {
            var oldModified = File.GetLastWriteTimeUtc(oldMoodDataPath);
            var newModified = File.GetLastWriteTimeUtc(newMoodDataPath);

            if (newModified > oldModified)
            {
                Log($"MigrateIfNeededAsync: New mood_data.json is newer (new: {newModified}, old: {oldModified})");
                return true;
            }
        }

        return false;
    }

    private async Task PerformMigrationAsync()
    {
        Log("PerformMigrationAsync: Starting data migration");

        var oldLocation = _folderShim.GetApplicationFolder();
        var newLocation = _folderShim.GetDocumentsFolder();

        // Ensure destination exists
        _folderShim.CreateDirectory(newLocation);
        Log($"PerformMigrationAsync: Ensured new location exists: {newLocation}");

        // Migrate key data files
        await MigrateFileIfExistsAsync(
            _folderShim.CombinePaths(oldLocation, "mood_data.json"),
            _folderShim.CombinePaths(newLocation, "mood_data.json"),
            "mood_data.json");

        await MigrateFileIfExistsAsync(
            _folderShim.CombinePaths(oldLocation, "schedule_config.json"),
            _folderShim.CombinePaths(newLocation, "schedule_config.json"),
            "schedule_config.json");

        // Migrate archives folder
        await MigrateArchivesFolderAsync(oldLocation, newLocation);

        Log("PerformMigrationAsync: Data migration completed");
    }

    private async Task MigrateFileIfExistsAsync(string sourcePath, string destinationPath, string fileName)
    {
        if (!_fileShim.Exists(sourcePath))
        {
            Log($"PerformMigrationAsync: Source file doesn't exist, skipping: {fileName}");
            return;
        }

        try
        {
            Log($"PerformMigrationAsync: Migrating {fileName}");

            // Read source
            var content = await _fileShim.ReadAllTextAsync(sourcePath);
            
            // Write to destination (overwrites if exists, but we checked for newer data already)
            await _fileShim.WriteAllTextAsync(destinationPath, content);
            
            Log($"PerformMigrationAsync: Successfully migrated {fileName}");
        }
        catch (Exception ex)
        {
            Log($"PerformMigrationAsync: Error migrating {fileName}: {ex.Message}");
            throw;
        }
    }

    private async Task MigrateArchivesFolderAsync(string oldLocation, string newLocation)
    {
        var oldArchivesPath = _folderShim.CombinePaths(oldLocation, "archives");
        var newArchivesPath = _folderShim.CombinePaths(newLocation, "archives");

        if (!_folderShim.DirectoryExists(oldArchivesPath))
        {
            Log("PerformMigrationAsync: No archives folder to migrate");
            return;
        }

        try
        {
            Log("PerformMigrationAsync: Starting archives migration");

            // Create archives directory in new location if needed
            if (!_folderShim.DirectoryExists(newArchivesPath))
                _folderShim.CreateDirectory(newArchivesPath);

            // Get all archive files
            var archiveFiles = _folderShim.GetFiles(oldArchivesPath, "*.json");

            foreach (var archiveFile in archiveFiles)
            {
                var fileName = _folderShim.GetFileName(archiveFile);
                var destinationPath = _folderShim.CombinePaths(newArchivesPath, fileName);

                try
                {
                    var content = await _fileShim.ReadAllTextAsync(archiveFile);
                    await _fileShim.WriteAllTextAsync(destinationPath, content);
                    Log($"PerformMigrationAsync: Migrated archive file: {fileName}");
                }
                catch (Exception ex)
                {
                    Log($"PerformMigrationAsync: Error migrating archive {fileName}: {ex.Message}");
                    // Continue with other files even if one fails
                }
            }

            Log("PerformMigrationAsync: Archives migration completed");
        }
        catch (Exception ex)
        {
            Log($"PerformMigrationAsync: Error during archives migration: {ex.Message}");
            throw;
        }
    }

    private async Task CreateMigrationMarkerAsync()
    {
        try
        {
            var newLocation = _folderShim.GetDocumentsFolder();
            _folderShim.CreateDirectory(newLocation);
            
            var markerPath = _folderShim.CombinePaths(newLocation, MigrationCompleteMarker);
            await _fileShim.WriteAllTextAsync(markerPath, "Migration completed");
            
            Log("CreateMigrationMarker: Migration marker created successfully");
        }
        catch (Exception ex)
        {
            Log($"CreateMigrationMarker: Error creating marker: {ex.Message}");
            // Don't throw - this is non-critical
        }
    }

    private void Log(string message)
    {
        _loggingService.LogDebug($"DataMigrationService: {message}");
    }
}
