using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Tests;

/// <summary>
/// Test helper for creating test data to verify the archiving functionality
/// </summary>
public static class TestDataCreator
{
    /// <summary>
    /// Creates test mood data spanning multiple years to test the archiving functionality
    /// </summary>
    /// <param name="dataService">The mood data service to use for testing</param>
    /// <returns>Task</returns>
    public static async Task CreateTestDataAsync(MoodDataService dataService)
    {
        Console.WriteLine("Creating test data spanning multiple years...");

        var testEntries = new List<MoodEntryOld>();

        // Create data from 4 years ago (should trigger archiving)
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-4));
        
        // Add entries for old data (4 years ago)
        for (int i = 0; i < 30; i++)
        {
            var entryDate = startDate.AddDays(i);
            var entry = new MoodEntryOld(entryDate)
            {
                MorningMood = Random.Shared.Next(1, 11),
                EveningMood = Random.Shared.Next(1, 11)
            };
            testEntries.Add(entry);
        }

        // Add some data from 2 years ago (should remain in active file)
        var recentStartDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-2));
        for (int i = 0; i < 20; i++)
        {
            var entryDate = recentStartDate.AddDays(i);
            var entry = new MoodEntryOld(entryDate)
            {
                MorningMood = Random.Shared.Next(1, 11),
                EveningMood = Random.Shared.Next(1, 11)
            };
            testEntries.Add(entry);
        }

        // Add recent data (last month)
        var veryRecentStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        for (int i = 0; i < 15; i++)
        {
            var entryDate = veryRecentStartDate.AddDays(i);
            var entry = new MoodEntryOld(entryDate)
            {
                MorningMood = Random.Shared.Next(1, 11),
                EveningMood = Random.Shared.Next(1, 11)
            };
            testEntries.Add(entry);
        }

        // Create a collection and save it (this should trigger archiving)
        var collection = new MoodCollection(testEntries);
        
        Console.WriteLine($"Test data created: {testEntries.Count} total entries");
        Console.WriteLine($"Oldest entry: {testEntries.Min(e => e.Date)}");
        Console.WriteLine($"Newest entry: {testEntries.Max(e => e.Date)}");
        
        // Save the collection - this should trigger archiving of old data
        Console.WriteLine("Saving collection (this should trigger archiving)...");
        await dataService.SaveMoodDataAsync(collection);
        
        // Load the data back to see what remains in the active file
        var loadedCollection = await dataService.LoadMoodDataAsync();
        Console.WriteLine($"After archiving, active file contains: {loadedCollection.Entries.Count} entries");
        
        if (loadedCollection.Entries.Any())
        {
            Console.WriteLine($"Oldest remaining entry: {loadedCollection.Entries.Min(e => e.Date)}");
            Console.WriteLine($"Newest remaining entry: {loadedCollection.Entries.Max(e => e.Date)}");
        }
    }

    /// <summary>
    /// Verifies that the archiving process worked correctly
    /// </summary>
    /// <param name="dataService">The mood data service to check</param>
    public static async Task VerifyArchivingAsync(MoodDataService dataService)
    {
        Console.WriteLine("\n=== Verifying Archiving Results ===");
        
        var archiveService = new DataArchiveService();
        var collection = await dataService.LoadMoodDataAsync();
        
        Console.WriteLine($"Current active entries: {collection.Entries.Count}");
        
        var oldestAge = archiveService.GetOldestEntryAge(collection);
        Console.WriteLine($"Oldest entry age: {oldestAge?.ToString("F2") ?? "N/A"} years");
        
        var shouldArchive = archiveService.ShouldArchive(collection, 3);
        Console.WriteLine($"Should archive more data: {shouldArchive}");
        
        // Check if archive files were created
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var archiveDirectory = Path.Combine(appDataPath, "WorkMood", "archives");
        
        if (Directory.Exists(archiveDirectory))
        {
            var archiveFiles = Directory.GetFiles(archiveDirectory, "mood_data_archive_*.json");
            Console.WriteLine($"Archive files created: {archiveFiles.Length}");
            
            foreach (var file in archiveFiles)
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"  {fileName} ({fileInfo.Length} bytes, created: {fileInfo.CreationTime})");
            }
        }
        else
        {
            Console.WriteLine("No archive directory found");
        }
    }
}