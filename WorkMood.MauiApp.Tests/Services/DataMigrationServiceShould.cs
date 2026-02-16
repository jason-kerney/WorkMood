using Moq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests for DataMigrationService - migrating data from LocalApplicationData to Documents
/// Location: MauiApp/Services/DataMigrationService.cs
/// Purpose: Verify safe migration of mood data to cloud-sync-enabled location with cleanup
/// </summary>
public class DataMigrationServiceShould
{
    private const string OldLocation = "C:\\Users\\Test\\AppData\\Local\\WorkMood";
    private const string NewLocation = "C:\\Users\\Test\\Documents\\WorkMood";

    #region Setup Helper

    private (Mock<IFolderShim>, Mock<IFileShim>, Mock<ILoggingService>, DataMigrationService) CreateServiceWithMocks()
    {
        var folderMock = new Mock<IFolderShim>();
        var fileMock = new Mock<IFileShim>();
        var loggingMock = new Mock<ILoggingService>();

        // Default setup
        folderMock.Setup(f => f.GetApplicationFolder()).Returns(OldLocation);
        folderMock.Setup(f => f.GetDocumentsFolder()).Returns(NewLocation);
        folderMock.Setup(f => f.CombinePaths(It.IsAny<string[]>()))
            .Returns<string[]>(p => Path.Combine(p));

        var service = new DataMigrationService(folderMock.Object, fileMock.Object, loggingMock.Object);

        return (folderMock, fileMock, loggingMock, service);
    }

    #endregion

    #region Migration Already Completed Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationDoesNotExist_ShouldSkipMigration()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(false); // Old location doesn't exist

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        folderMock.Verify(f => f.DeleteDirectory(It.IsAny<string>()), Times.Never,
            "Should not delete anything when old location doesn't exist");
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.Never,
            "Should not read files when old location empty");
    }

    #endregion

    #region No Data to Migrate Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationEmpty_ShouldSkipMigrationWithoutDeletion()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        // Setup: Old location doesn't exist
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(false);

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        folderMock.Verify(f => f.DeleteDirectory(It.IsAny<string>()), Times.Never,
            "Should not delete when old location doesn't exist");
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.Never,
            "Should not read files when old location empty");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationHasNoDataFiles_ShouldSkipMigration()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        // Setup: Old location exists but no data files
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        fileMock.Setup(f => f.Exists(It.IsAny<string>()))
            .Returns(false); // No data files

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.Never,
            "Should not read files when old location has no data");
    }

    #endregion

    #region New Location Has Newer Data Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenNewLocationHasNewerMoodData_ShouldDeleteOldLocation()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        var oldTime = DateTime.UtcNow.AddHours(-1);
        var newTime = DateTime.UtcNow;

        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(true);
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        // Verify old location is deleted
        folderMock.Verify(f => f.DeleteDirectory(OldLocation), Times.Once,
            "Should delete old location when new location has newer data");
    }

    #endregion

    #region Successful Migration Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationHasMoodData_ShouldMigrateMoodDataFile()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        // Setup conditions for migration
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        
        // Old location has mood data
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);

        var testData = "[{\"date\":\"2024-01-15\",\"mood\":5}]";
        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync(testData);

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.AtLeastOnce,
            "Should read mood data file");
        fileMock.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), testData), Times.AtLeastOnce,
            "Should write mood data to new location");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationHasScheduleConfig_ShouldMigrateScheduleConfigFile()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        // Setup conditions for migration
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        
        // Old location has schedule config
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("schedule_config.json"))))
            .Returns(true);

        var testData = "{\"startOfWork\":\"09:00\",\"endOfWork\":\"17:00\"}";
        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync(testData);

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        fileMock.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), testData), Times.AtLeastOnce,
            "Should write schedule config to new location");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenMigrationCompletes_ShouldDeleteOldLocation()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        // Setup conditions for migration
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);
        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync("test data");

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        folderMock.Verify(f => f.DeleteDirectory(OldLocation), Times.Once,
            "Should delete old location after successful migration");
    }

    #endregion

    #region Archive Migration Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenOldLocationHasArchives_ShouldMigrateArchiveFiles()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        var archiveFiles = new[] 
        { 
            Path.Combine(OldLocation, "archives", "archive_2024_01.json"),
            Path.Combine(OldLocation, "archives", "archive_2024_02.json")
        };

        // Setup conditions for migration - must have mood_data to trigger migration
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        
        // Need at least one main data file to exist for migration to proceed
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);
        
        folderMock.Setup(f => f.GetFiles(It.Is<string>(p => p.Contains("archives")), "*.json"))
            .Returns(archiveFiles);
        folderMock.Setup(f => f.GetFileName(It.IsAny<string>()))
            .Returns<string>(p => Path.GetFileName(p));

        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync("archive data");

        // Act
        await service.MigrateIfNeededAsync();

        // Assert
        // Verify we attempted to read and write archive files
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.AtLeastOnce,
            "Should read archive files");
        fileMock.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce,
            "Should write archive files to new location");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task MigrateIfNeededAsync_WhenFileReadFails_ShouldNotThrowException()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);
        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ThrowsAsync(new IOException("Disk error"));

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => service.MigrateIfNeededAsync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenDirectoryCreationFails_ShouldNotThrowException()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(true);
        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        fileMock.Setup(f => f.Exists(It.IsAny<string>()))
            .Returns(true);
        folderMock.Setup(f => f.CreateDirectory(It.IsAny<string>()))
            .Throws(new UnauthorizedAccessException("Access denied"));

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => service.MigrateIfNeededAsync());
        Assert.Null(exception);
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task MigrateIfNeededAsync_CalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var (folderMock, fileMock, _, service) = CreateServiceWithMocks();

        var callCount = 0;
        
        // First call: old location exists and has data
        // Second call: old location doesn't exist (was deleted)
        folderMock.Setup(f => f.DirectoryExists(OldLocation))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1; // Exists first time, not after
            });

        folderMock.Setup(f => f.DirectoryExists(NewLocation))
            .Returns(false);
        fileMock.Setup(f => f.Exists(It.Is<string>(p => p.Contains("mood_data.json"))))
            .Returns(true);
        fileMock.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
            .ReturnsAsync("data");

        // Act
        await service.MigrateIfNeededAsync(); // First call - should migrate
        await service.MigrateIfNeededAsync(); // Second call - should skip (old location gone)

        // Assert
        fileMock.Verify(f => f.ReadAllTextAsync(It.IsAny<string>()), Times.Once,
            "Should only read files once, not on second call");
    }

    #endregion
}
