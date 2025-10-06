using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
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
    /// Creates a standard set of mood entries for testing
    /// </summary>
    private static List<MoodEntry> CreateStandardMoodEntries()
    {
        return new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 6, EndOfWork = 4, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 3, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 5), StartOfWork = 7, EndOfWork = 6, CreatedAt = new DateTime(2025, 1, 5, 8, 15, 0), LastModified = new DateTime(2025, 1, 5, 17, 45, 0) },
            new() { Date = new DateOnly(2025, 1, 7), StartOfWork = 4, EndOfWork = 9, CreatedAt = new DateTime(2025, 1, 7, 7, 45, 0), LastModified = new DateTime(2025, 1, 7, 16, 15, 0) }
        };
    }

    /// <summary>
    /// Creates mood entries with only start-of-work values (no end-of-work)
    /// </summary>
    private static List<MoodEntry> CreateStartOnlyMoodEntries()
    {
        return new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = null, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 8, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 8, EndOfWork = null, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 8, 30, 0) },
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 3, EndOfWork = null, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 9, 0, 0) }
        };
    }

    /// <summary>
    /// Creates raw mood data points for testing RawData graph mode
    /// </summary>
    private static List<RawMoodDataPoint> CreateStandardRawMoodData()
    {
        return new List<RawMoodDataPoint>
        {
            new(new DateTime(2025, 1, 1, 8, 0, 0), 5, MoodType.StartOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 1, 17, 0, 0), 7, MoodType.EndOfWork, new DateOnly(2025, 1, 1)),
            new(new DateTime(2025, 1, 2, 8, 30, 0), 6, MoodType.StartOfWork, new DateOnly(2025, 1, 2)),
            new(new DateTime(2025, 1, 2, 16, 30, 0), 4, MoodType.EndOfWork, new DateOnly(2025, 1, 2)),
            new(new DateTime(2025, 1, 3, 9, 0, 0), 3, MoodType.StartOfWork, new DateOnly(2025, 1, 3)),
            new(new DateTime(2025, 1, 3, 18, 0, 0), 8, MoodType.EndOfWork, new DateOnly(2025, 1, 3))
        };
    }

    /// <summary>
    /// Standard date range for testing - using Last7Days which covers a week
    /// </summary>
    private static DateRange CreateStandardDateRange()
    {
        return DateRange.Last7Days;
    }

    /// <summary>
    /// Helper to create custom date range scenarios for testing edge cases
    /// Since DateRange is an enum, we'll use Last7Days but ensure our test data fits within the expected range
    /// </summary>
    private static DateRange CreateCustomDateRange()
    {
        return DateRange.Last7Days; // For tests, we'll use a standard range and adjust our test data accordingly
    }

    /// <summary>
    /// Standard colors for testing
    /// </summary>
    private static Microsoft.Maui.Graphics.Color StandardLineColor => Microsoft.Maui.Graphics.Colors.Blue;

    #endregion

    #region Impact Mode Tests

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_ShouldMatchApproval()
    {
        // Arrange
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

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
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

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
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

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

    [Fact]
    public async Task GenerateLineGraph_AverageMode_WithDataPointsAndGrid_ShouldMatchApproval()
    {
        // Arrange
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Average, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_AverageMode_StartOnlyEntries_ShouldMatchApproval()
    {
        // Arrange
        var moodEntries = CreateStartOnlyMoodEntries();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Average, 
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
        var rawDataPoints = CreateStandardRawMoodData();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            rawDataPoints, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_LineOnly_ShouldMatchApproval()
    {
        // Arrange
        var rawDataPoints = CreateStandardRawMoodData();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            rawDataPoints, 
            dateRange, 
            showDataPoints: false, 
            showAxesAndGrid: false, 
            showTitle: false, 
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
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            emptyEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_SingleDataPoint_ImpactMode_ShouldMatchApproval()
    {
        // Arrange
        var singleEntry = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 5, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
        };
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            singleEntry, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_EmptyData_ShouldMatchApproval()
    {
        // Arrange
        var emptyData = new List<RawMoodDataPoint>();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            emptyData, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_SmallDateRange_ShouldMatchApproval()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 6, EndOfWork = 4, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) }
        };
        var dateRange = CreateStandardDateRange(); // Using Last7Days range

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
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
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
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
        var moodEntries = CreateStandardMoodEntries();
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
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
        var extremeEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 1, EndOfWork = 10, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 2, EndOfWork = 10, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 1, EndOfWork = 9, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
        };
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            extremeEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_ExtremeNegativeValues_ShouldMatchApproval()
    {
        // Arrange - Create entries with maximum negative impact
        var extremeEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 10, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 9, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 8, EndOfWork = 1, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
        };
        var dateRange = CreateStandardDateRange();

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            extremeEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            GraphMode.Impact, 
            StandardLineColor);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion
}