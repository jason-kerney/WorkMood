# BasePage - Individual Test Plan

## Class Overview

**File**: `MauiApp/Infrastructure/BasePage.cs`  
**Type**: Abstract Base Class (ContentPage)  
**LOC**: 128 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Foundational abstract base class that provides common functionality for all pages in the WorkMood application. Implements Template Method pattern for consistent page lifecycle management, centralized error handling, resource cleanup, and dependency injection patterns. Essential architectural component supporting the entire page hierarchy.

### Dependencies

- **Microsoft.Maui.Controls.ContentPage** - MAUI page base class
- **WorkMood.MauiApp.Services.INavigationService** - Navigation service interface
- **WorkMood.MauiApp.Services.NavigationService** - Default navigation implementation
- **System.Collections.Generic** - Subscription management collections
- **System.Threading.Tasks** - Async/await operations

### Key Responsibilities

1. **Navigation service injection** - Constructor injection with fallback default
2. **Lifecycle management** - Template methods for OnAppearing/OnDisappearing
3. **Subscription tracking** - Automatic disposal of IDisposable resources
4. **Error handling** - Centralized error management with navigation integration
5. **Template method pattern** - Extensible initialization and cleanup hooks
6. **Memory leak prevention** - Automatic subscription cleanup on page disposal

### Current Architecture Assessment

**Testability Score: 6/10** ⚠️ **REQUIRES MODERATE REFACTORING**

**Design Strengths:**

1. **Clear separation of concerns** - Navigation, lifecycle, error handling separated
2. **Template method pattern** - Excellent extensibility with virtual methods
3. **Dependency injection** - Constructor injection for INavigationService
4. **Resource management** - Automatic subscription cleanup prevents memory leaks
5. **Error handling centralization** - Consistent error management across pages
6. **Null-safe defaults** - Fallback NavigationService if none provided

**Design Challenges Impacting Testability:**

1. **Abstract class testing complexity** - Requires concrete test implementations
2. **MAUI lifecycle dependency** - OnAppearing/OnDisappearing tied to MAUI framework
3. **Exception handling side effects** - Debug.WriteLine calls during error scenarios
4. **Navigation service default coupling** - Creates NavigationService in constructor
5. **Mixed sync/async patterns** - OnAppearing async, OnDisappearing sync
6. **Virtual method dependency** - Testing requires method override verification

**Recommended Refactoring for Testability:**

1. **Extract lifecycle interface** - Create IPageLifecycle for testing
2. **Inject logger service** - Replace Debug.WriteLine with testable logging
3. **Add lifecycle events** - Expose events for lifecycle state changes
4. **Factory pattern for defaults** - Remove direct NavigationService construction

## Usage Scenarios Analysis

### Typical Page Inheritance Patterns

```csharp
public partial class MainPage : BasePage
{
    public MainPage(INavigationService navigationService, MainPageViewModel viewModel) 
        : base(navigationService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        InitializeBasePage();
    }

    protected override async Task OnPageAppearingAsync()
    {
        await base.OnPageAppearingAsync();
        // Page-specific initialization
    }
}
```

### Business Logic Applications

- **Page lifecycle consistency** - Uniform appearing/disappearing behavior
- **Navigation service access** - Centralized navigation across all pages
- **Resource cleanup** - Automatic subscription disposal prevents memory leaks
- **Error handling** - Consistent error management and user feedback
- **Template customization** - Override virtual methods for page-specific behavior

## Comprehensive Test Plan

### Test Structure

```
BasePageTests/
├── Construction/
│   ├── Should_AcceptNavigationService_WhenProvided()
│   ├── Should_CreateDefaultNavigationService_WhenNullProvided()
│   ├── Should_InitializeSubscriptionsList_Always()
│   └── Should_InheritFromContentPage_Correctly()
├── LifecycleManagement/
│   ├── OnAppearing/
│   │   ├── Should_CallBaseOnAppearing_Always()
│   │   ├── Should_CallOnPageAppearingAsync_WhenOverridden()
│   │   ├── Should_HandleOnPageAppearingException_Gracefully()
│   │   └── Should_CallHandleErrorAsync_OnException()
│   ├── OnDisappearing/
│   │   ├── Should_CallBaseOnDisappearing_Always()
│   │   ├── Should_CallOnPageDisappearing_WhenOverridden()
│   │   ├── Should_DisposeAllSubscriptions_OnDisappearing()
│   │   ├── Should_ClearSubscriptionsList_OnDisappearing()
│   │   └── Should_HandleOnPageDisappearingException_Gracefully()
│   └── VirtualMethods/
│       ├── Should_ReturnCompletedTask_FromDefaultOnPageAppearingAsync()
│       ├── Should_ExecuteSuccessfully_FromDefaultOnPageDisappearing()
│       ├── Should_CallSetupViewModelBindings_FromInitializeBasePage()
│       └── Should_CallSetupEventSubscriptions_FromInitializeBasePage()
├── SubscriptionManagement/
│   ├── Should_AddSubscription_ToInternalList()
│   ├── Should_HandleNullSubscription_Gracefully()
│   ├── Should_DisposeAllSubscriptions_OnPageDisappearing()
│   ├── Should_ClearSubscriptionsList_AfterDisposal()
│   ├── Should_HandleSubscriptionDisposeException_Gracefully()
│   └── Should_AllowMultipleSubscriptions_FromSameSource()
├── ErrorHandling/
│   ├── Should_CallNavigationServiceShowError_FromHandleErrorAsync()
│   ├── Should_PassMessageAndException_ToNavigationService()
│   ├── Should_HandleNullException_InHandleErrorAsync()
│   ├── Should_PropagateNavigationServiceException_WhenErrorHandlingFails()
│   └── Should_WriteDebugMessage_OnOnDisappearingException()
├── NavigationServiceIntegration/
│   ├── Should_ExposeNavigationService_AsProtectedProperty()
│   ├── Should_UseProvidedNavigationService_WhenNotNull()
│   ├── Should_CreateDefaultNavigationService_WhenNull()
│   ├── Should_PassCurrentPageInstance_ToDefaultNavigationService()
│   └── Should_MaintainNavigationServiceReference_ThroughoutLifecycle()
├── TemplateMethodPattern/
│   ├── Should_CallSetupMethods_FromInitializeBasePage()
│   ├── Should_AllowOverride_OfSetupViewModelBindings()
│   ├── Should_AllowOverride_OfSetupEventSubscriptions()
│   ├── Should_AllowOverride_OfOnPageAppearingAsync()
│   ├── Should_AllowOverride_OfOnPageDisappearing()
│   └── Should_AllowOverride_OfHandleErrorAsync()
└── AbstractClassTesting/
    ├── ConcreteTestImplementation/
    │   ├── Should_InstantiateSuccessfully_WithTestImplementation()
    │   ├── Should_CallOverriddenMethods_Correctly()
    │   └── Should_MaintainBaseClassBehavior_InTestImplementation()
    └── MockingPatterns/
        ├── Should_MockNavigationService_Successfully()
        ├── Should_VerifyMethodCalls_OnMockedDependencies()
        └── Should_TestVirtualMethodOverrides_WithMocks()
```

### Test Implementation Examples

#### Construction Tests

```csharp
// Concrete test implementation for abstract class testing
public class TestBasePage : BasePage
{
    public TestBasePage(INavigationService? navigationService = null) 
        : base(navigationService) { }

    // Expose protected members for testing
    public new INavigationService NavigationService => base.NavigationService;
    public new void InitializeBasePage() => base.InitializeBasePage();
    public new Task HandleErrorAsync(string message, Exception? exception = null) 
        => base.HandleErrorAsync(message, exception);
    public new void AddSubscription(IDisposable subscription) 
        => base.AddSubscription(subscription);

    // Override virtual methods for testing
    public Task<bool> OnPageAppearingAsyncCalled { get; private set; }
    public bool OnPageDisappearingCalled { get; private set; }
    public bool SetupViewModelBindingsCalled { get; private set; }
    public bool SetupEventSubscriptionsCalled { get; private set; }

    protected override async Task OnPageAppearingAsync()
    {
        OnPageAppearingAsyncCalled = true;
        await base.OnPageAppearingAsync();
    }

    protected override void OnPageDisappearing()
    {
        OnPageDisappearingCalled = true;
        base.OnPageDisappearing();
    }

    protected override void SetupViewModelBindings()
    {
        SetupViewModelBindingsCalled = true;
        base.SetupViewModelBindings();
    }

    protected override void SetupEventSubscriptions()
    {
        SetupEventSubscriptionsCalled = true;
        base.SetupEventSubscriptions();
    }
}

[Test]
public void Construction_Should_AcceptNavigationService_WhenProvided()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();

    // Act
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Assert
    Assert.That(basePage.NavigationService, Is.EqualTo(mockNavigationService.Object));
    Assert.That(basePage.NavigationService, Is.Not.Null);
}

[Test]
public void Construction_Should_CreateDefaultNavigationService_WhenNullProvided()
{
    // Act
    var basePage = new TestBasePage(null);

    // Assert
    Assert.That(basePage.NavigationService, Is.Not.Null);
    Assert.That(basePage.NavigationService, Is.InstanceOf<NavigationService>());
}

[Test]
public void Construction_Should_InitializeSubscriptionsList_Always()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var mockSubscription = new Mock<IDisposable>();

    // Act
    basePage.AddSubscription(mockSubscription.Object);

    // Assert - Should not throw, indicating internal list is initialized
    Assert.DoesNotThrow(() => basePage.AddSubscription(mockSubscription.Object));
}

[Test]
public void Construction_Should_InheritFromContentPage_Correctly()
{
    // Arrange & Act
    var basePage = new TestBasePage();

    // Assert
    Assert.That(basePage, Is.InstanceOf<ContentPage>());
    Assert.That(basePage, Is.InstanceOf<BasePage>());
}
```

#### Lifecycle Management Tests

```csharp
[Test]
public async Task OnAppearing_Should_CallBaseOnAppearing_Always()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var baseOnAppearingCalled = false;

    // Note: Testing protected OnAppearing requires reflection or test helper
    var onAppearingMethod = typeof(BasePage).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act
    onAppearingMethod?.Invoke(basePage, null);

    // Allow async method to complete
    await Task.Delay(100);

    // Assert
    Assert.That(basePage.OnPageAppearingAsyncCalled, Is.True);
}

[Test]
public async Task OnAppearing_Should_CallOnPageAppearingAsync_WhenOverridden()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Use reflection to call protected OnAppearing
    var onAppearingMethod = typeof(BasePage).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act
    onAppearingMethod?.Invoke(basePage, null);
    await Task.Delay(100); // Allow async completion

    // Assert
    Assert.That(basePage.OnPageAppearingAsyncCalled, Is.True);
}

[Test]
public async Task OnAppearing_Should_HandleOnPageAppearingException_Gracefully()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new ThrowingTestBasePage(mockNavigationService.Object);
    
    var onAppearingMethod = typeof(BasePage).GetMethod("OnAppearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act & Assert
    Assert.DoesNotThrow(() => onAppearingMethod?.Invoke(basePage, null));
    
    // Verify error handling was called
    await Task.Delay(100);
    mockNavigationService.Verify(x => x.ShowErrorAsync(
        It.Is<string>(s => s.Contains("Failed to initialize page")), 
        It.IsAny<Exception>()), Times.Once);
}

// Test implementation that throws in OnPageAppearingAsync
public class ThrowingTestBasePage : TestBasePage
{
    public ThrowingTestBasePage(INavigationService navigationService) : base(navigationService) { }

    protected override async Task OnPageAppearingAsync()
    {
        await Task.CompletedTask;
        throw new InvalidOperationException("Test exception");
    }
}

[Test]
public void OnDisappearing_Should_CallBaseOnDisappearing_Always()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act
    onDisappearingMethod?.Invoke(basePage, null);

    // Assert
    Assert.That(basePage.OnPageDisappearingCalled, Is.True);
}

[Test]
public void OnDisappearing_Should_DisposeAllSubscriptions_OnDisappearing()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    
    var mockSubscription1 = new Mock<IDisposable>();
    var mockSubscription2 = new Mock<IDisposable>();
    var mockSubscription3 = new Mock<IDisposable>();

    basePage.AddSubscription(mockSubscription1.Object);
    basePage.AddSubscription(mockSubscription2.Object);
    basePage.AddSubscription(mockSubscription3.Object);

    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act
    onDisappearingMethod?.Invoke(basePage, null);

    // Assert
    mockSubscription1.Verify(x => x.Dispose(), Times.Once);
    mockSubscription2.Verify(x => x.Dispose(), Times.Once);
    mockSubscription3.Verify(x => x.Dispose(), Times.Once);
}

[Test]
public void OnDisappearing_Should_HandleOnPageDisappearingException_Gracefully()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new ThrowingOnDisappearingTestBasePage(mockNavigationService.Object);

    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act & Assert
    Assert.DoesNotThrow(() => onDisappearingMethod?.Invoke(basePage, null));
}

public class ThrowingOnDisappearingTestBasePage : TestBasePage
{
    public ThrowingOnDisappearingTestBasePage(INavigationService navigationService) 
        : base(navigationService) { }

    protected override void OnPageDisappearing()
    {
        throw new InvalidOperationException("Test exception in OnPageDisappearing");
    }
}
```

#### Subscription Management Tests

```csharp
[Test]
public void AddSubscription_Should_AddSubscription_ToInternalList()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var mockSubscription = new Mock<IDisposable>();

    // Act
    basePage.AddSubscription(mockSubscription.Object);

    // Trigger disposal through OnDisappearing
    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod?.Invoke(basePage, null);

    // Assert
    mockSubscription.Verify(x => x.Dispose(), Times.Once);
}

[Test]
public void AddSubscription_Should_HandleNullSubscription_Gracefully()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Act & Assert
    Assert.DoesNotThrow(() => basePage.AddSubscription(null!));
}

[Test]
public void SubscriptionManagement_Should_HandleSubscriptionDisposeException_Gracefully()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    
    var throwingSubscription = new Mock<IDisposable>();
    throwingSubscription.Setup(x => x.Dispose())
        .Throws(new InvalidOperationException("Dispose failed"));

    basePage.AddSubscription(throwingSubscription.Object);

    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act & Assert
    Assert.DoesNotThrow(() => onDisappearingMethod?.Invoke(basePage, null));
}

[Test]
public void SubscriptionManagement_Should_AllowMultipleSubscriptions_FromSameSource()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    
    var mockSubscription = new Mock<IDisposable>();

    // Act
    basePage.AddSubscription(mockSubscription.Object);
    basePage.AddSubscription(mockSubscription.Object);
    basePage.AddSubscription(mockSubscription.Object);

    // Trigger disposal
    var onDisappearingMethod = typeof(BasePage).GetMethod("OnDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    onDisappearingMethod?.Invoke(basePage, null);

    // Assert
    mockSubscription.Verify(x => x.Dispose(), Times.Exactly(3));
}
```

#### Error Handling Tests

```csharp
[Test]
public async Task HandleErrorAsync_Should_CallNavigationServiceShowError_Always()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var errorMessage = "Test error message";
    var exception = new InvalidOperationException("Test exception");

    // Act
    await basePage.HandleErrorAsync(errorMessage, exception);

    // Assert
    mockNavigationService.Verify(x => x.ShowErrorAsync(errorMessage, exception), Times.Once);
}

[Test]
public async Task HandleErrorAsync_Should_PassMessageAndException_ToNavigationService()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var errorMessage = "Specific error message";
    var exception = new ArgumentException("Specific exception");

    // Act
    await basePage.HandleErrorAsync(errorMessage, exception);

    // Assert
    mockNavigationService.Verify(x => x.ShowErrorAsync(
        It.Is<string>(s => s == errorMessage),
        It.Is<Exception>(e => e == exception)), Times.Once);
}

[Test]
public async Task HandleErrorAsync_Should_HandleNullException_Gracefully()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);
    var errorMessage = "Test error message";

    // Act
    await basePage.HandleErrorAsync(errorMessage, null);

    // Assert
    mockNavigationService.Verify(x => x.ShowErrorAsync(errorMessage, null), Times.Once);
}

[Test]
public async Task HandleErrorAsync_Should_PropagateNavigationServiceException_WhenErrorHandlingFails()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    mockNavigationService.Setup(x => x.ShowErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()))
        .ThrowsAsync(new InvalidOperationException("Navigation service failed"));
    
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => basePage.HandleErrorAsync("Test message", null));
    
    Assert.That(exception.Message, Is.EqualTo("Navigation service failed"));
}
```

#### Template Method Pattern Tests

```csharp
[Test]
public void InitializeBasePage_Should_CallSetupMethods_InCorrectOrder()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Act
    basePage.InitializeBasePage();

    // Assert
    Assert.That(basePage.SetupViewModelBindingsCalled, Is.True);
    Assert.That(basePage.SetupEventSubscriptionsCalled, Is.True);
}

[Test]
public void TemplateMethod_Should_AllowOverride_OfSetupViewModelBindings()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new CustomSetupTestBasePage(mockNavigationService.Object);

    // Act
    basePage.InitializeBasePage();

    // Assert
    Assert.That(basePage.CustomSetupViewModelBindingsCalled, Is.True);
}

[Test]
public void TemplateMethod_Should_AllowOverride_OfSetupEventSubscriptions()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new CustomSetupTestBasePage(mockNavigationService.Object);

    // Act
    basePage.InitializeBasePage();

    // Assert
    Assert.That(basePage.CustomSetupEventSubscriptionsCalled, Is.True);
}

public class CustomSetupTestBasePage : TestBasePage
{
    public bool CustomSetupViewModelBindingsCalled { get; private set; }
    public bool CustomSetupEventSubscriptionsCalled { get; private set; }

    public CustomSetupTestBasePage(INavigationService navigationService) : base(navigationService) { }

    protected override void SetupViewModelBindings()
    {
        CustomSetupViewModelBindingsCalled = true;
        base.SetupViewModelBindings();
    }

    protected override void SetupEventSubscriptions()
    {
        CustomSetupEventSubscriptionsCalled = true;
        base.SetupEventSubscriptions();
    }
}
```

#### Virtual Method Tests

```csharp
[Test]
public async Task VirtualMethods_Should_ReturnCompletedTask_FromDefaultOnPageAppearingAsync()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Use reflection to access the protected method
    var method = typeof(BasePage).GetMethod("OnPageAppearingAsync", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act
    var result = method?.Invoke(basePage, null) as Task;

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.IsCompleted, Is.True);
    Assert.That(result.Status, Is.EqualTo(TaskStatus.RanToCompletion));
}

[Test]
public void VirtualMethods_Should_ExecuteSuccessfully_FromDefaultOnPageDisappearing()
{
    // Arrange
    var mockNavigationService = new Mock<INavigationService>();
    var basePage = new TestBasePage(mockNavigationService.Object);

    // Use reflection to access the protected method
    var method = typeof(BasePage).GetMethod("OnPageDisappearing", 
        BindingFlags.NonPublic | BindingFlags.Instance);

    // Act & Assert
    Assert.DoesNotThrow(() => method?.Invoke(basePage, null));
}
```

### Test Fixtures Required

- **BasePageTestFixture** - Standard test fixture with mocked dependencies
- **ConcreteBasePageImplementation** - Concrete test class for abstract testing
- **NavigationServiceMockFactory** - Factory for consistent navigation service mocks
- **SubscriptionMockFactory** - Factory for IDisposable subscription mocks
- **ReflectionTestHelper** - Helper for accessing protected methods in tests

## Success Criteria

- [ ] **Construction behavior** - Proper dependency injection and default handling
- [ ] **Lifecycle management** - OnAppearing/OnDisappearing with exception handling
- [ ] **Subscription management** - Automatic resource cleanup and disposal
- [ ] **Error handling** - Centralized error management through navigation service
- [ ] **Template method pattern** - Virtual method override functionality
- [ ] **Navigation service integration** - Proper service injection and usage
- [ ] **Memory leak prevention** - Subscription disposal and cleanup verification

## Implementation Priority

**HIGH PRIORITY** - Critical architectural foundation for all pages. Moderate testability requiring careful abstract class testing patterns.

## Dependencies for Testing

- **NUnit** - Standard testing framework
- **Moq** - Mocking framework for INavigationService and IDisposable
- **System.Reflection** - For accessing protected methods in tests
- **Microsoft.Maui.Controls** - MAUI framework dependencies

## Implementation Estimate

**Effort: Medium-High (1.5 days)**

- Complex abstract class testing patterns required
- Extensive lifecycle management testing
- Template method pattern verification
- Error handling and exception scenarios
- Navigation service integration testing
- Resource management and memory leak prevention testing

This class represents the architectural foundation requiring comprehensive testing to ensure robust page lifecycle management across the entire application.