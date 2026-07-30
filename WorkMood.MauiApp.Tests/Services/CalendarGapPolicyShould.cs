using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class CalendarGapPolicyShould
{
    [Theory]
    [InlineData(DayOfWeek.Monday, false)]
    [InlineData(DayOfWeek.Saturday, true)]
    [InlineData(DayOfWeek.Sunday, true)]
    public void IdentifyWeekendDays(DayOfWeek dayOfWeek, bool expected)
    {
        Assert.Equal(expected, CalendarGapPolicy.IsWeekend(dayOfWeek));
    }

    [Fact]
    public void ConsumeCompressedSlot_ForWeekdaysAndRecordedWeekendsOnly()
    {
        var wednesday = new DateOnly(2025, 1, 8);
        var saturday = new DateOnly(2025, 1, 11);
        var noRecordedDates = new HashSet<DateOnly>();
        var recordedSaturday = new HashSet<DateOnly> { saturday };

        Assert.True(CalendarGapPolicy.ShouldConsumeCompressedDaySlot(wednesday, noRecordedDates));
        Assert.False(CalendarGapPolicy.ShouldConsumeCompressedDaySlot(saturday, noRecordedDates));
        Assert.True(CalendarGapPolicy.ShouldConsumeCompressedDaySlot(saturday, recordedSaturday));
    }

    [Fact]
    public void BreakWhenWeekdayMissing_ButNotForWeekendOnlyGap()
    {
        var friday = new DateOnly(2025, 1, 3);
        var thursday = new DateOnly(2025, 1, 2);
        var monday = new DateOnly(2025, 1, 6);

        Assert.False(CalendarGapPolicy.ShouldBreakWhenWeekdaysMissing(friday, monday));
        Assert.True(CalendarGapPolicy.ShouldBreakWhenWeekdaysMissing(thursday, monday));
    }

    [Fact]
    public void BreakForRecordedSeries_WhenIntermediateWeekendHasRecordedData()
    {
        var friday = new DateOnly(2025, 1, 3);
        var monday = new DateOnly(2025, 1, 6);
        var recordedDates = new HashSet<DateOnly>
        {
            friday,
            friday.AddDays(1),
            monday
        };

        var shouldBreak = CalendarGapPolicy.ShouldBreakForRecordedSeries(friday, monday, recordedDates);

        Assert.True(shouldBreak);
    }

    [Fact]
    public void MatchProjectorPolicy_WhenNoRecordedIntermediateWeekendDatesExist()
    {
        var thursday = new DateOnly(2025, 1, 2);
        var friday = new DateOnly(2025, 1, 3);
        var monday = new DateOnly(2025, 1, 6);
        var emptyRecordedDates = new HashSet<DateOnly>();

        Assert.Equal(
            CalendarGapPolicy.ShouldBreakWhenWeekdaysMissing(friday, monday),
            CalendarGapPolicy.ShouldBreakForRecordedSeries(friday, monday, emptyRecordedDates));

        Assert.Equal(
            CalendarGapPolicy.ShouldBreakWhenWeekdaysMissing(thursday, monday),
            CalendarGapPolicy.ShouldBreakForRecordedSeries(thursday, monday, emptyRecordedDates));
    }
}
