# RelayCommand&lt;T&gt; - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview

**File**: `MauiApp/Infrastructure/RelayCommand.cs`  
**Type**: Generic ICommand Implementation  
**LOC**: 39 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Generic implementation of ICommand pattern providing strongly-typed parameter support for MVVM applications. Encapsulates typed action delegates with conditional execution capabilities while maintaining type safety and proper parameter validation. Essential component for type-safe command binding in MAUI applications with complex parameter scenarios.

### Dependencies

- **System.Windows.Input.ICommand** - Standard command interface
- **WorkMood.MauiApp.Infrastructure.CommandManager** - Static command coordination
- **System.Action&lt;T?&gt;** - Typed action delegates for execution
- **System.Predicate&lt;T?&gt;** - Typed conditional execution delegates

### Key Responsibilities

1. **Type-safe action encapsulation** - Strongly-typed parameter handling
2. **Parameter type validation** - Runtime type checking and conversion
3. **Conditional execution** - Typed CanExecute logic with predicate delegates
4. **Event coordination** - CommandManager integration for state notifications
5. **Null-safe parameter handling** - Proper handling of nullable reference/value types
6. **Generic type support** - Works with any type parameter including value types

### Current Architecture Assessment

**Testability Score: 8/10** ✅ **GOOD TESTABILITY**

**Design Strengths:**

1. **Type safety** - Compile-time type checking for parameters
2. **Clear dependency injection** - Constructor injection for typed delegates
3. **Comprehensive parameter validation** - Handles type conversions and null values
4. **Standard interface compliance** - Proper ICommand implementation
5. **Event integration** - CommandManager coordination
6. **Value type awareness** - Proper handling of value vs reference types

**Design Challenges Impacting Testability:**

1. **Complex parameter validation logic** - Multiple type checking branches
2. **Generic type constraints** - Testing requires multiple type instantiations
3. **Runtime type checking** - Behavior depends on runtime parameter types
4. **Value type null handling** - Complex logic for nullable value types

**Minor Testability Improvements Needed:**

1. **Parameter validation logging** - Add visibility into type conversion failures
2. **Exception messaging** - More descriptive error messages for type mismatches

## Usage Scenarios Analysis

### Typical ViewModel Integration Patterns

```csharp
public class MyViewModel
{
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new RelayCommand<string>(SaveWithName, CanSave);
        DeleteCommand = new RelayCommand<int>(DeleteById, id => id > 0);
        EditCommand = new RelayCommand<MyModel>(EditModel, model => model != null);
    }

    private void SaveWithName(string? name) { /* Implementation */ }
    private bool CanSave(string? name) => !string.IsNullOrEmpty(name);
}
```

### Business Logic Applications

- **Type-safe UI binding** - Strongly-typed command parameters from UI controls
- **Model-specific commands** - Commands operating on specific business objects
- **Validation with types** - Type-aware conditional command execution
- **Parameter transformation** - Safe conversion between UI and business types

## Comprehensive Test Plan

### Test Structure

```
RelayCommandTTests/
├── Construction/
│   ├── Should_AcceptValidTypedAction_WithoutCanExecute()
│   ├── Should_AcceptValidTypedAction_WithCanExecute()
│   ├── Should_ThrowArgumentNullException_WhenActionIsNull()
│   ├── Should_AcceptNullCanExecute_Gracefully()
│   └── Should_InitializeGenericPropertiesCorrectly()
├── ParameterTypeValidation/
│   ├── ExactTypeMatching/
│   │   ├── Should_AcceptExactType_InCanExecute()
│   │   ├── Should_AcceptExactType_InExecute()
│   │   ├── Should_PassTypedParameter_ToCanExecute()
│   │   └── Should_PassTypedParameter_ToExecute()
│   ├── TypeConversion/
│   │   ├── Should_RejectIncompatibleType_InCanExecute()
│   │   ├── Should_RejectIncompatibleType_InExecute()
│   │   ├── Should_HandleInheritanceChain_Correctly()
│   │   └── Should_HandleInterfaceImplementation_Correctly()
│   ├── NullHandling/
│   │   ├── Should_AcceptNull_ForReferenceTypes()
│   │   ├── Should_RejectNull_ForValueTypes()
│   │   ├── Should_AcceptNull_ForNullableValueTypes()
│   │   └── Should_HandleDefaultValues_Appropriately()
│   └── EdgeCases/
│       ├── Should_HandleObjectParameter_Correctly()
│       ├── Should_HandleGenericConstraints_Properly()
│       ├── Should_HandleBoxedValueTypes_Correctly()
│       └── Should_HandleEnumTypes_Appropriately()
├── CanExecuteMethod/
│   ├── TypedParameters/
│   │   ├── Should_ReturnTrue_WhenTypedCanExecuteReturnsTrue()
│   │   ├── Should_ReturnFalse_WhenTypedCanExecuteReturnsFalse()
│   │   ├── Should_PassCorrectTypedParameter_ToCanExecute()
│   │   ├── Should_HandleNullTypedParameter_InCanExecute()
│   │   └── Should_HandleDefaultTypedParameter_InCanExecute()
│   ├── WithoutCanExecutePredicate/
│   │   ├── Should_ReturnTrue_ForValidTypedParameter()
│   │   ├── Should_ReturnFalse_ForInvalidTypedParameter()
│   │   ├── Should_ReturnTrue_WhenNoCanExecuteSpecified()
│   │   └── Should_HandleNullParameter_ConsistentlyWithoutCanExecute()
│   ├── ValueTypeHandling/
│   │   ├── Should_HandleValueTypeParameters_Correctly()
│   │   ├── Should_HandleNullableValueTypes_Correctly()
│   │   ├── Should_RejectNullForValueTypes()
│   │   └── Should_HandleDefaultValueTypeParameters()
│   └── ReferenceTypeHandling/
│       ├── Should_HandleReferenceTypeParameters_Correctly()
│       ├── Should_AcceptNullForReferenceTypes()
│       ├── Should_HandleInheritedTypes_Appropriately()
│       └── Should_HandleInterfaceTypes_Correctly()
├── ExecuteMethod/
│   ├── TypedExecution/
│   │   ├── Should_InvokeTypedAction_WithCorrectParameter()
│   │   ├── Should_PassTypedParameter_ToAction()
│   │   ├── Should_HandleNullTypedParameter_InExecute()
│   │   ├── Should_HandleDefaultTypedParameter_InExecute()
│   │   └── Should_ExecuteOnlyOnce_PerTypedInvocation()
│   ├── ParameterValidation/
│   │   ├── Should_ExecuteAction_ForValidTypedParameter()
│   │   ├── Should_IgnoreAction_ForInvalidTypedParameter()
│   │   ├── Should_HandleTypeConversion_Silently()
│   │   └── Should_NotThrow_ForInvalidParameter()
│   ├── ValueTypeExecution/
│   │   ├── Should_ExecuteAction_ForValueTypeParameter()
│   │   ├── Should_ExecuteAction_ForNullableValueType()
│   │   ├── Should_IgnoreExecution_ForNullValueType()
│   │   └── Should_HandleDefaultValueType_Appropriately()
│   └── ReferenceTypeExecution/
│       ├── Should_ExecuteAction_ForReferenceTypeParameter()
│       ├── Should_ExecuteAction_ForNullReferenceType()
│       ├── Should_HandleInheritedTypes_InExecution()
│       └── Should_ExecuteAction_ForInterfaceTypes()
├── GenericTypeSpecialization/
│   ├── StringCommands/
│   │   ├── Should_HandleStringParameters_Correctly()
│   │   ├── Should_AcceptNullString_Parameters()
│   │   ├── Should_ValidateStringLength_InCanExecute()
│   │   └── Should_ProcessStringContent_InExecute()
│   ├── IntegerCommands/
│   │   ├── Should_HandleIntegerParameters_Correctly()
│   │   ├── Should_RejectNullInteger_Parameters()
│   │   ├── Should_ValidateIntegerRange_InCanExecute()
│   │   └── Should_ProcessIntegerValue_InExecute()
│   ├── ObjectCommands/
│   │   ├── Should_HandleComplexObjectParameters()
│   │   ├── Should_AcceptNullObject_Parameters()
│   │   ├── Should_ValidateObjectState_InCanExecute()
│   │   └── Should_ProcessObjectProperties_InExecute()
│   ├── EnumCommands/
│   │   ├── Should_HandleEnumParameters_Correctly()
│   │   ├── Should_ValidateEnumValues_InCanExecute()
│   │   ├── Should_ProcessEnumCases_InExecute()
│   │   └── Should_HandleInvalidEnumValues_Gracefully()
│   └── NullableCommands/
│       ├── Should_HandleNullableInt_Parameters()
│       ├── Should_HandleNullableDateTime_Parameters()
│       ├── Should_HandleNullableBool_Parameters()
│       └── Should_DistinguishNull_FromDefaultValues()
├── CanExecuteChangedEvent/
│   ├── EventSubscription/
│   │   ├── Should_SubscribeToCommandManager_OnEventAdd()
│   │   ├── Should_UnsubscribeFromCommandManager_OnEventRemove()
│   │   ├── Should_HandleTypedEventSubscription_Correctly()
│   │   └── Should_IntegrateWithCommandManager_Properly()
│   ├── EventNotification/
│   │   ├── Should_ReceiveNotification_WhenCommandManagerInvalidates()
│   │   ├── Should_ReceiveNotification_FromRaiseCanExecuteChanged()
│   │   ├── Should_MaintainEventSubscription_AcrossTypeChanges()
│   │   └── Should_HandleGenericEventNotification_Correctly()
│   └── TypedEventHandling/
│       ├── Should_HandleEventSubscription_ForDifferentTypes()
│       ├── Should_MaintainEventState_PerGenericInstance()
│       ├── Should_ShareCommandManager_AcrossGenericTypes()
│       └── Should_HandleEventCleanup_ForGenericCommands()
├── RaiseCanExecuteChangedMethod/
│   ├── Should_CallCommandManagerInvalidate_WhenInvoked()
│   ├── Should_TriggerTypedCanExecuteChangedEvent()
│   ├── Should_NotifyAllTypedSubscribers()
│   ├── Should_WorkWithoutTypedSubscribers()
│   └── Should_HandleMultipleRaises_ForGenericCommand()
└── IntegrationPatterns/
    ├── MVVMIntegration/
    │   ├── Should_WorkInTypicalTypedViewModelScenario()
    │   ├── Should_SupportTypedDataBinding_Correctly()
    │   ├── Should_HandleTypedUICommandBinding()
    │   └── Should_SupportTypedCommandParameter_Binding()
    ├── TypeSafety/
    │   ├── Should_PreventRuntimeTypeErrors_InCanExecute()
    │   ├── Should_PreventRuntimeTypeErrors_InExecute()
    │   ├── Should_ProvideCompileTimeTypeSafety()
    │   └── Should_HandleGenericConstraints_Appropriately()
    ├── CommandCoordination/
    │   ├── Should_ParticipateInGlobalCommandInvalidation()
    │   ├── Should_CoordinateWithOtherTypedCommands()
    │   ├── Should_ShareCommandManager_WithNonGenericCommands()
    │   └── Should_MaintainTypeSpecificBehavior_InCoordination()
    └── LifecycleManagement/
        ├── Should_HandleTypedCommandLifecycle_Properly()
        ├── Should_CleanupTypedSubscriptions_WhenDisposed()
        ├── Should_MaintainTypeConsistency_ThroughoutLifecycle()
        └── Should_SupportTypedCommandReuse_Safely()
```

### Test Implementation Examples

#### Construction Tests

```csharp
[Test]
public void Construction_Should_AcceptValidTypedAction_WithoutCanExecute()
{
    // Arrange
    var actionExecuted = false;
    string? receivedParameter = null;
    Action<string?> action = param => 
    {
        actionExecuted = true;
        receivedParameter = param;
    };

    // Act
    var command = new RelayCommand<string>(action);

    // Assert
    Assert.That(command, Is.Not.Null);
    Assert.That(command.CanExecute("test"), Is.True);
    
    command.Execute("test parameter");
    Assert.That(actionExecuted, Is.True);
    Assert.That(receivedParameter, Is.EqualTo("test parameter"));
}

[Test]
public void Construction_Should_AcceptValidTypedAction_WithCanExecute()
{
    // Arrange
    var actionExecuted = false;
    Action<int> action = param => actionExecuted = true;
    Predicate<int> canExecute = param => param > 0;

    // Act
    var command = new RelayCommand<int>(action, canExecute);

    // Assert
    Assert.That(command, Is.Not.Null);
    Assert.That(command.CanExecute(5), Is.True);
    Assert.That(command.CanExecute(-1), Is.False);
    
    command.Execute(5);
    Assert.That(actionExecuted, Is.True);
}

[Test]
public void Construction_Should_ThrowArgumentNullException_WhenActionIsNull()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>((Action<string?>)null!));
    Assert.Throws<ArgumentNullException>(() => new RelayCommand<int>((Action<int>)null!, _ => true));
}

[Test]
public void Construction_Should_AcceptNullCanExecute_Gracefully()
{
    // Arrange
    Action<string?> action = _ => { };

    // Act & Assert
    Assert.DoesNotThrow(() => new RelayCommand<string>(action, null));
    
    var command = new RelayCommand<string>(action, null);
    Assert.That(command.CanExecute("test"), Is.True);
}
```

#### Parameter Type Validation Tests

```csharp
[Test]
public void ParameterTypeValidation_Should_AcceptExactType_InCanExecute()
{
    // Arrange
    Action<string?> action = _ => { };
    Predicate<string?> canExecute = _ => true;
    var command = new RelayCommand<string>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute("exact string type"), Is.True);
    Assert.That(command.CanExecute(string.Empty), Is.True);
}

[Test]
public void ParameterTypeValidation_Should_RejectIncompatibleType_InCanExecute()
{
    // Arrange
    Action<string?> action = _ => { };
    var command = new RelayCommand<string>(action);

    // Act & Assert
    Assert.That(command.CanExecute(42), Is.False);
    Assert.That(command.CanExecute(true), Is.False);
    Assert.That(command.CanExecute(DateTime.Now), Is.False);
    Assert.That(command.CanExecute(new object()), Is.False);
}

[Test]
public void ParameterTypeValidation_Should_AcceptNull_ForReferenceTypes()
{
    // Arrange
    Action<string?> action = _ => { };
    var command = new RelayCommand<string>(action);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.True);
}

[Test]
public void ParameterTypeValidation_Should_RejectNull_ForValueTypes()
{
    // Arrange
    Action<int> action = _ => { };
    var command = new RelayCommand<int>(action);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.False);
}

[Test]
public void ParameterTypeValidation_Should_AcceptNull_ForNullableValueTypes()
{
    // Arrange
    Action<int?> action = _ => { };
    var command = new RelayCommand<int?>(action);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.True);
    Assert.That(command.CanExecute(42), Is.True);
}

[Test]
public void ParameterTypeValidation_Should_HandleInheritanceChain_Correctly()
{
    // Arrange
    Action<object?> action = _ => { };
    var command = new RelayCommand<object>(action);

    // Act & Assert
    Assert.That(command.CanExecute("string inherits from object"), Is.True);
    Assert.That(command.CanExecute(42), Is.True);
    Assert.That(command.CanExecute(DateTime.Now), Is.True);
    Assert.That(command.CanExecute(new List<int>()), Is.True);
}

[Test]
public void ParameterTypeValidation_Should_HandleBoxedValueTypes_Correctly()
{
    // Arrange
    Action<int> action = _ => { };
    var command = new RelayCommand<int>(action);

    object boxedInt = 42;
    object boxedString = "not an int";

    // Act & Assert
    Assert.That(command.CanExecute(boxedInt), Is.True);
    Assert.That(command.CanExecute(boxedString), Is.False);
}
```

#### CanExecute Method Tests

```csharp
[Test]
public void CanExecute_Should_ReturnTrue_WhenTypedCanExecuteReturnsTrue()
{
    // Arrange
    Action<string?> action = _ => { };
    Predicate<string?> canExecute = param => !string.IsNullOrEmpty(param);
    var command = new RelayCommand<string>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute("valid string"), Is.True);
    Assert.That(command.CanExecute("another valid string"), Is.True);
}

[Test]
public void CanExecute_Should_ReturnFalse_WhenTypedCanExecuteReturnsFalse()
{
    // Arrange
    Action<string?> action = _ => { };
    Predicate<string?> canExecute = param => !string.IsNullOrEmpty(param);
    var command = new RelayCommand<string>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(null), Is.False);
    Assert.That(command.CanExecute(""), Is.False);
    Assert.That(command.CanExecute("   "), Is.True); // Not empty, just whitespace
}

[Test]
public void CanExecute_Should_PassCorrectTypedParameter_ToCanExecute()
{
    // Arrange
    string? receivedParameter = null;
    Action<string?> action = _ => { };
    Predicate<string?> canExecute = param => 
    {
        receivedParameter = param;
        return true;
    };
    var command = new RelayCommand<string>(action, canExecute);
    var testParameter = "test parameter";

    // Act
    command.CanExecute(testParameter);

    // Assert
    Assert.That(receivedParameter, Is.EqualTo(testParameter));
}

[Test]
public void CanExecute_Should_ReturnTrue_WhenNoCanExecuteSpecified()
{
    // Arrange
    Action<string?> action = _ => { };
    var command = new RelayCommand<string>(action);

    // Act & Assert
    Assert.That(command.CanExecute("any string"), Is.True);
    Assert.That(command.CanExecute(null), Is.True);
}

[Test]
public void CanExecute_Should_HandleValueTypeParameters_Correctly()
{
    // Arrange
    Action<int> action = _ => { };
    Predicate<int> canExecute = param => param > 0;
    var command = new RelayCommand<int>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(5), Is.True);
    Assert.That(command.CanExecute(-1), Is.False);
    Assert.That(command.CanExecute(0), Is.False);
    Assert.That(command.CanExecute(null), Is.False); // Null rejected for value types
}

[Test]
public void CanExecute_Should_HandleNullableValueTypes_Correctly()
{
    // Arrange
    Action<int?> action = _ => { };
    Predicate<int?> canExecute = param => param.HasValue && param.Value > 0;
    var command = new RelayCommand<int?>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute(5), Is.True);
    Assert.That(command.CanExecute(-1), Is.False);
    Assert.That(command.CanExecute(null), Is.False); // Predicate returns false for null
}
```

#### Execute Method Tests

```csharp
[Test]
public void Execute_Should_InvokeTypedAction_WithCorrectParameter()
{
    // Arrange
    string? receivedParameter = null;
    Action<string?> action = param => receivedParameter = param;
    var command = new RelayCommand<string>(action);
    var testParameter = "test parameter";

    // Act
    command.Execute(testParameter);

    // Assert
    Assert.That(receivedParameter, Is.EqualTo(testParameter));
}

[Test]
public void Execute_Should_HandleNullTypedParameter_InExecute()
{
    // Arrange
    string? receivedParameter = "initial value";
    Action<string?> action = param => receivedParameter = param;
    var command = new RelayCommand<string>(action);

    // Act
    command.Execute(null);

    // Assert
    Assert.That(receivedParameter, Is.Null);
}

[Test]
public void Execute_Should_ExecuteAction_ForValidTypedParameter()
{
    // Arrange
    var executionCount = 0;
    Action<int> action = _ => executionCount++;
    var command = new RelayCommand<int>(action);

    // Act
    command.Execute(42);

    // Assert
    Assert.That(executionCount, Is.EqualTo(1));
}

[Test]
public void Execute_Should_IgnoreAction_ForInvalidTypedParameter()
{
    // Arrange
    var executionCount = 0;
    Action<int> action = _ => executionCount++;
    var command = new RelayCommand<int>(action);

    // Act
    command.Execute("not an int");
    command.Execute(null);
    command.Execute(3.14);

    // Assert
    Assert.That(executionCount, Is.EqualTo(0));
}

[Test]
public void Execute_Should_HandleValueTypeExecution_Correctly()
{
    // Arrange
    var receivedValues = new List<int>();
    Action<int> action = param => receivedValues.Add(param);
    var command = new RelayCommand<int>(action);

    // Act
    command.Execute(42);
    command.Execute(0);
    command.Execute(-5);

    // Assert
    Assert.That(receivedValues, Is.EqualTo(new[] { 42, 0, -5 }));
}

[Test]
public void Execute_Should_HandleNullableValueTypeExecution()
{
    // Arrange
    var receivedValues = new List<int?>();
    Action<int?> action = param => receivedValues.Add(param);
    var command = new RelayCommand<int?>(action);

    // Act
    command.Execute(42);
    command.Execute(null);
    command.Execute(0);

    // Assert
    Assert.That(receivedValues, Is.EqualTo(new int?[] { 42, null, 0 }));
}

[Test]
public void Execute_Should_HandleDefaultValueType_Appropriately()
{
    // Arrange
    var receivedValues = new List<int>();
    Action<int> action = param => receivedValues.Add(param);
    var command = new RelayCommand<int>(action);

    // Act
    command.Execute(null); // Should be ignored for value types

    // Assert
    Assert.That(receivedValues, Is.Empty);
}
```

#### Generic Type Specialization Tests

```csharp
[Test]
public void StringCommands_Should_HandleStringParameters_Correctly()
{
    // Arrange
    var receivedStrings = new List<string?>();
    Action<string?> action = param => receivedStrings.Add(param);
    Predicate<string?> canExecute = param => param != null;
    var command = new RelayCommand<string>(action, canExecute);

    // Act
    var testStrings = new[] { "hello", "", "world", null };
    foreach (var str in testStrings)
    {
        if (command.CanExecute(str))
        {
            command.Execute(str);
        }
    }

    // Assert
    Assert.That(receivedStrings, Is.EqualTo(new[] { "hello", "", "world" }));
}

[Test]
public void IntegerCommands_Should_HandleIntegerParameters_Correctly()
{
    // Arrange
    var receivedInts = new List<int>();
    Action<int> action = param => receivedInts.Add(param);
    Predicate<int> canExecute = param => param >= 0;
    var command = new RelayCommand<int>(action, canExecute);

    // Act
    var testInts = new[] { 5, -1, 0, 42 };
    foreach (var value in testInts)
    {
        if (command.CanExecute(value))
        {
            command.Execute(value);
        }
    }

    // Assert
    Assert.That(receivedInts, Is.EqualTo(new[] { 5, 0, 42 }));
}

[Test]
public void ObjectCommands_Should_HandleComplexObjectParameters()
{
    // Arrange
    var receivedObjects = new List<TestObject?>();
    Action<TestObject?> action = param => receivedObjects.Add(param);
    Predicate<TestObject?> canExecute = param => param?.IsValid == true;
    var command = new RelayCommand<TestObject>(action, canExecute);

    var validObject = new TestObject { IsValid = true };
    var invalidObject = new TestObject { IsValid = false };

    // Act
    var testObjects = new[] { validObject, invalidObject, null };
    foreach (var obj in testObjects)
    {
        if (command.CanExecute(obj))
        {
            command.Execute(obj);
        }
    }

    // Assert
    Assert.That(receivedObjects, Is.EqualTo(new[] { validObject }));
}

private class TestObject
{
    public bool IsValid { get; set; }
}

[Test]
public void EnumCommands_Should_HandleEnumParameters_Correctly()
{
    // Arrange
    var receivedEnums = new List<TestEnum>();
    Action<TestEnum> action = param => receivedEnums.Add(param);
    Predicate<TestEnum> canExecute = param => param != TestEnum.Invalid;
    var command = new RelayCommand<TestEnum>(action, canExecute);

    // Act
    var testEnums = new[] { TestEnum.Valid1, TestEnum.Valid2, TestEnum.Invalid };
    foreach (var enumValue in testEnums)
    {
        if (command.CanExecute(enumValue))
        {
            command.Execute(enumValue);
        }
    }

    // Assert
    Assert.That(receivedEnums, Is.EqualTo(new[] { TestEnum.Valid1, TestEnum.Valid2 }));
}

private enum TestEnum
{
    Invalid,
    Valid1,
    Valid2
}

[Test]
public void NullableCommands_Should_DistinguishNull_FromDefaultValues()
{
    // Arrange
    var receivedValues = new List<int?>();
    Action<int?> action = param => receivedValues.Add(param);
    var command = new RelayCommand<int?>(action);

    // Act
    command.Execute(null);
    command.Execute(0);
    command.Execute(default(int?));

    // Assert
    Assert.That(receivedValues.Count, Is.EqualTo(3));
    Assert.That(receivedValues[0], Is.Null);
    Assert.That(receivedValues[1], Is.EqualTo(0));
    Assert.That(receivedValues[2], Is.Null); // default(int?) is null
}
```

#### Integration Pattern Tests

```csharp
[Test]
public void MVVMIntegration_Should_WorkInTypicalTypedViewModelScenario()
{
    // Arrange
    var viewModel = new TypedTestViewModel();
    
    // Act
    var canExecuteStringBefore = viewModel.StringCommand.CanExecute("test");
    var canExecuteIntBefore = viewModel.IntCommand.CanExecute(5);
    
    viewModel.SetStringValid(false);
    viewModel.SetIntValid(false);
    
    viewModel.StringCommand.RaiseCanExecuteChanged();
    viewModel.IntCommand.RaiseCanExecuteChanged();
    
    var canExecuteStringAfter = viewModel.StringCommand.CanExecute("test");
    var canExecuteIntAfter = viewModel.IntCommand.CanExecute(5);

    // Assert
    Assert.That(canExecuteStringBefore, Is.True);
    Assert.That(canExecuteIntBefore, Is.True);
    Assert.That(canExecuteStringAfter, Is.False);
    Assert.That(canExecuteIntAfter, Is.False);
}

private class TypedTestViewModel
{
    private bool _stringValid = true;
    private bool _intValid = true;

    public RelayCommand<string?> StringCommand { get; }
    public RelayCommand<int> IntCommand { get; }

    public TypedTestViewModel()
    {
        StringCommand = new RelayCommand<string?>(ProcessString, s => _stringValid);
        IntCommand = new RelayCommand<int>(ProcessInt, i => _intValid);
    }

    private void ProcessString(string? value) { /* Implementation */ }
    private void ProcessInt(int value) { /* Implementation */ }
    
    public void SetStringValid(bool valid) => _stringValid = valid;
    public void SetIntValid(bool valid) => _intValid = valid;
}

[Test]
public void TypeSafety_Should_PreventRuntimeTypeErrors_InCanExecute()
{
    // Arrange
    Action<string?> action = _ => { };
    Predicate<string?> canExecute = _ => true;
    var command = new RelayCommand<string>(action, canExecute);

    // Act & Assert
    Assert.That(command.CanExecute("valid string"), Is.True);
    Assert.That(command.CanExecute(42), Is.False);
    Assert.That(command.CanExecute(DateTime.Now), Is.False);
    Assert.That(command.CanExecute(new object()), Is.False);
}

[Test]
public void TypeSafety_Should_PreventRuntimeTypeErrors_InExecute()
{
    // Arrange
    var executionCount = 0;
    Action<string?> action = _ => executionCount++;
    var command = new RelayCommand<string>(action);

    // Act
    command.Execute("valid string");
    command.Execute(42);
    command.Execute(DateTime.Now);
    command.Execute(new object());

    // Assert
    Assert.That(executionCount, Is.EqualTo(1)); // Only string parameter executed
}

[Test]
public void CommandCoordination_Should_ParticipateInGlobalCommandInvalidation()
{
    // Arrange
    Action<string?> stringAction = _ => { };
    Action<int> intAction = _ => { };
    var stringCommand = new RelayCommand<string>(stringAction);
    var intCommand = new RelayCommand<int>(intAction);
    
    var stringEventFired = false;
    var intEventFired = false;

    stringCommand.CanExecuteChanged += (sender, args) => stringEventFired = true;
    intCommand.CanExecuteChanged += (sender, args) => intEventFired = true;

    // Act
    CommandManager.InvalidateRequerySuggested();

    // Assert
    Assert.That(stringEventFired, Is.True);
    Assert.That(intEventFired, Is.True);

    // Cleanup
    stringCommand.CanExecuteChanged -= (sender, args) => stringEventFired = true;
    intCommand.CanExecuteChanged -= (sender, args) => intEventFired = true;
}

[Test]
public void LifecycleManagement_Should_HandleTypedCommandLifecycle_Properly()
{
    // Arrange
    var executionCount = 0;
    Action<string?> action = _ => executionCount++;
    Predicate<string?> canExecute = _ => true;
    var command = new RelayCommand<string>(action, canExecute);

    // Act - Simulate typical typed command lifecycle
    var canExecute1 = command.CanExecute("test");
    command.Execute("test");
    
    var eventFired = false;
    EventHandler handler = (sender, args) => eventFired = true;
    command.CanExecuteChanged += handler;
    
    command.RaiseCanExecuteChanged();
    var canExecute2 = command.CanExecute("test2");
    command.Execute("test2");
    
    command.CanExecuteChanged -= handler;

    // Assert
    Assert.That(canExecute1, Is.True);
    Assert.That(canExecute2, Is.True);
    Assert.That(executionCount, Is.EqualTo(2));
    Assert.That(eventFired, Is.True);
}
```

### Test Fixtures Required

- **RelayCommandTTestFixture** - Generic test fixture with cleanup
- **TypedActionMockFactory** - Factory for creating typed test action delegates
- **TypedPredicateMockFactory** - Factory for creating typed test predicate delegates
- **GenericViewModelTestDouble** - Test ViewModel for typed integration testing
- **TypeValidationTestHelper** - Helper for type conversion and validation testing

## Success Criteria

- [ ] **Construction behavior** - Proper typed delegate injection and null handling
- [ ] **Type validation** - Correct parameter type checking and conversion
- [ ] **CanExecute logic** - Proper typed predicate evaluation
- [ ] **Execute behavior** - Type-safe action invocation with parameter validation
- [ ] **Generic type specialization** - Works correctly with various type parameters
- [ ] **Null handling** - Proper distinction between reference/value type nullability
- [ ] **Event coordination** - CommandManager integration maintains type safety
- [ ] **MVVM integration** - Type-safe command binding in typical scenarios

## Implementation Priority

**HIGH PRIORITY** - Critical typed MVVM infrastructure component with good testability. Essential for type-safe command patterns.

## Dependencies for Testing

- **xUnit** - Standard testing framework (Assert.NotNull, Assert.Equal, Assert.True syntax)
- **Custom test types** - Enums, classes, and structs for type testing
- **Generic test doubles** - Typed ViewModel implementations
- **CommandManager** - Integration with command coordination infrastructure

## Implementation Estimate

**Effort: Medium-High (1.25 days)**

- Comprehensive generic type testing patterns
- Type validation and conversion verification
- Multiple generic specialization testing
- Integration pattern validation
- Complex parameter handling scenarios
- Type safety verification across all methods

## Completion Requirements

Upon completion of testing implementation:

1. **Coverage Report**: Run `generate-coverage-report.ps1` and commit the updated `CoverageReport/Summary.txt` file showing improved coverage for RelayCommand<T> from 0% baseline
2. **Master Plan Update**: Before marking this component complete, re-read and update the Master Test Execution Plan with progress, learnings, and any discovered patterns
3. **Verification**: Request human confirmation before proceeding to next component as per Master Plan protocols

This class represents a sophisticated generic MVVM component requiring thorough testing to ensure type safety and proper generic behavior throughout the application.