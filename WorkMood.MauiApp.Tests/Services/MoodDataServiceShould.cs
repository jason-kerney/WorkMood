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