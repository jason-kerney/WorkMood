using FluentAssertions;
using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests for the SaveLineGraphAsync methods in LineGraphService using London style mocking.
/// Focuses on verifying file shim calls with path validation and parameter count.
/// </summary>
public class LineGraphServiceSaveTests
{
    private readonly Mock<IDrawShimFactory> _mockDrawShimFactory;
    private readonly Mock<IFileShimFactory> _mockFileShimFactory;
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<ICanvasShim> _mockCanvas;
    private readonly Mock<IBitmapShim> _mockBitmap;
    private readonly Mock<IImageShim> _mockImage;
    private readonly Mock<IDrawDataShim> _mockData;
    private readonly LineGraphService _sut;
    private readonly SimpleLineGraphService _simpleLineGraphService;

    public LineGraphServiceSaveTests()
    {
        _mockDrawShimFactory = new Mock<IDrawShimFactory>();
        _mockFileShimFactory = new Mock<IFileShimFactory>();
        _mockFileShim = new Mock<IFileShim>();
        _mockCanvas = new Mock<ICanvasShim>();
        _mockBitmap = new Mock<IBitmapShim>();
        _mockImage = new Mock<IImageShim>();
        _mockData = new Mock<IDrawDataShim>();

        // Setup factory to return our mock file shim
        _mockFileShimFactory.Setup(x => x.Create()).Returns(_mockFileShim.Object);

        // Setup draw factory to return mock objects to prevent errors
        _mockDrawShimFactory.Setup(x => x.BitmapFromDimensions(It.IsAny<int>(), It.IsAny<int>())).Returns(_mockBitmap.Object);
        _mockDrawShimFactory.Setup(x => x.CanvasFromBitmap(It.IsAny<IBitmapShim>())).Returns(_mockCanvas.Object);
        _mockDrawShimFactory.Setup(x => x.ImageFromBitmap(It.IsAny<IBitmapShim>())).Returns(_mockImage.Object);
        _mockImage.Setup(x => x.Encode(It.IsAny<SkiaSharp.SKEncodedImageFormat>(), It.IsAny<int>())).Returns(_mockData.Object);
        _mockData.Setup(x => x.ToArray()).Returns(new byte[] { 1, 2, 3, 4 });

        // Setup canvas clear to prevent errors
        _mockCanvas.Setup(x => x.Clear(It.IsAny<SkiaSharp.SKColor>()));

        // Setup colors to prevent errors
        var mockColors = new Mock<IColorShims>();
        var mockWhiteColor = new Mock<IColorShim>();
        var mockLightGrayColor = new Mock<IColorShim>();
        var mockBlackColor = new Mock<IColorShim>();
        var mockDarkGrayColor = new Mock<IColorShim>();
        
        mockWhiteColor.Setup(x => x.Raw).Returns(SkiaSharp.SKColors.White);
        mockLightGrayColor.Setup(x => x.Raw).Returns(SkiaSharp.SKColors.LightGray);
        mockBlackColor.Setup(x => x.Raw).Returns(SkiaSharp.SKColors.Black);
        mockDarkGrayColor.Setup(x => x.Raw).Returns(SkiaSharp.SKColors.DarkGray);
        
        mockColors.Setup(x => x.White).Returns(mockWhiteColor.Object);
        mockColors.Setup(x => x.LightGray).Returns(mockLightGrayColor.Object);
        mockColors.Setup(x => x.Black).Returns(mockBlackColor.Object);
        mockColors.Setup(x => x.DarkGray).Returns(mockDarkGrayColor.Object);
        mockColors.Setup(x => x.FromArgb(It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte>())).Returns(mockWhiteColor.Object);
        
        _mockDrawShimFactory.Setup(x => x.Colors).Returns(mockColors.Object);

        // Setup path effects to prevent errors
        var mockPathEffects = new Mock<IPathEffectShims>();
        var mockPathEffect = new Mock<IPathEffectShim>();
        mockPathEffects.Setup(x => x.CreateDash(It.IsAny<float[]>(), It.IsAny<float>())).Returns(mockPathEffect.Object);
        _mockDrawShimFactory.Setup(x => x.PathEffects).Returns(mockPathEffects.Object);

        // Setup paint creation to prevent errors
        var mockPaint = new Mock<IPaintShim>();
        _mockDrawShimFactory.Setup(x => x.PaintFromArgs(It.IsAny<PaintShimArgs>())).Returns(mockPaint.Object);

        // Setup FromRaw method to prevent errors
        _mockDrawShimFactory.Setup(x => x.FromRaw(It.IsAny<SkiaSharp.SKCanvas>())).Returns(_mockCanvas.Object);

        // Setup font factory to prevent errors
        var mockFonts = new Mock<IFontShimFactory>();
        var mockFontStyles = new Mock<IFontStyleShimFactory>();
        var mockTypeface = new Mock<ITypeFaceShim>();
        var mockFontStyle = new Mock<IFontStyleShim>();
        
        mockFontStyles.Setup(x => x.Bold).Returns(mockFontStyle.Object);
        mockFonts.Setup(x => x.Styles).Returns(mockFontStyles.Object);
        mockFonts.Setup(x => x.FromFamilyName(It.IsAny<string>(), It.IsAny<IFontStyleShim>())).Returns(mockTypeface.Object);
        _mockDrawShimFactory.Setup(x => x.Fonts).Returns(mockFonts.Object);

        // Setup additional canvas methods that might be called
        _mockCanvas.Setup(x => x.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<IPaintShim>()));
        _mockCanvas.Setup(x => x.DrawText(It.IsAny<string>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<IPaintShim>()));
        _mockCanvas.Setup(x => x.DrawText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IPaintShim>()));
        _mockCanvas.Setup(x => x.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<IPaintShim>()));
        _mockCanvas.Setup(x => x.DrawPath(It.IsAny<SkiaSharp.SKPath>(), It.IsAny<IPaintShim>()));
        _mockCanvas.Setup(x => x.DrawRect(It.IsAny<SkiaSharp.SKRect>(), It.IsAny<IPaintShim>()));

        _sut = new LineGraphService(_mockDrawShimFactory.Object, _mockFileShimFactory.Object, lineGraphGenerator: new LineGraphGenerator(_mockDrawShimFactory.Object, _mockFileShimFactory.Object));
        _simpleLineGraphService = new SimpleLineGraphService(new GraphDataTransformer(), new LineGraphGenerator(_mockDrawShimFactory.Object, _mockFileShimFactory.Object));
    }

    [Fact]
    public async Task SaveLineGraphAsync_WithShowTrendLineTrue_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\output.png";
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Blue;
        const int width = 800;
        const int height = 600;

        // Act
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints, 
            showAxesAndGrid, 
            showTitle, 
            showTrendLine, 
            expectedFilePath, 
            lineColor, 
            width, 
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveLineGraphAsync_WithShowTrendLineFalse_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\output_no_trend.png";
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = false;
        const bool showAxesAndGrid = false;
        const bool showTitle = false;
        const bool showTrendLine = false;
        var lineColor = Colors.Red;
        const int width = 1024;
        const int height = 768;

        // Act
        await _simpleLineGraphService.SaveAverageGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints, 
            showAxesAndGrid, 
            showTitle, 
            showTrendLine, 
            expectedFilePath, 
            lineColor, 
            width, 
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveLineGraphAsync_WithBackgroundImage_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\output_with_background.png";
        const string backgroundImagePath = @"C:\test\background.png";
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        var lineColor = Colors.Green;
        const int width = 800;
        const int height = 600;

        // Setup file exists for background image to prevent errors
        _mockFileShim.Setup(x => x.Exists(backgroundImagePath)).Returns(true);
        _mockDrawShimFactory.Setup(x => x.DecodeBitmapFromFile(backgroundImagePath)).Returns(_mockBitmap.Object);

        // Act
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints, 
            showAxesAndGrid, 
            showTitle, 
            false, // showTrendLine
            expectedFilePath, 
            backgroundImagePath, 
            lineColor, 
            width, 
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveLineGraphAsync_WithEmptyFilePath_CallsFileShimWithEmptyPath()
    {
        // Arrange
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Blue;

        // Act
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints, 
            showAxesAndGrid, 
            showTitle, 
            showTrendLine, 
            string.Empty, 
            lineColor);

        // Assert - Verify file shim is called with empty path (implementation doesn't validate)
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(string.Empty, It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveLineGraphAsync_WithDefaultDimensions_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\output_default_size.png";
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Purple;

        // Act - Using default width and height (800x600)
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints, 
            showAxesAndGrid, 
            showTitle, 
            showTrendLine, 
            expectedFilePath, 
            lineColor);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveLineGraphAsync_CreatesNewFileShimForEachCall()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\multiple_calls.png";
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Orange;

        // Act - Call the method twice
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, 
            showTrendLine, expectedFilePath, lineColor);
        
        await _simpleLineGraphService.SaveImpactGraphAsync(
            moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, 
            showTrendLine, expectedFilePath, lineColor);

        // Assert
        _mockFileShimFactory.Verify(x => x.Create(), Times.AtLeast(2));
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(@"C:\path\to\file.png")]
    [InlineData(@"/unix/path/to/file.png")]
    [InlineData(@"relative\path\file.png")]
    [InlineData(@"file.png")]
    public async Task SaveLineGraphAsync_WithVariousFilePaths_CallsFileShimWithExactPath(string filePath)
    {
        // Arrange
        var moodEntries = CreateTestMoodEntries();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        const GraphMode graphMode = GraphMode.Impact;
        var lineColor = Colors.Blue;

        // Act
        await _sut.SaveLineGraphAsync(
            moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, 
            showTrendLine, filePath, graphMode, lineColor);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(filePath, It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveRawDataGraphAsync_WithShowTrendLineTrue_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\raw_output.png";
        var rawDataPoints = CreateTestRawDataPoints();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Blue;
        const int width = 800;
        const int height = 600;

        // Act
        await _sut.SaveRawDataGraphAsync(
            rawDataPoints,
            dateRange,
            showDataPoints,
            showAxesAndGrid,
            showTitle,
            showTrendLine,
            expectedFilePath,
            lineColor,
            width,
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveRawDataGraphAsync_WithShowTrendLineFalse_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\raw_output_no_trend.png";
        var rawDataPoints = CreateTestRawDataPoints();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = false;
        const bool showAxesAndGrid = false;
        const bool showTitle = false;
        const bool showTrendLine = false;
        var lineColor = Colors.Red;
        const int width = 1024;
        const int height = 768;

        // Act
        await _sut.SaveRawDataGraphAsync(
            rawDataPoints,
            dateRange,
            showDataPoints,
            showAxesAndGrid,
            showTitle,
            showTrendLine,
            expectedFilePath,
            lineColor,
            width,
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task SaveRawDataGraphAsync_WithBackgroundImage_CallsFileShimWithCorrectPathAndParameterCount()
    {
        // Arrange
        const string expectedFilePath = @"C:\test\raw_output_with_background.png";
        const string backgroundImagePath = @"C:\test\background.png";
        var rawDataPoints = CreateTestRawDataPoints();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = false;
        var lineColor = Colors.Green;
        const int width = 800;
        const int height = 600;

        // Setup file exists for background image to prevent errors
        _mockFileShim.Setup(x => x.Exists(backgroundImagePath)).Returns(true);
        _mockDrawShimFactory.Setup(x => x.DecodeBitmapFromFile(backgroundImagePath)).Returns(_mockBitmap.Object);

        // Act
        await _sut.SaveRawDataGraphAsync(
            rawDataPoints,
            dateRange,
            showDataPoints,
            showAxesAndGrid,
            showTitle,
            showTrendLine,
            expectedFilePath,
            backgroundImagePath,
            lineColor,
            width,
            height);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(expectedFilePath, It.IsAny<byte[]>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Theory]
    [InlineData(@"C:\path\to\raw_file.png")]
    [InlineData(@"/unix/path/to/raw_file.png")]
    [InlineData(@"relative\path\raw_file.png")]
    [InlineData(@"raw_file.png")]
    public async Task SaveRawDataGraphAsync_WithVariousFilePaths_CallsFileShimWithExactPath(string filePath)
    {
        // Arrange
        var rawDataPoints = CreateTestRawDataPoints();
        var dateRange = CreateTestDateRange();
        const bool showDataPoints = true;
        const bool showAxesAndGrid = true;
        const bool showTitle = true;
        const bool showTrendLine = true;
        var lineColor = Colors.Blue;

        // Act
        await _sut.SaveRawDataGraphAsync(
            rawDataPoints,
            dateRange,
            showDataPoints,
            showAxesAndGrid,
            showTitle,
            showTrendLine,
            filePath,
            lineColor);

        // Assert
        _mockFileShim.Verify(x => x.WriteAllBytesAsync(filePath, It.IsAny<byte[]>()), Times.Once);
    }

    /// <summary>
    /// Creates test raw data points for testing purposes
    /// </summary>
    private static IEnumerable<RawMoodDataPoint> CreateTestRawDataPoints()
    {
        var today = new DateOnly(2025, 10, 8);
        var baseDateTime = today.ToDateTime(TimeOnly.MinValue);
        
        return new[]
        {
            new RawMoodDataPoint(baseDateTime.AddDays(-6), 3, MoodType.StartOfWork, today.AddDays(-6)),
            new RawMoodDataPoint(baseDateTime.AddDays(-5), 5, MoodType.EndOfWork, today.AddDays(-5)),
            new RawMoodDataPoint(baseDateTime.AddDays(-4), 7, MoodType.StartOfWork, today.AddDays(-4)),
            new RawMoodDataPoint(baseDateTime.AddDays(-3), 4, MoodType.EndOfWork, today.AddDays(-3)),
            new RawMoodDataPoint(baseDateTime.AddDays(-2), 6, MoodType.StartOfWork, today.AddDays(-2)),
            new RawMoodDataPoint(baseDateTime.AddDays(-1), 8, MoodType.EndOfWork, today.AddDays(-1)),
            new RawMoodDataPoint(baseDateTime, 5, MoodType.StartOfWork, today)
        };
    }

    /// <summary>
    /// Creates test mood entries for testing purposes
    /// </summary>
    private static IEnumerable<MoodEntry> CreateTestMoodEntries()
    {
        var today = new DateOnly(2025, 10, 8);
        var (_, entries) = MoodDataTestHelper.GetRandomFakeData(today.AddDays(-7), seed: 12345, count: 7);
        return entries;
    }

    /// <summary>
    /// Creates a test date range for testing purposes
    /// </summary>
    private static DateRangeInfo CreateTestDateRange()
    {
        var today = new DateOnly(2025, 10, 8);
        return new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
    }
}