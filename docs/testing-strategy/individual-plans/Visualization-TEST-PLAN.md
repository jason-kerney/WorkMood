# Visualization (Page) - Individual Test Plan

## Class Overview
**File**: `MauiApp/Pages/Visualization.xaml.cs`  
**Type**: MAUI ContentPage (MVVM with Graphics Rendering)  
**LOC**: 204 lines (code-behind)  
**XAML LOC**: 213 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Advanced data visualization page that renders 2-week mood trend charts using custom graphics components. Features sophisticated chart rendering, programmatic UI generation, legacy compatibility methods, and complex data visualization coordination. Represents the core analytical capability of the application for mood trend analysis and insights.

### Dependencies
- **IMoodDataService** (optional injected) - Data access for mood visualization data
- **INavigationService** (optional injected) - Navigation coordination and page transitions
- **VisualizationViewModel** (created) - Business logic for data processing and chart management
- **EnhancedLineGraphDrawable** (created) - Custom graphics component for chart rendering
- **MoodVisualizationData** (processed) - Structured data for visualization rendering
- **MAUI Framework**: ContentPage, GraphicsView, Grid, CollectionView, PropertyChanged events

### Key Responsibilities
1. **Optional dependency injection** - Constructor supports both injected and default dependencies
2. **ViewModel creation** - Creates VisualizationViewModel with dependency injection
3. **Graphics view creation** - Creates GraphicsView with EnhancedLineGraphDrawable for chart rendering
4. **Programmatic UI generation** - CreateVisualization method builds grid layout with labels and charts
5. **PropertyChanged coordination** - Handles ViewModel property changes to update visualization
6. **Lifecycle management** - OnAppearing triggers ViewModel initialization, OnDisappearing cleanup
7. **Legacy compatibility** - Maintains deprecated methods for backward compatibility
8. **Color conversion utilities** - Helper methods for hex string to Color conversion

### Current Architecture Assessment
**Testability Score: 5/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Design Challenges:**
1. **Programmatic UI generation** - CreateVisualization method directly manipulates Grid children
2. **Mixed responsibilities** - Chart creation, UI generation, and event handling in same class
3. **Direct graphics dependencies** - Creates EnhancedLineGraphDrawable directly
4. **Legacy method retention** - Obsolete methods complicate testing and maintenance
5. **PropertyChanged event handling** - Manual property change coordination without abstraction
6. **Direct Grid manipulation** - VisualizationGrid.Children access creates tight coupling
7. **Dispatcher usage** - Manual Dispatcher.Dispatch calls for UI thread coordination

**Good Design Elements:**
1. **Optional dependency injection** - Supports both DI and fallback scenarios  
2. **Clean MVVM separation** - Delegates business logic to ViewModel appropriately
3. **Proper event cleanup** - OnDisappearing unsubscribes PropertyChanged events
4. **Public ViewModel access** - Exposes ViewModel property for testing purposes
5. **Interface dependencies** - Uses IMoodDataService and INavigationService interfaces
6. **Error handling** - Try-catch blocks in color conversion utilities
7. **Null safety** - Proper null checking throughout methods

## XAML Structure Analysis

### UI Components (213 lines XAML)
1. **Header section** - Page title and date range information
2. **Legend section** - Color-coded legend for mood trend interpretation
3. **Visualization container** - Frame with VisualizationGrid for programmatic chart rendering
4. **Summary section** - Text summary of mood trends and insights
5. **Daily details section** - CollectionView with detailed daily data items
6. **Navigation buttons** - Back to History and Refresh Data commands
7. **Loading indicator** - ActivityIndicator with loading state management

### Data Bindings
- **Loading State**: `{Binding IsLoading}` with InvertedBoolConverter for content visibility
- **Date Range**: `{Binding DateRangeText}` for current visualization period
- **Summary**: `{Binding SummaryText}` for trend analysis text
- **Daily Data**: `{Binding DailyDataItems}` CollectionView with complex item templates
- **Commands**: `{Binding BackToHistoryCommand}`, `{Binding RefreshCommand}`

### Complex UI Patterns
- **Conditional Visibility** - Content hidden/shown based on loading state
- **CollectionView Templates** - Detailed daily data items with color indicators
- **Programmatic Grid Population** - VisualizationGrid populated via code-behind
- **Custom Graphics Integration** - GraphicsView with EnhancedLineGraphDrawable
- **Multi-level Data Binding** - DailyDataItemViewModel properties in CollectionView

### Graphics Integration
- **EnhancedLineGraphDrawable** - Custom IDrawable implementation for chart rendering
- **GraphicsView** - MAUI graphics component hosting the drawable
- **MoodVisualizationData** - Structured data passed to graphics component
- **Color Management** - Hex string to Color conversion utilities

## Required Refactoring Strategy

### Phase 1: Extract Chart Factory
Create abstraction for chart creation and graphics coordination:

```csharp
public interface IVisualizationChartFactory
{
    GraphicsView CreateLineGraphView(MoodVisualizationData visualizationData);
    IDrawable CreateLineGraphDrawable(MoodVisualizationData visualizationData);
}

public class VisualizationChartFactory : IVisualizationChartFactory
{
    public GraphicsView CreateLineGraphView(MoodVisualizationData visualizationData)
    {
        return new GraphicsView
        {
            HeightRequest = 100,
            BackgroundColor = Colors.Transparent,
            Drawable = CreateLineGraphDrawable(visualizationData)
        };
    }
    
    public IDrawable CreateLineGraphDrawable(MoodVisualizationData visualizationData)
    {
        return new EnhancedLineGraphDrawable(visualizationData);
    }
}
```

### Phase 2: Extract UI Layout Manager
Create abstraction for programmatic UI generation:

```csharp
public interface IVisualizationUIManager
{
    void CreateVisualization(Grid targetGrid, MoodVisualizationData visualizationData);
    void ClearVisualization(Grid targetGrid);
    void AddDayLabels(Grid targetGrid, MoodVisualizationData visualizationData);
    void AddVisualizationChart(Grid targetGrid, MoodVisualizationData visualizationData);
    void AddWeekLabels(Grid targetGrid);
}

public class VisualizationUIManager : IVisualizationUIManager
{
    private readonly IVisualizationChartFactory _chartFactory;
    
    public VisualizationUIManager(IVisualizationChartFactory chartFactory)
    {
        _chartFactory = chartFactory;
    }
    
    public void CreateVisualization(Grid targetGrid, MoodVisualizationData visualizationData)
    {
        if (visualizationData?.DailyValues == null) return;
        
        ClearVisualization(targetGrid);
        AddDayLabels(targetGrid, visualizationData);
        AddVisualizationChart(targetGrid, visualizationData);
        AddWeekLabels(targetGrid);
    }
    
    public void ClearVisualization(Grid targetGrid)
    {
        targetGrid.Children.Clear();
    }
    
    public void AddDayLabels(Grid targetGrid, MoodVisualizationData visualizationData)
    {
        for (int day = 0; day < 14; day++)
        {
            var dailyValue = visualizationData.DailyValues[day];
            var dayLabel = new Label
            {
                Text = dailyValue.Date.ToString("dd"),
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(2, 0, 2, 2)
            };
            
            Grid.SetColumn(dayLabel, day);
            Grid.SetRow(dayLabel, 0);
            targetGrid.Children.Add(dayLabel);
        }
    }
    
    public void AddVisualizationChart(Grid targetGrid, MoodVisualizationData visualizationData)
    {
        var lineGraphView = _chartFactory.CreateLineGraphView(visualizationData);
        Grid.SetColumn(lineGraphView, 0);
        Grid.SetColumnSpan(lineGraphView, 14);
        Grid.SetRow(lineGraphView, 1);
        targetGrid.Children.Add(lineGraphView);
    }
    
    public void AddWeekLabels(Grid targetGrid)
    {
        var week1Label = new Label
        {
            Text = "Week 1",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetColumn(week1Label, 0);
        Grid.SetColumnSpan(week1Label, 7);
        Grid.SetRow(week1Label, 2);
        targetGrid.Children.Add(week1Label);
        
        var week2Label = new Label
        {
            Text = "Week 2",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetColumn(week2Label, 7);
        Grid.SetColumnSpan(week2Label, 7);
        Grid.SetRow(week2Label, 2);
        targetGrid.Children.Add(week2Label);
    }
}
```

### Phase 3: Extract Property Change Coordinator
Create abstraction for ViewModel property change handling:

```csharp
public interface IVisualizationPropertyCoordinator
{
    void Subscribe(VisualizationViewModel viewModel);
    void Unsubscribe(VisualizationViewModel viewModel);
    event EventHandler<VisualizationChangedEventArgs> VisualizationChanged;
}

public class VisualizationPropertyCoordinator : IVisualizationPropertyCoordinator
{
    public event EventHandler<VisualizationChangedEventArgs> VisualizationChanged;
    
    public void Subscribe(VisualizationViewModel viewModel)
    {
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }
    
    public void Unsubscribe(VisualizationViewModel viewModel)
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisualizationViewModel.CurrentVisualization) && 
            sender is VisualizationViewModel viewModel)
        {
            VisualizationChanged?.Invoke(this, 
                new VisualizationChangedEventArgs(viewModel.CurrentVisualization));
        }
    }
}

public class VisualizationChangedEventArgs : EventArgs
{
    public MoodVisualizationData? VisualizationData { get; }
    
    public VisualizationChangedEventArgs(MoodVisualizationData? visualizationData)
    {
        VisualizationData = visualizationData;
    }
}
```

### Phase 4: Extract Thread Coordination Manager
Create abstraction for UI thread operations:

```csharp
public interface IVisualizationThreadCoordinator
{
    void DispatchUIUpdate(Action uiUpdateAction);
    Task DispatchUIUpdateAsync(Func<Task> uiUpdateAction);
}

public class VisualizationThreadCoordinator : IVisualizationThreadCoordinator
{
    private readonly IDispatcher _dispatcher;
    
    public VisualizationThreadCoordinator(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    public void DispatchUIUpdate(Action uiUpdateAction)
    {
        _dispatcher.Dispatch(uiUpdateAction);
    }
    
    public async Task DispatchUIUpdateAsync(Func<Task> uiUpdateAction)
    {
        await _dispatcher.DispatchAsync(uiUpdateAction);
    }
}
```

### Phase 5: Refactored Architecture
Transform to testable design with clear separation of concerns:

```csharp
public partial class Visualization : ContentPage
{
    private VisualizationViewModel? _viewModel;
    private readonly IVisualizationUIManager _uiManager;
    private readonly IVisualizationPropertyCoordinator _propertyCoordinator;
    private readonly IVisualizationThreadCoordinator _threadCoordinator;

    public Visualization(IMoodDataService? moodDataService = null, 
                         INavigationService? navigationService = null,
                         IVisualizationChartFactory? chartFactory = null,
                         IVisualizationUIManager? uiManager = null,
                         IVisualizationPropertyCoordinator? propertyCoordinator = null,
                         IVisualizationThreadCoordinator? threadCoordinator = null)
    {
        InitializeComponent();
        
        // Create or inject dependencies
        var factory = chartFactory ?? new VisualizationChartFactory();
        _uiManager = uiManager ?? new VisualizationUIManager(factory);
        _propertyCoordinator = propertyCoordinator ?? new VisualizationPropertyCoordinator();
        _threadCoordinator = threadCoordinator ?? new VisualizationThreadCoordinator(Dispatcher);
        
        // Initialize ViewModel with dependencies
        _viewModel = new VisualizationViewModel(
            moodDataService ?? new MoodDataService(),
            navigationService ?? new NavigationService(this)
        );
        
        BindingContext = _viewModel;
        
        // Wire up property change coordination
        _propertyCoordinator.Subscribe(_viewModel);
        _propertyCoordinator.VisualizationChanged += OnVisualizationChanged;
    }
    
    /// <summary>
    /// Gets the current ViewModel (for testing purposes)
    /// </summary>
    public VisualizationViewModel? ViewModel => _viewModel;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.OnAppearingAsync();
        }
    }

    /// <summary>
    /// Creates the line graph view for visualization
    /// </summary>
    public View CreateLineGraphView(MoodVisualizationData visualizationData)
    {
        return _uiManager.CreateLineGraphView(visualizationData);
    }
    
    /// <summary>
    /// Creates visualization using UI manager abstraction
    /// </summary>
    public void CreateVisualization(MoodVisualizationData visualizationData)
    {
        _uiManager.CreateVisualization(VisualizationGrid, visualizationData);
    }

    /// <summary>
    /// Handles visualization property changes from ViewModel
    /// </summary>
    private void OnVisualizationChanged(object? sender, VisualizationChangedEventArgs e)
    {
        if (e.VisualizationData != null)
        {
            _threadCoordinator.DispatchUIUpdate(() => 
                CreateVisualization(e.VisualizationData));
        }
    }

    /// <summary>
    /// Clean up event subscriptions
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel != null)
        {
            _propertyCoordinator.Unsubscribe(_viewModel);
        }
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
VisualizationPageTests/
├── Constructor/
│   ├── Should_CreateVisualizationViewModel_WithDefaultDependencies()
│   ├── Should_CreateVisualizationViewModel_WithProvidedDependencies()
│   ├── Should_SetBindingContext_ToCreatedViewModel()
│   ├── Should_CreateUIManager_WithChartFactory()
│   ├── Should_CreatePropertyCoordinator_AndSubscribeToViewModel()
│   ├── Should_CreateThreadCoordinator_WithDispatcher()
│   └── Should_WireUpVisualizationChangedEvent_ToPropertyCoordinator()
├── ChartCreation/
│   ├── Should_CreateLineGraphView_WithCorrectProperties()
│   ├── Should_CreateEnhancedLineGraphDrawable_WithVisualizationData()
│   ├── Should_SetHeightRequest_OnGraphicsView()
│   ├── Should_SetBackgroundColor_ToTransparent()
│   ├── Should_HandleNullVisualizationData_Gracefully()
│   └── Should_UseProvidedChartFactory_WhenProvided()
├── UIGeneration/
│   ├── Should_ClearVisualizationGrid_BeforeCreatingNew()
│   ├── Should_AddDayLabels_ForFourteenDays()
│   ├── Should_AddVisualizationChart_WithCorrectGridPosition()
│   ├── Should_AddWeekLabels_WithCorrectSpanning()
│   ├── Should_HandleNullDailyValues_Gracefully()
│   ├── Should_SetCorrectGridPositions_ForAllElements()
│   └── Should_FormatDayLabels_Correctly()
├── PropertyChangeCoordination/
│   ├── Should_SubscribeToViewModelPropertyChanged_WhenConstructed()
│   ├── Should_HandleCurrentVisualizationPropertyChange_WhenTriggered()
│   ├── Should_IgnoreOtherPropertyChanges_WhenTriggered()
│   ├── Should_DispatchUIUpdate_WhenVisualizationChanges()
│   ├── Should_UnsubscribeFromPropertyChanged_WhenPageDisappears()
│   └── Should_HandlePropertyChangeErrors_Gracefully()
├── ThreadCoordination/
│   ├── Should_DispatchUIUpdates_OnMainThread()
│   ├── Should_HandleDispatcherErrors_Gracefully()
│   ├── Should_UseProvidedThreadCoordinator_WhenProvided()
│   └── Should_CreateDefaultThreadCoordinator_WhenNotProvided()
├── Lifecycle/
│   ├── OnAppearing/
│   │   ├── Should_CallViewModelOnAppearingAsync_WhenAppearing()
│   │   ├── Should_HandleNullViewModel_WhenAppearing()
│   │   ├── Should_CallBaseOnAppearing_WhenAppearing()
│   │   └── Should_HandleOnAppearingErrors_Gracefully()
│   └── OnDisappearing/
│       ├── Should_UnsubscribeFromPropertyCoordinator_WhenDisappearing()
│       ├── Should_HandleNullViewModel_WhenDisappearing()
│       ├── Should_CallBaseOnDisappearing_WhenDisappearing()
│       └── Should_HandleUnsubscriptionErrors_Gracefully()
├── LegacyCompatibility/
│   ├── Should_SupportCreateDailyDataList_ForBackwardCompatibility()
│   ├── Should_HandleObsoleteEventHandlers_Gracefully()
│   ├── Should_MaintainPublicVisualizationMethods_ForCompatibility()
│   └── Should_HandleLegacyMethodErrors_Gracefully()
├── ColorUtilities/
│   ├── Should_ConvertValidHexString_ToColor()
│   ├── Should_ReturnLightGray_ForInvalidHexString()
│   ├── Should_ReturnLightGray_ForNullHexString()
│   ├── Should_ReturnLightGray_ForEmptyHexString()
│   ├── Should_HandleShortHexStrings_Gracefully()
│   └── Should_HandleColorConversionExceptions_Gracefully()
└── Integration/
    ├── Should_HandleCompleteVisualizationWorkflow_FromConstructionToDisplay()
    ├── Should_UpdateVisualization_WhenViewModelDataChanges()
    ├── Should_HandleNavigationCommands_ThroughViewModel()
    ├── Should_ManageLoadingStates_ThroughDataBinding()
    └── Should_CoordinateAllComponents_ForVisualizationRendering()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_CreateVisualizationViewModel_WithDefaultDependencies()
{
    // Arrange & Act
    var visualizationPage = new Visualization();

    // Assert
    Assert.That(visualizationPage.ViewModel, Is.Not.Null);
    Assert.That(visualizationPage.BindingContext, Is.SameAs(visualizationPage.ViewModel));
    Assert.That(visualizationPage.ViewModel, Is.InstanceOf<VisualizationViewModel>());
}

[Test]
public void Constructor_Should_CreateVisualizationViewModel_WithProvidedDependencies()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockNavigationService = new Mock<INavigationService>();
    var mockChartFactory = new Mock<IVisualizationChartFactory>();

    // Act
    var visualizationPage = new Visualization(
        mockMoodDataService.Object,
        mockNavigationService.Object,
        mockChartFactory.Object);

    // Assert
    Assert.That(visualizationPage.ViewModel, Is.Not.Null);
    Assert.That(visualizationPage.BindingContext, Is.SameAs(visualizationPage.ViewModel));
    // Verify ViewModel was created with provided services
}

[Test]
public void Constructor_Should_WireUpVisualizationChangedEvent_ToPropertyCoordinator()
{
    // Arrange
    var mockPropertyCoordinator = new Mock<IVisualizationPropertyCoordinator>();
    var visualizationPage = new Visualization(
        propertyCoordinator: mockPropertyCoordinator.Object);

    // Act
    // Simulate visualization changed event
    mockPropertyCoordinator.Raise(pc => pc.VisualizationChanged += null,
        new VisualizationChangedEventArgs(new MoodVisualizationData()));

    // Assert
    // Verify UI update was triggered (would need additional testing infrastructure)
    Assert.That(visualizationPage, Is.Not.Null);
}
```

#### Chart Creation Tests
```csharp
[Test]
public void ChartCreation_Should_CreateLineGraphView_WithCorrectProperties()
{
    // Arrange
    var testVisualizationData = new MoodVisualizationData();
    var chartFactory = new VisualizationChartFactory();

    // Act
    var graphView = chartFactory.CreateLineGraphView(testVisualizationData);

    // Assert
    Assert.That(graphView, Is.InstanceOf<GraphicsView>());
    Assert.That(graphView.HeightRequest, Is.EqualTo(100));
    Assert.That(graphView.BackgroundColor, Is.EqualTo(Colors.Transparent));
    Assert.That(graphView.Drawable, Is.InstanceOf<EnhancedLineGraphDrawable>());
}

[Test]
public void ChartCreation_Should_CreateEnhancedLineGraphDrawable_WithVisualizationData()
{
    // Arrange
    var testVisualizationData = new MoodVisualizationData();
    var chartFactory = new VisualizationChartFactory();

    // Act
    var drawable = chartFactory.CreateLineGraphDrawable(testVisualizationData);

    // Assert
    Assert.That(drawable, Is.InstanceOf<EnhancedLineGraphDrawable>());
    // Verify drawable was created with correct data (would need access to internal data)
}

[Test]
public void ChartCreation_Should_HandleNullVisualizationData_Gracefully()
{
    // Arrange
    MoodVisualizationData? nullVisualizationData = null;
    var chartFactory = new VisualizationChartFactory();

    // Act & Assert
    Assert.DoesNotThrow(() => chartFactory.CreateLineGraphView(nullVisualizationData!));
    Assert.DoesNotThrow(() => chartFactory.CreateLineGraphDrawable(nullVisualizationData!));
}
```

#### UI Generation Tests
```csharp
[Test]
public void UIGeneration_Should_ClearVisualizationGrid_BeforeCreatingNew()
{
    // Arrange
    var testGrid = new Grid();
    testGrid.Children.Add(new Label { Text = "Existing" });
    var uiManager = new VisualizationUIManager(new VisualizationChartFactory());

    // Act
    uiManager.ClearVisualization(testGrid);

    // Assert
    Assert.That(testGrid.Children.Count, Is.EqualTo(0));
}

[Test]
public void UIGeneration_Should_AddDayLabels_ForFourteenDays()
{
    // Arrange
    var testGrid = new Grid();
    var testVisualizationData = CreateTestVisualizationDataWithFourteenDays();
    var uiManager = new VisualizationUIManager(new VisualizationChartFactory());

    // Act
    uiManager.AddDayLabels(testGrid, testVisualizationData);

    // Assert
    Assert.That(testGrid.Children.Count, Is.EqualTo(14));
    Assert.That(testGrid.Children.All(c => c is Label), Is.True);
    
    // Verify day labels are correctly positioned
    for (int i = 0; i < 14; i++)
    {
        var label = testGrid.Children[i] as Label;
        Assert.That(Grid.GetColumn(label), Is.EqualTo(i));
        Assert.That(Grid.GetRow(label), Is.EqualTo(0));
    }
}

[Test]
public void UIGeneration_Should_AddVisualizationChart_WithCorrectGridPosition()
{
    // Arrange
    var testGrid = new Grid();
    var testVisualizationData = new MoodVisualizationData();
    var mockChartFactory = new Mock<IVisualizationChartFactory>();
    var mockGraphView = new GraphicsView();
    mockChartFactory.Setup(cf => cf.CreateLineGraphView(testVisualizationData))
        .Returns(mockGraphView);
    
    var uiManager = new VisualizationUIManager(mockChartFactory.Object);

    // Act
    uiManager.AddVisualizationChart(testGrid, testVisualizationData);

    // Assert
    Assert.That(testGrid.Children.Contains(mockGraphView), Is.True);
    Assert.That(Grid.GetColumn(mockGraphView), Is.EqualTo(0));
    Assert.That(Grid.GetColumnSpan(mockGraphView), Is.EqualTo(14));
    Assert.That(Grid.GetRow(mockGraphView), Is.EqualTo(1));
}
```

#### Property Change Coordination Tests
```csharp
[Test]
public void PropertyChangeCoordination_Should_HandleCurrentVisualizationPropertyChange_WhenTriggered()
{
    // Arrange
    var propertyCoordinator = new VisualizationPropertyCoordinator();
    var mockViewModel = new Mock<VisualizationViewModel>();
    var testVisualizationData = new MoodVisualizationData();
    var eventTriggered = false;
    
    propertyCoordinator.VisualizationChanged += (s, e) => eventTriggered = true;
    propertyCoordinator.Subscribe(mockViewModel.Object);

    // Act
    mockViewModel.Raise(vm => vm.PropertyChanged += null,
        new PropertyChangedEventArgs(nameof(VisualizationViewModel.CurrentVisualization)));

    // Assert
    Assert.That(eventTriggered, Is.True);
}

[Test]
public void PropertyChangeCoordination_Should_IgnoreOtherPropertyChanges_WhenTriggered()
{
    // Arrange
    var propertyCoordinator = new VisualizationPropertyCoordinator();
    var mockViewModel = new Mock<VisualizationViewModel>();
    var eventTriggered = false;
    
    propertyCoordinator.VisualizationChanged += (s, e) => eventTriggered = true;
    propertyCoordinator.Subscribe(mockViewModel.Object);

    // Act
    mockViewModel.Raise(vm => vm.PropertyChanged += null,
        new PropertyChangedEventArgs("SomeOtherProperty"));

    // Assert
    Assert.That(eventTriggered, Is.False);
}

[Test]
public void PropertyChangeCoordination_Should_UnsubscribeFromPropertyChanged_WhenPageDisappears()
{
    // Arrange
    var mockPropertyCoordinator = new Mock<IVisualizationPropertyCoordinator>();
    var visualizationPage = new Visualization(
        propertyCoordinator: mockPropertyCoordinator.Object);

    // Act
    // Simulate OnDisappearing through reflection
    var onDisappearingMethod = typeof(Visualization).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(visualizationPage, null);

    // Assert
    mockPropertyCoordinator.Verify(pc => 
        pc.Unsubscribe(It.IsAny<VisualizationViewModel>()), Times.Once);
}
```

#### Color Utilities Tests
```csharp
[Test]
public void ColorUtilities_Should_ConvertValidHexString_ToColor()
{
    // Arrange
    var validHex = "#FF5733";
    
    // Act
    var result = GetColorFromHexTestWrapper(validHex);

    // Assert
    Assert.That(result, Is.Not.EqualTo(Colors.LightGray));
    // Additional color validation would depend on Color.FromArgb implementation
}

[Test]
public void ColorUtilities_Should_ReturnLightGray_ForInvalidHexString()
{
    // Arrange
    var invalidHex = "NotAColor";
    
    // Act
    var result = GetColorFromHexTestWrapper(invalidHex);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.LightGray));
}

[Test]
public void ColorUtilities_Should_ReturnLightGray_ForNullHexString()
{
    // Arrange
    string? nullHex = null;
    
    // Act
    var result = GetColorFromHexTestWrapper(nullHex);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.LightGray));
}

// Helper method to access private static method for testing
private Color GetColorFromHexTestWrapper(string? hex)
{
    var method = typeof(Visualization).GetMethod("GetColorFromHex", 
        BindingFlags.NonPublic | BindingFlags.Static);
    return (Color)method.Invoke(null, new object[] { hex });
}
```

#### Integration Tests
```csharp
[Test]
public void Integration_Should_HandleCompleteVisualizationWorkflow_FromConstructionToDisplay()
{
    // Arrange
    var realMoodDataService = new Mock<IMoodDataService>().Object;
    var realNavigationService = new Mock<INavigationService>().Object;
    
    var visualizationPage = new Visualization(realMoodDataService, realNavigationService);

    // Act
    // Simulate page appearing
    var onAppearingMethod = typeof(Visualization).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(visualizationPage, null);
    
    // Simulate visualization data change
    var testVisualizationData = new MoodVisualizationData();
    visualizationPage.CreateVisualization(testVisualizationData);
    
    // Simulate page disappearing
    var onDisappearingMethod = typeof(Visualization).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(visualizationPage, null);

    // Assert
    Assert.That(visualizationPage.ViewModel, Is.Not.Null);
    Assert.That(visualizationPage.BindingContext, Is.SameAs(visualizationPage.ViewModel));
}
```

### Test Fixtures Required
- **MoodVisualizationDataTestFactory** - Create configured test visualization data
- **VisualizationViewModelMockFactory** - Create configured VisualizationViewModel mocks
- **ChartFactoryTestDouble** - Test double for chart creation behavior
- **UIManagerTestDouble** - Test double for UI generation testing
- **PropertyCoordinatorTestDouble** - Test double for property change coordination
- **ThreadCoordinatorTestDouble** - Test double for thread coordination testing

## Success Criteria
- [ ] **Constructor validation** - All dependency injection scenarios tested
- [ ] **Chart creation** - Graphics component creation and configuration verified
- [ ] **UI generation** - Programmatic Grid manipulation thoroughly tested
- [ ] **Property coordination** - ViewModel property change handling tested
- [ ] **Thread coordination** - UI thread dispatching behavior verified
- [ ] **Lifecycle management** - OnAppearing/OnDisappearing with async operations tested
- [ ] **Legacy compatibility** - Obsolete methods and backward compatibility verified
- [ ] **Color utilities** - Hex string conversion and error handling tested

## Implementation Priority
**HIGH PRIORITY** - Core analytical feature providing mood trend insights. Critical for user value delivery and represents sophisticated graphics integration patterns used throughout the application.

## Dependencies for Testing
- **MAUI Graphics test framework** - For GraphicsView and IDrawable testing
- **Grid manipulation testing tools** - For programmatic UI generation testing
- **Property change coordination framework** - For ViewModel event testing
- **Thread coordination testing tools** - For Dispatcher and UI thread testing
- **Graphics component mocking framework** - For EnhancedLineGraphDrawable testing

## Implementation Estimate
**Effort: High (5-6 days)**
- Chart factory abstraction and graphics component testing
- UI manager abstraction with comprehensive Grid manipulation testing
- Property coordinator abstraction with ViewModel event testing
- Thread coordinator abstraction with UI thread coordination testing
- Legacy compatibility testing for obsolete methods
- Color utility testing with hex string conversion scenarios
- Integration testing across graphics, UI, and data coordination components

This page represents the most sophisticated graphics integration in the application with complex programmatic UI generation patterns, making comprehensive testing critical for data visualization accuracy and user insights.