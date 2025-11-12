using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Graphics;

public class GridComponentShould
{
    private readonly GridComponent _component;

    public GridComponentShould()
    {
        _component = new GridComponent();
    }

    [Fact]
    public void ImplementIGraphComponentInterface()
    {
        Assert.IsAssignableFrom<IGraphComponent>(_component);
    }

    [Fact]
    public void Draw_ShouldSetCorrectStrokeProperties()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.VerifySet(c => c.StrokeColor = Colors.LightGray, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 0.5f, Times.Once);
    }

    [Fact]
    public void Draw_ShouldDrawCorrectNumberOfVerticalLines()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should draw 15 vertical lines (0 to 14 inclusive)
        canvas.Verify(c => c.DrawLine(It.IsAny<float>(), 20f, It.IsAny<float>(), 80f), Times.Exactly(15));
    }

    [Theory]
    [InlineData(0, 20f)] // First vertical line at margin
    [InlineData(1, 24.615385f)] // Second line at margin + pointSpacing
    [InlineData(7, 52.307693f)] // Middle line  
    [InlineData(14, 84.615385f)] // Last line at margin + (14 * pointSpacing)
    public void Draw_ShouldPositionVerticalLinesCorrectly(int lineIndex, float expectedX)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verify specific vertical line positions
        canvas.Verify(c => c.DrawLine(expectedX, 20f, expectedX, 80f), Times.Once);
    }

    [Fact]
    public void Draw_WithMaxAbsValueOne_ShouldDrawCorrectHorizontalLines()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0); // MaxAbsValue = 1, gridInterval = 1
        
        // Expected horizontal lines at y = centerY - (value * scaleFactor)
        // centerY = 50, scaleFactor = 30 (60 / (2 * 1))
        // Lines at: +1 (y=20), -1 (y=80), center line skipped
        var expectedY1 = 20f; // y = 50 - (1 * 30)
        var expectedY2 = 80f; // y = 50 - (-1 * 30)

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.DrawLine(20f, expectedY1, 80f, expectedY1), Times.Once);
        canvas.Verify(c => c.DrawLine(20f, expectedY2, 80f, expectedY2), Times.Once);
    }

    [Fact]
    public void Draw_WithMaxAbsValueThree_ShouldUseGridIntervalOne()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(3.0); // MaxAbsValue = 3, gridInterval = 1
        
        // Should draw horizontal lines at -3, -2, -1, +1, +2, +3 (center skipped)
        // centerY = 50, scaleFactor = 10 (60 / (2 * 3))

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should draw 6 horizontal lines (excluding center)
        canvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 80f, It.IsAny<float>()), Times.Exactly(6));
    }

    [Fact] 
    public void Draw_WithMaxAbsValueFour_ShouldUseGridIntervalTwo()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(4.0); // MaxAbsValue = 4, gridInterval = Math.Ceiling(4/3) = 2
        
        // Should draw horizontal lines at -4, -2, +2, +4 (center and odd values skipped)
        // centerY = 50, scaleFactor = 7.5 (60 / (2 * 4))

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should draw 4 horizontal lines
        canvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 80f, It.IsAny<float>()), Times.Exactly(4));
    }

    [Fact]
    public void Draw_ShouldSkipCenterLine()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(2.0);
        
        var centerY = 50f; // margin + (graphHeight / 2)

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should not draw line at center Y position
        canvas.Verify(c => c.DrawLine(20f, centerY, 80f, centerY), Times.Never);
    }

    [Theory]
    [InlineData(100f, 100f, 60f, 60f)] // Standard bounds
    [InlineData(200f, 150f, 160f, 110f)] // Wide bounds
    [InlineData(80f, 120f, 40f, 80f)] // Narrow bounds
    public void Draw_WithDifferentBounds_ShouldCalculateCorrectDimensions(float width, float height, float expectedGraphWidth, float expectedGraphHeight)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, width, height);
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verify dimensions by checking line drawing calls
        // Vertical lines should go from margin to height-margin
        canvas.Verify(c => c.DrawLine(It.IsAny<float>(), 20f, It.IsAny<float>(), height - 20f), Times.AtLeast(15));
        
        // Horizontal lines should go from margin to width-margin  
        canvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), width - 20f, It.IsAny<float>()), Times.AtLeast(1));

        // Verify calculated values
        Assert.Equal(expectedGraphWidth, width - 40f);
        Assert.Equal(expectedGraphHeight, height - 40f);
    }

    [Fact]
    public void Draw_WithZeroMaxAbsValue_ShouldUseMinimumValueOfOne()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = new MoodVisualizationData 
        { 
            DailyValues = Array.Empty<DailyMoodValue>(),
            MaxAbsoluteValue = 0.0 // Should default to 1.0 in Math.Max(1.0, 0.0)
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should behave as if maxAbsValue = 1.0
        canvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 80f, It.IsAny<float>()), Times.Exactly(2));
    }

    [Fact]
    public void Draw_ShouldCalculatePointSpacingCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 140, 100); // graphWidth = 100, pointSpacing = 100/13 ≈ 7.69
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Check specific positions based on calculated spacing
        var expectedSpacing = 100f / 13f; // ≈ 7.692308
        canvas.Verify(c => c.DrawLine(20f, 20f, 20f, 80f), Times.Once); // First line
        canvas.Verify(c => c.DrawLine(20f + expectedSpacing, 20f, 20f + expectedSpacing, 80f), Times.Once); // Second line
    }

    [Fact]
    public void Draw_WithVeryLargeMaxValue_ShouldHandleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(100.0); // Very large value

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);

        // Should still draw grid lines
        canvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.AtLeast(15));
    }

    [Fact]
    public void Draw_CalledMultipleTimes_ShouldProduceConsistentResults()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0);

        // Act
        _component.Draw(canvas.Object, bounds, data);
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should call stroke properties twice
        canvas.VerifySet(c => c.StrokeColor = Colors.LightGray, Times.Exactly(2));
        canvas.VerifySet(c => c.StrokeSize = 0.5f, Times.Exactly(2));
        
        // Should draw lines twice
        canvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.AtLeast(34));
    }

    [Fact]
    public void Draw_ShouldSetStrokePropertiesBeforeDrawingLines()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(1.0);
        
        var sequence = new MockSequence();
        canvas.InSequence(sequence).SetupSet(c => c.StrokeColor = Colors.LightGray);
        canvas.InSequence(sequence).SetupSet(c => c.StrokeSize = 0.5f);
        canvas.InSequence(sequence).Setup(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verification happens through sequence
        canvas.Verify();
    }

    [Theory]
    [InlineData(1.0, 1.0)] // maxAbsValue <= 3, gridInterval = 1
    [InlineData(2.5, 1.0)] // maxAbsValue <= 3, gridInterval = 1  
    [InlineData(3.0, 1.0)] // maxAbsValue <= 3, gridInterval = 1
    [InlineData(3.1, 2.0)] // maxAbsValue > 3, gridInterval = Math.Ceiling(3.1/3) = 2
    [InlineData(6.0, 2.0)] // maxAbsValue > 3, gridInterval = Math.Ceiling(6/3) = 2
    [InlineData(7.5, 3.0)] // maxAbsValue > 3, gridInterval = Math.Ceiling(7.5/3) = 3
    public void Draw_ShouldCalculateGridIntervalCorrectly(double maxAbsValue, double expectedInterval)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(maxAbsValue);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Calculate expected number of horizontal lines
        var expectedHorizontalLines = 0;
        for (double i = -maxAbsValue; i <= maxAbsValue; i += expectedInterval)
        {
            if (Math.Abs(i) >= 0.001) // Skip center line
                expectedHorizontalLines++;
        }
        
        canvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 80f, It.IsAny<float>()), Times.Exactly(expectedHorizontalLines));
    }

    [Fact]
    public void Draw_WithNegativeAndPositiveValues_ShouldDrawSymmetricalGrid()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(2.0); // Should draw lines at -2, -1, +1, +2
        
        var centerY = 50f;
        var scaleFactor = 30f / 2f; // graphHeight / (2 * maxAbsValue) = 60 / 4 = 15
        var expectedYPos2 = centerY - (2f * scaleFactor); // 50 - 30 = 20
        var expectedYPos1 = centerY - (1f * scaleFactor); // 50 - 15 = 35  
        var expectedYNeg1 = centerY - (-1f * scaleFactor); // 50 + 15 = 65
        var expectedYNeg2 = centerY - (-2f * scaleFactor); // 50 + 30 = 80

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should draw symmetrical horizontal lines
        canvas.Verify(c => c.DrawLine(20f, expectedYPos2, 80f, expectedYPos2), Times.Once);
        canvas.Verify(c => c.DrawLine(20f, expectedYPos1, 80f, expectedYPos1), Times.Once);
        canvas.Verify(c => c.DrawLine(20f, expectedYNeg1, 80f, expectedYNeg1), Times.Once);
        canvas.Verify(c => c.DrawLine(20f, expectedYNeg2, 80f, expectedYNeg2), Times.Once);
    }

    private MoodVisualizationData CreateTestData(double maxAbsoluteValue)
    {
        return new MoodVisualizationData
        {
            DailyValues = Array.Empty<DailyMoodValue>(),
            MaxAbsoluteValue = maxAbsoluteValue
        };
    }
}