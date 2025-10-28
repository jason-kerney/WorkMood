using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Strategies;
using Xunit;

namespace WorkMood.MauiApp.Tests.Strategies;

/// <summary>
/// Tests for DefaultMoodColorStrategy
/// </summary>
public class DefaultMoodColorStrategyShould
{
    private readonly DefaultMoodColorStrategy _strategy;

    public DefaultMoodColorStrategyShould()
    {
        _strategy = new DefaultMoodColorStrategy();
    }

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ShouldImplementInterface()
    {
        // Assert
        Assert.IsAssignableFrom<IMoodColorStrategy>(_strategy);
    }

    #endregion

    #region Positive Value Tests (Green Shades)

    [Fact]
    public void GetColorForValue_ShouldReturnGreenShade_WhenValueIsPositive()
    {
        // Arrange
        double positiveValue = 2.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(positiveValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Green); // Should have full green component
        Assert.True(result.Red < 1.0f);   // Red should be reduced for green shade
        Assert.True(result.Blue < 1.0f);  // Blue should be reduced for green shade
    }

    [Fact]
    public void GetColorForValue_ShouldReturnMoreIntenseGreen_WhenValueIsHigherPositive()
    {
        // Arrange
        double lowerPositive = 1.0;
        double higherPositive = 3.0;
        double maxAbsValue = 5.0;

        // Act
        var lowerResult = _strategy.GetColorForValue(lowerPositive, maxAbsValue);
        var higherResult = _strategy.GetColorForValue(higherPositive, maxAbsValue);

        // Assert
        Assert.NotNull(lowerResult);
        Assert.NotNull(higherResult);
        
        // Higher positive values should have more intense green (darker green)
        Assert.True(higherResult.Red < lowerResult.Red);
        Assert.True(higherResult.Blue < lowerResult.Blue);
        Assert.Equal(1.0f, lowerResult.Green);
        Assert.Equal(1.0f, higherResult.Green);
    }

    [Theory]
    [InlineData(0.5, 5.0)]
    [InlineData(1.0, 5.0)]
    [InlineData(2.5, 5.0)]
    [InlineData(4.0, 5.0)]
    public void GetColorForValue_ShouldReturnValidGreenShade_ForVariousPositiveValues(double value, double maxAbsValue)
    {
        // Act
        var result = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Green);
        Assert.InRange(result.Red, 0.2f, 0.8f);   // Should be within expected range
        Assert.InRange(result.Blue, 0.2f, 0.8f);  // Should be within expected range
    }

    [Fact]
    public void GetColorForValue_ShouldHandleMaxPositiveValue_Correctly()
    {
        // Arrange
        double maxValue = 5.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(maxValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Green);
        Assert.Equal(0.2f, result.Red);   // Should be at minimum (most intense green)
        Assert.Equal(0.2f, result.Blue);  // Should be at minimum (most intense green)
    }

    #endregion

    #region Negative Value Tests (Red Shades)

    [Fact]
    public void GetColorForValue_ShouldReturnRedShade_WhenValueIsNegative()
    {
        // Arrange
        double negativeValue = -2.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(negativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);   // Should have full red component
        Assert.True(result.Green < 1.0f); // Green should be reduced for red shade
        Assert.True(result.Blue < 1.0f);  // Blue should be reduced for red shade
    }

    [Fact]
    public void GetColorForValue_ShouldReturnMoreIntenseRed_WhenValueIsLowerNegative()
    {
        // Arrange
        double higherNegative = -1.0;  // Less negative (closer to zero)
        double lowerNegative = -3.0;   // More negative (further from zero)
        double maxAbsValue = 5.0;

        // Act
        var higherResult = _strategy.GetColorForValue(higherNegative, maxAbsValue);
        var lowerResult = _strategy.GetColorForValue(lowerNegative, maxAbsValue);

        // Assert
        Assert.NotNull(higherResult);
        Assert.NotNull(lowerResult);
        
        // More negative values should have more intense red (darker red)
        Assert.True(lowerResult.Green < higherResult.Green);
        Assert.True(lowerResult.Blue < higherResult.Blue);
        Assert.Equal(1.0f, higherResult.Red);
        Assert.Equal(1.0f, lowerResult.Red);
    }

    [Theory]
    [InlineData(-0.5, 5.0)]
    [InlineData(-1.0, 5.0)]
    [InlineData(-2.5, 5.0)]
    [InlineData(-4.0, 5.0)]
    public void GetColorForValue_ShouldReturnValidRedShade_ForVariousNegativeValues(double value, double maxAbsValue)
    {
        // Act
        var result = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);
        Assert.InRange(result.Green, 0.2f, 0.8f);  // Should be within expected range
        Assert.InRange(result.Blue, 0.2f, 0.8f);   // Should be within expected range
    }

    [Fact]
    public void GetColorForValue_ShouldHandleMaxNegativeValue_Correctly()
    {
        // Arrange
        double maxNegativeValue = -5.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(maxNegativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);
        Assert.Equal(0.2f, result.Green); // Should be at minimum (most intense red)
        Assert.Equal(0.2f, result.Blue);  // Should be at minimum (most intense red)
    }

    #endregion

    #region Zero Value Tests (Neutral)

    [Fact]
    public void GetColorForValue_ShouldReturnNeutralBlue_WhenValueIsZero()
    {
        // Arrange
        double zeroValue = 0.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(zeroValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.5f, result.Red);
        Assert.Equal(0.7f, result.Green);
        Assert.Equal(1.0f, result.Blue);
    }

    [Fact]
    public void GetColorForValue_ShouldReturnSameNeutralColor_ForDifferentMaxAbsValues()
    {
        // Arrange
        double zeroValue = 0.0;
        double maxAbsValue1 = 3.0;
        double maxAbsValue2 = 10.0;

        // Act
        var result1 = _strategy.GetColorForValue(zeroValue, maxAbsValue1);
        var result2 = _strategy.GetColorForValue(zeroValue, maxAbsValue2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Red, result2.Red);
        Assert.Equal(result1.Green, result2.Green);
        Assert.Equal(result1.Blue, result2.Blue);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void GetColorForValue_ShouldHandleVerySmallPositiveValue()
    {
        // Arrange
        double smallValue = 0.001;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(smallValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Green);
        Assert.True(result.Red > 0.2f);  // Should be close to light green
        Assert.True(result.Blue > 0.2f); // Should be close to light green
    }

    [Fact]
    public void GetColorForValue_ShouldHandleVerySmallNegativeValue()
    {
        // Arrange
        double smallNegativeValue = -0.001;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(smallNegativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);
        Assert.True(result.Green > 0.2f); // Should be close to light red
        Assert.True(result.Blue > 0.2f);  // Should be close to light red
    }

    [Fact]
    public void GetColorForValue_ShouldCapIntensityAtMaxValue()
    {
        // Arrange - Value exceeds maxAbsValue
        double largeValue = 10.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(largeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Green);
        Assert.Equal(0.2f, result.Red);   // Should be capped at most intense
        Assert.Equal(0.2f, result.Blue);  // Should be capped at most intense
    }

    [Fact]
    public void GetColorForValue_ShouldCapIntensityAtMaxNegativeValue()
    {
        // Arrange - Absolute value exceeds maxAbsValue
        double largeNegativeValue = -10.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(largeNegativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);
        Assert.Equal(0.2f, result.Green); // Should be capped at most intense
        Assert.Equal(0.2f, result.Blue);  // Should be capped at most intense
    }

    [Fact]
    public void GetColorForValue_ShouldHandleZeroMaxAbsValue()
    {
        // Arrange
        double value = 1.0;
        double zeroMaxAbsValue = 0.0;

        // Act
        var result = _strategy.GetColorForValue(value, zeroMaxAbsValue);

        // Assert
        Assert.NotNull(result);
        // Should handle division by zero gracefully and return most intense color
        Assert.Equal(1.0f, result.Green);
        Assert.Equal(0.2f, result.Red);
        Assert.Equal(0.2f, result.Blue);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 2.0)]
    [InlineData(-3.0, 3.0)]
    [InlineData(-1.5, 1.5)]
    public void GetColorForValue_ShouldHandleValueEqualToMaxAbsValue(double value, double maxAbsValue)
    {
        // Act
        var result = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        
        if (value > 0)
        {
            Assert.Equal(1.0f, result.Green);
            Assert.Equal(0.2f, result.Red);   // Most intense green
            Assert.Equal(0.2f, result.Blue);
        }
        else if (value < 0)
        {
            Assert.Equal(1.0f, result.Red);
            Assert.Equal(0.2f, result.Green); // Most intense red
            Assert.Equal(0.2f, result.Blue);
        }
    }

    #endregion

    #region Color Consistency Tests

    [Fact]
    public void GetColorForValue_ShouldProduceConsistentResults_ForSameInputs()
    {
        // Arrange
        double value = 2.5;
        double maxAbsValue = 5.0;

        // Act
        var result1 = _strategy.GetColorForValue(value, maxAbsValue);
        var result2 = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.Equal(result1.Red, result2.Red);
        Assert.Equal(result1.Green, result2.Green);
        Assert.Equal(result1.Blue, result2.Blue);
    }

    [Fact]
    public void GetColorForValue_ShouldProduceSymmetricalIntensity_ForOppositeValues()
    {
        // Arrange
        double positiveValue = 3.0;
        double negativeValue = -3.0;
        double maxAbsValue = 5.0;

        // Act
        var positiveResult = _strategy.GetColorForValue(positiveValue, maxAbsValue);
        var negativeResult = _strategy.GetColorForValue(negativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(positiveResult);
        Assert.NotNull(negativeResult);
        
        // Intensity should be symmetrical - same distance from neutral components
        Assert.Equal(positiveResult.Red, negativeResult.Green); // Green component of positive = Green component of negative
        Assert.Equal(positiveResult.Blue, negativeResult.Blue); // Blue components should be equal
    }

    [Fact]
    public void GetColorForValue_ShouldCreateMonotonicColorProgression_ForIncreasingValues()
    {
        // Arrange
        double[] values = { -4.0, -2.0, 0.0, 2.0, 4.0 };
        double maxAbsValue = 5.0;

        // Act
        var colors = values.Select(v => _strategy.GetColorForValue(v, maxAbsValue)).ToArray();

        // Assert
        Assert.Equal(5, colors.Length);
        
        // Verify progression makes sense
        // Most negative should be most intense red
        Assert.Equal(1.0f, colors[0].Red);
        Assert.Equal(0.2f + (1.0 - 0.8) * 0.6f, colors[0].Green, precision: 2);
        
        // Zero should be neutral
        Assert.Equal(0.5f, colors[2].Red);
        Assert.Equal(0.7f, colors[2].Green);
        Assert.Equal(1.0f, colors[2].Blue);
        
        // Most positive should be most intense green
        Assert.Equal(1.0f, colors[4].Green);
        Assert.Equal(0.2f + (1.0 - 0.8) * 0.6f, colors[4].Red, precision: 2);
    }

    #endregion

    #region Performance and Robustness Tests

    [Fact]
    public void GetColorForValue_ShouldHandleLargeNumberOfCalls_Efficiently()
    {
        // Arrange
        const int iterations = 10000;
        double maxAbsValue = 5.0;

        // Act & Assert - Should complete without performance issues
        for (int i = 0; i < iterations; i++)
        {
            double value = (i % 11) - 5; // Values from -5 to 5
            var result = _strategy.GetColorForValue(value, maxAbsValue);
            Assert.NotNull(result);
        }
    }

    [Theory]
    [InlineData(double.MaxValue, 1.0)]
    [InlineData(double.MinValue, 1.0)]
    [InlineData(1.0, double.MaxValue)]
    public void GetColorForValue_ShouldHandleExtremeValues_Gracefully(double value, double maxAbsValue)
    {
        // Act & Assert - Should not throw exceptions
        var result = _strategy.GetColorForValue(value, maxAbsValue);
        Assert.NotNull(result);
    }

    #endregion
}