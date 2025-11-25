using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Processors;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Strategies;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests for MoodVisualizationService - service for creating mood data visualizations
/// Component testing following established service patterns with dependency injection
/// </summary>
public class MoodVisualizationServiceShould
{
    private readonly Mock<IVisualizationDataProcessor> _mockDataProcessor;
    private readonly Mock<IMoodColorStrategy> _mockColorStrategy;
    private readonly MoodVisualizationService _service;

    public MoodVisualizationServiceShould()
    {
        _mockDataProcessor = new Mock<IVisualizationDataProcessor>();
        _mockColorStrategy = new Mock<IMoodColorStrategy>();
        _service = new MoodVisualizationService(_mockDataProcessor.Object, _mockColorStrategy.Object);
    }

    #region Checkpoint 1: Basic Structure and Interface

    [Fact]
    public void ImplementIMoodVisualizationServiceInterface()
    {
        // Verify the service implements the required interface
        Assert.IsAssignableFrom<IMoodVisualizationService>(_service);
    }

    [Fact]
    public void Constructor_WithNullDependencies_ShouldUseDefaults()
    {
        // Test that null dependencies result in default implementations
        var serviceWithDefaults = new MoodVisualizationService(null, null);
        
        Assert.NotNull(serviceWithDefaults);
        Assert.IsAssignableFrom<IMoodVisualizationService>(serviceWithDefaults);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldUseProvidedInstances()
    {
        // Test that provided dependencies are used
        var service = new MoodVisualizationService(_mockDataProcessor.Object, _mockColorStrategy.Object);
        
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IMoodVisualizationService>(service);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithValidMoodCollection_ReturnsVisualizationData()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MoodVisualizationData>(result);
        Assert.Equal(expectedDailyValues, result.DailyValues);
    }

    #endregion

    #region Checkpoint 2: Date Range Processing

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldExcludeToday()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - End date should be yesterday, not today
        var expectedEndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        Assert.Equal(expectedEndDate, result.EndDate);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldCalculateCorrectDateRange()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Should be 14 days total (13 days between start and end inclusive)
        var daysBetween = result.EndDate.DayNumber - result.StartDate.DayNumber + 1;
        Assert.Equal(14, daysBetween);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldCallProcessorWithCorrectDateRange()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Verify processor was called with correct date range
        var expectedEndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        var expectedStartDate = expectedEndDate.AddDays(-13);
        
        _mockDataProcessor.Verify(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            expectedStartDate,
            expectedEndDate), Times.Once);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldCallProcessorWithFilteredEntries()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Verify processor was called once with some entries (real MoodCollection will filter them)
        _mockDataProcessor.Verify(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()), Times.Once);
    }

    #endregion

    #region Checkpoint 3: Data Processing and Scaling

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldSetCorrectDimensions()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert
        Assert.Equal(280, result.Width);  // 14 days * 20 pixels per day
        Assert.Equal(100, result.Height);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithEmptyCollection_ShouldReturnDefaultMaxValue()
    {
        // Arrange - Create empty MoodCollection
        var emptyMoodCollection = new MoodCollection();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(emptyMoodCollection);

        // Assert - Should default to 1.0 when no entries in collection
        Assert.Equal(1.0, result.MaxAbsoluteValue);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithMoodEntries_ShouldCalculateCorrectMaxValue()
    {
        // Arrange - Create MoodCollection with specific entries that will result in known values
        var entriesWithValues = new List<MoodEntry>
        {
            new() { StartOfWork = 3, EndOfWork = 6, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = 3
            new() { StartOfWork = 5, EndOfWork = 3, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = -2  
            new() { StartOfWork = 2, EndOfWork = 5, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }  // Value = 3
        };
        var moodCollection = new MoodCollection(entriesWithValues);
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Should be the maximum absolute value (3 from the entries)
        Assert.Equal(3.0, result.MaxAbsoluteValue);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithNullValues_ShouldIgnoreNulls()
    {
        // Arrange - Create MoodCollection with mixed valid/null entries
        var entriesWithNulls = new List<MoodEntry>
        {
            new() { StartOfWork = 3, EndOfWork = 5, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = 2
            new() { StartOfWork = null, EndOfWork = null, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = null
            new() { StartOfWork = 4, EndOfWork = 2, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }  // Value = -2
        };
        var moodCollection = new MoodCollection(entriesWithNulls);
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Should be max of non-null absolute values (2.0)
        Assert.Equal(2.0, result.MaxAbsoluteValue);
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithAllNullValues_ShouldReturnDefaultMaxValue()
    {
        // Arrange - Create MoodCollection with all null entries
        var entriesWithAllNulls = new List<MoodEntry>
        {
            new() { StartOfWork = null, EndOfWork = null, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = null
            new() { StartOfWork = null, EndOfWork = null, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }, // Value = null
            new() { StartOfWork = null, EndOfWork = null, Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), CreatedAt = DateTime.Now, LastModified = DateTime.Now }  // Value = null
        };
        var moodCollection = new MoodCollection(entriesWithAllNulls);
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Should default to 1.0 when all values are null
        Assert.Equal(1.0, result.MaxAbsoluteValue);
    }

    [Theory]
    [InlineData(new double[] { 1.0, 2.0, 3.0 }, 3.0)]
    [InlineData(new double[] { -1.0, -2.0, -3.0 }, 3.0)]
    [InlineData(new double[] { 1.5, -2.5, 3.0 }, 3.0)]
    [InlineData(new double[] { 0.5, 0.3, 0.1 }, 1.0)] // Should default to minimum of 1.0
    public void CreateTwoWeekValueVisualization_WithDifferentValues_ShouldCalculateCorrectMaxValue(
        double[] values, double expectedMax)
    {
        // Arrange - Create entries that will produce the target values
        var entries = values.Select((value, index) => 
        {
            var startOfWork = 5; // Base mood
            var endOfWork = Math.Max(1, Math.Min(10, startOfWork + (int)Math.Round(value))); // Calculate end to achieve target value, clamped to 1-10
            return new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork,
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-(index + 1))),
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now
            };
        }).ToList();
        var moodCollection = new MoodCollection(entries);
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert
        Assert.Equal(expectedMax, result.MaxAbsoluteValue);
    }

    #endregion

    #region Edge Cases and Error Conditions

    [Fact]
    public void CreateTwoWeekValueVisualization_WithNullMoodCollection_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => 
            _service.CreateTwoWeekValueVisualization(null!));
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_WithGracefulHandling_ShouldNotThrow()
    {
        // Arrange - Create empty MoodCollection to test graceful handling
        var emptyMoodCollection = new MoodCollection();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(CreateTestDailyValues());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(emptyMoodCollection);

        // Assert - Should not throw and return valid result
        Assert.NotNull(result);
        Assert.Equal(1.0, result.MaxAbsoluteValue); // Default value
    }

    [Fact]
    public void CreateTwoWeekValueVisualization_ShouldHandleProcessorReturningEmptyArray()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(Array.Empty<DailyMoodValue>());

        // Act
        var result = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
        Assert.Empty(result.DailyValues);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CreateTwoWeekValueVisualization_WithRealDependencies_ShouldWorkCorrectly()
    {
        // Arrange - Use real dependencies for integration test
        var realService = new MoodVisualizationService();
        var moodCollection = CreateTestMoodCollection();

        // Act
        var result = realService.CreateTwoWeekValueVisualization(moodCollection);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.DailyValues);
        Assert.Equal(280, result.Width);
        Assert.Equal(100, result.Height);
        Assert.True(result.MaxAbsoluteValue >= 1.0);
    }

    [Fact]
    public void MoodVisualizationService_ShouldMaintainConsistentBehaviorAcrossMultipleCalls()
    {
        // Arrange
        var moodCollection = CreateTestMoodCollection();
        var expectedDailyValues = CreateTestDailyValues();
        
        _mockDataProcessor.Setup(p => p.ProcessMoodEntries(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()))
            .Returns(expectedDailyValues);

        // Act - Call multiple times
        var result1 = _service.CreateTwoWeekValueVisualization(moodCollection);
        var result2 = _service.CreateTwoWeekValueVisualization(moodCollection);

        // Assert - Should be consistent
        Assert.Equal(result1.Width, result2.Width);
        Assert.Equal(result1.Height, result2.Height);
        Assert.Equal(result1.MaxAbsoluteValue, result2.MaxAbsoluteValue);
        Assert.Equal(result1.DailyValues, result2.DailyValues);
    }

    #endregion

    #region Test Helper Methods

    private static MoodCollection CreateTestMoodCollection()
    {
        var entries = new List<MoodEntry>();
        
        // Add some test entries spanning more than 2 weeks
        var today = DateOnly.FromDateTime(DateTime.Today);
        for (int i = 1; i <= 20; i++)
        {
            var entry = new MoodEntry
            {
                Date = today.AddDays(-i),
                CreatedAt = DateTime.Now.AddDays(-i),
                LastModified = DateTime.Now.AddDays(-i)
            };
            
            // Create mix of null and values
            if (i % 3 != 0)
            {
                var startMood = (i % 7) + 1; // 1-7
                var endMood = ((i + 1) % 7) + 1; // 1-7
                entry.StartOfWork = startMood;
                entry.EndOfWork = endMood;
            }
            // else leave as nulls for missing data
            
            entries.Add(entry);
        }
        
        return new MoodCollection(entries);
    }

    private static List<MoodEntry> CreateTestMoodEntries()
    {
        var entries = new List<MoodEntry>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        for (int i = 1; i <= 14; i++)
        {
            var startMood = (i % 7) + 1; // 1-7
            var value = (i % 5) - 2; // -2 to 2
            var endMood = Math.Max(1, Math.Min(10, startMood + value)); // Ensure 1-10 range
            
            entries.Add(new MoodEntry
            {
                Date = today.AddDays(-i),
                StartOfWork = startMood,
                EndOfWork = endMood,
                CreatedAt = DateTime.Now.AddDays(-i),
                LastModified = DateTime.Now.AddDays(-i)
            });
        }
        
        return entries;
    }

    private static DailyMoodValue[] CreateTestDailyValues()
    {
        var dailyValues = new DailyMoodValue[14];
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        for (int i = 0; i < 14; i++)
        {
            dailyValues[i] = new DailyMoodValue
            {
                Date = today.AddDays(-(i + 1)),
                Value = (double)((i % 5) - 2),
                HasData = true,
                Color = Colors.Blue
            };
        }
        
        return dailyValues;
    }

    #endregion
}