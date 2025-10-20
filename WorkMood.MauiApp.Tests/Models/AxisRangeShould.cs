using WorkMood.MauiApp.Models;
using Xunit;

namespace WorkMood.MauiApp.Tests.Models;

/// <summary>
/// Tests for AxisRange immutable record
/// Location: MauiApp/Models/AxisRange.cs
/// Purpose: Verify record behavior, factory methods, and range operations
/// </summary>
public class AxisRangeShould
{
    #region Record Construction Tests (Checkpoint 1)

    [Fact]
    public void BeCreatableWithPrimaryConstructor()
    {
        // Act
        var axisRange = new AxisRange(1, 10);

        // Assert
        Assert.NotNull(axisRange);
        Assert.Equal(1, axisRange.Min);
        Assert.Equal(10, axisRange.Max);
    }

    [Fact]
    public void SupportRecordEqualitySemantics()
    {
        // Arrange
        var range1 = new AxisRange(5, 15);
        var range2 = new AxisRange(5, 15);
        var range3 = new AxisRange(6, 15);

        // Assert - Same values should be equal
        Assert.Equal(range1, range2);
        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
        
        // Different values should not be equal
        Assert.NotEqual(range1, range3);
        Assert.False(range1.Equals(range3));
        Assert.True(range1 != range3);
    }

    [Fact]
    public void SupportRecordHashCodeSemantics()
    {
        // Arrange
        var range1 = new AxisRange(-5, 5);
        var range2 = new AxisRange(-5, 5);
        var range3 = new AxisRange(-4, 5);

        // Assert - Equal records should have equal hash codes
        Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
        
        // Different records should have different hash codes (highly likely)
        Assert.NotEqual(range1.GetHashCode(), range3.GetHashCode());
    }

    #endregion

    #region Factory Method Tests (Checkpoint 2)

    [Fact]
    public void ProvideImpactFactoryWithCorrectRange()
    {
        // Act
        var impactRange = AxisRange.Impact;

        // Assert
        Assert.Equal(-9, impactRange.Min);
        Assert.Equal(9, impactRange.Max);
        Assert.Equal(18, impactRange.Range);
    }

    [Fact]
    public void ProvideAverageFactoryWithCorrectRange()
    {
        // Act
        var averageRange = AxisRange.Average;

        // Assert
        Assert.Equal(-5, averageRange.Min);
        Assert.Equal(5, averageRange.Max);
        Assert.Equal(10, averageRange.Range);
    }

    [Fact]
    public void ProvideRawDataFactoryWithCorrectRange()
    {
        // Act
        var rawDataRange = AxisRange.RawData;

        // Assert
        Assert.Equal(1, rawDataRange.Min);
        Assert.Equal(10, rawDataRange.Max);
        Assert.Equal(9, rawDataRange.Range);
    }

    [Fact]
    public void ReturnSameInstanceForStaticFactories()
    {
        // Act - Multiple calls to static properties
        var impact1 = AxisRange.Impact;
        var impact2 = AxisRange.Impact;
        var average1 = AxisRange.Average;
        var average2 = AxisRange.Average;

        // Assert - Should return equivalent values (record equality)
        Assert.Equal(impact1, impact2);
        Assert.Equal(average1, average2);
        
        // Verify they have correct values
        Assert.Equal(-9, impact1.Min);
        Assert.Equal(9, impact1.Max);
        Assert.Equal(-5, average1.Min);
        Assert.Equal(5, average1.Max);
    }

    #endregion

    #region Method Tests (Checkpoint 3)

    [Theory]
    [InlineData(1, 10, 9)]
    [InlineData(-5, 5, 10)]
    [InlineData(-9, 9, 18)]
    [InlineData(0, 0, 0)]
    [InlineData(-10, -5, 5)]
    public void CalculateRangeCorrectly(int min, int max, int expectedRange)
    {
        // Arrange
        var axisRange = new AxisRange(min, max);

        // Act
        var actualRange = axisRange.Range;

        // Assert
        Assert.Equal(expectedRange, actualRange);
    }

    [Theory]
    [InlineData(1, 10, 1, true)]      // Min boundary
    [InlineData(1, 10, 10, true)]     // Max boundary
    [InlineData(1, 10, 5, true)]      // Middle value
    [InlineData(1, 10, 0, false)]     // Below min
    [InlineData(1, 10, 11, false)]    // Above max
    [InlineData(-5, 5, -5, true)]     // Negative min boundary
    [InlineData(-5, 5, 5, true)]      // Positive max boundary
    [InlineData(-5, 5, 0, true)]      // Zero in range
    [InlineData(-5, 5, -6, false)]    // Below negative min
    [InlineData(-5, 5, 6, false)]     // Above positive max
    public void CheckIntContainmentCorrectly(int min, int max, int testValue, bool expected)
    {
        // Arrange
        var axisRange = new AxisRange(min, max);

        // Act
        var result = axisRange.Contains(testValue);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 10, 1.0f, true)]      // Min boundary (float)
    [InlineData(1, 10, 10.0f, true)]     // Max boundary (float)
    [InlineData(1, 10, 5.5f, true)]      // Middle value (float)
    [InlineData(1, 10, 0.9f, false)]     // Just below min
    [InlineData(1, 10, 10.1f, false)]    // Just above max
    [InlineData(-5, 5, -5.0f, true)]     // Negative boundary
    [InlineData(-5, 5, 2.75f, true)]     // Decimal within range
    [InlineData(-5, 5, -5.1f, false)]    // Just below negative min
    [InlineData(-5, 5, 5.1f, false)]     // Just above positive max
    public void CheckFloatContainmentCorrectly(int min, int max, float testValue, bool expected)
    {
        // Arrange
        var axisRange = new AxisRange(min, max);

        // Act
        var result = axisRange.Contains(testValue);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SupportBothContainsOverloads()
    {
        // Arrange
        var range = new AxisRange(0, 100);

        // Act & Assert - Both overloads should be available
        Assert.True(range.Contains(50));      // int version
        Assert.True(range.Contains(50.5f));   // float version
        Assert.False(range.Contains(-1));     // int version
        Assert.False(range.Contains(-1.5f));  // float version
    }

    [Fact]
    public void HandleEdgeCasesForContains()
    {
        // Arrange - Test with zero range
        var zeroRange = new AxisRange(5, 5);
        var negativeRange = new AxisRange(-10, -5);

        // Act & Assert
        Assert.True(zeroRange.Contains(5));
        Assert.True(zeroRange.Contains(5.0f));
        Assert.False(zeroRange.Contains(4));
        Assert.False(zeroRange.Contains(6));

        Assert.True(negativeRange.Contains(-7));
        Assert.True(negativeRange.Contains(-7.5f));
        Assert.False(negativeRange.Contains(-4));
        Assert.False(negativeRange.Contains(-11));
    }

    #endregion
}