using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class VisualizationDisplayProjectorShould
{
    [Fact]
    public void ReturnDisplayValues_WhenDisplayValuesArePresent()
    {
        var start = new DateOnly(2025, 1, 1);
        var data = new MoodVisualizationData
        {
            DailyValues =
            [
                new DailyMoodValue { Date = start, HasData = true, Value = 1.0 },
                new DailyMoodValue { Date = start.AddDays(1), HasData = true, Value = 2.0 }
            ],
            DisplayValues =
            [
                new DailyMoodValue { Date = start, HasData = true, Value = 1.0 }
            ]
        };

        var values = VisualizationDisplayProjector.GetDisplayValues(data);

        var only = Assert.Single(values);
        Assert.Equal(start, only.Date);
    }

    [Fact]
    public void FallBackToDailyValues_WhenDisplayValuesAreEmpty()
    {
        var start = new DateOnly(2025, 1, 1);
        var data = new MoodVisualizationData
        {
            DailyValues =
            [
                new DailyMoodValue { Date = start, HasData = true, Value = 1.0 },
                new DailyMoodValue { Date = start.AddDays(1), HasData = false, Value = null }
            ],
            DisplayValues = Array.Empty<DailyMoodValue>()
        };

        var values = VisualizationDisplayProjector.GetDisplayValues(data);

        Assert.Equal(2, values.Count);
        Assert.Equal(start, values[0].Date);
        Assert.Equal(start.AddDays(1), values[1].Date);
    }

    [Fact]
    public void ThrowWhenDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => VisualizationDisplayProjector.GetDisplayValues(null!));
    }
}
