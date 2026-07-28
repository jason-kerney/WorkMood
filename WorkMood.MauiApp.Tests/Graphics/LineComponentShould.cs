using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Graphics;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Graphics;

public class LineComponentShould
{
    private readonly LineComponent _component;

    public LineComponentShould()
    {
        _component = new LineComponent();
    }

    [Fact]
    public void ImplementIGraphComponentInterface()
    {
        Assert.IsAssignableFrom<IGraphComponent>(_component);
    }

    [Fact]
    public void Draw_WithTwoOrMoreDataPoints_ShouldSetCorrectStrokeProperties()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(new double[] { 1.0, 0.5 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Fact]
    public void Draw_WithOneDataPoint_ShouldNotSetStrokeProperties()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(new double[] { 1.0 });

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - No lines to draw, so stroke properties shouldn't be set
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Never);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Never);
    }

    [Fact]
    public void Draw_WithNoDataPoints_ShouldNotSetStrokeProperties()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(new double[0]);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - No data means no stroke properties set
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Never);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Never);
    }

    [Fact]
    public void Draw_WithEmptyDailyValues_ShouldNotSetStrokeProperties()
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
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Never);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Never);
    }

    [Fact]
    public void Draw_CalledMultipleTimes_ShouldSetStrokePropertiesEachTime()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(new double[] { 1.0, 0.5 });

        // Act
        _component.Draw(canvas.Object, bounds, data);
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should set stroke properties twice
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Exactly(2));
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Exactly(2));
    }

    [Fact]
    public void Draw_WithMixedValidAndInvalidData_ShouldSetStrokePropertiesWhenValidDataExists()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        
        var dailyValues = new DailyMoodValue[6];
        dailyValues[0] = new DailyMoodValue { HasData = true, Value = 1.0, Color = Colors.Blue }; // Valid
        dailyValues[1] = new DailyMoodValue { HasData = false, Value = 0.5, Color = Colors.Blue }; // Invalid
        dailyValues[2] = new DailyMoodValue { HasData = true, Value = null, Color = Colors.Blue }; // Invalid
        dailyValues[3] = new DailyMoodValue { HasData = true, Value = -0.5, Color = Colors.Blue }; // Valid
        dailyValues[4] = new DailyMoodValue { HasData = false, Value = null, Color = Colors.Blue }; // Invalid
        dailyValues[5] = new DailyMoodValue { HasData = true, Value = 0.8, Color = Colors.Blue }; // Valid
        
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should set stroke properties once since there are valid points to connect
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Fact]
    public void Draw_WithLessThan14Days_ShouldHandleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        
        var dailyValues = new DailyMoodValue[3]; // Less than 14 days
        dailyValues[0] = new DailyMoodValue { HasData = true, Value = 1.0, Color = Colors.Blue };
        dailyValues[1] = new DailyMoodValue { HasData = true, Value = 0.5, Color = Colors.Blue };
        dailyValues[2] = new DailyMoodValue { HasData = true, Value = -0.5, Color = Colors.Blue };
        
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 1.0
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should set stroke properties for line drawing
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Fact]
    public void Draw_WithVeryLargeValues_ShouldHandleCorrectlyWithoutException()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(new double[] { 1000.0, -500.0 });

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);
        
        // Should set stroke properties for line drawing
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Fact]
    public void Draw_WithZeroMaxAbsValue_ShouldUseMinimumValueOfOne()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = new MoodVisualizationData 
        { 
            DailyValues = new[]
            {
                new DailyMoodValue { HasData = true, Value = 0.0, Color = Colors.Blue },
                new DailyMoodValue { HasData = true, Value = 0.0, Color = Colors.Blue }
            },
            MaxAbsoluteValue = 0.0 // Should default to 1.0 in Math.Max(1.0, 0.0)
        };

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert - Should set stroke properties since there are two valid points to connect
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Theory]
    [InlineData(new double[] { 1.0, 0.5, -0.5, 0.0, 0.8 })] // 5 points should connect with lines
    [InlineData(new double[] { 1.0, 0.5, -0.5 })] // 3 points should connect with lines
    [InlineData(new double[] { 1.0, 0.5 })] // 2 points should connect with lines
    public void Draw_WithMultipleDataPoints_ShouldSetStrokeProperties(double[] values)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(values);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Theory]
    [InlineData(new double[] { 1.0 })] // 1 point cannot be connected
    [InlineData(new double[0])] // 0 points cannot be connected  
    public void Draw_WithInsufficientDataPoints_ShouldNotSetStrokeProperties(double[] values)
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        var data = CreateTestDataWithValues(values);

        // Act
        _component.Draw(canvas.Object, bounds, data);

        // Assert
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Never);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Never);
    }

    [Fact] 
    public void Draw_WithArrayBoundsSafety_ShouldHandleCorrectly()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 100, 100);
        
        // Create array with 20 items but the loop should only process first 14
        var dailyValues = new DailyMoodValue[20];
        for (int i = 0; i < 20; i++)
        {
            dailyValues[i] = new DailyMoodValue 
            { 
                HasData = true, 
                Value = i * 0.1, 
                Color = Colors.Blue 
            };
        }
        
        var data = new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = 2.0
        };

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);
        
        // Should set stroke properties since there are points to connect
        canvas.VerifySet(c => c.StrokeColor = Colors.DarkBlue, Times.Once);
        canvas.VerifySet(c => c.StrokeSize = 2f, Times.Once);
    }

    [Fact]
    public void Draw_WithBoundaryConditions_ShouldNotThrow()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(0, 0, 0, 0); // Zero bounds
        var data = CreateTestDataWithValues(new double[] { 1.0, -1.0 });

        // Act & Assert - Should handle zero bounds gracefully
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);
    }

    [Fact]
    public void Draw_WithNegativeBounds_ShouldNotThrow()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var bounds = new RectF(-10, -10, 50, 50); // Negative position
        var data = CreateTestDataWithValues(new double[] { 1.0, -1.0 });

        // Act & Assert - Should handle negative bounds gracefully
        var exception = Record.Exception(() => _component.Draw(canvas.Object, bounds, data));
        Assert.Null(exception);
    }

    private MoodVisualizationData CreateTestDataWithValues(double[] values)
    {
        var dailyValues = values.Select(v => new DailyMoodValue
        {
            HasData = true,
            Value = v,
            Color = Colors.Blue
        }).ToArray();

        return new MoodVisualizationData
        {
            DailyValues = dailyValues,
            MaxAbsoluteValue = values.Length > 0 ? values.Select(Math.Abs).Max() : 1.0
        };
    }

    #region Calendar Gap Tests

    // bounds width 170 with margin 20 -> graphWidth 130 -> pointSpacing (130/13) = 10,
    // giving clean whole-number x coordinates per day index: day N -> x = 20 + (N * 10).
    private static readonly RectF GapTestBounds = new(0, 0, 170, 100);

    [Fact]
    public void Draw_WithConsecutiveRecordedDays_ShouldConnectThem()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var baseDate = new DateOnly(2025, 1, 1);

        var dailyValues = new[]
        {
            new DailyMoodValue { Date = baseDate, HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { Date = baseDate.AddDays(1), HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { Date = baseDate.AddDays(2), HasData = true, Value = 1.0, Color = Colors.Blue }
        };

        var data = new MoodVisualizationData { DailyValues = dailyValues, MaxAbsoluteValue = 1.0 };

        // Act
        _component.Draw(canvas.Object, GapTestBounds, data);

        // Assert - all three consecutive recorded days connect
        canvas.Verify(c => c.DrawLine(20f, 20f, 30f, 20f), Times.Once);
        canvas.Verify(c => c.DrawLine(30f, 20f, 40f, 20f), Times.Once);
    }

    [Fact]
    public void Draw_WithMissingDayBetweenRecordedDays_ShouldNotConnectAcrossTheGap()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var baseDate = new DateOnly(2025, 1, 1);

        var dailyValues = new[]
        {
            new DailyMoodValue { Date = baseDate, HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { Date = baseDate.AddDays(1), HasData = true, Value = 1.0, Color = Colors.Blue },
            new DailyMoodValue { Date = baseDate.AddDays(2), HasData = false }, // missing day
            new DailyMoodValue { Date = baseDate.AddDays(3), HasData = true, Value = 1.0, Color = Colors.Blue }
        };

        var data = new MoodVisualizationData { DailyValues = dailyValues, MaxAbsoluteValue = 1.0 };

        // Act
        _component.Draw(canvas.Object, GapTestBounds, data);

        // Assert - the two recorded days before the gap still connect to each other
        canvas.Verify(c => c.DrawLine(20f, 20f, 30f, 20f), Times.Once);

        // Assert - neither recorded day connects across the missing day
        canvas.Verify(c => c.DrawLine(30f, 20f, 50f, 20f), Times.Never);
        canvas.Verify(c => c.DrawLine(20f, 20f, 50f, 20f), Times.Never);
    }

    [Fact]
    public void Draw_WithMultipleGapsInOneSeries_ShouldBreakAtEveryGap()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var baseDate = new DateOnly(2025, 1, 1);

        var dailyValues = new[]
        {
            new DailyMoodValue { Date = baseDate, HasData = true, Value = 1.0, Color = Colors.Blue },          // day 0, x=20
            new DailyMoodValue { Date = baseDate.AddDays(1), HasData = false },                                 // day 1, missing
            new DailyMoodValue { Date = baseDate.AddDays(2), HasData = true, Value = 1.0, Color = Colors.Blue }, // day 2, x=40
            new DailyMoodValue { Date = baseDate.AddDays(3), HasData = true, Value = 1.0, Color = Colors.Blue }, // day 3, x=50
            new DailyMoodValue { Date = baseDate.AddDays(4), HasData = false },                                 // day 4, missing
            new DailyMoodValue { Date = baseDate.AddDays(5), HasData = true, Value = 1.0, Color = Colors.Blue }  // day 5, x=70
        };

        var data = new MoodVisualizationData { DailyValues = dailyValues, MaxAbsoluteValue = 1.0 };

        // Act
        _component.Draw(canvas.Object, GapTestBounds, data);

        // Assert - middle pair (day 2 -> day 3) connects
        canvas.Verify(c => c.DrawLine(40f, 20f, 50f, 20f), Times.Once);

        // Assert - neither boundary gap is bridged
        canvas.Verify(c => c.DrawLine(20f, 20f, 40f, 20f), Times.Never); // day 0 -> day 2 (skips day 1)
        canvas.Verify(c => c.DrawLine(50f, 20f, 70f, 20f), Times.Never); // day 3 -> day 5 (skips day 4)
    }

    [Fact]
    public void Draw_WithGapAtRangeStart_ShouldNotConnectFirstIsolatedPoint()
    {
        // Arrange
        var canvas = new Mock<ICanvas>();
        var baseDate = new DateOnly(2025, 1, 1);

        var dailyValues = new[]
        {
            new DailyMoodValue { Date = baseDate, HasData = true, Value = 1.0, Color = Colors.Blue },          // day 0, isolated
            new DailyMoodValue { Date = baseDate.AddDays(1), HasData = false },                                 // day 1, missing
            new DailyMoodValue { Date = baseDate.AddDays(2), HasData = true, Value = 1.0, Color = Colors.Blue }, // day 2
            new DailyMoodValue { Date = baseDate.AddDays(3), HasData = true, Value = 1.0, Color = Colors.Blue }  // day 3
        };

        var data = new MoodVisualizationData { DailyValues = dailyValues, MaxAbsoluteValue = 1.0 };

        // Act
        _component.Draw(canvas.Object, GapTestBounds, data);

        // Assert - the trailing pair still connects
        canvas.Verify(c => c.DrawLine(40f, 20f, 50f, 20f), Times.Once);

        // Assert - the isolated leading point is never used as a line endpoint
        canvas.Verify(c => c.DrawLine(20f, 20f, 40f, 20f), Times.Never);
    }

    #endregion
}