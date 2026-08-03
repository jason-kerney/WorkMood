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
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMin_ShouldRequestGapsAsMin()
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, gapDisplayMode, _) =>
            {
                capturedGapDisplayMode = gapDisplayMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMin);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapDisplayMode.GapsAsMin, capturedGapDisplayMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMinAndGapSegmentSecondaryPenModeChanges_ShouldRequestSelectedSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = null;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMin);
        viewModel.SelectedGapSegmentSecondaryPenModeItem = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.GapSegmentSecondaryPenMode == GapSegmentSecondaryPenMode.FirstTriadic);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapSegmentSecondaryPenMode.FirstTriadic, capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMinAndGapFillColorIsMatchLineColor_ShouldRequestNullSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = GapSegmentSecondaryPenMode.Complementary;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMin);

        var matchLineColorOption = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.DisplayName == "Match Line Color");
        viewModel.SelectedGapSegmentSecondaryPenModeItem = matchLineColorOption;

        await viewModel.LoadDataAsync();

        Assert.Null(capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsShowGaps_ShouldNotRequestSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = GapSegmentSecondaryPenMode.Complementary;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.ShowGaps);
        viewModel.SelectedGapSegmentSecondaryPenModeItem = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.GapSegmentSecondaryPenMode == GapSegmentSecondaryPenMode.FirstTriadic);

        await viewModel.LoadDataAsync();

        Assert.Null(capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMax_ShouldRequestSelectedSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = null;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMax);
        viewModel.SelectedGapSegmentSecondaryPenModeItem = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.GapSegmentSecondaryPenMode == GapSegmentSecondaryPenMode.FirstTriadic);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapSegmentSecondaryPenMode.FirstTriadic, capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsAverage_ShouldRequestSelectedSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = null;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsAverage);
        viewModel.SelectedGapSegmentSecondaryPenModeItem = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.GapSegmentSecondaryPenMode == GapSegmentSecondaryPenMode.FirstTriadic);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapSegmentSecondaryPenMode.FirstTriadic, capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsSurroundingAverage_ShouldRequestSelectedSecondaryPenMode()
    {
        GapSegmentSecondaryPenMode? capturedGapSegmentSecondaryPenMode = null;
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, _, gapSegmentSecondaryPenMode) =>
            {
                capturedGapSegmentSecondaryPenMode = gapSegmentSecondaryPenMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsSurroundingAverage);
        viewModel.SelectedGapSegmentSecondaryPenModeItem = viewModel.GapSegmentSecondaryPenModes.Single(mode => mode.GapSegmentSecondaryPenMode == GapSegmentSecondaryPenMode.FirstTriadic);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapSegmentSecondaryPenMode.FirstTriadic, capturedGapSegmentSecondaryPenMode);
    }

    [Fact]
    public void GapDisplayModes_ShouldIncludeMatchPreviousAndMatchFollowingOptions()
    {
        var viewModel = CreateViewModel();

        Assert.Contains(viewModel.GapDisplayModes, mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMatchPreviousValue);
        Assert.Contains(viewModel.GapDisplayModes, mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMatchFollowingValue);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMatchPreviousValue_ShouldRequestSelectedGapDisplayMode()
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, gapDisplayMode, _) =>
            {
                capturedGapDisplayMode = gapDisplayMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMatchPreviousValue);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapDisplayMode.GapsAsMatchPreviousValue, capturedGapDisplayMode);
    }

    [Fact]
    public async Task LoadDataAsync_WhenGapDisplayModeIsGapsAsMatchFollowingValue_ShouldRequestSelectedGapDisplayMode()
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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, _, _, gapDisplayMode, _) =>
            {
                capturedGapDisplayMode = gapDisplayMode;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedGapDisplayModeItem = viewModel.GapDisplayModes.Single(mode => mode.GapDisplayMode == GapDisplayMode.GapsAsMatchFollowingValue);

        await viewModel.LoadDataAsync();

        Assert.Equal(GapDisplayMode.GapsAsMatchFollowingValue, capturedGapDisplayMode);
    }

    [Fact]
    public async Task LoadDataAsync_ShouldPassFullHistoryEntriesToGraphService_EvenWhenSomeAreOutsideSelectedRange()
    {
        IEnumerable<MoodEntry>? capturedEntries = null;
        var viewModel = CreateViewModel();

        var inRangeEntry = new MoodEntry(new DateOnly(2026, 7, 29))
        {
            StartOfWork = 4,
            EndOfWork = 6,
            CreatedAt = new DateTime(2026, 7, 29, 8, 0, 0),
            LastModified = new DateTime(2026, 7, 29, 17, 0, 0)
        };

        var outOfRangeEntry = new MoodEntry(new DateOnly(2026, 6, 1))
        {
            StartOfWork = 7,
            EndOfWork = 8,
            CreatedAt = new DateTime(2026, 6, 1, 8, 0, 0),
            LastModified = new DateTime(2026, 6, 1, 17, 0, 0)
        };

        _mockMoodDataService
            .Setup(service => service.LoadMoodDataAsync())
            .ReturnsAsync(new MoodCollection(new[] { inRangeEntry, outOfRangeEntry }));

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
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((entries, _, _, _, _, _, _, _, _, _, _, _) =>
            {
                capturedEntries = entries;
            })
            .ReturnsAsync([1, 2, 3]);

        await viewModel.LoadDataAsync();

        Assert.NotNull(capturedEntries);
        var entriesList = capturedEntries!.ToList();
        Assert.Equal(2, entriesList.Count);
        Assert.Contains(entriesList, entry => entry.Date == new DateOnly(2026, 7, 29));
        Assert.Contains(entriesList, entry => entry.Date == new DateOnly(2026, 6, 1));
    }

    [Fact]
    public void SelectedBackgroundColor_DefaultsToWhite_AndNotCustomized()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(Colors.White, viewModel.SelectedBackgroundColor);
        Assert.False(viewModel.IsBackgroundColorCustomized);
    }

    [Fact]
    public void BackgroundColorPicker_AllowsSelection_AndResetCommandReturnsToWhite()
    {
        var viewModel = CreateViewModel();

        viewModel.ToggleBackgroundColorPickerCommand.Execute(null);
        viewModel.SelectColorCommand.Execute("Red");

        Assert.Equal(Colors.Red, viewModel.SelectedBackgroundColor);
        Assert.True(viewModel.IsBackgroundColorCustomized);
        Assert.False(viewModel.IsColorPickerVisible);

        viewModel.ResetBackgroundColorCommand.Execute(null);

        Assert.Equal(Colors.White, viewModel.SelectedBackgroundColor);
        Assert.False(viewModel.IsBackgroundColorCustomized);
    }

    [Fact]
    public async Task LoadDataAsync_WhenBackgroundColorIsCustomized_ShouldUseBackgroundColorOverload()
    {
        Color capturedBackgroundColor = Colors.White;
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
                It.IsAny<Color>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<GapDisplayMode>(),
                It.IsAny<GapSegmentSecondaryPenMode?>()))
            .Callback<IEnumerable<MoodEntry>, GraphMode, DateRangeInfo, bool, bool, bool, bool, Color, Color, int, int, GapDisplayMode, GapSegmentSecondaryPenMode?>((_, _, _, _, _, _, _, _, backgroundColor, _, _, _, _) =>
            {
                capturedBackgroundColor = backgroundColor;
            })
            .ReturnsAsync([1, 2, 3]);

        viewModel.SelectedBackgroundColor = Colors.LightYellow;

        await viewModel.LoadDataAsync();

        Assert.Equal(Colors.LightYellow, capturedBackgroundColor);
    }

    [Fact]
    public void ToggleBackgroundColorPickerCommand_ShouldShowDerivedSuggestions_AndApplyComplementaryToBackgroundOnly()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedLineColor = Colors.Blue;

        viewModel.ToggleBackgroundColorPickerCommand.Execute(null);

        Assert.True(viewModel.IsBackgroundColorSuggestionVisible);
        Assert.Equal(Colors.White, viewModel.SelectedBackgroundColor);

        viewModel.ApplyBackgroundSuggestionCommand.Execute(GraphColorSuggestionMode.Complementary);

        Assert.Equal(Colors.Blue, viewModel.SelectedLineColor);
        Assert.Equal(Colors.Yellow, viewModel.SelectedBackgroundColor);
        Assert.True(viewModel.IsBackgroundColorCustomized);
    }

    [Fact]
    public void ToggleBackgroundColorPickerCommand_WhenOpened_ShouldRaiseBackgroundSuggestionVisibilityAsTrue()
    {
        var viewModel = CreateViewModel();
        List<bool> raisedVisibilityValues = [];

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GraphViewModel.IsBackgroundColorSuggestionVisible))
            {
                raisedVisibilityValues.Add(viewModel.IsBackgroundColorSuggestionVisible);
            }
        };

        viewModel.ToggleBackgroundColorPickerCommand.Execute(null);

        Assert.Equal([true], raisedVisibilityValues);
    }

    private GraphViewModel CreateViewModel()
    {
        return new GraphViewModel(
            _mockMoodDataService.Object,
            _mockLineGraphService.Object,
            new FakeDateShim(new DateOnly(2026, 7, 30)));
    }
}