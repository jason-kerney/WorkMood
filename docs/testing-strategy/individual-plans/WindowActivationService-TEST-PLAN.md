# WindowActivationService - Individual Test Plan

## Class Overview
**File**: `MauiApp/Services/WindowActivationService.cs`  
**Type**: Platform-specific Infrastructure Service  
**LOC**: 142 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Platform-specific service responsible for window activation and bringing the application window to the foreground across different platforms (Windows, Android, iOS, macOS). Implements complex Windows API calls for reliable window activation.

### Dependencies
- `ILoggingService` (injected) - for error handling and operation logging
- MAUI Framework: `MainThread`, `Application.Current`, `Window.Handler`
- Platform-specific APIs: Windows API P/Invoke calls, platform-specific activation methods

### Key Responsibilities
1. **Cross-platform window activation** - ActivateCurrentWindowAsync() with platform detection
2. **Windows API integration** - Direct Windows API calls for reliable activation
3. **Error handling** - Comprehensive exception handling with logging
4. **Platform-specific implementations** - Placeholder methods for Android, iOS, macOS

### Current Architecture Assessment
**Testability Score: 3/10** ⚠️ **REQUIRES SIGNIFICANT REFACTORING**

**Major Testing Challenges:**
1. **Platform-specific code** - Heavy use of compiler directives (#if WINDOWS)
2. **Static API calls** - Direct calls to MainThread.InvokeOnMainThreadAsync, Application.Current
3. **Windows P/Invoke** - Native API calls to SetForegroundWindow, ShowWindow, SetWindowPos
4. **Platform handlers** - Direct access to window.Handler?.PlatformView
5. **Complex conditional logic** - Platform detection and fallback mechanisms

## Required Refactoring Strategy

### Phase 1: Extract Platform Abstractions
Create platform-specific abstractions to isolate native API calls:

```csharp
public interface IPlatformWindowActivator
{
    Task ActivateWindowAsync(Window window);
    bool SupportsCurrentPlatform { get; }
}

public interface IWindowProvider
{
    Window? GetCurrentWindow();
}

public interface IMainThreadInvoker
{
    Task InvokeOnMainThreadAsync(Action action);
    Task InvokeOnMainThreadAsync(Func<Task> asyncAction);
}
```

### Phase 2: Extract Windows API Wrapper
Create abstraction for Windows-specific P/Invoke calls:

```csharp
public interface IWindowsApiWrapper
{
    bool SetForegroundWindow(IntPtr hWnd);
    bool ShowWindow(IntPtr hWnd, int nCmdShow);
    bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    IntPtr GetWindowHandle(object winUIWindow);
}
```

### Phase 3: Refactored Architecture
Transform service into coordinator with injected platform abstractions:

```csharp
public class WindowActivationService : IWindowActivationService
{
    private readonly ILoggingService _loggingService;
    private readonly IMainThreadInvoker _mainThreadInvoker;
    private readonly IWindowProvider _windowProvider;
    private readonly IEnumerable<IPlatformWindowActivator> _platformActivators;
    
    public async Task ActivateCurrentWindowAsync()
    {
        await _mainThreadInvoker.InvokeOnMainThreadAsync(async () =>
        {
            var window = _windowProvider.GetCurrentWindow();
            var activator = _platformActivators.FirstOrDefault(a => a.SupportsCurrentPlatform);
            await activator?.ActivateWindowAsync(window);
        });
    }
}
```

## Comprehensive Test Plan

### Test Structure
```
WindowActivationServiceTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenLoggingServiceIsNull()
│   ├── Should_ThrowArgumentNullException_WhenMainThreadInvokerIsNull()
│   ├── Should_ThrowArgumentNullException_WhenWindowProviderIsNull()
│   └── Should_ThrowArgumentNullException_WhenPlatformActivatorsIsNull()
├── ActivateCurrentWindowAsync/
│   ├── HappyPath/
│   │   ├── Should_InvokeMainThread_WhenActivatingWindow()
│   │   ├── Should_GetCurrentWindow_FromWindowProvider()
│   │   ├── Should_FindSupportedActivator_FromPlatformActivators()
│   │   ├── Should_CallActivateWindowAsync_OnSupportedPlatformActivator()
│   │   └── Should_LogSuccessfulActivation_WhenWindowActivated()
│   ├── EdgeCases/
│   │   ├── Should_HandleGracefully_WhenCurrentWindowIsNull()
│   │   ├── Should_HandleGracefully_WhenNoPlatformActivatorSupported()
│   │   ├── Should_HandleGracefully_WhenActivatorListIsEmpty()
│   │   └── Should_LogWarning_WhenNoWindowAvailable()
│   └── ErrorHandling/
│   │   ├── Should_CatchAndLogException_WhenMainThreadInvokeFails()
│   │   ├── Should_CatchAndLogException_WhenWindowProviderThrows()
│   │   ├── Should_CatchAndLogException_WhenActivatorThrows()
│   │   └── Should_NotRethrowException_WhenActivationFails()
│   └── Platform Integration/
│       ├── Should_SelectWindowsActivator_WhenOnWindowsPlatform()
│       ├── Should_SelectAndroidActivator_WhenOnAndroidPlatform()
│       ├── Should_SelectMacActivator_WhenOnMacPlatform()
│       └── Should_SelectiOSActivator_WhenOniOSPlatform()
```

### Platform-Specific Activator Tests

#### WindowsPlatformActivatorTests
```csharp
WindowsPlatformActivatorTests/
├── Constructor/
│   ├── Should_ThrowArgumentNullException_WhenWindowsApiWrapperIsNull()
│   └── Should_ThrowArgumentNullException_WhenLoggingServiceIsNull()
├── SupportsCurrentPlatform/
│   ├── Should_ReturnTrue_WhenRunningOnWindows()
│   └── Should_ReturnFalse_WhenRunningOnOtherPlatforms()
├── ActivateWindowAsync/
│   ├── HappyPath/
│   │   ├── Should_CallWinUIWindowActivate_WhenHandlerIsWinUIWindow()
│   │   ├── Should_GetWindowHandle_FromWinUIWindow()
│   │   ├── Should_CallSetForegroundWindow_WithCorrectHandle()
│   │   ├── Should_CallShowWindow_WithRestoreFlag()
│   │   ├── Should_CallSetWindowPos_ToMakeTopmost()
│   │   ├── Should_CallSetWindowPos_ToRemoveTopmostFlag()
│   │   └── Should_LogSuccessfulActivation_WhenAllCallsSucceed()
│   ├── EdgeCases/
│   │   ├── Should_HandleGracefully_WhenWindowHandlerIsNull()
│   │   ├── Should_HandleGracefully_WhenPlatformViewIsNotWinUIWindow()
│   │   ├── Should_HandleGracefully_WhenWindowHandleIsZero()
│   │   └── Should_LogWarning_WhenWindowCannotBeActivated()
│   └── ErrorHandling/
│       ├── Should_CatchAndLogException_WhenWinUIActivateThrows()
│       ├── Should_CatchAndLogException_WhenGetWindowHandleThrows()
│       ├── Should_CatchAndLogException_WhenWindowsApiCallThrows()
│       └── Should_CompleteTask_EvenWhenExceptionOccurs()
```

### Test Implementation Examples

#### Constructor Validation Tests
```csharp
[Test]
public void Constructor_Should_ThrowArgumentNullException_WhenLoggingServiceIsNull()
{
    // Arrange
    ILoggingService? nullLoggingService = null;
    var mockMainThreadInvoker = new Mock<IMainThreadInvoker>();
    var mockWindowProvider = new Mock<IWindowProvider>();
    var mockActivators = new List<IPlatformWindowActivator>();

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        new WindowActivationService(nullLoggingService!, mockMainThreadInvoker.Object, 
                                   mockWindowProvider.Object, mockActivators));
    
    Assert.That(exception.ParamName, Is.EqualTo("loggingService"));
}
```

#### Main Activation Flow Tests
```csharp
[Test]
public async Task ActivateCurrentWindowAsync_Should_InvokeMainThread_WhenActivatingWindow()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockMainThreadInvoker = new Mock<IMainThreadInvoker>();
    var mockWindowProvider = new Mock<IWindowProvider>();
    var mockWindow = new Mock<Window>();
    var mockActivator = new Mock<IPlatformWindowActivator>();
    
    mockWindowProvider.Setup(x => x.GetCurrentWindow()).Returns(mockWindow.Object);
    mockActivator.Setup(x => x.SupportsCurrentPlatform).Returns(true);
    
    var activators = new List<IPlatformWindowActivator> { mockActivator.Object };
    var service = new WindowActivationService(mockLoggingService.Object, 
                                            mockMainThreadInvoker.Object,
                                            mockWindowProvider.Object, activators);

    // Setup MainThread to execute the action immediately
    mockMainThreadInvoker.Setup(x => x.InvokeOnMainThreadAsync(It.IsAny<Func<Task>>()))
                        .Returns((Func<Task> action) => action());

    // Act
    await service.ActivateCurrentWindowAsync();

    // Assert
    mockMainThreadInvoker.Verify(x => x.InvokeOnMainThreadAsync(It.IsAny<Func<Task>>()), Times.Once);
    mockWindowProvider.Verify(x => x.GetCurrentWindow(), Times.Once);
    mockActivator.Verify(x => x.ActivateWindowAsync(mockWindow.Object), Times.Once);
}
```

#### Platform Selection Tests
```csharp
[Test]
public async Task ActivateCurrentWindowAsync_Should_SelectCorrectActivator_BasedOnPlatformSupport()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockMainThreadInvoker = new Mock<IMainThreadInvoker>();
    var mockWindowProvider = new Mock<IWindowProvider>();
    var mockWindow = new Mock<Window>();
    
    var mockWindowsActivator = new Mock<IPlatformWindowActivator>();
    var mockMacActivator = new Mock<IPlatformWindowActivator>();
    
    mockWindowProvider.Setup(x => x.GetCurrentWindow()).Returns(mockWindow.Object);
    mockWindowsActivator.Setup(x => x.SupportsCurrentPlatform).Returns(false);
    mockMacActivator.Setup(x => x.SupportsCurrentPlatform).Returns(true);
    
    var activators = new List<IPlatformWindowActivator> { mockWindowsActivator.Object, mockMacActivator.Object };
    var service = new WindowActivationService(mockLoggingService.Object, 
                                            mockMainThreadInvoker.Object,
                                            mockWindowProvider.Object, activators);

    mockMainThreadInvoker.Setup(x => x.InvokeOnMainThreadAsync(It.IsAny<Func<Task>>()))
                        .Returns((Func<Task> action) => action());

    // Act
    await service.ActivateCurrentWindowAsync();

    // Assert
    mockWindowsActivator.Verify(x => x.ActivateWindowAsync(It.IsAny<Window>()), Times.Never);
    mockMacActivator.Verify(x => x.ActivateWindowAsync(mockWindow.Object), Times.Once);
}
```

### Test Fixtures Required
- **MockWindowFactory** - Create mock Window objects with configurable handlers
- **TestMainThreadInvoker** - Synchronous implementation for testing
- **TestPlatformActivators** - Configurable activators for different test scenarios
- **WindowsApiMockWrapper** - Mock Windows API calls with configurable return values

### Integration Test Considerations
1. **Platform-specific test assemblies** - Separate test projects for each platform
2. **Conditional compilation** - Platform-specific tests using compiler directives  
3. **Mock platform detection** - Testable platform identification logic
4. **Handler simulation** - Mock MAUI platform handlers for different platforms

## Success Criteria
- [ ] **100% line coverage** for public methods after refactoring
- [ ] **95% branch coverage** for all conditional logic
- [ ] **Platform isolation** - No platform-specific code in main service
- [ ] **Comprehensive error handling** - All exceptions caught and logged appropriately
- [ ] **Mock-based testing** - No dependencies on actual platform APIs during testing
- [ ] **Integration verification** - Platform-specific activators work correctly on their target platforms

## Implementation Priority
**HIGH PRIORITY** - Core infrastructure service used by MainPageViewModel for window management. Platform abstraction enables reliable testing across different deployment targets.

## Dependencies for Testing
- Mock implementations for all platform abstractions
- Test-specific MainThread invoker (synchronous execution)
- Platform detection mocking capabilities
- Windows API wrapper with testable interface

## Refactoring Estimate
**Effort: High (3-5 days)**
- Platform abstraction layer creation
- Windows API wrapper implementation  
- Platform-specific activator implementations
- Comprehensive test suite creation
- Integration testing across platforms

This refactoring will significantly improve testability while maintaining the complex platform-specific functionality required for reliable window activation.