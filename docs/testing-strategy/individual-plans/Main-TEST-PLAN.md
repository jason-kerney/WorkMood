# Main (Page) - Individual Test Plan

## Class Overview
**File**: `MauiApp/Pages/Main.xaml.cs`  
**Type**: MAUI ContentPage (MVVM with Navigation Coordination)  
**LOC**: 103 lines (code-behind)  
**XAML LOC**: 124 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Main landing page that serves as the primary user entry point for the WorkMood application. Provides navigation to all major features (mood recording, history, graph, settings, about) through a clean, button-based interface. Features event-driven navigation coordination, dependency injection for services, and ViewModel lifecycle management.

### Dependencies
- **MainPageViewModel** (injected) - Business logic for commands, navigation events, and data management
- **IServiceProvider** (injected) - Service provider for resolving dependencies during navigation
- **INavigationService** (created) - Navigation service for page transitions and alerts
- **MAUI Framework**: ContentPage, data binding, command binding, lifecycle events

### Key Responsibilities
1. **Dependency injection setup** - Constructor injection for ViewModel and ServiceProvider
2. **Navigation service creation** - Creates NavigationService with page context
3. **Event subscription management** - Subscribes to ViewModel navigation events
4. **Lifecycle coordination** - OnAppearing triggers ViewModel initialization and date refresh
5. **Navigation delegation** - Handles navigation events from ViewModel to create appropriate pages
6. **Service resolution** - Uses DI container to resolve services for target pages
7. **Error handling** - Displays navigation errors through NavigationService.ShowErrorAsync

### Current Architecture Assessment
**Testability Score: 6/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Design Challenges:**
1. **Service locator pattern** - Direct Handler?.MauiContext?.Services usage creates tight coupling
2. **Mixed dependency strategies** - Some services injected, others resolved through service locator
3. **Constructor-based navigation** - Creates target pages directly in event handlers
4. **Event handler complexity** - Navigation logic scattered across multiple event handlers
5. **Implicit service dependencies** - GraphViewModel, ScheduleConfigService resolved without interface
6. **Navigation service lifecycle** - Created in constructor but used throughout page lifetime

**Good Design Elements:**
1. **MVVM separation** - Clear separation between View and ViewModel responsibilities
2. **Dependency injection foundation** - Primary dependencies properly injected
3. **Event-driven architecture** - Clean event subscription pattern with ViewModel
4. **Error handling** - Proper error display for navigation failures
5. **Lifecycle awareness** - Proper OnAppearing integration with ViewModel initialization
6. **Null safety** - ArgumentNullException checks for critical dependencies

## XAML Structure Analysis

### UI Components (124 lines XAML)
1. **Header section** - App title, subtitle, current date display with semantic markup
2. **Welcome message** - Styled welcome banner with brand colors
3. **Navigation buttons** - Five primary action buttons with consistent styling and emoji icons
4. **Footer section** - Status information about auto-save and data sync
5. **Layout structure** - Grid-based layout with ScrollView for accessibility

### Data Bindings
- **Date Display**: `{Binding CurrentDate}` - Dynamic date display from ViewModel
- **Commands**: All buttons bound to ViewModel commands (RecordMoodCommand, ViewHistoryCommand, etc.)
- **Styling**: Consistent use of StaticResource for colors and responsive design

### Command Bindings
- **RecordMoodCommand** → Triggers NavigateToMoodRecording event
- **ViewHistoryCommand** → Triggers NavigateToHistory event  
- **ViewGraphCommand** → Triggers NavigateToGraph event
- **SettingsCommand** → Triggers NavigateToSettings event
- **AboutCommand** → Triggers NavigateToAbout event

## Required Refactoring Strategy

### Phase 1: Extract Navigation Coordinator
Create abstraction for navigation event handling and page creation:

```csharp
public interface IMainPageNavigationCoordinator
{
    Task HandleMoodRecordingNavigationAsync(NavigateToMoodRecordingEventArgs args);
    Task HandleHistoryNavigationAsync();
    Task HandleGraphNavigationAsync();
    Task HandleSettingsNavigationAsync();
    Task HandleAboutNavigationAsync();
    Task HandleDisplayAlertAsync(DisplayAlertEventArgs args);
}

public class MainPageNavigationCoordinator : IMainPageNavigationCoordinator
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _loggingService;
    
    public MainPageNavigationCoordinator(
        INavigationService navigationService, 
        IServiceProvider serviceProvider,
        ILoggingService loggingService)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _loggingService = loggingService;
    }
    
    public async Task HandleMoodRecordingNavigationAsync(NavigateToMoodRecordingEventArgs args)
    {
        try
        {
            await _navigationService.NavigateAsync(() => 
                new MoodRecording(args.MoodDataService, args.DispatcherService, args.LoggingService));
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "Navigation failed for mood recording");
            await _navigationService.ShowErrorAsync("Failed to navigate to mood recording");
        }
    }
    
    public async Task HandleGraphNavigationAsync()
    {
        try
        {
            var graphViewModel = _serviceProvider.GetService<GraphViewModel>();
            if (graphViewModel != null)
            {
                await _navigationService.NavigateAsync(() => new Graph(graphViewModel));
            }
            else
            {
                await _navigationService.ShowErrorAsync("Failed to get graph view model service");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "Navigation failed for graph");
            await _navigationService.ShowErrorAsync("Failed to navigate to graph");
        }
    }
}
```

### Phase 2: Extract Event Subscription Manager
Create abstraction for ViewModel event management:

```csharp
public interface IViewModelEventManager
{
    void Subscribe(MainPageViewModel viewModel);
    void Unsubscribe(MainPageViewModel viewModel);
    event EventHandler<NavigateToMoodRecordingEventArgs> MoodRecordingRequested;
    event EventHandler HistoryRequested;
    event EventHandler GraphRequested;
    event EventHandler SettingsRequested;
    event EventHandler AboutRequested;
    event EventHandler<DisplayAlertEventArgs> AlertRequested;
}

public class MainPageViewModelEventManager : IViewModelEventManager
{
    public event EventHandler<NavigateToMoodRecordingEventArgs> MoodRecordingRequested;
    public event EventHandler HistoryRequested;
    public event EventHandler GraphRequested;
    public event EventHandler SettingsRequested;
    public event EventHandler AboutRequested;
    public event EventHandler<DisplayAlertEventArgs> AlertRequested;
    
    public void Subscribe(MainPageViewModel viewModel)
    {
        viewModel.NavigateToMoodRecording += (s, e) => MoodRecordingRequested?.Invoke(s, e);
        viewModel.NavigateToHistory += (s, e) => HistoryRequested?.Invoke(s, e);
        viewModel.NavigateToGraph += (s, e) => GraphRequested?.Invoke(s, e);
        viewModel.NavigateToSettings += (s, e) => SettingsRequested?.Invoke(s, e);
        viewModel.NavigateToAbout += (s, e) => AboutRequested?.Invoke(s, e);
        viewModel.DisplayAlert += (s, e) => AlertRequested?.Invoke(s, e);
    }
    
    public void Unsubscribe(MainPageViewModel viewModel)
    {
        // Proper event unsubscription implementation
        viewModel.NavigateToMoodRecording -= (s, e) => MoodRecordingRequested?.Invoke(s, e);
        // ... other unsubscriptions
    }
}
```

### Phase 3: Extract Page Lifecycle Manager
Create abstraction for page lifecycle operations:

```csharp
public interface IMainPageLifecycleManager
{
    Task HandlePageAppearingAsync();
    void HandlePageDisappearing();
}

public class MainPageLifecycleManager : IMainPageLifecycleManager
{
    private readonly MainPageViewModel _viewModel;
    private readonly ILoggingService _loggingService;
    
    public MainPageLifecycleManager(MainPageViewModel viewModel, ILoggingService loggingService)
    {
        _viewModel = viewModel;
        _loggingService = loggingService;
    }
    
    public async Task HandlePageAppearingAsync()
    {
        try
        {
            await _viewModel.InitializeAsync();
            _viewModel.RefreshCurrentDate();
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "Error during page appearing lifecycle");
        }
    }
    
    public void HandlePageDisappearing()
    {
        // Future cleanup operations
    }
}
```

### Phase 4: Refactored Architecture
Transform to testable design with clear separation of concerns:

```csharp
public partial class Main : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly IMainPageNavigationCoordinator _navigationCoordinator;
    private readonly IViewModelEventManager _eventManager;
    private readonly IMainPageLifecycleManager _lifecycleManager;

    public Main(MainPageViewModel viewModel, 
                IServiceProvider serviceProvider,
                ILoggingService loggingService = null)
    {
        InitializeComponent();
        
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
        
        // Create abstraction layers
        var navigationService = new NavigationService(this);
        _navigationCoordinator = new MainPageNavigationCoordinator(navigationService, serviceProvider, loggingService);
        _eventManager = new MainPageViewModelEventManager();
        _lifecycleManager = new MainPageLifecycleManager(_viewModel, loggingService);
        
        // Setup event coordination
        _eventManager.Subscribe(_viewModel);
        _eventManager.MoodRecordingRequested += _navigationCoordinator.HandleMoodRecordingNavigationAsync;
        _eventManager.HistoryRequested += (s, e) => _navigationCoordinator.HandleHistoryNavigationAsync();
        _eventManager.GraphRequested += (s, e) => _navigationCoordinator.HandleGraphNavigationAsync();
        _eventManager.SettingsRequested += (s, e) => _navigationCoordinator.HandleSettingsNavigationAsync();
        _eventManager.AboutRequested += (s, e) => _navigationCoordinator.HandleAboutNavigationAsync();
        _eventManager.AlertRequested += (s, e) => _navigationCoordinator.HandleDisplayAlertAsync(e);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _lifecycleManager.HandlePageAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _lifecycleManager.HandlePageDisappearing();
        _eventManager.Unsubscribe(_viewModel);
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
MainPageTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenViewModelIsNull()
│   ├── Should_ThrowArgumentNullException_WhenServiceProviderIsNull()
│   ├── Should_InitializeComponent_WhenConstructed()
│   ├── Should_SetBindingContext_ToProvidedViewModel()
│   ├── Should_CreateNavigationService_WithPageContext()
│   ├── Should_CreateNavigationCoordinator_WithCorrectDependencies()
│   ├── Should_CreateEventManager_AndSubscribeToViewModel()
│   └── Should_CreateLifecycleManager_WithCorrectDependencies()
├── EventSubscription/
│   ├── Should_SubscribeToNavigateToMoodRecording_WhenConstructed()
│   ├── Should_SubscribeToNavigateToHistory_WhenConstructed()
│   ├── Should_SubscribeToNavigateToGraph_WhenConstructed()
│   ├── Should_SubscribeToNavigateToSettings_WhenConstructed()
│   ├── Should_SubscribeToNavigateToAbout_WhenConstructed()
│   ├── Should_SubscribeToDisplayAlert_WhenConstructed()
│   └── Should_UnsubscribeFromAllEvents_WhenPageDisappears()
├── NavigationCoordination/
│   ├── MoodRecording/
│   │   ├── Should_NavigateToMoodRecording_WhenEventTriggered()
│   │   ├── Should_PassCorrectServices_ToMoodRecordingPage()
│   │   └── Should_HandleNavigationErrors_ForMoodRecording()
│   ├── History/
│   │   ├── Should_NavigateToHistory_WhenEventTriggered()
│   │   ├── Should_ResolveLoggingService_ForHistoryPage()
│   │   └── Should_HandleNavigationErrors_ForHistory()
│   ├── Graph/
│   │   ├── Should_NavigateToGraph_WhenEventTriggered()
│   │   ├── Should_ResolveGraphViewModel_FromServiceProvider()
│   │   ├── Should_ShowError_WhenGraphViewModelNotAvailable()
│   │   └── Should_HandleNavigationErrors_ForGraph()
│   ├── Settings/
│   │   ├── Should_NavigateToSettings_WhenEventTriggered()
│   │   ├── Should_ResolveScheduleConfigService_FromServiceProvider()
│   │   ├── Should_ShowError_WhenScheduleConfigServiceNotAvailable()
│   │   └── Should_HandleNavigationErrors_ForSettings()
│   └── About/
│       ├── Should_NavigateToAbout_WhenEventTriggered()
│       ├── Should_ResolveAboutPage_FromServiceProvider()
│       └── Should_HandleNavigationErrors_ForAbout()
├── AlertHandling/
│   ├── Should_ShowAlert_WhenDisplayAlertEventTriggered()
│   ├── Should_PassCorrectTitle_ToNavigationService()
│   ├── Should_PassCorrectMessage_ToNavigationService()
│   ├── Should_PassCorrectAcceptText_ToNavigationService()
│   └── Should_HandleAlertErrors_Gracefully()
├── Lifecycle/
│   ├── OnAppearing/
│   │   ├── Should_CallViewModelInitializeAsync_WhenAppearing()
│   │   ├── Should_CallViewModelRefreshCurrentDate_WhenAppearing()
│   │   ├── Should_CallBaseOnAppearing_WhenAppearing()
│   │   └── Should_HandleInitializationErrors_WhenAppearing()
│   └── OnDisappearing/
│       ├── Should_CallLifecycleManagerHandleDisappearing_WhenDisappearing()
│       ├── Should_UnsubscribeFromViewModelEvents_WhenDisappearing()
│       └── Should_CallBaseOnDisappearing_WhenDisappearing()
├── ServiceIntegration/
│   ├── Should_CreateNavigationService_WithCorrectPageReference()
│   ├── Should_PassServiceProvider_ToNavigationCoordinator()
│   ├── Should_ResolveServices_ThroughServiceProvider()
│   └── Should_HandleServiceResolutionFailures_Gracefully()
└── ErrorHandling/
    ├── Should_LogNavigationErrors_WhenNavigationFails()
    ├── Should_ShowErrorMessage_WhenNavigationFails()
    ├── Should_HandleServiceProviderErrors_Gracefully()
    └── Should_HandleEventSubscriptionErrors_Gracefully()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenViewModelIsNull()
{
    // Arrange
    MainPageViewModel? nullViewModel = null;
    var mockServiceProvider = new Mock<IServiceProvider>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new Main(nullViewModel!, mockServiceProvider.Object));
    Assert.That(exception.ParamName, Is.EqualTo("viewModel"));
}

[Test]
public void Constructor_Should_SetBindingContext_ToProvidedViewModel()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockLoggingService = new Mock<ILoggingService>();

    // Act
    var mainPage = new Main(mockViewModel.Object, mockServiceProvider.Object, mockLoggingService.Object);

    // Assert
    Assert.That(mainPage.BindingContext, Is.SameAs(mockViewModel.Object));
}

[Test]
public void Constructor_Should_CreateNavigationCoordinator_WithCorrectDependencies()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockLoggingService = new Mock<ILoggingService>();

    // Act
    var mainPage = new Main(mockViewModel.Object, mockServiceProvider.Object, mockLoggingService.Object);

    // Assert
    // Verify that navigation coordinator was created with correct dependencies
    // This would be tested through dependency injection verification or testing hooks
    Assert.That(mainPage, Is.Not.Null);
}
```

#### Navigation Tests
```csharp
[Test]
public async Task Navigation_Should_NavigateToMoodRecording_WhenEventTriggered()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockNavigationCoordinator = new Mock<IMainPageNavigationCoordinator>();
    
    var mainPage = new Main(mockViewModel.Object, mockServiceProvider.Object);
    
    var eventArgs = new NavigateToMoodRecordingEventArgs
    {
        MoodDataService = Mock.Of<IMoodDataService>(),
        DispatcherService = Mock.Of<IDispatcherService>(),
        LoggingService = Mock.Of<ILoggingService>()
    };

    // Act
    // Simulate NavigateToMoodRecording event from ViewModel
    mockViewModel.Raise(vm => vm.NavigateToMoodRecording += null, eventArgs);

    // Assert
    mockNavigationCoordinator.Verify(nc => 
        nc.HandleMoodRecordingNavigationAsync(eventArgs), Times.Once);
}

[Test]
public async Task Navigation_Should_ResolveGraphViewModel_FromServiceProvider()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockGraphViewModel = new Mock<GraphViewModel>();
    var mockNavigationService = new Mock<INavigationService>();
    
    mockServiceProvider.Setup(sp => sp.GetService<GraphViewModel>())
        .Returns(mockGraphViewModel.Object);

    var navigationCoordinator = new MainPageNavigationCoordinator(
        mockNavigationService.Object, 
        mockServiceProvider.Object, 
        Mock.Of<ILoggingService>());

    // Act
    await navigationCoordinator.HandleGraphNavigationAsync();

    // Assert
    mockServiceProvider.Verify(sp => sp.GetService<GraphViewModel>(), Times.Once);
    mockNavigationService.Verify(ns => 
        ns.NavigateAsync(It.IsAny<Func<Graph>>()), Times.Once);
}

[Test]
public async Task Navigation_Should_ShowError_WhenGraphViewModelNotAvailable()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockLoggingService = new Mock<ILoggingService>();
    
    mockServiceProvider.Setup(sp => sp.GetService<GraphViewModel>())
        .Returns((GraphViewModel)null);

    var navigationCoordinator = new MainPageNavigationCoordinator(
        mockNavigationService.Object, 
        mockServiceProvider.Object, 
        mockLoggingService.Object);

    // Act
    await navigationCoordinator.HandleGraphNavigationAsync();

    // Assert
    mockNavigationService.Verify(ns => 
        ns.ShowErrorAsync("Failed to get graph view model service"), Times.Once);
}
```

#### Event Management Tests
```csharp
[Test]
public void EventManagement_Should_SubscribeToAllViewModelEvents_WhenConstructed()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var eventManager = new MainPageViewModelEventManager();
    
    bool moodRecordingTriggered = false;
    bool historyTriggered = false;
    bool graphTriggered = false;
    bool settingsTriggered = false;
    bool aboutTriggered = false;
    bool alertTriggered = false;

    eventManager.MoodRecordingRequested += (s, e) => moodRecordingTriggered = true;
    eventManager.HistoryRequested += (s, e) => historyTriggered = true;
    eventManager.GraphRequested += (s, e) => graphTriggered = true;
    eventManager.SettingsRequested += (s, e) => settingsTriggered = true;
    eventManager.AboutRequested += (s, e) => aboutTriggered = true;
    eventManager.AlertRequested += (s, e) => alertTriggered = true;

    // Act
    eventManager.Subscribe(mockViewModel.Object);
    
    // Simulate ViewModel events
    mockViewModel.Raise(vm => vm.NavigateToMoodRecording += null, 
        new NavigateToMoodRecordingEventArgs());
    mockViewModel.Raise(vm => vm.NavigateToHistory += null, EventArgs.Empty);
    mockViewModel.Raise(vm => vm.NavigateToGraph += null, EventArgs.Empty);
    mockViewModel.Raise(vm => vm.NavigateToSettings += null, EventArgs.Empty);
    mockViewModel.Raise(vm => vm.NavigateToAbout += null, EventArgs.Empty);
    mockViewModel.Raise(vm => vm.DisplayAlert += null, 
        new DisplayAlertEventArgs("Test", "Message", "OK"));

    // Assert
    Assert.That(moodRecordingTriggered, Is.True);
    Assert.That(historyTriggered, Is.True);
    Assert.That(graphTriggered, Is.True);
    Assert.That(settingsTriggered, Is.True);
    Assert.That(aboutTriggered, Is.True);
    Assert.That(alertTriggered, Is.True);
}

[Test]
public void EventManagement_Should_UnsubscribeFromAllEvents_WhenPageDisappears()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mainPage = new Main(mockViewModel.Object, mockServiceProvider.Object);

    // Act
    // Simulate OnDisappearing through reflection
    var onDisappearingMethod = typeof(Main).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(mainPage, null);

    // Assert
    // Verify that events are no longer subscribed by checking subscription count
    // This would require refactoring to make event subscription testable
    Assert.That(mainPage, Is.Not.Null); // Placeholder assertion
}
```

#### Lifecycle Tests
```csharp
[Test]
public async Task Lifecycle_Should_CallViewModelInitializeAsync_WhenAppearing()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockLifecycleManager = new Mock<IMainPageLifecycleManager>();
    
    var mainPage = new Main(mockViewModel.Object, mockServiceProvider.Object);

    // Act
    // Simulate OnAppearing through reflection
    var onAppearingMethod = typeof(Main).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(mainPage, null);

    // Assert
    mockLifecycleManager.Verify(lm => lm.HandlePageAppearingAsync(), Times.Once);
}

[Test]
public async Task Lifecycle_Should_HandleInitializationErrors_WhenAppearing()
{
    // Arrange
    var mockViewModel = new Mock<MainPageViewModel>();
    var mockLoggingService = new Mock<ILoggingService>();
    
    mockViewModel.Setup(vm => vm.InitializeAsync())
        .ThrowsAsync(new InvalidOperationException("Test error"));

    var lifecycleManager = new MainPageLifecycleManager(
        mockViewModel.Object, mockLoggingService.Object);

    // Act
    await lifecycleManager.HandlePageAppearingAsync();

    // Assert
    mockLoggingService.Verify(ls => 
        ls.LogException(It.IsAny<Exception>(), 
            It.Is<string>(s => s.Contains("Error during page appearing"))), 
        Times.Once);
}
```

#### Integration Tests
```csharp
[Test]
public void Integration_Should_HandleCompleteNavigationWorkflow_FromViewModelToPage()
{
    // Arrange
    var realViewModel = new MainPageViewModel(/* real dependencies */);
    var realServiceProvider = new Mock<IServiceProvider>();
    
    // Setup service provider with real services
    realServiceProvider.Setup(sp => sp.GetService<GraphViewModel>())
        .Returns(new GraphViewModel(/* real dependencies */));
    
    var mainPage = new Main(realViewModel, realServiceProvider.Object);

    // Act
    // Trigger graph navigation command from ViewModel
    realViewModel.ViewGraphCommand.Execute(null);

    // Assert
    // Verify navigation occurred
    Assert.That(mainPage.BindingContext, Is.SameAs(realViewModel));
    // Additional assertions would verify navigation state
}
```

### Test Fixtures Required
- **MainPageViewModelMockFactory** - Create configured MainPageViewModel mocks
- **ServiceProviderTestDouble** - Mock service provider with configurable service resolution
- **NavigationServiceTestDouble** - Test double for navigation behavior verification
- **EventManagerTestDouble** - Testable version of event subscription management
- **LifecycleManagerTestDouble** - Test double for lifecycle operation testing

## Success Criteria
- [ ] **Constructor validation** - Dependency injection and null parameter handling tested
- [ ] **Event subscription** - ViewModel event subscription and unsubscription verified
- [ ] **Navigation coordination** - All navigation paths tested with proper service resolution
- [ ] **Service integration** - ServiceProvider usage and error handling tested
- [ ] **Lifecycle management** - OnAppearing/OnDisappearing behavior properly tested
- [ ] **Error handling** - All exception scenarios and error display tested
- [ ] **Alert handling** - DisplayAlert event handling and NavigationService integration tested

## Implementation Priority
**HIGH PRIORITY** - Primary user entry point for entire application. Critical for user onboarding and navigation to all major features. Represents core navigation patterns used throughout the app.

## Dependencies for Testing
- **MAUI ContentPage test framework** - For page lifecycle and data binding testing
- **Event handling test tools** - For ViewModel event subscription/unsubscription testing
- **Service provider mocking framework** - For dependency resolution testing
- **Navigation testing framework** - For page navigation behavior verification
- **Command binding testing tools** - For XAML command binding validation

## Implementation Estimate
**Effort: Moderate (3-4 days)**
- Navigation coordinator abstraction and testing
- Event management abstraction and comprehensive event testing
- Service provider integration testing with multiple resolution scenarios
- Lifecycle manager testing with async operation coordination
- Error handling testing across all navigation paths
- Alert display testing through NavigationService integration

This page serves as the application's primary navigation hub, making it critical for user experience and representing core navigation patterns that influence the entire application architecture.