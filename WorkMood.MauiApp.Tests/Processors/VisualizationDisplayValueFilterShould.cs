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
