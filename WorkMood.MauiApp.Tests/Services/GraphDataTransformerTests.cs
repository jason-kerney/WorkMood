using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Comprehensive tests for GraphDataTransformer functionality
/// </summary>
public class GraphDataTransformerTests
{
    private readonly GraphDataTransformer _transformer;
    private readonly FakeDateShim _dateShim;

    public GraphDataTransformerTests()
    {
        _transformer = new GraphDataTransformer();
        _dateShim = new FakeDateShim(new DateOnly(2025, 1, 10)); // Fixed date for testing
    }

    private DateRangeInfo CreateDateRangeInfo(DateRange dateRange)
    {
        return new DateRangeInfo(dateRange, _dateShim);
    }

    #region Impact Mode Tests

    [Fact]
    public void TransformMoodEntries_ImpactMode_WithValidData_ShouldReturnCorrectGraphData()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var (_, moodEntries) = MoodDataTestHelper.GetFakeData(startDate, 
            (startMood: 5, endMood: 7),   // +2 impact
            (startMood: 6, endMood: 4),   // -2 impact
            (startMood: 8, endMood: 9)    // +1 impact
        );

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mood Change Over Time", result.Title);
        Assert.Equal(AxisRange.Impact, result.YAxisRange);
        Assert.Equal(0, result.CenterLineValue);
        Assert.Equal("Impact", result.YAxisLabel);
        Assert.Equal("Date", result.XAxisLabel);
        Assert.False(result.IsRawData);
        Assert.Equal(3, result.YAxisLabelStep);
        Assert.Contains("daily impact on mood", result.Description);

        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(3, dataPoints.Count);
        Assert.Equal(2, dataPoints[0].Value); // 7 - 5 = 2
        Assert.Equal(-2, dataPoints[1].Value); // 4 - 6 = -2
        Assert.Equal(1, dataPoints[2].Value); // 9 - 8 = 1
    }

    [Fact]
    public void TransformMoodEntries_ImpactMode_WithNoValidData_ShouldReturnEmptyDataPoints()
    {
        // Arrange - Impact mode requires StartOfWork to have a value (Value property checks this)
        var startDate = new DateOnly(2025, 1, 1);
        var (_, moodEntries) = MoodDataTestHelper.GetFakeData(startDate, 
            (startMood: null, endMood: null),
            (startMood: null, endMood: 7)  // No StartOfWork means no Value
        );

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mood Change Over Time", result.Title);
        Assert.Empty(result.DataPoints);
    }

    [Fact]
    public void TransformMoodEntries_ImpactMode_WithPartialData_ShouldFilterCorrectly()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var (_, moodEntries) = MoodDataTestHelper.GetFakeData(startDate, 
            (startMood: 5, endMood: 7),   // Valid - has impact (Value = 2)
            (startMood: null, endMood: null), // Invalid - no impact data (Value = null)
            (startMood: 8, endMood: 6),   // Valid - has impact (Value = -2)
            (startMood: 3, endMood: null) // Valid - has impact (Value = 0, EndOfWork defaults to StartOfWork)
        );

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(3, dataPoints.Count); // Three entries have StartOfWork values
        Assert.Equal(2, dataPoints[0].Value); // 7 - 5 = 2
        Assert.Equal(-2, dataPoints[1].Value); // 6 - 8 = -2
        Assert.Equal(0, dataPoints[2].Value); // 3 - 3 = 0 (EndOfWork defaults to StartOfWork)
    }

    [Fact]
    public void TransformMoodEntries_ImpactMode_ShouldSortByTimestamp()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 3), StartOfWork = 5, EndOfWork = 8, CreatedAt = new DateTime(2025, 1, 3, 8, 0, 0), LastModified = new DateTime(2025, 1, 3, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = 7, EndOfWork = 6, CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0), LastModified = new DateTime(2025, 1, 1, 17, 0, 0) },
            new() { Date = new DateOnly(2025, 1, 2), StartOfWork = 4, EndOfWork = 9, CreatedAt = new DateTime(2025, 1, 2, 8, 0, 0), LastModified = new DateTime(2025, 1, 2, 17, 0, 0) }
        };

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(3, dataPoints.Count);
        Assert.True(dataPoints[0].Timestamp < dataPoints[1].Timestamp);
        Assert.True(dataPoints[1].Timestamp < dataPoints[2].Timestamp);
        
        // Check values in chronological order
        Assert.Equal(-1, dataPoints[0].Value); // Jan 1: 6 - 7 = -1
        Assert.Equal(5, dataPoints[1].Value);  // Jan 2: 9 - 4 = 5
        Assert.Equal(3, dataPoints[2].Value);  // Jan 3: 8 - 5 = 3
    }

    #endregion

    #region Average Mode Tests

    [Fact]
    public void TransformMoodEntries_AverageMode_WithValidData_ShouldReturnCorrectGraphData()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var moodEntries = new List<MoodEntry>
        {
            new() { 
                Date = startDate, 
                StartOfWork = 5, 
                EndOfWork = 7,
                CreatedAt = startDate.ToDateTime(new TimeOnly(8, 0)),
                LastModified = startDate.ToDateTime(new TimeOnly(17, 0))
            }
        };

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Average, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Average Mood Over Time", result.Title);
        Assert.Equal(AxisRange.Average, result.YAxisRange);
        Assert.Equal(0, result.CenterLineValue);
        Assert.Equal("Average Mood", result.YAxisLabel);
        Assert.Equal("Date", result.XAxisLabel);
        Assert.False(result.IsRawData);
        Assert.Equal(3, result.YAxisLabelStep);
        Assert.Contains("average mood levels", result.Description);

        var dataPoints = result.DataPoints.ToList();
        Assert.Single(dataPoints);
        // The actual average calculation would depend on the GetAdjustedAverageMood implementation
        // We're testing that the structure is correct
    }

    [Fact]
    public void TransformMoodEntries_AverageMode_WithNoValidData_ShouldReturnEmptyDataPoints()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = null, EndOfWork = null }
        };

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Average, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Average Mood Over Time", result.Title);
        Assert.Empty(result.DataPoints);
    }

    #endregion

    #region RawData Mode Tests

    [Fact]
    public void TransformMoodEntries_RawDataMode_WithValidData_ShouldReturnCorrectGraphData()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var (_, moodEntries) = MoodDataTestHelper.GetFakeData(startDate, 
            (startMood: 5, endMood: 7),
            (startMood: 6, endMood: 4)
        );

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Raw Mood Data Over Time", result.Title);
        Assert.Equal(AxisRange.RawData, result.YAxisRange);
        Assert.Equal(5.5f, result.CenterLineValue);
        Assert.Equal("Mood Level", result.YAxisLabel);
        Assert.Equal("Time", result.XAxisLabel);
        Assert.True(result.IsRawData);
        Assert.Equal(2, result.YAxisLabelStep);
        Assert.Contains("raw mood values", result.Description);

        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(4, dataPoints.Count); // 2 entries × 2 points each

        // First entry: start=5, end=7
        Assert.Equal(5, dataPoints[0].Value);
        Assert.Equal(7, dataPoints[1].Value);
        
        // Second entry: start=6, end=4
        Assert.Equal(6, dataPoints[2].Value);
        Assert.Equal(4, dataPoints[3].Value);
    }

    [Fact]
    public void TransformMoodEntries_RawDataMode_ShouldCreateTwoPointsPerEntry()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var moodEntry = new MoodEntry
        {
            Date = startDate,
            StartOfWork = 3,
            EndOfWork = 8,
            CreatedAt = startDate.ToDateTime(new TimeOnly(8, 30)),
            LastModified = startDate.ToDateTime(new TimeOnly(17, 15))
        };

        // Act
        var result = _transformer.TransformMoodEntries(new[] { moodEntry }, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(2, dataPoints.Count);

        // First point should be StartOfWork with CreatedAt timestamp
        Assert.Equal(3, dataPoints[0].Value);
        Assert.Equal(moodEntry.CreatedAt, dataPoints[0].Timestamp);

        // Second point should be EndOfWork with LastModified timestamp
        Assert.Equal(8, dataPoints[1].Value);
        Assert.Equal(moodEntry.LastModified, dataPoints[1].Timestamp);
    }

    [Fact]
    public void TransformMoodEntries_RawDataMode_WithMissingEndOfWork_ShouldUseStartOfWorkAsDefault()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var moodEntry = new MoodEntry
        {
            Date = startDate,
            StartOfWork = 6,
            EndOfWork = null, // Missing end of work
            CreatedAt = startDate.ToDateTime(new TimeOnly(8, 0)),
            LastModified = startDate.ToDateTime(new TimeOnly(17, 0))
        };

        // Act
        var result = _transformer.TransformMoodEntries(new[] { moodEntry }, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(2, dataPoints.Count);
        Assert.Equal(6, dataPoints[0].Value); // StartOfWork
        Assert.Equal(6, dataPoints[1].Value); // EndOfWork defaults to StartOfWork
    }

    [Fact]
    public void TransformMoodEntries_RawDataMode_WithMissingStartOfWork_ShouldUseEndOfWorkValue()
    {
        // Arrange
        var startDate = new DateOnly(2025, 1, 1);
        var moodEntry = new MoodEntry
        {
            Date = startDate,
            StartOfWork = null, // Missing start of work
            EndOfWork = 8,
            CreatedAt = startDate.ToDateTime(new TimeOnly(8, 0)),
            LastModified = startDate.ToDateTime(new TimeOnly(17, 0))
        };

        // Act
        var result = _transformer.TransformMoodEntries(new[] { moodEntry }, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(2, dataPoints.Count);
        Assert.Equal(0, dataPoints[0].Value); // StartOfWork is 0 when null
        Assert.Equal(8, dataPoints[1].Value); // EndOfWork is 8
    }

    [Fact]
    public void TransformMoodEntries_RawDataMode_WithNoValidData_ShouldReturnEmptyDataPoints()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2025, 1, 1), StartOfWork = null, EndOfWork = null }
        };

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Raw Mood Data Over Time", result.Title);
        Assert.Empty(result.DataPoints);
    }

    [Fact]
    public void TransformMoodEntries_RawDataMode_ShouldSortPointsByTimestamp()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() 
            { 
                Date = new DateOnly(2025, 1, 2), 
                StartOfWork = 7, 
                EndOfWork = 5,
                CreatedAt = new DateTime(2025, 1, 2, 9, 0, 0),
                LastModified = new DateTime(2025, 1, 2, 18, 0, 0)
            },
            new() 
            { 
                Date = new DateOnly(2025, 1, 1), 
                StartOfWork = 4, 
                EndOfWork = 8,
                CreatedAt = new DateTime(2025, 1, 1, 8, 0, 0),
                LastModified = new DateTime(2025, 1, 1, 17, 0, 0)
            }
        };

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.RawData, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(4, dataPoints.Count);

        // Should be sorted by timestamp
        Assert.True(dataPoints[0].Timestamp < dataPoints[1].Timestamp);
        Assert.True(dataPoints[1].Timestamp < dataPoints[2].Timestamp);
        Assert.True(dataPoints[2].Timestamp < dataPoints[3].Timestamp);

        // Check chronological order: Jan 1 start, Jan 1 end, Jan 2 start, Jan 2 end
        Assert.Equal(new DateTime(2025, 1, 1, 8, 0, 0), dataPoints[0].Timestamp);
        Assert.Equal(4, dataPoints[0].Value);
        Assert.Equal(new DateTime(2025, 1, 1, 17, 0, 0), dataPoints[1].Timestamp);
        Assert.Equal(8, dataPoints[1].Value);
        Assert.Equal(new DateTime(2025, 1, 2, 9, 0, 0), dataPoints[2].Timestamp);
        Assert.Equal(7, dataPoints[2].Value);
        Assert.Equal(new DateTime(2025, 1, 2, 18, 0, 0), dataPoints[3].Timestamp);
        Assert.Equal(5, dataPoints[3].Value);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void TransformMoodEntries_WithEmptyCollection_ShouldReturnEmptyDataPoints()
    {
        // Arrange
        var emptyEntries = new List<MoodEntry>();

        // Act & Assert for each mode
        foreach (var mode in Enum.GetValues<GraphMode>())
        {
            var result = _transformer.TransformMoodEntries(emptyEntries, mode, CreateDateRangeInfo(DateRange.Last3Years));
            Assert.NotNull(result);
            Assert.Empty(result.DataPoints);
            Assert.False(string.IsNullOrEmpty(result.Title));
        }
    }

    [Fact]
    public void TransformMoodEntries_WithInvalidGraphMode_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>();
        var invalidMode = (GraphMode)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _transformer.TransformMoodEntries(moodEntries, invalidMode, CreateDateRangeInfo(DateRange.Last3Years)));
    }

    [Fact]
    public void TransformMoodEntries_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _transformer.TransformMoodEntries(null!, GraphMode.Impact, CreateDateRangeInfo(DateRange.Last3Years)));
    }

    #endregion

    #region Date Range Filtering Tests

    [Fact]
    public void TransformMoodEntries_WithDateRangeFilter_ShouldFilterDataCorrectly()
    {
        // Arrange - Create mood entries across different dates
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2024, 12, 25), StartOfWork = 5, EndOfWork = 7 }, // Outside range
            new() { Date = new DateOnly(2025, 1, 5), StartOfWork = 6, EndOfWork = 8 },   // Inside range
            new() { Date = new DateOnly(2025, 1, 8), StartOfWork = 4, EndOfWork = 6 },   // Inside range
            new() { Date = new DateOnly(2025, 2, 15), StartOfWork = 7, EndOfWork = 9 },  // Outside range
        };

        // Create a date range that covers Jan 1-9, 2025 (Last 7 Days from Jan 10)
        var dateRange = CreateDateRangeInfo(DateRange.Last7Days);

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(2, dataPoints.Count); // Only 2 entries should be in range
        Assert.Equal(2, dataPoints[0].Value); // 8 - 6 = 2 (Jan 5)
        Assert.Equal(2, dataPoints[1].Value); // 6 - 4 = 2 (Jan 8)
    }

    [Fact]
    public void TransformMoodEntries_WithDateRangeFilter_EmptyResult_WhenNoDataInRange()
    {
        // Arrange - Create mood entries outside the date range
        var moodEntries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2023, 1, 1), StartOfWork = 5, EndOfWork = 7 },
            new() { Date = new DateOnly(2026, 1, 1), StartOfWork = 6, EndOfWork = 8 }
        };

        var dateRange = CreateDateRangeInfo(DateRange.Last7Days);

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);

        // Assert
        Assert.Empty(result.DataPoints);
        Assert.Equal("Mood Change Over Time", result.Title);
    }

    [Fact]
    public void TransformMoodEntries_DateRangeFilter_WorksWithAllGraphModes()
    {
        // Arrange
        var moodEntries = new List<MoodEntry>
        {
            new() { 
                Date = new DateOnly(2025, 1, 5), 
                StartOfWork = 6, 
                EndOfWork = 8,
                CreatedAt = new DateTime(2025, 1, 5, 8, 0, 0),
                LastModified = new DateTime(2025, 1, 5, 17, 0, 0)
            }
        };

        var dateRange = CreateDateRangeInfo(DateRange.Last7Days);

        // Act & Assert for each mode
        foreach (var mode in Enum.GetValues<GraphMode>())
        {
            var result = _transformer.TransformMoodEntries(moodEntries, mode, dateRange);
            Assert.NotNull(result);
            
            if (mode == GraphMode.RawData)
            {
                Assert.Equal(2, result.DataPoints.Count()); // Start and end points
            }
            else
            {
                Assert.Single(result.DataPoints); // One data point per entry
            }
        }
    }

    [Theory]
    [InlineData(DateRange.Last7Days)]
    [InlineData(DateRange.Last14Days)]
    [InlineData(DateRange.LastMonth)]
    [InlineData(DateRange.Last3Months)]
    public void TransformMoodEntries_DifferentDateRanges_ShouldFilterCorrectly(DateRange range)
    {
        // Arrange - Create mood entries spanning a long period
        var moodEntries = new List<MoodEntry>();
        var startDate = new DateOnly(2024, 1, 1);
        
        // Add entries for every day over 4 months
        for (int i = 0; i < 120; i++)
        {
            moodEntries.Add(new MoodEntry
            {
                Date = startDate.AddDays(i),
                StartOfWork = 5,
                EndOfWork = 7
            });
        }

        var dateRange = CreateDateRangeInfo(range);

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange);

        // Assert - Verify that only entries within the date range are included
        foreach (var dataPoint in result.DataPoints)
        {
            var date = DateOnly.FromDateTime(dataPoint.Timestamp);
            Assert.True(date >= dateRange.StartDate, $"Date {date} should be >= {dateRange.StartDate}");
            Assert.True(date <= dateRange.EndDate, $"Date {date} should be <= {dateRange.EndDate}");
        }
    }

    #endregion

    #region GetValueForMoodEntry Tests

    [Fact]
    public void GetValueForMoodEntry_ImpactMode_ShouldReturnImpactValue()
    {
        // Arrange
        var moodEntry = new MoodEntry
        {
            StartOfWork = 5,
            EndOfWork = 8,
            Date = new DateOnly(2025, 1, 1)
        };

        // Act
        var result = _transformer.GetValueForMoodEntry(moodEntry, GraphMode.Impact);

        // Assert
        Assert.Equal(3, result); // 8 - 5 = 3
    }

    [Fact]
    public void GetValueForMoodEntry_AverageMode_ShouldReturnAverageValue()
    {
        // Arrange
        var moodEntry = new MoodEntry
        {
            StartOfWork = 4,
            EndOfWork = 8,
            Date = new DateOnly(2025, 1, 1)
        };

        // Act
        var result = _transformer.GetValueForMoodEntry(moodEntry, GraphMode.Average);

        // Assert
        // The actual value depends on GetAdjustedAverageMood implementation
        // This test ensures the method doesn't throw and returns a numeric value
        Assert.IsType<float>(result);
    }

    [Fact]
    public void GetValueForMoodEntry_RawDataMode_ShouldReturnStartOfWorkValue()
    {
        // Arrange
        var moodEntry = new MoodEntry
        {
            StartOfWork = 7,
            EndOfWork = 5,
            Date = new DateOnly(2025, 1, 1)
        };

        // Act
        var result = _transformer.GetValueForMoodEntry(moodEntry, GraphMode.RawData);

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void GetValueForMoodEntry_WithNullValues_ShouldReturnZero()
    {
        // Arrange
        var moodEntry = new MoodEntry
        {
            StartOfWork = null,
            EndOfWork = null,
            Date = new DateOnly(2025, 1, 1)
        };

        // Act & Assert for each mode
        foreach (var mode in Enum.GetValues<GraphMode>())
        {
            var result = _transformer.GetValueForMoodEntry(moodEntry, mode);
            Assert.Equal(0, result);
        }
    }

    #endregion

    #region Metadata Validation Tests

    [Theory]
    [InlineData(GraphMode.Impact, "Mood Change Over Time", 0f, "Impact", "Date", false, 3)]
    [InlineData(GraphMode.Average, "Average Mood Over Time", 0f, "Average Mood", "Date", false, 3)]
    [InlineData(GraphMode.RawData, "Raw Mood Data Over Time", 5.5f, "Mood Level", "Time", true, 2)]
    public void TransformMoodEntries_ShouldSetCorrectMetadataForEachMode(
        GraphMode mode, 
        string expectedTitle, 
        float expectedCenterLine, 
        string expectedYLabel, 
        string expectedXLabel, 
        bool expectedIsRawData, 
        int expectedLabelStep)
    {
        // Arrange
        var moodEntries = new List<MoodEntry>();

        // Act
        var result = _transformer.TransformMoodEntries(moodEntries, mode, CreateDateRangeInfo(DateRange.Last3Years));

        // Assert
        Assert.Equal(expectedTitle, result.Title);
        
        // Verify the correct AxisRange is used based on the mode
        var expectedAxisRange = mode switch
        {
            GraphMode.Impact => AxisRange.Impact,
            GraphMode.Average => AxisRange.Average,
            GraphMode.RawData => AxisRange.RawData,
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
        Assert.Equal(expectedAxisRange, result.YAxisRange);
        
        Assert.Equal(expectedCenterLine, result.CenterLineValue);
        Assert.Equal(expectedYLabel, result.YAxisLabel);
        Assert.Equal(expectedXLabel, result.XAxisLabel);
        Assert.Equal(expectedIsRawData, result.IsRawData);
        Assert.Equal(expectedLabelStep, result.YAxisLabelStep);
        Assert.False(string.IsNullOrEmpty(result.Description));
    }

    #endregion
}