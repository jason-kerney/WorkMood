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

    #region CreateArchiveFileName Tests

    [Fact]
    public void CreateArchiveFileName_GenerateCorrectFormat_WhenGivenDateRange()
    {
        // Arrange
        var timestamp = new DateTime(2024, 12, 15, 14, 30, 45);
        _mockDateShim.Setup(d => d.Now())
            .Returns(timestamp);

        var oldestDate = new DateOnly(2023, 1, 1);
        var newestDate = new DateOnly(2023, 12, 31);

        // Act
        var result = _sut.CreateArchiveFileName(oldestDate, newestDate);

        // Assert
        result.Should().Be("mood_data_archive_2023-01-01_to_2023-12-31_20241215_143045.json");
    }

    [Fact]
    public void CreateArchiveFileName_HandleSingleDayRange_WhenOldestAndNewestSame()
    {
        // Arrange
        var timestamp = new DateTime(2024, 3, 20, 9, 15, 30);
        _mockDateShim.Setup(d => d.Now())
            .Returns(timestamp);

        var sameDate = new DateOnly(2024, 2, 14);

        // Act
        var result = _sut.CreateArchiveFileName(sameDate, sameDate);

        // Assert
        result.Should().Be("mood_data_archive_2024-02-14_to_2024-02-14_20240320_091530.json");
    }

    [Fact]
    public void CreateArchiveFileName_HandleDifferentYears_WhenRangeSpansMultipleYears()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0);
        _mockDateShim.Setup(d => d.Now())
            .Returns(timestamp);

        var oldestDate = new DateOnly(2022, 6, 15);
        var newestDate = new DateOnly(2024, 8, 25);

        // Act
        var result = _sut.CreateArchiveFileName(oldestDate, newestDate);

        // Assert
        result.Should().Be("mood_data_archive_2022-06-15_to_2024-08-25_20250101_000000.json");
    }

    #endregion

    #region IsNearYearTransition Tests

    [Fact]
    public void IsNearYearTransition_ReturnTrue_WhenNearStartOfYear()
    {
        // Arrange - January 10th (10 days from start of year)
        var currentDate = new DateTime(2024, 1, 10);
        _mockDateShim.Setup(d => d.GetToday())
            .Returns(currentDate);

        // Act
        var result = _sut.IsNearYearTransition(daysFromTransition: 14);

        // Assert
        result.Should().BeTrue("because January 10th is within 14 days of year start");
    }

    [Fact]
    public void IsNearYearTransition_ReturnTrue_WhenNearEndOfYear()
    {
        // Arrange - December 25th (6 days from end of year)
        var currentDate = new DateTime(2024, 12, 25);
        _mockDateShim.Setup(d => d.GetToday())
            .Returns(currentDate);

        // Act
        var result = _sut.IsNearYearTransition(daysFromTransition: 14);

        // Assert
        result.Should().BeTrue("because December 25th is within 14 days of year end");
    }

    [Fact]
    public void IsNearYearTransition_ReturnFalse_WhenInMiddleOfYear()
    {
        // Arrange - July 15th (far from both transitions)
        var currentDate = new DateTime(2024, 7, 15);
        _mockDateShim.Setup(d => d.GetToday())
            .Returns(currentDate);

        // Act
        var result = _sut.IsNearYearTransition(daysFromTransition: 14);

        // Assert
        result.Should().BeFalse("because July 15th is far from both year start and end");
    }

    [Fact]
    public void IsNearYearTransition_ReturnFalse_WhenJustOutsideThreshold()
    {
        // Arrange - January 20th (19 days from start, outside 14-day threshold)
        var currentDate = new DateTime(2024, 1, 20);
        _mockDateShim.Setup(d => d.GetToday())
            .Returns(currentDate);

        // Act
        var result = _sut.IsNearYearTransition(daysFromTransition: 14);

        // Assert
        result.Should().BeFalse("because January 20th is 19 days from start, outside 14-day threshold");
    }

    [Fact]
    public void IsNearYearTransition_RespectCustomThreshold_WhenProvidedDifferentValue()
    {
        // Arrange - January 25th (24 days from start)
        var currentDate = new DateTime(2024, 1, 25);
        _mockDateShim.Setup(d => d.GetToday())
            .Returns(currentDate);

        // Act
        var result = _sut.IsNearYearTransition(daysFromTransition: 30);

        // Assert
        result.Should().BeTrue("because January 25th is within 30-day threshold");
    }

    #endregion
}