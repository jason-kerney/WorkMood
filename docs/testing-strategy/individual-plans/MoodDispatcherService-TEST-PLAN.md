# MoodDispatcherService Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Overview

### Object Under Test

**Target**: `MoodDispatcherService`
**File**: `MauiApp/Services/MoodDispatcherService.cs` (366 lines)
**Type**: Core coordination service implementing IDisposable
**Current Coverage**: 0% (Source: CoverageReport/Summary.txt)
**Target Coverage**: 90%+

### Current Implementation Analysis

`MoodDispatcherService` is a critical coordination service that monitors for date changes and automatically manages mood records through a command pattern architecture. It serves as the central timing hub for the application, orchestrating auto-save operations, schedule cleanup, and reminder notifications.

**Key Characteristics**:
- **Timer-Based Coordination**: Uses System.Timers.Timer (30-second intervals)
- **Command Pattern**: Executes array of IDispatcherCommand implementations  
- **Event-Driven Architecture**: Publishes events for UI consumption
- **Date Change Detection**: Monitors for day transitions and triggers actions
- **Resource Management**: Implements IDisposable pattern properly
- **Error Resilience**: Comprehensive try-catch blocks with logging

## Section 1: Class Structure Analysis

### Constructor Dependencies
```csharp
public MoodDispatcherService(
    ScheduleConfigService scheduleConfigService, 
    ILoggingService loggingService, 
    [NotNull] params IDispatcherCommand[] commands)
```

**Dependencies**:
- `ScheduleConfigService` - Concrete dependency for schedule management
- `ILoggingService` - Abstracted logging interface (good for testing)
- `IDispatcherCommand[]` - Variable array of command implementations

### Public Interface
```csharp
// Methods
public void UpdateCurrentRecordState(MoodEntry? currentRecord)
public void Start()
public void Stop()
public void Dispose()

// Events
public event EventHandler<DateChangeEventArgs>? DateChanged;
public event EventHandler<AutoSaveEventArgs>? AutoSaveOccurred;
public event EventHandler<MorningReminderEventArgs>? MorningReminderOccurred;
public event EventHandler<EveningReminderEventArgs>? EveningReminderOccurred;
```

### Private Implementation
```csharp
// Core timer logic
private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
private async Task CheckForDateChange()
private async Task ProcessReminders(DateOnly currentDate)
private async Task HandleDateChange(DateOnly oldDate, DateOnly newDate)

// Helper methods
private AutoSaveDecision MapResultToDecision(CommandResult result)
private async Task CleanupScheduleConfig()
private void Log(string message)
```

### Event Arguments Classes
**Also defined in same file (need separate test coverage)**:
- `DateChangeEventArgs` - Date change notification data
- `AutoSaveEventArgs` - Auto-save occurrence data
- `MorningReminderEventArgs` - Morning reminder data
- `EveningReminderEventArgs` - Evening reminder data
- `AutoSaveDecision` enum - Decision types for auto-save actions

## Section 2: Testability Assessment

### Testability Score: 4/10 ⚠️ **REQUIRES SIGNIFICANT REFACTORING**

**Major Testability Issues**:
- ❌ **Hard Timer Dependency**: Uses System.Timers.Timer directly (not injectable)
- ❌ **Static DateTime Dependencies**: `DateTime.Today`, `DateOnly.FromDateTime(DateTime.Today)`
- ❌ **Concrete Service Dependency**: ScheduleConfigService (not abstracted)
- ❌ **Async Void Event Handler**: `OnTimerElapsed` method difficult to test
- ❌ **Threading Complexity**: Timer events create race conditions in tests

**Moderate Issues**:
- ⚠️ **Complex State Management**: Multiple private fields and state tracking
- ⚠️ **Exception Handling**: Extensive try-catch blocks mask testable failure paths
- ⚠️ **Event Firing Logic**: Complex event raising patterns need verification

**Good Architecture Elements**:
- ✅ **ILoggingService Abstraction**: Proper dependency injection
- ✅ **Command Pattern**: IDispatcherCommand array enables mocking
- ✅ **IDisposable Pattern**: Proper resource cleanup implementation
- ✅ **Event-Driven Design**: Clean separation of concerns via events

## Section 3: Required Refactoring Analysis

### Refactoring Requirements: SIGNIFICANT - Major Architectural Changes Needed ⚠️

**Critical Refactoring Tasks**:

#### 1. Abstract Timer Dependencies
```csharp
// BEFORE (Hard to test)
private readonly System.Timers.Timer _timer = null!;
_timer = new System.Timers.Timer(30000);
_timer.Elapsed += OnTimerElapsed;

// AFTER (Testable via interface)
public interface ITimerService
{
    event EventHandler<TimerElapsedEventArgs>? Elapsed;
    void Start(double intervalMs);
    void Stop();
    void Dispose();
}

private readonly ITimerService _timerService;
```

#### 2. Abstract DateTime Dependencies  
```csharp
// BEFORE (Hard to test)
var currentDate = DateOnly.FromDateTime(DateTime.Today);
_lastCheckedDate = DateOnly.FromDateTime(DateTime.Today);

// AFTER (Testable via shim)
private readonly IDateShim _dateShim;
var currentDate = _dateShim.GetTodayDate();
_lastCheckedDate = _dateShim.GetTodayDate();
```

#### 3. Abstract Schedule Service Dependency
```csharp
// BEFORE (Hard to test)  
private readonly ScheduleConfigService _scheduleConfigService;

// AFTER (Testable via interface)
public interface IScheduleConfigService
{
    Task<ScheduleConfig> LoadScheduleConfigAsync();
    Task UpdateScheduleConfigAsync(DateTime morningTime, DateTime eveningTime, ScheduleOverride? newOverride);
}

private readonly IScheduleConfigService _scheduleConfigService;
```

#### 4. Make Timer Events Testable
```csharp
// BEFORE (Async void - hard to test)
private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)

// AFTER (Testable method)  
private async void OnTimerElapsed(object? sender, TimerElapsedEventArgs e)
{
    await OnTimerElapsedAsync();
}

internal async Task OnTimerElapsedAsync() // Made testable
{
    // Move logic here for testing
}
```

### Required Interface Extractions

#### IScheduleConfigService Interface
```csharp
public interface IScheduleConfigService  
{
    Task<ScheduleConfig> LoadScheduleConfigAsync();
    Task UpdateScheduleConfigAsync(DateTime morningTime, DateTime eveningTime, ScheduleOverride? newOverride);
}
```

#### ITimerService Interface
```csharp
public interface ITimerService : IDisposable
{
    event EventHandler<TimerElapsedEventArgs>? Elapsed;
    bool IsEnabled { get; }
    void Start(double intervalMs);
    void Stop();
}

public class TimerElapsedEventArgs : EventArgs
{
    public DateTime SignalTime { get; set; }
}
```

### Refactoring Priority: **CRITICAL** 
This service cannot be effectively tested without significant architectural changes due to hard dependencies on System.Timers.Timer and DateTime.Today.

## Section 4: Test Strategy (Post-Refactoring)

### Testing Approach

After refactoring to inject dependencies, focus on:

1. **Constructor Testing**: Dependency validation and initialization
2. **Timer Event Testing**: Controlled timer firing and date change detection  
3. **Command Orchestration**: IDispatcherCommand execution and result handling
4. **Event Publishing**: Verify correct events fired with proper data
5. **State Management**: Current record tracking and date change persistence
6. **Error Handling**: Exception resilience and logging behavior
7. **Resource Management**: IDisposable implementation and cleanup

### Test Categories

#### 4.1 Constructor and Initialization Tests
- **Valid Dependencies**: All required services provided
- **Null Dependency Validation**: ArgumentNullException for null services
- **Empty Command Array**: ArgumentOutOfRangeException for zero commands
- **Timer Configuration**: Verify timer setup with correct interval
- **Initial State**: Verify _lastCheckedDate initialization

#### 4.2 Timer Event and Date Change Tests
- **Same Date Tick**: No date change, only reminder processing
- **Date Change Tick**: Date transition triggers full workflow
- **Multiple Date Changes**: Handling rapid date transitions
- **Timer Start/Stop**: Control timer state correctly
- **Exception Resilience**: Timer continues despite errors

#### 4.3 Command Execution Tests  
- **Single Command Success**: One command executes successfully
- **Multiple Commands**: All commands execute in sequence
- **Command Failures**: Individual command failures don't stop others
- **Command Results Processing**: Proper result aggregation and mapping
- **Morning Reminder Commands**: MorningReminderCommand handling
- **Evening Reminder Commands**: EveningReminderCommand handling

#### 4.4 Event Publishing Tests
- **DateChanged Event**: Fired with correct old/new dates
- **AutoSaveOccurred Event**: Fired when auto-save happens
- **MorningReminderOccurred Event**: Fired with reminder data
- **EveningReminderOccurred Event**: Fired with evening data
- **Event Data Validation**: All event args populated correctly

#### 4.5 State Management Tests
- **Current Record Tracking**: UpdateCurrentRecordState method
- **Date Persistence**: _lastCheckedDate properly updated
- **Schedule Cleanup**: CleanupScheduleConfig execution
- **Auto-Save Decision Mapping**: MapResultToDecision logic

#### 4.6 Resource Management Tests
- **Dispose Pattern**: Timer disposal and event cleanup
- **Multiple Dispose**: Safe to call Dispose multiple times
- **Start After Dispose**: Proper behavior after disposal

## Section 5: Test Implementation Strategy (Post-Refactoring)

### Test File Structure
```
WorkMood.MauiApp.Tests/
└── Services/
    ├── MoodDispatcherServiceTests.cs
    ├── DateChangeEventArgsTests.cs
    ├── AutoSaveEventArgsTests.cs
    ├── MorningReminderEventArgsTests.cs
    └── EveningReminderEventArgsTests.cs
```

### Test Class Organization
```csharp
[TestClass]
public class MoodDispatcherServiceTests
{
    private Mock<IScheduleConfigService> _mockScheduleService = null!;
    private Mock<ILoggingService> _mockLoggingService = null!;
    private Mock<ITimerService> _mockTimerService = null!;
    private Mock<IDateShim> _mockDateShim = null!;
    private Mock<IDispatcherCommand> _mockCommand1 = null!;
    private Mock<IDispatcherCommand> _mockCommand2 = null!;
    private List<IDispatcherCommand> _commands = null!;
    private MoodDispatcherService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockScheduleService = new Mock<IScheduleConfigService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _mockTimerService = new Mock<ITimerService>();
        _mockDateShim = new Mock<IDateShim>();
        _mockCommand1 = new Mock<IDispatcherCommand>();
        _mockCommand2 = new Mock<IDispatcherCommand>();
        _commands = new List<IDispatcherCommand> { _mockCommand1.Object, _mockCommand2.Object };
        
        // Setup default behaviors
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2024, 6, 15));
    }

    private MoodDispatcherService CreateService()
    {
        return new MoodDispatcherService(
            _mockScheduleService.Object,
            _mockLoggingService.Object, 
            _mockTimerService.Object,
            _mockDateShim.Object,
            _commands.ToArray()
        );
    }
}
```

### Mock Strategy
- **IScheduleConfigService**: Mock schedule operations
- **ILoggingService**: Mock logging calls for verification
- **ITimerService**: Mock timer events for controlled testing
- **IDateShim**: Mock date/time for predictable scenarios
- **IDispatcherCommand**: Mock command implementations

## Section 6: Detailed Test Specifications (Post-Refactoring)

### 6.1 Constructor Tests

#### Test: Valid Dependencies
```csharp
[TestMethod]
public void Constructor_WithValidDependencies_ShouldInitializeCorrectly()
{
    // Arrange & Act
    var service = CreateService();
    
    // Assert - Service should initialize without throwing
    Assert.IsNotNull(service);
    
    // Verify timer setup
    _mockTimerService.Verify(x => x.Start(30000), Times.Once);
    
    // Verify initial date setup
    _mockDateShim.Verify(x => x.GetTodayDate(), Times.Once);
}
```

#### Test: Null Dependency Validation
```csharp
[TestMethod]
[DataRow("scheduleService")]
[DataRow("loggingService")]  
[DataRow("timerService")]
[DataRow("dateShim")]
[DataRow("commands")]
public void Constructor_WithNullDependency_ShouldThrowArgumentNullException(string nullDependency)
{
    // Arrange
    var scheduleService = nullDependency == "scheduleService" ? null : _mockScheduleService.Object;
    var loggingService = nullDependency == "loggingService" ? null : _mockLoggingService.Object;
    var timerService = nullDependency == "timerService" ? null : _mockTimerService.Object;
    var dateShim = nullDependency == "dateShim" ? null : _mockDateShim.Object;
    var commands = nullDependency == "commands" ? null : _commands.ToArray();
    
    // Act & Assert
    Assert.ThrowsException<ArgumentNullException>(() =>
        new MoodDispatcherService(scheduleService!, loggingService!, timerService!, dateShim!, commands!));
}
```

### 6.2 Timer Event Tests

#### Test: Date Change Detection
```csharp
[TestMethod]
public async Task OnTimerElapsed_WhenDateChanges_ShouldHandleDateChange()
{
    // Arrange
    var service = CreateService();
    var oldDate = new DateOnly(2024, 6, 14);
    var newDate = new DateOnly(2024, 6, 15);
    
    // Setup date progression
    _mockDateShim.SetupSequence(x => x.GetTodayDate())
        .Returns(oldDate)    // Initial setup
        .Returns(newDate);   // Timer tick
    
    DateChangeEventArgs? capturedEvent = null;
    service.DateChanged += (sender, e) => capturedEvent = e;
    
    // Act - Simulate timer elapsed
    await service.OnTimerElapsedAsync(); // This method needs to be made testable
    
    // Assert
    Assert.IsNotNull(capturedEvent);
    Assert.AreEqual(oldDate, capturedEvent.OldDate);
    Assert.AreEqual(newDate, capturedEvent.NewDate);
}
```

### 6.3 Command Execution Tests

#### Test: Multiple Command Execution
```csharp
[TestMethod]
public async Task HandleDateChange_WithMultipleCommands_ShouldExecuteAllCommands()
{
    // Arrange
    var service = CreateService();
    var oldDate = new DateOnly(2024, 6, 14);
    var newDate = new DateOnly(2024, 6, 15);
    
    _mockCommand1.Setup(x => x.ProcessTickAsync(oldDate, newDate, It.IsAny<MoodEntry?>()))
        .ReturnsAsync(CommandResult.Succeeded("Command1 executed"));
    
    _mockCommand2.Setup(x => x.ProcessTickAsync(oldDate, newDate, It.IsAny<MoodEntry?>()))
        .ReturnsAsync(CommandResult.Succeeded("Command2 executed"));
    
    // Act
    await service.HandleDateChangeAsync(oldDate, newDate); // Method needs to be made testable
    
    // Assert
    _mockCommand1.Verify(x => x.ProcessTickAsync(oldDate, newDate, It.IsAny<MoodEntry?>()), Times.Once);
    _mockCommand2.Verify(x => x.ProcessTickAsync(oldDate, newDate, It.IsAny<MoodEntry?>()), Times.Once);
}
```

### 6.4 Event Publishing Tests

#### Test: Auto-Save Event Publishing
```csharp
[TestMethod]
public async Task HandleDateChange_WhenAutoSaveOccurs_ShouldPublishAutoSaveEvent()
{
    // Arrange
    var service = CreateService();
    var savedRecord = new MoodEntry { StartOfWorkMood = 7, EndOfWorkMood = 8 };
    var saveDate = new DateOnly(2024, 6, 14);
    
    _mockCommand1.Setup(x => x.ProcessTickAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<MoodEntry?>()))
        .ReturnsAsync(CommandResult.Succeeded("Auto-saved", savedRecord));
    
    AutoSaveEventArgs? capturedEvent = null;
    service.AutoSaveOccurred += (sender, e) => capturedEvent = e;
    
    // Act
    await service.HandleDateChangeAsync(saveDate, new DateOnly(2024, 6, 15));
    
    // Assert
    Assert.IsNotNull(capturedEvent);
    Assert.AreEqual(savedRecord, capturedEvent.SavedRecord);
    Assert.AreEqual(saveDate, capturedEvent.SavedDate);
}
```

### 6.5 Resource Management Tests

#### Test: Dispose Pattern
```csharp
[TestMethod]
public void Dispose_ShouldDisposeTimerAndLog()
{
    // Arrange
    var service = CreateService();
    
    // Act
    service.Dispose();
    
    // Assert
    _mockTimerService.Verify(x => x.Dispose(), Times.Once);
    _mockLoggingService.Verify(x => x.LogDebug("MoodDispatcherService: Disposed"), Times.Once);
}

[TestMethod]
public void Dispose_CalledMultipleTimes_ShouldOnlyDisposeOnce()
{
    // Arrange
    var service = CreateService();
    
    // Act
    service.Dispose();
    service.Dispose();
    service.Dispose();
    
    // Assert
    _mockTimerService.Verify(x => x.Dispose(), Times.Once);
}
```

## Section 7: Event Arguments Testing

### Event Args Test Files

Each EventArgs class needs separate comprehensive testing:

#### DateChangeEventArgsTests.cs
```csharp
[TestMethod]
public void DateChangeEventArgs_WithValidDates_ShouldAssignProperties()
{
    // Arrange
    var oldDate = new DateOnly(2024, 6, 14);
    var newDate = new DateOnly(2024, 6, 15);
    var decision = AutoSaveDecision.SaveRecord;
    
    // Act
    var eventArgs = new DateChangeEventArgs
    {
        OldDate = oldDate,
        NewDate = newDate,
        AutoSaveDecision = decision
    };
    
    // Assert
    Assert.AreEqual(oldDate, eventArgs.OldDate);
    Assert.AreEqual(newDate, eventArgs.NewDate);
    Assert.AreEqual(decision, eventArgs.AutoSaveDecision);
}
```

## Section 8: Implementation Checklist

### Pre-Implementation Tasks (CRITICAL REFACTORING REQUIRED)
- [ ] **Extract IScheduleConfigService Interface**: Create abstraction for ScheduleConfigService
- [ ] **Create ITimerService Interface**: Abstract System.Timers.Timer dependency
- [ ] **Integrate IDateShim**: Replace DateTime.Today with injected date service
- [ ] **Make Timer Methods Testable**: Extract async void logic to testable methods
- [ ] **Update Constructor**: Add new interface dependencies
- [ ] **Update MauiProgram.cs**: Register new service interfaces in DI container

### Implementation Tasks (Post-Refactoring)
- [ ] **Create Test Files**: Main service + 4 EventArgs test files  
- [ ] **Constructor Tests**: Dependency validation, null checks, initialization
- [ ] **Timer Event Tests**: Date change detection, same-date ticks, error handling
- [ ] **Command Execution Tests**: Single/multiple commands, failure resilience
- [ ] **Event Publishing Tests**: All event types with proper data
- [ ] **State Management Tests**: Record tracking, date persistence, cleanup
- [ ] **Resource Management Tests**: Dispose pattern, lifecycle management
- [ ] **EventArgs Tests**: Property assignment, immutability, edge cases

### Validation Tasks
- [ ] **Build Verification**: All refactoring compiles successfully
- [ ] **Integration Testing**: Timer service works with real System.Timers.Timer
- [ ] **Mock Verification**: All dependency interactions properly mocked
- [ ] **Coverage Verification**: Achieve 90%+ coverage after refactoring
- [ ] **Performance Validation**: Timer performance not degraded by abstractions

### Documentation Tasks
- [ ] **Refactoring Documentation**: Document interface extractions and rationale
- [ ] **Testing Patterns**: Document async event testing patterns for future use
- [ ] **Architecture Notes**: Document command pattern testing approach

## Test Implementation Estimate

**Complexity**: High (Complex service with significant refactoring required)
**Refactoring Time**: 8-12 hours (interface extraction, dependency injection updates)
**Testing Time**: 6-8 hours (comprehensive test suite for complex coordinator)
**Total Estimate**: 14-20 hours
**Estimated Test Count**: 25-35 tests (main service) + 15-20 tests (EventArgs)
**Expected Coverage**: 90%+ (after refactoring enables proper testing)

**Implementation Priority**: High (Critical coordination service)
**Risk Level**: High (Major refactoring required, complex timer/threading logic)

**Key Success Factors**:
- Successful interface extraction for timer and schedule dependencies
- Proper async/await testing patterns for timer events
- Comprehensive command execution and event publishing verification
- Clean separation of timer logic from business logic

---

## Commit Strategy (Arlo's Commit Notation)

### Phase 1: Refactoring for Testability
```
^r - extract IScheduleConfigService interface from ScheduleConfigService
^r - create ITimerService abstraction for System.Timers.Timer dependency  
^r - integrate IDateShim to replace DateTime.Today static dependency
^r - extract OnTimerElapsedAsync method for testable timer event handling
^r - update MoodDispatcherService constructor with abstracted dependencies
^r - register new service interfaces in MauiProgram dependency injection
```

### Phase 2: Test Implementation
```
^f - add comprehensive MoodDispatcherService tests with 90% coverage

- Constructor tests: dependency validation, null checks, initialization verification
- Timer event tests: date change detection, same-date processing, error resilience  
- Command execution tests: single/multiple commands, failure handling, result aggregation
- Event publishing tests: DateChanged, AutoSaveOccurred, reminder events with data validation
- State management tests: current record tracking, date persistence, schedule cleanup
- Resource management tests: IDisposable pattern, lifecycle management
- EventArgs tests: DateChangeEventArgs, AutoSaveEventArgs, reminder EventArgs classes
- Mock verification: all abstracted dependencies properly tested via interfaces
- Complex coordinator service with command pattern and event-driven architecture
```

**Risk Assessment**: `^` (Validated) - Complex service requiring significant refactoring for testability, but comprehensive test coverage planned with proper dependency abstractions and async testing patterns.

**Testing Confidence**: Medium-High - After refactoring, the service becomes highly testable through dependency injection and interface abstractions. Complex coordinator logic will be thoroughly verified. 🤖