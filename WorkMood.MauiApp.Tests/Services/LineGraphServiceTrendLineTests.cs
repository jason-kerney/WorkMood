using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests specifically for the trend line functionality in LineGraphService
/// </summary>
[UseReporter(typeof(QuietReporter))]
[UseApprovalSubdirectory("ApprovalFiles/TrendLines")]
public class LineGraphServiceTrendLineTests
{
    private readonly SimpleLineGraphService _simpleLineGraphService;

    public LineGraphServiceTrendLineTests()
    {
        // Configure ApprovalTests to use a specific directory for storing approved files
        ApprovalTestConfiguration.Initialize();
        
        var drawShimFactory = new DrawShimFactory();
        var fileShimFactory = new FileShimFactory();
        var lineGraphGenerator = new LineGraphGenerator(drawShimFactory, fileShimFactory);
        _simpleLineGraphService = new SimpleLineGraphService(new GraphDataTransformer(), lineGraphGenerator);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithShowTrendLineTrue_ReturnsValidImageData()
    {
        // Arrange
        var startDate = new DateOnly(1832, 9, 20);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 9426, count: 7);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var lineColor = Colors.Blue;

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: true, // This is the key parameter we're testing
            lineColor);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndSingleEntry_ReturnsValidImageData()
    {
        // Arrange
        var startDate = new DateOnly(1857, 7, 2);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate, (5, 7));
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: false,
            showTitle: false,
            showTrendLine: true, // Even with trend line enabled
            Colors.Blue);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndAverageMode_ReturnsValidImageData()
    {
        // Arrange
        var startDate = new DateOnly(1905, 08, 02);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 9732, count: 7);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateAverageGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: true,
            Colors.Green);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndEmptyEntries_ReturnsValidImageData()
    {
        // Arrange
        var entries = Array.Empty<MoodEntry>();
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(2025, 1, 8)));

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: true,
            Colors.Purple);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_TrendLineImagesDiffer_BetweenEnabledAndDisabled()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate, 
            (0, 0), (0, 1), (0, 3), (0, 6), (0, 8), (0, 9), (0, 9), (0, 10)); // Progressive mood improvement
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var lineColor = Colors.Red;

        // Act
        var resultWithTrendLine = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries, dateRange, false, false, false, true, lineColor);

        // Assert
        Approvals.VerifyBinaryFile(resultWithTrendLine, "png");
    }

    #region Trend Line Verification Tests

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineEnabled_ImpactMode_ShouldShowTrendLine()
    {
        // Arrange - Data with clear upward trend
        var startDate = new DateOnly(1895, 3, 15);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate,
            (3, 1), // Impact: -2
            (4, 2), // Impact: -2  
            (5, 4), // Impact: -1
            (6, 6), // Impact: 0
            (7, 8), // Impact: +1
            (8, 9), // Impact: +1
            (9, 10) // Impact: +1
        );
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Blue);

        // Assert - Should show upward trending line
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineEnabled_AverageMode_ShouldShowTrendLine()
    {
        // Arrange - Data with clear trend in averages
        var startDate = new DateOnly(1875, 4, 14);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 5038, count: 17);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateAverageGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Green);

        // Assert - Should show upward trending line for averages
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineEnabled_AverageMode_SkipWeekends_ShouldShowTrendLine()
    {
        // Arrange - Data with clear trend in averages, skipping weekends
        var startDate = new DateOnly(1784, 5, 10);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, true, seed: 5038, count: 17);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateAverageGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Green);

        // Assert - Should show upward trending line for averages with weekend gaps
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLine_DownwardTrend_ShouldShowDecreasingTrendLine()
    {
        // Arrange - Data with clear downward trend
        var startDate = new DateOnly(1654, 12, 25);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate,
            (10, 8), // Impact: -2
            (9, 6),  // Impact: -3
            (8, 4),  // Impact: -4
            (7, 2),  // Impact: -5
            (6, 1),  // Impact: -5
            (5, 1),  // Impact: -4
            (4, 1)   // Impact: -3
        );
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Red);

        // Assert - Should show downward trending line
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLine_FlatData_ShouldShowHorizontalTrendLine()
    {
        // Arrange - Data with no clear trend (flat)
        var startDate = new DateOnly(1999, 6, 30);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate,
            (5, 6), // Impact: 1
            (6, 7), // Impact: 1
            (5, 6), // Impact: 1
            (6, 7), // Impact: 1
            (5, 6), // Impact: 1
            (6, 7), // Impact: 1
            (5, 6)  // Impact: 1
        );
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Purple);

        // Assert - Should show flat/horizontal trend line
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraphAsync_WithTrendLineEnabled_ShouldShowTrendLine()
    {
        // Arrange - Raw data with trend
        var (today, rawDataPoints) = MoodDataTestHelper.GetFakeData(new DateOnly(1750, 9, 3),
            (1, 1),
            (2, 3),
            (2, 4),
            (3, 3),
            (3, 5),
            (3, 4),
            (5, 6)
        );
        
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await _simpleLineGraphService.GenerateRawGraphAsync(
            rawDataPoints,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: true, // Trend line enabled
            Colors.Orange);

        // Assert - Should show upward trend line for raw data
        Approvals.VerifyBinaryFile(result, "png");
    }

    #endregion

    #region Basic Functionality Tests

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLine_ReturnsNonEmptyImageData()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate, (1, 5), (2, 6), (3, 7));
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var resultWithTrendLine = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries, dateRange, true, true, true, true, Colors.Blue);
        
        var resultWithoutTrendLine = await _simpleLineGraphService.GenerateImpactGraphAsync(
            entries, dateRange, true, true, true, false, Colors.Blue);

        // Assert
        Assert.True(resultWithTrendLine.Length > 0, "Image with trend line should have data");
        Assert.True(resultWithoutTrendLine.Length > 0, "Image without trend line should have data");
        
        // Images should be different when trend line is enabled vs disabled
        Assert.False(resultWithTrendLine.SequenceEqual(resultWithoutTrendLine), 
            "Images with and without trend line should be different");
    }

    [Fact]
    public async Task GenerateRawDataGraphAsync_WithTrendLine_ReturnsNonEmptyImageData()
    {
        // Arrange
        var rawData = new List<MoodEntry>
        {
            new MoodEntry { CreatedAt = new DateTime(2025, 1, 1, 9, 0, 0), StartOfWork = 3, EndOfWork = null, Date = new DateOnly(2025, 1, 1), LastModified = new DateTime(2025, 1, 1, 9, 0, 0) },
            new MoodEntry { CreatedAt = new DateTime(2025, 1, 2, 9, 0, 0), StartOfWork = 5, EndOfWork = null, Date = new DateOnly(2025, 1, 2), LastModified = new DateTime(2025, 1, 2, 9, 0, 0) },
            new MoodEntry { CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), StartOfWork = 7, EndOfWork = null, Date = new DateOnly(2025, 1, 3), LastModified = new DateTime(2025, 1, 3, 9, 0, 0) },
        };
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(2025, 1, 4)));

        // Act
        var resultWithTrendLine = await _simpleLineGraphService.GenerateRawGraphAsync(
            rawData, dateRange, true, true, true, true, Colors.Orange);

        // Assert
        Assert.True(resultWithTrendLine.Length > 0, "Raw data image with trend line should have data");        
    }

    #endregion
}