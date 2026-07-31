using Microsoft.Maui.Graphics;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Tests.TestHelpers;
using WorkMood.MauiApp.ViewModels;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

public class VisualizationViewModelShould
{
    [Fact]
    public async Task OnAppearingAsync_GeneratesChartUsingImpactModeAndLast14DaysEndingYesterday()
    {
        var moodDataService = new Mock<IMoodDataService>();
        var navigationService = new Mock<INavigationService>();
        var lineGraphService = new Mock<ILineGraphService>();
        var fixedToday = new DateOnly(2026, 8, 1);
        var dateShim = new FakeDateShim(fixedToday);

        var dailyValues = CreateDailyValues(fixedToday.AddDays(-14), 14, includeData: true);
        moodDataService
            .Setup(service => service.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(CreateVisualizationData(dailyValues));

        var entries = new[]
        {
            new MoodEntry(fixedToday.AddDays(-1)) { StartOfWork = 4, EndOfWork = 6 },
            new MoodEntry(fixedToday.AddDays(-2)) { StartOfWork = 5, EndOfWork = 5 }
        };

        moodDataService
            .Setup(service => service.LoadMoodDataAsync())
            .ReturnsAsync(new MoodCollection(entries));

        DateRangeInfo? capturedDateRange = null;
        GraphMode capturedMode = GraphMode.RawData;

        lineGraphService
            .Setup(service => service.GenerateGraphAsync(
                It.IsAny<IEnumerable<MoodEntry>>(),
                It.IsAny<GraphMode>(),
                It.IsAny<DateRangeInfo>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<Color>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<GapDisplayMode>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode>((_, mode, range, _, _, _, _, _, _, _, _) =>
            {
                capturedMode = mode;
                capturedDateRange = range;
            })
            .ReturnsAsync([1, 2, 3]);

        var viewModel = new VisualizationViewModel(
            moodDataService.Object,
            navigationService.Object,
            lineGraphService.Object,
            dateShim,
            autoLoad: false);

        await viewModel.OnAppearingAsync();

        Assert.Equal(GraphMode.Impact, capturedMode);
        Assert.NotNull(capturedDateRange);
        Assert.Equal(DateRange.Last14Days, capturedDateRange!.DateRange);
        Assert.Equal(fixedToday.AddDays(-1), capturedDateRange.EndDate);
        Assert.Equal(fixedToday.AddDays(-14), capturedDateRange.StartDate);

        lineGraphService.Verify(service => service.GenerateGraphAsync(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<GraphMode>(),
            It.IsAny<DateRangeInfo>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<Color>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<GapDisplayMode>()), Times.Once);
    }

    [Fact]
    public async Task OnAppearingAsync_WithNoEntries_DoesNotGenerateChart()
    {
        var moodDataService = new Mock<IMoodDataService>();
        var navigationService = new Mock<INavigationService>();
        var lineGraphService = new Mock<ILineGraphService>();

        var fixedToday = new DateOnly(2026, 8, 1);
        var dateShim = new FakeDateShim(fixedToday);
        var emptyDailyValues = CreateDailyValues(fixedToday.AddDays(-14), 14, includeData: false);

        moodDataService
            .Setup(service => service.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(CreateVisualizationData(emptyDailyValues));
        moodDataService
            .Setup(service => service.LoadMoodDataAsync())
            .ReturnsAsync(new MoodCollection());

        var viewModel = new VisualizationViewModel(
            moodDataService.Object,
            navigationService.Object,
            lineGraphService.Object,
            dateShim,
            autoLoad: false);

        await viewModel.OnAppearingAsync();

        Assert.Null(viewModel.ChartImageSource);
        Assert.False(viewModel.HasData);

        lineGraphService.Verify(service => service.GenerateGraphAsync(
            It.IsAny<IEnumerable<MoodEntry>>(),
            It.IsAny<GraphMode>(),
            It.IsAny<DateRangeInfo>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<Color>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<GapDisplayMode>()), Times.Never);
    }

    [Fact]
    public async Task OnAppearingAsync_WhenChartGenerationFails_UsesErrorFallbackState()
    {
        var moodDataService = new Mock<IMoodDataService>();
        var navigationService = new Mock<INavigationService>();
        var lineGraphService = new Mock<ILineGraphService>();

        var fixedToday = new DateOnly(2026, 8, 1);
        var dateShim = new FakeDateShim(fixedToday);

        var dailyValues = CreateDailyValues(fixedToday.AddDays(-14), 14, includeData: true);

        moodDataService
            .Setup(service => service.GetTwoWeekVisualizationAsync())
            .ReturnsAsync(CreateVisualizationData(dailyValues));
        moodDataService
            .Setup(service => service.LoadMoodDataAsync())
            .ReturnsAsync(new MoodCollection(new[]
            {
                new MoodEntry(fixedToday.AddDays(-1)) { StartOfWork = 4, EndOfWork = 6 }
            }));

        lineGraphService
            .Setup(service => service.GenerateGraphAsync(
                It.IsAny<IEnumerable<MoodEntry>>(),
                It.IsAny<GraphMode>(),
                It.IsAny<DateRangeInfo>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<Color>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<GapDisplayMode>()))
            .ThrowsAsync(new InvalidOperationException("graph failure"));

        var viewModel = new VisualizationViewModel(
            moodDataService.Object,
            navigationService.Object,
            lineGraphService.Object,
            dateShim,
            autoLoad: false);

        await viewModel.OnAppearingAsync();

        Assert.Null(viewModel.ChartImageSource);
        Assert.False(viewModel.HasData);
        Assert.Equal("No data available", viewModel.DateRangeText);
    }

    private static MoodVisualizationData CreateVisualizationData(IReadOnlyList<DailyMoodValue> dailyValues)
    {
        return new MoodVisualizationData
        {
            DailyValues = dailyValues.ToArray(),
            DisplayValues = dailyValues.ToArray(),
            StartDate = dailyValues.Count > 0 ? dailyValues[0].Date : new DateOnly(2026, 1, 1),
            EndDate = dailyValues.Count > 0 ? dailyValues[^1].Date : new DateOnly(2026, 1, 14),
            Width = 280,
            Height = 100,
            MaxAbsoluteValue = 1.0
        };
    }

    private static DailyMoodValue[] CreateDailyValues(DateOnly startDate, int count, bool includeData)
    {
        return Enumerable.Range(0, count)
            .Select(index => new DailyMoodValue
            {
                Date = startDate.AddDays(index),
                HasData = includeData,
                Value = includeData ? 1.0 : null,
                Color = includeData ? Colors.Blue : Colors.LightGray
            })
            .ToArray();
    }
}
