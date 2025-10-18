# Settings (Page) - Individual Test Plan

## Class Overview
**File**: `MauiApp/Pages/Settings.xaml.cs`  
**Type**: MAUI ContentPage (MVVM Configuration Interface)  
**LOC**: 97 lines (code-behind)  
**XAML LOC**: 321 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Configuration interface for managing mood tracking schedule settings including daily reminder times, date-specific overrides, and schedule preview functionality. Features dual constructor patterns, sophisticated TimePicker integration, dynamic UI state management, and complex data binding patterns for schedule configuration management.

### Dependencies
- **ScheduleConfigService** (concrete class injected) - Schedule configuration persistence and validation
- **SettingsPageViewModel** (created) - Business logic for settings management and validation
- **NavigationService** (created) - Navigation coordination and user feedback
- **MAUI Framework**: ContentPage, TimePicker, DatePicker, CollectionView, Switch controls

### Key Responsibilities
1. **Dual constructor support** - Primary DI constructor and parameterless fallback constructor
2. **ViewModel creation** - Creates SettingsPageViewModel with injected dependencies
3. **TimePicker event coordination** - Handles TimePicker PropertyChanged events for two-way binding
4. **UI synchronization** - Syncs TimePicker and DatePicker values with ViewModel properties
5. **Lifecycle management** - OnAppearing loads configuration and wires events, OnDisappearing cleanup
6. **Event subscription management** - Manages PropertyChanged event subscription/unsubscription
7. **Data binding coordination** - Coordinates complex form state with ViewModel properties

### Current Architecture Assessment
**Testability Score: 6/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Design Challenges:**
1. **Concrete service dependency** - ScheduleConfigService injected as concrete class (though interface exists)
2. **ViewModel creation in constructor** - Creates ViewModel directly rather than injecting it
3. **TimePicker binding workarounds** - Manual PropertyChanged event handling for TimePicker limitations
4. **Dual constructor complexity** - Two constructor patterns create testing complexity
5. **Direct UI control access** - References named TimePicker and DatePicker controls directly
6. **Mixed abstraction levels** - NavigationService created, but could be injected

**Good Design Elements:**
1. **Clean MVVM separation** - Delegates business logic to ViewModel appropriately
2. **Interface-compatible dependency** - ScheduleConfigService implements IScheduleConfigService
3. **Proper lifecycle management** - OnAppearing/OnDisappearing with event subscription cleanup
4. **Memory leak prevention** - Proper event unsubscription patterns
5. **Null safety** - Proper PropertyChangedEventArgs property name checking
6. **Fallback constructor** - Supports both DI and non-DI scenarios

## XAML Structure Analysis

### UI Components (321 lines XAML)
1. **Header section** - Settings title and configuration description
2. **Morning reminder section** - Start of work time configuration with TimePicker
3. **Evening reminder section** - End of work time configuration with TimePicker
4. **Schedule overrides section** - CollectionView for existing overrides with edit/delete actions
5. **New override section** - Complex form with DatePicker, Switches, and conditional TimePickers
6. **Preview section** - Live preview of current schedule configuration
7. **Action buttons** - Save and Cancel commands

### Data Bindings
- **Basic Schedule**: `{Binding MorningTime}`, `{Binding EveningTime}` (with manual sync)
- **Override Management**: `{Binding ExistingOverrides}` CollectionView with item templates
- **New Override Form**: `{Binding IsNewOverrideEnabled}`, `{Binding NewOverrideDate}`, etc.
- **Toggle States**: `{Binding HasNewMorningOverride}`, `{Binding HasNewEveningOverride}`
- **Preview Text**: `{Binding MorningPreviewText}`, `{Binding EveningPreviewText}`
- **Commands**: `{Binding SaveCommand}`, `{Binding CancelCommand}`, override management commands

### Complex UI Patterns
- **Conditional Visibility** - TimePickers shown/hidden based on Switch states
- **CollectionView Templates** - Dynamic override items with embedded commands
- **Command Parameter Binding** - Edit/Delete commands with override item parameters
- **Resource References** - x:Reference SettingsRef for command binding context

### Command Bindings
- **SaveCommand** - Saves schedule configuration
- **CancelCommand** - Cancels changes and navigates back
- **EditOverrideCommand** - Edits existing schedule override
- **DeleteOverrideCommand** - Removes schedule override

## Required Refactoring Strategy

### Phase 1: Extract ViewModel Factory
Create abstraction for ViewModel creation to enable testing:

```csharp
public interface ISettingsPageViewModelFactory
{
    SettingsPageViewModel CreateViewModel(IScheduleConfigService scheduleConfigService, 
                                         INavigationService navigationService);
}

public class SettingsPageViewModelFactory : ISettingsPageViewModelFactory
{
    public SettingsPageViewModel CreateViewModel(IScheduleConfigService scheduleConfigService, 
                                                INavigationService navigationService)
    {
        return new SettingsPageViewModel(scheduleConfigService, navigationService);
    }
}
```

### Phase 2: Extract TimePicker Coordinator
Create abstraction for TimePicker event handling and synchronization:

```csharp
public interface ITimePickerCoordinator
{
    void SubscribeToTimePickers(TimePicker morningPicker, TimePicker eveningPicker);
    void UnsubscribeFromTimePickers(TimePicker morningPicker, TimePicker eveningPicker);
    void SyncTimePickersWithViewModel(SettingsPageViewModel viewModel, 
                                     TimePicker morningPicker, 
                                     TimePicker eveningPicker);
    event EventHandler<TimePickerChangedEventArgs> MorningTimeChanged;
    event EventHandler<TimePickerChangedEventArgs> EveningTimeChanged;
}

public class TimePickerCoordinator : ITimePickerCoordinator
{
    public event EventHandler<TimePickerChangedEventArgs> MorningTimeChanged;
    public event EventHandler<TimePickerChangedEventArgs> EveningTimeChanged;
    
    public void SubscribeToTimePickers(TimePicker morningPicker, TimePicker eveningPicker)
    {
        morningPicker.PropertyChanged += OnMorningTimePickerChanged;
        eveningPicker.PropertyChanged += OnEveningTimePickerChanged;
    }
    
    public void UnsubscribeFromTimePickers(TimePicker morningPicker, TimePicker eveningPicker)
    {
        morningPicker.PropertyChanged -= OnMorningTimePickerChanged;
        eveningPicker.PropertyChanged -= OnEveningTimePickerChanged;
    }
    
    public void SyncTimePickersWithViewModel(SettingsPageViewModel viewModel,
                                           TimePicker morningPicker,
                                           TimePicker eveningPicker)
    {
        morningPicker.Time = viewModel.MorningTime;
        eveningPicker.Time = viewModel.EveningTime;
    }
    
    private void OnMorningTimePickerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time) && sender is TimePicker picker)
        {
            MorningTimeChanged?.Invoke(this, new TimePickerChangedEventArgs(picker.Time));
        }
    }
    
    private void OnEveningTimePickerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time) && sender is TimePicker picker)
        {
            EveningTimeChanged?.Invoke(this, new TimePickerChangedEventArgs(picker.Time));
        }
    }
}
```

### Phase 3: Extract UI Synchronization Manager
Create abstraction for all UI control synchronization:

```csharp
public interface ISettingsUISynchronizer
{
    void SyncAllControlsWithViewModel(SettingsPageViewModel viewModel);
    void SyncTimePickersWithViewModel(SettingsPageViewModel viewModel);
    void SyncDatePickersWithViewModel(SettingsPageViewModel viewModel);
}

public class SettingsUISynchronizer : ISettingsUISynchronizer
{
    private readonly TimePicker _morningTimePicker;
    private readonly TimePicker _eveningTimePicker;
    private readonly DatePicker _newOverrideDatePicker;
    private readonly TimePicker _newOverrideMorningTimePicker;
    private readonly TimePicker _newOverrideEveningTimePicker;
    
    public SettingsUISynchronizer(TimePicker morningTimePicker,
                                TimePicker eveningTimePicker,
                                DatePicker newOverrideDatePicker,
                                TimePicker newOverrideMorningTimePicker,
                                TimePicker newOverrideEveningTimePicker)
    {
        _morningTimePicker = morningTimePicker;
        _eveningTimePicker = eveningTimePicker;
        _newOverrideDatePicker = newOverrideDatePicker;
        _newOverrideMorningTimePicker = newOverrideMorningTimePicker;
        _newOverrideEveningTimePicker = newOverrideEveningTimePicker;
    }
    
    public void SyncAllControlsWithViewModel(SettingsPageViewModel viewModel)
    {
        SyncTimePickersWithViewModel(viewModel);
        SyncDatePickersWithViewModel(viewModel);
    }
    
    public void SyncTimePickersWithViewModel(SettingsPageViewModel viewModel)
    {
        _morningTimePicker.Time = viewModel.MorningTime;
        _eveningTimePicker.Time = viewModel.EveningTime;
        _newOverrideMorningTimePicker.Time = viewModel.NewOverrideMorningTime;
        _newOverrideEveningTimePicker.Time = viewModel.NewOverrideEveningTime;
    }
    
    public void SyncDatePickersWithViewModel(SettingsPageViewModel viewModel)
    {
        _newOverrideDatePicker.Date = viewModel.NewOverrideDate;
    }
}
```

### Phase 4: Extract Lifecycle Manager
Create abstraction for page lifecycle operations:

```csharp
public interface ISettingsPageLifecycleManager
{
    Task HandlePageAppearingAsync();
    void HandlePageDisappearing();
}

public class SettingsPageLifecycleManager : ISettingsPageLifecycleManager
{
    private readonly SettingsPageViewModel _viewModel;
    private readonly ITimePickerCoordinator _timePickerCoordinator;
    private readonly ISettingsUISynchronizer _uiSynchronizer;
    private readonly TimePicker _morningTimePicker;
    private readonly TimePicker _eveningTimePicker;
    
    public SettingsPageLifecycleManager(SettingsPageViewModel viewModel,
                                       ITimePickerCoordinator timePickerCoordinator,
                                       ISettingsUISynchronizer uiSynchronizer,
                                       TimePicker morningTimePicker,
                                       TimePicker eveningTimePicker)
    {
        _viewModel = viewModel;
        _timePickerCoordinator = timePickerCoordinator;
        _uiSynchronizer = uiSynchronizer;
        _morningTimePicker = morningTimePicker;
        _eveningTimePicker = eveningTimePicker;
    }
    
    public async Task HandlePageAppearingAsync()
    {
        await _viewModel.LoadConfigurationAsync();
        _timePickerCoordinator.SubscribeToTimePickers(_morningTimePicker, _eveningTimePicker);
        _uiSynchronizer.SyncAllControlsWithViewModel(_viewModel);
    }
    
    public void HandlePageDisappearing()
    {
        _timePickerCoordinator.UnsubscribeFromTimePickers(_morningTimePicker, _eveningTimePicker);
    }
}
```

### Phase 5: Refactored Architecture
Transform to testable design with full dependency injection:

```csharp
public partial class Settings : ContentPage
{
    private readonly SettingsPageViewModel _viewModel;
    private readonly ITimePickerCoordinator _timePickerCoordinator;
    private readonly ISettingsPageLifecycleManager _lifecycleManager;
    private readonly ISettingsUISynchronizer _uiSynchronizer;

    public Settings(IScheduleConfigService scheduleConfigService,
                    ISettingsPageViewModelFactory viewModelFactory = null,
                    INavigationService navigationService = null,
                    ITimePickerCoordinator timePickerCoordinator = null)
    {
        InitializeComponent();
        
        // Create or inject dependencies
        var factory = viewModelFactory ?? new SettingsPageViewModelFactory();
        var navService = navigationService ?? new NavigationService(this);
        _viewModel = factory.CreateViewModel(scheduleConfigService, navService);
        BindingContext = _viewModel;
        
        // Create coordination layers
        _timePickerCoordinator = timePickerCoordinator ?? new TimePickerCoordinator();
        _uiSynchronizer = new SettingsUISynchronizer(
            MorningTimePicker, EveningTimePicker, NewOverrideDatePicker,
            NewOverrideMorningTimePicker, NewOverrideEveningTimePicker);
        _lifecycleManager = new SettingsPageLifecycleManager(
            _viewModel, _timePickerCoordinator, _uiSynchronizer,
            MorningTimePicker, EveningTimePicker);
        
        // Wire up TimePicker events to ViewModel
        _timePickerCoordinator.MorningTimeChanged += (s, e) => _viewModel.MorningTime = e.Time;
        _timePickerCoordinator.EveningTimeChanged += (s, e) => _viewModel.EveningTime = e.Time;
    }

    public Settings() : this(new ScheduleConfigService())
    {
        // Fallback constructor for design-time or when DI is not available
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
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
SettingsPageTests/
├── Constructor/
│   ├── Primary/
│   │   ├── Should_ThrowArgumentNullException_WhenScheduleConfigServiceIsNull()
│   │   ├── Should_CreateViewModel_WithProvidedScheduleConfigService()
│   │   ├── Should_SetBindingContext_ToCreatedViewModel()
│   │   ├── Should_CreateTimePickerCoordinator_WhenNotProvided()
│   │   ├── Should_UseProvidedTimePickerCoordinator_WhenProvided()
│   │   ├── Should_CreateNavigationService_WhenNotProvided()
│   │   └── Should_WireUpTimePickerEvents_ToViewModelProperties()
│   └── Fallback/
│       ├── Should_CreateDefaultScheduleConfigService_WhenParameterless()
│       ├── Should_CallPrimaryConstructor_WithDefaultService()
│       └── Should_InitializeAllDependencies_InFallbackMode()
├── ViewModelCreation/
│   ├── Should_UseProvidedViewModelFactory_WhenProvided()
│   ├── Should_CreateDefaultViewModelFactory_WhenNotProvided()
│   ├── Should_PassScheduleConfigService_ToViewModelFactory()
│   ├── Should_PassNavigationService_ToViewModelFactory()
│   └── Should_HandleViewModelCreationErrors_Gracefully()
├── TimePickerCoordination/
│   ├── Should_SubscribeToMorningTimePicker_WhenPageAppears()
│   ├── Should_SubscribeToEveningTimePicker_WhenPageAppears()
│   ├── Should_UpdateViewModelMorningTime_WhenMorningPickerChanges()
│   ├── Should_UpdateViewModelEveningTime_WhenEveningPickerChanges()
│   ├── Should_IgnoreNonTimePropertyChanges_FromTimePickers()
│   ├── Should_UnsubscribeFromTimePickers_WhenPageDisappears()
│   └── Should_HandleTimePickerEventErrors_Gracefully()
├── UISynchronization/
│   ├── Should_SyncMorningTimePicker_WithViewModelOnAppearing()
│   ├── Should_SyncEveningTimePicker_WithViewModelOnAppearing()
│   ├── Should_SyncNewOverrideDatePicker_WithViewModelOnAppearing()
│   ├── Should_SyncNewOverrideMorningTimePicker_WithViewModelOnAppearing()
│   ├── Should_SyncNewOverrideEveningTimePicker_WithViewModelOnAppearing()
│   ├── Should_SyncAllControls_WhenViewModelChanges()
│   └── Should_HandleUISyncErrors_Gracefully()
├── Lifecycle/
│   ├── OnAppearing/
│   │   ├── Should_CallViewModelLoadConfigurationAsync_WhenAppearing()
│   │   ├── Should_SubscribeToTimePickerEvents_WhenAppearing()
│   │   ├── Should_SyncAllUIControls_WhenAppearing()
│   │   ├── Should_CallBaseOnAppearing_WhenAppearing()
│   │   └── Should_HandleAppearingErrors_Gracefully()
│   └── OnDisappearing/
│       ├── Should_UnsubscribeFromTimePickerEvents_WhenDisappearing()
│       ├── Should_CallBaseOnDisappearing_WhenDisappearing()
│       └── Should_HandleDisappearingErrors_Gracefully()
├── ServiceIntegration/
│   ├── Should_PassScheduleConfigService_ToViewModel()
│   ├── Should_UseScheduleConfigServiceInterface_ForDependency()
│   ├── Should_HandleScheduleConfigServiceErrors_Gracefully()
│   └── Should_VerifyServiceInterface_IsRespected()
└── ErrorHandling/
    ├── Should_LogErrors_WhenErrorsOccur()
    ├── Should_HandleViewModelErrors_Gracefully()
    ├── Should_HandleTimePickerErrors_Gracefully()
    ├── Should_HandleUISynchronizationErrors_Gracefully()
    └── Should_HandleLifecycleErrors_Gracefully()
```

### Test Implementation Examples

#### Constructor Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenScheduleConfigServiceIsNull()
{
    // Arrange
    IScheduleConfigService? nullScheduleConfigService = null;

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new Settings(nullScheduleConfigService!));
    Assert.That(exception.ParamName, Is.EqualTo("scheduleConfigService"));
}

[Test]
public void Constructor_Should_CreateViewModel_WithProvidedScheduleConfigService()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockViewModelFactory = new Mock<ISettingsPageViewModelFactory>();
    var mockNavigationService = new Mock<INavigationService>();
    var mockViewModel = new Mock<SettingsPageViewModel>();
    
    mockViewModelFactory.Setup(vf => vf.CreateViewModel(
        mockScheduleConfigService.Object,
        mockNavigationService.Object))
        .Returns(mockViewModel.Object);

    // Act
    var settingsPage = new Settings(
        mockScheduleConfigService.Object,
        mockViewModelFactory.Object,
        mockNavigationService.Object);

    // Assert
    mockViewModelFactory.Verify(vf => vf.CreateViewModel(
        mockScheduleConfigService.Object,
        mockNavigationService.Object), Times.Once);
    Assert.That(settingsPage.BindingContext, Is.SameAs(mockViewModel.Object));
}

[Test]
public void Constructor_Should_WireUpTimePickerEvents_ToViewModelProperties()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockTimePickerCoordinator = new Mock<ITimePickerCoordinator>();
    var testTime = new TimeSpan(9, 30, 0);
    
    var settingsPage = new Settings(
        mockScheduleConfigService.Object,
        timePickerCoordinator: mockTimePickerCoordinator.Object);

    // Act
    // Simulate morning time change event
    mockTimePickerCoordinator.Raise(tpc => tpc.MorningTimeChanged += null,
        new TimePickerChangedEventArgs(testTime));

    // Assert
    var viewModel = settingsPage.BindingContext as SettingsPageViewModel;
    Assert.That(viewModel.MorningTime, Is.EqualTo(testTime));
}
```

#### TimePicker Coordination Tests
```csharp
[Test]
public void TimePickerCoordination_Should_UpdateViewModelMorningTime_WhenMorningPickerChanges()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var realTimePickerCoordinator = new TimePickerCoordinator();
    var testTimePicker = new TimePicker();
    var testTime = new TimeSpan(8, 15, 0);
    
    var settingsPage = new Settings(
        mockScheduleConfigService.Object,
        timePickerCoordinator: realTimePickerCoordinator);
    
    var viewModel = settingsPage.BindingContext as SettingsPageViewModel;

    // Act
    realTimePickerCoordinator.SubscribeToTimePickers(testTimePicker, new TimePicker());
    testTimePicker.Time = testTime;

    // Assert
    Assert.That(viewModel.MorningTime, Is.EqualTo(testTime));
}

[Test]
public void TimePickerCoordination_Should_IgnoreNonTimePropertyChanges_FromTimePickers()
{
    // Arrange
    var timePickerCoordinator = new TimePickerCoordinator();
    var testTimePicker = new TimePicker();
    var initialTime = new TimeSpan(9, 0, 0);
    var eventFired = false;
    
    timePickerCoordinator.MorningTimeChanged += (s, e) => eventFired = true;
    timePickerCoordinator.SubscribeToTimePickers(testTimePicker, new TimePicker());
    testTimePicker.Time = initialTime;
    eventFired = false; // Reset

    // Act
    testTimePicker.BackgroundColor = Colors.Red; // Non-Time property change

    // Assert
    Assert.That(eventFired, Is.False);
}

[Test]
public void TimePickerCoordination_Should_UnsubscribeFromTimePickers_WhenPageDisappears()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockTimePickerCoordinator = new Mock<ITimePickerCoordinator>();
    
    var settingsPage = new Settings(
        mockScheduleConfigService.Object,
        timePickerCoordinator: mockTimePickerCoordinator.Object);

    // Act
    // Simulate OnDisappearing through reflection
    var onDisappearingMethod = typeof(Settings).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(settingsPage, null);

    // Assert
    mockTimePickerCoordinator.Verify(tpc => 
        tpc.UnsubscribeFromTimePickers(It.IsAny<TimePicker>(), It.IsAny<TimePicker>()), 
        Times.Once);
}
```

#### UI Synchronization Tests
```csharp
[Test]
public void UISynchronization_Should_SyncAllControls_WhenViewModelChanges()
{
    // Arrange
    var mockMorningTimePicker = new Mock<TimePicker>();
    var mockEveningTimePicker = new Mock<TimePicker>();
    var mockDatePicker = new Mock<DatePicker>();
    var mockMorningOverridePicker = new Mock<TimePicker>();
    var mockEveningOverridePicker = new Mock<TimePicker>();
    
    var uiSynchronizer = new SettingsUISynchronizer(
        mockMorningTimePicker.Object,
        mockEveningTimePicker.Object,
        mockDatePicker.Object,
        mockMorningOverridePicker.Object,
        mockEveningOverridePicker.Object);
    
    var mockViewModel = new Mock<SettingsPageViewModel>();
    var testMorningTime = new TimeSpan(8, 30, 0);
    var testEveningTime = new TimeSpan(17, 45, 0);
    mockViewModel.Setup(vm => vm.MorningTime).Returns(testMorningTime);
    mockViewModel.Setup(vm => vm.EveningTime).Returns(testEveningTime);

    // Act
    uiSynchronizer.SyncAllControlsWithViewModel(mockViewModel.Object);

    // Assert
    mockMorningTimePicker.VerifySet(tp => tp.Time = testMorningTime, Times.Once);
    mockEveningTimePicker.VerifySet(tp => tp.Time = testEveningTime, Times.Once);
}

[Test]
public void UISynchronization_Should_HandleUISyncErrors_Gracefully()
{
    // Arrange
    var faultyTimePicker = new Mock<TimePicker>();
    faultyTimePicker.SetupSet(tp => tp.Time = It.IsAny<TimeSpan>())
        .Throws(new InvalidOperationException("UI sync error"));
    
    var uiSynchronizer = new SettingsUISynchronizer(
        faultyTimePicker.Object,
        new TimePicker(),
        new DatePicker(),
        new TimePicker(),
        new TimePicker());
    
    var mockViewModel = new Mock<SettingsPageViewModel>();

    // Act & Assert
    Assert.DoesNotThrow(() => 
        uiSynchronizer.SyncAllControlsWithViewModel(mockViewModel.Object));
}
```

#### Lifecycle Tests
```csharp
[Test]
public async Task Lifecycle_Should_CallViewModelLoadConfigurationAsync_WhenAppearing()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockLifecycleManager = new Mock<ISettingsPageLifecycleManager>();
    
    var settingsPage = new Settings(mockScheduleConfigService.Object);

    // Act
    // Simulate OnAppearing through reflection
    var onAppearingMethod = typeof(Settings).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(settingsPage, null);

    // Assert
    mockLifecycleManager.Verify(lm => lm.HandlePageAppearingAsync(), Times.Once);
}

[Test]
public async Task Lifecycle_Should_HandleAppearingErrors_Gracefully()
{
    // Arrange
    var mockScheduleConfigService = new Mock<IScheduleConfigService>();
    var mockViewModel = new Mock<SettingsPageViewModel>();
    var mockTimePickerCoordinator = new Mock<ITimePickerCoordinator>();
    var mockUISynchronizer = new Mock<ISettingsUISynchronizer>();
    
    mockViewModel.Setup(vm => vm.LoadConfigurationAsync())
        .ThrowsAsync(new InvalidOperationException("Configuration load error"));

    var lifecycleManager = new SettingsPageLifecycleManager(
        mockViewModel.Object,
        mockTimePickerCoordinator.Object,
        mockUISynchronizer.Object,
        new TimePicker(),
        new TimePicker());

    // Act & Assert
    Assert.DoesNotThrowAsync(async () => 
        await lifecycleManager.HandlePageAppearingAsync());
}
```

#### Integration Tests
```csharp
[Test]
public void Integration_Should_HandleCompleteSettingsWorkflow_FromConstructionToSave()
{
    // Arrange
    var realScheduleConfigService = new Mock<IScheduleConfigService>().Object;
    
    var settingsPage = new Settings(realScheduleConfigService);

    // Act
    // Simulate page appearing
    var onAppearingMethod = typeof(Settings).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onAppearingMethod.Invoke(settingsPage, null);
    
    // Simulate user interaction - change morning time
    var viewModel = settingsPage.BindingContext as SettingsPageViewModel;
    viewModel.MorningTime = new TimeSpan(9, 0, 0);
    
    // Simulate page disappearing
    var onDisappearingMethod = typeof(Settings).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod.Invoke(settingsPage, null);

    // Assert
    Assert.That(settingsPage.BindingContext, Is.Not.Null);
    Assert.That(settingsPage.BindingContext, Is.InstanceOf<SettingsPageViewModel>());
    Assert.That(viewModel.MorningTime, Is.EqualTo(new TimeSpan(9, 0, 0)));
}
```

### Test Fixtures Required
- **ScheduleConfigServiceTestDouble** - Test double with configurable schedule data
- **SettingsPageViewModelMockFactory** - Create configured SettingsPageViewModel mocks
- **TimePickerTestDouble** - Testable version of TimePicker for event testing
- **UISynchronizerTestDouble** - Test double for UI synchronization behavior
- **NavigationServiceTestDouble** - Test double for navigation behavior verification

## Success Criteria
- [ ] **Constructor validation** - Both DI and fallback constructor patterns tested
- [ ] **ViewModel creation** - Factory pattern tested with service dependencies
- [ ] **TimePicker coordination** - PropertyChanged event handling and synchronization verified
- [ ] **UI synchronization** - All control sync patterns tested including error scenarios
- [ ] **Lifecycle management** - OnAppearing/OnDisappearing behavior with async operations tested
- [ ] **Service integration** - IScheduleConfigService dependency properly tested
- [ ] **Event management** - Subscription/unsubscription patterns validated

## Implementation Priority
**HIGH PRIORITY** - Critical configuration interface for application scheduling functionality. Represents complex form management patterns and TimePicker integration challenges used throughout the application.

## Dependencies for Testing
- **MAUI Controls test framework** - For TimePicker, DatePicker, and CollectionView testing
- **Event handling test tools** - For PropertyChanged event coordination testing
- **Service mock framework** - For IScheduleConfigService dependency testing
- **UI binding testing framework** - For complex data binding validation
- **Async lifecycle testing tools** - For page lifecycle behavior with async operations

## Implementation Estimate
**Effort: High (4-5 days)**
- ViewModel factory abstraction and testing
- TimePicker coordinator abstraction with comprehensive event testing
- UI synchronization manager testing across multiple control types
- Lifecycle manager testing with async configuration loading
- Complex data binding validation for schedule override management
- CollectionView template testing for dynamic override items
- Dual constructor pattern testing scenarios

This page represents sophisticated configuration management with complex TimePicker integration patterns, making comprehensive testing critical for user settings persistence and schedule management reliability.