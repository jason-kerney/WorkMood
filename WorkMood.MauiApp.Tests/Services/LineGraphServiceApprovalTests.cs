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
[UseReporter(typeof(ClipboardReporter))]
[UseApprovalSubdirectory("ApprovalFiles")]
public class LineGraphServiceApprovalTests
{
    private readonly LineGraphService _lineGraphService;
    private readonly IDrawShimFactory _drawShimFactory;
    private readonly IFileShimFactory _fileShimFactory;

    public LineGraphServiceApprovalTests()
    {
        _drawShimFactory = new DrawShimFactory();
        _fileShimFactory = new FileShimFactory();
        _lineGraphService = new LineGraphService(_drawShimFactory, _fileShimFactory);
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
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
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
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false, 
            GraphMode.Impact, 
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
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            Microsoft.Maui.Graphics.Colors.Red);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Average Mode Tests

    // [Fact]
    // public async Task GenerateLineGraph_AverageMode_WithDataPointsAndGrid_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var moodEntries = CreateStandardMoodEntries();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         moodEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Average, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateLineGraph_AverageMode_StartOnlyEntries_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var moodEntries = CreateStartOnlyMoodEntries();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         moodEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Average, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // #endregion

    // #region Raw Data Mode Tests

    // [Fact]
    // public async Task GenerateRawDataGraph_WithDataPointsAndGrid_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var rawDataPoints = CreateStandardRawMoodData();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
    //         rawDataPoints, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateRawDataGraph_LineOnly_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var rawDataPoints = CreateStandardRawMoodData();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
    //         rawDataPoints, 
    //         dateRange, 
    //         showDataPoints: false, 
    //         showAxesAndGrid: false, 
    //         showTitle: false, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion

    #region Edge Case Tests

    // [Fact]
    // public async Task GenerateLineGraph_EmptyData_ImpactMode_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var emptyEntries = new List<MoodEntry>();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         emptyEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateLineGraph_SingleDataPoint_ImpactMode_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var singleEntry = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 5, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
    //     };
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         singleEntry, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateRawDataGraph_EmptyData_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var emptyData = new List<RawMoodDataPoint>();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
    //         emptyData, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateLineGraph_SmallDateRange_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var moodEntries = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 6, EndOfWork = 4, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) }
    //     };
    //     var dateRange = CreateStandardDateRange(); // Using Last7Days range

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         moodEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion

    #region Dimension Tests

    // [Fact]
    // public async Task GenerateLineGraph_LargeSize_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var moodEntries = CreateStandardMoodEntries();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         moodEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor,
    //         width: 1200, 
    //         height: 800);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateLineGraph_SmallSize_ShouldMatchApproval()
    // {
    //     // Arrange
    //     var moodEntries = CreateStandardMoodEntries();
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         moodEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor,
    //         width: 400, 
    //         height: 300);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion

    #region Extreme Value Tests

    // [Fact]
    // public async Task GenerateLineGraph_ExtremePositiveValues_ShouldMatchApproval()
    // {
    //     // Arrange - Create entries with maximum positive impact
    //     var extremeEntries = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 1, EndOfWork = 10, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 2, EndOfWork = 10, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
    //         new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 1, EndOfWork = 9, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
    //     };
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         extremeEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    // [Fact]
    // public async Task GenerateLineGraph_ExtremeNegativeValues_ShouldMatchApproval()
    // {
    //     // Arrange - Create entries with maximum negative impact
    //     var extremeEntries = new List<MoodEntry>
    //     {
    //         new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 10, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
    //         new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 9, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
    //         new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 8, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
    //     };
    //     var dateRange = CreateStandardDateRange();

    //     // Act
    //     var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
    //         extremeEntries, 
    //         dateRange, 
    //         showDataPoints: true, 
    //         showAxesAndGrid: true, 
    //         showTitle: true, 
    //         GraphMode.Impact, 
    //         StandardLineColor);

    //     // Assert
    //     Approvals.VerifyBinaryFile(imageBytes, "png");
    // }

    #endregion
}