using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Comprehensive edge case tests for LineGraphService
/// </summary>
[UseReporter(typeof(ClipboardReporter))]
[UseApprovalSubdirectory("ApprovalFiles")]
public class LineGraphServiceEdgeCaseTests
{
    private readonly LineGraphService _lineGraphService;

    public LineGraphServiceEdgeCaseTests()
    {
        var drawShimFactory = new DrawShimFactory();
        var fileShimFactory = new FileShimFactory();
        _lineGraphService = new LineGraphService(drawShimFactory, fileShimFactory);
        
        ApprovalTestConfiguration.Initialize();
    }

    #region Data Sparsity Tests

    [Fact]
    public async Task GenerateLineGraph_VerySparseMoodData_ShouldMatchApproval()
    {
        // Arrange - Only one entry in a 30-day range
        var (today, sparseMoodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 15), seed: 6734, count: 5);
        var wideRange = new DateRangeInfo(DateRange.LastMonth, new FakeDateShim(today.AddDays(5)));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            sparseMoodEntries, 
            wideRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            Microsoft.Maui.Graphics.Colors.Blue);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_DenseMoodData_ShouldMatchApproval()
    {
        // Arrange - Daily entries for month
        var (today, denseMoodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1935, 10, 18), true, seed: 3171, count: 31);
        var dateRange = new DateRangeInfo(DateRange.LastMonth, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            denseMoodEntries, 
            dateRange, 
            showDataPoints: false, // Hide individual points due to density
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            Microsoft.Maui.Graphics.Colors.Purple);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task GenerateLineGraph_AllMinimumValues_ShouldMatchApproval()
    {
        // Arrange - All mood entries at minimum values (1)
        var (today, minMoodEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(2025, 1, 3), (1, 1), (1, 2), (1, 3), (1, 1), (1, 1), (1, 2), (2, 1), (1, 1));
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            minMoodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Average, 
            Microsoft.Maui.Graphics.Colors.Red);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_AllMaximumValues_ShouldMatchApproval()
    {
        // Arrange - All mood entries at maximum values (10)
        var (today, maxMoodEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(1869, 11, 3), (10, 10), (10, 10), (10, 10), (10, 10), (10, 10), (10, 10), (10, 10), (10, 10));
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            maxMoodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Average, 
            Microsoft.Maui.Graphics.Colors.Green);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Timestamp Edge Cases

    [Fact]
    public async Task GenerateRawDataGraph_SameTimestampEntries_ShouldMatchApproval()
    {
        // Arrange - Multiple entries with identical timestamps (edge case)
        var sameTimeEntries = new List<RawMoodDataPoint>
        {
            new(new DateTime(2025, 1, 1, 8, 0, 0), 5, MoodType.StartOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 1, 8, 0, 0), 7, MoodType.EndOfWork, new DateOnly(2025, 1, 1)), // Same timestamp
            new(new DateTime(2025, 1, 1, 17, 0, 0), 6, MoodType.StartOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 1, 17, 0, 0), 4, MoodType.EndOfWork, new DateOnly(2025, 1, 1)) // Same timestamp
        };
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(2025, 1, 2)));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            sameTimeEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            Microsoft.Maui.Graphics.Colors.Orange);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_MidnightTimestamps_ShouldMatchApproval()
    {
        // Arrange - Entries at edge times (midnight)
        var midnightEntries = new List<RawMoodDataPoint>
        {
            new(new DateTime(2025, 1, 1, 0, 0, 0), 5, MoodType.StartOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 1, 23, 59, 59), 7, MoodType.EndOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 2, 0, 0, 1), 6, MoodType.StartOfWork, new DateOnly(2025, 1, 2)),
            new(new DateTime(2025, 1, 2, 23, 58, 0), 4, MoodType.EndOfWork, new DateOnly(2025, 1, 2))
        };
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(2025, 1, 4)));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            midnightEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            Microsoft.Maui.Graphics.Colors.Magenta);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Date Range Edge Cases

    [Fact]
    public async Task GenerateLineGraph_SingleDayRange_ShouldMatchApproval()
    {
        // Arrange - Date range spanning only one day
        var (today, singleDayEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1888, 4, 4), seed: 7332, count: 1);
        var singleDayRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            singleDayEntries, 
            singleDayRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            Microsoft.Maui.Graphics.Colors.Teal);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_DataOutsideRange_ShouldMatchApproval()
    {
        // Arrange - Mood entries that fall outside the requested date range
        var (today,outsideRangeEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1848, 5, 8), seed: 2317, count: 90);
        var limitedRange = new DateRangeInfo(DateRange.LastMonth, new FakeDateShim(today.AddDays(-30)));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            outsideRangeEntries, 
            limitedRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            Microsoft.Maui.Graphics.Colors.Brown);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Null Value Handling Tests

    // [Fact]
    // public async Task GenerateLineGraph_MixedNullValues_ImpactMode_ShouldMatchApproval()
    // {
    //     // Arrange - Mix of entries with null EndOfWork values
    //     var mixedNullEntries = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 6, EndOfWork = null, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 8, 30, 0) }, // Null end
    //         new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 3, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 4), StartOfWork = null, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 4, 9, 0, 0), LastModified = new DateTime(2025, 1, 4, 18, 0, 0) }, // Null start - should be filtered out in Impact mode
    //         new() { Date = new DateOnly(2025, 1, 5), StartOfWork = 7, EndOfWork = null, CreatedAt = new DateTime(2025, 1, 5, 8, 15, 0), LastModified = new DateTime(2025, 1, 5, 8, 15, 0) } // Null end
    //     };
    //     var dateRange = DateRange.Last7Days;

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         mixedNullEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         Microsoft.Maui.Graphics.Colors.Navy);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion

    #region Performance Stress Tests

    // [Fact]
    // public async Task GenerateLineGraph_LargeDataset_ShouldMatchApproval()
    // {
    //     // Arrange - Generate a year's worth of data
    //     var largeMoodEntries = new List<MoodEntry>();
    //     var startDate = new DateOnly(2025, 1, 1);
        
    //     for (int i = 0; i < 365; i++)
    //     {
    //         var date = startDate.AddDays(i);
    //         // Create varying but predictable mood patterns
    //         var dayOfYear = i + 1;
    //         var startMood = 5 + (int)(Math.Sin(dayOfYear * 0.1) * 2); // Seasonal variation
    //         var endMood = startMood + (int)(Math.Cos(dayOfYear * 0.05) * 3); // Daily variation
    //         startMood = Math.Clamp(startMood, 1, 10);
    //         endMood = Math.Clamp(endMood, 1, 10);
            
    //         largeMoodEntries.Add(new MoodEntry 
    //         { 
    //             Date = date, 
    //             StartOfWork = startMood, 
    //             EndOfWork = endMood,
    //             CreatedAt = date.ToDateTime(new TimeOnly(8, 0, 0)),
    //             LastModified = date.ToDateTime(new TimeOnly(17, 0, 0))
    //         });
    //     }
        
    //     var yearRange = DateRange.LastYear;

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         largeMoodEntries, 
    //         yearRange, 
    //         showDataPoints: false, // Too many points to show individually
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         Microsoft.Maui.Graphics.Colors.DarkGreen,
    //         width: 1600,  // Larger canvas to accommodate more data
    //         height: 800);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion

    #region Zero Impact Tests

    // [Fact]
    // public async Task GenerateLineGraph_AllZeroImpact_ShouldMatchApproval()
    // {
    //     // Arrange - All entries have identical start and end values (zero impact)
    //     var zeroImpactEntries = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 5, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 7, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
    //         new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 3, EndOfWork = 3, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 4), StartOfWork = 9, EndOfWork = 9, CreatedAt = new DateTime(2025, 1, 4, 8, 15, 0), LastModified = new DateTime(2025, 1, 4, 17, 45, 0) }
    //     };
    //     var dateRange = DateRange.Last7Days;

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         zeroImpactEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         Microsoft.Maui.Graphics.Colors.Gray);

    //     // Assert - Should show a flat line at y=0
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion
}