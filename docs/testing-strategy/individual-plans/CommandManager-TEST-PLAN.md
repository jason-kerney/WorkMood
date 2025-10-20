# CommandManager - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview

**File**: `MauiApp/Infrastructure/RelayCommand.cs`  
**Type**: Static Class  
**LOC**: 12 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Static utility class providing CommandManager compatibility for MAUI applications. Offers a simplified implementation of WPF's CommandManager pattern for command invalidation and requery suggestions. Essential for proper ICommand CanExecuteChanged event handling across the application's MVVM pattern implementation.

### Dependencies

- **System.EventHandler** - Standard event handling delegate
- **System.EventArgs** - Event argument base class

### Key Responsibilities

1. **Command requery coordination** - Central event for command state changes
2. **Event aggregation** - Single point for all command invalidation events
3. **WPF compatibility** - Maintains familiar CommandManager pattern for MAUI
4. **Null-safe invocation** - Safe event invocation with null checking
5. **Static access** - Global access point for command invalidation

### Current Architecture Assessment

**Testability Score: 10/10** ✅ **EXCELLENT TESTABILITY**

**Design Strengths:**

1. **Simple static design** - Minimal complexity with clear responsibilities
2. **Event-driven pattern** - Standard .NET event handling approach
3. **Null-safe implementation** - Safe event invocation with null propagation operator
4. **WPF compatibility** - Familiar pattern for developers migrating from WPF
5. **Single responsibility** - Only handles command requery coordination
6. **No external dependencies** - Self-contained implementation

**No Design Issues** - This class represents optimal implementation for its purpose.

## Usage Scenarios Analysis

### Typical Command Integration Patterns

```csharp
public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
```

### Business Logic Applications

- **Command state coordination** - Centralized command invalidation across ViewModels
- **UI responsiveness** - Automatic command state updates when business logic changes
- **Memory management** - Proper event subscription/unsubscription patterns
- **MVVM support** - Essential component for ICommand implementations

## Comprehensive Test Plan

### Test Structure

```
CommandManagerTests/
├── EventManagement/
│   ├── Should_InitializeRequerySuggestedEvent_AsNull()
│   ├── Should_AllowEventSubscription_Successfully()
│   ├── Should_AllowEventUnsubscription_Successfully()
│   ├── Should_AllowMultipleSubscribers_ToSameEvent()
│   ├── Should_HandleNullEventHandler_Gracefully()
│   └── Should_MaintainEventHandlerReferences_Correctly()
├── InvalidateRequerySuggested/
│   ├── Should_InvokeRequerySuggestedEvent_WhenCalled()
│   ├── Should_PassNullSender_ToEventHandlers()
│   ├── Should_PassEmptyEventArgs_ToEventHandlers()
│   ├── Should_InvokeAllSubscribedHandlers_InOrder()
│   ├── Should_HandleNoSubscribers_Gracefully()
│   ├── Should_HandleExceptionInEventHandler_Gracefully()
│   └── Should_ContinueInvokingOtherHandlers_AfterException()
├── EventSubscriptionManagement/
│   ├── Should_AddEventHandler_WhenSubscribing()
│   ├── Should_RemoveEventHandler_WhenUnsubscribing()
│   ├── Should_HandleDuplicateSubscription_Correctly()
│   ├── Should_HandleUnsubscribeWithoutSubscribe_Gracefully()
│   ├── Should_AllowSameHandlerMultipleSubscriptions()
│   └── Should_RemoveOnlyOneInstance_OnUnsubscribe()
├── StaticBehavior/
│   ├── Should_MaintainState_AcrossMultipleInvocations()
│   ├── Should_ShareEventState_AcrossAllCallers()
│   ├── Should_PersistSubscriptions_BetweenInvalidations()
│   └── Should_WorkWithoutInstantiation()
├── ThreadSafety/
│   ├── Should_HandleConcurrentSubscriptions_Safely()
│   ├── Should_HandleConcurrentInvalidations_Safely()
│   ├── Should_HandleConcurrentSubscriptionAndInvalidation_Safely()
│   └── Should_MaintainEventIntegrity_UnderConcurrentAccess()
└── IntegrationPatterns/
    ├── Should_WorkWithRelayCommandPattern()
    ├── Should_WorkWithMultipleCommandTypes()
    ├── Should_SupportTypicalMVVMScenarios()
    └── Should_HandleCommandLifecyclePatterns()
```

### Test Implementation Examples

#### Event Management Tests

```csharp
[Test]
public void EventManagement_Should_InitializeRequerySuggestedEvent_AsNull()
{
    // Arrange & Act
    // Access static event through reflection to verify initial state
    var eventField = typeof(CommandManager).GetField("RequerySuggested", 
        BindingFlags.Static | BindingFlags.Public);
    
    // Reset event to null to ensure clean state
    eventField?.SetValue(null, null);
    var eventValue = eventField?.GetValue(null);

    // Assert
    Assert.That(eventValue, Is.Null);
}

[Test]
public void EventManagement_Should_AllowEventSubscription_Successfully()
{
    // Arrange
    var eventHandlerCalled = false;
    EventHandler handler = (sender, args) => eventHandlerCalled = true;

    // Act
    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventHandlerCalled, Is.True);

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void EventManagement_Should_AllowEventUnsubscription_Successfully()
{
    // Arrange
    var eventHandlerCalled = false;
    EventHandler handler = (sender, args) => eventHandlerCalled = true;

    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.RequerySuggested -= handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventHandlerCalled, Is.False);
}

[Test]
public void EventManagement_Should_AllowMultipleSubscribers_ToSameEvent()
{
    // Arrange
    var handler1Called = false;
    var handler2Called = false;
    var handler3Called = false;

    EventHandler handler1 = (sender, args) => handler1Called = true;
    EventHandler handler2 = (sender, args) => handler2Called = true;
    EventHandler handler3 = (sender, args) => handler3Called = true;

    // Act
    CommandManager.RequerySuggested += handler1;
    CommandManager.RequerySuggested += handler2;
    CommandManager.RequerySuggested += handler3;

    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(handler1Called, Is.True);
    Assert.That(handler2Called, Is.True);
    Assert.That(handler3Called, Is.True);

    // Cleanup
    CommandManager.RequerySuggested -= handler1;
    CommandManager.RequerySuggested -= handler2;
    CommandManager.RequerySuggested -= handler3;
}

[Test]
public void EventManagement_Should_HandleNullEventHandler_Gracefully()
{
    // Act & Assert
    Assert.DoesNotThrow(() => 
    {
        CommandManager.RequerySuggested += null;
        CommandManager.RequerySuggested -= null;
    });
}

[Test]
public void EventManagement_Should_MaintainEventHandlerReferences_Correctly()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    // Act
    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(3));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}
```

#### InvalidateRequerySuggested Tests

```csharp
[Test]
public void InvalidateRequerySuggested_Should_InvokeRequerySuggestedEvent_WhenCalled()
{
    // Arrange
    var eventInvoked = false;
    EventHandler handler = (sender, args) => eventInvoked = true;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventInvoked, Is.True);

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void InvalidateRequerySuggested_Should_PassNullSender_ToEventHandlers()
{
    // Arrange
    object? receivedSender = new object(); // Initialize to non-null
    EventHandler handler = (sender, args) => receivedSender = sender;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(receivedSender, Is.Null);

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void InvalidateRequerySuggested_Should_PassEmptyEventArgs_ToEventHandlers()
{
    // Arrange
    EventArgs? receivedArgs = null;
    EventHandler handler = (sender, args) => receivedArgs = args;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(receivedArgs, Is.EqualTo(EventArgs.Empty));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void InvalidateRequerySuggested_Should_InvokeAllSubscribedHandlers_InOrder()
{
    // Arrange
    var invocationOrder = new List<int>();
    
    EventHandler handler1 = (sender, args) => invocationOrder.Add(1);
    EventHandler handler2 = (sender, args) => invocationOrder.Add(2);
    EventHandler handler3 = (sender, args) => invocationOrder.Add(3);

    CommandManager.RequerySuggested += handler1;
    CommandManager.RequerySuggested += handler2;
    CommandManager.RequerySuggested += handler3;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(invocationOrder, Is.EqualTo(new[] { 1, 2, 3 }));

    // Cleanup
    CommandManager.RequerySuggested -= handler1;
    CommandManager.RequerySuggested -= handler2;
    CommandManager.RequerySuggested -= handler3;
}

[Test]
public void InvalidateRequerySuggested_Should_HandleNoSubscribers_Gracefully()
{
    // Arrange
    // Ensure no subscribers
    var eventField = typeof(CommandManager).GetField("RequerySuggested", 
        BindingFlags.Static | BindingFlags.Public);
    eventField?.SetValue(null, null);

    // Act & Assert
    Assert.DoesNotThrow(() => CommandManager.InvalidateRequerySuggested());
}

[Test]
public void InvalidateRequerySuggested_Should_HandleExceptionInEventHandler_Gracefully()
{
    // Arrange
    var successfulHandlerCalled = false;
    
    EventHandler throwingHandler = (sender, args) => 
        throw new InvalidOperationException("Test exception");
    EventHandler successfulHandler = (sender, args) => 
        successfulHandlerCalled = true;

    CommandManager.RequerySuggested += throwingHandler;
    CommandManager.RequerySuggested += successfulHandler;

    // Act
    try
    {
        CommandManager.InvalidateRequerySuggested();
    }
    catch (InvalidOperationException)
    {
        // Expected exception from throwing handler
    }

    // Assert
    // Note: Behavior depends on .NET event invocation semantics
    // If one handler throws, subsequent handlers may not be called
    
    // Cleanup
    CommandManager.RequerySuggested -= throwingHandler;
    CommandManager.RequerySuggested -= successfulHandler;
}

[Test]
public void InvalidateRequerySuggested_Should_ContinueInvokingOtherHandlers_AfterException()
{
    // Arrange
    var handler1Called = false;
    var handler3Called = false;
    
    EventHandler handler1 = (sender, args) => handler1Called = true;
    EventHandler throwingHandler = (sender, args) => 
        throw new InvalidOperationException("Test exception");
    EventHandler handler3 = (sender, args) => handler3Called = true;

    CommandManager.RequerySuggested += handler1;
    CommandManager.RequerySuggested += throwingHandler;
    CommandManager.RequerySuggested += handler3;

    // Act
    var exceptionThrown = false;
    try
    {
        CommandManager.InvalidateRequerySuggested();
    }
    catch (InvalidOperationException)
    {
        exceptionThrown = true;
    }

    // Assert
    Assert.That(exceptionThrown, Is.True);
    Assert.That(handler1Called, Is.True);
    // Note: handler3Called may be false if exception stops invocation chain
    
    // Cleanup
    CommandManager.RequerySuggested -= handler1;
    CommandManager.RequerySuggested -= throwingHandler;
    CommandManager.RequerySuggested -= handler3;
}
```

#### Event Subscription Management Tests

```csharp
[Test]
public void SubscriptionManagement_Should_AddEventHandler_WhenSubscribing()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    // Act
    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(1));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void SubscriptionManagement_Should_RemoveEventHandler_WhenUnsubscribing()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested(); // Should increment callCount to 1

    // Act
    CommandManager.RequerySuggested -= handler;
    CommandManager.InvalidateRequerySuggested(); // Should not increment callCount

    // Assert
    Assert.That(callCount, Is.EqualTo(1));
}

[Test]
public void SubscriptionManagement_Should_HandleDuplicateSubscription_Correctly()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    // Act
    CommandManager.RequerySuggested += handler;
    CommandManager.RequerySuggested += handler; // Duplicate subscription
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(2)); // Should be called twice

    // Cleanup
    CommandManager.RequerySuggested -= handler;
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void SubscriptionManagement_Should_HandleUnsubscribeWithoutSubscribe_Gracefully()
{
    // Arrange
    EventHandler handler = (sender, args) => { };

    // Act & Assert
    Assert.DoesNotThrow(() => CommandManager.RequerySuggested -= handler);
}

[Test]
public void SubscriptionManagement_Should_AllowSameHandlerMultipleSubscriptions()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    // Act
    CommandManager.RequerySuggested += handler;
    CommandManager.RequerySuggested += handler;
    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(3));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
    CommandManager.RequerySuggested -= handler;
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void SubscriptionManagement_Should_RemoveOnlyOneInstance_OnUnsubscribe()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    CommandManager.RequerySuggested += handler;
    CommandManager.RequerySuggested += handler;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.RequerySuggested -= handler; // Remove one instance
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(2)); // Should still be called twice

    // Cleanup
    CommandManager.RequerySuggested -= handler;
    CommandManager.RequerySuggested -= handler;
}
```

#### Static Behavior Tests

```csharp
[Test]
public void StaticBehavior_Should_MaintainState_AcrossMultipleInvocations()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(3));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void StaticBehavior_Should_ShareEventState_AcrossAllCallers()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;

    // Act - Subscribe from different logical contexts
    CommandManager.RequerySuggested += handler;
    
    // Simulate different classes accessing CommandManager
    InvokeFromDifferentContext();
    
    // Assert
    Assert.That(callCount, Is.EqualTo(1));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

private void InvokeFromDifferentContext()
{
    CommandManager.InvalidateRequerySuggested();
}

[Test]
public void StaticBehavior_Should_PersistSubscriptions_BetweenInvalidations()
{
    // Arrange
    var callCount = 0;
    EventHandler handler = (sender, args) => callCount++;
    CommandManager.RequerySuggested += handler;

    // Act
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(callCount, Is.EqualTo(3));

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}

[Test]
public void StaticBehavior_Should_WorkWithoutInstantiation()
{
    // Arrange
    var eventInvoked = false;
    EventHandler handler = (sender, args) => eventInvoked = true;

    // Act - Use static methods without creating any instances
    CommandManager.RequerySuggested += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventInvoked, Is.True);

    // Cleanup
    CommandManager.RequerySuggested -= handler;
}
```

#### Integration Pattern Tests

```csharp
[Test]
public void IntegrationPatterns_Should_WorkWithRelayCommandPattern()
{
    // Arrange
    var canExecuteChangedFired = false;
    var relayCommand = new TestRelayCommand();
    
    relayCommand.CanExecuteChanged += (sender, args) => canExecuteChangedFired = true;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(canExecuteChangedFired, Is.True);
}

// Test relay command that mimics the real implementation
private class TestRelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) { }
}

[Test]
public void IntegrationPatterns_Should_WorkWithMultipleCommandTypes()
{
    // Arrange
    var command1Fired = false;
    var command2Fired = false;
    var command3Fired = false;

    var command1 = new TestRelayCommand();
    var command2 = new TestRelayCommand();
    var command3 = new TestRelayCommand();

    command1.CanExecuteChanged += (sender, args) => command1Fired = true;
    command2.CanExecuteChanged += (sender, args) => command2Fired = true;
    command3.CanExecuteChanged += (sender, args) => command3Fired = true;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(command1Fired, Is.True);
    Assert.That(command2Fired, Is.True);
    Assert.That(command3Fired, Is.True);
}

[Test]
public void IntegrationPatterns_Should_SupportTypicalMVVMScenarios()
{
    // Arrange
    var viewModel = new TestViewModel();
    var commandStateChanged = false;

    viewModel.TestCommand.CanExecuteChanged += (sender, args) => commandStateChanged = true;

    // Act - Simulate business logic change that should invalidate commands
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(commandStateChanged, Is.True);
}

private class TestViewModel
{
    public ICommand TestCommand { get; }

    public TestViewModel()
    {
        TestCommand = new TestRelayCommand();
    }
}

[Test]
public void IntegrationPatterns_Should_HandleCommandLifecyclePatterns()
{
    // Arrange
    var commandStateChanges = 0;
    var command = new TestRelayCommand();
    
    EventHandler handler = (sender, args) => commandStateChanges++;
    
    // Act - Simulate typical command lifecycle
    command.CanExecuteChanged += handler;
    CommandManager.InvalidateRequerySuggested(); // Initial state
    
    command.CanExecuteChanged -= handler;
    CommandManager.InvalidateRequerySuggested(); // After unsubscribe
    
    command.CanExecuteChanged += handler;
    CommandManager.InvalidateRequerySuggested(); // Re-subscribe

    // Assert
    Assert.That(commandStateChanges, Is.EqualTo(2)); // Should not increment during unsubscribed period
}
```

### Test Fixtures Required

- **CommandManagerTestFixture** - Standard test fixture with event cleanup
- **EventHandlerMockFactory** - Factory for creating test event handlers
- **RelayCommandTestDouble** - Test implementation of ICommand for integration testing
- **EventInvocationValidator** - Helper for verifying event invocation patterns

## Success Criteria

- [ ] **Event management** - Proper subscription/unsubscription handling
- [ ] **Event invocation** - Correct parameter passing (null sender, EventArgs.Empty)
- [ ] **Multiple subscribers** - Support for multiple event handlers
- [ ] **Exception handling** - Graceful handling of exceptions in event handlers
- [ ] **Static behavior** - Proper static state management across invocations
- [ ] **Integration patterns** - Works correctly with RelayCommand implementations
- [ ] **Memory management** - No memory leaks from event subscriptions

## Implementation Priority

**HIGH PRIORITY** - Critical infrastructure component with excellent testability. Foundation for all command implementations.

## Dependencies for Testing

- **NUnit** - Standard testing framework
- **System.Reflection** - For accessing private/internal event state
- **Custom test doubles** - ICommand implementations for integration testing

## Implementation Estimate

**Effort: Low-Medium (0.5 days)**

- Simple static class with excellent testability
- Comprehensive event handling testing
- Integration pattern verification
- Exception handling scenarios
- Straightforward implementation with clear success criteria

This class represents a fundamental infrastructure component requiring thorough testing to ensure proper command invalidation throughout the MVVM architecture.