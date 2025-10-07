// using ApprovalTests;
// using ApprovalTests.Namers;
// using ApprovalTests.Reporters;
// using WorkMood.MauiApp.Models;
// using WorkMood.MauiApp.Services;
// using WorkMood.MauiApp.Shims;
// using Xunit;
// using SkiaSharp;

// namespace WorkMood.MauiApp.Tests.Services;

// /// <summary>
// /// Additional approval tests for LineGraphService background image functionality
// /// </summary>
// [UseReporter(typeof(ClipboardReporter))]
// [UseApprovalSubdirectory("ApprovalFiles")]
// public class LineGraphServiceBackgroundTests
// {
//     private readonly LineGraphService _lineGraphService;
//     private readonly IDrawShimFactory _drawShimFactory;
//     private readonly IFileShimFactory _fileShimFactory;
//     private readonly string _testImagesPath;

//     public LineGraphServiceBackgroundTests()
//     {
//         _drawShimFactory = new DrawShimFactory();
//         _fileShimFactory = new FileShimFactory();
//         _lineGraphService = new LineGraphService(_drawShimFactory, _fileShimFactory);
        
//         // Create test images directory
//         _testImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestImages");
//         Directory.CreateDirectory(_testImagesPath);
        
//         // Generate test background images
//         CreateTestBackgroundImages();
//     }

//     #region Test Data Helpers

//     private static List<MoodEntry> CreateTestMoodEntries()
//     {
//         return new List<MoodEntry>
//         {
//             new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 5, EndOfWork = 7, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
//             new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 6, EndOfWork = 4, CreatedAt = new DateTime(2025, 1, 2, 8, 30, 0), LastModified = new DateTime(2025, 1, 2, 16, 30, 0) },
//             new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 3, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 9, 0, 0), LastModified = new DateTime(2025, 1, 3, 18, 0, 0) }
//         };
//     }

//     private static List<RawMoodDataPoint> CreateTestRawMoodData()
//     {
//         return new List<RawMoodDataPoint>
//         {
//             new(new DateTime(2025, 1, 1, 8, 0, 0), 5, MoodType.StartOfWork, new DateOnly(2025, 1, 1)),
//             new(new DateTime(2025, 1, 1, 17, 0, 0), 7, MoodType.EndOfWork, new DateOnly(2025, 1, 1)),
//             new(new DateTime(2025, 1, 2, 8, 30, 0), 6, MoodType.StartOfWork, new DateOnly(2025, 1, 2)),
//             new(new DateTime(2025, 1, 2, 16, 30, 0), 4, MoodType.EndOfWork, new DateOnly(2025, 1, 2))
//         };
//     }

//     private static DateRange CreateTestDateRange()
//     {
//         return DateRange.Last7Days;
//     }

//     #endregion

//     #region Test Background Image Creation

//     private void CreateTestBackgroundImages()
//     {
//         CreateSolidColorBackground("blue_background.png", SKColors.LightBlue);
//         CreateSolidColorBackground("green_background.png", SKColors.LightGreen);
//         CreateGradientBackground("gradient_background.png");
//         CreatePatternBackground("pattern_background.png");
//     }

//     private void CreateSolidColorBackground(string filename, SKColor color)
//     {
//         var filePath = Path.Combine(_testImagesPath, filename);
//         using var bitmap = new SKBitmap(800, 600);
//         using var canvas = new SKCanvas(bitmap);
//         canvas.Clear(color);
        
//         using var image = SKImage.FromBitmap(bitmap);
//         using var data = image.Encode(SKEncodedImageFormat.Png, 100);
//         File.WriteAllBytes(filePath, data.ToArray());
//     }

//     private void CreateGradientBackground(string filename)
//     {
//         var filePath = Path.Combine(_testImagesPath, filename);
//         using var bitmap = new SKBitmap(800, 600);
//         using var canvas = new SKCanvas(bitmap);
        
//         var colors = new[] { SKColors.LightBlue, SKColors.White, SKColors.LightPink };
//         var positions = new float[] { 0, 0.5f, 1 };
        
//         using var shader = SKShader.CreateLinearGradient(
//             new SKPoint(0, 0),
//             new SKPoint(800, 600),
//             colors,
//             positions,
//             SKShaderTileMode.Clamp);
            
//         using var paint = new SKPaint { Shader = shader };
//         canvas.DrawRect(new SKRect(0, 0, 800, 600), paint);
        
//         using var image = SKImage.FromBitmap(bitmap);
//         using var data = image.Encode(SKEncodedImageFormat.Png, 100);
//         File.WriteAllBytes(filePath, data.ToArray());
//     }

//     private void CreatePatternBackground(string filename)
//     {
//         var filePath = Path.Combine(_testImagesPath, filename);
//         using var bitmap = new SKBitmap(800, 600);
//         using var canvas = new SKCanvas(bitmap);
//         canvas.Clear(SKColors.White);
        
//         using var paint = new SKPaint
//         {
//             Color = SKColors.LightGray,
//             Style = SKPaintStyle.Stroke,
//             StrokeWidth = 1
//         };
        
//         // Draw a grid pattern
//         for (int x = 0; x < 800; x += 40)
//         {
//             canvas.DrawLine(x, 0, x, 600, paint);
//         }
//         for (int y = 0; y < 600; y += 40)
//         {
//             canvas.DrawLine(0, y, 800, y, paint);
//         }
        
//         using var image = SKImage.FromBitmap(bitmap);
//         using var data = image.Encode(SKEncodedImageFormat.Png, 100);
//         File.WriteAllBytes(filePath, data.ToArray());
//     }

//     #endregion

//     #region Background Image Tests - Line Graphs

//     [Fact]
//     public async Task GenerateLineGraph_ImpactMode_WithBlueBackground_ShouldMatchApproval()
//     {
//         // Arrange
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "blue_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             GraphMode.Impact, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.Yellow);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateLineGraph_AverageMode_WithGradientBackground_ShouldMatchApproval()
//     {
//         // Arrange
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "gradient_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             GraphMode.Average, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.DarkRed);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateLineGraph_WithPatternBackground_NoGridNoTitle_ShouldMatchApproval()
//     {
//         // Arrange
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "pattern_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: false, 
//             showTitle: false, 
//             GraphMode.Impact, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.Purple);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateLineGraph_WithNonExistentBackground_ShouldFallbackToWhite()
//     {
//         // Arrange
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var nonExistentPath = Path.Combine(_testImagesPath, "non_existent.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             GraphMode.Impact, 
//             nonExistentPath,
//             Microsoft.Maui.Graphics.Colors.Blue);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     #endregion

//     #region Background Image Tests - Raw Data Graphs

//     [Fact]
//     public async Task GenerateRawDataGraph_WithBlueBackground_ShouldMatchApproval()
//     {
//         // Arrange
//         var rawDataPoints = CreateTestRawMoodData();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "blue_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
//             rawDataPoints, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.Orange);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateRawDataGraph_WithGradientBackground_ShouldMatchApproval()
//     {
//         // Arrange
//         var rawDataPoints = CreateTestRawMoodData();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "gradient_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
//             rawDataPoints, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.DarkBlue);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateRawDataGraph_WithNonExistentBackground_ShouldFallbackToWhite()
//     {
//         // Arrange
//         var rawDataPoints = CreateTestRawMoodData();
//         var dateRange = CreateTestDateRange();
//         var nonExistentPath = Path.Combine(_testImagesPath, "missing_file.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateRawDataGraphAsync(
//             rawDataPoints, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             nonExistentPath,
//             Microsoft.Maui.Graphics.Colors.Green);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     #endregion

//     #region Color Contrast Tests

//     [Fact]
//     public async Task GenerateLineGraph_DarkBackgroundWithLightLine_ShouldMatchApproval()
//     {
//         // Arrange
//         CreateSolidColorBackground("dark_background.png", SKColors.DarkSlateGray);
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "dark_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             GraphMode.Impact, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.Yellow);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     [Fact]
//     public async Task GenerateLineGraph_LightBackgroundWithDarkLine_ShouldMatchApproval()
//     {
//         // Arrange
//         CreateSolidColorBackground("light_background.png", SKColors.Beige);
//         var moodEntries = CreateTestMoodEntries();
//         var dateRange = CreateTestDateRange();
//         var backgroundPath = Path.Combine(_testImagesPath, "light_background.png");

//         // Act
//         var imageBytes = await _lineGraphService.GenerateLineGraphAsync(
//             moodEntries, 
//             dateRange, 
//             showDataPoints: true, 
//             showAxesAndGrid: true, 
//             showTitle: true, 
//             GraphMode.Impact, 
//             backgroundPath,
//             Microsoft.Maui.Graphics.Colors.DarkBlue);

//         // Assert
//         Approvals.VerifyBinaryFile(imageBytes, "png");
//     }

//     #endregion
// }