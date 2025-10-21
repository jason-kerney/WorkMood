# RelayCommand - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview

**File**: `MauiApp/Infrastructure/RelayCommand.cs`  
**Type**: ICommand Implementation  
**LOC**: 35 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Concrete implementation of ICommand pattern providing a flexible, reusable command wrapper for MVVM applications. Encapsulates action delegates with optional parameter support and conditional execution capabilities. Essential component for binding UI actions to ViewModel logic with proper MAUI/WPF compatibility.

### Dependencies

- **System.Windows.Input.ICommand** - Standard command interface
- **WorkMood.MauiApp.Infrastructure.CommandManager** - Static command coordination
- **System.Action** - Action delegates for execution
- **System.Predicate** - Conditional execution delegates

### Key Responsibilities

1. **Action encapsulation** - Wraps parameterless and parameterized actions
2. **Conditional execution** - Supports CanExecute logic with predicate delegates
3. **Event coordination** - Integrates with CommandManager for state change notifications
4. **Parameter handling** - Flexible parameter support for UI binding scenarios
5. **MVVM integration** - Provides standard ICommand interface for data binding
6. **Constructor overloads** - Multiple construction patterns for different use cases

### Current Architecture Assessment

**Testability Score: 9/10** ✅ **EXCELLENT TESTABILITY**

**Design Strengths:**

1. **Clear dependency injection** - Constructor injection for action and predicate delegates
2. **Null-safe implementation** - Comprehensive null checking with proper exceptions
3. **Multiple construction patterns** - Overloaded constructors for different scenarios
4. **Standard interface compliance** - Proper ICommand implementation
5. **Event integration** - Proper CommandManager integration for coordination
6. **Immutable delegate references** - Private readonly fields prevent external mutation

**Minor Design Consideration:**

1. **Parameter type flexibility** - object? parameter allows any type but requires runtime validation

**No Significant Design Issues** - This class represents excellent command implementation.

## Usage Scenarios Analysis

### Typical ViewModel Integration Patterns

```csharp
public class MyViewModel
{
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        DeleteCommand = new RelayCommand(param => Delete(param), param => CanDelete(param));
        RefreshCommand = new RelayCommand(Refresh); // No CanExecute
    }

    private void Save() { /* Implementation */ }
    private bool CanSave() => /* Condition */;
}
```

### Business Logic Applications

- **UI action binding** - Connect buttons and controls to ViewModel methods
- **Conditional command execution** - Enable/disable commands based on application state
- **Parameter passing** - Support parameterized commands from UI controls
- **Command coordination** - Participate in application-wide command state management

## Comprehensive Test Plan

### Test Structure

```
RelayCommandTests/
├── Construction/
│   ├── ActionConstructor/
│   │   ├── Should_AcceptValidAction_WithoutCanExecute()
│   │   ├── Should_AcceptValidAction_WithCanExecute()
│   │   ├── Should_ThrowArgumentNullException_WhenActionIsNull()
│   │   ├── Should_AcceptNullCanExecute_Gracefully()
│   │   └── Should_InitializePropertiesCorrectly()
│   ├── ParameterizedConstructor/
│   │   ├── Should_AcceptValidParameterizedAction_WithoutCanExecute()
│   │   ├── Should_AcceptValidParameterizedAction_WithCanExecute()
│   │   ├── Should_ThrowArgumentNullException_WhenParameterizedActionIsNull()
│   │   ├── Should_AcceptNullParameterizedCanExecute_Gracefully()
│   │   └── Should_InitializeParameterizedPropertiesCorrectly()
│   └── ConstructorOverloads/
│       ├── Should_ConvertParameterlessAction_ToParameterizedAction()
│       ├── Should_ConvertParameterlessCanExecute_ToParameterizedPredicate()
│       ├── Should_HandleNullParameterlessCanExecute_Correctly()
│       └── Should_WrapParameterlessActions_Properly()
├── CanExecuteMethod/
│   ├── WithCanExecutePredicate/
│   │   ├── Should_ReturnTrue_WhenCanExecuteReturnsTrue()
│   │   ├── Should_ReturnFalse_WhenCanExecuteReturnsFalse()
│   │   ├── Should_PassParameter_ToCanExecutePredicate()
│   │   ├── Should_HandleNullParameter_InCanExecute()
│   │   └── Should_HandleVariousParameterTypes_InCanExecute()
│   ├── WithoutCanExecutePredicate/
│   │   ├── Should_ReturnTrue_WhenNoCanExecuteSpecified()
│   │   ├── Should_ReturnTrue_ForAnyParameter_WhenNoCanExecute()
│   │   ├── Should_ReturnTrue_ForNullParameter_WhenNoCanExecute()
│   │   └── Should_BeConsistent_AcrossMultipleCalls()
│   └── EdgeCases/
│       ├── Should_HandleCanExecuteException_Gracefully()
│       ├── Should_HandleCanExecuteChangingResult_Correctly()
│       └── Should_MaintainConsistency_UnderConcurrentAccess()
├── ExecuteMethod/
│   ├── ParameterizedExecution/
│   │   ├── Should_InvokeAction_WithParameter()
│   │   ├── Should_PassCorrectParameter_ToAction()
│   │   ├── Should_HandleNullParameter_Correctly()
│   │   ├── Should_HandleVariousParameterTypes()
│   │   └── Should_ExecuteOnlyOnce_PerInvocation()
│   ├── ParameterlessExecution/
│   │   ├── Should_InvokeParameterlessAction_IgnoringParameter()
│   │   ├── Should_CallWrappedParameterlessAction()
│   │   ├── Should_IgnoreParameterValue_ForParameterlessAction()
│   │   └── Should_ExecuteParameterlessAction_Consistently()
│   └── ExecutionBehavior/
│       ├── Should_ExecuteRegardless_OfCanExecuteState()
│       ├── Should_HandleActionException_ByPropagating()
│       ├── Should_AllowMultipleExecutions()
│       └── Should_MaintainStateConsistency_AfterExecution()
├── CanExecuteChangedEvent/
│   ├── EventSubscription/
│   │   ├── Should_SubscribeToCommandManager_OnEventAdd()
│   │   ├── Should_UnsubscribeFromCommandManager_OnEventRemove()
│   │   ├── Should_HandleMultipleSubscriptions_Correctly()
│   │   ├── Should_HandleSubscriptionAfterUnsubscription()
│   │   └── Should_HandleNullEventHandler_Gracefully()
│   ├── EventNotification/
│   │   ├── Should_ReceiveNotification_WhenCommandManagerInvalidates()
│   │   ├── Should_NotReceiveNotification_AfterUnsubscribing()
│   │   ├── Should_ReceiveNotification_FromRaiseCanExecuteChanged()
│   │   └── Should_PassCorrectEventArgs_ToSubscribers()
│   └── EventIntegration/
│       ├── Should_IntegrateWithCommandManager_Properly()
│       ├── Should_ShareNotifications_WithOtherCommands()
│       ├── Should_HandleCommandManagerStateChanges()
│       └── Should_MaintainEventSubscription_ThroughoutLifecycle()
├── RaiseCanExecuteChangedMethod/
│   ├── Should_CallCommandManagerInvalidate_WhenInvoked()
│   ├── Should_TriggerCanExecuteChangedEvent()
│   ├── Should_NotifyAllSubscribers()
│   ├── Should_WorkWithoutSubscribers()
│   └── Should_HandleMultipleRaises_Correctly()
└── IntegrationPatterns/
    ├── MVVMIntegration/
    │   ├── Should_WorkInTypicalViewModelScenario()
    │   ├── Should_SupportDataBinding_Correctly()
    │   ├── Should_HandleUICommandBinding()
    │   └── Should_SupportCommandParameter_Binding()
    ├── CommandCoordination/
    │   ├── Should_ParticipateInGlobalCommandInvalidation()
    │   ├── Should_CoordinateWithOtherRelayCommands()
    │   ├── Should_HandleApplicationStateChanges()
    │   └── Should_SupportConditionalCommandEnabling()
    └── LifecycleManagement/
        ├── Should_HandleCommandLifecycle_Properly()
        ├── Should_CleanupSubscriptions_WhenDisposed()
        ├── Should_MaintainConsistency_ThroughoutLifecycle()
        └── Should_SupportCommandReuse_Safely()
```

### Test Implementation Examples

#### Construction Tests

```csharp
[Fact]
public void Construction_Should_AcceptValidAction_WithoutCanExecute()
{
    // Arrange
    var actionExecuted = false;
    Action<object?> action = _ => actionExecuted = true;

    // Act
    var command = new RelayCommand(action);

    // Assert
    Assert.NotNull(command);
    Assert.True(command.CanExecute(null));
    
    // Verify action can be executed
    command.Execute(null);
    Assert.True(actionExecuted);
}

[Fact]
public void Construction_Should_ThrowArgumentNullException_WhenActionIsNull()
{
    // Arrange
    Action<object?> action = null!;

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() => new RelayCommand(action));
    Assert.Equal("execute", exception.ParamName);
}
```

#### CanExecute Tests

```csharp
[Fact]
public void CanExecute_Should_ReturnTrue_WhenNoCanExecuteSpecified()
{
    // Arrange
    var command = new RelayCommand(_ => { });

    // Act & Assert
    Assert.True(command.CanExecute(null));
    Assert.True(command.CanExecute("test"));
    Assert.True(command.CanExecute(42));
}

[Theory]
[InlineData(true, true)]
[InlineData(false, false)]
public void CanExecute_Should_ReturnCorrectValue_WhenCanExecuteSpecified(bool canExecuteResult, bool expected)
{
    // Arrange
    var command = new RelayCommand(_ => { }, _ => canExecuteResult);

    // Act
    var result = command.CanExecute(null);

    // Assert
    Assert.Equal(expected, result);
}
```

#### Execute Tests

```csharp
[Fact]
public void Execute_Should_InvokeAction_WithCorrectParameter()
{
    // Arrange
    object? receivedParameter = null;
    var command = new RelayCommand(param => receivedParameter = param);
    var testParameter = "test";

    // Act
    command.Execute(testParameter);

    // Assert
    Assert.Equal(testParameter, receivedParameter);
}

[Fact]
public void Execute_Should_InvokeParameterlessAction_FromOverload()
{
    // Arrange
    var actionExecuted = false;
    var command = new RelayCommand(() => actionExecuted = true);

    // Act
    command.Execute("ignored parameter");

    // Assert
    Assert.True(actionExecuted);
}
```

#### Event Tests

```csharp
[Fact]
public void CanExecuteChanged_Should_SubscribeToCommandManager()
{
    // Arrange
    var command = new RelayCommand(_ => { });
    var eventRaised = false;
    EventHandler handler = (_, _) => eventRaised = true;

    // Act
    command.CanExecuteChanged += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.True(eventRaised);
}

[Fact]
public void RaiseCanExecuteChanged_Should_TriggerEvent()
{
    // Arrange
    var command = new RelayCommand(_ => { });
    var eventRaised = false;
    command.CanExecuteChanged += (_, _) => eventRaised = true;

    // Act
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.True(eventRaised);
}
```
    Assert.That(command.CanExecute(null), Is.True);
    
    command.Execute(null);
    Assert.That(actionExecuted, Is.True);
}

[Test]
public void Construction_Should_AcceptValidAction_WithCanExecute()
{
    // Arrange
    var actionExecuted = false;
    Action<object?> action = _ => actionExecuted = true;
    Predicate<object?> canExecute = _ => true;

    // Act
    var command = new RelayCommand(action, canExecute);

    // Assert
    Assert.That(command, Is.Not.Null);
    Assert.That(command.CanExecute(null), Is.True);
    
    command.Execute(null);
    Assert.That(actionExecuted, Is.True);
}

[Test]
public void Construction_Should_ThrowArgumentNullException_WhenActionIsNull()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action<object?>)null!));
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action<object?>)null!, _ => true));
}

[Test]
public void Construction_Should_AcceptNullCanExecute_Gracefully()
{
    // Arrange
    Action<object?> action = _ => { };

    // Act & Assert
    Assert.DoesNotThrow(() => new RelayCommand(action, null));
    
    var command = new RelayCommand(action, null);
    Assert.That(command.CanExecute(null), Is.True);
}

[Test]
public void Construction_Should_ConvertParameterlessAction_ToParameterizedAction()
{
    // Arrange
    var actionExecuted = false;
    Action action = () => actionExecuted = true;

    // Act
    var command = new RelayCommand(action);
    command.Execute("parameter");

    // Assert
    Assert.That(actionExecuted, Is.True);
}

[Test]
public void Construction_Should_ConvertParameterlessCanExecute_ToParameterizedPredicate()
{
    // Arrange
    Action action = () => { };
    Func<bool> canExecute = () => false;

    // Act
    var command = new RelayCommand(action, canExecute);

    // Assert
    Assert.That(command.CanExecute("any parameter"), Is.False);
    Assert.That(command.CanExecute(null), Is.False);
    Assert.That(command.CanExecute(42), Is.False);
}

[Test]
public void Construction_Should_ThrowArgumentNullException_WhenParameterlessActionIsNull()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action)null!));
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action)null!, () => true));
}
```

#### CanExecute Method Tests

```csharp
[Test]
public void CanExecute_Should_ReturnTrue_WhenCanExecuteReturnsTrue()
{
    // Arrange
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = _ => true;
    var command = new RelayCommand(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.True);
    Assert.That(command.CanExecute("parameter"), Is.True);
    Assert.That(command.CanExecute(42), Is.True);
}

[Test]
public void CanExecute_Should_ReturnFalse_WhenCanExecuteReturnsFalse()
{
    // Arrange
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = _ => false;
    var command = new RelayCommand(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.False);
    Assert.That(command.CanExecute("parameter"), Is.False);
    Assert.That(command.CanExecute(42), Is.False);
}

[Test]
public void CanExecute_Should_PassParameter_ToCanExecutePredicate()
{
    // Arrange
    object? receivedParameter = null;
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = param => 
    {
        receivedParameter = param;
        return true;
    };
    var command = new RelayCommand(action, canExecute);
    var testParameter = "test parameter";

    // Act
    command.CanExecute(testParameter);

    // Assert
    Assert.That(receivedParameter, Is.EqualTo(testParameter));
}

[Test]
public void CanExecute_Should_HandleNullParameter_InCanExecute()
{
    // Arrange
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = param => param != null;
    var command = new RelayCommand(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.False);
    Assert.That(command.CanExecute("not null"), Is.True);
}

[Test]
public void CanExecute_Should_HandleVariousParameterTypes_InCanExecute()
{
    // Arrange
    var receivedParameters = new List<object?>();
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = param => 
    {
        receivedParameters.Add(param);
        return true;
    };
    var command = new RelayCommand(action, canExecute);

    var testParameters = new object?[] { null, "string", 42, true, DateTime.Now, new object() };

    // Act
    foreach (var parameter in testParameters)
    {
        command.CanExecute(parameter);
    }

    // Assert
    Assert.That(receivedParameters, Is.EqualTo(testParameters));
}

[Test]
public void CanExecute_Should_ReturnTrue_WhenNoCanExecuteSpecified()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.True);
    Assert.That(command.CanExecute("parameter"), Is.True);
    Assert.That(command.CanExecute(42), Is.True);
    Assert.That(command.CanExecute(new object()), Is.True);
}

[Test]
public void CanExecute_Should_BeConsistent_AcrossMultipleCalls()
{
    // Arrange
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = _ => true;
    var command = new RelayCommand(action, canExecute);

    // Act & Assert
    for (int i = 0; i < 10; i++)
    {
        Assert.That(command.CanExecute("test"), Is.True);
    }
}

[Test]
public void CanExecute_Should_HandleCanExecuteException_ByPropagating()
{
    // Arrange
    Action<object?> action = _ => { };
    Predicate<object?> canExecute = _ => throw new InvalidOperationException("Test exception");
    var command = new RelayCommand(action, canExecute);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => command.CanExecute("test"));
}
```

#### Execute Method Tests

```csharp
[Test]
public void Execute_Should_InvokeAction_WithParameter()
{
    // Arrange
    object? receivedParameter = null;
    Action<object?> action = param => receivedParameter = param;
    var command = new RelayCommand(action);
    var testParameter = "test parameter";

    // Act
    command.Execute(testParameter);

    // Assert
    Assert.That(receivedParameter, Is.EqualTo(testParameter));
}

[Test]
public void Execute_Should_PassCorrectParameter_ToAction()
{
    // Arrange
    var receivedParameters = new List<object?>();
    Action<object?> action = param => receivedParameters.Add(param);
    var command = new RelayCommand(action);

    var testParameters = new object?[] { null, "string", 42, true, DateTime.Now };

    // Act
    foreach (var parameter in testParameters)
    {
        command.Execute(parameter);
    }

    // Assert
    Assert.That(receivedParameters, Is.EqualTo(testParameters));
}

[Test]
public void Execute_Should_HandleNullParameter_Correctly()
{
    // Arrange
    object? receivedParameter = new object(); // Initialize to non-null
    Action<object?> action = param => receivedParameter = param;
    var command = new RelayCommand(action);

    // Act
    command.Execute(null);

    // Assert
    Assert.That(receivedParameter, Is.Null);
}

[Test]
public void Execute_Should_ExecuteOnlyOnce_PerInvocation()
{
    // Arrange
    var executionCount = 0;
    Action<object?> action = _ => executionCount++;
    var command = new RelayCommand(action);

    // Act
    command.Execute("test");

    // Assert
    Assert.That(executionCount, Is.EqualTo(1));
}

[Test]
public void Execute_Should_InvokeParameterlessAction_IgnoringParameter()
{
    // Arrange
    var actionExecuted = false;
    Action action = () => actionExecuted = true;
    var command = new RelayCommand(action);

    // Act
    command.Execute("ignored parameter");

    // Assert
    Assert.That(actionExecuted, Is.True);
}

[Test]
public void Execute_Should_ExecuteRegardless_OfCanExecuteState()
{
    // Arrange
    var actionExecuted = false;
    Action<object?> action = _ => actionExecuted = true;
    Predicate<object?> canExecute = _ => false; // Always false
    var command = new RelayCommand(action, canExecute);

    // Act
    command.Execute("test");

    // Assert
    Assert.That(actionExecuted, Is.True);
    Assert.That(command.CanExecute("test"), Is.False); // Verify CanExecute is still false
}

[Test]
public void Execute_Should_HandleActionException_ByPropagating()
{
    // Arrange
    Action<object?> action = _ => throw new InvalidOperationException("Test exception");
    var command = new RelayCommand(action);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => command.Execute("test"));
}

[Test]
public void Execute_Should_AllowMultipleExecutions()
{
    // Arrange
    var executionCount = 0;
    Action<object?> action = _ => executionCount++;
    var command = new RelayCommand(action);

    // Act
    command.Execute("test1");
    command.Execute("test2");
    command.Execute("test3");

    // Assert
    Assert.That(executionCount, Is.EqualTo(3));
}
```

#### CanExecuteChanged Event Tests

```csharp
[Test]
public void CanExecuteChangedEvent_Should_SubscribeToCommandManager_OnEventAdd()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;

    // Act
    command.CanExecuteChanged += handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    command.CanExecuteChanged -= handler;
}

[Test]
public void CanExecuteChangedEvent_Should_UnsubscribeFromCommandManager_OnEventRemove()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;

    command.CanExecuteChanged += handler;

    // Act
    command.CanExecuteChanged -= handler;
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(eventFired, Is.False);
}

[Test]
public void CanExecuteChangedEvent_Should_HandleMultipleSubscriptions_Correctly()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var event1Fired = false;
    var event2Fired = false;
    var event3Fired = false;

    EventHandler handler1 = (sender, args) => event1Fired = true;
    EventHandler handler2 = (sender, args) => event2Fired = true;
    EventHandler handler3 = (sender, args) => event3Fired = true;

    // Act
    command.CanExecuteChanged += handler1;
    command.CanExecuteChanged += handler2;
    command.CanExecuteChanged += handler3;

    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(event1Fired, Is.True);
    Assert.That(event2Fired, Is.True);
    Assert.That(event3Fired, Is.True);

    // Cleanup
    command.CanExecuteChanged -= handler1;
    command.CanExecuteChanged -= handler2;
    command.CanExecuteChanged -= handler3;
}

[Test]
public void CanExecuteChangedEvent_Should_ReceiveNotification_FromRaiseCanExecuteChanged()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;

    command.CanExecuteChanged += handler;

    // Act
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    command.CanExecuteChanged -= handler;
}
```

#### RaiseCanExecuteChanged Method Tests

```csharp
[Test]
public void RaiseCanExecuteChanged_Should_CallCommandManagerInvalidate_WhenInvoked()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventFired = false;

    // Subscribe to CommandManager to verify invalidation
    EventHandler commandManagerHandler = (sender, args) => eventFired = true;
    CommandManager.RequerySuggested += commandManagerHandler;

    // Act
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    CommandManager.RequerySuggested -= commandManagerHandler;
}

[Test]
public void RaiseCanExecuteChanged_Should_TriggerCanExecuteChangedEvent()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;

    command.CanExecuteChanged += handler;

    // Act
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    command.CanExecuteChanged -= handler;
}

[Test]
public void RaiseCanExecuteChanged_Should_NotifyAllSubscribers()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var event1Fired = false;
    var event2Fired = false;

    EventHandler handler1 = (sender, args) => event1Fired = true;
    EventHandler handler2 = (sender, args) => event2Fired = true;

    command.CanExecuteChanged += handler1;
    command.CanExecuteChanged += handler2;

    // Act
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.That(event1Fired, Is.True);
    Assert.That(event2Fired, Is.True);

    // Cleanup
    command.CanExecuteChanged -= handler1;
    command.CanExecuteChanged -= handler2;
}

[Test]
public void RaiseCanExecuteChanged_Should_WorkWithoutSubscribers()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);

    // Act & Assert
    Assert.DoesNotThrow(() => command.RaiseCanExecuteChanged());
}

[Test]
public void RaiseCanExecuteChanged_Should_HandleMultipleRaises_Correctly()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand(action);
    var eventCount = 0;
    EventHandler handler = (sender, args) => eventCount++;

    command.CanExecuteChanged += handler;

    // Act
    command.RaiseCanExecuteChanged();
    command.RaiseCanExecuteChanged();
    command.RaiseCanExecuteChanged();

    // Assert
    Assert.That(eventCount, Is.EqualTo(3));

    // Cleanup
    command.CanExecuteChanged -= handler;
}
```

#### Integration Pattern Tests

```csharp
[Test]
public void MVVMIntegration_Should_WorkInTypicalViewModelScenario()
{
    // Arrange
    var viewModel = new TestViewModel();
    
    // Act
    var canExecuteBefore = viewModel.SaveCommand.CanExecute(null);
    viewModel.SetCanSave(false);
    viewModel.SaveCommand.RaiseCanExecuteChanged();
    var canExecuteAfter = viewModel.SaveCommand.CanExecute(null);

    // Assert
    Assert.That(canExecuteBefore, Is.True);
    Assert.That(canExecuteAfter, Is.False);
}

private class TestViewModel
{
    private bool _canSave = true;

    public ICommand SaveCommand { get; }

    public TestViewModel()
    {
        SaveCommand = new RelayCommand(Save, () => _canSave);
    }

    private void Save() { /* Implementation */ }
    
    public void SetCanSave(bool canSave) => _canSave = canSave;
}

[Test]
public void CommandCoordination_Should_ParticipateInGlobalCommandInvalidation()
{
    // Arrange
    Action<object?> action = _ => { };
    var command1 = new RelayCommand(action);
    var command2 = new RelayCommand(action);
    
    var command1EventFired = false;
    var command2EventFired = false;

    command1.CanExecuteChanged += (sender, args) => command1EventFired = true;
    command2.CanExecuteChanged += (sender, args) => command2EventFired = true;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(command1EventFired, Is.True);
    Assert.That(command2EventFired, Is.True);

    // Cleanup
    command1.CanExecuteChanged -= (sender, args) => command1EventFired = true;
    command2.CanExecuteChanged -= (sender, args) => command2EventFired = true;
}

[Test]
public void LifecycleManagement_Should_HandleCommandLifecycle_Properly()
{
    // Arrange
    var executionCount = 0;
    Action<object?> action = _ => executionCount++;
    var command = new RelayCommand(action);

    // Act - Simulate typical command lifecycle
    var canExecute1 = command.CanExecute("test");
    command.Execute("test");
    
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;
    command.CanExecuteChanged += handler;
    
    command.RaiseCanExecuteChanged();
    var canExecute2 = command.CanExecute("test");
    command.Execute("test");
    
    command.CanExecuteChanged -= handler;

    // Assert
    Assert.That(canExecute1, Is.True);
    Assert.That(canExecute2, Is.True);
    Assert.That(executionCount, Is.EqualTo(2));
    Assert.That(eventFired, Is.True);
}
```

### Test Fixtures Required

- **RelayCommandTestFixture** - Standard test fixture with cleanup
- **ActionMockFactory** - Factory for creating test action delegates
- **PredicateMockFactory** - Factory for creating test predicate delegates
- **ViewModelTestDouble** - Test ViewModel for integration testing
- **CommandManagerTestHelper** - Helper for CommandManager state verification

## Success Criteria

- [ ] **Construction behavior** - Proper delegate injection and null handling
- [ ] **CanExecute logic** - Correct predicate evaluation and parameter passing
- [ ] **Execute behavior** - Proper action invocation with parameter handling
- [ ] **Event coordination** - CommandManager integration and event propagation
- [ ] **Constructor overloads** - Both parameterized and parameterless patterns
- [ ] **Exception handling** - Proper propagation of delegate exceptions
- [ ] **MVVM integration** - Works correctly in typical ViewModel scenarios

## Implementation Priority

**HIGH PRIORITY** - Critical MVVM infrastructure component with excellent testability. Foundation for all command patterns.

## Dependencies for Testing

- **xUnit** - Standard testing framework (Assert.NotNull, Assert.Equal, Assert.True syntax)
- **Custom test doubles** - ViewModel and delegate implementations
- **CommandManager** - Integration with command coordination infrastructure

## Implementation Estimate

**Effort: Medium (1.0 day)**

- Comprehensive delegate testing patterns
- Event coordination verification
- Multiple constructor overload testing
- Integration pattern validation
- Exception handling scenarios
- MVVM lifecycle testing

## Completion Requirements

Upon completion of testing implementation:

1. **Coverage Report**: Run `generate-coverage-report.ps1` and commit the updated `CoverageReport/Summary.txt` file showing improved coverage for RelayCommand from 0% baseline
2. **Master Plan Update**: Before marking this component complete, re-read and update the Master Test Execution Plan with progress, learnings, and any discovered patterns
3. **Verification**: Request human confirmation before proceeding to next component as per Master Plan protocols

This class represents a fundamental MVVM component requiring thorough testing to ensure proper command behavior throughout the application.