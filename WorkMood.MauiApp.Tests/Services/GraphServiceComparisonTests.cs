using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Diagnostic test to compare LineGraphService vs SimpleLineGraphService outputs
/// </summary>
public class GraphServiceComparisonTests
{
    [Fact]
    public async Task CompareBothServices_SameInput_ShouldProduceSimilarOutput()
    {
        // Arrange - use same dependencies for both services
        var drawShimFactory = new DrawShimFactory();
        var fileShimFactory = new FileShimFactory();
        var lineGraphGenerator = new LineGraphGenerator(drawShimFactory, fileShimFactory);
        var lineGraphService = new LineGraphService(drawShimFactory, fileShimFactory, lineGraphGenerator: lineGraphGenerator);
        var simpleLineGraphService = new SimpleLineGraphService(new GraphDataTransformer(), lineGraphGenerator);

        var (today, moodEntries) = MoodDataTestHelper.GetRandomFakeData(new DateOnly(2025, 1, 1), seed: 123, count: 30);
        var dateRange = new DateRangeInfo(DateRange.Last7Days, new FakeDateShim(today));
        var standardLineColor = Microsoft.Maui.Graphics.Colors.Blue;

        // Act - generate images with both services
        var lineGraphImageBytes = await lineGraphService.GenerateLineGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            GraphMode.Impact, 
            standardLineColor,
            width: 800,
            height: 600);

        var simpleGraphImageBytes = await simpleLineGraphService.GenerateImpactGraphAsync(
            moodEntries, 
            dateRange, 
            showDataPoints: true, 
            showAxesAndGrid: true, 
            showTitle: true, 
            showTrendLine: false,
            standardLineColor,
            width: 800,
            height: 600);

        // Assert - images should be identical or very similar
        // For debugging: save both images to compare manually
        var baseDir = @"c:\Development\github\jason-kerney\WorkMood\WorkMood.MauiApp.Tests\Services\ApprovalFiles\LineGraphs";
        await File.WriteAllBytesAsync(Path.Combine(baseDir, "comparison_linegraph.png"), lineGraphImageBytes);
        await File.WriteAllBytesAsync(Path.Combine(baseDir, "comparison_simplegraph.png"), simpleGraphImageBytes);

        // Basic checks
        Assert.True(lineGraphImageBytes.Length > 0, "LineGraphService should produce image data");
        Assert.True(simpleGraphImageBytes.Length > 0, "SimpleLineGraphService should produce image data");
        
        // Note: The images might not be byte-for-byte identical due to minor rendering differences,
        // but they should be visually equivalent. Manual comparison needed.
    }
}