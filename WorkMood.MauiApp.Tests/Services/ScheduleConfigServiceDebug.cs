using System.Text.Json;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class ScheduleConfigServiceDebug
{
    [Fact]
    public void DebugOverrideIssue()
    {
        // Test ScheduleOverride creation and HasOverride
        var overrideDate = new DateOnly(2023, 10, 20);
        var newOverride = new ScheduleOverride(overrideDate, TimeSpan.FromHours(10), TimeSpan.FromHours(16));
        
        Assert.True(newOverride.HasOverride, "HasOverride should be true");
        Assert.Equal(overrideDate, newOverride.Date);
        Assert.Equal(TimeSpan.FromHours(10), newOverride.MorningTime);
        Assert.Equal(TimeSpan.FromHours(16), newOverride.EveningTime);
        
        // Test ScheduleConfig SetOverride method
        var config = new ScheduleConfig();
        config.SetOverride(overrideDate, TimeSpan.FromHours(10), TimeSpan.FromHours(16));
        
        Assert.Single(config.Overrides);
        Assert.Equal(overrideDate, config.Overrides[0].Date);
        Assert.Equal(TimeSpan.FromHours(10), config.Overrides[0].MorningTime);
        Assert.Equal(TimeSpan.FromHours(16), config.Overrides[0].EveningTime);
    }
}