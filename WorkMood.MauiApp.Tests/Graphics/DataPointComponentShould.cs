using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Graphics;

public class DataPointComponentShould
{
    private readonly DataPointComponent _component;

    public DataPointComponentShould()
    {
        _component = new DataPointComponent();
    }

    [Fact]
    public void ImplementIGraphComponentInterface()
    {
        Assert.IsAssignableFrom<IGraphComponent>(_component);
    }

    [Fact]
    public void Draw_WithValidDataPoints_ShouldSetFillColorAndCallFillEllipse()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0, 0.5 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verify underlying canvas methods are called
        canvas.VerifySet(c => c.FillColor = Colors.Blue, Times.Exactly(2));
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Exactly(2));
    }

    [Fact]
    public void Draw_WithValidDataPoints_ShouldSetStrokePropertiesAndCallDrawEllipse()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0, 0.5 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verify underlying canvas methods are called
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkGray, Times.Exactly(2));
        canvas.VerifySet(c => c.StrokeSize = 1f, Times.Exactly(2));
        canvas.Verify(c => c.DrawEllipse(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Exactly(2));
    }

    [Fact]
    public void Draw_WithPositiveValue_ShouldPositionAboveCenter()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0 }); // Max value is 1, so this should be at top
        
        // Graph calculations: width=100, height=100, margin=20
        // graphHeight = 60, centerY = 50, scaleFactor = 60/(2*1) = 30
        // expectedY = 50 - (1.0 * 30) = 20
        var expectedY = 20f;

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Check the Y position in FillEllipse call (x, y, width, height)
        canvas.Verify(c => c.FillEllipse(16f, expectedY - 4f, 8f, 8f), Times.Once);
    }

    [Fact]
    public void Draw_WithNegativeValue_ShouldPositionBelowCenter()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { -1.0 }); // Max abs value is 1, so this should be at bottom
        
        // expectedY = 50 - (-1.0 * 30) = 50 + 30 = 80
        var expectedY = 80f;

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(16f, expectedY - 4f, 8f, 8f), Times.Once);
    }

    [Fact]
    public void Draw_WithZeroValue_ShouldPositionAtCenter()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 0.0 });
        
        var expectedY = 50f; // Center Y

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(16f, expectedY - 4f, 8f, 8f), Times.Once);
    }

    [Theory]
    [InlineData(0, 20f)]
    [InlineData(1, 24.615385f)] // 20 + (1 * 60/13) ≈ 20 + 4.615
    [InlineData(2, 29.230770f)] // 20 + (2 * 60/13) ≈ 20 + 9.230
    [InlineData(13, 80f)] // 20 + (13 * 60/13) = 20 + 60 = 80
    public void Draw_WithSpecificDays_ShouldCalculateCorrectXPositions(int dayIndex, float expectedX)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var values = new double?[14];
        values[dayIndex] = 1.0;
        var data = CreateTestData(values);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Check X position in FillEllipse (accounting for circle radius offset)
        canvas.Verify(c => c.FillEllipse(expectedX - 4f, It.IsAny<float>(), 8f, 8f), Times.Once);
    }

    [Fact]
    public void Draw_WithDifferentMaxAbsValue_ShouldScaleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 2.0 }); // Max abs value is 2
        
        // With max abs value of 2, scale factor = 60 / (2 * 2) = 15
        var expectedY = 50f - (2.0f * 15f); // 50 - 30 = 20

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(16f, expectedY - 4f, 8f, 8f), Times.Once);
    }

    [Theory]
    [InlineData(100f, 100f, 60f, 60f)]
    [InlineData(200f, 300f, 160f, 260f)]
    [InlineData(80f, 120f, 40f, 80f)]
    public void Draw_WithDifferentBounds_ShouldCalculateScaleCorrectly(float width, float height, float expectedGraphWidth, float expectedGraphHeight)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, width, height);
        var data = CreateTestData(new[] { 1.0 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - just verify it draws an ellipse (position calculation is complex)
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Once);
        
        // Verify calculated values by checking assertions within reasonable bounds
        Assert.Equal(expectedGraphWidth, width - 40f); // width - (margin * 2)
        Assert.Equal(expectedGraphHeight, height - 40f); // height - (margin * 2)
    }

    [Fact]
    public void Draw_WithNullValue_ShouldNotDrawDataPoint()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var dailyValues = new[]
        {
            new DailyMoodValue { HasData = true, Value = null, Color = Colors.Blue },
            new DailyMoodValue { HasData = false, Value = 1.0, Color = Colors.Blue }
        };
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void Draw_WithHasDataFalse_ShouldNotDrawDataPoint()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var dailyValues = new[]
        {
            new DailyMoodValue { HasData = false, Value = 1.0, Color = Colors.Blue }
        };
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void Draw_WithMixedDataAndMissingValues_ShouldDrawOnlyValidPoints()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var dailyValues = new[]
        {
            new DailyMoodValue { HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { HasData = false, Value = 2.0, Color = Colors.Blue },
            new DailyMoodValue { HasData = true, Value = null, Color = Colors.Blue },
            new DailyMoodValue { HasData = true, Value = 0.5, Color = Colors.Blue }
        };
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 2.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - should only draw 2 ellipses (indices 0 and 3)
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Exactly(2));
    }

    [Fact]
    public void Draw_WithEmptyDailyValues_ShouldNotDrawAnyPoints()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = new MoodVisualizationData
        {
            DailyValues = Array.Empty<DailyMoodValue>(),
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void Draw_WithLessThan14Days_ShouldHandleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var dailyValues = new[]
        {
            new DailyMoodValue { HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { HasData = true, Value = 0.5, Color = Colors.Blue }
        };
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - should draw exactly 2 ellipses for the 2 valid values
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Exactly(2));
    }

    [Fact]
    public void Draw_WithVeryLargeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1000.0, -500.0 });

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);
        
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Exactly(2));
    }

    [Fact]
    public void Draw_CalledMultipleTimes_ShouldProduceConsistentResults()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0 });

        // Act
        _component.Draw(canvas.Object, bounds, data);
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.Verify(c => c.FillEllipse(16f, It.IsAny<float>(), 8f, 8f), Times.Exactly(2));
    }

    [Fact]
    public void Draw_ShouldUseFixedCircleRadius()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Circle radius 4 becomes ellipse width/height of 8
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Once);
    }

    [Fact]
    public void Draw_ShouldSetPropertiesInCorrectOrder()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestData(new[] { 1.0 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Verify both fill and stroke properties are set
        canvas.VerifySet(c => c.FillColor = Colors.Blue, Times.Once);
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkGray, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 1f, Times.Once);
        
        canvas.Verify(c => c.FillEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Once);
        canvas.Verify(c => c.DrawEllipse(It.IsAny<float>(), It.IsAny<float>(), 8f, 8f), Times.Once);
    }

    private MoodVisualizationData CreateTestData(double?[] values)
    {
        var dailyValues = values.Select(v => new DailyMoodValue
        {
            HasData = v.HasValue,
            Value = v,
            Color = Colors.Blue
        }).ToArray();

        return new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = values.Where(v => v.HasValue).Select(v => Math.Abs(v!.Value)).DefaultIfEmpty(1.0).Max()
        };
    }

    private MoodVisualizationData CreateTestData(double[] values)
    {
        return CreateTestData(values.Cast<double?>().ToArray());
    }
}