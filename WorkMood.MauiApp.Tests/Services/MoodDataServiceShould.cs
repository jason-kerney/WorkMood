using System.IO;
using System.Text.Json;
using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class MoodDataServiceShould
{
    private readonly Mock<IDataArchiveService> _mockArchiveService;
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<IJsonSerializerShim> _mockJsonSerializerShim;

    public MoodDataServiceShould()
    {
        _mockArchiveService = new Mock<IDataArchiveService>();
        _mockFolderShim = new Mock<IFolderShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockFileShim = new Mock<IFileShim>();
        _mockJsonSerializerShim = new Mock<IJsonSerializerShim>();
        
        // Setup basic folder operations that constructor expects
        _mockFolderShim.Setup(x => x.GetApplicationFolder()).Returns("C:\\TestApp");
        _mockFolderShim.Setup(x => x.CreateDirectory(It.IsAny<string>()));
        _mockFolderShim.Setup(x => x.CombinePaths("C:\\TestApp", "mood_data.json")).Returns("C:\\TestApp\\mood_data.json");
        
        // Setup logging operations to avoid issues with Log method
        _mockDateShim.Setup(x => x.Now()).Returns(new DateTime(2024, 10, 15, 10, 30, 45));
        _mockFolderShim.Setup(x => x.GetDesktopFolder()).Returns("C:\\Users\\Test\\Desktop");
        _mockFolderShim.Setup(x => x.CombinePaths("C:\\Users\\Test\\Desktop", "WorkMood_Debug.log")).Returns("C:\\Users\\Test\\Desktop\\WorkMood_Debug.log");
        _mockFileShim.Setup(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public void CreateInstance_Successfully_WhenAllDependenciesProvided()
    {
        // Act
        var sut = CreateMoodDataService();

        // Assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<MoodDataService>();
        _mockFolderShim.Verify(x => x.GetApplicationFolder(), Times.Once);
        _mockFolderShim.Verify(x => x.CreateDirectory("C:\\TestApp"), Times.Once);
    }

    [Fact]
    public void ThrowArgumentNullException_WhenArchiveServiceIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            null!,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object,
            new Mock<ILoggingService>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("archiveService");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFolderShimIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            _mockArchiveService.Object,
            null!,
            _mockDateShim.Object,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object,
            new Mock<ILoggingService>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("folderShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenDateShimIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            null!,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object,
            new Mock<ILoggingService>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("dateShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFileShimIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            null!,
            _mockJsonSerializerShim.Object,
            new Mock<ILoggingService>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("fileShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenJsonSerializerShimIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            _mockFileShim.Object,
            null!,
            new Mock<ILoggingService>().Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("jsonSerializerShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenLoggingServiceIsNull()
    {
        // Act
        var act = () => new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("loggingService");
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnEmptyCollection_WhenFileDoesNotExist()
    {
        // Arrange
        var sut = CreateMoodDataService();
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(false);

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.Entries.Should().BeEmpty();
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnMoodCollection_WhenFileContainsValidJson()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var testDate = new DateOnly(2024, 10, 15);
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = testDate, StartOfWork = 4, EndOfWork = 3 }
        };
        
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result.Entries.Should().HaveCount(1);
        result.Entries.First().Date.Should().Be(testDate);
        result.Entries.First().StartOfWork.Should().Be(4);
        result.Entries.First().EndOfWork.Should().Be(3);
        
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task SaveMoodDataAsync_SaveDataSuccessfully_WhenValidCollectionProvided()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var testDate = new DateOnly(2024, 10, 15);
        var moodCollection = new MoodCollection(new List<MoodEntry>
        {
            new MoodEntry { Date = testDate, StartOfWork = 4, EndOfWork = 3 }
        });
        
        var archivedCollection = new MoodCollection(moodCollection.Entries); // Same data for this test
        var expectedJson = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockArchiveService.Setup(x => x.ArchiveOldDataAsync(moodCollection, 3))
                          .ReturnsAsync(archivedCollection);
        _mockJsonSerializerShim.Setup(x => x.Serialize(archivedCollection.Entries, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedJson);
        _mockFileShim.Setup(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson))
                     .Returns(Task.CompletedTask);

        // Act
        await sut.SaveMoodDataAsync(moodCollection);

        // Assert
        _mockArchiveService.Verify(x => x.ArchiveOldDataAsync(moodCollection, 3), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Serialize(archivedCollection.Entries, It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson), Times.Once);
    }

    [Fact]
    public async Task SaveMoodDataAsync_ThrowInvalidOperationException_WhenFileWriteFails()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var moodCollection = new MoodCollection(new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 }
        });
        
        var archivedCollection = new MoodCollection(moodCollection.Entries);
        var expectedJson = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockArchiveService.Setup(x => x.ArchiveOldDataAsync(moodCollection, 3))
                          .ReturnsAsync(archivedCollection);
        _mockJsonSerializerShim.Setup(x => x.Serialize(archivedCollection.Entries, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedJson);
        _mockFileShim.Setup(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson))
                     .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        var act = async () => await sut.SaveMoodDataAsync(moodCollection);
        
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("Failed to save mood data");
        exception.WithInnerException<IOException>();
        
        _mockArchiveService.Verify(x => x.ArchiveOldDataAsync(moodCollection, 3), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Serialize(archivedCollection.Entries, It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson), Times.Once);
    }

    [Fact]
    public async Task GetMoodEntryAsync_ReturnMoodEntry_WhenDateExists()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var testDate = new DateOnly(2024, 10, 15);
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = testDate, StartOfWork = 4, EndOfWork = 3 },
            new MoodEntry { Date = new DateOnly(2024, 10, 14), StartOfWork = 3, EndOfWork = 4 }
        };
        
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3},{"date":"2024-10-14","startOfWork":3,"endOfWork":4}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act
        var result = await sut.GetMoodEntryAsync(testDate);

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(testDate);
        result.StartOfWork.Should().Be(4);
        result.EndOfWork.Should().Be(3);
        
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
    }

    [Fact]
    public async Task GetMoodEntryAsync_ReturnNull_WhenDateDoesNotExist()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var searchDate = new DateOnly(2024, 10, 16); // Date not in collection
        var existingEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 },
            new MoodEntry { Date = new DateOnly(2024, 10, 14), StartOfWork = 3, EndOfWork = 4 }
        };
        
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3},{"date":"2024-10-14","startOfWork":3,"endOfWork":4}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(existingEntries);

        // Act
        var result = await sut.GetMoodEntryAsync(searchDate);

        // Assert
        result.Should().BeNull();
        
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
    }

    [Fact]
    public void ClearCache_DoesNotThrow_WhenCalled()
    {
        // Arrange
        var sut = CreateMoodDataService();

        // Act
        var act = () => sut.ClearCache();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GetDataFilePath_ReturnCorrectPath_WhenCalled()
    {
        // Arrange
        var sut = CreateMoodDataService();

        // Act
        var result = sut.GetDataFilePath();

        // Assert
        result.Should().Be("C:\\TestApp\\mood_data.json");
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnEmptyCollection_WhenFileIsEmpty()
    {
        // Arrange
        var sut = CreateMoodDataService();
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync("");

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.Entries.Should().BeEmpty();
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnEmptyCollection_WhenFileIsWhitespace()
    {
        // Arrange
        var sut = CreateMoodDataService();
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync("   \n\t   ");

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.Entries.Should().BeEmpty();
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnEmptyCollection_WhenJsonDeserializationThrowsJsonException()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var invalidJson = """{"invalid": json}""";
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(invalidJson);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(invalidJson, It.IsAny<JsonSerializerOptions>()))
                               .Throws(new JsonException("Invalid JSON format"));

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.Entries.Should().BeEmpty();
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(invalidJson, It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnEmptyCollection_WhenDeserializationThrowsGeneralException()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Throws(new InvalidOperationException("Deserialization failed"));

        // Act
        var result = await sut.LoadMoodDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.Entries.Should().BeEmpty();
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task LoadMoodDataAsync_ReturnCachedCollection_WhenCalledMultipleTimes()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act - call twice
        var result1 = await sut.LoadMoodDataAsync();
        var result2 = await sut.LoadMoodDataAsync();

        // Assert
        result1.Should().BeSameAs(result2); // Should return same cached instance
        result1.Count.Should().Be(1);
        result2.Count.Should().Be(1);
        
        // Should only call file operations once (first time)
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task SaveMoodEntryAsync_SaveSingleEntry_WhenEntryProvided()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var entryToSave = new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 };
        var existingCollection = new MoodCollection();
        var expectedJson = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(false);
        _mockArchiveService.Setup(x => x.ArchiveOldDataAsync(It.IsAny<MoodCollection>(), 3))
                          .ReturnsAsync((MoodCollection collection, int years) => collection);
        _mockJsonSerializerShim.Setup(x => x.Serialize(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedJson);
        _mockFileShim.Setup(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson))
                     .Returns(Task.CompletedTask);

        // Act
        await sut.SaveMoodEntryAsync(entryToSave);

        // Assert
        _mockArchiveService.Verify(x => x.ArchiveOldDataAsync(It.IsAny<MoodCollection>(), 3), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Serialize(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson), Times.Once);
    }

    [Fact]
    public async Task SaveMoodEntryAsync_WithAutoSave_SaveSingleEntryWithDefaults_WhenEntryProvided()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var entryToSave = new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 };
        var existingCollection = new MoodCollection();
        var expectedJson = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(false);
        _mockArchiveService.Setup(x => x.ArchiveOldDataAsync(It.IsAny<MoodCollection>(), 3))
                          .ReturnsAsync((MoodCollection collection, int years) => collection);
        _mockJsonSerializerShim.Setup(x => x.Serialize(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedJson);
        _mockFileShim.Setup(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson))
                     .Returns(Task.CompletedTask);

        // Act
        await sut.SaveMoodEntryAsync(entryToSave, useAutoSaveDefaults: true);

        // Assert
        _mockArchiveService.Verify(x => x.ArchiveOldDataAsync(It.IsAny<MoodCollection>(), 3), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Serialize(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("C:\\TestApp\\mood_data.json", expectedJson), Times.Once);
    }

    [Fact]
    public async Task GetRecentMoodEntriesAsync_ReturnRecentEntries_WhenCollectionHasData()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 },
            new MoodEntry { Date = new DateOnly(2024, 10, 14), StartOfWork = 3, EndOfWork = 4 },
            new MoodEntry { Date = new DateOnly(2024, 10, 13), StartOfWork = 2, EndOfWork = 5 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3},{"date":"2024-10-14","startOfWork":3,"endOfWork":4},{"date":"2024-10-13","startOfWork":2,"endOfWork":5}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act
        var result = await sut.GetRecentMoodEntriesAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Date.Should().Be(new DateOnly(2024, 10, 15)); // Most recent
        result.Last().Date.Should().Be(new DateOnly(2024, 10, 14)); // Second most recent
    }

    [Fact]
    public async Task GetRecentMoodEntriesWithArchiveAsync_ReturnActiveEntries_WhenSufficientActiveData()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 },
            new MoodEntry { Date = new DateOnly(2024, 10, 14), StartOfWork = 3, EndOfWork = 4 },
            new MoodEntry { Date = new DateOnly(2024, 10, 13), StartOfWork = 2, EndOfWork = 5 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3},{"date":"2024-10-14","startOfWork":3,"endOfWork":4},{"date":"2024-10-13","startOfWork":2,"endOfWork":5}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);
        
        // The method will check if we have sufficient entries, and since we have 3 entries and request only 2,
        // it should return without checking archives
        sut.ClearCache(); // Ensure we load fresh data

        // Act
        var result = await sut.GetRecentMoodEntriesWithArchiveAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Date.Should().Be(new DateOnly(2024, 10, 15));
        // Note: IsNearYearTransition may or may not be called depending on implementation logic
    }

    [Fact]
    public async Task GetRecentMoodEntriesWithArchiveAsync_CombineActiveAndArchived_WhenNearYearTransitionAndInsufficientData()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var today = new DateOnly(2024, 10, 15);
        var activeEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 }
        };
        var archivedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 13), StartOfWork = 2, EndOfWork = 5 },
            new MoodEntry { Date = new DateOnly(2024, 10, 12), StartOfWork = 3, EndOfWork = 2 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(activeEntries);
        _mockArchiveService.Setup(x => x.IsNearYearTransition(14)).Returns(true);
        _mockArchiveService.Setup(x => x.GetArchivedEntriesInRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                          .ReturnsAsync(archivedEntries);

        // Act
        var result = await sut.GetRecentMoodEntriesWithArchiveAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.OrderByDescending(e => e.Date).First().Date.Should().Be(new DateOnly(2024, 10, 15));
        result.OrderByDescending(e => e.Date).ElementAt(1).Date.Should().Be(new DateOnly(2024, 10, 13));
        result.OrderByDescending(e => e.Date).Last().Date.Should().Be(new DateOnly(2024, 10, 12));
        
        _mockArchiveService.Verify(x => x.IsNearYearTransition(14), Times.Once);
        _mockArchiveService.Verify(x => x.GetArchivedEntriesInRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentMoodEntriesWithArchiveAsync_ReturnActiveOnly_WhenArchiveServiceThrows()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var today = new DateOnly(2024, 10, 15);
        var activeEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(activeEntries);
        _mockArchiveService.Setup(x => x.IsNearYearTransition(14)).Returns(true);
        _mockArchiveService.Setup(x => x.GetArchivedEntriesInRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                          .ThrowsAsync(new IOException("Archive not accessible"));

        // Act
        var result = await sut.GetRecentMoodEntriesWithArchiveAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Date.Should().Be(new DateOnly(2024, 10, 15));
        
        _mockArchiveService.Verify(x => x.IsNearYearTransition(14), Times.Once);
        _mockArchiveService.Verify(x => x.GetArchivedEntriesInRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task GetMoodStatisticsAsync_ReturnStatistics_WhenCollectionHasData()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var today = new DateOnly(2024, 10, 15);
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 },
            new MoodEntry { Date = new DateOnly(2024, 10, 14), StartOfWork = 3, EndOfWork = 4 },
            new MoodEntry { Date = new DateOnly(2024, 10, 13), StartOfWork = 2, EndOfWork = 5 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3},{"date":"2024-10-14","startOfWork":3,"endOfWork":4},{"date":"2024-10-13","startOfWork":2,"endOfWork":5}]""";
        
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act
        var result = await sut.GetMoodStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalEntries.Should().Be(3);
        result.OverallAverageMood.Should().NotBeNull();
        result.Last7DaysAverageMood.Should().NotBeNull();
        result.Last30DaysAverageMood.Should().NotBeNull();
        result.Trend.Should().NotBeNull();
        
        _mockDateShim.Verify(x => x.GetTodayDate(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetTwoWeekVisualizationAsync_ReturnVisualizationData_WhenCalled()
    {
        // Arrange
        var sut = CreateMoodDataService();
        var expectedEntries = new List<MoodEntry>
        {
            new MoodEntry { Date = new DateOnly(2024, 10, 15), StartOfWork = 4, EndOfWork = 3 }
        };
        var jsonContent = """[{"date":"2024-10-15","startOfWork":4,"endOfWork":3}]""";
        
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("C:\\TestApp\\mood_data.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<List<MoodEntry>>(jsonContent, It.IsAny<JsonSerializerOptions>()))
                               .Returns(expectedEntries);

        // Act
        var result = await sut.GetTwoWeekVisualizationAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<MoodVisualizationData>();
    }

    [Fact]
    public async Task ClearCache_ClearsInternalCache_WhenCalled()
    {
        // Arrange
        var sut = CreateMoodDataService();

        // Act & Assert
        sut.ClearCache(); // Should not throw

        // Verify cache is cleared by setting up mock to track subsequent LoadMoodDataAsync calls
        _mockFileShim.Setup(x => x.Exists("C:\\TestApp\\mood_data.json")).Returns(false);
        
        // If cache was cleared, this should trigger file system operations
        await sut.LoadMoodDataAsync();
        
        _mockFileShim.Verify(x => x.Exists("C:\\TestApp\\mood_data.json"), Times.Once);
    }

    private MoodDataService CreateMoodDataService()
    {
        return new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object,
            new Mock<ILoggingService>().Object);
    }
}

// Tests for DateOnlyJsonConverter
public class DateOnlyJsonConverterShould
{
    [Fact]
    public void Read_ReturnDateOnly_WhenValidDateStringProvided()
    {
        // Arrange
        var converter = new DateOnlyJsonConverter();
        var json = "\"2024-10-15\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to the string token
        
        // Act
        var result = converter.Read(ref reader, typeof(DateOnly), new JsonSerializerOptions());
        
        // Assert
        result.Should().Be(new DateOnly(2024, 10, 15));
    }

    [Fact]
    public void Write_WriteCorrectDateString_WhenDateOnlyProvided()
    {
        // Arrange
        var converter = new DateOnlyJsonConverter();
        var date = new DateOnly(2024, 10, 15);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        
        // Act
        converter.Write(writer, date, new JsonSerializerOptions());
        writer.Flush();
        
        // Assert
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be("\"2024-10-15\"");
    }

    [Fact]
    public void Read_ThrowException_WhenInvalidDateStringProvided()
    {
        // Arrange
        var converter = new DateOnlyJsonConverter();
        var json = "\"invalid-date\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to the string token
        
        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(DateOnly), new JsonSerializerOptions());
            Assert.Fail("Expected FormatException was not thrown");
        }
        catch (FormatException)
        {
            // Expected exception - test passes
        }
    }
}