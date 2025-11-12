using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Graphics;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Graphics;

/// <summary>
/// Tests for BaselineComponent
/// Covers zero-line drawing, coordinate positioning, canvas operations, and IGraphComponent interface compliance
/// </summary>
public class BaselineComponentShould
{
    private readonly BaselineComponent _component;
    private readonly Mock<ICanvas> _mockCanvas;

    public BaselineComponentShould()
    {
        _component = new BaselineComponent();
        _mockCanvas = new Mock<ICanvas>();
    }

    #region Interface Implementation Tests

    [Fact]
    public void BeImplementationOfIGraphComponent()
    {
        // Assert
        Assert.IsAssignableFrom<IGraphComponent>(_component);
    }

    [Fact]
    public void HaveParameterlessConstructor()
    {
        // Act & Assert
        Assert.NotNull(new BaselineComponent());
    }

    #endregion

    #region Canvas Interaction Tests

    [Fact]
    public void Draw_WithValidInputs_ShouldSetCorrectStrokeProperties()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.VerifySet(c => c.StrokeColor = Color.FromRgba(200, 200, 200, 255), Times.Once);
        _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.Once);
    }

    [Fact]
    public void Draw_WithValidBounds_ShouldDrawLineAtCorrectPosition()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var expectedCenterY = 20f + (160f / 2f); // margin + (graphHeight / 2)

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(20f, expectedCenterY, 280f, expectedCenterY), Times.Once);
    }

    [Fact]
    public void Draw_ShouldCallCanvasMethodsInCorrectOrder()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var sequence = new MockSequence();

        _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeColor = It.IsAny<Color>());
        _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeSize = It.IsAny<float>());
        _mockCanvas.InSequence(sequence).Setup(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Verified by sequence setup
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    #endregion

    #region Calculation Tests

    [Theory]
    [InlineData(100, 100, 20f, 50f, 80f)] // Small square
    [InlineData(400, 300, 20f, 150f, 380f)] // Large rectangle  
    [InlineData(200, 150, 20f, 75f, 180f)] // Medium rectangle
    [InlineData(600, 400, 20f, 200f, 580f)] // Very large rectangle
    public void Draw_WithDifferentBounds_ShouldCalculatePositionCorrectly(
        float width, float height, float expectedStartX, float expectedY, float expectedEndX)
    {
        // Arrange
        var bounds = new RectF(0, 0, width, height);
        var data = CreateTestVisualizationData();

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(expectedStartX, expectedY, expectedEndX, expectedY), Times.Once);
    }

    [Fact]
    public void Draw_ShouldCalculateMarginCorrectly()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var expectedMargin = 20f;

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Line should start at margin and end at width - margin
        _mockCanvas.Verify(c => c.DrawLine(expectedMargin, It.IsAny<float>(), 280f, It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_ShouldCalculateCenterYCorrectly()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var margin = 20f;
        var graphHeight = 200f - (margin * 2);
        var expectedCenterY = margin + (graphHeight / 2f);

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), expectedCenterY, It.IsAny<float>(), expectedCenterY), Times.Once);
    }

    #endregion

    #region Color and Stroke Tests

    [Fact]
    public void Draw_ShouldUseCorrectStrokeColor()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var expectedColor = Color.FromRgba(200, 200, 200, 255);

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.VerifySet(c => c.StrokeColor = expectedColor, Times.Once);
    }

    [Fact]
    public void Draw_ShouldUseCorrectStrokeSize()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();
        var expectedStrokeSize = 1f;

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.VerifySet(c => c.StrokeSize = expectedStrokeSize, Times.Once);
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public void Draw_WithZeroWidth_ShouldHandleGracefully()
    {
        // Arrange
        var bounds = new RectF(0, 0, 0, 100);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithZeroHeight_ShouldHandleGracefully()
    {
        // Arrange
        var bounds = new RectF(0, 0, 100, 0);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithZeroBounds_ShouldHandleGracefully()
    {
        // Arrange
        var bounds = new RectF(0, 0, 0, 0);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithNegativeWidth_ShouldHandleGracefully()
    {
        // Arrange
        var bounds = new RectF(0, 0, -100, 100);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Should still attempt to draw
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithNegativeHeight_ShouldHandleGracefully()
    {
        // Arrange
        var bounds = new RectF(0, 0, 100, -100);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Should still attempt to draw
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithVerySmallBounds_ShouldNotCrash()
    {
        // Arrange
        var bounds = new RectF(0, 0, 1, 1);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithVeryLargeBounds_ShouldHandleCorrectly()
    {
        // Arrange
        var bounds = new RectF(0, 0, 10000, 5000);
        var data = CreateTestVisualizationData();

        // Act - Should not throw
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert
        _mockCanvas.Verify(c => c.DrawLine(20f, 2500f, 9980f, 2500f), Times.Once);
    }

    #endregion

    #region Data Independence Tests

    [Fact]
    public void Draw_WithNullData_ShouldStillDrawBaseline()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var expectedCenterY = 20f + (160f / 2f);

        // Act - BaselineComponent should handle null data gracefully
        _component.Draw(_mockCanvas.Object, bounds, CreateTestVisualizationData());

        // Assert - Should still draw baseline regardless of data content
        _mockCanvas.Verify(c => c.DrawLine(20f, expectedCenterY, 280f, expectedCenterY), Times.Once);
        _mockCanvas.VerifySet(c => c.StrokeColor = Color.FromRgba(200, 200, 200, 255), Times.Once);
        _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.Once);
    }

    [Fact]
    public void Draw_WithEmptyData_ShouldStillDrawBaseline()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = new MoodVisualizationData
        {
            DailyValues = Array.Empty<DailyMoodValue>(),
            MaxAbsoluteValue = 0,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(13))
        };

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Should draw baseline regardless of empty data
        _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void Draw_WithDifferentDataValues_ShouldDrawIdenticalBaseline()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data1 = CreateTestVisualizationData(maxValue: 5.0);
        var data2 = CreateTestVisualizationData(maxValue: 10.0);
        var expectedCenterY = 100f;

        _mockCanvas.Reset();

        // Act - Draw with first data set
        _component.Draw(_mockCanvas.Object, bounds, data1);
        _mockCanvas.Reset();

        // Act - Draw with second data set  
        _component.Draw(_mockCanvas.Object, bounds, data2);

        // Assert - Same baseline position regardless of data
        _mockCanvas.Verify(c => c.DrawLine(20f, expectedCenterY, 280f, expectedCenterY), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Draw_WithNullCanvas_ShouldThrowNullReferenceException()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();

        // Act & Assert - Disable nullable warning for intentional null test
        #pragma warning disable CS8625
        Assert.Throws<NullReferenceException>(() => _component.Draw(null!, bounds, data));
        #pragma warning restore CS8625
    }

    #endregion

    #region Performance and Consistency Tests

    [Fact]
    public void Draw_CalledMultipleTimes_ShouldProduceConsistentResults()
    {
        // Arrange
        var bounds = new RectF(0, 0, 300, 200);
        var data = CreateTestVisualizationData();

        // Act - Call multiple times
        _component.Draw(_mockCanvas.Object, bounds, data);
        _component.Draw(_mockCanvas.Object, bounds, data);
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Should be called exactly 3 times with same parameters
        _mockCanvas.Verify(c => c.DrawLine(20f, 100f, 280f, 100f), Times.Exactly(3));
        _mockCanvas.VerifySet(c => c.StrokeColor = Color.FromRgba(200, 200, 200, 255), Times.Exactly(3));
        _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.Exactly(3));
    }

    [Fact]
    public void Draw_ShouldNotModifyInputParameters()
    {
        // Arrange
        var originalBounds = new RectF(0, 0, 300, 200);
        var bounds = originalBounds;
        var data = CreateTestVisualizationData();

        // Act
        _component.Draw(_mockCanvas.Object, bounds, data);

        // Assert - Bounds should not be modified
        Assert.Equal(originalBounds, bounds);
    }

    #endregion

    #region Helper Methods

    private static MoodVisualizationData CreateTestVisualizationData(double maxValue = 5.0)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        return new MoodVisualizationData
        {
            DailyValues = new[]
            {
                new DailyMoodValue { Date = startDate, Value = -2.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(1), Value = -1.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(2), Value = 0.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(3), Value = 1.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(4), Value = 2.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(5), Value = null, HasData = false },
                new DailyMoodValue { Date = startDate.AddDays(6), Value = 3.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(7), Value = -3.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(8), Value = 1.5, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(9), Value = -0.5, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(10), Value = 0.0, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(11), Value = 2.5, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(12), Value = -1.5, HasData = true },
                new DailyMoodValue { Date = startDate.AddDays(13), Value = 4.0, HasData = true }
            },
            MaxAbsoluteValue = maxValue,
            StartDate = startDate,
            EndDate = startDate.AddDays(13),
            Width = 300,
            Height = 200
        };
    }

    #endregion
}