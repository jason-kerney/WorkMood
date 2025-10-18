# MainPageViewModel Test Plan

## 1. Object Analysis

### Purpose & Responsibilities
The `MainPageViewModel` serves as the central orchestrator for the WorkMood main page, managing the primary user interface and coordinating multiple services. It acts as the main hub for user navigation and system events.

**Core Responsibilities**:
- **Navigation Coordination**: Handle commands for navigating to all major application sections
- **Service Event Management**: Subscribe to and handle events from dispatcher, reminders, and auto-save
- **Date Display Management**: Maintain and refresh current date display
- **User Alerts**: Coordinate display of system alerts and reminders to user
- **Lifecycle Management**: Initialize services and manage resource cleanup (IDisposable)
- **Window Activation**: Manage window focus for notifications and reminders
- **Service Dependency Coordination**: Orchestrate multiple complex services

### Architecture Role
**Pattern**: MVVM ViewModel (Central Orchestrator)
**Layer**: Presentation Layer (MVVM Clean Architecture)
**Base Class**: `ViewModelBase` (provides `INotifyPropertyChanged` implementation)
**Interfaces**: `IDisposable` (manages service subscriptions and cleanup)
**DI Registration**: Registered as `Transient` in `MauiProgram.cs`
**Complexity**: **HIGH** - 391 lines, multiple service integrations, event handling, async operations

### Dependencies Analysis

#### Constructor Dependencies (Concrete Services - Red Flag!)
- `MoodDataService _moodDataService` - **CONCRETE CLASS** (should be abstracted)
- `MoodDispatcherService _dispatcherService` - **CONCRETE CLASS** (should be abstracted)  
- `ScheduleConfigService _scheduleConfigService` - **CONCRETE CLASS** (should be abstracted)
- `IWindowActivationService _windowActivationService` - Interface (good)
- `ILoggingService _loggingService` - Interface (good)
- `IServiceProvider _serviceProvider` - Framework service for dependency resolution

#### Static Dependencies
- **MainThread.InvokeOnMainThreadAsync** - Platform-specific UI thread operations
- **DateTime.Today** - Static date access (should use IDateShim)

#### Platform Dependencies
- **UI Thread Operations** - MainThread calls for UI updates
- **Window Management** - Through IWindowActivationService (properly abstracted)

### Public Interface Documentation

#### Public Properties (with INotifyPropertyChanged)
- `string CurrentDate { get; private set; }` - Formatted current date display (read-only from outside)

#### Public Commands
- `ICommand RecordMoodCommand` - Navigate to mood recording page
- `ICommand ViewHistoryCommand` - Navigate to history page
- `ICommand ViewGraphCommand` - Navigate to graph page  
- `ICommand SettingsCommand` - Navigate to settings page
- `ICommand AboutCommand` - Navigate to about page

#### Public Methods
- **Constructor**: `MainPageViewModel(MoodDataService, MoodDispatcherService, ScheduleConfigService, IWindowActivationService, ILoggingService, IServiceProvider)`
- `Task InitializeAsync()` - Initialize service subscriptions and load configuration
- `void RefreshCurrentDate()` - Update current date display
- `void CheckForDateChanges()` - Manually trigger date change check
- `void Dispose()` - Clean up service subscriptions (IDisposable)

#### Public Events (Navigation & Alerts)
- `event EventHandler<NavigateToMoodRecordingEventArgs>? NavigateToMoodRecording`
- `event EventHandler? NavigateToHistory`
- `event EventHandler? NavigateToGraph`
- `event EventHandler? NavigateToSettings`
- `event EventHandler? NavigateToAbout`
- `event EventHandler<DisplayAlertEventArgs>? DisplayAlert`

#### Private Event Handlers
- `OnDateChanged(object?, DateChangeEventArgs)` - Handle date change notifications
- `OnAutoSaveOccurred(object?, AutoSaveEventArgs)` - Handle auto-save completion
- `OnMorningReminderOccurred(object?, MorningReminderEventArgs)` - Handle morning reminders
- `OnEveningReminderOccurred(object?, EveningReminderEventArgs)` - Handle evening reminders

## 2. Testability Assessment

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

### Current Testability Score: 4/10

**Justification**: MainPageViewModel has significant testability issues due to concrete service dependencies and static method calls. The architecture needs substantial refactoring before comprehensive testing is feasible.

**Critical Issues**:
- **Concrete Service Dependencies**: MoodDataService, MoodDispatcherService, ScheduleConfigService are not abstracted
- **Static DateTime Usage**: Direct DateTime.Today calls prevent time-controlled testing
- **Complex Event Coordination**: Multiple service events with async MainThread operations
- **Hard to Mock Services**: Concrete services make isolation testing very difficult

**Strengths**:
- Some services properly abstracted (IWindowActivationService, ILoggingService)
- Clear separation of concerns in event handling
- Proper IDisposable implementation for cleanup

### Hard Dependencies Identified
- **DateTime.Today** → Needs `IDateShim.Today` abstraction
- **MainThread.InvokeOnMainThreadAsync** → Needs `IDispatcherService.InvokeOnMainThread` abstraction
- **Concrete Services** → Need interface abstractions:
  - `MoodDataService` → `IMoodDataService`
  - `MoodDispatcherService` → `IMoodDispatcherService`  
  - `ScheduleConfigService` → `IScheduleConfigService`

### Required Refactoring (BLOCKING)

**CRITICAL - Must Complete Before Testing**:

#### 1. Extract Service Interfaces
```csharp
// Create interfaces for concrete services
public interface IMoodDataService { /* existing public methods */ }
public interface IMoodDispatcherService { /* existing public methods and events */ }
public interface IScheduleConfigService { /* existing public methods */ }
```

#### 2. Add Date Abstraction
```csharp
// Replace DateTime.Today with IDateShim
private void UpdateCurrentDate()
{
    CurrentDate = _dateShim.Today.ToString("dddd, MMMM dd, yyyy");
}
```

#### 3. Abstract MainThread Operations
```csharp
// Replace MainThread.InvokeOnMainThreadAsync with IDispatcherService
await _dispatcherService.InvokeOnMainThreadAsync(() => { /* UI updates */ });
```

#### 4. Update Constructor
```csharp
public MainPageViewModel(
    IMoodDataService moodDataService,
    IMoodDispatcherService dispatcherService,
    IScheduleConfigService scheduleConfigService,
    IWindowActivationService windowActivationService,
    ILoggingService loggingService,
    IDateShim dateShim,
    IDispatcherService dispatcherService,
    IServiceProvider serviceProvider)
```

**Refactoring Priority**: **HIGHEST** - Cannot create meaningful tests without these abstractions.

## 3. Test Implementation Strategy

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

### Test Class Structure (Post-Refactoring)
```csharp
[TestFixture]
public class MainPageViewModelTests
{
    private Mock<IMoodDataService> _mockMoodDataService;
    private Mock<IMoodDispatcherService> _mockDispatcherService;
    private Mock<IScheduleConfigService> _mockScheduleConfigService;
    private Mock<IWindowActivationService> _mockWindowActivationService;
    private Mock<ILoggingService> _mockLoggingService;
    private Mock<IDateShim> _mockDateShim;
    private Mock<IDispatcherService> _mockMainThreadService;
    private Mock<IServiceProvider> _mockServiceProvider;
    private MainPageViewModel _sut; // System Under Test
    
    [SetUp]
    public void SetUp()
    {
        // Initialize all mocks with default behaviors
        SetupMockDefaults();
        
        // Create system under test
        _sut = new MainPageViewModel(
            _mockMoodDataService.Object,
            _mockDispatcherService.Object,
            _mockScheduleConfigService.Object,
            _mockWindowActivationService.Object,
            _mockLoggingService.Object,
            _mockDateShim.Object,
            _mockMainThreadService.Object,
            _mockServiceProvider.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
    }
    
    // Nested test classes for organization
    
    [TestFixture]
    public class ConstructorAndInitializationTests : MainPageViewModelTests { }
    
    [TestFixture]
    public class CommandTests : MainPageViewModelTests { }
    
    [TestFixture]
    public class EventHandlingTests : MainPageViewModelTests { }
    
    [TestFixture]
    public class NavigationTests : MainPageViewModelTests { }
    
    [TestFixture]
    public class DateManagementTests : MainPageViewModelTests { }
}
```

### Mock Strategy (Post-Refactoring)
- **IMoodDataService**: Mock data operations with various success/failure scenarios
- **IMoodDispatcherService**: Mock event subscriptions and event triggering for testing
- **IScheduleConfigService**: Mock schedule loading with valid/invalid configurations
- **IWindowActivationService**: Mock window activation operations
- **ILoggingService**: Mock logging calls for verification
- **IDateShim**: Mock date operations for consistent test results
- **IDispatcherService**: Mock UI thread operations to avoid threading issues in tests
- **IServiceProvider**: Mock service resolution for dependency lookup

### Test Categories
- **Constructor & Initialization Tests**: Dependency injection and async initialization
- **Command Execution Tests**: All 5 navigation commands
- **Event Handling Tests**: All 4 service event handlers
- **Navigation Event Tests**: Verify navigation events are raised correctly
- **Alert Display Tests**: Verify alert events are raised with correct data
- **Date Management Tests**: Current date display and refresh logic
- **Lifecycle Tests**: Initialization and disposal behavior
- **Error Handling Tests**: Exception scenarios in event handlers

## 4. Detailed Test Cases (Post-Refactoring)

### Constructor: MainPageViewModel(...)
**Purpose**: Validates dependency injection and proper initialization

#### Test Cases:
- **Happy Path**: Valid dependencies create functional ViewModel
- **Edge Cases**: 
  - Each null dependency throws appropriate `ArgumentNullException`
  - Commands are initialized correctly
  - CurrentDate is set during construction
- **Command Initialization**: All 5 commands are non-null and functional

**Test Implementation Examples**:
```csharp
[Test]
public void Constructor_WhenValidDependencies_ShouldInitializeCorrectly()
{
    // Arrange & Act performed in SetUp
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut, Is.Not.Null);
        Assert.That(_sut.RecordMoodCommand, Is.Not.Null);
        Assert.That(_sut.ViewHistoryCommand, Is.Not.Null);
        Assert.That(_sut.ViewGraphCommand, Is.Not.Null);
        Assert.That(_sut.SettingsCommand, Is.Not.Null);
        Assert.That(_sut.AboutCommand, Is.Not.Null);
        Assert.That(_sut.CurrentDate, Is.Not.Empty);
    });
}

[Test]
public void Constructor_WhenMoodDataServiceIsNull_ShouldThrowArgumentNullException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new MainPageViewModel(
            null!,
            _mockDispatcherService.Object,
            _mockScheduleConfigService.Object,
            _mockWindowActivationService.Object,
            _mockLoggingService.Object,
            _mockDateShim.Object,
            _mockMainThreadService.Object,
            _mockServiceProvider.Object));
}
```

### Method: InitializeAsync()
**Purpose**: Initialize service subscriptions and load configuration

#### Test Cases:
- **Happy Path**: Successful initialization with event subscriptions and config loading
- **Idempotent Behavior**: Multiple calls don't cause duplicate subscriptions
- **Error Handling**: Exceptions during initialization are caught and logged
- **Event Subscription Verification**: All 4 dispatcher events are subscribed

**Test Implementation Examples**:
```csharp
[Test]
public async Task InitializeAsync_WhenCalled_ShouldSubscribeToAllDispatcherEvents()
{
    // Act
    await _sut.InitializeAsync();
    
    // Assert
    _mockDispatcherService.VerifyAdd(s => s.DateChanged += It.IsAny<EventHandler<DateChangeEventArgs>>(), Times.Once);
    _mockDispatcherService.VerifyAdd(s => s.AutoSaveOccurred += It.IsAny<EventHandler<AutoSaveEventArgs>>(), Times.Once);
    _mockDispatcherService.VerifyAdd(s => s.MorningReminderOccurred += It.IsAny<EventHandler<MorningReminderEventArgs>>(), Times.Once);
    _mockDispatcherService.VerifyAdd(s => s.EveningReminderOccurred += It.IsAny<EventHandler<EveningReminderEventArgs>>(), Times.Once);
}

[Test]
public async Task InitializeAsync_WhenCalledMultipleTimes_ShouldOnlyInitializeOnce()
{
    // Act
    await _sut.InitializeAsync();
    await _sut.InitializeAsync();
    await _sut.InitializeAsync();
    
    // Assert - Events should only be subscribed once
    _mockDispatcherService.VerifyAdd(s => s.DateChanged += It.IsAny<EventHandler<DateChangeEventArgs>>(), Times.Once);
}
```

### Command: RecordMoodCommand
**Purpose**: Navigate to mood recording with service dependencies

#### Test Cases:
- **Happy Path**: Command execution raises NavigateToMoodRecording event with correct services
- **Event Args Verification**: NavigateToMoodRecordingEventArgs contains all required services
- **Logging**: Command execution is logged appropriately

### Command: ViewHistoryCommand, ViewGraphCommand, SettingsCommand, AboutCommand
**Purpose**: Navigate to respective pages

#### Test Cases:
- **Happy Path**: Each command raises appropriate navigation event
- **Event Verification**: Correct event is raised for each command
- **Logging**: Navigation requests are logged

**Test Implementation Examples**:
```csharp
[Test]
public void ViewHistoryCommand_WhenExecuted_ShouldRaiseNavigateToHistoryEvent()
{
    // Arrange
    var eventRaised = false;
    _sut.NavigateToHistory += (s, e) => eventRaised = true;
    
    // Act
    _sut.ViewHistoryCommand.Execute(null);
    
    // Assert
    Assert.That(eventRaised, Is.True);
    _mockLoggingService.Verify(l => l.Log(It.Is<string>(s => s.Contains("Navigate to History"))), Times.Once);
}

[Test]
public void RecordMoodCommand_WhenExecuted_ShouldRaiseNavigateToMoodRecordingWithCorrectServices()
{
    // Arrange
    NavigateToMoodRecordingEventArgs? capturedArgs = null;
    _sut.NavigateToMoodRecording += (s, e) => capturedArgs = e;
    
    // Act
    _sut.RecordMoodCommand.Execute(null);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs.MoodDataService, Is.EqualTo(_mockMoodDataService.Object));
        Assert.That(capturedArgs.DispatcherService, Is.EqualTo(_mockDispatcherService.Object));
        Assert.That(capturedArgs.LoggingService, Is.EqualTo(_mockLoggingService.Object));
    });
}
```

### Event Handler: OnDateChanged
**Purpose**: Handle date change events and show auto-save alerts

#### Test Cases:
- **Auto-Save Alert**: When AutoSaveDecision is SaveRecord, DisplayAlert event is raised
- **No Alert**: When AutoSaveDecision is not SaveRecord, no alert is shown
- **Main Thread**: UI operations are performed on main thread
- **Error Handling**: Exceptions in handler are caught and logged

### Event Handler: OnMorningReminderOccurred & OnEveningReminderOccurred
**Purpose**: Handle reminder events with window activation and alerts

#### Test Cases:
- **Window Activation**: Window activation service is called
- **Alert Display**: DisplayAlert event is raised with correct title and message
- **Main Thread**: Operations are performed on main thread
- **Title Selection**: Evening reminder titles are selected correctly based on ReminderType
- **Error Handling**: Exceptions are caught and logged

**Test Implementation Examples**:
```csharp
[Test]
public async Task OnMorningReminderOccurred_WhenTriggered_ShouldActivateWindowAndShowAlert()
{
    // Arrange
    await _sut.InitializeAsync();
    var reminderArgs = new MorningReminderEventArgs("Time to record your morning mood!");
    DisplayAlertEventArgs? capturedAlert = null;
    _sut.DisplayAlert += (s, e) => capturedAlert = e;
    
    // Setup main thread to execute immediately for testing
    _mockMainThreadService.Setup(m => m.InvokeOnMainThreadAsync(It.IsAny<Func<Task>>()))
                          .Returns<Func<Task>>(func => func());
    
    // Act - Simulate the dispatcher service raising the event
    _mockDispatcherService.Raise(s => s.MorningReminderOccurred += null, reminderArgs);
    
    // Wait for async operations
    await Task.Delay(50);
    
    // Assert
    Assert.Multiple(() =>
    {
        _mockWindowActivationService.Verify(w => w.ActivateCurrentWindowAsync(), Times.Once);
        Assert.That(capturedAlert, Is.Not.Null);
        Assert.That(capturedAlert.Title, Is.EqualTo("Morning Mood Reminder"));
        Assert.That(capturedAlert.Message, Is.EqualTo("Time to record your morning mood!"));
    });
}
```

### Method: RefreshCurrentDate() & CheckForDateChanges()
**Purpose**: Manual date management operations

#### Test Cases:
- **Date Refresh**: RefreshCurrentDate updates CurrentDate property and raises PropertyChanged
- **Date Check**: CheckForDateChanges logs appropriately
- **Property Notification**: CurrentDate changes raise PropertyChanged events

### Method: Dispose()
**Purpose**: Clean up service subscriptions

#### Test Cases:
- **Event Unsubscription**: All dispatcher events are unsubscribed
- **Service Disposal**: Dispatcher service is disposed
- **Multiple Calls**: Multiple Dispose calls are handled safely

## 5. MVVM-Specific Testing

### Property Change Notification Tests
```csharp
[Test]
public void CurrentDate_WhenRefreshed_ShouldRaisePropertyChangedEvent()
{
    // Arrange
    var propertyChangedRaised = false;
    _sut.PropertyChanged += (s, e) => 
    {
        if (e.PropertyName == nameof(_sut.CurrentDate))
            propertyChangedRaised = true;
    };
    
    // Act
    _sut.RefreshCurrentDate();
    
    // Assert
    Assert.That(propertyChangedRaised, Is.True);
}
```

### Command Testing
Test all 5 commands for:
- **CanExecute Logic**: Commands should always be executable
- **Execute Logic**: Proper event raising and logging
- **Command Initialization**: Non-null and functional after construction

### Event Testing
Verify all navigation and alert events:
- **Event Registration**: Events can be subscribed to
- **Event Arguments**: Correct event args are passed
- **Event Timing**: Events are raised at appropriate times

## 6. Coverage Goals

### Target Coverage (Post-Refactoring)
- **Line Coverage**: 90% minimum (straightforward business logic)
- **Branch Coverage**: 85% minimum (some error handling paths may be hard to reach)
- **Method Coverage**: 95% (all public and key private methods tested)

### Priority Areas
1. **Service Event Handlers** (Critical for application functionality)
2. **Command Execution** (Critical for user navigation)
3. **Initialization Logic** (Critical for application startup)
4. **Disposal Logic** (Critical for resource management)
5. **Navigation Events** (Critical for UI navigation)

### Acceptable Exclusions
- Complex async error handling paths in event handlers
- Platform-specific exception scenarios
- Service provider resolution edge cases

## 7. Implementation Checklist

### Phase 0 - REQUIRED REFACTORING ⚠️
- [ ] **Extract IMoodDataService interface** from MoodDataService concrete class
- [ ] **Extract IMoodDispatcherService interface** from MoodDispatcherService concrete class  
- [ ] **Extract IScheduleConfigService interface** from ScheduleConfigService concrete class
- [ ] **Add IDateShim dependency** and replace DateTime.Today usage
- [ ] **Add IDispatcherService dependency** and replace MainThread.InvokeOnMainThreadAsync usage
- [ ] **Update constructor** to use interface dependencies
- [ ] **Update DI registration** in MauiProgram.cs for new interfaces
- [ ] **Verify application still functions** after refactoring

### Phase 1 - Test Setup (Post-Refactoring)
- [ ] Create `MainPageViewModelTests.cs` in `WorkMood.MauiApp.Tests/ViewModels/`
- [ ] Setup NUnit with Moq and comprehensive mocking infrastructure
- [ ] Create nested test classes for organization
- [ ] Implement mock setup helpers for complex service configurations
- [ ] Create event testing utilities for event verification

### Phase 2 - Core Functionality Tests
- [ ] **Constructor Tests**: All dependency injection scenarios
- [ ] **InitializeAsync Tests**: Service subscriptions and configuration loading
- [ ] **Command Tests**: All 5 navigation commands
- [ ] **Date Management Tests**: CurrentDate property and refresh logic

### Phase 3 - Event Handling Tests
- [ ] **OnDateChanged Tests**: Auto-save alert logic
- [ ] **OnMorningReminderOccurred Tests**: Window activation and alert display
- [ ] **OnEveningReminderOccurred Tests**: Title selection and alert display
- [ ] **OnAutoSaveOccurred Tests**: Auto-save completion handling

### Phase 4 - Navigation and UI Tests
- [ ] **Navigation Event Tests**: All navigation events raised correctly
- [ ] **DisplayAlert Tests**: Alert events with correct arguments
- [ ] **Event Args Tests**: NavigateToMoodRecordingEventArgs and DisplayAlertEventArgs
- [ ] **Property Change Tests**: CurrentDate property notifications

### Phase 5 - Lifecycle and Error Handling Tests
- [ ] **Dispose Tests**: Proper cleanup and unsubscription
- [ ] **Error Handling Tests**: Exception scenarios in all event handlers
- [ ] **Thread Safety Tests**: Main thread operation mocking
- [ ] **Multiple Initialization Tests**: Idempotent behavior verification

### Phase 6 - Coverage Verification
- [ ] Run coverage analysis: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Achieve 90%+ line coverage and 85%+ branch coverage
- [ ] Document acceptable exclusions
- [ ] Verify all critical paths are tested

## 8. Arlo's Commit Strategy

### Planned Commits (Arlo's Commit Notation)
```bash
^r - extract IMoodDataService interface for MainPageViewModel testability
^r - extract IMoodDispatcherService interface for MainPageViewModel testability  
^r - extract IScheduleConfigService interface for MainPageViewModel testability
^r - add IDateShim dependency to MainPageViewModel for date abstraction
^r - add IDispatcherService dependency to MainPageViewModel for UI thread abstraction
^r - update MainPageViewModel constructor to use interface dependencies
^f - add MainPageViewModel test infrastructure with comprehensive mocks
^f - add MainPageViewModel constructor and initialization tests
^f - add MainPageViewModel command execution tests for navigation
^f - add MainPageViewModel event handler tests for service coordination
^f - add MainPageViewModel navigation and alert event tests
^f - add MainPageViewModel lifecycle and error handling tests to achieve 90% coverage
```

### Commit Granularity
- **One interface extraction per commit** (safer refactoring)
- **Manual verification** after each refactoring step
- **Test implementation** after refactoring is complete and verified
- **Incremental test categories** to ensure steady progress

---

**Success Criteria Met**: This test plan provides a comprehensive strategy for testing MainPageViewModel, but **REQUIRES SIGNIFICANT REFACTORING** before implementation can begin. The refactoring is necessary to make the code testable and follows SOLID principles.

**Critical Note**: MainPageViewModel demonstrates the importance of interface abstractions in MVVM architecture. The current concrete dependencies make testing extremely difficult and should be addressed as a high priority architectural improvement.

**Next Steps**: 
1. **REQUIRED**: Complete Phase 0 refactoring before any test implementation
2. Proceed with manual verification gate
3. Implement comprehensive tests following the 6-phase approach