# MorningReminderEventArgs - Individual Test Plan

## Class Overview
**File**: `MauiApp/Services/MoodDispatcherService.cs` (lines 335-342)  
**Type**: Event Arguments Class  
**LOC**: 8 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Event arguments class that carries morning reminder information from `MoodDispatcherService` to event subscribers (primarily `MainPageViewModel`). Extends the standard `EventArgs` pattern to provide structured data for morning mood reminder notifications, enabling UI components to display appropriate alerts and messages.

### Dependencies
- **System.EventArgs** - Base class for event argument patterns
- **Framework**: Standard .NET DateTime, TimeSpan, and string types

### Key Responsibilities
1. **Event data transport** - Carry structured reminder data between services and UI
2. **Timing information** - Store MorningTime (when reminder was scheduled)
3. **Duration tracking** - Store TimeSinceMorning (how late the reminder is)
4. **State information** - Store CallCount (reminder frequency tracking)
5. **User messaging** - Store Message (user-facing reminder text)

### Current Architecture Assessment
**Testability Score: 10/10** ✅ **OUTSTANDING TESTABILITY**

**Excellent Design Elements:**
1. **Pure event args class** - No business logic, external dependencies, or side effects
2. **Simple properties** - Basic auto-properties with clear data types
3. **Standard pattern** - Follows .NET EventArgs convention properly
4. **Immutable-friendly** - Can be easily tested with various property combinations
5. **Type safety** - Uses appropriate .NET types with sensible defaults
6. **Clear purpose** - Single responsibility as event data container

**No Refactoring Required** - This class exemplifies excellent testable design following .NET conventions.

## Usage Analysis

### Event Declaration Pattern
```csharp
// Declared in MoodDispatcherService
public event EventHandler<MorningReminderEventArgs>? MorningReminderOccurred;
```

### Event Creation Pattern
```csharp
// Created in MoodDispatcherService from MorningReminderData
MorningReminderOccurred?.Invoke(this, new MorningReminderEventArgs
{
    MorningTime = reminderData.MorningTime,
    TimeSinceMorning = reminderData.TimeSinceMorning,
    CallCount = reminderData.CallCount,
    Message = result.Message ?? "Time to record your morning mood!"
});
```

### Event Consumption Pattern
```csharp
// Consumed in MainPageViewModel
private async void OnMorningReminderOccurred(object? sender, MorningReminderEventArgs e)
{
    _loggingService.Log($"MainPageViewModel: Morning reminder triggered - {e.Message}");
    await _windowActivationService.ActivateCurrentWindowAsync();
    var alertArgs = new DisplayAlertEventArgs("Morning Mood Reminder", e.Message, "OK");
    DisplayAlert?.Invoke(this, alertArgs);
}
```

## Comprehensive Test Plan

### Test Structure
```
MorningReminderEventArgsTests/
├── Constructor/
│   ├── Should_InitializeWithDefaultValues_WhenCreatedWithDefaultConstructor()
│   ├── Should_InheritFromEventArgs_WhenCreated()
│   └── Should_AllowParameterlessConstruction()
├── Properties/
│   ├── MorningTime/
│   │   ├── Should_SetAndGetMorningTime_WhenAssigned()
│   │   ├── Should_HandleDateTimeMinValue_WhenAssigned()
│   │   ├── Should_HandleDateTimeMaxValue_WhenAssigned()
│   │   └── Should_HandleSpecificDateTime_WhenAssigned()
│   ├── TimeSinceMorning/
│   │   ├── Should_SetAndGetTimeSinceMorning_WhenAssigned()
│   │   ├── Should_HandleTimeSpanZero_WhenAssigned()
│   │   ├── Should_HandlePositiveTimeSpan_WhenAssigned()
│   │   ├── Should_HandleNegativeTimeSpan_WhenAssigned()
│   │   └── Should_HandleLargeTimeSpan_WhenAssigned()
│   ├── CallCount/
│   │   ├── Should_SetAndGetCallCount_WhenAssigned()
│   │   ├── Should_HandleZeroCallCount_WhenAssigned()
│   │   ├── Should_HandlePositiveCallCount_WhenAssigned()
│   │   ├── Should_HandleNegativeCallCount_WhenAssigned()
│   │   └── Should_HandleLargeCallCount_WhenAssigned()
│   └── Message/
│       ├── Should_SetAndGetMessage_WhenAssigned()
│       ├── Should_DefaultToEmptyString_WhenNotAssigned()
│       ├── Should_HandleNullMessage_WhenAssigned()
│       ├── Should_HandleEmptyMessage_WhenAssigned()
│       ├── Should_HandleWhitespaceMessage_WhenAssigned()
│       └── Should_HandleLongMessage_WhenAssigned()
├── ObjectInitializer/
│   ├── Should_InitializeAllProperties_WithObjectInitializerSyntax()
│   ├── Should_InitializePartialProperties_WithObjectInitializerSyntax()
│   └── Should_HandleMixedPropertyTypes_WithObjectInitializerSyntax()
├── Inheritance/
│   ├── Should_BeAssignableToEventArgs_WhenCreated()
│   ├── Should_BeUsableInEventHandlerSignature_WhenCreated()
│   └── Should_SupportsEventArgsPolymorphism_WhenUsedInGenericContext()
├── Equality/ (if implementing IEquatable)
│   ├── Should_BeEqual_WhenAllPropertiesMatch()
│   ├── Should_NotBeEqual_WhenMorningTimeDiffers()
│   ├── Should_NotBeEqual_WhenTimeSinceMorningDiffers()
│   ├── Should_NotBeEqual_WhenCallCountDiffers()
│   ├── Should_NotBeEqual_WhenMessageDiffers()
│   └── Should_HandleNullComparison_Appropriately()
└── EventUsage/
    ├── Should_SupportEventHandlerPattern_WithCorrectSignature()
    ├── Should_CarryDataCorrectly_ThroughEventInvocation()
    └── Should_BeSerializable_ForEventSystemCompatibility()
```

### Test Implementation Examples

#### Basic Property Tests
```csharp
[Test]
public void MorningTime_Should_SetAndGetMorningTime_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var expectedMorningTime = new DateTime(2024, 10, 17, 9, 0, 0);

    // Act
    eventArgs.MorningTime = expectedMorningTime;

    // Assert
    Assert.That(eventArgs.MorningTime, Is.EqualTo(expectedMorningTime));
}

[Test]
public void TimeSinceMorning_Should_SetAndGetTimeSinceMorning_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var expectedTimeSpan = TimeSpan.FromMinutes(8.5);

    // Act
    eventArgs.TimeSinceMorning = expectedTimeSpan;

    // Assert
    Assert.That(eventArgs.TimeSinceMorning, Is.EqualTo(expectedTimeSpan));
}

[Test]
public void CallCount_Should_SetAndGetCallCount_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var expectedCallCount = 6;

    // Act
    eventArgs.CallCount = expectedCallCount;

    // Assert
    Assert.That(eventArgs.CallCount, Is.EqualTo(expectedCallCount));
}

[Test]
public void Message_Should_SetAndGetMessage_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var expectedMessage = "Time to record your morning mood!";

    // Act
    eventArgs.Message = expectedMessage;

    // Assert
    Assert.That(eventArgs.Message, Is.EqualTo(expectedMessage));
}

[Test]
public void Message_Should_DefaultToEmptyString_WhenNotAssigned()
{
    // Arrange & Act
    var eventArgs = new MorningReminderEventArgs();

    // Assert
    Assert.That(eventArgs.Message, Is.EqualTo(string.Empty));
}
```

#### Inheritance Tests
```csharp
[Test]
public void Constructor_Should_InheritFromEventArgs_WhenCreated()
{
    // Arrange & Act
    var eventArgs = new MorningReminderEventArgs();

    // Assert
    Assert.That(eventArgs, Is.InstanceOf<EventArgs>());
}

[Test]
public void Inheritance_Should_BeAssignableToEventArgs_WhenCreated()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();

    // Act
    EventArgs baseEventArgs = eventArgs;

    // Assert
    Assert.That(baseEventArgs, Is.Not.Null);
    Assert.That(baseEventArgs, Is.SameAs(eventArgs));
}

[Test]
public void Inheritance_Should_BeUsableInEventHandlerSignature_WhenCreated()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs
    {
        MorningTime = new DateTime(2024, 10, 17, 9, 0, 0),
        Message = "Test reminder"
    };
    
    MorningReminderEventArgs? receivedEventArgs = null;
    EventHandler<MorningReminderEventArgs> handler = (sender, e) => receivedEventArgs = e;

    // Act
    handler.Invoke(this, eventArgs);

    // Assert
    Assert.That(receivedEventArgs, Is.Not.Null);
    Assert.That(receivedEventArgs, Is.SameAs(eventArgs));
    Assert.That(receivedEventArgs.Message, Is.EqualTo("Test reminder"));
}
```

#### Object Initializer Tests
```csharp
[Test]
public void ObjectInitializer_Should_InitializeAllProperties_WithObjectInitializerSyntax()
{
    // Arrange
    var expectedMorningTime = new DateTime(2024, 10, 17, 9, 15, 30);
    var expectedTimeSince = TimeSpan.FromMinutes(12.5);
    var expectedCallCount = 8;
    var expectedMessage = "You're 12.5 minutes late for your morning mood check!";

    // Act
    var eventArgs = new MorningReminderEventArgs
    {
        MorningTime = expectedMorningTime,
        TimeSinceMorning = expectedTimeSince,
        CallCount = expectedCallCount,
        Message = expectedMessage
    };

    // Assert
    Assert.That(eventArgs.MorningTime, Is.EqualTo(expectedMorningTime));
    Assert.That(eventArgs.TimeSinceMorning, Is.EqualTo(expectedTimeSince));
    Assert.That(eventArgs.CallCount, Is.EqualTo(expectedCallCount));
    Assert.That(eventArgs.Message, Is.EqualTo(expectedMessage));
}

[Test]
public void ObjectInitializer_Should_InitializePartialProperties_WithObjectInitializerSyntax()
{
    // Arrange & Act
    var eventArgs = new MorningReminderEventArgs
    {
        MorningTime = new DateTime(2024, 10, 17, 9, 0, 0),
        Message = "Morning reminder!"
        // TimeSinceMorning and CallCount left as defaults
    };

    // Assert
    Assert.That(eventArgs.MorningTime, Is.EqualTo(new DateTime(2024, 10, 17, 9, 0, 0)));
    Assert.That(eventArgs.TimeSinceMorning, Is.EqualTo(default(TimeSpan)));
    Assert.That(eventArgs.CallCount, Is.EqualTo(default(int)));
    Assert.That(eventArgs.Message, Is.EqualTo("Morning reminder!"));
}
```

#### Message Property Edge Cases
```csharp
[Test]
public void Message_Should_HandleNullMessage_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();

    // Act
    eventArgs.Message = null!; // Intentional null assignment

    // Assert
    Assert.That(eventArgs.Message, Is.Null);
}

[Test]
public void Message_Should_HandleEmptyMessage_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();

    // Act
    eventArgs.Message = "";

    // Assert
    Assert.That(eventArgs.Message, Is.EqualTo(""));
}

[Test]
public void Message_Should_HandleWhitespaceMessage_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var whitespaceMessage = "   \t\n   ";

    // Act
    eventArgs.Message = whitespaceMessage;

    // Assert
    Assert.That(eventArgs.Message, Is.EqualTo(whitespaceMessage));
}

[Test]
public void Message_Should_HandleLongMessage_WhenAssigned()
{
    // Arrange
    var eventArgs = new MorningReminderEventArgs();
    var longMessage = new string('A', 1000); // 1000 character message

    // Act
    eventArgs.Message = longMessage;

    // Assert
    Assert.That(eventArgs.Message, Is.EqualTo(longMessage));
    Assert.That(eventArgs.Message.Length, Is.EqualTo(1000));
}
```

#### Real-world Event Usage Tests
```csharp
[Test]
public void EventUsage_Should_CarryDataCorrectly_ThroughEventInvocation()
{
    // Arrange
    var expectedMorningTime = new DateTime(2024, 10, 17, 9, 7, 30);
    var expectedTimeSince = TimeSpan.FromMinutes(7.5);
    var expectedCallCount = 4;
    var expectedMessage = "Time to record your morning mood!";
    
    MorningReminderEventArgs? capturedEventArgs = null;
    object? capturedSender = null;
    
    EventHandler<MorningReminderEventArgs> handler = (sender, e) =>
    {
        capturedSender = sender;
        capturedEventArgs = e;
    };

    var eventArgs = new MorningReminderEventArgs
    {
        MorningTime = expectedMorningTime,
        TimeSinceMorning = expectedTimeSince,
        CallCount = expectedCallCount,
        Message = expectedMessage
    };
    
    var mockSender = new object();

    // Act
    handler.Invoke(mockSender, eventArgs);

    // Assert
    Assert.That(capturedSender, Is.SameAs(mockSender));
    Assert.That(capturedEventArgs, Is.Not.Null);
    Assert.That(capturedEventArgs.MorningTime, Is.EqualTo(expectedMorningTime));
    Assert.That(capturedEventArgs.TimeSinceMorning, Is.EqualTo(expectedTimeSince));
    Assert.That(capturedEventArgs.CallCount, Is.EqualTo(expectedCallCount));
    Assert.That(capturedEventArgs.Message, Is.EqualTo(expectedMessage));
}

[Test]
public void EventUsage_Should_SimulateRealWorldReminderScenario()
{
    // Arrange - Simulate typical morning reminder event scenario
    var morningScheduledTime = new DateTime(2024, 10, 17, 9, 0, 0);
    var currentTime = new DateTime(2024, 10, 17, 9, 8, 15);
    var timeSinceScheduled = currentTime - morningScheduledTime;
    var reminderCallCount = 6;
    var reminderMessage = "Good morning! Don't forget to record how you're feeling this morning.";

    // Act - Create event args as MoodDispatcherService would
    var eventArgs = new MorningReminderEventArgs
    {
        MorningTime = morningScheduledTime,
        TimeSinceMorning = timeSinceScheduled,
        CallCount = reminderCallCount,
        Message = reminderMessage
    };

    // Assert - Verify data integrity for UI consumption
    Assert.That(eventArgs.MorningTime, Is.EqualTo(morningScheduledTime));
    Assert.That(eventArgs.TimeSinceMorning, Is.EqualTo(TimeSpan.FromMinutes(8.25)));
    Assert.That(eventArgs.CallCount, Is.EqualTo(6));
    Assert.That(eventArgs.Message, Is.EqualTo(reminderMessage));
    
    // Verify event args can be used in UI context
    Assert.That(eventArgs.TimeSinceMorning.TotalMinutes, Is.GreaterThan(0));
    Assert.That(eventArgs.TimeSinceMorning.TotalMinutes, Is.LessThan(10)); // Within expected window
    Assert.That(eventArgs.Message, Is.Not.Empty);
}
```

#### Default Values Test
```csharp
[Test]
public void Constructor_Should_InitializeWithDefaultValues_WhenCreatedWithDefaultConstructor()
{
    // Act
    var eventArgs = new MorningReminderEventArgs();

    // Assert
    Assert.That(eventArgs.MorningTime, Is.EqualTo(default(DateTime)));
    Assert.That(eventArgs.TimeSinceMorning, Is.EqualTo(default(TimeSpan)));
    Assert.That(eventArgs.CallCount, Is.EqualTo(default(int)));
    Assert.That(eventArgs.Message, Is.EqualTo(string.Empty)); // Explicitly set default
}
```

### Test Fixtures Required
- **EventHandlerTestFixture** - Helper methods for testing event handler patterns
- **MessageTestData** - Test data for various message scenarios (null, empty, long, etc.)
- **EventArgsFactory** - Create test event args with various configurations

### Integration Test Considerations
1. **Event system compatibility** - Verify works with .NET event infrastructure
2. **Serialization support** - If events need to cross boundaries or be persisted
3. **Thread safety** - If events might be raised from different threads
4. **Memory management** - Ensure no memory leaks from event handlers

## Success Criteria
- [ ] **100% line coverage** (should be trivial for an 8-line class)
- [ ] **100% branch coverage** (no branching logic in properties)
- [ ] **Property validation** - All properties can be set and retrieved correctly
- [ ] **Edge case handling** - DateTime.Min/Max, null messages, negative values tested
- [ ] **EventArgs inheritance** - Proper inheritance behavior validated
- [ ] **Event system compatibility** - Works correctly with .NET event handlers
- [ ] **Default value correctness** - Message defaults to empty string as expected

## Implementation Priority
**LOW PRIORITY** - Simple event args class with excellent testability. Can be implemented quickly as part of comprehensive test suite coverage goals.

## Dependencies for Testing
- **None** - Pure unit testing with no external dependencies
- Standard NUnit testing framework sufficient
- EventHandler delegates for event pattern testing

## Implementation Estimate
**Effort: Very Low (30 minutes - 1 hour)**
- Create test class with comprehensive property coverage
- Add edge case tests for all property types
- Verify EventArgs inheritance behavior
- Add event handler pattern tests
- Test default value behavior and object initializers

This is another example of a perfectly testable class that requires minimal effort but contributes to overall coverage metrics and validates the event communication patterns used throughout the application.