using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Adapters;
using WorkMood.MauiApp.Processors;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Adapters;

public class VisualizationDataAdapterShould
{
    private readonly Mock<IMoodDataService> _mockMoodDataService;

    public VisualizationDataAdapterShould()
    {
        _mockMoodDataService = new Mock<IMoodDataService>();
    }

    private MoodVisualizationData CreateSampleVisualizationData()
    {
        return new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 14),
            Width = 800,
            Height = 400,
            MaxAbsoluteValue = 3.0,
            DailyValues = new[]
            {
                new DailyMoodValue { Date = new DateOnly(2024, 10, 1), Value = 1.5, HasData = true, Color = Colors.Green },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 2), Value = -0.5, HasData = true, Color = Colors.Orange },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 3), Value = null, HasData = false, Color = Colors.Transparent },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 4), Value = 2.5, HasData = true, Color = Colors.Blue },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 5), Value = 0.0, HasData = true, Color = Colors.Gray }
            }
        };
    }

    #region GetMoodDayInfoListAsync Tests

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldReturnCorrectCount_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal(5, result.Count);
        _mockMoodDataService.Verify(x => x.GetTwoWeekVisualizationAsync(), Times.Once);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapDateCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal(new DateOnly(2024, 10, 1), result[0].Date);
        Assert.Equal(new DateOnly(2024, 10, 2), result[1].Date);
        Assert.Equal(new DateOnly(2024, 10, 3), result[2].Date);
        Assert.Equal(new DateOnly(2024, 10, 4), result[3].Date);
        Assert.Equal(new DateOnly(2024, 10, 5), result[4].Date);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapValueCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal(1.5, result[0].Value);
        Assert.Equal(-0.5, result[1].Value);
        Assert.Null(result[2].Value);
        Assert.Equal(2.5, result[3].Value);
        Assert.Equal(0.0, result[4].Value);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapHasDataCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.True(result[0].HasData);
        Assert.True(result[1].HasData);
        Assert.False(result[2].HasData);
        Assert.True(result[3].HasData);
        Assert.True(result[4].HasData);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapColorHexCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal("#008000", result[0].ColorHex); // Green
        Assert.Equal("#FFA500", result[1].ColorHex); // Orange
        Assert.Equal("#000000", result[2].ColorHex); // Transparent (RGB only - no alpha)
        Assert.Equal("#0000FF", result[3].ColorHex); // Blue
        Assert.Equal("#808080", result[4].ColorHex); // Gray
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapDayOfWeekCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        // October 1, 2024 is a Tuesday
        Assert.Equal("Tuesday", result[0].DayOfWeek);
        Assert.Equal("Wednesday", result[1].DayOfWeek);
        Assert.Equal("Thursday", result[2].DayOfWeek);
        Assert.Equal("Friday", result[3].DayOfWeek);
        Assert.Equal("Saturday", result[4].DayOfWeek);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMapValueDescriptionCorrectly_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal("Moderately improved", result[0].ValueDescription); // 1.5
        Assert.Equal("Slightly declined", result[1].ValueDescription); // -0.5
        Assert.Equal("No data", result[2].ValueDescription); // null
        Assert.Equal("Significantly improved", result[3].ValueDescription); // 2.5
        Assert.Equal("No change", result[4].ValueDescription); // 0.0
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldReturnEmptyList_WhenVisualizationDataHasNoDailyValues()
    {
        // Arrange
        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 14),
            DailyValues = Array.Empty<DailyMoodValue>()
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldThrowNullReferenceException_WhenMoodDataServiceIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            VisualizationDataAdapter.GetMoodDayInfoListAsync(null!));
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldHandleServiceException_WhenServiceThrows()
    {
        // Arrange
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object));
        
        Assert.Equal("Service error", exception.Message);
    }

    #endregion

    #region GetVisualizationSummaryAsync Tests

    [Fact]
    public async Task GetVisualizationSummaryAsync_ShouldCallService_WhenCalled()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetVisualizationSummaryAsync(_mockMoodDataService.Object);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _mockMoodDataService.Verify(x => x.GetTwoWeekVisualizationAsync(), Times.Once);
    }

    [Fact]
    public async Task GetVisualizationSummaryAsync_ShouldReturnFormattedSummary_WhenVisualizationDataProvided()
    {
        // Arrange
        var visualizationData = CreateSampleVisualizationData();
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetVisualizationSummaryAsync(_mockMoodDataService.Object);

        // Assert
        // Should delegate to MoodVisualizationFormatter.GetVisualizationSummary
        Assert.Contains("days of data", result);
        Assert.Contains("2024-10-01", result);
        Assert.Contains("2024-10-14", result);
    }

    [Fact]
    public async Task GetVisualizationSummaryAsync_ShouldThrowNullReferenceException_WhenMoodDataServiceIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            VisualizationDataAdapter.GetVisualizationSummaryAsync(null!));
    }

    [Fact]
    public async Task GetVisualizationSummaryAsync_ShouldHandleServiceException_WhenServiceThrows()
    {
        // Arrange
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            VisualizationDataAdapter.GetVisualizationSummaryAsync(_mockMoodDataService.Object));
        
        Assert.Equal("Service error", exception.Message);
    }

    #endregion

    #region Color Hex Conversion Tests (Via Integration)

    [Theory]
    [InlineData(1.0, 0.0, 0.0, "#FF0000")] // Red
    [InlineData(0.0, 1.0, 0.0, "#00FF00")] // Green  
    [InlineData(0.0, 0.0, 1.0, "#0000FF")] // Blue
    [InlineData(1.0, 1.0, 1.0, "#FFFFFF")] // White
    [InlineData(0.0, 0.0, 0.0, "#000000")] // Black
    [InlineData(0.5, 0.5, 0.5, "#7F7F7F")] // Gray (0.5 * 255 = 127.5 → 127 → 0x7F)
    [InlineData(1.0, 0.5, 0.0, "#FF7F00")] // Orange (0.5 * 255 = 127.5 → 127 → 0x7F)
    public async Task GetMoodDayInfoListAsync_ShouldConvertColorToHexCorrectly_WhenGivenSpecificColors(
        float red, float green, float blue, string expectedHex)
    {
        // Arrange
        var color = new Color(red, green, blue);
        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 1),
            DailyValues = new[]
            {
                new DailyMoodValue { Date = new DateOnly(2024, 10, 1), Value = 1.0, HasData = true, Color = color }
            }
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedHex, result[0].ColorHex);
    }

    #endregion

    #region Value Description Tests (Via Integration)

    [Theory]
    [InlineData(3.0, "Significantly improved")]
    [InlineData(2.5, "Significantly improved")]
    [InlineData(2.0, "Significantly improved")]
    [InlineData(1.5, "Moderately improved")]
    [InlineData(1.0, "Moderately improved")]
    [InlineData(0.5, "Slightly improved")]
    [InlineData(0.1, "Slightly improved")]
    [InlineData(0.0, "No change")]
    [InlineData(-0.1, "Slightly declined")]
    [InlineData(-0.5, "Slightly declined")]
    [InlineData(-1.0, "Moderately declined")]
    [InlineData(-1.5, "Moderately declined")]
    [InlineData(-2.0, "Moderately declined")]
    [InlineData(-2.5, "Significantly declined")]
    [InlineData(-3.0, "Significantly declined")]
    public async Task GetMoodDayInfoListAsync_ShouldConvertValueToDescriptionCorrectly_WhenGivenSpecificValues(
        double value, string expectedDescription)
    {
        // Arrange
        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 1),
            DailyValues = new[]
            {
                new DailyMoodValue { Date = new DateOnly(2024, 10, 1), Value = value, HasData = true, Color = Colors.Blue }
            }
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedDescription, result[0].ValueDescription);
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldReturnNoDataDescription_WhenValueIsNull()
    {
        // Arrange
        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 1),
            DailyValues = new[]
            {
                new DailyMoodValue { Date = new DateOnly(2024, 10, 1), Value = null, HasData = false, Color = Colors.Transparent }
            }
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal("No data", result[0].ValueDescription);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldHandleLargeDataSet_WhenManyDailyValuesProvided()
    {
        // Arrange
        var dailyValues = new List<DailyMoodValue>();
        for (int i = 0; i < 100; i++)
        {
            dailyValues.Add(new DailyMoodValue
            {
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Value = i % 10 - 5, // Values from -5 to 4
                HasData = i % 3 != 0, // Some missing data
                Color = new Color(i % 255 / 255f, (i * 2) % 255 / 255f, (i * 3) % 255 / 255f)
            });
        }

        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 4, 10),
            DailyValues = dailyValues.ToArray()
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert
        Assert.Equal(100, result.Count);
        Assert.All(result, moodDay =>
        {
            Assert.NotNull(moodDay.DayOfWeek);
            Assert.NotEmpty(moodDay.DayOfWeek);
            Assert.NotNull(moodDay.ColorHex);
            Assert.NotEmpty(moodDay.ColorHex);
            Assert.NotNull(moodDay.ValueDescription);
            Assert.NotEmpty(moodDay.ValueDescription);
        });
    }

    [Fact]
    public async Task GetMoodDayInfoListAsync_ShouldMaintainOrder_WhenDailyValuesProvided()
    {
        // Arrange
        var visualizationData = new MoodVisualizationData
        {
            StartDate = new DateOnly(2024, 10, 1),
            EndDate = new DateOnly(2024, 10, 7),
            DailyValues = new[]
            {
                new DailyMoodValue { Date = new DateOnly(2024, 10, 7), Value = 1.0, HasData = true, Color = Colors.Red },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 3), Value = 2.0, HasData = true, Color = Colors.Green },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 1), Value = 3.0, HasData = true, Color = Colors.Blue },
                new DailyMoodValue { Date = new DateOnly(2024, 10, 5), Value = 4.0, HasData = true, Color = Colors.Yellow }
            }
        };
        _mockMoodDataService.Setup(x => x.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(visualizationData);

        // Act
        var result = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_mockMoodDataService.Object);

        // Assert - Should maintain the order from the source data
        Assert.Equal(4, result.Count);
        Assert.Equal(new DateOnly(2024, 10, 7), result[0].Date);
        Assert.Equal(new DateOnly(2024, 10, 3), result[1].Date);
        Assert.Equal(new DateOnly(2024, 10, 1), result[2].Date);
        Assert.Equal(new DateOnly(2024, 10, 5), result[3].Date);
    }

    #endregion
}