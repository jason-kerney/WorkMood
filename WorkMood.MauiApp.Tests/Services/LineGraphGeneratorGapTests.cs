using Microsoft.Maui.Graphics;
using Moq;
using SkiaSharp;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Deterministic (non-approval) tests proving <see cref="LineGraphGenerator"/> breaks the
/// rendered line at missing calendar days instead of bridging them, using a mocked
/// <see cref="ICanvasShim"/> to capture the <see cref="SKPath"/> instances passed to
/// <see cref="ICanvasShim.DrawPath"/>.
/// </summary>
public class LineGraphGeneratorGapTests
{
    private static (LineGraphGenerator Generator, List<SKPath> CapturedPaths) CreateGeneratorWithPathCapture()
    {
        var mockDrawFactory = new Mock<IDrawShimFactory>();
        var mockCanvas = new Mock<ICanvasShim>();
        var mockBitmap = new Mock<IBitmapShim>();
        var mockImage = new Mock<IImageShim>();
        var mockData = new Mock<IDrawDataShim>();
        var mockPaint = new Mock<IPaintShim>();

        mockDrawFactory.Setup(f => f.BitmapFromDimensions(It.IsAny<int>(), It.IsAny<int>())).Returns(mockBitmap.Object);
        mockDrawFactory.Setup(f => f.CanvasFromBitmap(It.IsAny<IBitmapShim>())).Returns(mockCanvas.Object);
        mockDrawFactory.Setup(f => f.ImageFromBitmap(It.IsAny<IBitmapShim>())).Returns(mockImage.Object);
        mockImage.Setup(i => i.Encode(It.IsAny<SKEncodedImageFormat>(), It.IsAny<int>())).Returns(mockData.Object);
        mockData.Setup(d => d.ToArray()).Returns([1, 2, 3, 4]);
        mockDrawFactory.Setup(f => f.PaintFromArgs(It.IsAny<PaintShimArgs>())).Returns(mockPaint.Object);

        var mockColors = new Mock<IColorShims>();
        var mockWhite = new Mock<IColorShim>();
        mockWhite.Setup(c => c.Raw).Returns(SKColors.White);
        mockColors.Setup(c => c.White).Returns(mockWhite.Object);
        mockColors.Setup(c => c.FromArgb(It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte>())).Returns(mockWhite.Object);
        mockDrawFactory.Setup(f => f.Colors).Returns(mockColors.Object);

        mockCanvas.Setup(c => c.Clear(It.IsAny<SKColor>()));
        mockCanvas.Setup(c => c.DrawRect(It.IsAny<SKRect>(), It.IsAny<IPaintShim>()));

        var capturedPaths = new List<SKPath>();
        mockCanvas.Setup(c => c.DrawPath(It.IsAny<SKPath>(), It.IsAny<IPaintShim>()))
            .Callback<SKPath, IPaintShim>((path, _) => capturedPaths.Add(new SKPath(path)));

        var generator = new LineGraphGenerator(mockDrawFactory.Object, new Mock<IFileShimFactory>().Object);
        return (generator, capturedPaths);
    }

    private static GraphData CreateGraphData(IEnumerable<FilledGraphDataPoint> dataPoints) => new()
    {
        DataPoints = dataPoints.ToList(),
        Title = "Gap Test",
        YAxisLabel = "Mood",
        XAxisLabel = "Time",
        YAxisRange = new AxisRange(0, 10)
    };

    [Fact]
    public async Task GenerateLineGraphAsync_WithNoCalendarGaps_DrawsOneContinuousPath()
    {
        // Arrange
        var (generator, capturedPaths) = CreateGeneratorWithPathCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 1, 10, 0, 0), 5),
            new(new DateTime(2025, 1, 2, 10, 0, 0), 6),
            new(new DateTime(2025, 1, 3, 10, 0, 0), 7)
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - all three consecutive days are drawn as a single connected path
        var path = Assert.Single(capturedPaths);
        Assert.Equal(3, path.PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithMissingCalendarDayBetweenPoints_DrawsSeparatePathsPerSegment()
    {
        // Arrange
        var (generator, capturedPaths) = CreateGeneratorWithPathCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 1, 10, 0, 0), 5),
            new(new DateTime(2025, 1, 2, 10, 0, 0), 6),
            // Jan 3 has no recorded point - a calendar-day gap
            new(new DateTime(2025, 1, 4, 10, 0, 0), 7),
            new(new DateTime(2025, 1, 5, 10, 0, 0), 8)
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - the missing Jan 3 breaks the line into two separate 2-point paths
        Assert.Equal(2, capturedPaths.Count);
        Assert.Equal(2, capturedPaths[0].PointCount);
        Assert.Equal(2, capturedPaths[1].PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithIsolatedSinglePointSegment_DoesNotDrawAPathForIt()
    {
        // Arrange
        var (generator, capturedPaths) = CreateGeneratorWithPathCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 1, 10, 0, 0), 5), // isolated - Jan 2 is missing
            new(new DateTime(2025, 1, 3, 10, 0, 0), 6),
            new(new DateTime(2025, 1, 4, 10, 0, 0), 7)
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - only the contiguous Jan 3 -> Jan 4 pair produces a path; the isolated
        // Jan 1 point is never connected to anything
        var path = Assert.Single(capturedPaths);
        Assert.Equal(2, path.PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithRawDataStyleMultiplePointsPerDay_KeepsSameDayPointsConnected()
    {
        // Arrange - two points on the same calendar day (e.g. start-of-work / end-of-work)
        // followed by a calendar-day gap before the next recorded day.
        var (generator, capturedPaths) = CreateGeneratorWithPathCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 1, 9, 0, 0), 5),
            new(new DateTime(2025, 1, 1, 17, 0, 0), 6),
            // Jan 2 has no recorded point - a calendar-day gap
            new(new DateTime(2025, 1, 3, 9, 0, 0), 7)
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - the two same-day points connect; the isolated Jan 3 point draws nothing
        var path = Assert.Single(capturedPaths);
        Assert.Equal(2, path.PointCount);
    }
}
