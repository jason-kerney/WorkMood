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
[UseReporter(typeof(QuietReporter))]
[UseApprovalSubdirectory("ApprovalFiles/EdgeCases")]
public class LineGraphServiceEdgeCaseTests
{
    private readonly LineGraphService _lineGraphService;
    private readonly SimpleLineGraphService _simpleLineGraphService;

    public LineGraphServiceEdgeCaseTests()
    {
        var drawShimFactory = new DrawShimFactory();
        var fileShimFactory = new FileShimFactory();
        var lineGraphGenerator = new LineGraphGenerator(drawShimFactory, fileShimFactory);
        _lineGraphService = new LineGraphService(drawShimFactory, fileShimFactory, lineGraphGenerator: lineGraphGenerator);
        _simpleLineGraphService = new SimpleLineGraphService(new GraphDataTransformer(), lineGraphGenerator);
        
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
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            sparseMoodEntries, 
            wideRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
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
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            denseMoodEntries, 
            dateRange, 
            showDataPoints: false, // Hide individual points due to density
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
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
        var imageBytes = await _simpleLineGraphService.GenerateAverageGraphAsync(
            minMoodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
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
        var imageBytes = await _simpleLineGraphService.GenerateAverageGraphAsync(
            maxMoodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
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
            showTrendLine: false,
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
            showTrendLine: false,
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
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            singleDayEntries, 
            singleDayRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
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
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            outsideRangeEntries,
            limitedRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: false,
            Microsoft.Maui.Graphics.Colors.Brown);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Null Value Handling Tests

    [Fact]
    public async Task GenerateLineGraph_MixedNullValues_ImpactMode_ShouldMatchApproval()
    {
        // Arrange - Mix of entries with null EndOfWork values
        var (today, mixedNullEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(2025, 1, 1), 
            (5, 7), 
            (6, null), // Null end
            (3, 8), 
            (null, 8), // Null start - should be filtered out in Impact mode
            (7, null)  // Null end
        );
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            mixedNullEntries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: false,
            Microsoft.Maui.Graphics.Colors.Navy);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Performance Stress Tests

    [Fact]
    public async Task GenerateLineGraph_LastYearDataset_ShouldMatchApproval()
    {
        // Arrange - Generate a year's worth of data
        var (today, largeMoodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 1), seed: 1234, count: 365);
        var yearRange = new DateRangeInfo(DateRange.LastYear, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            largeMoodEntries,
            yearRange,
            showDataPoints: false, // Too many points to show individually
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: false,
            Microsoft.Maui.Graphics.Colors.DarkGreen,
            width: 1600, // Larger canvas to accommodate more data
            height: 800);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Zero Impact Tests

    [Fact]
    public async Task GenerateLineGraph_AllZeroImpact_ShouldMatchApproval()
    {
        // Arrange - All entries have identical start and end values (zero impact)
        var (today, zeroImpactEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(2025, 1, 1), 
            (5, 5), 
            (7, 7), 
            (3, 3), 
            (9, 9),
            (2, 2),
            (1, 1),
            (1, 1),
            (9, 9)
        );
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            zeroImpactEntries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line should be flat
            GraphMode.Impact,
            Microsoft.Maui.Graphics.Colors.Green);

        // Assert - Should show a flat line at y=0
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion
}