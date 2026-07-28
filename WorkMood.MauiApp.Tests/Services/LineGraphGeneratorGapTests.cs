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
    private sealed class RenderCapture
    {
        public List<SKPath> Paths { get; } = [];

        public List<(string Text, float X, float Y)> DrawnText { get; } = [];

        public List<(float X, float Y, int Radius)> DrawnCircles { get; } = [];
    }

    private static (LineGraphGenerator Generator, RenderCapture Capture) CreateGeneratorWithCapture()
    {
        var mockDrawFactory = new Mock<IDrawShimFactory>();
        var mockCanvas = new Mock<ICanvasShim>();
        var mockBitmap = new Mock<IBitmapShim>();
        var mockImage = new Mock<IImageShim>();
        var mockData = new Mock<IDrawDataShim>();
        var mockPaint = new Mock<IPaintShim>();
        var mockPathEffects = new Mock<IPathEffectShims>();
        var mockPathEffect = new Mock<IPathEffectShim>();
        var mockFonts = new Mock<IFontShimFactory>();
        var mockFontStyles = new Mock<IFontStyleShimFactory>();
        var mockTypeface = new Mock<ITypeFaceShim>();

        mockPathEffects.Setup(p => p.CreateDash(It.IsAny<float[]>(), It.IsAny<float>())).Returns(mockPathEffect.Object);
        mockDrawFactory.Setup(f => f.PathEffects).Returns(mockPathEffects.Object);

        mockFontStyles.Setup(s => s.Bold).Returns(new FontStyleShim(SKFontStyle.Bold));
        mockFonts.Setup(f => f.Styles).Returns(mockFontStyles.Object);
        mockFonts.Setup(f => f.FromFamilyName(It.IsAny<string>(), It.IsAny<IFontStyleShim>())).Returns(mockTypeface.Object);
        mockDrawFactory.Setup(f => f.Fonts).Returns(mockFonts.Object);

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

        var capture = new RenderCapture();

        mockCanvas.Setup(c => c.DrawPath(It.IsAny<SKPath>(), It.IsAny<IPaintShim>()))
            .Callback<SKPath, IPaintShim>((path, _) => capture.Paths.Add(new SKPath(path)));

        mockCanvas.Setup(c => c.DrawText(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<IPaintShim>()))
            .Callback<string, float, float, IPaintShim>((text, x, y, _) => capture.DrawnText.Add((text, x, y)));

        mockCanvas.Setup(c => c.DrawText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPaintShim>()))
            .Callback<string, int, int, IPaintShim>((text, x, y, _) => capture.DrawnText.Add((text, x, y)));

        mockCanvas.Setup(c => c.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<IPaintShim>()))
            .Callback<float, float, int, IPaintShim>((x, y, radius, _) => capture.DrawnCircles.Add((x, y, radius)));

        var generator = new LineGraphGenerator(mockDrawFactory.Object, new Mock<IFileShimFactory>().Object);
        return (generator, capture);
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
        var (generator, capture) = CreateGeneratorWithCapture();
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
        var path = Assert.Single(capture.Paths);
        Assert.Equal(3, path.PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithMissingWeekdayBetweenPoints_DrawsSeparatePathsPerSegment()
    {
        // Arrange
        var (generator, capture) = CreateGeneratorWithCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 2, 10, 0, 0), 5), // Thursday
            new(new DateTime(2025, 1, 6, 10, 0, 0), 6)  // Monday (Friday missing)
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - missing weekday still breaks continuity (regression lock)
        Assert.Empty(capture.Paths);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithUnrecordedWeekendBetweenFridayAndMonday_DrawsSingleContinuousPath()
    {
        // Arrange
        var (generator, capture) = CreateGeneratorWithCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 3, 10, 0, 0), 5), // Friday
            new(new DateTime(2025, 1, 6, 10, 0, 0), 7)  // Monday
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 7));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert - weekend-only gap is compressed and does not break the segment
        var path = Assert.Single(capture.Paths);
        Assert.Equal(2, path.PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithRawDataStyleMultiplePointsPerDay_KeepsSameDayPointsConnected()
    {
        // Arrange - two points on the same calendar day (e.g. start-of-work / end-of-work)
        // followed by a calendar-day gap before the next recorded day.
        var (generator, capture) = CreateGeneratorWithCapture();
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
        var path = Assert.Single(capture.Paths);
        Assert.Equal(2, path.PointCount);
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithRecordedSaturdayAndMissingSunday_PreservesSaturdaySlotCompressionPolicy()
    {
        // Arrange
        var (generator, capture) = CreateGeneratorWithCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 3, 10, 0, 0), 5), // Friday
            new(new DateTime(2025, 1, 4, 10, 0, 0), 6), // Saturday recorded
            new(new DateTime(2025, 1, 6, 10, 0, 0), 7)  // Monday, Sunday missing
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 6));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert
        var path = Assert.Single(capture.Paths);
        Assert.Equal(3, path.PointCount);

        var fridayX = path.GetPoint(0).X;
        var saturdayX = path.GetPoint(1).X;
        var mondayX = path.GetPoint(2).X;

        var fridayToSaturday = saturdayX - fridayX;
        var saturdayToMonday = mondayX - saturdayX;

        Assert.True(fridayToSaturday > 0);
        Assert.True(saturdayToMonday > fridayToSaturday, "Monday should be farther than one day from Saturday when Sunday is compressed out.");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithUnrecordedWeekend_CompressesWeekendSpaceInXAxisLabels()
    {
        // Arrange
        var (generator, capture) = CreateGeneratorWithCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 3, 10, 0, 0), 5), // Friday
            new(new DateTime(2025, 1, 6, 10, 0, 0), 7)  // Monday
        };
        var graphData = CreateGraphData(dataPoints);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 6));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: true, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert
        var mondayLabel = Assert.Single(capture.DrawnText, t => t.Text == "01/06");
        var fridayLabel = Assert.Single(capture.DrawnText, t => t.Text == "01/03");
        Assert.True(mondayLabel.X > fridayLabel.X);
        Assert.True(mondayLabel.X - fridayLabel.X < 680f, "Weekend compression should reduce Monday distance from Friday compared with full-width calendar spacing.");
    }

    [Fact]
    public async Task GenerateLineGraphAsync_WithRawDataAcrossCompressedWeekend_KeepsPointXMonotonic()
    {
        // Arrange
        var (generator, capture) = CreateGeneratorWithCapture();
        var dataPoints = new List<FilledGraphDataPoint>
        {
            new(new DateTime(2025, 1, 3, 9, 0, 0), 5),
            new(new DateTime(2025, 1, 3, 17, 0, 0), 6),
            new(new DateTime(2025, 1, 6, 9, 0, 0), 7),
            new(new DateTime(2025, 1, 6, 17, 0, 0), 8)
        };
        var graphData = new GraphData
        {
            DataPoints = dataPoints,
            Title = "Raw Data",
            YAxisLabel = "Mood",
            XAxisLabel = "Time",
            YAxisRange = new AxisRange(0, 10),
            IsRawData = true
        };
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new DateOnly(2025, 1, 6));

        // Act
        await generator.GenerateLineGraphAsync(graphData, dateRange, showDataPoints: false, showAxesAndGrid: false, showTitle: false, showTrendLine: false, Colors.Blue, 800, 600);

        // Assert
        var path = Assert.Single(capture.Paths);
        Assert.Equal(4, path.PointCount);
        Assert.True(path.GetPoint(0).X < path.GetPoint(1).X);
        Assert.True(path.GetPoint(1).X < path.GetPoint(2).X);
        Assert.True(path.GetPoint(2).X < path.GetPoint(3).X);
    }
}
