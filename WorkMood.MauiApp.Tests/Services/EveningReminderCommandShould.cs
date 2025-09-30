using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class EveningReminderCommandShould
{
    private readonly Mock<IMoodDataService> _mockMoodDataService;
    private readonly Mock<IScheduleConfigService> _mockScheduleConfigService;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly EveningReminderCommand _sut;
    private readonly DateOnly _testDate = new(2025, 9, 30);
    private readonly DateOnly _previousDate = new(2025, 9, 29);
    private readonly ScheduleConfig _defaultScheduleConfig;

    public EveningReminderCommandShould()
    {
        _mockMoodDataService = new Mock<IMoodDataService>();
        _mockScheduleConfigService = new Mock<IScheduleConfigService>();
        _mockDateShim = new Mock<IDateShim>();
        _mockFolderShim = new Mock<IFolderShim>();

        // Create default schedule config with evening time at 17:00 (5:00 PM)
        _defaultScheduleConfig = new ScheduleConfig(
            morningTime: new TimeSpan(9, 0, 0),  // 9:00 AM
            eveningTime: new TimeSpan(17, 0, 0)  // 5:00 PM
        );

        // Setup default behavior
        _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync())
                                  .ReturnsAsync(_defaultScheduleConfig);
        
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(_testDate);
        _mockDateShim.Setup(x => x.GetToday()).Returns(new DateTime(2025, 9, 30));
        _mockFolderShim.Setup(x => x.GetDesktopFolder()).Returns(@"C:\Users\Test\Desktop");

        _sut = new EveningReminderCommand(_mockMoodDataService.Object, _mockScheduleConfigService.Object, _mockDateShim.Object, _mockFolderShim.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ThrowArgumentNullException_WhenMoodDataServiceIsNull()
    {
        // Act & Assert
        var act = () => new EveningReminderCommand(null!, _mockScheduleConfigService.Object, _mockDateShim.Object, _mockFolderShim.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("moodDataService");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenScheduleConfigServiceIsNull()
    {
        // Act & Assert
        var act = () => new EveningReminderCommand(_mockMoodDataService.Object, null!, _mockDateShim.Object, _mockFolderShim.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("scheduleConfigService");
    }

    [Fact]
    public void NotThrowException_WhenDateShimIsNull()
    {
        // Act & Assert - The constructor doesn't validate dateShim
        var act = () => new EveningReminderCommand(_mockMoodDataService.Object, _mockScheduleConfigService.Object, null!, _mockFolderShim.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public void NotThrowException_WhenFolderShimIsNull()
    {
        // Act & Assert - The constructor doesn't validate folder
        var act = () => new EveningReminderCommand(_mockMoodDataService.Object, _mockScheduleConfigService.Object, _mockDateShim.Object, null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateSuccessfully_WhenAllDependenciesProvided()
    {
        // Act & Assert
        var command = new EveningReminderCommand(_mockMoodDataService.Object, _mockScheduleConfigService.Object, _mockDateShim.Object, _mockFolderShim.Object);
        command.Should().NotBeNull();
    }

    #endregion

    #region ProcessTickAsync - Timing Logic Tests

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenCurrentTimeIsBeforeEveningTime()
    {
        // Arrange
        var eveningTime = new TimeSpan(17, 0, 0); // 5:00 PM
        var currentTime = new DateTime(2025, 9, 30, 16, 30, 0); // 4:30 PM - before evening time
        
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Current time is before evening time");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenCurrentTimeIsMoreThan10MinutesAfterEveningTime()
    {
        // Arrange
        var eveningTime = new TimeSpan(17, 0, 0); // 5:00 PM
        var currentTime = new DateTime(2025, 9, 30, 17, 15, 0); // 5:15 PM - more than 10 minutes after
        
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Current time is more than 10 minutes past evening time");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ProcessTickAsync_ProcessReminder_WhenCurrentTimeIsWithin10MinutesAfterEveningTime()
    {
        // Arrange
        var eveningTime = new TimeSpan(17, 0, 0); // 5:00 PM
        var currentTime = new DateTime(2025, 9, 30, 17, 5, 0); // 5:05 PM - within 10 minutes
        
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);
        
        // Setup mood entry with no moods recorded (will trigger both reminders)
        var todayRecord = new MoodEntry(_testDate);
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert - First call should be skipped (odd call count)
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Skipping reminder - call count: 1");
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnNoAction_WhenCurrentTimeIsExactlyAtEveningTime()
    {
        // Arrange
        var eveningTime = new TimeSpan(17, 0, 0); // 5:00 PM
        var currentTime = new DateTime(2025, 9, 30, 17, 0, 0); // Exactly 5:00 PM
        
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert - Exactly at evening time is still considered "before" due to <= comparison
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Current time is before evening time");
        result.Data.Should().BeNull();
    }

    #endregion

    #region ProcessTickAsync - Call Count and Alternating Logic Tests

    [Fact]
    public async Task ProcessTickAsync_SkipReminderOnOddCallCounts()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Act - First call (call count = 1, odd)
        var result1 = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result1.Success.Should().BeTrue();
        result1.Message.Should().Be("Skipping reminder - call count: 1");
    }

    [Fact]
    public async Task ProcessTickAsync_SendReminderOnEvenCallCounts()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Act - First call (odd, should skip)
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        
        // Second call (even, should send reminder)
        var result2 = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result2.Success.Should().BeTrue();
        result2.Message.Should().Be("Time to record your evening mood! You also missed recording your morning mood today.");
        result2.Data.Should().BeOfType<EveningReminderData>();
        
        var reminderData = (EveningReminderData)result2.Data!;
        reminderData.CallCount.Should().Be(2);
        reminderData.ReminderType.Should().Be(EveningReminderType.EveningAndMissedMorning);
        reminderData.MorningMissed.Should().BeTrue();
        reminderData.EveningNeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessTickAsync_ResetCallCountForNewDay()
    {
        // Arrange - First day processing
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Process two calls on first day
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Arrange - Next day
        var nextDay = new DateOnly(2025, 10, 1);
        var nextDayCurrentTime = new DateTime(2025, 10, 1, 17, 5, 0); // 5:05 PM - within evening window
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(nextDay);
        _mockDateShim.Setup(x => x.GetToday()).Returns(new DateTime(2025, 10, 1));
        _mockDateShim.Setup(x => x.Now()).Returns(nextDayCurrentTime);
        
        var nextDayRecord = new MoodEntry(nextDay);
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(nextDay))
                           .ReturnsAsync(nextDayRecord);

        // Act - First call on new day should be odd (call count reset to 1)
        var result = await _sut.ProcessTickAsync(_testDate, nextDay);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Skipping reminder - call count: 1");
    }

    [Fact]
    public async Task ProcessTickAsync_ContinueCallCountSequenceOnSameDay()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Act - Process 4 calls on same day
        var result1 = await _sut.ProcessTickAsync(_previousDate, _testDate); // Call count 1 (odd, skip)
        var result2 = await _sut.ProcessTickAsync(_previousDate, _testDate); // Call count 2 (even, send)
        var result3 = await _sut.ProcessTickAsync(_previousDate, _testDate); // Call count 3 (odd, skip)
        var result4 = await _sut.ProcessTickAsync(_previousDate, _testDate); // Call count 4 (even, send)

        // Assert
        result1.Message.Should().Contain("call count: 1");
        result2.Message.Should().NotContain("Skipping");
        result3.Message.Should().Contain("call count: 3");
        result4.Message.Should().NotContain("Skipping");

        var reminderData4 = (EveningReminderData)result4.Data!;
        reminderData4.CallCount.Should().Be(4);
    }

    #endregion

    #region ProcessTickAsync - Reminder Type Determination Tests

    [Fact]
    public async Task ProcessTickAsync_ReturnNoReminderType_WhenBothMoodsRecorded()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var todayRecord = new MoodEntry(_testDate)
        {
            StartOfWork = 7,
            EndOfWork = 8
        };
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("All moods have been recorded for today");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnEveningOnlyType_WhenOnlyMorningMoodRecorded()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var todayRecord = new MoodEntry(_testDate)
        {
            StartOfWork = 7,
            EndOfWork = null
        };
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);

        // Act - Need to call twice to get even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Time to record your evening mood!");
        
        var reminderData = (EveningReminderData)result.Data!;
        reminderData.ReminderType.Should().Be(EveningReminderType.EveningOnly);
        reminderData.MorningMissed.Should().BeFalse();
        reminderData.EveningNeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnMissedMorningType_WhenOnlyEveningMoodRecorded()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var todayRecord = new MoodEntry(_testDate)
        {
            StartOfWork = null,
            EndOfWork = 8
        };
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);

        // Act - Need to call twice to get even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Don't forget to record your missed morning mood!");
        
        var reminderData = (EveningReminderData)result.Data!;
        reminderData.ReminderType.Should().Be(EveningReminderType.OnlyMissedMorning);
        reminderData.MorningMissed.Should().BeTrue();
        reminderData.EveningNeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnEveningAndMissedMorningType_WhenNoMoodsRecorded()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Act - Need to call twice to get even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Time to record your evening mood! You also missed recording your morning mood today.");
        
        var reminderData = (EveningReminderData)result.Data!;
        reminderData.ReminderType.Should().Be(EveningReminderType.EveningAndMissedMorning);
        reminderData.MorningMissed.Should().BeTrue();
        reminderData.EveningNeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessTickAsync_HandleNullMoodEntry_AsNoMoodsRecorded()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync((MoodEntry?)null);

        // Act - Need to call twice to get even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Time to record your evening mood! You also missed recording your morning mood today.");
        
        var reminderData = (EveningReminderData)result.Data!;
        reminderData.ReminderType.Should().Be(EveningReminderType.EveningAndMissedMorning);
    }

    #endregion

    #region ProcessTickAsync - Current Record Parameter Tests

    [Fact]
    public async Task ProcessTickAsync_UseProvidedCurrentRecord_WhenProvided()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var providedRecord = new MoodEntry(_testDate) { StartOfWork = 5 };

        // Don't setup the mood data service mock since we're providing the record directly

        // Act - Need to call twice to get even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate, providedRecord);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate, providedRecord);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Time to record your evening mood!");
        
        // Verify that GetMoodEntryAsync was not called since we provided the record
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTickAsync_FetchMoodEntry_WhenCurrentRecordNotProvided()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();

        // Act
        await _sut.ProcessTickAsync(_previousDate, _testDate, currentRecord: null);

        // Assert - Verify that GetMoodEntryAsync was called
        _mockMoodDataService.Verify(x => x.GetMoodEntryAsync(_testDate), Times.Once);
    }

    #endregion

    #region ProcessTickAsync - Schedule Configuration Tests

    [Fact]
    public async Task ProcessTickAsync_UseScheduleConfigurationEveningTime()
    {
        // Arrange
        var customConfig = new ScheduleConfig(
            morningTime: new TimeSpan(8, 0, 0),
            eveningTime: new TimeSpan(18, 30, 0)  // 6:30 PM
        );
        _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync())
                                  .ReturnsAsync(customConfig);

        var currentTime = new DateTime(2025, 9, 30, 18, 35, 0); // 6:35 PM - 5 minutes after custom evening time
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);
        
        SetupMoodEntryWithNoMoods();

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert - Should be within 10-minute window and process
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("call count: 1");  // First call, should skip
        
        // Verify the schedule config was loaded
        _mockScheduleConfigService.Verify(x => x.LoadScheduleConfigAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessTickAsync_HandleScheduleConfigWithOverrides()
    {
        // Arrange - Create config with an override for today
        var configWithOverride = new ScheduleConfig(
            morningTime: new TimeSpan(9, 0, 0),
            eveningTime: new TimeSpan(17, 0, 0)
        );
        configWithOverride.SetOverride(_testDate, new TimeSpan(8, 30, 0), new TimeSpan(16, 30, 0));
        
        _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync())
                                  .ReturnsAsync(configWithOverride);

        var currentTime = new DateTime(2025, 9, 30, 16, 35, 0); // 4:35 PM - 5 minutes after override evening time
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);
        
        SetupMoodEntryWithNoMoods();

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert - Should be within 10-minute window of override evening time
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("call count: 1");
    }

    #endregion

    #region ProcessTickAsync - Error Handling Tests

    [Fact]
    public async Task ProcessTickAsync_ReturnFailedResult_WhenScheduleConfigServiceThrows()
    {
        // Arrange
        var exception = new InvalidOperationException("Config service error");
        _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync())
                                  .ThrowsAsync(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error processing evening reminder: Config service error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnFailedResult_WhenMoodDataServiceThrows()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var exception = new InvalidOperationException("Mood data service error");
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ThrowsAsync(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error processing evening reminder: Mood data service error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ProcessTickAsync_ReturnFailedResult_WhenDateShimThrows()
    {
        // Arrange
        var exception = new InvalidOperationException("Date service error");
        _mockDateShim.Setup(x => x.GetTodayDate())
                     .Throws(exception);

        // Act
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error processing evening reminder: Date service error");
        result.Data.Should().BeNull();
    }

    #endregion

    #region EveningReminderData Tests

    [Fact]
    public async Task ProcessTickAsync_PopulateReminderDataCorrectly_WhenSendingReminder()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        var todayRecord = new MoodEntry(_testDate) { StartOfWork = 6 }; // Only morning recorded
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);

        var currentTime = new DateTime(2025, 9, 30, 17, 3, 0); // 5:03 PM
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);

        // Act - Need even call count
        await _sut.ProcessTickAsync(_previousDate, _testDate);
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        result.Data.Should().BeOfType<EveningReminderData>();
        var data = (EveningReminderData)result.Data!;
        
        data.EveningTime.Should().Be(new DateTime(2025, 9, 30, 17, 0, 0)); // 5:00 PM
        data.TimeSinceEvening.Should().Be(TimeSpan.FromMinutes(3));
        data.CallCount.Should().Be(2);
        data.ReminderType.Should().Be(EveningReminderType.EveningOnly);
        data.MorningMissed.Should().BeFalse();
        data.EveningNeeded.Should().BeTrue();
    }

    #endregion

    #region File Logging Tests

    [Fact]
    public async Task ProcessTickAsync_WriteToLogFile_WhenLoggingEnabled()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();
        
        var desktopPath = @"C:\Users\Test\Desktop";
        _mockFolderShim.Setup(x => x.GetDesktopFolder()).Returns(desktopPath);

        // We can't easily test the actual file writing without a file system abstraction,
        // but we can at least verify the folder shim is called
        // Act
        await _sut.ProcessTickAsync(_previousDate, _testDate);

        // Assert
        _mockFolderShim.Verify(x => x.GetDesktopFolder(), Times.AtLeast(1));
    }

    [Fact]
    public async Task ProcessTickAsync_ContinueExecution_WhenLoggingFails()
    {
        // Arrange
        SetupWithinEveningTimeWindow();
        SetupMoodEntryWithNoMoods();
        
        // Make folder shim throw an exception (simulating logging failure)
        _mockFolderShim.Setup(x => x.GetDesktopFolder())
                       .Throws(new UnauthorizedAccessException("Cannot access desktop"));

        // Act & Assert - Should not throw, logging errors are ignored
        var result = await _sut.ProcessTickAsync(_previousDate, _testDate);
        
        result.Success.Should().BeTrue(); // Should continue execution despite logging error
        result.Message.Should().Contain("call count: 1");
    }

    #endregion

    #region Helper Methods

    private void SetupWithinEveningTimeWindow()
    {
        var currentTime = new DateTime(2025, 9, 30, 17, 5, 0); // 5:05 PM - within 10 minutes after 5:00 PM
        _mockDateShim.Setup(x => x.Now()).Returns(currentTime);
    }

    private void SetupMoodEntryWithNoMoods()
    {
        var todayRecord = new MoodEntry(_testDate);
        _mockMoodDataService.Setup(x => x.GetMoodEntryAsync(_testDate))
                           .ReturnsAsync(todayRecord);
    }

    #endregion
}