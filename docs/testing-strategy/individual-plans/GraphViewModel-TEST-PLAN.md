# GraphViewModel Test Plan

## 1. Object Analysis

### Purpose & Responsibilities
The `GraphViewModel` is the most complex ViewModel in WorkMood, serving as the business logic controller for the Graph page. It handles mood data visualization with extensive customization options, making it a critical component for user experience.

**Core Responsibilities**:
- **Data Visualization**: Generate and display mood data graphs with various configurations
- **Date Range Management**: Handle date range selection and automatic graph updates
- **Graph Customization**: Control visual aspects (colors, backgrounds, data points, axes, titles)
- **Export & Sharing**: Provide graph export and sharing functionality
- **User Interaction**: Handle color picker, background loading, and display options
- **State Management**: Track loading states, data availability, and user interface visibility
- **Graph Mode Management**: Switch between different graph display modes (Impact, etc.)

### Architecture Role
**Pattern**: MVVM ViewModel (Complex Business Logic Controller)
**Layer**: Presentation Layer (MVVM Clean Architecture)
**Base Class**: `ViewModelBase` (provides `INotifyPropertyChanged` implementation)
**DI Registration**: Registered as `Transient` in `MauiProgram.cs`
**Complexity**: **HIGH** - 922 lines, 25+ properties, 6 commands, multiple async operations

### Dependencies Analysis

#### Constructor Dependencies (Interfaces/Services)
- `IMoodDataService _moodDataService` - Retrieves mood data for visualization
- `ILineGraphService _lineGraphService` - Generates graph images and handles drawing operations
- `IDateShim _dateShim` - Provides date operations for date range calculations

#### Static Dependencies
- **None identified** - All dependencies properly injected through constructor

#### Platform Dependencies
- **File System Access** - For custom background loading and graph export (through services)
- **UI Thread Operations** - Property change notifications for complex UI updates

### Public Interface Documentation

#### Public Properties (with INotifyPropertyChanged)
**Core Data Properties**:
- `ObservableCollection<DateRangeItem> DateRanges { get; }` - Available date range options (read-only)
- `DateRangeItem SelectedDateRange { get; set; }` - Currently selected date range (triggers graph update)
- `ImageSource? GraphImageSource { get; set; }` - Generated graph image for display

**State Management Properties**:
- `bool IsLoading { get; set; }` - Indicates graph generation in progress
- `bool HasGraphData { get; set; }` - Whether graph has data to display
- `bool HasNoData { get; set; }` - Inverse of HasGraphData for UI binding
- `string StatusMessage { get; set; }` - User feedback messages
- `bool HasStatusMessage { get; set; }` - Whether status message should be visible

**Display Configuration Properties**:
- `bool ShowDataPoints { get; set; }` - Control data point visibility (triggers graph update)
- `bool ShowAxesAndGrid { get; set; }` - Control axes and grid visibility (triggers graph update)
- `bool ShowTitle { get; set; }` - Control title visibility (triggers graph update)
- `bool ShowTrendLine { get; set; }` - Control trend line visibility (triggers graph update)

**Custom Background Properties**:
- `ImageSource? CustomBackgroundSource { get; set; }` - Custom background image
- `bool HasCustomBackground { get; set; }` - Whether custom background is set
- `int CustomBackgroundWidth { get; set; }` - Custom background width
- `int CustomBackgroundHeight { get; set; }` - Custom background height
- `string CustomBackgroundPath { get; set; }` - Path to custom background file

**Color Management Properties**:
- `Color SelectedLineColor { get; set; }` - Current line color (triggers graph update)
- `bool IsColorPickerVisible { get; set; }` - Color picker UI visibility
- `double RedValue { get; set; }` - Red component (0-255) for custom colors
- `double GreenValue { get; set; }` - Green component (0-255) for custom colors
- `double BlueValue { get; set; }` - Blue component (0-255) for custom colors

**Graph Mode Properties**:
- `GraphMode SelectedGraphMode { get; set; }` - Current graph display mode
- `GraphModeItem SelectedGraphModeItem { get; set; }` - Wrapper for graph mode with display name
- `ObservableCollection<GraphModeItem> GraphModes { get; }` - Available graph modes (read-only)

**Layout Properties**:
- `double AvailableContainerWidth { get; set; }` - Available container width for layout
- `double AvailableContainerHeight { get; set; }` - Available container height for layout
- `int GraphWidth { get; }` - Calculated graph width (computed property)
- `int ExportGraphWidth { get; }` - Export-specific graph width (computed property)
- `int EffectiveGraphWidth { get; }` - Effective width considering custom background

#### Public Commands
- `ICommand ExportGraphCommand` - Exports current graph to file system
- `ICommand ShareGraphCommand` - Shares current graph through platform sharing
- `ICommand LoadCustomBackgroundCommand` - Loads custom background image from file system
- `ICommand ClearCustomBackgroundCommand` - Removes custom background
- `ICommand ToggleColorPickerCommand` - Shows/hides color picker UI
- `ICommand SelectColorCommand` - Selects predefined color from picker

#### Public Methods
- **Constructor**: `GraphViewModel(IMoodDataService, ILineGraphService, IDateShim)`
- **Constructor Overload**: `GraphViewModel(IMoodDataService, ILineGraphService)` - Uses default DateShim
- `Task UpdateGraphAsync()` - Main method to regenerate graph (likely public based on usage)

#### Events
- **PropertyChanged** (inherited from ViewModelBase) - Raised for all property changes

## 2. Testability Assessment

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

### Current Testability Score: 7/10

**Justification**: GraphViewModel has good dependency injection but is highly complex with many interconnected properties and async operations. The high complexity creates testing challenges but the architecture is sound.

**Strengths**:
- All external dependencies properly injected
- Uses IDateShim abstraction for testable date operations
- Clear separation of concerns with service dependencies
- Properties trigger appropriate side effects

**Challenges**:
- **High Complexity**: 25+ properties with interdependencies
- **Async Operations**: Multiple async methods with complex state management
- **Auto-Update Triggers**: Property changes trigger automatic graph regeneration
- **Complex State Logic**: Multiple boolean flags that must be coordinated

### Hard Dependencies Identified
- **File System Operations** - Handled through IMoodDataService and ILineGraphService (good)
- **Image Processing** - Handled through ILineGraphService (good)
- **None requiring immediate attention** - Architecture is sound

### Required Refactoring
**Minor Improvements Recommended**:
- Consider extracting color management logic into separate service for better testability
- May benefit from state machine pattern for managing loading/data/error states
- Consider breaking down into smaller, focused ViewModels if possible

**No Blocking Issues**: Object is testable as-is, but complexity requires careful test design.

## 3. Test Implementation Strategy

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

### Test Class Structure
```csharp
[TestFixture]
public class GraphViewModelTests
{
    private Mock<IMoodDataService> _mockMoodDataService;
    private LineGraphServiceSpy _lineGraphServiceSpy;
    private Mock<IDateShim> _mockDateShim;
    private GraphViewModel _sut; // System Under Test
    
    // Test data builders
    private MoodCollection _testMoodData;
    private List<DateRangeItem> _testDateRanges;
    private ImageSource _testGraphImage;
    
    [SetUp]
    public void SetUp()
    {
        // Mock initialization
        _mockMoodDataService = new Mock<IMoodDataService>();
        _mockDateShim = new Mock<IDateShim>();
        
        // Create spy that can forward to real service or return test doubles
        var realLineGraphService = new LineGraphService(/* real dependencies */);
        _lineGraphServiceSpy = new LineGraphServiceSpy(realLineGraphService);
        
        // Test data setup
        SetupTestData();
        
        // System under test creation
        _sut = new GraphViewModel(_mockMoodDataService.Object, _lineGraphServiceSpy, _mockDateShim.Object);
    }
    
    [TearDown] 
    public void TearDown()
    {
        // Reset spy state
        _lineGraphServiceSpy?.Reset();
        _sut?.Dispose(); // If IDisposable
    }
    
    // Nested test classes for organization
    
    [TestFixture]
    public class ConstructorTests : GraphViewModelTests { }
    
    [TestFixture] 
    public class PropertyTests : GraphViewModelTests { }
    
    [TestFixture]
    public class CommandTests : GraphViewModelTests { }
    
    [TestFixture]
    public class GraphUpdateTests : GraphViewModelTests { }
    
    [TestFixture]
    public class ColorManagementTests : GraphViewModelTests { }
}
```

### Mock Strategy
- **IMoodDataService**: Mock data retrieval with various scenarios (success, empty, error)
- **ILineGraphService**: **Use SPY PATTERN** - Create spy that forwards calls to real service to avoid drawing errors, while allowing verification of calls and return value overrides
- **IDateShim**: Mock date operations for consistent test results
- **ImageSource**: Use test doubles for image-related testing
- **Test Data Builders**: Create builders for MoodCollection, DateRangeItem, GraphModeItem

#### ILineGraphService Spy Implementation Strategy
```csharp
public class LineGraphServiceSpy : ILineGraphService
{
    private readonly ILineGraphService _realService;
    private readonly List<GraphData> _generateGraphCalls = new();
    
    public LineGraphServiceSpy(ILineGraphService realService)
    {
        _realService = realService;
    }
    
    public async Task<ImageSource> GenerateGraphAsync(GraphData graphData)
    {
        _generateGraphCalls.Add(graphData);
        
        // Option 1: Forward to real service (for integration-like tests)
        if (ShouldForwardToReal)
            return await _realService.GenerateGraphAsync(graphData);
            
        // Option 2: Return test ImageSource (for pure unit tests)
        return TestImageSource ?? await _realService.GenerateGraphAsync(graphData);
    }
    
    // Spy verification methods
    public IReadOnlyList<GraphData> GenerateGraphCalls => _generateGraphCalls.AsReadOnly();
    public bool WasGenerateGraphCalled => _generateGraphCalls.Any();
    public GraphData? LastGenerateGraphCall => _generateGraphCalls.LastOrDefault();
    
    // Control spy behavior
    public bool ShouldForwardToReal { get; set; } = true;
    public ImageSource? TestImageSource { get; set; }
}
```

### Test Categories
- **Constructor & Initialization Tests**: Dependency injection and setup
- **Property Change Notification Tests**: All 25+ properties
- **Graph Update Integration Tests**: Auto-update triggers and async operations
- **Command Execution Tests**: All 6 commands with success/error scenarios
- **State Management Tests**: Loading states, data availability coordination
- **Color Management Tests**: RGB color calculations and picker interactions
- **Date Range Tests**: Date range calculations and selection
- **Custom Background Tests**: File loading and background management
- **Error Handling Tests**: Service failures and exception scenarios

## 4. Detailed Test Cases

Due to GraphViewModel's complexity, I'll provide key test case categories with representative examples:

### Constructor: GraphViewModel(...)
**Purpose**: Validates dependency injection and proper initialization

#### Test Cases:
- **Happy Path**: Valid dependencies initialize all collections and default values
- **Edge Cases**: 
  - Null `moodDataService` throws `ArgumentNullException`
  - Null `lineGraphService` throws `ArgumentNullException`  
  - Null `dateShim` throws `ArgumentNullException`
- **Initialization Verification**: DateRanges and GraphModes collections populated
- **Default Values**: All properties have expected default values

**Test Implementation Examples**:
```csharp
[Test]
public void Constructor_WhenValidDependencies_ShouldInitializeCorrectly()
{
    // Arrange
    var mockMoodData = new Mock<IMoodDataService>();
    var mockLineGraph = new Mock<ILineGraphService>();
    var mockDateShim = new Mock<IDateShim>();
    
    // Act
    var sut = new GraphViewModel(mockMoodData.Object, mockLineGraph.Object, mockDateShim.Object);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(sut, Is.Not.Null);
        Assert.That(sut.DateRanges, Is.Not.Empty);
        Assert.That(sut.GraphModes, Is.Not.Empty);
        Assert.That(sut.IsLoading, Is.False);
        Assert.That(sut.HasNoData, Is.True);
        Assert.That(sut.ShowDataPoints, Is.True);
        Assert.That(sut.ShowAxesAndGrid, Is.True);
        Assert.That(sut.ShowTitle, Is.True);
        Assert.That(sut.SelectedLineColor, Is.EqualTo(Colors.Blue));
    });
}

[Test]
public void Constructor_WhenMoodDataServiceIsNull_ShouldThrowArgumentNullException()
{
    // Arrange
    var mockLineGraph = new Mock<ILineGraphService>();
    var mockDateShim = new Mock<IDateShim>();
    
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new GraphViewModel(null, mockLineGraph.Object, mockDateShim.Object));
}
```

### Property: SelectedDateRange
**Purpose**: Controls date range for graph data with auto-update trigger

#### Test Cases:
- **Happy Path**: Setting valid date range triggers PropertyChanged and UpdateGraphAsync
- **Edge Cases**: Setting same value doesn't trigger updates
- **Side Effects**: GraphWidth and ExportGraphWidth properties notify changes
- **Async Behavior**: UpdateGraphAsync called automatically

**Test Implementation Examples**:
```csharp
[Test]
public async Task SelectedDateRange_WhenChanged_ShouldTriggerGraphUpdateAndNotifyProperties()
{
    // Arrange
    var newDateRange = new DateRangeItem(DateRange.Last7Days, _mockDateShim.Object);
    var propertyChangedEvents = new List<string>();
    
    _sut.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName);
    
    // Act
    _sut.SelectedDateRange = newDateRange;
    
    // Wait for async operations
    await Task.Delay(100);
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.SelectedDateRange, Is.EqualTo(newDateRange));
        Assert.That(propertyChangedEvents, Contains.Item(nameof(_sut.SelectedDateRange)));
        Assert.That(propertyChangedEvents, Contains.Item(nameof(_sut.GraphWidth)));
        Assert.That(propertyChangedEvents, Contains.Item(nameof(_sut.ExportGraphWidth)));
    });
    
    // Verify service was called for graph update
    _mockLineGraphService.Verify(s => s.GenerateGraphAsync(It.IsAny<GraphData>()), Times.AtLeastOnce);
}
```

### Property: ShowDataPoints, ShowAxesAndGrid, ShowTitle
**Purpose**: Control graph visual elements with auto-update triggers

#### Test Cases:
- **Happy Path**: Changing values triggers PropertyChanged and UpdateGraphAsync
- **Performance**: Verify only necessary updates occur
- **Coordination**: Multiple changes should batch appropriately

### Property: SelectedLineColor & RGB Components
**Purpose**: Manages line color with RGB component synchronization

#### Test Cases:
- **Color Synchronization**: Setting SelectedLineColor updates RGB values
- **RGB Updates**: Changing RGB components updates SelectedLineColor
- **Graph Updates**: Color changes trigger graph regeneration
- **Property Notifications**: RGB properties notify changes correctly

**Test Implementation Examples**:
```csharp
[Test]
public void SelectedLineColor_WhenChanged_ShouldUpdateRGBValuesAndNotifyProperties()
{
    // Arrange
    var newColor = Colors.Red;
    var propertyChangedEvents = new List<string>();
    
    _sut.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName);
    
    // Act
    _sut.SelectedLineColor = newColor;
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.SelectedLineColor, Is.EqualTo(newColor));
        Assert.That(_sut.RedValue, Is.EqualTo(255).Within(1));
        Assert.That(_sut.GreenValue, Is.EqualTo(0).Within(1));
        Assert.That(_sut.BlueValue, Is.EqualTo(0).Within(1));
        Assert.That(propertyChangedEvents, Contains.Item(nameof(_sut.SelectedLineColor)));
    });
}

[Test]
public void RedValue_WhenChanged_ShouldUpdateSelectedLineColorAndNotifyProperties()
{
    // Arrange
    var propertyChangedEvents = new List<string>();
    _sut.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName);
    
    // Act
    _sut.RedValue = 128;
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.RedValue, Is.EqualTo(128).Within(1));
        Assert.That(_sut.SelectedLineColor.Red, Is.EqualTo(128/255.0f).Within(0.01f));
        Assert.That(propertyChangedEvents, Contains.Item(nameof(_sut.SelectedLineColor)));
    });
}
```

### Command: ExportGraphCommand
**Purpose**: Exports current graph to file system

#### Test Cases:
- **Happy Path**: Successful export through service
- **Error Handling**: Service exceptions handled gracefully
- **State Management**: Loading states managed during export
- **User Feedback**: Status messages provided for success/failure

### Command: LoadCustomBackgroundCommand
**Purpose**: Loads custom background image from file system

#### Test Cases:
- **Happy Path**: Successful background loading with property updates
- **File Not Found**: Graceful handling of missing files
- **Invalid File**: Handling of non-image files
- **Property Updates**: CustomBackground properties updated correctly

### Method: UpdateGraphAsync (Integration Test)
**Purpose**: Core graph generation method coordinating multiple services

#### Test Cases:
- **Happy Path**: Successful graph generation with valid data
- **No Data**: Appropriate handling when no mood data available  
- **Service Errors**: LineGraphService exceptions handled properly
- **State Coordination**: Loading, HasGraphData, HasNoData managed correctly
- **Status Messages**: User feedback provided throughout process

**Test Implementation Examples**:
```csharp
[Test]
public async Task UpdateGraphAsync_WhenValidData_ShouldGenerateGraphAndUpdateProperties()
{
    // Arrange
    var testMoodData = CreateTestMoodData();
    var testGraphImage = CreateTestImageSource();
    
    _mockMoodDataService.Setup(s => s.GetMoodDataAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                        .ReturnsAsync(testMoodData);
    _mockLineGraphService.Setup(s => s.GenerateGraphAsync(It.IsAny<GraphData>()))
                         .ReturnsAsync(testGraphImage);
    
    // Act
    await _sut.UpdateGraphAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.GraphImageSource, Is.EqualTo(testGraphImage));
        Assert.That(_sut.HasGraphData, Is.True);
        Assert.That(_sut.HasNoData, Is.False);
        Assert.That(_sut.IsLoading, Is.False);
    });
}

[Test]
public async Task UpdateGraphAsync_WhenNoData_ShouldShowNoDataStateAndStatusMessage()
{
    // Arrange
    var emptyMoodData = new MoodCollection();
    
    _mockMoodDataService.Setup(s => s.GetMoodDataAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                        .ReturnsAsync(emptyMoodData);
    
    // Act
    await _sut.UpdateGraphAsync();
    
    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(_sut.HasGraphData, Is.False);
        Assert.That(_sut.HasNoData, Is.True);
        Assert.That(_sut.HasStatusMessage, Is.True);
        Assert.That(_sut.StatusMessage, Contains.Substring("no data"));
    });
}
```

## 5. MVVM-Specific Testing

### Property Change Notification Tests
Given the 25+ properties, create parameterized tests:

```csharp
[TestCase(nameof(GraphViewModel.IsLoading), true)]
[TestCase(nameof(GraphViewModel.HasGraphData), true)]
[TestCase(nameof(GraphViewModel.ShowDataPoints), false)]
[TestCase(nameof(GraphViewModel.ShowAxesAndGrid), false)]
public void Property_WhenChanged_ShouldRaisePropertyChangedEvent(string propertyName, object newValue)
{
    // Generic property change notification test
}
```

### Command Testing
Test all 6 commands for:
- **CanExecute Logic**: When commands should be enabled/disabled
- **Execute Logic**: Command behavior and side effects
- **Async Commands**: Proper async/await handling
- **Exception Handling**: Command error scenarios

### Data Binding Scenarios
- **Two-way Binding**: Properties that support two-way binding
- **Computed Properties**: GraphWidth, ExportGraphWidth calculations
- **Collection Binding**: DateRanges and GraphModes ObservableCollections
- **Image Binding**: GraphImageSource and CustomBackgroundSource

### Navigation Testing
**N/A** - GraphViewModel doesn't handle navigation directly

## 6. Coverage Goals

### Target Coverage
- **Line Coverage**: 85% minimum (complex async operations may have unreachable paths)
- **Branch Coverage**: 80% minimum (many conditional logic paths)
- **Method Coverage**: 95% (most public and key private methods tested)

### Priority Areas
1. **Constructor and Initialization** (Critical for DI)
2. **Property Change Notifications** (Critical for MVVM)
3. **UpdateGraphAsync Method** (Core business logic)
4. **Command Execution** (User interaction scenarios)
5. **Color Management Logic** (Complex RGB calculations)
6. **State Management** (Loading, data availability coordination)

### Acceptable Exclusions
- Exception handling catch blocks that only log or show messages
- Platform-specific error handling paths
- Generated property setter boilerplate
- Complex graphics rendering error paths in service calls

### Measurement Strategy
- **Incremental Coverage**: Measure after each test category implementation
- **Focus Areas**: Prioritize untested branches in core business logic
- **Integration Testing**: Verify service interactions don't create coverage gaps

## 7. Implementation Checklist

### Phase 1 - Testability ✅
- [x] **Object Analysis Complete**: GraphViewModel is complex but testable
- [x] **Dependencies Identified**: All dependencies injectable and mockable
- [x] **Architecture Assessment**: No refactoring required, some optimizations possible

### Phase 2 - Test Setup
- [ ] Create `GraphViewModelTests.cs` in `WorkMood.MauiApp.Tests/ViewModels/`
- [ ] Setup NUnit test framework with Moq and async test support
- [ ] **Implement LineGraphServiceSpy** to forward calls to real service while enabling verification
- [ ] Create nested test classes for organization (Constructor, Property, Command, etc.)
- [ ] Create comprehensive mock setup with default behaviors for IMoodDataService and IDateShim
- [ ] Build test data builders for MoodCollection, DateRangeItem, GraphModeItem
- [ ] Setup spy configuration for both pure unit tests (test doubles) and integration-like tests (real service)

### Phase 3 - Core Tests
- [ ] **Constructor Tests**: All dependency injection scenarios
- [ ] **Property Tests**: Core properties (SelectedDateRange, GraphImageSource, state flags)
- [ ] **Graph Update Tests**: UpdateGraphAsync with various data scenarios
- [ ] **Command Tests**: All 6 commands with success scenarios

### Phase 4 - Complex Integration Tests
- [ ] **Color Management**: RGB synchronization and color picker interactions
- [ ] **Custom Background**: File loading and background management
- [ ] **Auto-Update Logic**: Property changes triggering graph updates
- [ ] **State Coordination**: Loading states and data availability logic

### Phase 5 - Edge Cases and Error Handling
- [ ] **Service Failures**: Mock service exceptions and error handling
- [ ] **Invalid Data**: Empty collections, null values, invalid file paths
- [ ] **Async Scenarios**: Cancellation, concurrent operations, timeouts
- [ ] **Property Edge Cases**: Same value settings, invalid color values

### Phase 6 - Coverage Verification
- [ ] Run coverage analysis: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Filter coverage report for GraphViewModel: `--classfilters:*GraphViewModel*`
- [ ] Achieve 85%+ line coverage and 80%+ branch coverage
- [ ] Document acceptable exclusions and coverage gaps

### Phase 7 - Performance and Integration Testing
- [ ] **Performance Tests**: Verify property changes don't cause performance issues
- [ ] **Memory Tests**: Ensure no memory leaks from ImageSource objects
- [ ] **Integration Tests**: Full graph generation scenarios with real-ish data
- [ ] **Stress Tests**: Multiple rapid property changes and updates

### Phase 8 - Code Review and Optimization
- [ ] Verify test naming follows patterns and is maintainable
- [ ] Ensure proper mock verification and cleanup
- [ ] Validate test independence and proper async handling
- [ ] Review for test performance and maintainability

## 8. Arlo's Commit Strategy

### Planned Commits (Arlo's Commit Notation)
```bash
^f - add LineGraphServiceSpy implementation for realistic GraphViewModel testing
^f - add GraphViewModel test infrastructure with spy pattern and comprehensive mocks
^f - add GraphViewModel constructor tests for dependency injection validation
^f - add GraphViewModel core property tests with change notification verification
^f - add GraphViewModel command tests for user interaction scenarios
^f - add GraphViewModel UpdateGraphAsync tests for core business logic coverage
^f - add GraphViewModel color management tests for RGB synchronization logic
^f - add GraphViewModel custom background tests for file handling scenarios
^f - add GraphViewModel state management tests for loading and data coordination
^f - add GraphViewModel error handling tests to achieve 85% coverage target
^f - add GraphViewModel integration tests for full graph generation scenarios
```

### Commit Granularity
- **One major functionality area per commit** (constructor, properties, commands, etc.)
- **Manual verification** after each commit to ensure tests pass and coverage improves
- **Integration testing** after core functionality is covered
- **Performance verification** before marking object complete

---

**Success Criteria Met**: This test plan provides comprehensive guidance for testing GraphViewModel, the most complex object in WorkMood. The plan accounts for the high complexity while maintaining focus on critical business logic and MVVM compliance.

**Complexity Note**: GraphViewModel may benefit from being split into smaller, focused ViewModels in future refactoring, but the current architecture is testable and this plan addresses all scenarios.

**Next Steps**: Proceed with manual verification gate, then implement tests following the detailed 8-phase checklist above.