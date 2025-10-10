using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Tests.TestHelpers;
using Xunit;

namespace WorkMood.MauiApp.Tests.Models;

/// <summary>
/// Comprehensive tests for DateRangeInfo functionality
/// </summary>
public class DateRangeInfoTests
{
    private readonly FakeDateShim _dateShim;
    private readonly DateOnly _testEndDate;

    public DateRangeInfoTests()
    {
        _testEndDate = new DateOnly(2025, 10, 9); // October 9, 2025 (yesterday from Oct 10)
        _dateShim = new FakeDateShim(new DateOnly(2025, 10, 10)); // October 10, 2025 (today)
    }

    #region Constructor with DateShim Tests

    [Fact]
    public void Constructor_WithDateShim_ShouldCallDateOnlyConstructor()
    {
        // Arrange & Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last7Days, _dateShim);

        // Assert
        Assert.Equal(DateRange.Last7Days, dateRangeInfo.DateRange);
        Assert.Equal("Last 7 Days", dateRangeInfo.DisplayName);
        Assert.Equal(_testEndDate, dateRangeInfo.EndDate);
        Assert.Equal(_testEndDate.AddDays(-6), dateRangeInfo.StartDate);
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
    public void Constructor_WithDateShim_AllDateRanges_ShouldWorkCorrectly(DateRange dateRange)
    {
        // Arrange & Act
        var dateRangeInfo = new DateRangeInfo(dateRange, _dateShim);

        // Assert
        Assert.Equal(dateRange, dateRangeInfo.DateRange);
        Assert.False(string.IsNullOrEmpty(dateRangeInfo.DisplayName));
        Assert.True(dateRangeInfo.StartDate <= dateRangeInfo.EndDate);
        Assert.Equal(_testEndDate, dateRangeInfo.EndDate);
    }

    #endregion

    #region Constructor with DateOnly Tests

    [Fact]
    public void Constructor_WithDateOnly_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var endDate = new DateOnly(2025, 6, 15);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last14Days, endDate);

        // Assert
        Assert.Equal(DateRange.Last14Days, dateRangeInfo.DateRange);
        Assert.Equal("Last 14 Days", dateRangeInfo.DisplayName);
        Assert.Equal(endDate, dateRangeInfo.EndDate);
        Assert.Equal(endDate.AddDays(-13), dateRangeInfo.StartDate);
    }

    [Theory]
    [InlineData("2025-01-15")]
    [InlineData("2025-06-30")]
    [InlineData("2025-12-31")]
    [InlineData("2024-02-29")] // Leap year
    [InlineData("2023-02-28")] // Non-leap year
    public void Constructor_WithDateOnly_DifferentEndDates_ShouldWorkCorrectly(string endDateString)
    {
        // Arrange
        var endDate = DateOnly.Parse(endDateString);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last7Days, endDate);

        // Assert
        Assert.Equal(endDate, dateRangeInfo.EndDate);
        Assert.Equal(endDate.AddDays(-6), dateRangeInfo.StartDate);
        Assert.Equal(7, (dateRangeInfo.EndDate.DayNumber - dateRangeInfo.StartDate.DayNumber) + 1);
    }

    #endregion

    #region Display Name Tests

    [Theory]
    [InlineData(DateRange.Last7Days, "Last 7 Days")]
    [InlineData(DateRange.Last14Days, "Last 14 Days")]
    [InlineData(DateRange.LastMonth, "Last Month")]
    [InlineData(DateRange.Last3Months, "Last 3 Months")]
    [InlineData(DateRange.Last6Months, "Last 6 Months")]
    [InlineData(DateRange.LastYear, "Last Year")]
    [InlineData(DateRange.Last2Years, "Last 2 Years")]
    [InlineData(DateRange.Last3Years, "Last 3 Years")]
    public void DisplayName_ShouldMatchDescriptionAttribute(DateRange dateRange, string expectedDisplayName)
    {
        // Arrange & Act
        var dateRangeInfo = new DateRangeInfo(dateRange, _testEndDate);

        // Assert
        Assert.Equal(expectedDisplayName, dateRangeInfo.DisplayName);
    }

    [Fact]
    public void DisplayName_WithInvalidEnumValue_ShouldFallbackToToString()
    {
        // Arrange
        var invalidDateRange = (DateRange)999;

        // Act
        var dateRangeInfo = new DateRangeInfo(invalidDateRange, _testEndDate);

        // Assert
        Assert.Equal("999", dateRangeInfo.DisplayName);
    }

    #endregion

    #region Date Range Calculation Tests

    [Fact]
    public void Last7Days_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 10, 9);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last7Days, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 10, 3), dateRangeInfo.StartDate); // 9 - 6 = 3
        Assert.Equal(endDate, dateRangeInfo.EndDate);
        Assert.Equal(7, (dateRangeInfo.EndDate.DayNumber - dateRangeInfo.StartDate.DayNumber) + 1);
    }

    [Fact]
    public void Last14Days_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 10, 20);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last14Days, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 10, 7), dateRangeInfo.StartDate); // 20 - 13 = 7
        Assert.Equal(endDate, dateRangeInfo.EndDate);
        Assert.Equal(14, (dateRangeInfo.EndDate.DayNumber - dateRangeInfo.StartDate.DayNumber) + 1);
    }

    [Fact]
    public void LastMonth_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 6, 15);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.LastMonth, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 5, 16), dateRangeInfo.StartDate); // 1 month back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void LastMonth_WithMonthBoundary_ShouldHandleCorrectly()
    {
        // Arrange - End of March, should go back to end of February
        var endDate = new DateOnly(2025, 3, 31);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.LastMonth, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 3, 1), dateRangeInfo.StartDate); // Feb 28 + 1 = Mar 1
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void LastMonth_WithLeapYear_ShouldHandleCorrectly()
    {
        // Arrange - March in leap year
        var endDate = new DateOnly(2024, 3, 29); // 2024 is a leap year

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.LastMonth, endDate);

        // Assert
        // AddMonths(-1) gives us Feb 29, then AddDays(1) gives us Mar 1
        Assert.Equal(new DateOnly(2024, 3, 1), dateRangeInfo.StartDate); 
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void Last3Months_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 7, 15);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last3Months, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 4, 16), dateRangeInfo.StartDate); // 3 months back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void Last6Months_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 12, 31);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last6Months, endDate);

        // Assert
        Assert.Equal(new DateOnly(2025, 7, 1), dateRangeInfo.StartDate); // 6 months back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void LastYear_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 8, 20);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.LastYear, endDate);

        // Assert
        Assert.Equal(new DateOnly(2024, 8, 21), dateRangeInfo.StartDate); // 1 year back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void LastYear_WithLeapYearTransition_ShouldHandleCorrectly()
    {
        // Arrange - From leap year to non-leap year
        var endDate = new DateOnly(2025, 2, 28); // Non-leap year

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.LastYear, endDate);

        // Assert
        // AddYears(-1) gives us Feb 28, 2024, then AddDays(1) gives us Feb 29, 2024
        Assert.Equal(new DateOnly(2024, 2, 29), dateRangeInfo.StartDate); 
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void Last2Years_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 5, 10);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last2Years, endDate);

        // Assert
        Assert.Equal(new DateOnly(2023, 5, 11), dateRangeInfo.StartDate); // 2 years back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void Last3Years_ShouldCalculateCorrectRange()
    {
        // Arrange
        var endDate = new DateOnly(2025, 1, 1);

        // Act
        var dateRangeInfo = new DateRangeInfo(DateRange.Last3Years, endDate);

        // Assert
        Assert.Equal(new DateOnly(2022, 1, 2), dateRangeInfo.StartDate); // 3 years back + 1 day
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Constructor_WithMinDate_ShouldThrowWhenSubtractingDays()
    {
        // Arrange
        var minDate = DateOnly.MinValue;

        // Act & Assert - Should throw because we can't subtract days from MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new DateRangeInfo(DateRange.Last7Days, minDate));
    }

    [Fact]
    public void Constructor_WithMaxDate_ShouldNotThrow()
    {
        // Arrange
        var maxDate = DateOnly.MaxValue;

        // Act & Assert - Should not throw
        var dateRangeInfo = new DateRangeInfo(DateRange.Last7Days, maxDate);
        Assert.Equal(maxDate, dateRangeInfo.EndDate);
    }

    [Fact]
    public void DefaultCase_ShouldFallbackToLast7Days()
    {
        // Arrange
        var invalidDateRange = (DateRange)999;
        var endDate = new DateOnly(2025, 6, 15);

        // Act
        var dateRangeInfo = new DateRangeInfo(invalidDateRange, endDate);

        // Assert - Should behave like Last7Days
        Assert.Equal(endDate.AddDays(-6), dateRangeInfo.StartDate);
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    #endregion

    #region Property Immutability Tests

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var dateRangeInfo = new DateRangeInfo(DateRange.LastMonth, _testEndDate);

        // Act & Assert - Properties should have private setters
        var dateRangeProperty = typeof(DateRangeInfo).GetProperty(nameof(DateRangeInfo.DateRange));
        var displayNameProperty = typeof(DateRangeInfo).GetProperty(nameof(DateRangeInfo.DisplayName));
        var startDateProperty = typeof(DateRangeInfo).GetProperty(nameof(DateRangeInfo.StartDate));
        var endDateProperty = typeof(DateRangeInfo).GetProperty(nameof(DateRangeInfo.EndDate));

        Assert.True(dateRangeProperty?.SetMethod?.IsPrivate ?? true);
        Assert.True(displayNameProperty?.SetMethod?.IsPrivate ?? true);
        Assert.True(startDateProperty?.SetMethod?.IsPrivate ?? true);
        Assert.True(endDateProperty?.SetMethod?.IsPrivate ?? true);
    }

    #endregion

    #region Cross-Month/Year Boundary Tests

    [Theory]
    [InlineData("2025-01-31", DateRange.LastMonth, "2025-01-01")] // January: AddMonths(-1) gives Dec 31, AddDays(1) gives Jan 1
    [InlineData("2025-03-31", DateRange.LastMonth, "2025-03-01")] // March: AddMonths(-1) gives Feb 28, AddDays(1) gives Mar 1
    [InlineData("2024-03-31", DateRange.LastMonth, "2024-03-01")] // March leap year: AddMonths(-1) gives Feb 29, AddDays(1) gives Mar 1  
    [InlineData("2025-05-31", DateRange.LastMonth, "2025-05-01")] // May: AddMonths(-1) gives Apr 30, AddDays(1) gives May 1
    public void MonthBoundary_ShouldHandleCorrectly(string endDateString, DateRange range, string expectedStartString)
    {
        // Arrange
        var endDate = DateOnly.Parse(endDateString);
        var expectedStart = DateOnly.Parse(expectedStartString);

        // Act
        var dateRangeInfo = new DateRangeInfo(range, endDate);

        // Assert
        Assert.Equal(expectedStart, dateRangeInfo.StartDate);
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    [Theory]
    [InlineData("2025-01-01", DateRange.LastYear, "2024-01-02")] // Year boundary
    [InlineData("2024-02-29", DateRange.LastYear, "2023-03-01")] // Leap year to non-leap year
    [InlineData("2023-02-28", DateRange.LastYear, "2022-03-01")] // Non-leap year to non-leap year
    public void YearBoundary_ShouldHandleCorrectly(string endDateString, DateRange range, string expectedStartString)
    {
        // Arrange
        var endDate = DateOnly.Parse(endDateString);
        var expectedStart = DateOnly.Parse(expectedStartString);

        // Act
        var dateRangeInfo = new DateRangeInfo(range, endDate);

        // Assert
        Assert.Equal(expectedStart, dateRangeInfo.StartDate);
        Assert.Equal(endDate, dateRangeInfo.EndDate);
    }

    #endregion

    #region Consistency Tests

    [Theory]
    [InlineData(DateRange.Last7Days)]
    [InlineData(DateRange.Last14Days)]
    [InlineData(DateRange.LastMonth)]
    [InlineData(DateRange.Last3Months)]
    [InlineData(DateRange.Last6Months)]
    [InlineData(DateRange.LastYear)]
    [InlineData(DateRange.Last2Years)]
    [InlineData(DateRange.Last3Years)]
    public void BothConstructors_ShouldProduceSameResult(DateRange dateRange)
    {
        // Arrange
        var testDate = new DateOnly(2025, 7, 15);
        var dateShim = new FakeDateShim(testDate.AddDays(1)); // Tomorrow

        // Act
        var fromDateShim = new DateRangeInfo(dateRange, dateShim);
        var fromDateOnly = new DateRangeInfo(dateRange, testDate);

        // Assert
        Assert.Equal(fromDateShim.DateRange, fromDateOnly.DateRange);
        Assert.Equal(fromDateShim.DisplayName, fromDateOnly.DisplayName);
        Assert.Equal(fromDateShim.StartDate, fromDateOnly.StartDate);
        Assert.Equal(fromDateShim.EndDate, fromDateOnly.EndDate);
    }

    [Fact]
    public void StartDate_ShouldAlwaysBeBeforeOrEqualToEndDate()
    {
        // Arrange
        var endDate = new DateOnly(2025, 6, 15);

        // Act & Assert
        foreach (var dateRange in Enum.GetValues<DateRange>())
        {
            var dateRangeInfo = new DateRangeInfo(dateRange, endDate);
            Assert.True(dateRangeInfo.StartDate <= dateRangeInfo.EndDate, 
                $"StartDate should be <= EndDate for {dateRange}");
        }
    }

    #endregion
}