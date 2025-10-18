# MoodRecording (Page) - Individual Test Plan

## Class Overview
**File**: `MauiApp/Pages/MoodRecording.xaml.cs`  
**Type**: MAUI ContentPage (MVVM Form Interface)  
**LOC**: 42 lines (code-behind)  
**XAML LOC**: 245 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Core user interface for recording daily mood check-ins with two separate mood entry sessions (start of work and end of work). Features a sophisticated mood selection interface with 10-point scales, dynamic UI state management, edit/save workflows, and complex data binding patterns. Represents the primary value-delivery interface for the entire application.

### Dependencies
- **MoodDataService** (concrete class injected) - Data persistence and retrieval operations
- **MoodDispatcherService** (concrete class injected) - Event coordination and scheduling
- **ILoggingService** (interface injected) - Debug and operational logging
- **MoodRecordingViewModel** (created) - Business logic for form state management and validation
- **INavigationService** (created) - Navigation coordination and error display
- **MAUI Framework**: ContentPage, data binding, complex command binding, converter usage

### Key Responsibilities
1. **Concrete service injection** - Constructor accepts concrete service implementations
2. **ViewModel creation** - Creates MoodRecordingViewModel with injected services
3. **Event subscription management** - Subscribes to ViewModel error and navigation events
4. **Navigation service setup** - Creates NavigationService for error display and back navigation
5. **Lifecycle coordination** - OnAppearing triggers ViewModel data refresh
6. **Error handling delegation** - Routes ViewModel errors to NavigationService
7. **Memory management** - OnDisappearing unsubscribes events to prevent leaks

### Current Architecture Assessment
**Testability Score: 5/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Design Challenges:**
1. **Concrete service dependencies** - MoodDataService and MoodDispatcherService are concrete classes, not interfaces
2. **ViewModel creation in constructor** - Creates ViewModel directly rather than injecting it
3. **Tight coupling to service implementations** - Cannot be tested without real service instances
4. **Navigation service creation** - Creates NavigationService in constructor rather than injecting
5. **Mixed abstraction levels** - ILoggingService is interface, others are concrete
6. **Limited dependency injection** - Only partial use of DI patterns

**Good Design Elements:**
1. **Clean MVVM separation** - Pure delegation to ViewModel for business logic
2. **Event-driven architecture** - Proper event subscription and cleanup patterns
3. **Lifecycle awareness** - Proper OnAppearing data refresh and OnDisappearing cleanup
4. **Error handling delegation** - Clean separation of error display concerns
5. **Memory leak prevention** - Proper event unsubscription patterns
6. **Minimal code-behind** - Only 42 lines focused on coordination logic

## XAML Structure Analysis

### UI Components (245 lines XAML)
1. **Header section** - Title and current date display with semantic markup
2. **Morning mood section** - Start of work check-in with 10-point button scale, edit controls
3. **Evening mood section** - End of work check-in with 10-point button scale, edit controls
4. **Action buttons** - Save morning, save evening, back to main navigation
5. **Complex state management** - Visibility, enabling, color, and text bindings throughout

### Data Bindings
- **Current Date**: `{Binding CurrentDate}` - Dynamic date display
- **Morning Mood**: Complex state bindings for 10 individual mood buttons (IsSelected, BorderColor)
- **Evening Mood**: Complex state bindings for 10 individual mood buttons (IsSelected, BorderColor, IsEnabled)
- **UI State**: Visibility bindings for edit buttons, mood buttons, info text
- **Commands**: MorningMoodSelectedCommand, EveningMoodSelectedCommand, Save commands
- **Labels**: MorningMoodLabel, EveningMoodLabel for current mood display

### Converter Usage
- **BoolToColorConverter** - Converts selection state to button colors
- **InverseBoolConverter** - Inverts boolean values for conditional visibility

### Command Bindings
- **MorningMoodSelectedCommand** - Parameters 1-10 for mood selection
- **EveningMoodSelectedCommand** - Parameters 1-10 for mood selection  
- **EditMorningCommand** - Toggles morning mood editing state
- **EditEveningCommand** - Toggles evening mood editing state
- **SaveMorningCommand** - Saves morning mood entry
- **SaveEveningCommand** - Saves evening mood entry
- **BackToMainCommand** - Navigation back to main page

## Required Refactoring Strategy

### Phase 1: Extract Service Interfaces
Create interfaces for concrete service dependencies:

```csharp
public interface IMoodDispatcherService : IDisposable
{
    event EventHandler<MorningReminderEventArgs> MorningReminderTriggered;
    event EventHandler<EveningReminderEventArgs> EveningReminderTriggered;
    Task StartAsync();
    Task StopAsync();
    void UpdateSchedule(ScheduleConfig config);
}

// Update constructor to use interfaces
public MoodRecording(IMoodDataService moodDataService, 
                     IMoodDispatcherService dispatcherService, 
                     ILoggingService loggingService)
```

### Phase 2: Extract ViewModel Factory
Create abstraction for ViewModel creation to enable testing:

```csharp
public interface IMoodRecordingViewModelFactory
{
    MoodRecordingViewModel CreateViewModel(IMoodDataService moodDataService, 
                                          IMoodDispatcherService dispatcherService, 
                                          ILoggingService loggingService);
}

public class MoodRecordingViewModelFactory : IMoodRecordingViewModelFactory
{
    public MoodRecordingViewModel CreateViewModel(IMoodDataService moodDataService, 
                                                 IMoodDispatcherService dispatcherService, 
                                                 ILoggingService loggingService)
    {
        return new MoodRecordingViewModel(moodDataService, dispatcherService, loggingService);
    }
}
```

### Phase 3: Extract Event Coordination
Create abstraction for event handling between page and ViewModel:

```csharp
public interface IMoodRecordingEventCoordinator
{
    void Subscribe(MoodRecordingViewModel viewModel);
    void Unsubscribe(MoodRecordingViewModel viewModel);
    event EventHandler<string> ErrorOccurred;
    event EventHandler NavigateBackRequested;
}

public class MoodRecordingEventCoordinator : IMoodRecordingEventCoordinator
{
    public event EventHandler<string> ErrorOccurred;
    public event EventHandler NavigateBackRequested;
    
    public void Subscribe(MoodRecordingViewModel viewModel)
    {
        viewModel.ErrorOccurred += (s, e) => ErrorOccurred?.Invoke(s, e);
        viewModel.NavigateBackRequested += (s, e) => NavigateBackRequested?.Invoke(s, e);
    }
    
    public void Unsubscribe(MoodRecordingViewModel viewModel)
    {
        viewModel.ErrorOccurred -= (s, e) => ErrorOccurred?.Invoke(s, e);
        viewModel.NavigateBackRequested -= (s, e) => NavigateBackRequested?.Invoke(s, e);
    }
}
```

### Phase 4: Extract Lifecycle Manager
Create abstraction for page lifecycle operations:

```csharp
public interface IMoodRecordingLifecycleManager
{
    Task HandlePageAppearingAsync();
    void HandlePageDisappearing();
}

public class MoodRecordingLifecycleManager : IMoodRecordingLifecycleManager
{
    private readonly MoodRecordingViewModel _viewModel;
    private readonly ILoggingService _loggingService;
    
    public MoodRecordingLifecycleManager(MoodRecordingViewModel viewModel, ILoggingService loggingService)
    {
        _viewModel = viewModel;
        _loggingService = loggingService;
    }
    
    public async Task HandlePageAppearingAsync()
    {
        try
        {
            await _viewModel.RefreshMoodDataAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogException(ex, "Error during MoodRecording page appearing");
        }
    }
    
    public void HandlePageDisappearing()
    {
        // Future cleanup operations
    }
}
```

### Phase 5: Refactored Architecture
Transform to testable design with full dependency injection:

```csharp
public partial class MoodRecording : ContentPage
{
    private readonly MoodRecordingViewModel _viewModel;
    private readonly IMoodRecordingEventCoordinator _eventCoordinator;
    private readonly IMoodRecordingLifecycleManager _lifecycleManager;
    private readonly INavigationService _navigationService;

    public MoodRecording(IMoodDataService moodDataService, 
                         IMoodDispatcherService dispatcherService, 
                         ILoggingService loggingService,
                         IMoodRecordingViewModelFactory viewModelFactory = null,
                         INavigationService navigationService = null)
    {
        InitializeComponent();
        
        // Create or inject dependencies
        var factory = viewModelFactory ?? new MoodRecordingViewModelFactory();
        _viewModel = factory.CreateViewModel(moodDataService, dispatcherService, loggingService);
        _navigationService = navigationService ?? new NavigationService(this);
        
        // Setup ViewModel
        BindingContext = _viewModel;
        
        // Create coordination layers
        _eventCoordinator = new MoodRecordingEventCoordinator();
        _lifecycleManager = new MoodRecordingLifecycleManager(_viewModel, loggingService);
        
        // Wire up events
        _eventCoordinator.Subscribe(_viewModel);
        _eventCoordinator.ErrorOccurred += OnErrorOccurred;
        _eventCoordinator.NavigateBackRequested += OnNavigateBackRequested;
    }

    private async void OnErrorOccurred(object? sender, string errorMessage)
    {
        await _navigationService.ShowErrorAsync(errorMessage);
    }

    private async void OnNavigateBackRequested(object? sender, EventArgs e)
    {
        await _navigationService.GoBackAsync();
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
        _eventCoordinator.Unsubscribe(_viewModel);
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
MoodRecordingPageTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenMoodDataServiceIsNull()
│   ├── Should_ThrowArgumentNullException_WhenDispatcherServiceIsNull()
│   ├── Should_AcceptNullLoggingService_WithoutException()
│   ├── Should_CreateViewModel_WithProvidedServices()
│   ├── Should_SetBindingContext_ToCreatedViewModel()
│   ├── Should_CreateNavigationService_WithPageContext()
│   ├── Should_CreateEventCoordinator_AndSubscribeToViewModel()
│   └── Should_CreateLifecycleManager_WithCorrectDependencies()
├── ViewModelCreation/
│   ├── Should_UseProvidedViewModelFactory_WhenProvided()
│   ├── Should_CreateDefaultViewModelFactory_WhenNotProvided()
│   ├── Should_PassAllServices_ToViewModelFactory()
│   └── Should_HandleViewModelCreationErrors_Gracefully()
├── EventCoordination/
│   ├── Should_SubscribeToErrorOccurred_WhenConstructed()
│   ├── Should_SubscribeToNavigateBackRequested_WhenConstructed()
│   ├── Should_RouteErrorsToNavigationService_WhenErrorOccurs()
│   ├── Should_TriggerBackNavigation_WhenBackRequested()
│   ├── Should_UnsubscribeFromEvents_WhenPageDisappears()
│   └── Should_HandleEventSubscriptionErrors_Gracefully()
├── NavigationService/
│   ├── Should_ShowError_WhenErrorEventTriggered()
│   ├── Should_GoBack_WhenNavigateBackEventTriggered()
│   ├── Should_UseProvidedNavigationService_WhenProvided()
│   ├── Should_CreateDefaultNavigationService_WhenNotProvided()
│   └── Should_HandleNavigationErrors_Gracefully()
├── Lifecycle/
│   ├── OnAppearing/
│   │   ├── Should_CallViewModelRefreshMoodDataAsync_WhenAppearing()
│   │   ├── Should_CallLifecycleManagerHandleAppearing_WhenAppearing()
│   │   ├── Should_HandleRefreshErrors_WhenAppearing()
│   │   └── Should_CallBaseOnAppearing_WhenAppearing()
│   └── OnDisappearing/
│       ├── Should_CallLifecycleManagerHandleDisappearing_WhenDisappearing()
│       ├── Should_UnsubscribeFromViewModel_WhenDisappearing()
│       ├── Should_CallBaseOnDisappearing_WhenDisappearing()
│       └── Should_HandleUnsubscriptionErrors_Gracefully()
├── ServiceIntegration/
│   ├── Should_PassMoodDataService_ToViewModel()
│   ├── Should_PassMoodDispatcherService_ToViewModel()
│   ├── Should_PassLoggingService_ToViewModel()
│   ├── Should_HandleNullLoggingService_InViewModel()
│   └── Should_VerifyServiceInterfaces_AreRespected()
└── ErrorHandling/
    ├── Should_LogErrors_WhenErrorsOccur()
    ├── Should_DisplayErrors_ThroughNavigationService()
    ├── Should_HandleViewModelErrors_Gracefully()
    └── Should_HandleLifecycleErrors_Gracefully()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenMoodDataServiceIsNull()
{
    // Arrange
    IMoodDataService? nullMoodDataService = null;
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new MoodRecording(nullMoodDataService!, mockDispatcherService.Object, mockLoggingService.Object));
    Assert.That(exception.ParamName, Is.EqualTo("moodDataService"));
}

[Test]
public void Constructor_Should_CreateViewModel_WithProvidedServices()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockViewModelFactory = new Mock<IMoodRecordingViewModelFactory>();
    var mockViewModel = new Mock<MoodRecordingViewModel>();
    
    mockViewModelFactory.Setup(vf => vf.CreateViewModel(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object))
        .Returns(mockViewModel.Object);

    // Act
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object,
        mockViewModelFactory.Object);

    // Assert
    mockViewModelFactory.Verify(vf => vf.CreateViewModel(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object), Times.Once);
    Assert.That(moodRecordingPage.BindingContext, Is.SameAs(mockViewModel.Object));
}

[Test]
public void Constructor_Should_AcceptNullLoggingService_WithoutException()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    ILoggingService? nullLoggingService = null;

    // Act & Assert
    Assert.DoesNotThrow(() =>
        new MoodRecording(mockMoodDataService.Object, mockDispatcherService.Object, nullLoggingService));
}
```

#### Event Coordination Tests
```csharp
[Test]
public void EventCoordination_Should_RouteErrorsToNavigationService_WhenErrorOccurs()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockNavigationService = new Mock<INavigationService>();
    var mockEventCoordinator = new Mock<IMoodRecordingEventCoordinator>();
    
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object,
        navigationService: mockNavigationService.Object);

    const string testErrorMessage = "Test error occurred";

    // Act
    // Simulate error event from ViewModel through event coordinator
    mockEventCoordinator.Raise(ec => ec.ErrorOccurred += null, 
        (object)moodRecordingPage, testErrorMessage);

    // Assert
    mockNavigationService.Verify(ns => ns.ShowErrorAsync(testErrorMessage), Times.Once);
}

[Test]
public void EventCoordination_Should_TriggerBackNavigation_WhenBackRequested()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockNavigationService = new Mock<INavigationService>();
    var mockEventCoordinator = new Mock<IMoodRecordingEventCoordinator>();
    
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object,
        navigationService: mockNavigationService.Object);

    // Act
    // Simulate navigate back event from ViewModel
    mockEventCoordinator.Raise(ec => ec.NavigateBackRequested += null, 
        (object)moodRecordingPage, EventArgs.Empty);

    // Assert
    mockNavigationService.Verify(ns => ns.GoBackAsync(), Times.Once);
}

[Test]
public void EventCoordination_Should_UnsubscribeFromEvents_WhenPageDisappears()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockEventCoordinator = new Mock<IMoodRecordingEventCoordinator>();
    var mockViewModel = new Mock<MoodRecordingViewModel>();
    
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object);

    // Act
    // Simulate OnDisappearing through reflection
    var onDisappearingMethod = typeof(MoodRecording).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(moodRecordingPage, null);

    // Assert
    mockEventCoordinator.Verify(ec => ec.Unsubscribe(It.IsAny<MoodRecordingViewModel>()), Times.Once);
}
```

#### Lifecycle Tests
```csharp
[Test]
public async Task Lifecycle_Should_CallViewModelRefreshMoodDataAsync_WhenAppearing()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockLifecycleManager = new Mock<IMoodRecordingLifecycleManager>();
    
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object);

    // Act
    // Simulate OnAppearing through reflection
    var onAppearingMethod = typeof(MoodRecording).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(moodRecordingPage, null);

    // Assert
    mockLifecycleManager.Verify(lm => lm.HandlePageAppearingAsync(), Times.Once);
}

[Test]
public async Task Lifecycle_Should_HandleRefreshErrors_WhenAppearing()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockViewModel = new Mock<MoodRecordingViewModel>();
    
    mockViewModel.Setup(vm => vm.RefreshMoodDataAsync())
        .ThrowsAsync(new InvalidOperationException("Test refresh error"));

    var lifecycleManager = new MoodRecordingLifecycleManager(
        mockViewModel.Object, mockLoggingService.Object);

    // Act
    await lifecycleManager.HandlePageAppearingAsync();

    // Assert
    mockLoggingService.Verify(ls => 
        ls.LogException(It.IsAny<Exception>(), 
            It.Is<string>(s => s.Contains("Error during MoodRecording page appearing"))), 
        Times.Once);
}
```

#### Service Integration Tests
```csharp
[Test]
public void ServiceIntegration_Should_PassAllServices_ToViewModelFactory()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    var mockLoggingService = new Mock<ILoggingService>();
    var mockViewModelFactory = new Mock<IMoodRecordingViewModelFactory>();
    var mockViewModel = new Mock<MoodRecordingViewModel>();
    
    mockViewModelFactory.Setup(vf => vf.CreateViewModel(
        It.IsAny<IMoodDataService>(),
        It.IsAny<IMoodDispatcherService>(),
        It.IsAny<ILoggingService>()))
        .Returns(mockViewModel.Object);

    // Act
    var moodRecordingPage = new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object,
        mockViewModelFactory.Object);

    // Assert
    mockViewModelFactory.Verify(vf => vf.CreateViewModel(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        mockLoggingService.Object), Times.Once);
}

[Test]
public void ServiceIntegration_Should_HandleNullLoggingService_InViewModel()
{
    // Arrange
    var mockMoodDataService = new Mock<IMoodDataService>();
    var mockDispatcherService = new Mock<IMoodDispatcherService>();
    ILoggingService? nullLoggingService = null;
    var mockViewModelFactory = new Mock<IMoodRecordingViewModelFactory>();
    var mockViewModel = new Mock<MoodRecordingViewModel>();
    
    mockViewModelFactory.Setup(vf => vf.CreateViewModel(
        It.IsAny<IMoodDataService>(),
        It.IsAny<IMoodDispatcherService>(),
        null))
        .Returns(mockViewModel.Object);

    // Act & Assert
    Assert.DoesNotThrow(() => new MoodRecording(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        nullLoggingService,
        mockViewModelFactory.Object));
    
    mockViewModelFactory.Verify(vf => vf.CreateViewModel(
        mockMoodDataService.Object,
        mockDispatcherService.Object,
        null), Times.Once);
}
```

#### Integration Tests
```csharp
[Test]
public void Integration_Should_HandleCompleteUserWorkflow_FromConstructionToNavigation()
{
    // Arrange
    var realMoodDataService = new Mock<IMoodDataService>().Object;
    var realDispatcherService = new Mock<IMoodDispatcherService>().Object;
    var realLoggingService = new Mock<ILoggingService>().Object;
    
    var moodRecordingPage = new MoodRecording(
        realMoodDataService, realDispatcherService, realLoggingService);

    // Act
    // Simulate page appearing
    var onAppearingMethod = typeof(MoodRecording).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(moodRecordingPage, null);
    
    // Simulate page disappearing
    var onDisappearingMethod = typeof(MoodRecording).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(moodRecordingPage, null);

    // Assert
    Assert.That(moodRecordingPage.BindingContext, Is.Not.Null);
    Assert.That(moodRecordingPage.BindingContext, Is.InstanceOf<MoodRecordingViewModel>());
}
```

### Test Fixtures Required
- **MoodDataServiceTestDouble** - Test double with configurable mood data responses
- **MoodDispatcherServiceTestDouble** - Test double for event scheduling and coordination
- **MoodRecordingViewModelMockFactory** - Create configured MoodRecordingViewModel mocks
- **NavigationServiceTestDouble** - Test double for navigation and error display testing
- **EventCoordinatorTestDouble** - Testable version of event coordination

## Success Criteria
- [ ] **Constructor validation** - All dependency injection scenarios tested including null handling
- [ ] **ViewModel creation** - Factory pattern tested with all service combinations
- [ ] **Event coordination** - ViewModel event subscription, handling, and cleanup verified
- [ ] **Navigation integration** - Error display and back navigation thoroughly tested
- [ ] **Lifecycle management** - OnAppearing/OnDisappearing behavior with error handling tested
- [ ] **Service integration** - All service dependencies and interface compliance verified
- [ ] **Memory management** - Event subscription/unsubscription patterns validated

## Implementation Priority
**HIGHEST PRIORITY** - Core value-delivery interface for the entire application. Critical user workflow for mood recording functionality. Represents primary user interaction patterns and complex form state management.

## Dependencies for Testing
- **IMoodDispatcherService interface** - Currently missing, needs to be extracted from concrete class
- **MAUI Form testing framework** - For complex data binding and command testing
- **Service mock framework** - For comprehensive service dependency testing
- **Event handling test tools** - For ViewModel event coordination testing
- **Lifecycle testing framework** - For page lifecycle behavior validation

## Implementation Estimate
**Effort: High (4-5 days)**
- Service interface extraction (IMoodDispatcherService) for concrete dependency
- ViewModel factory abstraction and testing
- Event coordinator abstraction with comprehensive event testing
- Navigation service integration testing with error scenarios
- Lifecycle manager testing with async operation coordination
- Complex data binding validation for 20+ mood selection buttons
- Form state management testing across multiple UI sections

This page represents the core value proposition of the application with complex form state management, making comprehensive testing critical for user experience and data integrity.