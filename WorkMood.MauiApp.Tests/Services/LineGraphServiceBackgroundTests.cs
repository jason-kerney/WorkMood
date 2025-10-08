using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;
using SkiaSharp;
using Xunit.Sdk;
using WorkMood.MauiApp.Tests.TestHelpers;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Additional approval tests for LineGraphService background image functionality
/// </summary>
[UseReporter(typeof(ClipboardReporter))]
[UseApprovalSubdirectory("ApprovalFiles")]
public class LineGraphServiceBackgroundTests
{
    private readonly LineGraphService _lineGraphService;
    private readonly IDrawShimFactory _drawShimFactory;
    private readonly IFileShimFactory _fileShimFactory;
    private readonly string _testImagesPath;

    public LineGraphServiceBackgroundTests()
    {
        _drawShimFactory = new DrawShimFactory();
        _fileShimFactory = new FileShimFactory();
        _lineGraphService = new LineGraphService(_drawShimFactory, _fileShimFactory);

        // Create test images directory
        _testImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestImages");
        Directory.CreateDirectory(_testImagesPath);

        // Generate test background images
        CreateTestBackgroundImages();
    }

    #region Test Background Image Creation

    private void CreateTestBackgroundImages()
    {
        CreateSolidColorBackground("blue_background.png", SKColors.LightBlue);
        CreateSolidColorBackground("green_background.png", SKColors.LightGreen);
        CreateGradientBackground("gradient_background.png");
        CreatePatternBackground("pattern_background.png");
    }

    private void CreateSolidColorBackground(string filename, SKColor color)
    {
        var filePath = Path.Combine(_testImagesPath, filename);
        if (File.Exists(filePath))
            return;

        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
            
        File.WriteAllBytes(filePath, data.ToArray());
    }

    private void CreateGradientBackground(string filename)
    {
        var filePath = Path.Combine(_testImagesPath, filename);
        if (File.Exists(filePath))
            return;

        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);
        
        var colors = new[] { SKColors.LightBlue, SKColors.White, SKColors.LightPink };
        var positions = new float[] { 0, 0.5f, 1 };
        
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(800, 600),
            colors,
            positions,
            SKShaderTileMode.Clamp);
            
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(new SKRect(0, 0, 800, 600), paint);
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    private void CreatePatternBackground(string filename)
    {
        var filePath = Path.Combine(_testImagesPath, filename);
        if (File.Exists(filePath))
            return;

        using var bitmap = new SKBitmap(800, 600);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        
        using var paint = new SKPaint
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        
        // Draw a grid pattern
        for (int x = 0; x < 800; x += 40)
        {
            canvas.DrawLine(x, 0, x, 600, paint);
        }
        for (int y = 0; y < 600; y += 40)
        {
            canvas.DrawLine(0, y, 800, y, paint);
        }
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    #endregion

    #region Background Image Tests - Line Graphs

    [Fact]
    public async Task GenerateLineGraph_ImpactMode_WithBlueBackground_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1803, 11, 28), seed: 8697, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "blue_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Impact, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.Yellow);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_AverageMode_WithGradientBackground_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1718, 4, 27), seed: 1097, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "gradient_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Average, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.DarkRed);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_WithPatternBackground_NoGridNoTitle_ShouldMatchApproval()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1726, 7, 11), seed: 1220, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "pattern_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: false, 
            showTitle: false, 
            showTrendLine: false,
            GraphMode.Impact, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.Purple);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_WithNonExistentBackground_ShouldFallbackToWhite()
    {
        // Arrange
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1892, 6, 21), seed: 7815, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var nonExistentPath = Path.Combine(_testImagesPath, "non_existent.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Impact, 
            nonExistentPath,
            Microsoft.Maui.Graphics.Colors.Blue);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Background Image Tests - Raw Data Graphs

    [Fact]
    public async Task GenerateRawDataGraph_WithBlueBackground_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1948, 7, 13), seed: 3162, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "blue_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data), 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.Orange);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_WithGradientBackground_ShouldMatchApproval()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1912, 5, 24), seed: 3993, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "gradient_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data), 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.DarkBlue);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateRawDataGraph_WithNonExistentBackground_ShouldFallbackToWhite()
    {
        // Arrange
        var (today, data) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2024, 8, 28), seed: 8411, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var nonExistentPath = Path.Combine(_testImagesPath, "missing_file.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
            MoodDataTestHelper.ConvertToRawMoodDataPoints(data), 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            nonExistentPath,
            Microsoft.Maui.Graphics.Colors.Green);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    #region Color Contrast Tests

    [Fact]
    public async Task GenerateLineGraph_DarkBackgroundWithLightLine_ShouldMatchApproval()
    {
        // Arrange
        CreateSolidColorBackground("dark_background.png", SKColors.DarkSlateGray);
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1921, 05, 03), seed: 3031, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "dark_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Impact, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.Yellow);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    [Fact]
    public async Task GenerateLineGraph_LightBackgroundWithDarkLine_ShouldMatchApproval()
    {
        // Arrange
        CreateSolidColorBackground("light_background.png", SKColors.Beige);
        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(1739, 09, 23), seed: 7900, count: 14);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var backgroundPath = Path.Combine(_testImagesPath, "light_background.png");

        // Act
        var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Impact, 
            backgroundPath,
            Microsoft.Maui.Graphics.Colors.DarkBlue);

        // Assert
        Approvals.VerifyBinaryFile(imageBytes, "png");
    }

    #endregion

    [Fact]
    public async Task GenerateLineGraphAsync_WithBackgroundImageAndTrendLine_ReturnsValidImageData()
    {
        // Arrange
        var service = new LineGraphService();
        var startDate = new DateOnly(1937, 7, 19);
        var (today, entries) = MoodDataTestHelper.GetRandomFakeData(startDate, seed: 2795, count: 10);
        
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var lineColor = Colors.Orange;
        var backgroundImagePath = Path.Combine(_testImagesPath, "light_background.png");

        // Act
        var result = await service.GenerateLineGraphAsync(
            entries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: true, // Test the newly added parameter
            GraphMode.Impact, 
            backgroundImagePath,
            lineColor);

        // Assert
        Approvals.VerifyBinaryFile(result, "png");
    }
}