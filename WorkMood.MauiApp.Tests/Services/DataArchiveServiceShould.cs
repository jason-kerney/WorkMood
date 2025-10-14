using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class DataArchiveServiceShould
{
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<IJsonSerializerShim> _mockJsonSerializerShim;
    private readonly DataArchiveService _sut;

    public DataArchiveServiceShould()
    {
        _mockFolderShim = new Mock<IFolderShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockFileShim = new Mock<IFileShim>();
        _mockJsonSerializerShim = new Mock<IJsonSerializerShim>();

        // Setup default behavior for constructor
        _mockFolderShim.Setup(x => x.GetArchiveFolder()).Returns(@"C:\Archive");
        _mockFolderShim.Setup(x => x.CreateDirectory(It.IsAny<string>()));

        _sut = new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ThrowArgumentNullException_WhenFolderShimIsNull()
    {
        // Act & Assert
        // This is what we WANT the constructor to do - proper parameter validation
        var act = () => new DataArchiveService(
            null!, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("folderShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenDateShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            null!, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("dateShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFileShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            null!, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("fileShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenJsonSerializerShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            null!);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("jsonSerializerShim");
    }

    [Fact]
    public void SuccessfullyCreateService_WhenAllParametersAreValid()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().NotThrow();
    }

    #endregion

    #region GetOldestEntryAge Tests

    [Fact]
    public void GetOldestEntryAge_ReturnNull_WhenCollectionIsEmpty()
    {
        // Arrange
        var emptyCollection = new MoodCollection(new List<MoodEntry>());

        // Act
        var result = _sut.GetOldestEntryAge(emptyCollection);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetOldestEntryAge_CalculateCorrectAge_WhenCollectionHasEntries()
    {
        // Arrange
        var today = new DateOnly(2025, 10, 14);
        var threeYearsAgo = new DateOnly(2022, 10, 14);
        var oneYearAgo = new DateOnly(2024, 10, 14);
        
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);

        var entries = new List<MoodEntry>
        {
            new MoodEntry { Date = oneYearAgo, StartOfWork = 5 },
            new MoodEntry { Date = threeYearsAgo, StartOfWork = 7 }, // This is oldest
            new MoodEntry { Date = today, StartOfWork = 6 }
        };
        var collection = new MoodCollection(entries);

        // Act
        var result = _sut.GetOldestEntryAge(collection);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeApproximately(3.0, 0.1, "because the oldest entry is approximately 3 years old");
    }

    #endregion

    #region ShouldArchive Tests

    [Fact]
    public void ShouldArchive_ReturnFalse_WhenCollectionIsEmpty()
    {
        // Arrange
        var emptyCollection = new MoodCollection(new List<MoodEntry>());

        // Act
        var result = _sut.ShouldArchive(emptyCollection, thresholdYears: 3);

        // Assert
        result.Should().BeFalse("because empty collections should not trigger archiving");
    }

    [Fact]
    public void ShouldArchive_ReturnTrue_WhenOldestEntryExceedsThreshold()
    {
        // Arrange
        var currentDate = new DateOnly(2024, 12, 1);
        _mockDateShim.Setup(d => d.GetTodayDate())
            .Returns(currentDate);

        var entries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2020, 1, 1), StartOfWork = 8 }, // ~5 years old
            new() { Date = new DateOnly(2023, 6, 1), EndOfWork = 7 }    // ~1.5 years old
        };
        var collection = new MoodCollection(entries);

        // Act
        var result = _sut.ShouldArchive(collection, thresholdYears: 3);

        // Assert
        result.Should().BeTrue("because oldest entry (5 years) exceeds threshold (3 years)");
    }

    [Fact]
    public void ShouldArchive_ReturnFalse_WhenOldestEntryWithinThreshold()
    {
        // Arrange
        var currentDate = new DateOnly(2024, 12, 1);
        _mockDateShim.Setup(d => d.GetTodayDate())
            .Returns(currentDate);

        var entries = new List<MoodEntry>
        {
            new() { Date = new DateOnly(2022, 6, 1), StartOfWork = 7 }, // ~2.5 years old
            new() { Date = new DateOnly(2024, 1, 1), EndOfWork = 8 }    // ~11 months old
        };
        var collection = new MoodCollection(entries);

        // Act
        var result = _sut.ShouldArchive(collection, thresholdYears: 3);

        // Assert
        result.Should().BeFalse("because oldest entry (2.5 years) is within threshold (3 years)");
    }

    #endregion
}