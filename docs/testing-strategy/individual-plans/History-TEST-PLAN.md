# History (Page) - Individual Test Plan

## Class Overview
**File**: `MauiApp/Pages/History.xaml.cs`  
**Type**: MAUI ContentPage (MVVM with Complex UI Logic)  
**LOC**: 178 lines (code-behind)  
**XAML LOC**: 111 lines  
**Current Coverage**: 0% (estimated)

### Purpose
History page that displays mood statistics, trends, and recent check-ins with programmatic UI generation. Features complex constructor overloading for dependency injection compatibility, property change event handling, factory-based UI creation, and lifecycle management. Goes beyond simple MVVM to include significant UI manipulation logic.

### Dependencies
- **HistoryViewModel** (injected) - Business logic for data management and statistics
- **IMoodEntryViewFactory** (injected) - Factory for creating mood entry UI views
- **INavigationService** (created/injected) - Navigation handling
- **ILoggingService** (optional/injected) - Debug and operational logging
- **MAUI Framework**: ContentPage, PropertyChanged events, MainThread operations
- **Complex Lifecycle**: OnAppearing, OnDisappearing with async data loading

### Key Responsibilities
1. **Dual constructor support** - Primary DI constructor and backwards compatibility constructor
2. **Event subscription management** - PropertyChanged event handling and cleanup
3. **Programmatic UI generation** - Factory-based creation of mood entry views
4. **Lifecycle coordination** - OnAppearing data loading and UI updates
5. **Navigation delegation** - Visualization navigation handler setup
6. **Thread safety** - MainThread.InvokeOnMainThreadAsync for UI updates
7. **Resource cleanup** - Event unsubscription in OnDisappearing

### Current Architecture Assessment
**Testability Score: 4/10** ⚠️ **REQUIRES SIGNIFICANT REFACTORING**

**Design Challenges:**
1. **Multiple constructor patterns** - Primary DI vs backwards compatibility creates complexity
2. **Direct UI manipulation** - UpdateRecentEntriesUI() directly modifies StackLayout children
3. **Event handling complexity** - PropertyChanged subscription with async UI updates
4. **Lifecycle interdependencies** - OnAppearing triggers both ViewModel and UI operations
5. **Factory pattern integration** - Direct dependency on IMoodEntryViewFactory for UI creation
6. **Mixed responsibilities** - Navigation setup, event handling, UI generation, lifecycle management
7. **Exception handling isolation** - Try-catch blocks limit testability of error scenarios

**Good Design Elements:**
1. **Dependency injection support** - Primary constructor follows DI patterns
2. **Null safety** - Proper null checking for injected dependencies
3. **Event cleanup** - OnDisappearing properly unsubscribes events
4. **Async lifecycle awareness** - Proper async/await patterns in OnAppearing
5. **Logging integration** - Debug and operational logging throughout

## XAML Structure Analysis

### UI Components (111 lines XAML)
1. **Statistics summary** - Grid layout with progress metrics (TotalEntries, OverallAverage, etc.)
2. **Trend visualization** - Trend text with dynamic color binding
3. **Visualization button** - Command binding to OpenVisualizationCommand
4. **Loading/error states** - ActivityIndicator, error messages, no data handling
5. **Programmatic content** - HistoryEntriesStack for factory-generated UI

### Data Bindings
- **Statistics**: `{Binding TotalEntries}`, `{Binding OverallAverage}`, `{Binding Last7Days}`, `{Binding Last30Days}`
- **Trend**: `{Binding Trend}`, `{Binding TrendColor}`
- **Commands**: `{Binding OpenVisualizationCommand}`
- **State**: `{Binding IsLoading}`, `{Binding HasNoData}`, `{Binding HasError}`, `{Binding ErrorMessage}`

## Required Refactoring Strategy

### Phase 1: Extract UI Management Abstraction
Create abstraction for programmatic UI operations:

```csharp
public interface IHistoryUIManager
{
    void ClearEntries();
    void AddEntry(MoodEntry entry);
    void UpdateAllEntries(IEnumerable<MoodEntry> entries);
    int GetCurrentEntryCount();
}

public class HistoryUIManager : IHistoryUIManager
{
    private readonly StackLayout _entriesStack;
    private readonly IMoodEntryViewFactory _viewFactory;
    private readonly ILoggingService _loggingService;
    
    public HistoryUIManager(StackLayout entriesStack, IMoodEntryViewFactory viewFactory, ILoggingService loggingService)
    {
        _entriesStack = entriesStack;
        _viewFactory = viewFactory;
        _loggingService = loggingService;
    }
    
    public void UpdateAllEntries(IEnumerable<MoodEntry> entries)
    {
        ClearEntries();
        foreach (var entry in entries)
        {
            AddEntry(entry);
        }
    }
}
```

### Phase 2: Extract Event Handling Coordinator
Create abstraction for event subscription and handling:

```csharp
public interface IViewModelEventCoordinator
{
    void Subscribe(HistoryViewModel viewModel);
    void Unsubscribe(HistoryViewModel viewModel);
    event EventHandler<string> PropertyChangeRequested;
}

public class HistoryViewModelEventCoordinator : IViewModelEventCoordinator
{
    public event EventHandler<string> PropertyChangeRequested;
    
    public void Subscribe(HistoryViewModel viewModel)
    {
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }
    
    public void Unsubscribe(HistoryViewModel viewModel)
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChangeRequested?.Invoke(this, e.PropertyName);
    }
}
```

### Phase 3: Extract Lifecycle Manager
Create abstraction for page lifecycle operations:

```csharp
public interface IPageLifecycleManager
{
    Task HandlePageAppearingAsync();
    void HandlePageDisappearing();
}

public class HistoryPageLifecycleManager : IPageLifecycleManager
{
    private readonly HistoryViewModel _viewModel;
    private readonly IHistoryUIManager _uiManager;
    
    public HistoryPageLifecycleManager(HistoryViewModel viewModel, IHistoryUIManager uiManager)
    {
        _viewModel = viewModel;
        _uiManager = uiManager;
    }
    
    public async Task HandlePageAppearingAsync()
    {
        await _viewModel.InitializeAsync();
        await MainThread.InvokeOnMainThreadAsync(() => 
            _uiManager.UpdateAllEntries(_viewModel.RecentEntries));
    }
}
```

### Phase 4: Refactored Architecture
Transform to testable design with clear separation of concerns:

```csharp
public partial class History : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    private readonly IHistoryUIManager _uiManager;
    private readonly IViewModelEventCoordinator _eventCoordinator;
    private readonly IPageLifecycleManager _lifecycleManager;

    public History(HistoryViewModel viewModel, 
                   IMoodEntryViewFactory viewFactory,
                   INavigationService navigationService = null,
                   ILoggingService loggingService = null)
    {
        InitializeComponent();
        
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        
        // Create abstraction layers
        _uiManager = new HistoryUIManager(HistoryEntriesStack, viewFactory, loggingService);
        _eventCoordinator = new HistoryViewModelEventCoordinator();
        _lifecycleManager = new HistoryPageLifecycleManager(_viewModel, _uiManager);
        
        // Setup bindings and events
        BindingContext = _viewModel;
        _eventCoordinator.Subscribe(_viewModel);
        _eventCoordinator.PropertyChangeRequested += OnPropertyChangeRequested;
        
        _viewModel.SetVisualizationNavigationHandler(HandleVisualizationNavigationAsync);
    }

    private async void OnPropertyChangeRequested(object sender, string propertyName)
    {
        if (propertyName == nameof(HistoryViewModel.RecentEntries))
        {
            await MainThread.InvokeOnMainThreadAsync(() => 
                _uiManager.UpdateAllEntries(_viewModel.RecentEntries));
        }
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
HistoryPageTests/
├── Constructor/
│   ├── Primary/
│   │   ├── Should_ThrowArgumentNullException_WhenViewModelIsNull()
│   │   ├── Should_ThrowArgumentNullException_WhenViewFactoryIsNull()
│   │   ├── Should_InitializeComponent_WhenConstructed()
│   │   ├── Should_SetBindingContext_ToProvidedViewModel()
│   │   ├── Should_CreateUIManager_WithCorrectDependencies()
│   │   ├── Should_SubscribeToEventCoordinator_WhenConstructed()
│   │   └── Should_SetupVisualizationNavigationHandler_WhenConstructed()
│   └── BackwardsCompatibility/
│       ├── Should_CreateDefaultDependencies_WhenNoParametersProvided()
│       ├── Should_UseProvidedMoodDataService_WhenProvided()
│       ├── Should_UseProvidedLoggingService_WhenProvided()
│       ├── Should_StartInitializeAsync_WhenConstructed()
│       └── Should_SetupAllEventHandlers_WhenConstructed()
├── EventHandling/
│   ├── Should_SubscribeToViewModelPropertyChanged_WhenConstructed()
│   ├── Should_HandleRecentEntriesPropertyChange_WhenTriggered()
│   ├── Should_UpdateUIOnMainThread_WhenPropertyChanges()
│   ├── Should_IgnoreOtherPropertyChanges_WhenTriggered()
│   ├── Should_UnsubscribeFromEvents_WhenPageDisappears()
│   └── Should_HandleEventSubscriptionErrors_Gracefully()
├── UIManagement/
│   ├── Should_ClearExistingEntries_BeforeUpdating()
│   ├── Should_CreateEntryViewsUsingFactory_WhenUpdating()
│   ├── Should_AddAllEntriesToStack_WhenUpdating()
│   ├── Should_LogUIOperations_WhenAvailable()
│   ├── Should_HandleFactoryExceptions_Gracefully()
│   └── Should_UpdateEntryCount_AfterUIUpdate()
├── Lifecycle/
│   ├── OnAppearing/
│   │   ├── Should_CallViewModelInitializeAsync_WhenAppearing()
│   │   ├── Should_ForceUIUpdate_AfterInitialization()
│   │   ├── Should_HandleInitializationErrors_Gracefully()
│   │   └── Should_CallBaseOnAppearing_WhenAppearing()
│   └── OnDisappearing/
│       ├── Should_UnsubscribeFromPropertyChanged_WhenDisappearing()
│       ├── Should_CallBaseOnDisappearing_WhenDisappearing()
│       └── Should_HandleUnsubscriptionErrors_Gracefully()
├── Navigation/
│   ├── Should_SetVisualizationNavigationHandler_WhenConstructed()
│   ├── Should_CreateVisualizationPageWithService_WhenNavigating()
│   ├── Should_NavigateUsingNavigationService_WhenHandlerCalled()
│   └── Should_HandleNavigationErrors_Gracefully()
├── ViewModelIntegration/
│   ├── Should_BindStatisticsProperties_ToViewModelData()
│   ├── Should_BindTrendProperties_ToViewModelData()
│   ├── Should_BindLoadingStates_ToViewModelData()
│   ├── Should_BindErrorStates_ToViewModelData()
│   └── Should_BindCommands_ToViewModelCommands()
├── Threading/
│   ├── Should_InvokeUIUpdatesOnMainThread_WhenPropertyChanges()
│   ├── Should_HandleMainThreadInvocationErrors_Gracefully()
│   ├── Should_InitializeAsyncOnTaskRun_InBackwardsCompatibility()
│   └── Should_MaintainThreadSafety_DuringUIUpdates()
└── ErrorHandling/
    ├── Should_LogAndIgnoreUIUpdateErrors_WhenExceptionOccurs()
    ├── Should_HandleViewModelInitializationErrors_Gracefully()
    ├── Should_HandleFactoryCreationErrors_Gracefully()
    └── Should_HandleNavigationErrors_Gracefully()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenViewModelIsNull()
{
    // Arrange
    HistoryViewModel? nullViewModel = null;
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockNavigationService = new Mock<INavigationService>();
    var mockLoggingService = new Mock<ILoggingService>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new History(nullViewModel!, mockViewFactory.Object, mockNavigationService.Object, mockLoggingService.Object));
    Assert.That(exception.ParamName, Is.EqualTo("viewModel"));
}

[Test]
public void Constructor_Should_SetBindingContext_ToProvidedViewModel()
{
    // Arrange
    var mockViewModel = new Mock<HistoryViewModel>();
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockUIManager = new Mock<IHistoryUIManager>();
    var mockEventCoordinator = new Mock<IViewModelEventCoordinator>();
    var mockLifecycleManager = new Mock<IPageLifecycleManager>();

    // Act
    var historyPage = new History(mockViewModel.Object, mockViewFactory.Object);

    // Assert
    Assert.That(historyPage.BindingContext, Is.SameAs(mockViewModel.Object));
}
```

#### Event Handling Tests
```csharp
[Test]
public void EventHandling_Should_HandleRecentEntriesPropertyChange_WhenTriggered()
{
    // Arrange
    var mockViewModel = new Mock<HistoryViewModel>();
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockUIManager = new Mock<IHistoryUIManager>();
    var eventCoordinator = new HistoryViewModelEventCoordinator();
    
    var historyPage = new History(mockViewModel.Object, mockViewFactory.Object);
    
    // Setup test data
    var testEntries = new List<MoodEntry>
    {
        new MoodEntry { Date = new DateOnly(2024, 10, 17), StartOfWork = 7 }
    };
    mockViewModel.Setup(vm => vm.RecentEntries).Returns(testEntries);

    // Act
    eventCoordinator.Subscribe(mockViewModel.Object);
    
    // Simulate PropertyChanged event
    mockViewModel.Raise(vm => vm.PropertyChanged += null, 
                       new PropertyChangedEventArgs(nameof(HistoryViewModel.RecentEntries)));

    // Assert
    // Verify UI manager was called to update entries
    mockUIManager.Verify(ui => ui.UpdateAllEntries(testEntries), Times.Once);
}

[Test]
public void EventHandling_Should_UnsubscribeFromEvents_WhenPageDisappears()
{
    // Arrange
    var mockViewModel = new Mock<HistoryViewModel>();
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockEventCoordinator = new Mock<IViewModelEventCoordinator>();
    
    var historyPage = new History(mockViewModel.Object, mockViewFactory.Object);

    // Act
    // Simulate OnDisappearing through reflection
    var onDisappearingMethod = typeof(History).GetMethod("OnDisappearing", BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(historyPage, null);

    // Assert
    mockEventCoordinator.Verify(ec => ec.Unsubscribe(mockViewModel.Object), Times.Once);
}
```

#### UI Management Tests
```csharp
[Test]
public void UIManagement_Should_CreateEntryViewsUsingFactory_WhenUpdating()
{
    // Arrange
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockLoggingService = new Mock<ILoggingService>();
    var testStackLayout = new StackLayout();
    
    var uiManager = new HistoryUIManager(testStackLayout, mockViewFactory.Object, mockLoggingService.Object);
    
    var testEntries = new List<MoodEntry>
    {
        new MoodEntry { Date = new DateOnly(2024, 10, 17), StartOfWork = 7 },
        new MoodEntry { Date = new DateOnly(2024, 10, 16), StartOfWork = 6 }
    };
    
    var mockView1 = new Mock<View>();
    var mockView2 = new Mock<View>();
    mockViewFactory.Setup(vf => vf.CreateEntryView(testEntries[0])).Returns(mockView1.Object);
    mockViewFactory.Setup(vf => vf.CreateEntryView(testEntries[1])).Returns(mockView2.Object);

    // Act
    uiManager.UpdateAllEntries(testEntries);

    // Assert
    mockViewFactory.Verify(vf => vf.CreateEntryView(testEntries[0]), Times.Once);
    mockViewFactory.Verify(vf => vf.CreateEntryView(testEntries[1]), Times.Once);
    Assert.That(testStackLayout.Children.Count, Is.EqualTo(2));
    Assert.That(testStackLayout.Children[0], Is.SameAs(mockView1.Object));
    Assert.That(testStackLayout.Children[1], Is.SameAs(mockView2.Object));
}

[Test]
public void UIManagement_Should_ClearExistingEntries_BeforeUpdating()
{
    // Arrange
    var mockViewFactory = new Mock<IMoodEntryViewFactory>();
    var mockLoggingService = new Mock<ILoggingService>();
    var testStackLayout = new StackLayout();
    
    // Add existing child
    testStackLayout.Children.Add(new Label { Text = "Existing" });
    
    var uiManager = new HistoryUIManager(testStackLayout, mockViewFactory.Object, mockLoggingService.Object);
    
    var testEntries = new List<MoodEntry>
    {
        new MoodEntry { Date = new DateOnly(2024, 10, 17), StartOfWork = 7 }
    };
    
    var mockView = new Mock<View>();
    mockViewFactory.Setup(vf => vf.CreateEntryView(testEntries[0])).Returns(mockView.Object);

    // Act
    uiManager.UpdateAllEntries(testEntries);

    // Assert
    Assert.That(testStackLayout.Children.Count, Is.EqualTo(1));
    Assert.That(testStackLayout.Children[0], Is.SameAs(mockView.Object));
}
```

#### Lifecycle Tests
```csharp
[Test]
public async Task Lifecycle_Should_CallViewModelInitializeAsync_WhenAppearing()
{
    // Arrange
    var mockViewModel = new Mock<HistoryViewModel>();
    var mockUIManager = new Mock<IHistoryUIManager>();
    
    var lifecycleManager = new HistoryPageLifecycleManager(mockViewModel.Object, mockUIManager.Object);

    // Act
    await lifecycleManager.HandlePageAppearingAsync();

    // Assert
    mockViewModel.Verify(vm => vm.InitializeAsync(), Times.Once);
}

[Test]
public async Task Lifecycle_Should_ForceUIUpdate_AfterInitialization()
{
    // Arrange
    var mockViewModel = new Mock<HistoryViewModel>();
    var mockUIManager = new Mock<IHistoryUIManager>();
    
    var testEntries = new List<MoodEntry>
    {
        new MoodEntry { Date = new DateOnly(2024, 10, 17), StartOfWork = 7 }
    };
    mockViewModel.Setup(vm => vm.RecentEntries).Returns(testEntries);
    
    var lifecycleManager = new HistoryPageLifecycleManager(mockViewModel.Object, mockUIManager.Object);

    // Act
    await lifecycleManager.HandlePageAppearingAsync();

    // Assert
    mockUIManager.Verify(ui => ui.UpdateAllEntries(testEntries), Times.Once);
}
```

#### Integration Tests
```csharp
[Test]
public void Integration_Should_HandleCompleteWorkflow_FromConstructionToDataDisplay()
{
    // Arrange
    var realViewModel = new HistoryViewModel(/* real dependencies */);
    var realViewFactory = new MoodEntryViewFactory();
    
    // Act
    var historyPage = new History(realViewModel, realViewFactory);
    
    // Simulate data loading
    realViewModel.RecentEntries.Add(new MoodEntry 
    { 
        Date = new DateOnly(2024, 10, 17), 
        StartOfWork = 7 
    });

    // Assert
    Assert.That(historyPage.BindingContext, Is.SameAs(realViewModel));
    // UI testing framework would verify the entry was added to HistoryEntriesStack
}
```

### Test Fixtures Required
- **HistoryViewModelMockFactory** - Create configured HistoryViewModel mocks with test data
- **MoodEntryViewFactoryTestDouble** - Test double for UI view creation
- **StackLayoutTestDouble** - Testable version of StackLayout for UI manipulation tests
- **MainThreadTestInvoker** - Synchronous implementation for testing MainThread.InvokeOnMainThreadAsync
- **NavigationServiceTestDouble** - Test double for navigation behavior

## Success Criteria
- [ ] **Constructor validation** - Both DI and backwards compatibility patterns tested
- [ ] **Event handling** - PropertyChanged subscription, handling, and cleanup verified
- [ ] **UI management** - Factory-based view creation and StackLayout manipulation tested
- [ ] **Lifecycle coordination** - OnAppearing/OnDisappearing behavior properly tested
- [ ] **Threading safety** - MainThread operations properly tested and abstracted
- [ ] **Navigation delegation** - Visualization navigation handler setup and execution verified
- [ ] **Error resilience** - All exception handling scenarios tested

## Implementation Priority
**HIGH PRIORITY** - Complex page with significant business logic in code-behind. Critical for user engagement and represents sophisticated UI generation patterns used throughout the application.

## Dependencies for Testing
- **MAUI Controls Test Framework** - For StackLayout and View manipulation testing
- **PropertyChanged event testing tools** - For ViewModel integration testing
- **MainThread operation mocking** - For thread-safe UI update testing
- **Factory pattern testing framework** - For IMoodEntryViewFactory behavior verification
- **Async lifecycle testing tools** - For OnAppearing/OnDisappearing behavior validation

## Implementation Estimate
**Effort: High (4-5 days)**
- Complex abstraction layer creation (UI manager, event coordinator, lifecycle manager)
- Comprehensive constructor pattern testing (primary + backwards compatibility)
- Event handling abstraction and testing framework
- Factory integration testing with StackLayout manipulation
- Threading and MainThread operation testing
- Navigation handler setup and execution testing
- Lifecycle coordination testing across async operations

This page represents the most complex code-behind logic encountered so far, requiring substantial refactoring to achieve testability while maintaining the sophisticated UI generation and event handling patterns essential for the user experience.