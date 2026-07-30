using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class VisualizationDisplayProjectorShould
{
    [Fact]
    public void NotBreakForFridayToMonday()
    {
        var friday = new DateOnly(2025, 1, 3);
        var monday = new DateOnly(2025, 1, 6);

        var shouldBreak = VisualizationDisplayProjector.ShouldBreakDisplaySegment(friday, monday);

        Assert.False(shouldBreak);
    }

    [Fact]
    public void BreakForThursdayToMonday_WhenWeekdayIsMissing()
    {
        var thursday = new DateOnly(2025, 1, 2);
        var monday = new DateOnly(2025, 1, 6);

        var shouldBreak = VisualizationDisplayProjector.ShouldBreakDisplaySegment(thursday, monday);

        Assert.True(shouldBreak);
    }

    [Fact]
    public void NotBreakForNonIncreasingDates()
    {
        var date = new DateOnly(2025, 1, 6);

        Assert.False(VisualizationDisplayProjector.ShouldBreakDisplaySegment(date, date));
        Assert.False(VisualizationDisplayProjector.ShouldBreakDisplaySegment(date, date.AddDays(-1)));
    }
}
