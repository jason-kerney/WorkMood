using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.ViewModels;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

public class StorageSettingsViewModelShould
{
    private const string CurrentPath = "C:\\WorkMood\\Current";
    private const string NewPath = "C:\\WorkMood\\Archive";

    private readonly Mock<IMoodDataService> _moodDataService;
    private readonly Mock<INavigationService> _navigationService;
    private readonly Mock<IFolderPickerShim> _folderPickerShim;
    private readonly Mock<IPathValidationShim> _pathValidationShim;

    public StorageSettingsViewModelShould()
    {
        _moodDataService = new Mock<IMoodDataService>();
        _navigationService = new Mock<INavigationService>();
        _folderPickerShim = new Mock<IFolderPickerShim>();
        _pathValidationShim = new Mock<IPathValidationShim>();

        _moodDataService.Setup(x => x.GetMoodDataDirectory()).Returns(CurrentPath);
        _navigationService.Setup(x => x.GoToRootAsync()).Returns(Task.CompletedTask);
    }

    [Fact]
    public void ThrowArgumentNullException_WhenMoodDataServiceIsNull()
    {
        // Act
        Action act = () => new StorageSettingsViewModel(
            null!,
            _navigationService.Object,
            _folderPickerShim.Object,
            _pathValidationShim.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("moodDataService");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenNavigationServiceIsNull()
    {
        // Act
        Action act = () => new StorageSettingsViewModel(
            _moodDataService.Object,
            null!,
            _folderPickerShim.Object,
            _pathValidationShim.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("navigationService");
    }

    [Fact]
    public void InitializeCurrentPath_FromMoodDataService()
    {
        // Act
        var sut = CreateSut();

        // Assert
        sut.CurrentPath.Should().Be(CurrentPath);
    }

    [Fact]
    public async Task BrowseCommand_WhenUserPicksValidPath_UpdatesSelection()
    {
        // Arrange
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync(NewPath);
        _pathValidationShim.Setup(x => x.IsAbsolutePath(NewPath)).Returns(true);
        _pathValidationShim.Setup(x => x.HasWritePermission(NewPath)).Returns(true);

        var sut = CreateSut();

        // Act
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => sut.SelectedPath == NewPath);

        // Assert
        sut.IsSelectedPathValid.Should().BeTrue();
        sut.ValidationMessage.Should().Be("Path is valid and writable.");
    }

    [Fact]
    public async Task BrowseCommand_WhenUserCancels_LeavesSelectionUnchanged()
    {
        // Arrange
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync((string?)null);

        var sut = CreateSut();

        // Act
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => _folderPickerShim.Invocations.Count == 1);

        // Assert
        sut.SelectedPath.Should().BeEmpty();
        sut.IsSelectedPathValid.Should().BeFalse();
        sut.ValidationMessage.Should().BeEmpty();
        _pathValidationShim.Verify(x => x.IsAbsolutePath(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BrowseCommand_WhenPickedPathMatchesCurrentPath_DisablesMigration()
    {
        // Arrange
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync(CurrentPath);

        var sut = CreateSut();

        // Act
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => sut.ValidationMessage == "Selected path is already the current location.");

        // Assert
        sut.IsSelectedPathValid.Should().BeFalse();
        sut.MigrateCommand.CanExecute(null).Should().BeFalse();
        _moodDataService.Verify(x => x.MigrateMoodDataAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MigrateCommand_WhenMigrationSucceeds_NavigatesToRoot()
    {
        // Arrange
        _moodDataService.SetupSequence(x => x.GetMoodDataDirectory())
            .Returns(CurrentPath)
            .Returns(NewPath);
        _moodDataService.Setup(x => x.MigrateMoodDataAsync(NewPath)).Returns(Task.CompletedTask);
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync(NewPath);
        _pathValidationShim.Setup(x => x.IsAbsolutePath(NewPath)).Returns(true);
        _pathValidationShim.Setup(x => x.HasWritePermission(NewPath)).Returns(true);

        var sut = CreateSut();
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => sut.IsSelectedPathValid);

        // Act
        sut.MigrateCommand.Execute(null);
        await WaitForConditionAsync(() => _navigationService.Invocations.Any(invocation => invocation.Method.Name == nameof(INavigationService.GoToRootAsync)));

        // Assert
        sut.CurrentPath.Should().Be(NewPath);
        sut.SelectedPath.Should().BeEmpty();
        sut.ValidationMessage.Should().Be("Migration complete.");
        _moodDataService.Verify(x => x.MigrateMoodDataAsync(NewPath), Times.Once);
        _navigationService.Verify(x => x.GoToRootAsync(), Times.Once);
    }

    [Fact]
    public async Task MigrateCommand_WhenMigrationFails_DoesNotNavigateToRoot()
    {
        // Arrange
        _moodDataService.Setup(x => x.MigrateMoodDataAsync(NewPath))
            .ThrowsAsync(new InvalidOperationException("config save failed"));
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync(NewPath);
        _pathValidationShim.Setup(x => x.IsAbsolutePath(NewPath)).Returns(true);
        _pathValidationShim.Setup(x => x.HasWritePermission(NewPath)).Returns(true);

        var sut = CreateSut();
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => sut.IsSelectedPathValid);

        // Act
        sut.MigrateCommand.Execute(null);
        await WaitForConditionAsync(() => sut.ValidationMessage.StartsWith("Migration failed:", StringComparison.Ordinal));

        // Assert
        sut.CurrentPath.Should().Be(CurrentPath);
        _navigationService.Verify(x => x.GoToRootAsync(), Times.Never);
    }

    [Fact]
    public async Task MigrateCommand_WhileMigrationIsRunning_TracksBusyState()
    {
        // Arrange
        var migrationCompletion = new TaskCompletionSource<object?>();
        _moodDataService.Setup(x => x.MigrateMoodDataAsync(NewPath)).Returns(migrationCompletion.Task);
        _moodDataService.SetupSequence(x => x.GetMoodDataDirectory())
            .Returns(CurrentPath)
            .Returns(NewPath);
        _folderPickerShim.Setup(x => x.PickFolderAsync()).ReturnsAsync(NewPath);
        _pathValidationShim.Setup(x => x.IsAbsolutePath(NewPath)).Returns(true);
        _pathValidationShim.Setup(x => x.HasWritePermission(NewPath)).Returns(true);

        var sut = CreateSut();
        sut.BrowseCommand.Execute(null);
        await WaitForConditionAsync(() => sut.IsSelectedPathValid);

        // Act
        sut.MigrateCommand.Execute(null);
        await WaitForConditionAsync(() => sut.IsMigrating);
        migrationCompletion.SetResult(null);
        await WaitForConditionAsync(() => !sut.IsMigrating);

        // Assert
        sut.IsMigrating.Should().BeFalse();
        _navigationService.Verify(x => x.GoToRootAsync(), Times.Once);
    }

    private StorageSettingsViewModel CreateSut()
    {
        return new StorageSettingsViewModel(
            _moodDataService.Object,
            _navigationService.Object,
            _folderPickerShim.Object,
            _pathValidationShim.Object);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, int timeoutMilliseconds = 2000)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
        {
            if (condition())
                return;

            await Task.Delay(20);
        }

        throw new TimeoutException("Condition was not met within the allotted time.");
    }
}