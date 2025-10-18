# MorningReminderCommand - Individual Test Plan

## Class Overview
**File**: `MauiApp/Services/MorningReminderCommand.cs`  
**Type**: Business Logic Service / Dispatcher Command  
**LOC**: 117 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Timer-based command that handles morning mood reminder alerts based on schedule configuration. Reminds users to record their morning mood if they haven't done so within 10 minutes of the scheduled time. Implements sophisticated business rules for reminder timing and frequency control.

### Dependencies
- `MoodDataService` (concrete class) - for retrieving existing mood entries
- `ScheduleConfigService` (concrete class) - for morning time schedule configuration
- `ILoggingService` (injected) - for debug logging
- `IDispatcherCommand` interface implementation

### Key Responsibilities
1. **Schedule-aware reminders** - ProcessTickAsync() checks current time against configured morning time
2. **Business rule enforcement** - 10-minute window after morning time, skip if already recorded
3. **Frequency control** - Internal call counting with even-number reminder logic
4. **State management** - Track last reminder date and call count across invocations
5. **Data creation** - Generate MorningReminderData with timing information

### Current Architecture Assessment
**Testability Score: 6/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Testing Challenges:**
1. **Concrete dependencies** - Direct dependencies on MoodDataService and ScheduleConfigService
2. **DateTime.Now usage** - Hard-coded current time dependency
3. **DateTime.Today usage** - Hard-coded current date dependency  
4. **Instance state** - _callCount and _lastReminderDate fields create test interdependency
5. **Complex business logic** - Multiple conditional branches with time-sensitive logic

**Good Design Elements:**
1. **Single responsibility** - Clear focus on morning reminder logic
2. **Comprehensive error handling** - Try-catch with proper CommandResult responses
3. **Structured return values** - CommandResult pattern with success/failure/no-action states
4. **Detailed logging** - Good observability for debugging

## Required Refactoring Strategy

### Phase 1: Extract Time Abstractions
Create abstractions to make time-dependent logic testable:

```csharp
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
    DateOnly TodayAsDateOnly { get; }
}

public interface IReminderStateManager
{
    int GetCallCount(DateOnly date);
    void IncrementCallCount(DateOnly date);
    void ResetCallCountForNewDay(DateOnly date);
}
```

### Phase 2: Extract Service Interfaces
Convert concrete dependencies to interfaces (if not already abstracted):

```csharp
// These may already exist - verify and use existing interfaces
public interface IMoodDataService 
{
    Task<MoodEntry?> GetMoodEntryAsync(DateOnly date);
}

public interface IScheduleConfigService
{
    Task<ScheduleConfig> LoadScheduleConfigAsync();
}
```

### Phase 3: Refactored Architecture
Transform to dependency-injected design with testable abstractions:

```csharp
public class MorningReminderCommand : IDispatcherCommand
{
    private readonly IMoodDataService _moodDataService;
    private readonly IScheduleConfigService _scheduleConfigService;
    private readonly ILoggingService _loggingService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IReminderStateManager _stateManager;

    public async Task<CommandResult> ProcessTickAsync(DateOnly oldDate, DateOnly newDate, MoodEntry? currentRecord = null)
    {
        var today = _dateTimeProvider.TodayAsDateOnly;
        var now = _dateTimeProvider.Now;
        
        var config = await _scheduleConfigService.LoadScheduleConfigAsync();
        var effectiveMorningTime = config.GetEffectiveMorningTimeToday();
        var morningTime = _dateTimeProvider.Today.Add(effectiveMorningTime);
        
        // ... rest of logic with injected dependencies
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
MorningReminderCommandTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenMoodDataServiceIsNull()
│   ├── Should_ThrowArgumentNullException_WhenScheduleConfigServiceIsNull()
│   ├── Should_ThrowArgumentNullException_WhenLoggingServiceIsNull()
│   ├── Should_ThrowArgumentNullException_WhenDateTimeProviderIsNull()
│   └── Should_ThrowArgumentNullException_WhenStateManagerIsNull()
├── ProcessTickAsync/
│   ├── HappyPath/
│   │   ├── Should_SendReminder_WhenWithin10MinutesOfMorningTimeAndEvenCallCount()
│   │   ├── Should_CreateCorrectReminderData_WhenSendingReminder()
│   │   ├── Should_IncrementCallCount_WhenProcessingTick()
│   │   └── Should_LogDebugMessages_WhenSendingReminder()
│   ├── TimeConditions/
│   │   ├── Should_ReturnNoAction_WhenCurrentTimeIsBeforeMorningTime()
│   │   ├── Should_ReturnNoAction_WhenCurrentTimeIsMoreThan10MinutesAfterMorningTime()
│   │   ├── Should_ProcessCorrectly_WhenExactlyAtMorningTime()
│   │   ├── Should_ProcessCorrectly_WhenExactly10MinutesAfterMorningTime()
│   │   └── Should_HandleMidnightCrossover_WhenMorningTimeIsEarlyNextDay()
│   ├── MoodRecordConditions/
│   │   ├── Should_ReturnNoAction_WhenMorningMoodAlreadyRecorded()
│   │   ├── Should_ProcessNormally_WhenNoMoodRecordExists()
│   │   ├── Should_ProcessNormally_WhenMoodRecordExistsButNoStartOfWork()
│   │   └── Should_UseCurrentRecord_WhenProvidedInParameters()
│   ├── CallCountLogic/
│   │   ├── Should_SendReminder_WhenCallCountIsEven()
│   │   ├── Should_SkipReminder_WhenCallCountIsOdd()
│   │   ├── Should_ResetCallCount_WhenNewDayDetected()
│   │   ├── Should_ContinueCallCount_WhenSameDayAsLastReminder()
│   │   └── Should_HandleFirstCallOfDay_Correctly()
│   ├── ScheduleConfiguration/
│   │   ├── Should_UseEffectiveMorningTime_FromScheduleConfig()
│   │   ├── Should_HandleScheduleOverrides_WhenConfigured()
│   │   ├── Should_HandleDefaultMorningTime_WhenNoOverrides()
│   │   └── Should_HandleTimeZoneChanges_InMorningTimeCalculation()
│   ├── EdgeCases/
│   │   ├── Should_HandleGracefully_WhenScheduleConfigServiceReturnsNull()
│   │   ├── Should_HandleGracefully_WhenMoodDataServiceReturnsNull()
│   │   ├── Should_HandleCorrectly_WhenOldDateEqualsNewDate()
│   │   └── Should_HandleDaylightSavingTimeTransitions()
│   └── ErrorHandling/
│       ├── Should_ReturnFailedResult_WhenScheduleConfigServiceThrows()
│       ├── Should_ReturnFailedResult_WhenMoodDataServiceThrows()
│       ├── Should_LogException_WhenErrorOccurs()
│       └── Should_NotRethrowException_WhenProcessingFails()
```

### Test Implementation Examples

#### Constructor Validation Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenMoodDataServiceIsNull()
{
    // Arrange
    IMoodDataService? nullMoodDataService = null;
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockDateTimeProvider = new Mock<IDateTimeProvider>();
    var mockStateManager = new Mock<IReminderStateManager>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new MorningReminderCommand(nullMoodDataService!, mockScheduleConfigService.Object, 
                                  mockLoggingService.Object, mockDateTimeProvider.Object, 
                                  mockStateManager.Object));
    
    Assert.That(exception.ParamName, Is.EqualTo("moodDataService"));
}
```

#### Time Condition Tests
```csharp
[Test]
public async Task ProcessTickAsync_Should_ReturnNoAction_WhenCurrentTimeIsBeforeMorningTime()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockDateTimeProvider = new Mock<IDateTimeProvider>();
    var mockStateManager = new Mock<IReminderStateManager>();
    
    var today = new DateOnly(2024, 10, 17);
    var morningTime = new TimeSpan(9, 0, 0); // 9:00 AM
    var currentTime = new DateTime(2024, 10, 17, 8, 30, 0); // 8:30 AM
    
    var mockConfig = new Mock<ScheduleConfig>();
    mockConfig.Setup(x => x.GetEffectiveMorningTimeToday()).Returns(morningTime);
    
    mockDateTimeProvider.Setup(x => x.TodayAsDateOnly).Returns(today);
    mockDateTimeProvider.Setup(x => x.Now).Returns(currentTime);
    mockDateTimeProvider.Setup(x => x.Today).Returns(currentTime.Date);
    mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync()).ReturnsAsync(mockConfig.Object);
    
    var command = new MorningReminderCommand(mockMoodDataService.Object, 
                                            mockScheduleConfigService.Object,
                                            mockLoggingService.Object,
                                            mockDateTimeProvider.Object,
                                            mockStateManager.Object);

    // Act
    var result = await command.ProcessTickAsync(today, today);

    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.Message, Is.EqualTo("Current time is before morning time"));
    mockMoodDataService.Verify(x => x.GetMoodEntryAsync(It.IsAny<DateOnly>()), Times.Never);
}
```

#### Call Count Logic Tests
```csharp
[Test]
public async Task ProcessTickAsync_Should_SendReminder_WhenCallCountIsEven()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockDateTimeProvider = new Mock<IDateTimeProvider>();
    var mockStateManager = new Mock<IReminderStateManager>();
    
    var today = new DateOnly(2024, 10, 17);
    var morningTime = new TimeSpan(9, 0, 0); // 9:00 AM
    var currentTime = new DateTime(2024, 10, 17, 9, 5, 0); // 9:05 AM (within 10 minutes)
    
    var mockConfig = new Mock<ScheduleConfig>();
    mockConfig.Setup(x => x.GetEffectiveMorningTimeToday()).Returns(morningTime);
    
    mockDateTimeProvider.Setup(x => x.TodayAsDateOnly).Returns(today);
    mockDateTimeProvider.Setup(x => x.Now).Returns(currentTime);
    mockDateTimeProvider.Setup(x => x.Today).Returns(currentTime.Date);
    mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync()).ReturnsAsync(mockConfig.Object);
    mockMoodDataService.Setup(x => x.GetMoodEntryAsync(today)).ReturnsAsync((MoodEntry?)null);
    
    // Setup state manager to return even call count (2)
    mockStateManager.Setup(x => x.GetCallCount(today)).Returns(1); // Will be incremented to 2
    
    var command = new MorningReminderCommand(mockMoodDataService.Object, 
                                            mockScheduleConfigService.Object,
                                            mockLoggingService.Object,
                                            mockDateTimeProvider.Object,
                                            mockStateManager.Object);

    // Act
    var result = await command.ProcessTickAsync(today, today);

    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.Message, Is.EqualTo("Morning mood reminder sent"));
    Assert.That(result.Data, Is.InstanceOf<MorningReminderData>());
    
    mockStateManager.Verify(x => x.IncrementCallCount(today), Times.Once);
}
```

#### Business Logic Integration Tests
```csharp
[Test]
public async Task ProcessTickAsync_Should_CreateCorrectReminderData_WhenSendingReminder()
{
    // Arrange - setup for successful reminder scenario
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockDateTimeProvider = new Mock<IDateTimeProvider>();
    var mockStateManager = new Mock<IReminderStateManager>();
    
    var today = new DateOnly(2024, 10, 17);
    var morningTime = new TimeSpan(9, 0, 0);
    var currentTime = new DateTime(2024, 10, 17, 9, 7, 30); // 7.5 minutes after
    var expectedCallCount = 4;
    
    var mockConfig = new Mock<ScheduleConfig>();
    mockConfig.Setup(x => x.GetEffectiveMorningTimeToday()).Returns(morningTime);
    
    mockDateTimeProvider.Setup(x => x.TodayAsDateOnly).Returns(today);
    mockDateTimeProvider.Setup(x => x.Now).Returns(currentTime);
    mockDateTimeProvider.Setup(x => x.Today).Returns(currentTime.Date);
    mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync()).ReturnsAsync(mockConfig.Object);
    mockMoodDataService.Setup(x => x.GetMoodEntryAsync(today)).ReturnsAsync((MoodEntry?)null);
    mockStateManager.Setup(x => x.GetCallCount(today)).Returns(expectedCallCount - 1);
    
    var command = new MorningReminderCommand(mockMoodDataService.Object, 
                                            mockScheduleConfigService.Object,
                                            mockLoggingService.Object,
                                            mockDateTimeProvider.Object,
                                            mockStateManager.Object);

    // Act
    var result = await command.ProcessTickAsync(today, today);

    // Assert
    Assert.That(result.Success, Is.True);
    var reminderData = result.Data as MorningReminderData;
    Assert.That(reminderData, Is.Not.Null);
    Assert.That(reminderData.MorningTime, Is.EqualTo(new DateTime(2024, 10, 17, 9, 0, 0)));
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(TimeSpan.FromMinutes(7.5)));
    Assert.That(reminderData.CallCount, Is.EqualTo(expectedCallCount));
}
```

### Test Fixtures Required
- **TestDateTimeProvider** - Configurable time provider for deterministic testing
- **TestReminderStateManager** - In-memory state manager for isolated testing
- **MockScheduleConfig** - Configurable schedule configuration with overrides
- **MoodEntryTestFactory** - Create test mood entries with various states

### Integration Test Scenarios
1. **End-to-end reminder flow** - Full process from timer tick to reminder data creation
2. **Multi-day state management** - Call count reset and continuation across date changes
3. **Schedule override integration** - Effective morning time calculation with overrides
4. **Real-world timing scenarios** - Edge cases around midnight, DST, time zones

## Success Criteria
- [ ] **100% line coverage** for all business logic paths
- [ ] **95% branch coverage** for conditional logic (time checks, call count logic)
- [ ] **State isolation** - No test interdependencies from instance state
- [ ] **Time determinism** - All time-dependent logic testable with fixed inputs
- [ ] **Error resilience** - All exceptions properly handled and logged
- [ ] **Business rule validation** - All reminder timing rules thoroughly tested

## Implementation Priority
**MEDIUM-HIGH PRIORITY** - Core business logic service that drives user engagement through timely reminders. Complex timing logic requires thorough testing to prevent notification failures.

## Dependencies for Testing
- Interface abstractions for existing services (may already exist)
- Time abstraction layer for deterministic testing
- State management abstraction for isolated testing
- Comprehensive mock setup for schedule configuration scenarios

## Refactoring Estimate
**Effort: Medium (2-3 days)**
- Time provider abstraction implementation
- State manager abstraction and implementation
- Interface verification/creation for existing services
- Comprehensive test suite creation with timing scenarios
- Integration testing across different schedule configurations

This refactoring will enable comprehensive testing of sophisticated business logic while maintaining the complex timing and frequency control behavior.