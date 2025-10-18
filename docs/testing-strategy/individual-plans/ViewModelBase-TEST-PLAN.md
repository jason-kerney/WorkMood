# ViewModelBase - Individual Test Plan

## Class Overview

**File**: `MauiApp/Infrastructure/ViewModelBase.cs`  
**Type**: Abstract Base Class (INotifyPropertyChanged)  
**LOC**: 32 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Abstract base class providing fundamental INotifyPropertyChanged implementation for all ViewModels in the MVVM architecture. Offers property change notification infrastructure with caller member name automatic detection and optimized property setting with equality comparison. Essential foundation for data binding and UI reactivity throughout the application.

### Dependencies

- **System.ComponentModel.INotifyPropertyChanged** - Property change notification interface
- **System.ComponentModel.PropertyChangedEventArgs** - Property change event arguments
- **System.Runtime.CompilerServices.CallerMemberNameAttribute** - Automatic property name detection
- **System.Collections.Generic.EqualityComparer&lt;T&gt;** - Generic equality comparison

### Key Responsibilities

1. **Property change notification** - INotifyPropertyChanged event raising infrastructure
2. **Automatic property name detection** - CallerMemberName attribute usage
3. **Optimized property setting** - Equality comparison before change notification
4. **Generic type support** - SetProperty&lt;T&gt; handles any property type
5. **Performance optimization** - Prevents unnecessary change notifications
6. **MVVM foundation** - Base class for all application ViewModels

### Current Architecture Assessment

**Testability Score: 10/10** ✅ **EXCELLENT TESTABILITY**

**Design Strengths:**

1. **Clean abstraction** - Minimal, focused responsibility for property change notification
2. **Generic type support** - SetProperty&lt;T&gt; handles all property types with type safety
3. **Performance optimization** - EqualityComparer prevents unnecessary notifications
4. **Caller member name** - Automatic property name detection reduces errors
5. **Virtual methods** - Extensible design allows derived class customization
6. **Standard interface compliance** - Proper INotifyPropertyChanged implementation
7. **Null-safe implementation** - Safe event invocation with null propagation

**No Design Issues** - This class represents optimal ViewModelBase implementation.

## Usage Scenarios Analysis

### Typical ViewModel Inheritance Patterns

```csharp
public class MyViewModel : ViewModelBase
{
    private string? _name;
    private int _age;
    private bool _isEnabled;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}
```

### Business Logic Applications

- **Data binding foundation** - Automatic UI updates when properties change
- **MVVM pattern support** - Standard ViewModel base implementation
- **Performance optimization** - Prevents redundant UI updates through equality checking
- **Development efficiency** - Reduces boilerplate code in derived ViewModels

## Comprehensive Test Plan

### Test Structure

```
ViewModelBaseTests/
├── PropertyChangedEvent/
│   ├── EventSubscription/
│   │   ├── Should_AllowEventSubscription_Successfully()
│   │   ├── Should_AllowEventUnsubscription_Successfully()
│   │   ├── Should_AllowMultipleSubscribers()
│   │   ├── Should_HandleNullEventHandler_Gracefully()
│   │   └── Should_MaintainEventHandlerReferences_Correctly()
│   ├── EventInvocation/
│   │   ├── Should_InvokePropertyChangedEvent_WhenCalled()
│   │   ├── Should_PassCorrectPropertyName_InEventArgs()
│   │   ├── Should_PassCorrectSender_InEvent()
│   │   ├── Should_HandleNullPropertyName_Gracefully()
│   │   └── Should_HandleEmptyPropertyName_Gracefully()
│   └── CallerMemberName/
│       ├── Should_AutoDetectPropertyName_FromCallerMemberName()
│       ├── Should_UseProvidedPropertyName_WhenSpecified()
│       ├── Should_OverrideCallerMemberName_WithExplicitName()
│       └── Should_HandleNullCallerMemberName_Appropriately()
├── OnPropertyChangedMethod/
│   ├── BasicFunctionality/
│   │   ├── Should_RaisePropertyChangedEvent_WhenCalled()
│   │   ├── Should_PassPropertyName_ToEventArgs()
│   │   ├── Should_PassCurrentInstance_AsSender()
│   │   ├── Should_HandleNullPropertyName_Safely()
│   │   └── Should_AllowMultipleInvocations()
│   ├── VirtualMethodBehavior/
│   │   ├── Should_AllowOverride_InDerivedClasses()
│   │   ├── Should_CallOverriddenImplementation_WhenOverridden()
│   │   ├── Should_MaintainBaseClassBehavior_WhenNotOverridden()
│   │   └── Should_SupportCustomEventHandling_InOverrides()
│   ├── EventHandlerManagement/
│   │   ├── Should_HandleNoSubscribers_Gracefully()
│   │   ├── Should_NotifyAllSubscribers_WhenMultipleExist()
│   │   ├── Should_HandleEventHandlerException_Appropriately()
│   │   └── Should_ContinueNotification_AfterHandlerException()
│   └── CallerMemberNameIntegration/
│       ├── Should_ReceiveCallerMemberName_FromSetProperty()
│       ├── Should_ReceiveCallerMemberName_FromPropertySetters()
│       ├── Should_UseExplicitPropertyName_WhenProvided()
│       └── Should_HandleComplexPropertyNames_Correctly()
├── SetPropertyMethod/
│   ├── ValueTypes/
│   │   ├── Should_SetIntProperty_WhenValueChanges()
│   │   ├── Should_SetBoolProperty_WhenValueChanges()
│   │   ├── Should_SetDoubleProperty_WhenValueChanges()
│   │   ├── Should_SetEnumProperty_WhenValueChanges()
│   │   ├── Should_SetStructProperty_WhenValueChanges()
│   │   └── Should_SetDateTimeProperty_WhenValueChanges()
│   ├── ReferenceTypes/
│   │   ├── Should_SetStringProperty_WhenValueChanges()
│   │   ├── Should_SetObjectProperty_WhenValueChanges()
│   │   ├── Should_SetCollectionProperty_WhenValueChanges()
│   │   ├── Should_SetNullProperty_WhenValueChanges()
│   │   └── Should_SetComplexObjectProperty_WhenValueChanges()
│   ├── EqualityComparison/
│   │   ├── Should_ReturnFalse_WhenValuesAreEqual()
│   │   ├── Should_ReturnTrue_WhenValuesAreDifferent()
│   │   ├── Should_NotRaiseEvent_WhenValuesAreEqual()
│   │   ├── Should_RaiseEvent_WhenValuesAreDifferent()
│   │   ├── Should_UseEqualityComparer_ForComparison()
│   │   └── Should_HandleNullEquality_Correctly()
│   ├── PropertyChangeNotification/
│   │   ├── Should_RaisePropertyChanged_WhenValueChanges()
│   │   ├── Should_NotRaisePropertyChanged_WhenValueSame()
│   │   ├── Should_PassCorrectPropertyName_WhenChanged()
│   │   ├── Should_RaiseEventAfter_SettingField()
│   │   └── Should_UseCallerMemberName_WhenPropertyNameNotProvided()
│   ├── FieldModification/
│   │   ├── Should_UpdateField_WhenValueChanges()
│   │   ├── Should_NotUpdateField_WhenValueSame()
│   │   ├── Should_UpdateFieldBefore_RaisingEvent()
│   │   ├── Should_HandleFieldReference_Correctly()
│   │   └── Should_MaintainFieldValue_AfterUpdate()
│   └── GenericTypeHandling/
│       ├── Should_HandleGenericValueTypes_Correctly()
│       ├── Should_HandleGenericReferenceTypes_Correctly()
│       ├── Should_HandleGenericNullableTypes_Correctly()
│       ├── Should_HandleGenericCollectionTypes_Correctly()
│       └── Should_HandleGenericCustomTypes_Correctly()
├── AbstractClassTesting/
│   ├── ConcreteImplementation/
│   │   ├── Should_InstantiateSuccessfully_WithConcreteClass()
│   │   ├── Should_InheritPropertyChangedEvent_Correctly()
│   │   ├── Should_InheritSetPropertyMethod_Correctly()
│   │   ├── Should_InheritOnPropertyChangedMethod_Correctly()
│   │   └── Should_SupportPolymorphicBehavior_Appropriately()
│   ├── VirtualMethodOverrides/
│   │   ├── Should_AllowOnPropertyChanged_Override()
│   │   ├── Should_AllowSetProperty_Override()
│   │   ├── Should_CallOverriddenMethods_FromDerivedClass()
│   │   └── Should_MaintainBaseClassContract_InOverrides()
│   └── MultipleInheritanceLevels/
│       ├── Should_WorkThroughMultipleInheritanceLevels()
│       ├── Should_MaintainEventChain_ThroughInheritance()
│       ├── Should_SupportNestedPropertyChanges()
│       └── Should_HandleComplexInheritanceHierarchy()
├── PerformanceOptimization/
│   ├── EqualityComparisonEfficiency/
│   │   ├── Should_UseOptimalEqualityComparer_ForValueTypes()
│   │   ├── Should_UseOptimalEqualityComparer_ForReferenceTypes()
│   │   ├── Should_AvoidUnnecessaryComparisons_WhenPossible()
│   │   └── Should_HandleLargeObjectComparison_Efficiently()
│   ├── EventRaisingEfficiency/
│   │   ├── Should_MinimizeEventRaising_WhenValuesUnchanged()
│   │   ├── Should_OptimizeEventArguments_Creation()
│   │   ├── Should_HandleFrequentPropertyChanges_Efficiently()
│   │   └── Should_AvoidMemoryAllocations_WhenPossible()
│   └── PropertyChangePatterns/
│       ├── Should_HandleBatchPropertyChanges_Efficiently()
│       ├── Should_SupportHighFrequencyChanges_WithoutDegradation()
│       ├── Should_HandleLargePropertySets_Efficiently()
│       └── Should_MinimizeGarbageCollection_Impact()
└── IntegrationPatterns/
    ├── MVVMScenarios/
    │   ├── Should_SupportTypicalViewModelScenarios()
    │   ├── Should_IntegrateWithDataBinding_Correctly()
    │   ├── Should_HandleComplexPropertyDependencies()
    │   └── Should_SupportPropertyValidation_Patterns()
    ├── UIBindingPatterns/
    │   ├── Should_NotifyUI_WhenPropertiesChange()
    │   ├── Should_SupportTwoWayDataBinding()
    │   ├── Should_HandleCollectionPropertyBinding()
    │   └── Should_WorkWithConverters_Appropriately()
    ├── LifecycleManagement/
    │   ├── Should_HandleViewModelLifecycle_Properly()
    │   ├── Should_CleanupEventSubscriptions_Appropriately()
    │   ├── Should_SupportViewModelReuse_Safely()
    │   └── Should_HandleViewModelDisposal_Gracefully()
    └── ConcurrencyPatterns/
        ├── Should_HandleConcurrentPropertyChanges_Safely()
        ├── Should_MaintainEventConsistency_UnderConcurrency()
        ├── Should_SupportThreadSafePropertyAccess()
        └── Should_HandleCrossThreadNotification_Appropriately()
```

### Test Implementation Examples

#### Concrete Test Implementation for Abstract Class

```csharp
// Concrete test implementation for abstract ViewModelBase
public class TestViewModel : ViewModelBase
{
    private string? _name;
    private int _age;
    private bool _isEnabled;
    private object? _data;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public object? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    // Expose protected methods for testing
    public new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
    }

    public new bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        return base.SetProperty(ref field, value, propertyName);
    }
}

// Custom ViewModelBase for testing virtual method overrides
public class CustomViewModel : ViewModelBase
{
    public List<string> OnPropertyChangedCalls { get; } = new();
    public bool SuppressPropertyChanged { get; set; }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChangedCalls.Add(propertyName ?? "null");
        
        if (!SuppressPropertyChanged)
        {
            base.OnPropertyChanged(propertyName);
        }
    }

    private string? _customProperty;
    public string? CustomProperty
    {
        get => _customProperty;
        set => SetProperty(ref _customProperty, value);
    }
}
```

#### PropertyChanged Event Tests

```csharp
[Test]
public void PropertyChangedEvent_Should_AllowEventSubscription_Successfully()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventFired = false;
    PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

    // Act
    viewModel.PropertyChanged += handler;
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void PropertyChangedEvent_Should_AllowEventUnsubscription_Successfully()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventFired = false;
    PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.PropertyChanged -= handler;
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(eventFired, Is.False);
}

[Test]
public void PropertyChangedEvent_Should_AllowMultipleSubscribers()
{
    // Arrange
    var viewModel = new TestViewModel();
    var event1Fired = false;
    var event2Fired = false;
    var event3Fired = false;

    PropertyChangedEventHandler handler1 = (sender, args) => event1Fired = true;
    PropertyChangedEventHandler handler2 = (sender, args) => event2Fired = true;
    PropertyChangedEventHandler handler3 = (sender, args) => event3Fired = true;

    // Act
    viewModel.PropertyChanged += handler1;
    viewModel.PropertyChanged += handler2;
    viewModel.PropertyChanged += handler3;

    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(event1Fired, Is.True);
    Assert.That(event2Fired, Is.True);
    Assert.That(event3Fired, Is.True);

    // Cleanup
    viewModel.PropertyChanged -= handler1;
    viewModel.PropertyChanged -= handler2;
    viewModel.PropertyChanged -= handler3;
}

[Test]
public void PropertyChangedEvent_Should_PassCorrectPropertyName_InEventArgs()
{
    // Arrange
    var viewModel = new TestViewModel();
    string? receivedPropertyName = null;
    PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(receivedPropertyName, Is.EqualTo("TestProperty"));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void PropertyChangedEvent_Should_PassCorrectSender_InEvent()
{
    // Arrange
    var viewModel = new TestViewModel();
    object? receivedSender = null;
    PropertyChangedEventHandler handler = (sender, args) => receivedSender = sender;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(receivedSender, Is.EqualTo(viewModel));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}
```

#### OnPropertyChanged Method Tests

```csharp
[Test]
public void OnPropertyChanged_Should_RaisePropertyChangedEvent_WhenCalled()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventFired = false;
    PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(eventFired, Is.True);

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void OnPropertyChanged_Should_HandleNullPropertyName_Safely()
{
    // Arrange
    var viewModel = new TestViewModel();
    string? receivedPropertyName = "initial";
    PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.OnPropertyChanged(null);

    // Assert
    Assert.That(receivedPropertyName, Is.Null);

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void OnPropertyChanged_Should_HandleNoSubscribers_Gracefully()
{
    // Arrange
    var viewModel = new TestViewModel();

    // Act & Assert
    Assert.DoesNotThrow(() => viewModel.OnPropertyChanged("TestProperty"));
}

[Test]
public void OnPropertyChanged_Should_AllowOverride_InDerivedClasses()
{
    // Arrange
    var viewModel = new CustomViewModel();

    // Act
    viewModel.OnPropertyChanged("TestProperty");

    // Assert
    Assert.That(viewModel.OnPropertyChangedCalls, Contains.Item("TestProperty"));
}

[Test]
public void OnPropertyChanged_Should_CallOverriddenImplementation_WhenOverridden()
{
    // Arrange
    var viewModel = new CustomViewModel();
    var eventFired = false;
    PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.CustomProperty = "test";

    // Assert
    Assert.That(viewModel.OnPropertyChangedCalls, Contains.Item("CustomProperty"));
    Assert.That(eventFired, Is.True);

    // Cleanup
    viewModel.PropertyChanged -= handler;
}
```

#### SetProperty Method Tests

```csharp
[Test]
public void SetProperty_Should_SetIntProperty_WhenValueChanges()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventFired = false;
    PropertyChangedEventHandler handler = (sender, args) => eventFired = true;

    viewModel.PropertyChanged += handler;

    // Act
    var result = viewModel.Age = 25;

    // Assert
    Assert.That(viewModel.Age, Is.EqualTo(25));
    Assert.That(eventFired, Is.True);

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_SetStringProperty_WhenValueChanges()
{
    // Arrange
    var viewModel = new TestViewModel();
    var propertyName = "";
    PropertyChangedEventHandler handler = (sender, args) => propertyName = args.PropertyName ?? "";

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Name = "Test Name";

    // Assert
    Assert.That(viewModel.Name, Is.EqualTo("Test Name"));
    Assert.That(propertyName, Is.EqualTo("Name"));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_ReturnFalse_WhenValuesAreEqual()
{
    // Arrange
    var viewModel = new TestViewModel();
    var field = 42;

    // Act
    var result1 = viewModel.SetProperty(ref field, 42);
    var result2 = viewModel.SetProperty(ref field, 42);

    // Assert
    Assert.That(result1, Is.False);
    Assert.That(result2, Is.False);
    Assert.That(field, Is.EqualTo(42));
}

[Test]
public void SetProperty_Should_ReturnTrue_WhenValuesAreDifferent()
{
    // Arrange
    var viewModel = new TestViewModel();
    var field = 42;

    // Act
    var result = viewModel.SetProperty(ref field, 100);

    // Assert
    Assert.That(result, Is.True);
    Assert.That(field, Is.EqualTo(100));
}

[Test]
public void SetProperty_Should_NotRaiseEvent_WhenValuesAreEqual()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;
    viewModel.Age = 25;
    eventCount = 0; // Reset counter

    // Act
    viewModel.Age = 25; // Set to same value

    // Assert
    Assert.That(eventCount, Is.EqualTo(0));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_RaiseEvent_WhenValuesAreDifferent()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Age = 25;
    viewModel.Age = 30;

    // Assert
    Assert.That(eventCount, Is.EqualTo(2));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_UseCallerMemberName_WhenPropertyNameNotProvided()
{
    // Arrange
    var viewModel = new TestViewModel();
    string? receivedPropertyName = null;
    PropertyChangedEventHandler handler = (sender, args) => receivedPropertyName = args.PropertyName;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Name = "Test";

    // Assert
    Assert.That(receivedPropertyName, Is.EqualTo("Name"));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_UpdateFieldBefore_RaisingEvent()
{
    // Arrange
    var viewModel = new TestViewModel();
    var fieldValue = "";
    PropertyChangedEventHandler handler = (sender, args) => fieldValue = viewModel.Name ?? "";

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Name = "Updated Value";

    // Assert
    Assert.That(fieldValue, Is.EqualTo("Updated Value"));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_HandleNullEquality_Correctly()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Name = null;
    viewModel.Name = null; // Set to null again
    viewModel.Name = "test";
    viewModel.Name = null; // Back to null

    // Assert
    Assert.That(eventCount, Is.EqualTo(3)); // null -> null (no event), null -> "test" (event), "test" -> null (event)

    // Cleanup
    viewModel.PropertyChanged -= handler;
}
```

#### Generic Type Handling Tests

```csharp
[Test]
public void SetProperty_Should_HandleGenericValueTypes_Correctly()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Age = 25;      // int
    viewModel.IsEnabled = true;  // bool

    // Assert
    Assert.That(viewModel.Age, Is.EqualTo(25));
    Assert.That(viewModel.IsEnabled, Is.True);
    Assert.That(eventCount, Is.EqualTo(2));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_HandleGenericReferenceTypes_Correctly()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    var receivedPropertyNames = new List<string?>();
    PropertyChangedEventHandler handler = (sender, args) => 
    {
        eventCount++;
        receivedPropertyNames.Add(args.PropertyName);
    };

    viewModel.PropertyChanged += handler;

    var testObject = new object();

    // Act
    viewModel.Name = "Test String";
    viewModel.Data = testObject;

    // Assert
    Assert.That(viewModel.Name, Is.EqualTo("Test String"));
    Assert.That(viewModel.Data, Is.EqualTo(testObject));
    Assert.That(eventCount, Is.EqualTo(2));
    Assert.That(receivedPropertyNames, Is.EqualTo(new[] { "Name", "Data" }));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

[Test]
public void SetProperty_Should_HandleGenericNullableTypes_Correctly()
{
    // Arrange
    var viewModel = new GenericTestViewModel<int?>();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.Value = 42;
    viewModel.Value = null;
    viewModel.Value = 100;

    // Assert
    Assert.That(viewModel.Value, Is.EqualTo(100));
    Assert.That(eventCount, Is.EqualTo(3));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

// Generic test ViewModel for testing generic type scenarios
public class GenericTestViewModel<T> : ViewModelBase
{
    private T? _value;

    public T? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
}

[Test]
public void SetProperty_Should_HandleGenericCollectionTypes_Correctly()
{
    // Arrange
    var viewModel = new GenericTestViewModel<List<string>>();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;

    var list1 = new List<string> { "item1", "item2" };
    var list2 = new List<string> { "item3", "item4" };

    // Act
    viewModel.Value = list1;
    viewModel.Value = list1; // Same reference, should not raise event
    viewModel.Value = list2; // Different reference, should raise event

    // Assert
    Assert.That(viewModel.Value, Is.EqualTo(list2));
    Assert.That(eventCount, Is.EqualTo(2));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}
```

#### Performance Optimization Tests

```csharp
[Test]
public void Performance_Should_UseOptimalEqualityComparer_ForValueTypes()
{
    // Arrange
    var viewModel = new TestViewModel();
    var sw = Stopwatch.StartNew();

    // Act
    for (int i = 0; i < 10000; i++)
    {
        viewModel.Age = i;
    }

    sw.Stop();

    // Assert
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100)); // Should be very fast
    Assert.That(viewModel.Age, Is.EqualTo(9999));
}

[Test]
public void Performance_Should_MinimizeEventRaising_WhenValuesUnchanged()
{
    // Arrange
    var viewModel = new TestViewModel();
    var eventCount = 0;
    PropertyChangedEventHandler handler = (sender, args) => eventCount++;

    viewModel.PropertyChanged += handler;
    viewModel.Age = 25;
    eventCount = 0; // Reset

    // Act
    for (int i = 0; i < 1000; i++)
    {
        viewModel.Age = 25; // Same value each time
    }

    // Assert
    Assert.That(eventCount, Is.EqualTo(0)); // No events should be raised

    // Cleanup
    viewModel.PropertyChanged -= handler;
}
```

#### Integration Pattern Tests

```csharp
[Test]
public void MVVMIntegration_Should_SupportTypicalViewModelScenarios()
{
    // Arrange
    var viewModel = new ComplexTestViewModel();
    var propertyChanges = new List<string?>();
    PropertyChangedEventHandler handler = (sender, args) => propertyChanges.Add(args.PropertyName);

    viewModel.PropertyChanged += handler;

    // Act
    viewModel.FirstName = "John";
    viewModel.LastName = "Doe";
    viewModel.Age = 30;

    // Assert
    Assert.That(propertyChanges, Is.EqualTo(new[] { "FirstName", "LastName", "Age", "FullName" }));
    Assert.That(viewModel.FullName, Is.EqualTo("John Doe"));

    // Cleanup
    viewModel.PropertyChanged -= handler;
}

public class ComplexTestViewModel : ViewModelBase
{
    private string? _firstName;
    private string? _lastName;
    private int _age;

    public string? FirstName
    {
        get => _firstName;
        set
        {
            if (SetProperty(ref _firstName, value))
            {
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string? LastName
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
```

### Test Fixtures Required

- **ViewModelBaseTestFixture** - Standard test fixture with cleanup
- **ConcreteViewModelImplementation** - Test implementation of abstract base
- **CustomViewModelImplementation** - Test implementation with virtual method overrides
- **GenericViewModelImplementation** - Generic test implementation for type testing
- **PropertyChangedEventValidator** - Helper for validating event behavior

## Success Criteria

- [ ] **PropertyChanged event** - Proper event subscription/unsubscription and invocation
- [ ] **OnPropertyChanged method** - Correct event raising and parameter handling
- [ ] **SetProperty method** - Optimized property setting with equality comparison
- [ ] **Generic type support** - Works correctly with all property types
- [ ] **CallerMemberName** - Automatic property name detection
- [ ] **Virtual method overrides** - Extensible design in derived classes
- [ ] **Performance optimization** - Minimal overhead and efficient equality comparison
- [ ] **MVVM integration** - Works correctly in typical ViewModel scenarios

## Implementation Priority

**HIGH PRIORITY** - Critical MVVM infrastructure component with excellent testability. Foundation for all ViewModels.

## Dependencies for Testing

- **NUnit** - Standard testing framework
- **System.Diagnostics.Stopwatch** - Performance measurement
- **Custom test ViewModels** - Concrete implementations for testing
- **Property change validation helpers** - Event verification utilities

## Implementation Estimate

**Effort: Medium (1.0 day)**

- Abstract class testing patterns
- Generic type testing across multiple scenarios
- Property change notification verification
- Performance optimization validation
- Virtual method override testing
- MVVM integration pattern verification

This class represents a fundamental MVVM component requiring thorough testing to ensure reliable property change notification throughout the application architecture.