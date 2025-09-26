using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Tests;

/// <summary>
/// Test helper for creating year boundary test data to verify the archived data loading functionality
/// </summary>
public static class YearBoundaryTestDataCreator
{
    /// <summary>
    /// Creates test data that spans year boundaries to test the archive-aware history functionality
    /// </summary>
    /// <param name="dataService">The mood data service to use for testing</param>
    /// <returns>Task</returns>
    public static async Task CreateYearBoundaryTestDataAsync(MoodDataService dataService)
    {
        Console.WriteLine("Creating year boundary test data...");

        var testEntries = new List<MoodEntryOld>();

        // Get current date info
        var today = DateTime.Today;
        var currentYear = today.Year;
        
        Console.WriteLine($"Current date: {today:yyyy-MM-dd}");
        Console.WriteLine($"Current year: {currentYear}");

        // Create data from 4 years ago that should be archived
        var oldYear = currentYear - 4;
        var oldStartDate = new DateOnly(oldYear, 12, 1); // December of old year
        
        for (int i = 0; i < 31; i++) // Full month of December
        {
            var entryDate = oldStartDate.AddDays(i);
            if (entryDate.Month == 12) // Only December entries
            {
                var entry = new MoodEntryOld(entryDate)
                {
                    MorningMood = Random.Shared.Next(3, 8), // Reasonable mood range
                    EveningMood = Random.Shared.Next(3, 8)
                };
                testEntries.Add(entry);
            }
        }

        // Create some data from January of the following year (still old, should be archived)
        var nextOldYear = oldYear + 1;
        var nextOldStartDate = new DateOnly(nextOldYear, 1, 1);
        
        for (int i = 0; i < 31; i++) // Full month of January
        {
            var entryDate = nextOldStartDate.AddDays(i);
            if (entryDate.Month == 1) // Only January entries
            {
                var entry = new MoodEntryOld(entryDate)
                {
                    MorningMood = Random.Shared.Next(4, 9), // Slightly different range
                    EveningMood = Random.Shared.Next(4, 9)
                };
                testEntries.Add(entry);
            }
        }

        // Create sparse data in the middle years (should remain in active file but not much data)
        var middleYear = currentYear - 1;
        for (int i = 0; i < 5; i++) // Only 5 entries scattered through the year
        {
            var randomMonth = Random.Shared.Next(1, 13);
            var randomDay = Random.Shared.Next(1, 28); // Safe day range
            var entryDate = new DateOnly(middleYear, randomMonth, randomDay);
            
            var entry = new MoodEntryOld(entryDate)
            {
                MorningMood = Random.Shared.Next(5, 10),
                EveningMood = Random.Shared.Next(5, 10)
            };
            testEntries.Add(entry);
        }

        // Create recent data, but simulate being near year boundary
        // If we're near end of year, create data leading up to now
        // If we're near beginning of year, create data from end of last year
        var isNearEndOfYear = today.Month >= 11; // November or December
        var isNearStartOfYear = today.Month <= 2; // January or February
        
        if (isNearEndOfYear)
        {
            Console.WriteLine("Simulating near end of year scenario");
            // Create data from November onwards
            var recentStartDate = new DateOnly(currentYear, 11, 1);
            for (int i = 0; i < 10; i++) // Only 10 entries, less than requested
            {
                var entryDate = recentStartDate.AddDays(i * 3); // Every 3 days
                if (entryDate <= DateOnly.FromDateTime(today))
                {
                    var entry = new MoodEntryOld(entryDate)
                    {
                        MorningMood = Random.Shared.Next(6, 10),
                        EveningMood = Random.Shared.Next(6, 10)
                    };
                    testEntries.Add(entry);
                }
            }
        }
        else if (isNearStartOfYear)
        {
            Console.WriteLine("Simulating near start of year scenario");
            // Create sparse data from start of current year
            var recentStartDate = new DateOnly(currentYear, 1, 1);
            for (int i = 0; i < 8; i++) // Only 8 entries, less than requested
            {
                var entryDate = recentStartDate.AddDays(i * 4); // Every 4 days
                if (entryDate <= DateOnly.FromDateTime(today))
                {
                    var entry = new MoodEntryOld(entryDate)
                    {
                        MorningMood = Random.Shared.Next(6, 10),
                        EveningMood = Random.Shared.Next(6, 10)
                    };
                    testEntries.Add(entry);
                }
            }
            
            // Add some entries from end of previous year (should be in active file)
            var prevYearEnd = new DateOnly(currentYear - 1, 12, 15);
            for (int i = 0; i < 15; i++) // 15 more entries from end of previous year
            {
                var entryDate = prevYearEnd.AddDays(i);
                if (entryDate.Year == currentYear - 1) // Only previous year
                {
                    var entry = new MoodEntryOld(entryDate)
                    {
                        MorningMood = Random.Shared.Next(5, 9),
                        EveningMood = Random.Shared.Next(5, 9)
                    };
                    testEntries.Add(entry);
                }
            }
        }
        else
        {
            Console.WriteLine("Not near year boundary, creating normal recent data");
            // Create normal recent data
            for (int i = 0; i < 15; i++)
            {
                var entryDate = DateOnly.FromDateTime(today.AddDays(-i));
                var entry = new MoodEntryOld(entryDate)
                {
                    MorningMood = Random.Shared.Next(5, 10),
                    EveningMood = Random.Shared.Next(5, 10)
                };
                testEntries.Add(entry);
            }
        }

        // Create and save the collection
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
    /// Tests the archive-aware recent entries functionality
    /// </summary>
    /// <param name="dataService">The mood data service to test</param>
    public static async Task TestArchiveAwareHistoryAsync(MoodDataService dataService)
    {
        Console.WriteLine("\n=== Testing Archive-Aware History Loading ===");
        
        try
        {
            var archiveService = new DataArchiveService();
            
            // Check if we're near a year transition
            var nearTransition = archiveService.IsNearYearTransition(14);
            Console.WriteLine($"Near year transition (14 days): {nearTransition}");
            
            // Test regular recent entries
            Console.WriteLine("\nTesting regular GetRecentMoodEntriesAsync(10):");
            var regularEntries = await dataService.GetRecentMoodEntriesAsync(10);
            Console.WriteLine($"Regular method returned: {regularEntries.Count()} entries");
            
            foreach (var entry in regularEntries.OrderByDescending(e => e.Date))
            {
                Console.WriteLine($"  {entry.Date}: Morning={entry.MorningMood}, Evening={entry.EveningMood}");
            }
            
            // Test archive-aware recent entries
            Console.WriteLine("\nTesting archive-aware GetRecentMoodEntriesWithArchiveAsync(10):");
            var archiveAwareEntries = await dataService.GetRecentMoodEntriesWithArchiveAsync(10);
            Console.WriteLine($"Archive-aware method returned: {archiveAwareEntries.Count()} entries");
            
            foreach (var entry in archiveAwareEntries.OrderByDescending(e => e.Date))
            {
                Console.WriteLine($"  {entry.Date}: Morning={entry.MorningMood}, Evening={entry.EveningMood}");
            }
            
            // Compare results
            if (archiveAwareEntries.Count() > regularEntries.Count())
            {
                Console.WriteLine($"\n✅ Archive-aware method found {archiveAwareEntries.Count() - regularEntries.Count()} additional entries from archives!");
            }
            else if (archiveAwareEntries.Count() == regularEntries.Count())
            {
                Console.WriteLine("\n✅ Both methods returned the same number of entries (expected when not near year transition or when there's sufficient recent data)");
            }
            
            Console.WriteLine("\n=== Archive-Aware History Test Completed ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}