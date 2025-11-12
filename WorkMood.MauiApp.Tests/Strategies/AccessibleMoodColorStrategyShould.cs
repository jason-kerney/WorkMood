using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Strategies;
using Xunit;

namespace WorkMood.MauiApp.Tests.Strategies;

/// <summary>
/// Tests for AccessibleMoodColorStrategy
/// </summary>
public class AccessibleMoodColorStrategyShould
{
    private readonly AccessibleMoodColorStrategy _strategy;

    public AccessibleMoodColorStrategyShould()
    {
        _strategy = new AccessibleMoodColorStrategy();
    }

    #region Interface Implementation Tests

    [Fact]
    public void Strategy_ShouldImplementInterface()
    {
        // Assert
        Assert.IsAssignableFrom<IMoodColorStrategy>(_strategy);
    }

    #endregion

    #region Positive Value Tests (Blue Shades)

    [Fact]
    public void GetColorForValue_ShouldReturnBlueShade_WhenValueIsPositive()
    {
        // Arrange
        double positiveValue = 2.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(positiveValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Blue);  // Should have full blue component
        Assert.True(result.Red < 1.0f);   // Red should be reduced for blue shade
        Assert.True(result.Green < 1.0f); // Green should be reduced for blue shade
    }

    [Fact]
    public void GetColorForValue_ShouldReturnMoreIntenseBlue_WhenValueIsHigherPositive()
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
        
        // Higher positive values should have more intense blue (darker blue)
        Assert.True(higherResult.Red < lowerResult.Red);
        Assert.True(higherResult.Green < lowerResult.Green);
        Assert.Equal(1.0f, lowerResult.Blue);
        Assert.Equal(1.0f, higherResult.Blue);
    }

    [Theory]
    [InlineData(0.5, 5.0)]
    [InlineData(1.0, 5.0)]
    [InlineData(2.5, 5.0)]
    [InlineData(4.0, 5.0)]
    public void GetColorForValue_ShouldReturnValidBlueShade_ForVariousPositiveValues(double value, double maxAbsValue)
    {
        // Act
        var result = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Blue);
        Assert.InRange(result.Red, 0.1f, 0.6f);   // Should be within expected range
        Assert.InRange(result.Green, 0.3f, 0.7f); // Should be within expected range
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
        Assert.Equal(1.0f, result.Blue);
        Assert.Equal(0.1f, result.Red);   // Should be at minimum (most intense blue)
        Assert.Equal(0.3f, result.Green); // Should be at minimum (most intense blue)
    }

    #endregion

    #region Negative Value Tests (Orange Shades)

    [Fact]
    public void GetColorForValue_ShouldReturnOrangeShade_WhenValueIsNegative()
    {
        // Arrange
        double negativeValue = -2.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(negativeValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);   // Should have full red component for orange
        Assert.True(result.Green > 0.0f && result.Green < 1.0f); // Orange needs some green
        Assert.True(result.Blue < 1.0f);  // Blue should be reduced for orange shade
    }

    [Fact]
    public void GetColorForValue_ShouldReturnMoreIntenseOrange_WhenValueIsLowerNegative()
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
        
        // More negative values should have more intense orange (darker orange)
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
    public void GetColorForValue_ShouldReturnValidOrangeShade_ForVariousNegativeValues(double value, double maxAbsValue)
    {
        // Act
        var result = _strategy.GetColorForValue(value, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1.0f, result.Red);
        Assert.InRange(result.Green, 0.5f, 0.8f); // Should be within expected range for orange (updated for accessibility)
        Assert.InRange(result.Blue, 0.1f, 0.3f);  // Should be within expected range for orange
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
        Assert.Equal(0.5f, result.Green); // Should be at minimum (most intense orange) - updated for accessibility
        Assert.Equal(0.1f, result.Blue);  // Should be at minimum (most intense orange)
    }

    #endregion

    #region Zero Value Tests (Neutral Gray)

    [Fact]
    public void GetColorForValue_ShouldReturnNeutralGray_WhenValueIsZero()
    {
        // Arrange
        double zeroValue = 0.0;
        double maxAbsValue = 5.0;

        // Act
        var result = _strategy.GetColorForValue(zeroValue, maxAbsValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.6f, result.Red);
        Assert.Equal(0.6f, result.Green);
        Assert.Equal(0.6f, result.Blue);
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
        Assert.Equal(1.0f, result.Blue);
        Assert.True(result.Red > 0.1f);  // Should be close to light blue
        Assert.True(result.Green > 0.3f); // Should be close to light blue
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
        Assert.True(result.Green > 0.4f); // Should be close to light orange
        Assert.True(result.Blue > 0.1f);  // Should be close to light orange
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
        Assert.Equal(1.0f, result.Blue);
        Assert.Equal(0.1f, result.Red);   // Should be capped at most intense
        Assert.Equal(0.3f, result.Green); // Should be capped at most intense
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
        Assert.Equal(0.5f, result.Green); // Should be capped at most intense - updated for accessibility
        Assert.Equal(0.1f, result.Blue);  // Should be capped at most intense
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
        Assert.Equal(1.0f, result.Blue);
        Assert.Equal(0.1f, result.Red);
        Assert.Equal(0.3f, result.Green);
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
            Assert.Equal(1.0f, result.Blue);
            Assert.Equal(0.1f, result.Red);   // Most intense blue
            Assert.Equal(0.3f, result.Green);
        }
        else if (value < 0)
        {
            Assert.Equal(1.0f, result.Red);
            Assert.Equal(0.5f, result.Green); // Most intense orange - updated for accessibility
            Assert.Equal(0.1f, result.Blue);
        }
    }

    #endregion

    #region Accessibility and Color Differentiation Tests

    [Fact]
    public void GetColorForValue_ShouldProvideGoodContrast_BetweenPositiveAndNegative()
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
        
        // Verify blue vs orange contrast
        Assert.Equal(1.0f, positiveResult.Blue);  // Blue for positive
        Assert.Equal(1.0f, negativeResult.Red);   // Red (orange) for negative
        
        // Colors should be sufficiently different
        var blueDominance = positiveResult.Blue - Math.Max(positiveResult.Red, positiveResult.Green);
        var redDominance = negativeResult.Red - Math.Max(negativeResult.Green, negativeResult.Blue);
        
        Assert.True(blueDominance > 0.3f, "Blue should be clearly dominant in positive colors");
        Assert.True(redDominance > 0.3f, "Red should be clearly dominant in negative colors");
    }

    [Fact]
    public void GetColorForValue_ShouldUseColorBlindFriendlyPalette()
    {
        // Arrange
        double[] values = { -3.0, 0.0, 3.0 };
        double maxAbsValue = 5.0;

        // Act
        var colors = values.Select(v => _strategy.GetColorForValue(v, maxAbsValue)).ToArray();

        // Assert
        // Negative should be orange (high red, medium green, low blue)
        Assert.Equal(1.0f, colors[0].Red);
        Assert.True(colors[0].Green > 0.4f && colors[0].Green < 0.7f);
        Assert.True(colors[0].Blue < 0.3f);
        
        // Zero should be neutral gray
        Assert.Equal(0.6f, colors[1].Red);
        Assert.Equal(0.6f, colors[1].Green);
        Assert.Equal(0.6f, colors[1].Blue);
        
        // Positive should be blue (low red, low-medium green, high blue)
        Assert.True(colors[2].Red < 0.6f);
        Assert.True(colors[2].Green < 0.7f);
        Assert.Equal(1.0f, colors[2].Blue);
    }

    [Fact]
    public void GetColorForValue_ShouldDifferFromDefaultStrategy_ForAccessibility()
    {
        // Arrange
        var defaultStrategy = new DefaultMoodColorStrategy();
        double positiveValue = 2.0;
        double negativeValue = -2.0;
        double maxAbsValue = 5.0;

        // Act
        var accessiblePos = _strategy.GetColorForValue(positiveValue, maxAbsValue);
        var accessibleNeg = _strategy.GetColorForValue(negativeValue, maxAbsValue);
        var defaultPos = defaultStrategy.GetColorForValue(positiveValue, maxAbsValue);
        var defaultNeg = defaultStrategy.GetColorForValue(negativeValue, maxAbsValue);

        // Assert
        // Accessible strategy should use blue for positive (not green)
        Assert.Equal(1.0f, accessiblePos.Blue);
        Assert.Equal(1.0f, defaultPos.Green);
        Assert.NotEqual(accessiblePos.Blue, defaultPos.Blue);
        
        // Both should use red for negative, but accessible adds more green for orange
        Assert.Equal(1.0f, accessibleNeg.Red);
        Assert.Equal(1.0f, defaultNeg.Red);
        Assert.True(accessibleNeg.Green > defaultNeg.Green); // Orange has more green than pure red
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
        
        // Intensity should be symmetrical - same distance from base values
        var posIntensity = 3.0 / 5.0; // 0.6
        var negIntensity = 3.0 / 5.0; // 0.6
        
        // Check that intensities are applied consistently
        Assert.Equal(0.1f + (1.0f - 0.6f) * 0.5f, positiveResult.Red, precision: 2);
        Assert.Equal(0.5f + (1.0f - 0.6f) * 0.3f, negativeResult.Green, precision: 2); // Updated base from 0.4 to 0.5
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
        // Most negative should be most intense orange
        Assert.Equal(1.0f, colors[0].Red);
        Assert.Equal(0.5f + (1.0 - 0.8) * 0.3f, colors[0].Green, precision: 2); // Updated base from 0.4 to 0.5
        
        // Zero should be neutral gray
        Assert.Equal(0.6f, colors[2].Red);
        Assert.Equal(0.6f, colors[2].Green);
        Assert.Equal(0.6f, colors[2].Blue);
        
        // Most positive should be most intense blue
        Assert.Equal(1.0f, colors[4].Blue);
        Assert.Equal(0.1f + (1.0 - 0.8) * 0.5f, colors[4].Red, precision: 2);
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

    #region Strategy Comparison Tests

    [Fact]
    public void AccessibleStrategy_ShouldProvideBetterAccessibility_ThanDefaultStrategy()
    {
        // Arrange
        var defaultStrategy = new DefaultMoodColorStrategy();
        double[] testValues = { -3.0, -1.0, 0.0, 1.0, 3.0 };
        double maxAbsValue = 5.0;

        // Act
        var accessibleColors = testValues.Select(v => _strategy.GetColorForValue(v, maxAbsValue)).ToArray();
        var defaultColors = testValues.Select(v => defaultStrategy.GetColorForValue(v, maxAbsValue)).ToArray();

        // Assert - Verify accessibility improvements
        foreach (int i in new[] { 0, 1, 3, 4 }) // Skip neutral (index 2)
        {
            if (testValues[i] > 0)
            {
                // Positive: Accessible uses blue vs default green
                Assert.Equal(1.0f, accessibleColors[i].Blue);
                Assert.Equal(1.0f, defaultColors[i].Green);
                // The key difference is the color focus: blue vs green (not blue intensity)
                Assert.True(accessibleColors[i].Blue >= defaultColors[i].Blue);
                Assert.NotEqual(accessibleColors[i].Green, defaultColors[i].Green);
            }
            else if (testValues[i] < 0)
            {
                // Negative: Both use red, but accessible adds green for orange
                Assert.Equal(1.0f, accessibleColors[i].Red);
                Assert.Equal(1.0f, defaultColors[i].Red);
                Assert.True(accessibleColors[i].Green > defaultColors[i].Green);
            }
        }
    }

    #endregion
}