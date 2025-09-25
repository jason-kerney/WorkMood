using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Tests;

/// <summary>
/// Simple console program to test the data archiving functionality
/// This can be called from a debug method or unit test
/// </summary>
public static class ArchivingTestRunner
{
    /// <summary>
    /// Runs the archiving test sequence
    /// </summary>
    public static async Task RunTestAsync()
    {
        Console.WriteLine("=== WorkMood Data Archiving Test ===\n");
        
        try
        {
            // Create the services
            var archiveService = new DataArchiveService();
            var dataService = new MoodDataService(archiveService);
            
            Console.WriteLine("Services created successfully.");
            
            // Clear any existing data for clean test
            var existingCollection = await dataService.LoadMoodDataAsync();
            Console.WriteLine($"Existing data entries: {existingCollection.Entries.Count}");
            
            // Create test data
            await TestDataCreator.CreateTestDataAsync(dataService);
            
            // Verify the results
            await TestDataCreator.VerifyArchivingAsync(dataService);
            
            Console.WriteLine("\n=== Test completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Runs the year boundary archiving test sequence
    /// </summary>
    public static async Task RunYearBoundaryTestAsync()
    {
        Console.WriteLine("=== WorkMood Year Boundary Archive Test ===\n");
        
        try
        {
            // Create the services
            var archiveService = new DataArchiveService();
            var dataService = new MoodDataService(archiveService);
            
            Console.WriteLine("Services created successfully.");
            
            // Clear any existing data for clean test
            var existingCollection = await dataService.LoadMoodDataAsync();
            Console.WriteLine($"Existing data entries: {existingCollection.Entries.Count}");
            
            // Create year boundary test data
            await YearBoundaryTestDataCreator.CreateYearBoundaryTestDataAsync(dataService);
            
            // Test the archive-aware history loading
            await YearBoundaryTestDataCreator.TestArchiveAwareHistoryAsync(dataService);
            
            Console.WriteLine("\n=== Year Boundary Test completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}