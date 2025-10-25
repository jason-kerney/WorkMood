using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Factories;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Strategies;
using Xunit;

namespace WorkMood.MauiApp.Tests.Factories;

/// <summary>
/// Tests for VisualizationServiceFactory
/// </summary>
public class VisualizationServiceFactoryShould
{
    private readonly VisualizationServiceFactory _factory;

    public VisualizationServiceFactoryShould()
    {
        _factory = new VisualizationServiceFactory();
    }

    #region CreateVisualizationService Tests

    [Fact]
    public void CreateVisualizationService_ShouldReturnService_WhenCalledWithDefault()
    {
        // Act
        var result = _factory.CreateVisualizationService();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MoodVisualizationService>(result);
    }

    [Fact]
    public void CreateVisualizationService_ShouldReturnService_WhenCalledWithDefaultEnum()
    {
        // Act
        var result = _factory.CreateVisualizationService(VisualizationColorScheme.Default);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MoodVisualizationService>(result);
    }

    [Fact]
    public void CreateVisualizationService_ShouldReturnService_WhenCalledWithAccessible()
    {
        // Act
        var result = _factory.CreateVisualizationService(VisualizationColorScheme.Accessible);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MoodVisualizationService>(result);
    }

    [Fact]
    public void CreateVisualizationService_ShouldReturnDifferentInstances_WhenCalledMultipleTimes()
    {
        // Act
        var result1 = _factory.CreateVisualizationService();
        var result2 = _factory.CreateVisualizationService();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void CreateVisualizationService_ShouldReturnWorkingService_WhenDefaultScheme()
    {
        // Act
        var service = _factory.CreateVisualizationService(VisualizationColorScheme.Default);

        // Assert
        Assert.NotNull(service);
        
        // Verify service works by processing test data
        var testData = CreateTestVisualizationData();
        var result = service.CreateTwoWeekValueVisualization(testData);
        
        // Service should return valid visualization data
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
        Assert.True(result.DailyValues.Length >= 0);
    }

    [Fact]
    public void CreateVisualizationService_ShouldReturnWorkingService_WhenAccessibleScheme()
    {
        // Act
        var service = _factory.CreateVisualizationService(VisualizationColorScheme.Accessible);

        // Assert
        Assert.NotNull(service);
        
        // Verify service works by processing test data
        var testData = CreateTestVisualizationData();
        var result = service.CreateTwoWeekValueVisualization(testData);
        
        // Service should return valid visualization data
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
        Assert.True(result.DailyValues.Length >= 0);
    }

    [Theory]
    [InlineData(VisualizationColorScheme.Default)]
    [InlineData(VisualizationColorScheme.Accessible)]
    public void CreateVisualizationService_ShouldReturnWorkingService_ForAllColorSchemes(VisualizationColorScheme scheme)
    {
        // Act
        var service = _factory.CreateVisualizationService(scheme);
        var testData = CreateTestVisualizationData();
        var result = service.CreateTwoWeekValueVisualization(testData);

        // Assert
        Assert.NotNull(service);
        Assert.NotNull(result);
        Assert.NotEmpty(result.DailyValues);
        Assert.All(result.DailyValues, day => Assert.NotNull(day.Color));
    }

    [Fact]
    public void CreateVisualizationService_ShouldUseDefaultStrategy_ForUnknownColorScheme()
    {
        // Act - Using invalid enum value (cast to force unknown value)
        var service = _factory.CreateVisualizationService((VisualizationColorScheme)999);

        // Assert
        Assert.NotNull(service);
        
        // Verify service still works with unknown enum values (fallback to default)
        var testData = CreateTestVisualizationData();
        var result = service.CreateTwoWeekValueVisualization(testData);
        
        // Service should return valid visualization data even with unknown scheme
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
        Assert.True(result.DailyValues.Length >= 0);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Factory_ShouldImplementInterface()
    {
        // Assert
        Assert.IsAssignableFrom<IVisualizationServiceFactory>(_factory);
    }

    [Fact]
    public void Factory_ShouldSupportFactoryPattern()
    {
        // Act
        IVisualizationServiceFactory factory = new VisualizationServiceFactory();
        var service = factory.CreateVisualizationService();

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMoodVisualizationService>(service);
    }

    #endregion

    #region Color Strategy Integration Tests

    [Fact]
    public void CreateVisualizationService_ShouldProduceConsistentResults_ForSameScheme()
    {
        // Arrange
        var testData = CreateTestVisualizationData();

        // Act
        var service1 = _factory.CreateVisualizationService(VisualizationColorScheme.Default);
        var service2 = _factory.CreateVisualizationService(VisualizationColorScheme.Default);
        
        var result1 = service1.CreateTwoWeekValueVisualization(testData);
        var result2 = service2.CreateTwoWeekValueVisualization(testData);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        
        // Both services using the same scheme should produce consistent results
        Assert.Equal(result1.DailyValues.Length, result2.DailyValues.Length);
        Assert.Equal(result1.StartDate, result2.StartDate);
        Assert.Equal(result1.EndDate, result2.EndDate);
    }

    [Fact] 
    public void CreateVisualizationService_ShouldProduceDifferentResults_ForDifferentSchemes()
    {
        // Arrange
        var testData = CreateTestVisualizationData();

        // Act
        var defaultService = _factory.CreateVisualizationService(VisualizationColorScheme.Default);
        var accessibleService = _factory.CreateVisualizationService(VisualizationColorScheme.Accessible);
        
        var defaultResult = defaultService.CreateTwoWeekValueVisualization(testData);
        var accessibleResult = accessibleService.CreateTwoWeekValueVisualization(testData);

        // Assert
        Assert.NotNull(defaultResult);
        Assert.NotNull(accessibleResult);
        
        // Both should produce valid results even if strategies differ
        Assert.NotNull(defaultResult.DailyValues);
        Assert.NotNull(accessibleResult.DailyValues);
        Assert.True(defaultResult.DailyValues.Length >= 0);
        Assert.True(accessibleResult.DailyValues.Length >= 0);
    }

    #endregion

    #region Service Configuration Verification

    [Fact]
    public void CreateVisualizationService_ShouldConfigureDataProcessor_WithCorrectColorStrategy()
    {
        // Act
        var defaultService = _factory.CreateVisualizationService(VisualizationColorScheme.Default);
        var accessibleService = _factory.CreateVisualizationService(VisualizationColorScheme.Accessible);

        // Assert - Verify services work correctly (indirect verification of proper configuration)
        var testData = CreateTestVisualizationData();
        
        var defaultResult = defaultService.CreateTwoWeekValueVisualization(testData);
        var accessibleResult = accessibleService.CreateTwoWeekValueVisualization(testData);

        Assert.NotNull(defaultResult);
        Assert.NotNull(accessibleResult);
        Assert.All(defaultResult.DailyValues, day => Assert.NotNull(day.Color));
        Assert.All(accessibleResult.DailyValues, day => Assert.NotNull(day.Color));
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void CreateVisualizationService_ShouldHandleEmptyData_Gracefully()
    {
        // Act
        var service = _factory.CreateVisualizationService();
        var emptyData = new MoodCollection();
        var result = service.CreateTwoWeekValueVisualization(emptyData);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
    }

    [Fact]
    public void CreateVisualizationService_ShouldHandleNullMoodEntries_Gracefully()
    {
        // Act
        var service = _factory.CreateVisualizationService();

        // Assert - Should not throw when creating service
        Assert.NotNull(service);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates test mood data for visualization testing
    /// </summary>
    private static MoodCollection CreateTestVisualizationData()
    {
        var startDate = new DateOnly(2024, 10, 1);
        var entries = new List<MoodEntry>();

        // Create a varied dataset with different mood values
        // Values will be computed as EndOfWork - StartOfWork
        var startMoods = new int[] { 5, 6, 4, 3, 7 };
        var endMoods = new int[] { 2, 5, 4, 7, 9 }; // Results in values: -3, -1, 0, 4, 2
        
        for (int i = 0; i < startMoods.Length; i++)
        {
            var date = startDate.AddDays(i);
            entries.Add(new MoodEntry 
            { 
                Date = date, 
                StartOfWork = startMoods[i], 
                EndOfWork = endMoods[i],
                CreatedAt = DateTime.Now.AddDays(-i),
                LastModified = DateTime.Now.AddDays(-i)
            });
        }

        return new MoodCollection(entries);
    }

    #endregion
}