using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Tests.TestHelpers;
using WorkMood.MauiApp.ViewModels;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

public class GraphViewModelShould
{
    private readonly Mock<IMoodDataService> _mockMoodDataService;
    private readonly Mock<ILineGraphService> _mockLineGraphService;

    public GraphViewModelShould()
    {
        _mockMoodDataService = new Mock<IMoodDataService>();
        _mockLineGraphService = new Mock<ILineGraphService>();
    }

    [Fact]
    public void IsGraphConfigVisible_DefaultsToTrue()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.IsGraphConfigVisible);
    }

    [Fact]
    public void ToggleGraphConfigCommand_TogglesVisibility()
    {
        var viewModel = CreateViewModel();

        viewModel.ToggleGraphConfigCommand.Execute(null);

        Assert.False(viewModel.IsGraphConfigVisible);

        viewModel.ToggleGraphConfigCommand.Execute(null);

        Assert.True(viewModel.IsGraphConfigVisible);
    }

    [Fact]
    public void GraphConfigToggleText_ReflectsVisibility()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("Hide Graph Controls", viewModel.GraphConfigToggleText);

        viewModel.ToggleGraphConfigCommand.Execute(null);

        Assert.Equal("Show Graph Controls", viewModel.GraphConfigToggleText);
    }

    [Fact]
    public void ToggleGraphConfigCommand_WhenCollapsing_ClosesColorPicker()
    {
        var viewModel = CreateViewModel();
        viewModel.IsColorPickerVisible = true;

        viewModel.ToggleGraphConfigCommand.Execute(null);

        Assert.False(viewModel.IsGraphConfigVisible);
        Assert.False(viewModel.IsColorPickerVisible);
    }

    [Fact]
    public void ToggleGraphConfigCommand_WhenCollapsingWhileColorPickerAlreadyHidden_LeavesPickerHiddenAndDoesNotCorruptState()
    {
        var viewModel = CreateViewModel();
        viewModel.IsColorPickerVisible = false;

        viewModel.ToggleGraphConfigCommand.Execute(null);

        Assert.False(viewModel.IsGraphConfigVisible);
        Assert.False(viewModel.IsColorPickerVisible);
        Assert.Equal("Show Graph Controls", viewModel.GraphConfigToggleText);
    }

    [Fact]
    public void ToggleGraphConfigCommand_RepeatedToggles_StayConsistentForVisibilityTextAndColorPicker()
    {
        var viewModel = CreateViewModel();
        viewModel.IsColorPickerVisible = true;

        viewModel.ToggleGraphConfigCommand.Execute(null); // collapse
        viewModel.ToggleGraphConfigCommand.Execute(null); // expand
        viewModel.ToggleGraphConfigCommand.Execute(null); // collapse

        Assert.False(viewModel.IsGraphConfigVisible);
        Assert.Equal("Show Graph Controls", viewModel.GraphConfigToggleText);
        Assert.False(viewModel.IsColorPickerVisible);
    }

    [Fact]
    public void DisplayHeight_WhenContainerHeightIncreases_Increases()
    {
        var viewModel = CreateViewModel();

        viewModel.UpdateContainerSize(1200, 300);
        var smallerHeightDisplay = viewModel.DisplayHeight;

        viewModel.UpdateContainerSize(1200, 700);
        var largerHeightDisplay = viewModel.DisplayHeight;

        Assert.True(largerHeightDisplay > smallerHeightDisplay);
    }

    [Fact]
    public async Task LoadDataAsync_WhenMissingWeekdaysAsZeroIsEnabled_ShouldRequestGapsAsZero()
    {
        var capturedGapDisplayMode = GapDisplayMode.ShowGaps;
        var viewModel = CreateViewModel();

        _mockMoodDataService
            .Setup(service => service.LoadMoodDataAsync())
            .ReturnsAsync(new MoodCollection(new[]
            {
                new MoodEntry(new DateOnly(2026, 7, 29)) { StartOfWork = 4, EndOfWork = 6 }
            }));

        _mockLineGraphService
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
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode>((_, _, _, _, _, _, _, _, _, _, gapDisplayMode) =>
            {
                capturedGapDisplayMode = gapDisplayMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.ShowMissingWeekdaysAsZero = true;

        await viewModel.LoadDataAsync();

        Assert.Equal(GapDisplayMode.GapsAsZero, capturedGapDisplayMode);
    }

    private GraphViewModel CreateViewModel()
    {
        return new GraphViewModel(
            _mockMoodDataService.Object,
            _mockLineGraphService.Object,
            new FakeDateShim(new DateOnly(2026, 7, 30)));
    }
}