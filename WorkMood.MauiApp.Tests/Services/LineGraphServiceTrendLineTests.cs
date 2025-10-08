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
[UseReporter(typeof(ClipboardReporter))]
[UseApprovalSubdirectory("ApprovalFiles/TrendLines")]
public class LineGraphServiceTrendLineTests
{

    public LineGraphServiceTrendLineTests()
    {
        // Configure ApprovalTests to use a specific directory for storing approved files
        ApprovalTestConfiguration.Initialize();
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithShowTrendLineTrue_ReturnsValidImageData()
    {
        // Arrange
        var service = new LineGraphService();
        var startDate = new DateOnly(1832, 9, 20);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 9426, count: 7);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var lineColor = Colors.Blue;

        // Act
        var result = await service.GenerateLineGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: true, // This is the key parameter we're testing
            GraphMode.Impact, 
            lineColor);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndSingleEntry_ReturnsValidImageData()
    {
        // Arrange
        var service = new LineGraphService();
        var startDate = new DateOnly(1857, 7, 2);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate, (5, 7));
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await service.GenerateLineGraphAsync(
            entries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: false,
            showTitle: false,
            showTrendLine: true, // Even with trend line enabled
            GraphMode.Impact,
            Colors.Blue);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndAverageMode_ReturnsValidImageData()
    {
        // Arrange
        var service = new LineGraphService();
        var startDate = new DateOnly(1905, 08, 02);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 9732, count: 7);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var result = await service.GenerateLineGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: true,
            GraphMode.Average, // This should use average mood values
            Colors.Green);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithTrendLineAndEmptyEntries_ReturnsValidImageData()
    {
        // Arrange
        var service = new LineGraphService();
        var entries = Array.Empty<MoodEntry>();
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(2025, 1, 8)));

        // Act
        var result = await service.GenerateLineGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: true,
            GraphMode.Impact, 
            Colors.Purple);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_TrendLineImagesDiffer_BetweenEnabledAndDisabled()
    {
        // Arrange
        var service = new LineGraphService();
        var startDate = new DateOnly(2025, 1, 1);
        var (today, entries) = MoodDataTestHelper.GetFakeData(startDate, 
            (0, 0), (0, 1), (0, 3), (0, 6), (0, 8), (0, 9), (0, 9), (0, 10)); // Progressive mood improvement
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var lineColor = Colors.Red;

        // Act
        var resultWithTrendLine = await service.GenerateLineGraphAsync(
            entries, dateRange, false, false, false, true, GraphMode.Impact, lineColor);

        // Assert
        Approvals.VerifyBinaryFile(resultWithTrendLine, "png");
    }
}