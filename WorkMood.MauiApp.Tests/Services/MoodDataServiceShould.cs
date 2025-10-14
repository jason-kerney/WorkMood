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
            _mockJsonSerializerShim.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("archiveService");
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

    private MoodDataService CreateMoodDataService()
    {
        return new MoodDataService(
            _mockArchiveService.Object,
            _mockFolderShim.Object,
            _mockDateShim.Object,
            _mockFileShim.Object,
            _mockJsonSerializerShim.Object);
    }
}