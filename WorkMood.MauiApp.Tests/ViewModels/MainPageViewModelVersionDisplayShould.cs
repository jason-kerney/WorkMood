using Moq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.ViewModels;
using WhatsYourVersion;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

/// <summary>
/// Tests for MainPageViewModel version display feature (TrackerSubtitle property)
/// Verifies app version is retrieved from whats-your-version library and displayed on main page header
/// Location: MauiApp/ViewModels/MainPageViewModel.cs (lines 20, 23, 33, 45, 62-65, 165-176)
/// </summary>
public class MainPageViewModelVersionDisplayShould
{
    private static MoodDispatcherService CreateDispatcherService(ILoggingService loggingService)
    {
        var mockCommand = new Mock<IDispatcherCommand>();
        return new MoodDispatcherService(new ScheduleConfigService(loggingService), loggingService, mockCommand.Object);
    }

    [Fact]
    public void InitializeTrackerSubtitleWithVersionOnConstruction()
    {
        // Arrange
        var mockWindowActivationService = new Mock<IWindowActivationService>();
        var mockLoggingService = new Mock<ILoggingService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockVersionRetriever = new Mock<IVersionRetriever>();
        
        var versionInfo = new VersionInfo { Version = "0.5.3", BuildDateUtc = "09 Apr 2026 14:30:45 UTC" };
        mockVersionRetriever.Setup(v => v.GetVersion()).Returns(versionInfo);

        // Act
        var viewModel = new MainPageViewModel(
            new MoodDataService(),
            CreateDispatcherService(mockLoggingService.Object),
            new ScheduleConfigService(mockLoggingService.Object),
            mockWindowActivationService.Object,
            mockLoggingService.Object,
            mockServiceProvider.Object,
            mockVersionRetriever.Object);

        // Assert
        Assert.Equal("Daily Mood Tracker v0.5.3", viewModel.TrackerSubtitle);
        mockVersionRetriever.Verify(v => v.GetVersion(), Times.Once);
    }

    [Fact]
    public void UseFallbackVersionWhenRetrieverThrows()
    {
        // Arrange
        var mockWindowActivationService = new Mock<IWindowActivationService>();
        var mockLoggingService = new Mock<ILoggingService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockVersionRetriever = new Mock<IVersionRetriever>();
        
        mockVersionRetriever.Setup(v => v.GetVersion()).Throws(new Exception("Version retrieval failed"));

        // Act
        var viewModel = new MainPageViewModel(
            new MoodDataService(),
            CreateDispatcherService(mockLoggingService.Object),
            new ScheduleConfigService(mockLoggingService.Object),
            mockWindowActivationService.Object,
            mockLoggingService.Object,
            mockServiceProvider.Object,
            mockVersionRetriever.Object);

        // Assert
        Assert.Equal("Daily Mood Tracker v0.1.0", viewModel.TrackerSubtitle);
        mockLoggingService.Verify(l => l.LogException(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ThrowArgumentNullExceptionWhenVersionRetrieverIsNull()
    {
        // Arrange
        var mockWindowActivationService = new Mock<IWindowActivationService>();
        var mockLoggingService = new Mock<ILoggingService>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new MainPageViewModel(
            new MoodDataService(),
            CreateDispatcherService(mockLoggingService.Object),
            new ScheduleConfigService(mockLoggingService.Object),
            mockWindowActivationService.Object,
            mockLoggingService.Object,
            mockServiceProvider.Object,
            null!));

        Assert.Equal("versionRetriever", exception.ParamName);
    }

    [Theory]
    [InlineData("0.5.0")]
    [InlineData("1.0.0")]
    [InlineData("2.3.4")]
    public void DisplayVersionNumberCorrectly(string version)
    {
        // Arrange
        var mockWindowActivationService = new Mock<IWindowActivationService>();
        var mockLoggingService = new Mock<ILoggingService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockVersionRetriever = new Mock<IVersionRetriever>();
        
        var versionInfo = new VersionInfo { Version = version, BuildDateUtc = null };
        mockVersionRetriever.Setup(v => v.GetVersion()).Returns(versionInfo);

        // Act
        var viewModel = new MainPageViewModel(
            new MoodDataService(),
            CreateDispatcherService(mockLoggingService.Object),
            new ScheduleConfigService(mockLoggingService.Object),
            mockWindowActivationService.Object,
            mockLoggingService.Object,
            mockServiceProvider.Object,
            mockVersionRetriever.Object);

        // Assert
        Assert.Equal($"Daily Mood Tracker v{version}", viewModel.TrackerSubtitle);
    }
}
