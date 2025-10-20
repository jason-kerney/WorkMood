# MoodRecordingViewModel Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## 1. Object Analysis

### Purpose & Responsibilities
The `MoodRecordingViewModel` is the core business logic controller for mood data entry, implementing sophisticated mood recording workflows with complex state management and business rules. It serves as the primary interface between users and the mood tracking system.

**Core Responsibilities**:

- **Dual-Period Mood Management**: Handle both morning (StartOfWork) and evening (EndOfWork) mood recordings for each day
- **Complex Business Rules Enforcement**: Implement sequential workflow where morning must be saved before evening can be recorded
- **UI State Orchestration**: Manage complex visibility and enabled states for 20+ mood selection buttons and editing controls
- **Data Persistence**: Save and load mood entries through MoodDataService with comprehensive error handling
- **Edit Mode Management**: Support post-save editing with cancel functionality and state restoration
- **User Experience Flow**: Provide immediate feedback, validation, and guided workflow navigation
- **Error Handling & Validation**: Comprehensive input validation with user-friendly error messaging

### Architecture Role
**Pattern**: MVVM ViewModel (Complex Business Logic Controller)
**Layer**: Presentation Layer (MVVM Clean Architecture)
**Base Class**: `ViewModelBase` (provides `INotifyPropertyChanged` implementation)
**Complexity**: **VERY HIGH** - 749 lines, intricate state management, business rule enforcement
**DI Registration**: Registered as `Transient` in `MauiProgram.cs`

### Dependencies Analysis

#### Constructor Dependencies (Mixed - Some Concrete!)
- `MoodDataService _moodDataService` - **CONCRETE CLASS** (should be abstracted)
- `MoodDispatcherService _dispatcherService` - **CONCRETE CLASS** (should be abstracted)  
- `ILoggingService _loggingService` - Interface (good)

#### Static Dependencies (Testing Blockers)
- **DateTime.Today** - Used in 3 locations for current date operations
- **DateOnly.FromDateTime(DateTime.Today)** - Date conversion operations

#### Business Logic Dependencies
- **MoodEntry Model** - Data structure for mood storage
- **Complex UI State Rules** - Sequential workflow enforcement (morning → evening)
- **Validation Logic** - Input validation and business rule checking

### Public Interface Documentation

#### Public Properties (with INotifyPropertyChanged)

**Core Data Properties**:
- `string CurrentDate { get; }` - Formatted current date display (read-only)
- `bool IsLoading { get; set; }` - Loading state indicator
- `int? SelectedMorningMood { get; set; }` - Currently selected morning mood (1-10, nullable)
- `int? SelectedEveningMood { get; set; }` - Currently selected evening mood (1-10, nullable)

**State Management Properties**:
- `bool MorningMoodSaved { get; set; }` - Indicates if morning mood has been saved
- `bool EveningMoodSaved { get; set; }` - Indicates if evening mood has been saved  
- `bool IsEditingMorning { get; set; }` - Morning edit mode flag
- `bool IsEditingEvening { get; set; }` - Evening edit mode flag

**UI Display Properties**:
- `string MorningMoodLabel { get; set; }` - Dynamic label for morning mood section
- `string EveningMoodLabel { get; set; }` - Dynamic label for evening mood section
- `bool IsMorningInfoVisible { get; set; }` - Morning info panel visibility
- `bool IsEveningInfoVisible { get; set; }` - Evening info panel visibility
- `bool IsEditMorningVisible { get; set; }` - Morning edit button visibility
- `bool IsEditEveningVisible { get; set; }` - Evening edit button visibility
- `string EditMorningText { get; set; }` - Dynamic morning edit button text
- `string EditEveningText { get; set; }` - Dynamic evening edit button text

**Computed Properties (Business Logic)**:
- `bool IsSaveMorningEnabled { get; }` - Morning save button enabled state
- `bool IsSaveEveningEnabled { get; }` - Evening save button enabled state
- `Color MorningMoodBorderColor { get; }` - Dynamic border color based on state
- `Color EveningMoodBorderColor { get; }` - Dynamic border color based on state
- `bool AreMorningMoodButtonsVisible { get; }` - Morning mood button visibility
- `bool AreEveningMoodButtonsVisible { get; }` - Evening mood button visibility  
- `bool AreEveningMoodButtonsEnabled { get; }` - Evening mood button enabled state

**Individual Mood Button Properties (20 total)**:
- `bool IsMorningMood[1-10]Selected { get; }` - Individual morning button selection states
- `bool IsEveningMood[1-10]Selected { get; }` - Individual evening button selection states
- `bool IsEveningMood[1-10]Enabled { get; }` - Individual evening button enabled states

#### Public Commands
- `ICommand MorningMoodSelectedCommand` - Handle morning mood selection (parameter: int mood)
- `ICommand EveningMoodSelectedCommand` - Handle evening mood selection (parameter: int mood)
- `ICommand SaveMorningCommand` - Save morning mood to data service
- `ICommand SaveEveningCommand` - Save evening mood to data service
- `ICommand EditMorningCommand` - Enter/exit morning edit mode
- `ICommand EditEveningCommand` - Enter/exit evening edit mode
- `ICommand BackToMainCommand` - Navigate back to main page
- `ICommand LoadDataCommand` - Reload today's mood data

#### Public Events
- `event EventHandler<string>? ErrorOccurred` - Error messaging to UI
- `event EventHandler? NavigateBackRequested` - Navigation back to main page

#### Public Methods
- **Constructor**: `MoodRecordingViewModel(MoodDataService, MoodDispatcherService, ILoggingService)`

#### Private Methods (Key Business Logic)
- `Task LoadTodaysMoodAsync()` - Load existing mood data for current date
- `void UpdateUIState()` - Comprehensive UI state recalculation
- `void UpdateMorningMoodLabel()` - Dynamic morning label updates
- `void UpdateEveningMoodLabel()` - Dynamic evening label updates
- `void ExecuteMorningMoodSelected(int mood)` - Morning selection business logic
- `void ExecuteEveningMoodSelected(int mood)` - Evening selection with workflow enforcement
- `async void ExecuteSaveMorning()` - Morning save with error handling
- `async void ExecuteSaveEvening()` - Evening save with error handling
- `void ExecuteEditMorning()` - Morning edit mode toggle with state restoration
- `void ExecuteEditEvening()` - Evening edit mode toggle with state restoration

## 2. Testability Assessment

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

### Current Testability Score: 3/10

**Justification**: MoodRecordingViewModel has severe testability issues due to concrete service dependencies and static date operations. The complexity of business rules makes testing critical, but architectural barriers prevent effective unit testing.

**Critical Issues**:

- **Concrete Service Dependencies**: `MoodDataService` and `MoodDispatcherService` prevent isolation
- **Static DateTime Usage**: `DateTime.Today` calls in 3 locations prevent controlled testing
- **Complex State Machine**: 749 lines of interconnected state logic difficult to test comprehensively
- **Async Constructor Logic**: `InitializeViewModel()` called from constructor performs async operations
- **Event-Driven Architecture**: Command executions trigger complex event chains

**Strengths**:
- Some abstractions exist (ILoggingService interface)
- Clear separation of command execution logic
- Comprehensive error handling patterns
- Well-structured property change notifications

### Hard Dependencies Identified

**Static Date Dependencies**:
```csharp
// CURRENT - Hard to test
public string CurrentDate => DateTime.Today.ToString("dddd, MMMM dd, yyyy");
var today = DateOnly.FromDateTime(DateTime.Today);
_currentMoodEntry = new MoodEntry { Date = DateOnly.FromDateTime(DateTime.Today) };
```

**Concrete Service Dependencies**:
```csharp
// CURRENT - Hard to test  
private readonly MoodDataService _moodDataService;
private readonly MoodDispatcherService _dispatcherService;
```

**Async Constructor Logic**:
```csharp
// CURRENT - Hard to test
public MoodRecordingViewModel(...)
{
    // ... 
    InitializeCommands();
    InitializeViewModel(); // ← Async operation in constructor
}
```

### Required Refactoring (BLOCKING)

**CRITICAL - Must Complete Before Testing**:

#### 1. Extract Service Interfaces
```csharp
// Create interfaces for concrete services
public interface IMoodDataService 
{
    Task<MoodEntry?> GetMoodEntryAsync(DateOnly date);
    Task SaveMoodEntryAsync(MoodEntry entry);
    // ... other public methods
}

public interface IMoodDispatcherService
{
    void NotifyMoodSaved(MoodEntry entry);
    // ... other public methods and events
}
```

#### 2. Add Date Abstraction
```csharp
// Replace DateTime.Today with IDateShim
private readonly IDateShim _dateShim;

public string CurrentDate => _dateShim.Today.ToString("dddd, MMMM dd, yyyy");
private async Task LoadTodaysMoodAsync()
{
    var today = DateOnly.FromDateTime(_dateShim.Today);
    // ...
}
```

#### 3. Separate Initialization from Construction
```csharp
// Move async logic out of constructor
public MoodRecordingViewModel(
    IMoodDataService moodDataService,
    IMoodDispatcherService dispatcherService,
    ILoggingService loggingService,
    IDateShim dateShim)
{
    // Constructor only sets dependencies and initializes commands
    _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
    _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
    _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
    
    InitializeCommands();
    // Remove InitializeViewModel() call - make it explicit
}

// Add public initialization method
public async Task InitializeAsync()
{
    await LoadTodaysMoodAsync();
    UpdateUIState();
}
```

#### 4. Update Constructor
```csharp
public MoodRecordingViewModel(
    IMoodDataService moodDataService,
    IMoodDispatcherService dispatcherService, 
    ILoggingService loggingService,
    IDateShim dateShim)
```

**Refactoring Priority**: **HIGHEST** - Business logic too critical to leave untested.

## 3. Test Implementation Strategy

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

### Test Class Structure (Post-Refactoring)
```csharp
[TestFixture]
public class MoodRecordingViewModelTests
{
    private Mock<IMoodDataService> _mockMoodDataService;
    private Mock<IMoodDispatcherService> _mockDispatcherService;
    private Mock<ILoggingService> _mockLoggingService;
    private Mock<IDateShim> _mockDateShim;
    private MoodRecordingViewModel _sut; // System Under Test
    private DateTime _testDate = new DateTime(2024, 3, 15); // Friday for testing
    
    [SetUp]
    public void SetUp()
    {
        // Initialize all mocks with default behaviors
        SetupMockDefaults();
        
        // Create system under test
        _sut = new MoodRecordingViewModel(
            _mockMoodDataService.Object,
            _mockDispatcherService.Object,
            _mockLoggingService.Object,
            _mockDateShim.Object);
    }
    
    [TearDown] 
    public void TearDown()
    {
        _sut?.Dispose(); // If IDisposable implemented later
    }
    
    private void SetupMockDefaults()
    {
        // Setup consistent test date
        _mockDateShim.Setup(d => d.Today).Returns(_testDate);
        
        // Setup data service defaults
        _mockMoodDataService.Setup(m => m.GetMoodEntryAsync(It.IsAny<DateOnly>()))
                           .ReturnsAsync((MoodEntry?)null); // No existing data by default
        _mockMoodDataService.Setup(m => m.SaveMoodEntryAsync(It.IsAny<MoodEntry>()))
                           .Returns(Task.CompletedTask);
        
        // Setup logging service
        _mockLoggingService.Setup(l => l.LogInfo(It.IsAny<string>()));
        _mockLoggingService.Setup(l => l.LogError(It.IsAny<string>()));
    }
    
    // Nested test classes for organization
    
    [TestFixture]
    public class ConstructorAndInitializationTests : MoodRecordingViewModelTests { }
    
    [TestFixture]
    public class MoodSelectionTests : MoodRecordingViewModelTests { }
    
    [TestFixture]
    public class BusinessRuleTests : MoodRecordingViewModelTests { }
    
    [TestFixture]
    public class SaveOperationTests : MoodRecordingViewModelTests { }
    
    [TestFixture] 
    public class EditModeTests : MoodRecordingViewModelTests { }
    
    [TestFixture]
    public class UIStateTests : MoodRecordingViewModelTests { }
    
    [TestFixture]
    public class NavigationAndEventTests : MoodRecordingViewModelTests { }
}
```

### Mock Strategy (Post-Refactoring)
- **IMoodDataService**: Mock data operations with various success/failure scenarios and existing data states
- **IMoodDispatcherService**: Mock notification calls for coordination
- **ILoggingService**: Mock logging calls for verification of business logic flow
- **IDateShim**: Mock date operations for consistent test results across different dates

### Test Categories
- **Constructor & Initialization Tests**: Dependency injection and initial state setup
- **Mood Selection Tests**: Morning and evening mood selection with validation
- **Business Rule Tests**: Sequential workflow enforcement (morning before evening)
- **Save Operation Tests**: Data persistence with error handling
- **Edit Mode Tests**: Edit mode entry/exit and state restoration
- **UI State Tests**: Complex visibility and enabled state calculations  
- **Navigation & Event Tests**: Error events and navigation events

## 4. Detailed Test Cases (Post-Refactoring)

### Constructor: MoodRecordingViewModel(...)
**Purpose**: Validates dependency injection and initial state setup

#### Test Cases:
- **Happy Path**: Valid dependencies create functional ViewModel with correct initial state
- **Edge Cases**: 
  - Each null dependency throws appropriate `ArgumentNullException`
  - All commands are initialized and non-null
  - Initial properties have expected default values
- **Command Initialization**: All 8 commands are non-null and have proper CanExecute logic

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
        Assert.That(_sut.MorningMoodSelectedCommand, Is.Not.Null);
        Assert.That(_sut.EveningMoodSelectedCommand, Is.Not.Null);
        Assert.That(_sut.SaveMorningCommand, Is.Not.Null);
        Assert.That(_sut.SaveEveningCommand, Is.Not.Null);
        Assert.That(_sut.EditMorningCommand, Is.Not.Null);
        Assert.That(_sut.EditEveningCommand, Is.Not.Null);
        Assert.That(_sut.BackToMainCommand, Is.Not.Null);
        Assert.That(_sut.LoadDataCommand, Is.Not.Null);
        
        // Initial state
        Assert.That(_sut.CurrentDate, Is.EqualTo("Friday, March 15, 2024"));
        Assert.That(_sut.SelectedMorningMood, Is.Null);
        Assert.That(_sut.SelectedEveningMood, Is.Null);
        Assert.That(_sut.MorningMoodSaved, Is.False);
        Assert.That(_sut.EveningMoodSaved, Is.False);
    });
}

[Test]
public void Constructor_WhenMoodDataServiceIsNull_ShouldThrowArgumentNullException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new MoodRecordingViewModel(
            null!,
            _mockDispatcherService.Object,
            _mockLoggingService.Object,
            _mockDateShim.Object));
}
```

### Method: InitializeAsync()
**Purpose**: Load existing mood data and set initial UI state

#### Test Cases:
- **No Existing Data**: Fresh day with no saved moods
- **Morning Only Saved**: Existing morning mood, no evening mood
- **Both Moods Saved**: Complete mood entry exists  
- **Data Service Error**: Exception during data loading
- **UI State Updates**: Proper state calculation after data loading

**Test Implementation Examples**:
```csharp
[Test]
public async Task InitializeAsync_WhenNoExistingData_ShouldSetupFreshState()
{
    // Arrange
    _mockMoodDataService.Setup(m => m.GetMoodEntryAsync(It.IsAny<DateOnly>()))
                       .ReturnsAsync((MoodEntry?)null);
    
    // Act
    await _sut.InitializeAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.MorningMoodSaved, Is.False);
        Assert.That(_sut.EveningMoodSaved, Is.False);
        Assert.That(_sut.SelectedMorningMood, Is.Null);
        Assert.That(_sut.SelectedEveningMood, Is.Null);
        Assert.That(_sut.MorningMoodLabel, Is.EqualTo("No mood selected"));
        Assert.That(_sut.EveningMoodLabel, Is.EqualTo("Save morning mood first"));
    });
}

[Test]
public async Task InitializeAsync_WhenMorningMoodExists_ShouldLoadMorningState()
{
    // Arrange
    var existingEntry = new MoodEntry 
    { 
        Date = DateOnly.FromDateTime(_testDate),
        StartOfWork = 7,
        EndOfWork = null
    };
    _mockMoodDataService.Setup(m => m.GetMoodEntryAsync(It.IsAny<DateOnly>()))
                       .ReturnsAsync(existingEntry);
    
    // Act
    await _sut.InitializeAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.MorningMoodSaved, Is.True);
        Assert.That(_sut.EveningMoodSaved, Is.False);
        Assert.That(_sut.SelectedMorningMood, Is.EqualTo(7));
        Assert.That(_sut.SelectedEveningMood, Is.Null);
        Assert.That(_sut.MorningMoodLabel, Is.EqualTo("Mood saved (hidden to prevent bias)"));
        Assert.That(_sut.AreEveningMoodButtonsEnabled, Is.True); // Can now select evening
    });
}
```

### Command: MorningMoodSelectedCommand
**Purpose**: Handle morning mood selection with validation

#### Test Cases:
- **Valid Selection**: Mood values 1-10 are accepted
- **Multiple Selections**: Changing selection updates properly
- **UI State Updates**: Selection triggers proper property change notifications
- **Business Logic**: Selected mood updates internal MoodEntry

### Command: EveningMoodSelectedCommand  
**Purpose**: Handle evening mood selection with business rule enforcement

#### Test Cases:
- **Business Rule Enforcement**: Cannot select evening without morning saved
- **Valid Selection After Morning**: Evening selection works when morning is saved
- **Edit Mode Selection**: Can select evening when in edit mode even if not saved
- **Error Event**: Proper error event raised when business rules violated
- **Saved State Blocking**: Cannot select when saved unless in edit mode

**Test Implementation Examples**:
```csharp
[Test]
public void EveningMoodSelectedCommand_WhenMorningNotSaved_ShouldRaiseErrorEvent()
{
    // Arrange
    string? errorMessage = null;
    _sut.ErrorOccurred += (s, e) => errorMessage = e;
    
    // Act
    _sut.EveningMoodSelectedCommand.Execute(5);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(errorMessage, Is.EqualTo("Please save your morning mood before selecting evening mood."));
        Assert.That(_sut.SelectedEveningMood, Is.Null); // Should not be set
        _mockLoggingService.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Blocking evening selection"))), Times.Once);
    });
}

[Test]
public void EveningMoodSelectedCommand_WhenMorningSavedAndNotInEditMode_ShouldAllowSelection()
{
    // Arrange
    _sut.MorningMoodSaved = true;
    
    // Act
    _sut.EveningMoodSelectedCommand.Execute(8);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.SelectedEveningMood, Is.EqualTo(8));
        _mockLoggingService.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Setting evening mood to: 8"))), Times.Once);
    });
}
```

### Command: SaveMorningCommand & SaveEveningCommand
**Purpose**: Persist mood data to service with error handling

#### Test Cases:
- **Successful Save**: Happy path data persistence
- **Validation Failure**: Save without selection shows error
- **Service Exception**: Data service errors are caught and reported
- **State Updates**: Successful save updates saved flags and UI state
- **Navigation**: Successful morning save triggers navigation back
- **Dispatcher Notification**: Save operations notify dispatcher service

### Command: EditMorningCommand & EditEveningCommand
**Purpose**: Enter/exit edit mode with state restoration

#### Test Cases:
- **Enter Edit Mode**: Clicking edit enables editing and updates UI
- **Cancel Edit Mode**: Clicking edit again restores saved state  
- **State Restoration**: Canceling edit restores previously saved mood values
- **UI State Changes**: Edit mode affects button visibility and enabled states
- **Edit Text Updates**: Edit button text changes between "Edit" and "Cancel"

### Computed Properties: UI State Logic
**Purpose**: Complex business logic for UI visibility and enabled states

#### Test Cases:
- **IsSaveMorningEnabled**: Only enabled when morning mood selected
- **IsSaveEveningEnabled**: Only enabled when evening mood selected AND morning saved
- **AreMorningMoodButtonsVisible**: Visible based on edit mode and saved state
- **AreEveningMoodButtonsEnabled**: Enabled only when morning saved OR evening editing
- **Border Color Logic**: Colors change based on saved/editing states
- **Individual Button States**: 20 mood buttons have correct enabled/selected states

### Property Change Notifications
**Purpose**: Verify INotifyPropertyChanged implementation

#### Test Cases:
- **Mood Selection Changes**: Selecting mood raises PropertyChanged for selection and related UI properties
- **Save State Changes**: Saving raises PropertyChanged for saved flags and dependent properties
- **Edit Mode Changes**: Edit mode changes raise PropertyChanged for visibility and enabled properties
- **Cascading Updates**: Single property changes trigger multiple related property notifications

**Test Implementation Examples**:
```csharp
[Test]
public void SelectedMorningMood_WhenChanged_ShouldRaiseMultiplePropertyChangedEvents()
{
    // Arrange
    var changedProperties = new List<string>();
    _sut.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName ?? "");
    
    // Act
    _sut.SelectedMorningMood = 6;
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(changedProperties, Contains.Item(nameof(_sut.SelectedMorningMood)));
        Assert.That(changedProperties, Contains.Item(nameof(_sut.IsSaveMorningEnabled)));
        Assert.That(changedProperties, Contains.Item(nameof(_sut.MorningMoodBorderColor)));
        
        // Should raise events for all 10 mood button selection states
        for (int i = 1; i <= 10; i++)
        {
            Assert.That(changedProperties, Contains.Item($"IsMorningMood{i}Selected"));
        }
    });
}
```

## 5. MVVM-Specific Testing

### Property Change Notification Testing
**Complex Cascading Notifications**: MoodRecordingViewModel has some of the most complex property change cascading in the application.

**Testing Strategy**:
```csharp
[Test]
public void MorningMoodSaved_WhenChanged_ShouldTriggerExtensiveUIUpdates()
{
    // Arrange
    var changedProperties = new HashSet<string>();
    _sut.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName ?? "");
    
    // Act
    _sut.MorningMoodSaved = true;
    
    // Assert - Verify all expected cascading changes
    var expectedProperties = new[]
    {
        nameof(_sut.MorningMoodSaved),
        nameof(_sut.IsSaveMorningEnabled),
        nameof(_sut.IsSaveEveningEnabled), 
        nameof(_sut.MorningMoodBorderColor),
        nameof(_sut.EveningMoodBorderColor),
        nameof(_sut.AreMorningMoodButtonsVisible),
        nameof(_sut.AreEveningMoodButtonsEnabled),
        "IsEveningMood1Enabled", "IsEveningMood2Enabled", /* ... all 10 evening buttons */
    };
    
    foreach (var property in expectedProperties)
    {
        Assert.That(changedProperties, Contains.Item(property), 
                   $"Property {property} should have been notified");
    }
}
```

### Command Testing Strategy
**8 Commands with Different Behaviors**:

- **Parameter Commands**: `MorningMoodSelectedCommand`, `EveningMoodSelectedCommand` (take int parameter)
- **Conditional Commands**: `SaveMorningCommand`, `SaveEveningCommand` (have CanExecute logic)
- **Toggle Commands**: `EditMorningCommand`, `EditEveningCommand` (toggle states)
- **Navigation Commands**: `BackToMainCommand` (triggers events)
- **Async Commands**: `LoadDataCommand` (async operations)

### Data Binding Scenarios
**Two-Way Binding Properties**:
- Mood selection properties bind to UI buttons
- Edit mode properties control UI visibility
- Label properties provide dynamic text display

### Business Rule Testing
**Sequential Workflow Rules**:
1. Morning must be saved before evening can be selected
2. Saved moods cannot be changed unless in edit mode
3. Edit mode allows temporary changes that can be cancelled
4. Navigation occurs automatically after morning save

## 6. Coverage Goals

### Target Coverage (Post-Refactoring)
- **Line Coverage**: 95% minimum (complex business logic must be thoroughly tested)
- **Branch Coverage**: 90% minimum (many conditional paths in UI state logic) 
- **Method Coverage**: 98% (all public and key private methods tested)

### Priority Areas
1. **Business Rule Enforcement** (Critical - prevents data corruption)
2. **Command Execution Logic** (Critical - core user interactions)
3. **UI State Management** (High - affects user experience)
4. **Data Persistence** (High - data integrity)
5. **Error Handling** (High - user experience)

### Acceptable Exclusions
- Complex async exception handling in edge cases
- Platform-specific UI thread operations
- Constructor validation (already covered by DI framework)

## 7. Implementation Checklist

### Phase 0 - REQUIRED REFACTORING ⚠️
- [ ] **Extract IMoodDataService interface** from MoodDataService concrete class
- [ ] **Extract IMoodDispatcherService interface** from MoodDispatcherService concrete class
- [ ] **Add IDateShim dependency** and replace all DateTime.Today usage
- [ ] **Separate async initialization** from constructor (add InitializeAsync method)
- [ ] **Update constructor** to use interface dependencies
- [ ] **Update DI registration** in MauiProgram.cs for new interfaces
- [ ] **Verify application still functions** after refactoring

### Phase 1 - Test Infrastructure (Post-Refactoring)
- [ ] Create `MoodRecordingViewModelTests.cs` in `WorkMood.MauiApp.Tests/ViewModels/`
- [ ] Setup NUnit with comprehensive mocking for all dependencies
- [ ] Create nested test classes for logical organization
- [ ] Implement test data builders for MoodEntry scenarios
- [ ] Create helper methods for complex state setup

### Phase 2 - Core Functionality Tests
- [ ] **Constructor Tests**: All dependency injection scenarios
- [ ] **InitializeAsync Tests**: Data loading with various existing states
- [ ] **Mood Selection Tests**: Morning and evening selection logic
- [ ] **Property Change Tests**: Comprehensive PropertyChanged verification

### Phase 3 - Business Logic Tests
- [ ] **Business Rule Tests**: Sequential workflow enforcement
- [ ] **Save Operation Tests**: Data persistence with success/failure scenarios
- [ ] **Edit Mode Tests**: Edit/cancel functionality with state restoration
- [ ] **Validation Tests**: Input validation and error messaging

### Phase 4 - UI State Tests
- [ ] **Computed Property Tests**: All 25+ computed properties for UI state
- [ ] **Command State Tests**: CanExecute logic for conditional commands
- [ ] **Visibility Tests**: Complex visibility rules for UI elements
- [ ] **Color Logic Tests**: Dynamic border color calculations

### Phase 5 - Integration & Error Tests
- [ ] **Event Tests**: ErrorOccurred and NavigateBackRequested events
- [ ] **Service Integration Tests**: Verify proper service method calls
- [ ] **Error Handling Tests**: Exception scenarios in all commands
- [ ] **Navigation Tests**: Navigation event triggering

### Phase 6 - Coverage Verification
- [ ] Run coverage analysis: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Achieve 95%+ line coverage and 90%+ branch coverage
- [ ] Document acceptable exclusions
- [ ] Verify all critical business paths are tested

## 8. Arlo's Commit Strategy

### Planned Commits (Arlo's Commit Notation)
```bash
^r - extract IMoodDataService interface for MoodRecordingViewModel testability
^r - extract IMoodDispatcherService interface for MoodRecordingViewModel testability
^r - add IDateShim dependency to MoodRecordingViewModel for date abstraction
^r - separate async initialization from MoodRecordingViewModel constructor
^r - update MoodRecordingViewModel constructor to use interface dependencies
^f - add MoodRecordingViewModel test infrastructure with comprehensive mocks
^f - add MoodRecordingViewModel constructor and initialization tests
^f - add MoodRecordingViewModel mood selection and business rule tests
^f - add MoodRecordingViewModel save operation and error handling tests
^f - add MoodRecordingViewModel edit mode and state management tests
^f - add MoodRecordingViewModel UI state and computed property tests
^f - add MoodRecordingViewModel event and navigation tests to achieve 95% coverage
```

### Commit Granularity
- **One interface extraction per commit** (safer refactoring approach)
- **Manual verification** after each refactoring step
- **Test implementation** in logical groupings after refactoring complete
- **Incremental coverage improvement** to track progress

---

**Success Criteria Met**: This test plan provides a comprehensive strategy for testing MoodRecordingViewModel, but **REQUIRES SIGNIFICANT REFACTORING** before implementation can begin. This is the most complex ViewModel in the application with critical business logic that must be thoroughly tested.

**Critical Note**: MoodRecordingViewModel contains the core business logic of the WorkMood application. The sequential workflow rules (morning before evening) and complex state management make this one of the most important components to test thoroughly. The current architecture with concrete dependencies makes this testing impossible without substantial refactoring.

**Business Impact**: This ViewModel handles the primary user interaction - recording mood data. Bugs in this component directly affect data integrity and user experience. The refactoring investment will pay significant dividends in code quality and maintainability.

**Next Steps**: 
1. **REQUIRED**: Complete Phase 0 refactoring (highest priority due to business criticality)
2. Proceed with manual verification gate  
3. Implement comprehensive test suite following the 6-phase approach