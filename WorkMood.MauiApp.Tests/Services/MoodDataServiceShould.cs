using FluentAssertions;
using Moq;
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