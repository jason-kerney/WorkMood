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
/// Approval tests for LineGraphService to verify visual output of generated graphs
/// </summary>
[UseReporter(typeof(QuietReporter))]
[UseApprovalSubdirectory("ApprovalFiles/LineGraphs")]
public class LineGraphServiceApprovalTests
{
    private readonly LineGraphService _lineGraphService;
    private readonly SimpleLineGraphService _simpleLineGraphService;
    private readonly IDrawShimFactory _drawShimFactory;
    private readonly IFileShimFactory _fileShimFactory;

    public LineGraphServiceApprovalTests()
    {
        _drawShimFactory = new DrawShimFactory();
        _fileShimFactory = new FileShimFactory();
        var lineGraphGenerator = new LineGraphGenerator(_drawShimFactory, _fileShimFactory);
        _lineGraphService = new LineGraphService(_drawShimFactory, _fileShimFactory, lineGraphGenerator: lineGraphGenerator);
        _simpleLineGraphService = new SimpleLineGraphService(new GraphDataTransformer(), lineGraphGenerator);
    }

    #region Test Data Helpers
    /// <summary>
    /// Standard colors for testing
    /// </summary>
    private static readonly Microsoft.Maui.Graphics.Color StandardLineColor = Microsoft.Maui.Graphics.Colors.Blue;

    #endregion

    #region Impact Mode Tests

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 1), seed: 123, count: 30);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 800,
            height: 600);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_Skip_Weekends_at_end_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-7), true, seed: 123, count: 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 800, 
            height: 600);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_Skip_Weekends_at_start_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-9), true, seed: 123, count: 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 800, 
            height: 600);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_Skip_Weekends_in_middle_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-12), true, seed: 123, count: 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 800, 
            height: 600);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_Skip_Weekends_on_both_sides_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-8), true, seed: 123, count: 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 800, 
            height: 600);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_LineOnly_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 1), seed: 52689, count: 30);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithRedLineColor_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1995, 6, 12), 8677, 20);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            Microsoft.Maui.Graphics.Colors.Red);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Average Mode Tests

    [Fact]
    public async Task GenerateLineGraph_AverageMode_WithDataPointsAndGrid_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(4411, 6, 12), 8677, 20);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateAverageGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_AverageMode_StartOnlyEntries_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2031, 7, 8), 7023, 20);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateAverageGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Raw Data Mode Tests

    [Fact]
    public async Task GenerateRawDataGraph_WithDataPointsAndGrid_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1970, 2, 22), 5815, 20);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        // var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
        var imageBytes = await _simpleLineGraphService.GenerateRawGraphAsync(
            data,
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_WithDataPointsAndGrid_skip_weekends_at_both_sides_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-8), true, 5815, 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateRawGraphAsync(
            data,
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_WithDataPointsAndGrid_skip_weekends_at_beginning_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-9), true, 6082, 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data),
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_WithDataPointsAndGrid_skip_weekends_at_end_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 10, 6).AddDays(-7), true, 6082, 7);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data),
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_LineOnly_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 1), seed: 3767, count: 30);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data),
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GenerateLineGraph_EmptyData_ImpactMode_ShouldMatchApproval()
    {
        // Arrange
        var emptyEntries = new List<MoodEntry>();
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(1925, 1, 15)));

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            emptyEntries,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: true,
            showTrendLine: false,
            GraphMode.Impact,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_SingleDataPoint_ImpactMode_ShouldMatchApproval()
    {
        // Arrange
        var (today, singleEntry) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 3), 5106, 1);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            singleEntry, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_EmptyData_ShouldMatchApproval()
    {
        // Arrange
        var emptyData = new List<RawMoodDataPoint>();
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(new DateOnly(1999, 12, 9)));

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            emptyData, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true,
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_SmallDateRange_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 9, 13), seed: 1299, count: 2);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Dimension Tests

    [Fact]
    public async Task GenerateLineGraph_LargeSize_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1778, 1, 3), 95, 10);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 1200, 
            height: 800);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_SmallSize_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2000, 12, 31), seed: 97, count: 30);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor,
            width: 400, 
            height: 300);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Extreme Value Tests

    [Fact]
    public async Task GenerateLineGraph_ExtremePositiveValues_ShouldMatchApproval()
    {
        // Arrange - Create entries with maximum positive impact
        var (today, extremeEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(1796, 02, 25), [(1, 10), (10, 2), (9, 1), (1, 10)]);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            extremeEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ExtremeNegativeValues_ShouldMatchApproval()
    {
        // Arrange - Create entries with maximum negative impact
        var (today, extremeEntries) = MoodDataTestHelper.GetFakeData(new DateOnly(1825, 02, 28), [(10, 1), (10, 1), (9, 1), (1, 9), (1, 8), (8, 1)]);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));

        // Act
        var imageBytes = await _simpleLineGraphService.GenerateImpactGraphAsync(
            extremeEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion
}