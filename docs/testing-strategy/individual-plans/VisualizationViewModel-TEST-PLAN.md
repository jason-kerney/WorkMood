# VisualizationViewModel Test Plan

## 1. Object Analysis

### Purpose & Responsibilities
The `VisualizationViewModel` provides advanced mood data visualization capabilities, presenting two-week trend analysis with detailed daily breakdowns and comprehensive error handling for no-data scenarios.

**Core Responsibilities**:

- **Advanced Data Visualization**: Load and present two-week mood trend visualization data
- **Daily Data Management**: Manage collection of daily data items with detailed formatting
- **Adaptive Error Handling**: Create meaningful empty states when no data is available
- **Color Coordination**: Calculate and assign appropriate colors for mood visualization
- **Summary Generation**: Provide comprehensive summary text for user insights
- **Performance Optimization**: Use background threading for data processing with UI dispatcher coordination
- **Factory Integration**: Coordinate with visualization service factory for data processing

### Architecture Role
**Pattern**: MVVM ViewModel (Advanced Data Visualization)
**Layer**: Presentation Layer (MVVM Clean Architecture)
**Base Class**: `ViewModelBase` (provides `INotifyPropertyChanged` implementation)
**Complexity**: **HIGH** - 314 lines, complex data processing, factory coordination, threading
**DI Registration**: Registered as `Transient` in `MauiProgram.cs`

### Dependencies Analysis

#### Constructor Dependencies (Mixed Quality)
- `IMoodDataService _moodDataService` - Interface (good abstraction)
- `INavigationService _navigationService` - Interface (good abstraction)
- `IMoodVisualizationService _visualizationService` - Interface with **factory fallback** (design concern)

#### Static Dependencies (Testing Blockers)
- **DateTime.Today** - Used once in `CreateEmptyVisualizationAsync()` for default date range
- **Application.Current?.Dispatcher.Dispatch** - Used twice for UI thread operations (major testing blocker)

#### Factory Dependencies (Design Concerns)
- **VisualizationServiceFactory** - Direct instantiation in constructor when service not provided
- **VisualizationDataAdapter** - Static adapter class for data processing

#### Threading Dependencies
- **Task.Run()** - Background thread operations for data processing
- **UI Dispatcher** - Application.Current dispatcher for collection updates

### Public Interface Documentation

#### Public Properties (with INotifyPropertyChanged)

**Data Properties**:
- `MoodVisualizationData? CurrentVisualization { get; private set; }` - Main visualization data object
- `string DateRangeText { get; private set; }` - Formatted date range display
- `string SummaryText { get; private set; }` - Summary description text

**State Properties**:
- `bool IsLoading { get; private set; }` - Loading state indicator
- `bool HasData { get; private set; }` - Data availability indicator

**Collection Properties**:
- `ObservableCollection<DailyDataItemViewModel> DailyDataItems { get; }` - Daily data items collection

#### Public Commands
- `ICommand RefreshCommand` - Refresh visualization data
- `ICommand BackToHistoryCommand` - Navigate back to history page

#### Public Methods
- **Constructor**: `VisualizationViewModel(IMoodDataService, INavigationService, IMoodVisualizationService?)` (optional service)
- `Task OnAppearingAsync()` - Load data when page appears

#### Private Methods (Key Business Logic)
- `async Task LoadVisualizationDataAsync()` - Main data loading orchestration
- `async Task PopulateDailyDataItemsAsync(List<MoodDayInfo>)` - Populate collection with threading
- `Color GetColorForDay(DateOnly date)` - Color calculation for specific dates
- `async Task HandleVisualizationErrorAsync(Exception ex)` - Error handling with user-friendly messaging
- `async Task CreateEmptyVisualizationAsync()` - Create empty state visualization
- `async Task RefreshDataAsync()` - Refresh data implementation
- `async Task NavigateBackAsync()` - Navigation implementation

#### Nested Class: DailyDataItemViewModel
**Properties**:
- `DateOnly Date { get; }` - Date for the data item
- `double? Value { get; }` - Mood value (nullable for no data)
- `bool HasData { get; }` - Data availability flag
- `Color Color { get; }` - Display color for the item
- `string Description { get; }` - Formatted description
- `string DateString { get; }` - Formatted date string (MM/dd)
- `string ValueString { get; }` - Formatted value string

## 2. Testability Assessment

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

### Current Testability Score: 5/10

**Justification**: VisualizationViewModel has moderate testability issues due to UI dispatcher dependencies and factory patterns. While core logic is testable, threading and UI operations create significant barriers.

**Strengths**:
- **Interface Dependencies**: Two of three dependencies (`IMoodDataService`, `INavigationService`) are properly abstracted
- **Clear Business Logic**: Data processing logic is well-separated and testable
- **Comprehensive Error Handling**: Well-structured exception handling patterns
- **Observable Collections**: Proper collection management for UI binding

**Significant Issues**:
- **UI Dispatcher Dependencies**: `Application.Current?.Dispatcher.Dispatch` prevents testing collection updates
- **Factory Fallback**: Constructor creates service when not provided (violates DI principles)
- **Static Adapter**: `VisualizationDataAdapter` static calls make integration testing difficult
- **Threading Complexity**: Task.Run with dispatcher coordination complicates test setup

### Hard Dependencies Identified

**UI Dispatcher Dependencies (Major)**:
```csharp
// CURRENT - Blocks testing
Application.Current?.Dispatcher.Dispatch(() =>
{
    DailyDataItems.Clear();
    foreach (var item in items)
    {
        DailyDataItems.Add(item);
    }
});
```

**Factory Fallback (Design Issue)**:
```csharp
// CURRENT - Violates DI principles
_visualizationService = visualizationService ?? new VisualizationServiceFactory().CreateVisualizationService();
```

**Static Dependencies**:
```csharp
// CURRENT - Minor testing issue
var endDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
```

**Static Adapter Usage**:
```csharp
// CURRENT - Complicates integration testing
var dailyInfoList = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_moodDataService);
SummaryText = await VisualizationDataAdapter.GetVisualizationSummaryAsync(_moodDataService);
```

### Required Refactoring (Moderate Priority)

**MODERATE PRIORITY - Needed for Comprehensive Testing**:

#### 1. Abstract UI Dispatcher Operations
```csharp
// Add IDispatcherService for UI thread operations
public interface IDispatcherService
{
    void Dispatch(Action action);
    Task DispatchAsync(Func<Task> action);
}

// Replace Application.Current dispatcher usage
private async Task PopulateDailyDataItemsAsync(List<MoodDayInfo> dailyInfoList)
{
    await Task.Run(() =>
    {
        var items = dailyInfoList.Select(/* ... */).ToList();

        _dispatcherService.Dispatch(() =>
        {
            DailyDataItems.Clear();
            foreach (var item in items)
            {
                DailyDataItems.Add(item);
            }
        });
    });
}
```

#### 2. Remove Factory Fallback Pattern
```csharp
// Update constructor to require all dependencies
public VisualizationViewModel(
    IMoodDataService moodDataService,
    INavigationService navigationService,
    IMoodVisualizationService visualizationService,
    IDispatcherService dispatcherService,
    IDateShim dateShim)
{
    _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
    _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    _visualizationService = visualizationService ?? throw new ArgumentNullException(nameof(visualizationService));
    _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
    _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
    
    // Remove: _ = LoadVisualizationDataAsync(); // Constructor should not perform async operations
}
```

#### 3. Abstract Static Dependencies
```csharp
// Replace DateTime.Today with IDateShim
private async Task CreateEmptyVisualizationAsync()
{
    var endDate = DateOnly.FromDateTime(_dateShim.Today).AddDays(-1);
    // ...
}
```

#### 4. Abstract Static Adapter (Optional)
```csharp
// Consider injecting adapter as dependency for better testability
private readonly IVisualizationDataAdapter _visualizationDataAdapter;

// Or make adapter methods non-static and testable
```

**Refactoring Priority**: **MODERATE** - Essential for comprehensive testing of collection operations.

## 3. Test Implementation Strategy

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

### Test Class Structure (Post-Refactoring)
```csharp
[TestFixture]
public class VisualizationViewModelTests
{
    private Mock<IMoodDataService> _mockMoodDataService;
    private Mock<INavigationService> _mockNavigationService;
    private Mock<IMoodVisualizationService> _mockVisualizationService;
    private Mock<IDispatcherService> _mockDispatcherService;
    private Mock<IDateShim> _mockDateShim;
    private VisualizationViewModel _sut; // System Under Test
    private DateTime _testDate = new DateTime(2024, 6, 15);
    
    [SetUp]
    public void SetUp()
    {
        // Initialize mocks with default behaviors
        SetupMockDefaults();
        
        // Create system under test
        _sut = new VisualizationViewModel(
            _mockMoodDataService.Object,
            _mockNavigationService.Object,
            _mockVisualizationService.Object,
            _mockDispatcherService.Object,
            _mockDateShim.Object);
    }
    
    private void SetupMockDefaults()
    {
        // Setup date service
        _mockDateShim.Setup(d => d.Today).Returns(_testDate);
        
        // Setup dispatcher to execute immediately
        _mockDispatcherService.Setup(d => d.Dispatch(It.IsAny<Action>()))
                              .Callback<Action>(action => action());
        
        // Setup default visualization data
        var defaultVisualization = new MoodVisualizationData
        {
            DailyValues = new DailyMoodValue[]
            {
                new DailyMoodValue { Date = DateOnly.FromDateTime(_testDate.AddDays(-1)), Value = 7.0, HasData = true, Color = Colors.Green }
            },
            StartDate = DateOnly.FromDateTime(_testDate.AddDays(-13)),
            EndDate = DateOnly.FromDateTime(_testDate.AddDays(-1)),
            Width = 280,
            Height = 100,
            MaxAbsoluteValue = 10.0
        };
        
        _mockMoodDataService.Setup(m => m.GetTwoWeekVisualizationAsync())
                           .ReturnsAsync(defaultVisualization);
        
        // Setup navigation service
        _mockNavigationService.Setup(n => n.GoBackAsync())
                              .Returns(Task.CompletedTask);
        
        // Setup visualization service
        _mockVisualizationService.Setup(v => v.ProcessVisualizationAsync(It.IsAny<MoodVisualizationData>()))
                                 .ReturnsAsync(defaultVisualization);
    }
    
    // Nested test classes for organization
    
    [TestFixture]
    public class ConstructorAndInitializationTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class DataLoadingTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class CollectionManagementTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class ErrorHandlingTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class CommandTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class ColorCalculationTests : VisualizationViewModelTests { }
    
    [TestFixture]
    public class EmptyStateTests : VisualizationViewModelTests { }
}
```

### Mock Strategy (Post-Refactoring)
- **IMoodDataService**: Mock visualization data operations with various data scenarios
- **INavigationService**: Mock navigation operations
- **IMoodVisualizationService**: Mock visualization processing service
- **IDispatcherService**: Mock UI thread operations for collection testing
- **IDateShim**: Mock date operations for consistent test results

### Test Categories
- **Constructor & Initialization Tests**: Dependency injection and initial state
- **Data Loading Tests**: Visualization data loading with various scenarios
- **Collection Management Tests**: DailyDataItems collection operations
- **Error Handling Tests**: No data scenarios and exception handling
- **Command Tests**: RefreshCommand and BackToHistoryCommand execution
- **Color Calculation Tests**: GetColorForDay logic with various data states
- **Empty State Tests**: CreateEmptyVisualizationAsync functionality

## 4. Detailed Test Cases (Post-Refactoring)

### Constructor: VisualizationViewModel(...)
**Purpose**: Validates dependency injection and initial state setup

#### Test Cases:
- **Happy Path**: Valid dependencies create functional ViewModel with correct initial state
- **Edge Cases**: 
  - Each null dependency throws appropriate `ArgumentNullException`
  - Commands are initialized and non-null
  - DailyDataItems collection is initialized and empty
  - Initial loading state is correct
- **Async Constructor Issue**: Constructor should not perform async operations (refactoring needed)

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
        Assert.That(_sut.RefreshCommand, Is.Not.Null);
        Assert.That(_sut.BackToHistoryCommand, Is.Not.Null);
        Assert.That(_sut.DailyDataItems, Is.Not.Null);
        Assert.That(_sut.DailyDataItems.Count, Is.EqualTo(0));
        
        // Initial state
        Assert.That(_sut.IsLoading, Is.True); // Starts loading
        Assert.That(_sut.HasData, Is.False);
        Assert.That(_sut.DateRangeText, Is.EqualTo("Loading..."));
        Assert.That(_sut.SummaryText, Is.EqualTo("Loading summary..."));
    });
}

[Test]
public void Constructor_WhenMoodDataServiceIsNull_ShouldThrowArgumentNullException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new VisualizationViewModel(
            null!,
            _mockNavigationService.Object,
            _mockVisualizationService.Object,
            _mockDispatcherService.Object,
            _mockDateShim.Object));
}
```

### Method: LoadVisualizationDataAsync() (via OnAppearingAsync)
**Purpose**: Main data loading orchestration with comprehensive state management

#### Test Cases:
- **Happy Path**: Successful loading updates all properties and collections
- **Loading State Management**: IsLoading is true during operation, false after completion
- **Date Range Formatting**: DateRangeText formatted correctly from visualization data
- **Summary Generation**: SummaryText updated from adapter service
- **Collection Population**: DailyDataItems populated correctly with background processing
- **Data Detection**: HasData set correctly based on visualization content
- **Exception Handling**: Exceptions trigger error handling flow

**Test Implementation Examples**:
```csharp
[Test]
public async Task OnAppearingAsync_WhenValidData_ShouldLoadAllContent()
{
    // Arrange
    var testVisualization = new MoodVisualizationData
    {
        DailyValues = new[]
        {
            new DailyMoodValue { Date = DateOnly.FromDateTime(_testDate.AddDays(-1)), Value = 8.0, HasData = true, Color = Colors.Green }
        },
        StartDate = DateOnly.FromDateTime(_testDate.AddDays(-13)),
        EndDate = DateOnly.FromDateTime(_testDate.AddDays(-1))
    };
    
    _mockMoodDataService.Setup(m => m.GetTwoWeekVisualizationAsync())
                        .ReturnsAsync(testVisualization);
    
    // Mock static adapter calls (would need adapter interface in real refactoring)
    // This is a limitation of the current architecture
    
    // Act
    await _sut.OnAppearingAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.CurrentVisualization, Is.EqualTo(testVisualization));
        Assert.That(_sut.DateRangeText, Contains.Substring("Jun 02 - Jun 14, 2024"));
        Assert.That(_sut.HasData, Is.True);
        Assert.That(_sut.IsLoading, Is.False);
        
        // Collection should be populated (via dispatcher mock)
        Assert.That(_sut.DailyDataItems.Count, Is.GreaterThan(0));
    });
    
    _mockMoodDataService.Verify(m => m.GetTwoWeekVisualizationAsync(), Times.Once);
}

[Test]
public async Task OnAppearingAsync_WhenServiceThrowsException_ShouldHandleErrorGracefully()
{
    // Arrange
    _mockMoodDataService.Setup(m => m.GetTwoWeekVisualizationAsync())
                        .ThrowsAsync(new InvalidOperationException("No data available"));
    
    // Act
    await _sut.OnAppearingAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.HasData, Is.False);
        Assert.That(_sut.DateRangeText, Is.EqualTo("No data available"));
        Assert.That(_sut.SummaryText, Contains.Substring("Start recording your daily moods"));
        Assert.That(_sut.IsLoading, Is.False);
        Assert.That(_sut.CurrentVisualization, Is.Not.Null); // Empty visualization created
    });
}
```

### Method: PopulateDailyDataItemsAsync()
**Purpose**: Background processing with UI thread coordination

#### Test Cases:
- **Background Processing**: Data processing occurs on background thread
- **UI Thread Updates**: Collection updates dispatched to UI thread
- **Collection Clearing**: Existing items cleared before adding new ones
- **Item Creation**: DailyDataItemViewModel instances created correctly
- **Dispatcher Integration**: Dispatcher service called for UI updates

### Method: GetColorForDay()
**Purpose**: Color calculation for specific dates

#### Test Cases:
- **Data Available**: Returns color from visualization data when date exists
- **No Data**: Returns LightGray when date not found
- **Null Visualization**: Returns LightGray when CurrentVisualization is null
- **Multiple Dates**: Correct colors returned for different dates

**Test Implementation Examples**:
```csharp
[Test]
public void GetColorForDay_WhenDateHasData_ShouldReturnCorrectColor()
{
    // Arrange
    var testDate = DateOnly.FromDateTime(_testDate);
    var testVisualization = new MoodVisualizationData
    {
        DailyValues = new[]
        {
            new DailyMoodValue { Date = testDate, Color = Colors.Blue, HasData = true }
        }
    };
    
    // Use reflection to test private method or make it internal for testing
    var method = typeof(VisualizationViewModel).GetMethod("GetColorForDay", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
    
    // Act
    var result = (Color)method.Invoke(_sut, new object[] { testDate });
    
    // Assert
    Assert.That(result, Is.EqualTo(Colors.Blue));
}

[Test]
public void GetColorForDay_WhenDateNotFound_ShouldReturnLightGray()
{
    // Arrange
    var testDate = DateOnly.FromDateTime(_testDate);
    var emptyVisualization = new MoodVisualizationData { DailyValues = new DailyMoodValue[0] };
    
    // Set private field using reflection
    var field = typeof(VisualizationViewModel).GetField("_currentVisualization", 
                   BindingFlags.NonPublic | BindingFlags.Instance);
    field.SetValue(_sut, emptyVisualization);
    
    var method = typeof(VisualizationViewModel).GetMethod("GetColorForDay", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
    
    // Act
    var result = (Color)method.Invoke(_sut, new object[] { testDate });
    
    // Assert
    Assert.That(result, Is.EqualTo(Colors.LightGray));
}
```

### Method: CreateEmptyVisualizationAsync()
**Purpose**: Create meaningful empty state when no data available

#### Test Cases:
- **Date Range Calculation**: Creates 14-day range ending yesterday
- **Empty Daily Values**: All daily values have HasData = false
- **Default Colors**: All daily values use LightGray color
- **Visualization Structure**: Proper MoodVisualizationData structure created
- **Collection Update**: Single placeholder item added to DailyDataItems
- **Dispatcher Usage**: Collection updates use dispatcher service

### Command: RefreshCommand
**Purpose**: Refresh visualization data

#### Test Cases:
- **Data Refresh**: Command execution triggers LoadVisualizationDataAsync
- **Loading State**: Refresh sets loading state appropriately
- **Service Calls**: Data service called again during refresh
- **Collection Updates**: Collection repopulated with fresh data

### Command: BackToHistoryCommand
**Purpose**: Navigate back to history page

#### Test Cases:
- **Navigation**: Command execution calls navigation service
- **Service Integration**: GoBackAsync called on navigation service
- **Exception Handling**: Navigation exceptions handled appropriately

### Nested Class: DailyDataItemViewModel
**Purpose**: Individual daily data item representation

#### Test Cases:
- **Property Initialization**: All properties set correctly from constructor parameters
- **Date Formatting**: DateString formatted as MM/dd
- **Value Formatting**: ValueString shows formatted value or empty for no data
- **Null Handling**: Null values handled correctly in formatting
- **Immutability**: All properties are read-only after construction

**Test Implementation Examples**:
```csharp
[Test]
public void DailyDataItemViewModel_WhenCreatedWithData_ShouldFormatCorrectly()
{
    // Arrange
    var testDate = new DateOnly(2024, 6, 15);
    var testValue = 7.5;
    var testColor = Colors.Green;
    var testDescription = "Good day";
    
    // Act
    var item = new DailyDataItemViewModel(testDate, testValue, true, testColor, testDescription);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(item.Date, Is.EqualTo(testDate));
        Assert.That(item.Value, Is.EqualTo(testValue));
        Assert.That(item.HasData, Is.True);
        Assert.That(item.Color, Is.EqualTo(testColor));
        Assert.That(item.Description, Is.EqualTo(testDescription));
        Assert.That(item.DateString, Is.EqualTo("06/15"));
        Assert.That(item.ValueString, Is.EqualTo("Value: 7.5"));
    });
}

[Test]
public void DailyDataItemViewModel_WhenCreatedWithoutData_ShouldHandleEmptyValues()
{
    // Arrange
    var testDate = new DateOnly(2024, 6, 15);
    
    // Act
    var item = new DailyDataItemViewModel(testDate, null, false, Colors.Gray, "No data");
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(item.HasData, Is.False);
        Assert.That(item.Value, Is.Null);
        Assert.That(item.ValueString, Is.EqualTo(""));
        Assert.That(item.DateString, Is.EqualTo("06/15"));
    });
}
```

## 5. MVVM-Specific Testing

### Property Change Notification Testing
**Complex Data Dependencies**: Visualization properties depend on multiple data sources.

**Testing Strategy**:
```csharp
[Test]
public async Task LoadVisualizationDataAsync_ShouldRaisePropertyChangedForAllProperties()
{
    // Arrange
    var changedProperties = new List<string>();
    _sut.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName ?? "");
    
    // Act
    await _sut.OnAppearingAsync();
    
    // Assert
    var expectedProperties = new[]
    {
        nameof(_sut.CurrentVisualization),
        nameof(_sut.DateRangeText),
        nameof(_sut.SummaryText),
        nameof(_sut.IsLoading),
        nameof(_sut.HasData)
    };
    
    foreach (var property in expectedProperties)
    {
        Assert.That(changedProperties, Contains.Item(property), 
                   $"Property {property} should have been notified");
    }
}
```

### Command Testing Strategy
**2 Commands with Different Patterns**:
- **Async Commands**: `RefreshCommand`, `BackToHistoryCommand` (async operations)
- **Data Loading**: Complex data orchestration
- **Navigation**: Simple navigation service integration

### Collection Testing
**Background Processing with UI Thread Coordination**:
- DailyDataItems collection management
- Background thread data processing
- UI dispatcher coordination
- Observable collection notifications

### Threading Testing
**Complex Threading Scenarios**:
- Task.Run for background processing
- Dispatcher.Dispatch for UI updates
- Async/await coordination

## 6. Coverage Goals

### Target Coverage (Post-Refactoring)
- **Line Coverage**: 90% minimum (complex data processing and threading)
- **Branch Coverage**: 85% minimum (error handling and conditional logic)
- **Method Coverage**: 95% (all public and key private methods tested)

### Priority Areas
1. **Data Loading Logic** (Critical for visualization functionality)
2. **Error Handling** (Critical for user experience with no data)
3. **Collection Management** (Important for UI binding)
4. **Color Calculation** (Important for visual representation)
5. **Empty State Creation** (User experience for new users)

### Acceptable Exclusions
- Complex async exception handling edge cases
- Static adapter calls (until refactored)
- Framework dispatcher internals

## 7. Implementation Checklist

### Phase 0 - REQUIRED REFACTORING ⚠️
- [ ] **Add IDispatcherService interface** and implementation for UI thread operations
- [ ] **Remove factory fallback pattern** from constructor (require all dependencies)
- [ ] **Add IDateShim dependency** and replace DateTime.Today usage
- [ ] **Remove async operation from constructor** (make initialization explicit)
- [ ] **Update DI registration** in MauiProgram.cs for new dependencies
- [ ] **Verify application still functions** after refactoring

### Phase 1 - Test Infrastructure (Post-Refactoring)
- [ ] Create `VisualizationViewModelTests.cs` in `WorkMood.MauiApp.Tests/ViewModels/`
- [ ] Setup NUnit with comprehensive mocking for all dependencies
- [ ] Create nested test classes for logical organization
- [ ] Implement test data builders for MoodVisualizationData and related objects
- [ ] Setup reflection utilities for testing private methods

### Phase 2 - Core Functionality Tests
- [ ] **Constructor Tests**: Dependency injection and initial state
- [ ] **OnAppearingAsync Tests**: Main data loading orchestration
- [ ] **LoadVisualizationDataAsync Tests**: Comprehensive data loading scenarios
- [ ] **Property Change Tests**: PropertyChanged notification verification

### Phase 3 - Data Processing Tests
- [ ] **PopulateDailyDataItemsAsync Tests**: Background processing and dispatcher coordination
- [ ] **GetColorForDay Tests**: Color calculation for various data states
- [ ] **CreateEmptyVisualizationAsync Tests**: Empty state creation
- [ ] **Collection Management Tests**: DailyDataItems operations

### Phase 4 - Command and Navigation Tests
- [ ] **RefreshCommand Tests**: Data refresh functionality
- [ ] **BackToHistoryCommand Tests**: Navigation service integration
- [ ] **Error Handling Tests**: Exception scenarios and error state management
- [ ] **Loading State Tests**: State management during async operations

### Phase 5 - Nested Class and Integration Tests
- [ ] **DailyDataItemViewModel Tests**: Nested class constructor and formatting
- [ ] **Threading Tests**: Background processing coordination
- [ ] **Dispatcher Tests**: UI thread operation mocking
- [ ] **Integration Tests**: End-to-end data loading scenarios

### Phase 6 - Coverage Verification
- [ ] Run coverage analysis: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Achieve 90%+ line coverage and 85%+ branch coverage
- [ ] Document acceptable exclusions
- [ ] Verify all user scenarios and error paths are tested

## 8. Arlo's Commit Strategy

### Planned Commits (Arlo's Commit Notation)
```bash
^r - add IDispatcherService interface for VisualizationViewModel UI thread abstraction
^r - remove factory fallback pattern from VisualizationViewModel constructor
^r - add IDateShim dependency to VisualizationViewModel for date abstraction
^r - remove async operation from VisualizationViewModel constructor
^r - update VisualizationViewModel constructor to require all dependencies
^f - add VisualizationViewModel test infrastructure with comprehensive mocks
^f - add VisualizationViewModel constructor and initialization tests
^f - add VisualizationViewModel data loading and error handling tests
^f - add VisualizationViewModel collection management and threading tests
^f - add VisualizationViewModel command and navigation tests
^f - add DailyDataItemViewModel tests and integration tests to achieve 90% coverage
```

### Commit Granularity
- **One refactoring concern per commit** (safer incremental approach)
- **Manual verification** after each refactoring step
- **Test implementation** after refactoring is complete and verified
- **Incremental coverage building** to track progress

---

**Success Criteria Met**: This test plan provides a comprehensive strategy for testing VisualizationViewModel, but **REQUIRES MODERATE REFACTORING** before comprehensive testing can begin. The threading and UI dispatcher dependencies are the primary barriers.

**Architectural Analysis**: VisualizationViewModel demonstrates **mixed architecture quality**:
- **Strengths**: Good interface usage for core dependencies, comprehensive error handling, advanced data processing
- **Issues**: UI dispatcher dependencies, factory fallback pattern, static adapter usage, async constructor

**Business Impact**: Visualization functionality is critical for user engagement and insights. The two-week trend analysis provides users with valuable feedback about their mood patterns.

**Next Steps**: 
1. **RECOMMENDED**: Complete Phase 0 refactoring to improve testability
2. Proceed with manual verification gate
3. Implement comprehensive test suite following the 6-phase approach