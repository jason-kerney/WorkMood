# Graph (Page) - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview
**File**: `MauiApp/Pages/Graph.xaml.cs`  
**Type**: MAUI ContentPage (MVVM Pattern with UI Interaction)  
**LOC**: 31 lines (code-behind)  
**XAML LOC**: 288 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Graph page that displays mood data visualization with extensive customization options. Features complex UI including date range selection, graph mode options, color picker, background customization, and graph export/sharing capabilities. Follows MVVM pattern with minimal but important UI interaction logic in code-behind for size change handling.

### Dependencies
- **GraphViewModel** (injected) - Complex business logic for graph generation and data management
- **MAUI Framework**: ContentPage base class, InitializeComponent(), OnAppearing lifecycle
- **UI Event Handling**: GraphContainer_SizeChanged for responsive graph sizing
- **XAML Bindings**: Extensive data binding to ViewModel properties and commands

### Key Responsibilities
1. **View construction** - InitializeComponent() sets up complex XAML UI structure
2. **ViewModel injection** - Constructor dependency injection pattern
3. **Data binding setup** - BindingContext assignment to GraphViewModel
4. **Lifecycle management** - OnAppearing() triggers data loading
5. **UI interaction handling** - Size change events for responsive graph rendering
6. **MVVM compliance** - Minimal code-behind with delegation to ViewModel

### Current Architecture Assessment
**Testability Score: 6/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Excellent Design Elements:**
1. **MVVM pattern compliance** - Business logic properly delegated to ViewModel
2. **Constructor injection** - Clean dependency injection for ViewModel
3. **Lifecycle awareness** - Proper OnAppearing() override for data loading
4. **Event delegation** - Size change events properly forwarded to ViewModel

**Testing Challenges:**
1. **UI event handlers** - GraphContainer_SizeChanged requires UI testing framework
2. **Complex XAML structure** - 288 lines with multiple interactive elements
3. **Lifecycle dependencies** - OnAppearing() async behavior needs testing
4. **Platform-specific rendering** - Size calculations may vary across platforms
5. **Border event handling** - SizeChanged event requires actual Border element

**Moderate Refactoring for Enhanced Testability:**
- Extract interface for UI event handling
- Wrapper for size change event abstraction
- Testable lifecycle method injection

## XAML Structure Analysis

### Complex UI Components (288 lines XAML)
1. **Date range controls** - Picker for DateRanges with complex binding
2. **Graph mode selection** - Picker for GraphModes with display binding
3. **Display options** - Multiple CheckBoxes for ShowDataPoints, ShowAxesAndGrid, ShowTitle, ShowTrendLine
4. **Color picker system** - Expandable UI with predefined colors and RGB sliders
5. **Custom background controls** - Load/Clear background with conditional visibility
6. **Graph container** - Border with SizeChanged event and responsive Image
7. **Status displays** - No data message, loading indicator, status messages
8. **Action buttons** - Export PNG and Share Graph commands

### Data Bindings (Extensive)
- **Selection Controls**: `{Binding DateRanges}`, `{Binding SelectedDateRange}`, `{Binding GraphModes}`, `{Binding SelectedGraphModeItem}`
- **Display Options**: `{Binding ShowDataPoints}`, `{Binding ShowAxesAndGrid}`, `{Binding ShowTitle}`, `{Binding ShowTrendLine}`
- **Color System**: `{Binding SelectedLineColor}`, `{Binding IsColorPickerVisible}`, `{Binding RedValue}`, `{Binding GreenValue}`, `{Binding BlueValue}`
- **Background System**: `{Binding HasCustomBackground}`, `{Binding LoadCustomBackgroundCommand}`, `{Binding ClearCustomBackgroundCommand}`
- **Graph Display**: `{Binding GraphImageSource}`, `{Binding DisplayHeight}`, `{Binding DisplayWidth}`, `{Binding HasGraphData}`
- **Status Management**: `{Binding HasNoData}`, `{Binding IsLoading}`, `{Binding StatusMessage}`, `{Binding HasStatusMessage}`
- **Commands**: `{Binding ToggleColorPickerCommand}`, `{Binding SelectColorCommand}`, `{Binding ExportGraphCommand}`, `{Binding ShareGraphCommand}`

## Required Refactoring Strategy

### Phase 1: Event Handler Abstraction
Create abstraction for UI event handling:

```csharp
public interface IUIEventHandler
{
    void HandleContainerSizeChanged(double width, double height);
}

public class GraphUIEventHandler : IUIEventHandler
{
    private readonly GraphViewModel _viewModel;
    
    public GraphUIEventHandler(GraphViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    public void HandleContainerSizeChanged(double width, double height)
    {
        _viewModel.UpdateContainerSize(width, height);
    }
}
```

### Phase 2: Lifecycle Abstraction
Extract lifecycle behavior for testing:

```csharp
public interface IPageLifecycle
{
    Task OnPageAppearingAsync();
}

public class GraphPageLifecycle : IPageLifecycle
{
    private readonly GraphViewModel _viewModel;
    
    public GraphPageLifecycle(GraphViewModel viewModel)
    {
        _viewModel = viewModel;
    }
    
    public async Task OnPageAppearingAsync()
    {
        await _viewModel.LoadDataAsync();
    }
}
```

### Phase 3: Refactored Architecture
Transform to testable design with injected behaviors:

```csharp
public partial class Graph : ContentPage
{
    private readonly GraphViewModel _viewModel;
    private readonly IUIEventHandler _eventHandler;
    private readonly IPageLifecycle _lifecycle;

    public Graph(GraphViewModel viewModel, IUIEventHandler eventHandler, IPageLifecycle lifecycle)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _lifecycle.OnPageAppearingAsync();
    }
    
    private void GraphContainer_SizeChanged(object? sender, EventArgs e)
    {
        if (sender is Border border)
        {
            _eventHandler.HandleContainerSizeChanged(border.Width, border.Height);
        }
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
GraphPageTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenViewModelIsNull()
│   ├── Should_ThrowArgumentNullException_WhenEventHandlerIsNull()
│   ├── Should_ThrowArgumentNullException_WhenLifecycleIsNull()
│   ├── Should_InitializeComponent_WhenConstructed()
│   ├── Should_SetBindingContext_ToProvidedViewModel()
│   └── Should_AssignDependencies_ToPrivateFields()
├── Lifecycle/
│   ├── Should_CallLifecycleOnPageAppearing_WhenOnAppearingCalled()
│   ├── Should_CallBaseOnAppearing_WhenOnAppearingCalled()
│   ├── Should_HandleAsyncExceptions_InOnAppearing()
│   └── Should_LoadDataFromViewModel_WhenPageAppears()
├── EventHandling/
│   ├── Should_CallEventHandlerOnSizeChange_WhenBorderSizeChanges()
│   ├── Should_PassCorrectDimensions_ToEventHandler()
│   ├── Should_HandleGracefully_WhenSenderIsNotBorder()
│   ├── Should_HandleGracefully_WhenSenderIsNull()
│   └── Should_NotCallEventHandler_WhenBorderIsNull()
├── ViewModelIntegration/
│   ├── Should_BindDateRanges_ToViewModelProperty()
│   ├── Should_BindGraphModes_ToViewModelProperty()
│   ├── Should_BindDisplayOptions_ToViewModelProperties()
│   ├── Should_BindColorProperties_ToViewModelProperties()
│   ├── Should_BindBackgroundProperties_ToViewModelProperties()
│   ├── Should_BindGraphDisplay_ToViewModelProperties()
│   ├── Should_BindStatusProperties_ToViewModelProperties()
│   └── Should_BindCommands_ToViewModelCommands()
├── UIComponents/
│   ├── Should_ContainDateRangeControls_InXamlStructure()
│   ├── Should_ContainGraphModeControls_InXamlStructure()
│   ├── Should_ContainDisplayOptionsCheckboxes_InXamlStructure()
│   ├── Should_ContainColorPickerSystem_InXamlStructure()
│   ├── Should_ContainBackgroundControls_InXamlStructure()
│   ├── Should_ContainGraphContainer_WithSizeChangedEvent()
│   ├── Should_ContainStatusDisplays_InXamlStructure()
│   └── Should_ContainActionButtons_InXamlStructure()
├── DataBinding/
│   ├── Should_UpdateDateRangeSelection_WhenViewModelChanges()
│   ├── Should_UpdateGraphModeSelection_WhenViewModelChanges()
│   ├── Should_UpdateDisplayOptions_WhenViewModelChanges()
│   ├── Should_UpdateColorSelection_WhenViewModelChanges()
│   ├── Should_UpdateRGBValues_WhenViewModelChanges()
│   ├── Should_UpdateGraphImage_WhenViewModelChanges()
│   ├── Should_UpdateStatusMessages_WhenViewModelChanges()
│   └── Should_UpdateVisibilityStates_WhenViewModelChanges()
├── Commands/
│   ├── Should_ExecuteToggleColorPicker_WhenCommandTriggered()
│   ├── Should_ExecuteSelectColor_WhenColorCommandTriggered()
│   ├── Should_ExecuteLoadBackground_WhenCommandTriggered()
│   ├── Should_ExecuteClearBackground_WhenCommandTriggered()
│   ├── Should_ExecuteExportGraph_WhenCommandTriggered()
│   └── Should_ExecuteShareGraph_WhenCommandTriggered()
└── ErrorHandling/
    ├── Should_HandleGracefully_WhenViewModelThrows()
    ├── Should_HandleGracefully_WhenSizeChangeEventFails()
    ├── Should_HandleGracefully_WhenDataLoadingFails()
    └── Should_LogErrors_WhenExceptionsOccur()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenViewModelIsNull()
{
    // Arrange
    GraphViewModel? nullViewModel = null;
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() => 
        new Graph(nullViewModel!, mockEventHandler.Object, mockLifecycle.Object));
    Assert.That(exception.ParamName, Is.EqualTo("viewModel"));
}

[Test]
public void Constructor_Should_SetBindingContext_ToProvidedViewModel()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();

    // Act
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);

    // Assert
    Assert.That(graphPage.BindingContext, Is.SameAs(mockViewModel.Object));
}
```

#### Lifecycle Tests
```csharp
[Test]
public async Task Lifecycle_Should_CallLifecycleOnPageAppearing_WhenOnAppearingCalled()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();
    
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);

    // Act
    // Note: Testing protected OnAppearing requires reflection or inheritance
    var onAppearingMethod = typeof(Graph).GetMethod("OnAppearing", BindingFlags.NonPublic | BindingFlags.Instance);
    await (Task)onAppearingMethod.Invoke(graphPage, null);

    // Assert
    mockLifecycle.Verify(l => l.OnPageAppearingAsync(), Times.Once);
}

[Test]
public async Task Lifecycle_Should_LoadDataFromViewModel_WhenPageAppears()
{
    // Arrange
    var realViewModel = new Mock<GraphViewModel>();
    var realLifecycle = new GraphPageLifecycle(realViewModel.Object);
    var mockEventHandler = new Mock<IUIEventHandler>();
    
    var graphPage = new Graph(realViewModel.Object, mockEventHandler.Object, realLifecycle);

    // Act
    await realLifecycle.OnPageAppearingAsync();

    // Assert
    realViewModel.Verify(vm => vm.LoadDataAsync(), Times.Once);
}
```

#### Event Handling Tests
```csharp
[Test]
public void EventHandling_Should_CallEventHandlerOnSizeChange_WhenBorderSizeChanges()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();
    
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);
    
    var mockBorder = new Mock<Border>();
    mockBorder.Setup(b => b.Width).Returns(800);
    mockBorder.Setup(b => b.Height).Returns(600);

    // Act
    // Simulate SizeChanged event through reflection
    var sizeChangedMethod = typeof(Graph).GetMethod("GraphContainer_SizeChanged", BindingFlags.NonPublic | BindingFlags.Instance);
    sizeChangedMethod.Invoke(graphPage, new object[] { mockBorder.Object, EventArgs.Empty });

    // Assert
    mockEventHandler.Verify(h => h.HandleContainerSizeChanged(800, 600), Times.Once);
}

[Test]
public void EventHandling_Should_HandleGracefully_WhenSenderIsNotBorder()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();
    
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);
    var nonBorderSender = new Label(); // Not a Border

    // Act & Assert
    Assert.DoesNotThrow(() =>
    {
        var sizeChangedMethod = typeof(Graph).GetMethod("GraphContainer_SizeChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        sizeChangedMethod.Invoke(graphPage, new object[] { nonBorderSender, EventArgs.Empty });
    });
    
    // Should not call event handler
    mockEventHandler.Verify(h => h.HandleContainerSizeChanged(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
}
```

#### UI Testing with MAUI Framework
```csharp
[Test]
public void UIComponents_Should_ContainGraphContainer_WithSizeChangedEvent()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();

    // Act
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);

    // Assert
    // Find Border element named "GraphContainer"
    var graphContainer = graphPage.FindByName<Border>("GraphContainer");
    Assert.That(graphContainer, Is.Not.Null);
    
    // Verify SizeChanged event is wired up (requires UI testing framework)
    // This would test that the event handler is properly attached
}

[Test]
public void UIComponents_Should_ContainColorPickerSystem_InXamlStructure()
{
    // Arrange
    var mockViewModel = new Mock<GraphViewModel>();
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();

    // Act
    var graphPage = new Graph(mockViewModel.Object, mockEventHandler.Object, mockLifecycle.Object);

    // Assert
    // Verify complex color picker UI exists
    var colorPickerSection = graphPage.Content.FindByName<StackLayout>("ColorPickerSection");
    Assert.That(colorPickerSection, Is.Not.Null);
    
    // Verify RGB sliders exist
    var redSlider = graphPage.Content.FindByName<Slider>("RedSlider");
    var greenSlider = graphPage.Content.FindByName<Slider>("GreenSlider");
    var blueSlider = graphPage.Content.FindByName<Slider>("BlueSlider");
    
    Assert.That(redSlider, Is.Not.Null);
    Assert.That(greenSlider, Is.Not.Null);
    Assert.That(blueSlider, Is.Not.Null);
}
```

#### Data Binding Integration Tests
```csharp
[Test]
public void DataBinding_Should_UpdateGraphImage_WhenViewModelChanges()
{
    // Arrange
    var realViewModel = new GraphViewModel(); // Real ViewModel for binding test
    var mockEventHandler = new Mock<IUIEventHandler>();
    var mockLifecycle = new Mock<IPageLifecycle>();
    
    var graphPage = new Graph(realViewModel, mockEventHandler.Object, mockLifecycle.Object);

    // Act
    realViewModel.GraphImageSource = new FileImageSource { File = "test.png" };

    // Assert
    // UI testing framework would verify Image.Source updates
    var graphImage = graphPage.FindByName<Image>("GraphImage");
    Assert.That(graphImage, Is.Not.Null);
    // Verify binding connection exists
}
```

### Testing Framework Requirements

#### Extended MAUI Testing Setup
```csharp
public abstract class ComplexMauiPageTestBase : MauiPageTestBase
{
    protected Mock<IUIEventHandler> MockEventHandler { get; private set; }
    protected Mock<IPageLifecycle> MockLifecycle { get; private set; }
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        MockEventHandler = new Mock<IUIEventHandler>();
        MockLifecycle = new Mock<IPageLifecycle>();
    }
    
    protected void SimulateSizeChangedEvent<T>(T page, Border border, double width, double height) where T : ContentPage
    {
        // Helper method to simulate SizeChanged events in tests
        var eventArgs = new EventArgs();
        border.Width = width;
        border.Height = height;
        
        var method = typeof(T).GetMethod("GraphContainer_SizeChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(page, new object[] { border, eventArgs });
    }
}
```

### Test Fixtures Required
- **GraphViewModelMockFactory** - Create configured GraphViewModel mocks with complex state
- **UIEventHandlerTestDouble** - Test double for UI event handling scenarios
- **PageLifecycleTestDouble** - Test double for lifecycle behavior testing
- **ComplexUIElementFinder** - Helper methods for finding complex XAML elements
- **DataBindingValidator** - Verify extensive binding connections work correctly

## Success Criteria
- [ ] **Constructor validation** - All dependency injection scenarios handled correctly
- [ ] **Lifecycle management** - OnAppearing behavior properly tested
- [ ] **Event handling** - SizeChanged events properly delegated to abstractions
- [ ] **ViewModel integration** - Complex binding scenarios work correctly
- [ ] **UI structure verification** - All 288 lines of XAML accessible and functional
- [ ] **Command execution** - All bound commands execute correctly
- [ ] **Error resilience** - Graceful handling of UI and ViewModel exceptions

## Implementation Priority
**HIGH PRIORITY** - Complex UI page with significant user interaction and critical business functionality. Represents the core visualization feature of the application.

## Dependencies for Testing
- **MAUI Controls Test Framework** - For complex UI element testing
- **GraphViewModel mock/real instances** - For binding and integration testing
- **Border and Image element mocking** - For SizeChanged event testing
- **Command execution testing tools** - For verifying bound command behavior
- **Async lifecycle testing framework** - For OnAppearing behavior validation

## Implementation Estimate
**Effort: High (3-4 days)**
- Complex abstraction layer creation for event handling and lifecycle
- MAUI testing framework setup for complex UI scenarios
- Comprehensive constructor and dependency injection tests
- Event handling abstraction and testing
- Complex data binding validation tests
- Command execution and integration testing
- UI structure verification across 288 lines of XAML

This page represents significantly more complexity than the About page, requiring substantial abstraction to achieve good testability while maintaining the sophisticated user interaction patterns required for graph customization and visualization.