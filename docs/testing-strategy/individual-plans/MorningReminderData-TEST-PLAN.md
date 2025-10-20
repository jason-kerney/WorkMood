# MorningReminderData - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview
**File**: `MauiApp/Services/MorningReminderCommand.cs` (lines 112-117)  
**Type**: Data Transfer Object (DTO)  
**LOC**: 6 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Simple data class that carries morning reminder information from `MorningReminderCommand` to event consumers via the `MoodDispatcherService`. Acts as a structured data container for timing and state information related to morning mood reminders.

### Dependencies
- **None** - Pure data class with no external dependencies
- **Framework**: Standard .NET DateTime and TimeSpan types

### Key Responsibilities
1. **Data transport** - Carry structured reminder data between services
2. **Timing information** - Store MorningTime (when reminder was scheduled)
3. **Duration tracking** - Store TimeSinceMorning (how late the reminder is)
4. **State information** - Store CallCount (reminder frequency tracking)

### Current Architecture Assessment
**Testability Score: 10/10** ✅ **OUTSTANDING TESTABILITY**

**Excellent Design Elements:**
1. **Pure data class** - No business logic, external dependencies, or side effects
2. **Simple properties** - Basic auto-properties with clear data types
3. **Immutable-friendly** - Can be easily tested with various property combinations
4. **Clear purpose** - Single responsibility as data container
5. **Type safety** - Uses appropriate .NET types (DateTime, TimeSpan, int)

**No Refactoring Required** - This class exemplifies excellent testable design.

## Usage Analysis

### Creation Pattern
```csharp
// Created in MorningReminderCommand.ProcessTickAsync()
var reminderData = new MorningReminderData
{
    MorningTime = morningTime,
    TimeSinceMorning = timeSinceMorning,
    CallCount = _callCount
};
```

### Consumption Pattern
```csharp
// Consumed in MoodDispatcherService
if (result.Success && result.Data is MorningReminderData reminderData)
{
    MorningReminderOccurred?.Invoke(this, new MorningReminderEventArgs
    {
        MorningTime = reminderData.MorningTime,
        TimeSinceMorning = reminderData.TimeSinceMorning,
        CallCount = reminderData.CallCount,
        Message = result.Message ?? "Time to record your morning mood!"
    });
}
```

## Comprehensive Test Plan

### Test Structure
```
MorningReminderDataTests/
├── Constructor/
│   ├── Should_InitializeWithDefaultValues_WhenCreatedWithDefaultConstructor()
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
│   └── CallCount/
│       ├── Should_SetAndGetCallCount_WhenAssigned()
│       ├── Should_HandleZeroCallCount_WhenAssigned()
│       ├── Should_HandlePositiveCallCount_WhenAssigned()
│       ├── Should_HandleNegativeCallCount_WhenAssigned()
│       └── Should_HandleLargeCallCount_WhenAssigned()
├── ObjectInitializer/
│   ├── Should_InitializeAllProperties_WithObjectInitializerSyntax()
│   ├── Should_InitializePartialProperties_WithObjectInitializerSyntax()
│   └── Should_HandleMixedPropertyTypes_WithObjectInitializerSyntax()
├── Equality/ (if implementing IEquatable)
│   ├── Should_BeEqual_WhenAllPropertiesMatch()
│   ├── Should_NotBeEqual_WhenMorningTimeDiffers()
│   ├── Should_NotBeEqual_WhenTimeSinceMorningDiffers()
│   ├── Should_NotBeEqual_WhenCallCountDiffers()
│   └── Should_HandleNullComparison_Appropriately()
└── Serialization/ (if used with JSON)
    ├── Should_SerializeToJson_WithAllProperties()
    ├── Should_DeserializeFromJson_WithAllProperties()
    └── Should_HandleRoundTripSerialization_WithoutDataLoss()
```

### Test Implementation Examples

#### Basic Property Tests
```csharp
[Test]
public void MorningTime_Should_SetAndGetMorningTime_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();
    var expectedMorningTime = new DateTime(2024, 10, 17, 9, 0, 0);

    // Act
    reminderData.MorningTime = expectedMorningTime;

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(expectedMorningTime));
}

[Test]
public void TimeSinceMorning_Should_SetAndGetTimeSinceMorning_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();
    var expectedTimeSpan = TimeSpan.FromMinutes(7.5);

    // Act
    reminderData.TimeSinceMorning = expectedTimeSpan;

    // Assert
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(expectedTimeSpan));
}

[Test]
public void CallCount_Should_SetAndGetCallCount_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();
    var expectedCallCount = 4;

    // Act
    reminderData.CallCount = expectedCallCount;

    // Assert
    Assert.That(reminderData.CallCount, Is.EqualTo(expectedCallCount));
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
    var expectedCallCount = 6;

    // Act
    var reminderData = new MorningReminderData
    {
        MorningTime = expectedMorningTime,
        TimeSinceMorning = expectedTimeSince,
        CallCount = expectedCallCount
    };

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(expectedMorningTime));
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(expectedTimeSince));
    Assert.That(reminderData.CallCount, Is.EqualTo(expectedCallCount));
}

[Test]
public void ObjectInitializer_Should_InitializePartialProperties_WithObjectInitializerSyntax()
{
    // Arrange & Act
    var reminderData = new MorningReminderData
    {
        MorningTime = new DateTime(2024, 10, 17, 9, 0, 0),
        CallCount = 2
        // TimeSinceMorning left as default
    };

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(new DateTime(2024, 10, 17, 9, 0, 0)));
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(default(TimeSpan)));
    Assert.That(reminderData.CallCount, Is.EqualTo(2));
}
```

#### Default Value Tests
```csharp
[Test]
public void Constructor_Should_InitializeWithDefaultValues_WhenCreatedWithDefaultConstructor()
{
    // Act
    var reminderData = new MorningReminderData();

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(default(DateTime)));
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(default(TimeSpan)));
    Assert.That(reminderData.CallCount, Is.EqualTo(default(int)));
}
```

#### Edge Case Tests
```csharp
[Test]
public void MorningTime_Should_HandleDateTimeMinValue_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();

    // Act
    reminderData.MorningTime = DateTime.MinValue;

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(DateTime.MinValue));
}

[Test]
public void MorningTime_Should_HandleDateTimeMaxValue_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();

    // Act
    reminderData.MorningTime = DateTime.MaxValue;

    // Assert
    Assert.That(reminderData.MorningTime, Is.EqualTo(DateTime.MaxValue));
}

[Test]
public void TimeSinceMorning_Should_HandleNegativeTimeSpan_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();
    var negativeTimeSpan = TimeSpan.FromMinutes(-5); // 5 minutes before morning time

    // Act
    reminderData.TimeSinceMorning = negativeTimeSpan;

    // Assert
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(negativeTimeSpan));
}

[Test]
public void CallCount_Should_HandleNegativeCallCount_WhenAssigned()
{
    // Arrange
    var reminderData = new MorningReminderData();

    // Act
    reminderData.CallCount = -1;

    // Assert
    Assert.That(reminderData.CallCount, Is.EqualTo(-1));
}
```

#### Real-world Usage Simulation Tests
```csharp
[Test]
public void Usage_Should_SimulateRealWorldReminderScenario()
{
    // Arrange - Simulate typical morning reminder scenario
    var morningScheduledTime = new DateTime(2024, 10, 17, 9, 0, 0);
    var currentTime = new DateTime(2024, 10, 17, 9, 7, 30);
    var timeSinceScheduled = currentTime - morningScheduledTime;
    var reminderCallCount = 4;

    // Act - Create reminder data as MorningReminderCommand would
    var reminderData = new MorningReminderData
    {
        MorningTime = morningScheduledTime,
        TimeSinceMorning = timeSinceScheduled,
        CallCount = reminderCallCount
    };

    // Assert - Verify data integrity
    Assert.That(reminderData.MorningTime, Is.EqualTo(morningScheduledTime));
    Assert.That(reminderData.TimeSinceMorning, Is.EqualTo(TimeSpan.FromMinutes(7.5)));
    Assert.That(reminderData.CallCount, Is.EqualTo(4));
    
    // Verify timing calculation makes sense
    Assert.That(reminderData.TimeSinceMorning.TotalMinutes, Is.GreaterThan(0));
    Assert.That(reminderData.TimeSinceMorning.TotalMinutes, Is.LessThan(10)); // Within 10-minute window
}
```

### Enhanced Test Considerations

#### If the class needs equality comparison:
```csharp
[Test]
public void Equality_Should_BeEqual_WhenAllPropertiesMatch()
{
    // Arrange
    var morningTime = new DateTime(2024, 10, 17, 9, 0, 0);
    var timeSince = TimeSpan.FromMinutes(5);
    var callCount = 2;

    var reminderData1 = new MorningReminderData
    {
        MorningTime = morningTime,
        TimeSinceMorning = timeSince,
        CallCount = callCount
    };

    var reminderData2 = new MorningReminderData
    {
        MorningTime = morningTime,
        TimeSinceMorning = timeSince,
        CallCount = callCount
    };

    // Act & Assert
    Assert.That(reminderData1.Equals(reminderData2), Is.True);
    Assert.That(reminderData1.GetHashCode(), Is.EqualTo(reminderData2.GetHashCode()));
}
```

#### If the class needs JSON serialization:
```csharp
[Test]
public void Serialization_Should_HandleRoundTripSerialization_WithoutDataLoss()
{
    // Arrange
    var original = new MorningReminderData
    {
        MorningTime = new DateTime(2024, 10, 17, 9, 15, 30),
        TimeSinceMorning = TimeSpan.FromMinutes(8.25),
        CallCount = 6
    };

    // Act
    var json = JsonSerializer.Serialize(original);
    var deserialized = JsonSerializer.Deserialize<MorningReminderData>(json);

    // Assert
    Assert.That(deserialized, Is.Not.Null);
    Assert.That(deserialized.MorningTime, Is.EqualTo(original.MorningTime));
    Assert.That(deserialized.TimeSinceMorning, Is.EqualTo(original.TimeSinceMorning));
    Assert.That(deserialized.CallCount, Is.EqualTo(original.CallCount));
}
```

## Success Criteria
- [ ] **100% line coverage** (should be trivial for a 6-line class)
- [ ] **100% branch coverage** (no branching logic in properties)
- [ ] **Property validation** - All properties can be set and retrieved correctly
- [ ] **Edge case handling** - DateTime.Min/Max, negative values, zero values tested
- [ ] **Usage pattern validation** - Object initializer syntax works correctly
- [ ] **Type safety** - All property types behave as expected

## Implementation Priority
**LOW PRIORITY** - Simple data class with excellent testability. Can be implemented quickly as part of comprehensive test suite coverage goals.

## Dependencies for Testing
- **None** - Pure unit testing with no external dependencies
- Standard NUnit testing framework sufficient

## Implementation Estimate
**Effort: Very Low (30 minutes - 1 hour)**
- Create test class with comprehensive property coverage
- Add edge case tests for DateTime and TimeSpan boundaries
- Verify object initializer patterns work correctly
- Optional: Add equality and serialization tests if needed

This is an example of a perfectly testable data class that requires minimal effort but contributes to overall coverage metrics and serves as a foundation for testing more complex classes that depend on it.