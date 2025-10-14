using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class AutoSaveCommandShould
{
    private readonly Mock<IMoodDataService> _mockMoodDataService;
    private readonly AutoSaveCommand _sut;
    private readonly DateOnly _oldDate = new(2025, 9, 29);
    private readonly DateOnly _newDate = new(2025, 9, 30);

    public AutoSaveCommandShould()
    {
        _mockMoodDataService = new Mock<IMoodDataService>();
        _sut = new AutoSaveCommand(_mockMoodDataService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ThrowArgumentNullException_WhenMoodDataServiceIsNull()
    {
        // Act & Assert
        var act = () => new AutoSaveCommand(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("moodDataService");
    }

    #endregion

    #region ProcessTickAsync - Success Scenarios

    [Fact]
    public async Task ProcessTickAsync_ReturnSuccessWithSavedRecord_WhenRecordHasStartOfWorkOnly()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 7
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be($"Auto-saved record for {_oldDate:yyyy-MM-dd}");
        result.Data.Should().BeSameAs(record);
        
        // Verify the mood data service was called correctly
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnSuccessWithSavedRecord_WhenCurrentRecordProvided()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 5
        };

        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate, record);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be($"Auto-saved record for {_oldDate:yyyy-MM-dd}");
        result.Data.Should().BeSameAs(record);
        
        // Verify GetMoodEntryAsync was NOT called since record was provided
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ProcessTickAsync_ReturnSuccessWithSavedRecord_WhenRecordHasValidStartOfWorkMood(int startOfWorkMood)
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = startOfWorkMood
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be($"Auto-saved record for {_oldDate:yyyy-MM-dd}");
        result.Data.Should().BeSameAs(record);
        
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    #endregion

    #region ProcessTickAsync - No Action Scenarios

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenNoRecordFound()
    {
        // Arrange
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync((MoodEntry?)null);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("No record found for the previous date");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenCurrentRecordIsNullAndServiceReturnsNull()
    {
        // Arrange - When currentRecord is null, service is called and returns null
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync((MoodEntry?)null);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("No record found for the previous date");
        result.Data.Should().BeNull();
        
        // When currentRecord is null, GetMoodEntryAsync should be called
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenRecordAlreadyComplete()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 6,
            EndOfWork = 8
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Record already complete");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenRecordHasInvalidState()
    {
        // Arrange - Record exists but has no StartOfWork mood
        var record = new MoodEntry(_oldDate);
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Record exists but has no valid mood data");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenRecordOnlyHasEndOfWork()
    {
        // Arrange - Record has EndOfWork but not StartOfWork (invalid state)
        var record = new MoodEntry(_oldDate)
        {
            EndOfWork = 7
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Record exists but has no valid mood data");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region ProcessTickAsync - Error Scenarios

    [Fact]
    public async Task ProcessTickAsync_ReturnFailure_WhenGetMoodEntryAsyncThrowsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Database error");
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ThrowsAsync(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error during auto-save: Database error");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnFailure_WhenSaveMoodEntryAsyncThrowsException()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 7
        };
        var exception = new InvalidOperationException("Save failed");
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .ThrowsAsync(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error during auto-save: Save failed");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnFailure_WhenCurrentRecordCausesSaveException()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 5
        };
        var exception = new ArgumentException("Invalid record");

        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(record, true))
                           .ThrowsAsync(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate, record);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error during auto-save: Invalid record");
        result.Data.Should().BeNull();
        
        // Should not call GetMoodEntryAsync when currentRecord is provided
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    #endregion

    #region Integration Tests for DetermineAutoSaveAction Logic

    [Theory]
    [InlineData(1, null)]
    [InlineData(5, null)]
    [InlineData(10, null)]
    public async Task ProcessTickAsync_TriggerSave_WhenRecordHasStartOfWorkButNoEndOfWork(int startOfWork, int? endOfWork)
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = startOfWork,
            EndOfWork = endOfWork
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be($"Auto-saved record for {_oldDate:yyyy-MM-dd}");
        result.Data.Should().BeSameAs(record);
        
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 7)]
    [InlineData(10, 5)]
    public async Task ProcessTickAsync_SkipSave_WhenRecordAlreadyHasBothMoods(int startOfWork, int endOfWork)
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = startOfWork,
            EndOfWork = endOfWork
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Record already complete");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_SkipSave_WhenRecordExistsButHasNoMoodData()
    {
        // Arrange - Empty record (no StartOfWork or EndOfWork)
        var record = new MoodEntry(_oldDate);
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Record exists but has no valid mood data");
        result.Data.Should().BeNull();
        
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public async Task ProcessTickAsync_HandleSameDateParameters()
    {
        // Arrange - Same date for old and new (edge case)
        var sameDate = new DateOnly(2025, 9, 30);
        var record = new MoodEntry(sameDate)
        {
            StartOfWork = 6
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(sameDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(sameDate, sameDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be($"Auto-saved record for {sameDate:yyyy-MM-dd}");
        result.Data.Should().BeSameAs(record);
    }

    [Fact]
    public async Task ProcessTickAsync_UseCorrectAutoSaveDefaults()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 7
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        
        // Verify that SaveMoodEntryAsync was called with useAutoSaveDefaults = true
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(record, true), Times.Once);
    }

    [Fact]
    public async Task ProcessTickAsync_PreferCurrentRecordOverServiceCall()
    {
        // Arrange
        var currentRecord = new MoodEntry(_oldDate)
        {
            StartOfWork = 8
        };

        // Also setup the service to return a different record to ensure it's not called
        var serviceRecord = new MoodEntry(_oldDate)
        {
            StartOfWork = 5
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(serviceRecord);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessTickAsync(_oldDate, _newDate, currentRecord);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeSameAs(currentRecord);
        result.Data.Should().NotBeSameAs(serviceRecord);
        
        // Verify GetMoodEntryAsync was not called since currentRecord was provided
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(currentRecord, true), Times.Once);
    }

    #endregion

    #region Behavior Verification Tests

    [Fact]
    public async Task ProcessTickAsync_CallGetMoodEntryAsync_OnlyWhenCurrentRecordNotProvided()
    {
        // Test 1: With current record - should not call GetMoodEntryAsync
        var record = new MoodEntry(_oldDate) { StartOfWork = 6 };
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        await _sut.ProcessTickAsync(_oldDate, _newDate, record);
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);

        // Reset mock
        _mockMoodDataService.Reset();
        
        // Test 2: Without current record - should call GetMoodEntryAsync
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(It.IsAny<MoodEntry>(), true))
                           .Returns(Task.CompletedTask);

        await _sut.ProcessTickAsync(_oldDate, _newDate);
        
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_oldDate), Times.Once);
    }

    [Fact]
    public async Task ProcessTickAsync_PassCorrectParametersToSave()
    {
        // Arrange
        var record = new MoodEntry(_oldDate)
        {
            StartOfWork = 9
        };
        
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_oldDate))
                           .ReturnsAsync(record);
        _mockMoodDataService.Setup(x => x.SaveMoodEntryAsync(record, true))
                           .Returns(Task.CompletedTask);

        // Act
        await _sut.ProcessTickAsync(_oldDate, _newDate);

        // Assert - Verify exact parameters
        _mockMoodDataService.Verify(x => x.SaveMoodEntryAsync(
            It.Is<MoodEntry>(e => e == record), 
            It.Is<bool>(b => b == true)), 
            Times.Once);
    }

    #endregion
}
