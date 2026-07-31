using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Processors;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Processors;

public class VisualizationDisplayValueFilterShould
{
    [Fact]
    public void ThrowWhenDailyValuesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => VisualizationDisplayValueFilter.BuildDisplayValues(null!));
    }

    [Fact]
    public void ExcludeUnrecordedWeekendDays()
    {
        var startDate = new DateOnly(2025, 7, 7); // Monday
        var dailyValues = CreateSequentialDailyValues(startDate);

        dailyValues[5] = CreateDailyValue(startDate.AddDays(5), hasData: false, value: null);
        dailyValues[6] = CreateDailyValue(startDate.AddDays(6), hasData: false, value: null);

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues);

        Assert.Equal(12, result.Length);
        Assert.DoesNotContain(result, value => value.Date == startDate.AddDays(5));
        Assert.DoesNotContain(result, value => value.Date == startDate.AddDays(6));
    }

    [Fact]
    public void RetainRecordedWeekendDays()
    {
        var startDate = new DateOnly(2025, 7, 7); // Monday
        var dailyValues = CreateSequentialDailyValues(startDate);

        dailyValues[5] = CreateDailyValue(startDate.AddDays(5), hasData: true, value: 1.5);
        dailyValues[6] = CreateDailyValue(startDate.AddDays(6), hasData: false, value: null);

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues);

        Assert.Contains(result, value => value.Date == startDate.AddDays(5));
        Assert.DoesNotContain(result, value => value.Date == startDate.AddDays(6));
    }

    [Fact]
    public void ReturnEmpty_WhenInputIsEmpty()
    {
        var result = VisualizationDisplayValueFilter.BuildDisplayValues(Array.Empty<DailyMoodValue>());

        Assert.Empty(result);
    }

    [Fact]
    public void ReturnEmpty_WhenAllDaysAreUnrecordedWeekends()
    {
        // Saturday/Sunday pair, both unrecorded.
        var saturday = new DateOnly(2025, 7, 12);
        var dailyValues = new[]
        {
            CreateDailyValue(saturday, hasData: false, value: null),
            CreateDailyValue(saturday.AddDays(1), hasData: false, value: null)
        };

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues);

        Assert.Empty(result);
    }

    [Fact]
    public void RetainAllDays_WhenAllDaysAreRecorded()
    {
        var startDate = new DateOnly(2025, 7, 7); // Monday
        var dailyValues = CreateSequentialDailyValues(startDate); // 14 days, all HasData = true, including the weekend.

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues);

        Assert.Equal(14, result.Length);
    }

    [Fact]
    public void ThrowArgumentNullException_WhenAnElementIsNull()
    {
        var startDate = new DateOnly(2025, 7, 7); // Monday
        var dailyValues = CreateSequentialDailyValues(startDate);
        dailyValues[3] = null!;

        Assert.Throws<ArgumentNullException>(() => VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues));
    }

    [Fact]
    public void DuplicateDates_AreDecidedByCombinedRecordedStatusAcrossAllEntriesSharingThatDate()
    {
        // Documents a known, intentional edge behavior: BuildDisplayValues assumes at most one
        // entry per DateOnly. Production callers (VisualizationDataProcessor) never violate this.
        // If violated, inclusion for an unrecorded weekend date is decided by whether ANY entry
        // sharing that date is recorded, not by each entry's own HasData/Value independently.
        var saturday = new DateOnly(2025, 7, 12);
        var dailyValues = new[]
        {
            CreateDailyValue(saturday, hasData: false, value: null),
            CreateDailyValue(saturday, hasData: true, value: 2.0)
        };

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(dailyValues);

        Assert.Equal(2, result.Length);
        Assert.All(result, value => Assert.Equal(saturday, value.Date));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, false, false, true)]
    [InlineData(DayOfWeek.Monday, true, true, true)]
    [InlineData(DayOfWeek.Saturday, false, false, false)]
    [InlineData(DayOfWeek.Saturday, true, true, true)]
    [InlineData(DayOfWeek.Sunday, false, false, false)]
    [InlineData(DayOfWeek.Sunday, true, true, true)]
    public void MatchCalendarGapPolicyDecision_ForRepresentativeDateAndRecordedStatusCombinations(
        DayOfWeek dayOfWeek, bool hasData, bool valueHasValue, bool expectedIncluded)
    {
        // Consolidation guard: prevents this filter from silently re-diverging from
        // CalendarGapPolicy.ShouldConsumeCompressedDaySlot in the future. The actual
        // pre/post behavior-preservation evidence for this refactor is
        // ExcludeUnrecordedWeekendDays and RetainRecordedWeekendDays above.
        var date = FindDateFor(dayOfWeek);
        var dailyValue = CreateDailyValue(date, hasData, valueHasValue ? 1.0 : null);

        var result = VisualizationDisplayValueFilter.BuildDisplayValues(new[] { dailyValue });

        var recordedDates = hasData && valueHasValue
            ? new HashSet<DateOnly> { date }
            : new HashSet<DateOnly>();
        var expectedFromPolicy = CalendarGapPolicy.ShouldConsumeCompressedDaySlot(date, recordedDates);

        Assert.Equal(expectedIncluded, expectedFromPolicy);
        Assert.Equal(expectedIncluded, result.Length == 1);
    }

    private static DateOnly FindDateFor(DayOfWeek dayOfWeek)
    {
        var date = new DateOnly(2025, 7, 7); // Monday
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static DailyMoodValue[] CreateSequentialDailyValues(DateOnly startDate)
    {
        return Enumerable.Range(0, 14)
            .Select(day => CreateDailyValue(startDate.AddDays(day), hasData: true, value: 1.0))
            .ToArray();
    }

    private static DailyMoodValue CreateDailyValue(DateOnly date, bool hasData, double? value)
    {
        return new DailyMoodValue
        {
            Date = date,
            HasData = hasData,
            Value = value,
            Color = hasData && value.HasValue ? Colors.Blue : Colors.LightGray
        };
    }
}
