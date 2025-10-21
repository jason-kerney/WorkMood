using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Shims;
using Moq;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

public class DateRangeItemShould
{
    private readonly Mock<IDateShim> _mockDateShim;

    public DateRangeItemShould()
    {
        _mockDateShim = new Mock<IDateShim>();
        // Configure the mock to return a reasonable date
        var testDate = new DateOnly(2024, 6, 15); // Middle of 2024, plenty of room to subtract
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(testDate);
    }

    [Fact]
    public void Constructor_WhenGivenValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.NotNull(item.DateRange);
        Assert.NotNull(item.DisplayName);
    }

    [Fact]
    public void Constructor_WhenDateShimIsNull_ThrowsNullReferenceException()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;

        // Act & Assert
        // Note: The current implementation throws NullReferenceException instead of ArgumentNullException
        // This indicates that null validation could be improved in the actual implementation
        Assert.Throws<NullReferenceException>(() => new DateRangeItem(dateRange, null!));
    }

    [Theory]
    [InlineData(DateRange.Last7Days)]
    [InlineData(DateRange.Last14Days)]
    [InlineData(DateRange.LastMonth)]
    [InlineData(DateRange.Last3Months)]
    [InlineData(DateRange.Last6Months)]
    [InlineData(DateRange.LastYear)]
    [InlineData(DateRange.Last2Years)]
    [InlineData(DateRange.Last3Years)]
    public void Constructor_WithAllDateRangeValues_CreatesValidObject(DateRange dateRange)
    {
        // Arrange
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.NotNull(item.DateRange);
        Assert.NotNull(item.DisplayName);
        Assert.NotEmpty(item.DisplayName);
    }

    [Fact]
    public void DateRangeProperty_IsReadOnly()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var mockDateShim = _mockDateShim.Object;
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Act & Assert
        // DateRange property should be read-only (only getter)
        var propertyInfo = typeof(DateRangeItem).GetProperty(nameof(DateRangeItem.DateRange));
        Assert.NotNull(propertyInfo);
        Assert.True(propertyInfo.CanRead);
        Assert.False(propertyInfo.CanWrite);
    }

    [Fact]
    public void DateRangeInfoProperty_IsReadOnly()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var mockDateShim = _mockDateShim.Object;
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Act & Assert
        // DateRange property should be read-only (only getter)
        var propertyInfo = typeof(DateRangeItem).GetProperty(nameof(DateRangeItem.DateRange));
        Assert.NotNull(propertyInfo);
        Assert.True(propertyInfo.CanRead);
        Assert.False(propertyInfo.CanWrite);
    }

    [Fact]
    public void DisplayNameProperty_IsReadOnly()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var mockDateShim = _mockDateShim.Object;
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Act & Assert
        // DisplayName property should be read-only (only getter)
        var propertyInfo = typeof(DateRangeItem).GetProperty(nameof(DateRangeItem.DisplayName));
        Assert.NotNull(propertyInfo);
        Assert.True(propertyInfo.CanRead);
        Assert.False(propertyInfo.CanWrite);
    }

    // Checkpoint 2: IDateShim Integration and DateRangeInfo Testing

    [Fact]
    public void Constructor_UsesDateShimForDateRangeInfoCreation()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var testDate = new DateOnly(2024, 6, 15);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(testDate);

        // Act
        var item = new DateRangeItem(dateRange, _mockDateShim.Object);

        // Assert
        _mockDateShim.Verify(x => x.GetTodayDate(), Times.Once);
        Assert.Equal(dateRange, item.DateRange.DateRange);
    }

    [Fact]
    public void Constructor_CreatesDateRangeInfoWithCorrectDateRange()
    {
        // Arrange
        var dateRange = DateRange.Last3Months;
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.True(item.DateRange.StartDate <= item.DateRange.EndDate);
    }

    [Fact]
    public void DisplayName_MatchesDateRangeInfoDisplayName()
    {
        // Arrange
        var dateRange = DateRange.Last6Months;
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.Equal(item.DateRange.DisplayName, item.DisplayName);
        Assert.NotNull(item.DisplayName);
        Assert.NotEmpty(item.DisplayName);
    }

    [Theory]
    [InlineData(DateRange.Last7Days, "Last 7 Days")]
    [InlineData(DateRange.Last14Days, "Last 14 Days")]
    [InlineData(DateRange.LastMonth, "Last Month")]
    [InlineData(DateRange.Last3Months, "Last 3 Months")]
    [InlineData(DateRange.Last6Months, "Last 6 Months")]
    [InlineData(DateRange.LastYear, "Last Year")]
    [InlineData(DateRange.Last2Years, "Last 2 Years")]
    [InlineData(DateRange.Last3Years, "Last 3 Years")]
    public void DisplayName_ReflectsCorrectDescriptionForDateRange(DateRange dateRange, string expectedDescription)
    {
        // Arrange
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.Equal(expectedDescription, item.DisplayName);
    }

    [Fact]
    public void DateRangeInfo_HasValidStartAndEndDates()
    {
        // Arrange
        var dateRange = DateRange.LastYear;
        var testToday = new DateOnly(2024, 6, 15);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(testToday);
        var expectedEndDate = testToday.AddDays(-1); // Yesterday
        var expectedStartDate = expectedEndDate.AddYears(-1).AddDays(1);

        // Act
        var item = new DateRangeItem(dateRange, _mockDateShim.Object);

        // Assert
        Assert.Equal(expectedStartDate, item.DateRange.StartDate);
        Assert.Equal(expectedEndDate, item.DateRange.EndDate);
        Assert.True(item.DateRange.StartDate <= item.DateRange.EndDate);
    }

    // Checkpoint 3: Edge Cases and Performance Testing

    [Fact]
    public void Constructor_WithDateAtMinimumYear_HandlesEdgeCaseGracefully()
    {
        // Arrange
        var dateRange = DateRange.Last3Years;
        var testDate = new DateOnly(2020, 1, 10); // Early date to test edge cases
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(testDate);

        // Act
        var item = new DateRangeItem(dateRange, _mockDateShim.Object);

        // Assert
        Assert.NotNull(item.DateRange);
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.True(item.DateRange.StartDate <= item.DateRange.EndDate);
    }

    [Fact]
    public void Constructor_WithDateAtMaximumYear_HandlesEdgeCaseGracefully()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var testDate = new DateOnly(2030, 12, 20); // Future date to test edge cases
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(testDate);

        // Act
        var item = new DateRangeItem(dateRange, _mockDateShim.Object);

        // Assert
        Assert.NotNull(item.DateRange);
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.True(item.DateRange.StartDate <= item.DateRange.EndDate);
    }

    [Fact]
    public void Constructor_PerformanceTest_CreatesObjectQuickly()
    {
        // Arrange
        var dateRange = DateRange.LastMonth;
        var mockDateShim = _mockDateShim.Object;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var item = new DateRangeItem(dateRange, mockDateShim);
        }
        stopwatch.Stop();

        // Assert
        // Should create 1000 objects in less than 100ms (reasonable performance expectation)
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"Performance test failed: took {stopwatch.ElapsedMilliseconds}ms to create 1000 objects");
    }

    [Theory]
    [InlineData(DateRange.Last7Days)]
    [InlineData(DateRange.Last14Days)]
    [InlineData(DateRange.LastMonth)]
    [InlineData(DateRange.Last3Months)]
    [InlineData(DateRange.Last6Months)]
    [InlineData(DateRange.LastYear)]
    [InlineData(DateRange.Last2Years)]
    [InlineData(DateRange.Last3Years)]
    public void Constructor_WithLeapYearDate_HandlesAllDateRanges(DateRange dateRange)
    {
        // Arrange - Use a leap year date
        var leapYearDate = new DateOnly(2024, 2, 29); // Leap year Feb 29
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(leapYearDate);

        // Act
        var item = new DateRangeItem(dateRange, _mockDateShim.Object);

        // Assert
        Assert.Equal(dateRange, item.DateRange.DateRange);
        Assert.True(item.DateRange.StartDate <= item.DateRange.EndDate);
        Assert.NotEmpty(item.DisplayName);
    }

    [Fact]
    public void Constructor_CreatesTwoInstancesWithSameParameters_AreIndependent()
    {
        // Arrange
        var dateRange = DateRange.LastYear;
        var mockDateShim = _mockDateShim.Object;

        // Act
        var item1 = new DateRangeItem(dateRange, mockDateShim);
        var item2 = new DateRangeItem(dateRange, mockDateShim);

        // Assert
        Assert.NotSame(item1, item2); // Different object instances
        Assert.Equal(item1.DateRange.DateRange, item2.DateRange.DateRange); // Same properties
        Assert.Equal(item1.DisplayName, item2.DisplayName); // Same display name
        Assert.NotSame(item1.DateRange, item2.DateRange); // Different DateRangeInfo instances
    }

    [Fact]
    public void Constructor_WithDifferentDateShimConfigurations_ProducesDifferentResults()
    {
        // Arrange
        var dateRange = DateRange.Last7Days;
        var mockDateShim1 = new Mock<IDateShim>();
        var mockDateShim2 = new Mock<IDateShim>();
        
        var date1 = new DateOnly(2024, 1, 15);
        var date2 = new DateOnly(2024, 6, 15);
        
        mockDateShim1.Setup(x => x.GetTodayDate()).Returns(date1);
        mockDateShim2.Setup(x => x.GetTodayDate()).Returns(date2);

        // Act
        var item1 = new DateRangeItem(dateRange, mockDateShim1.Object);
        var item2 = new DateRangeItem(dateRange, mockDateShim2.Object);

        // Assert
        Assert.Equal(item1.DateRange.DateRange, item2.DateRange.DateRange); // Same enum
        Assert.Equal(item1.DisplayName, item2.DisplayName); // Same display name
        Assert.NotEqual(item1.DateRange.StartDate, item2.DateRange.StartDate); // Different dates
        Assert.NotEqual(item1.DateRange.EndDate, item2.DateRange.EndDate); // Different dates
    }
}