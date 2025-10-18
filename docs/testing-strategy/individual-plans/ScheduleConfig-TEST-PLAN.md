# ScheduleConfig Test Plan

## Object Analysis

**File**: `MauiApp/Models/ScheduleConfig.cs`  
**Type**: Configuration Domain Model  
**Primary Purpose**: Manage work schedule times with date-specific override support  
**Key Functionality**: Default time configuration, override management, effective time calculation

### Purpose & Responsibilities

The `ScheduleConfig` class serves as a configuration model for work schedule management. It provides:

- Default morning and evening work times (8:20 AM, 5:20 PM)
- Date-specific schedule overrides through `ScheduleOverride` collection
- Effective time calculation considering overrides
- Override management (add, remove, cleanup)
- JSON serialization support for persistence

### Architecture Role

- **Layer**: Domain Model Layer
- **Pattern**: Configuration Model with Override Logic
- **MVVM Role**: Data model used by ScheduleConfigService and ViewModels
- **Clean Architecture**: Domain configuration entity with business rules

### Dependencies Analysis

#### Direct Dependencies

**ScheduleOverride Class**:
- Collection property `List<ScheduleOverride> Overrides`
- Calls `override_.AppliesToDate(date)` method
- Creates new `ScheduleOverride` instances in `SetOverride()`

**DateTime Static Dependencies**:
- `DateTime.Today` in `GetEffectiveMorningTimeToday()` and `GetEffectiveEveningTimeToday()`
- `DateTime.Today.AddDays(-30)` in `CleanupOldOverrides()`

#### Platform Dependencies

- **System.Text.Json**: JSON serialization attributes
- **.NET Collections**: `List<T>` for overrides collection
- **LINQ**: `FirstOrDefault`, `RemoveAll` for collection operations

### Public Interface Documentation

#### Properties

**`TimeSpan MorningTime { get; set; }`**

- **Purpose**: Default morning work time
- **Type**: `TimeSpan` - Time of day for morning schedule
- **Default**: 08:20:00 (8:20 AM)
- **Serialization**: JSON property name "morningTime"

**`TimeSpan EveningTime { get; set; }`**

- **Purpose**: Default evening work time  
- **Type**: `TimeSpan` - Time of day for evening schedule
- **Default**: 17:20:00 (5:20 PM)
- **Serialization**: JSON property name "eveningTime"

**`List<ScheduleOverride> Overrides { get; set; }`**

- **Purpose**: Collection of date-specific schedule overrides
- **Type**: `List<ScheduleOverride>` - Mutable collection
- **Default**: Empty list
- **Serialization**: JSON property name "overrides"

#### Constructors

**`ScheduleConfig()`**

- **Purpose**: Creates configuration with default schedule times
- **Behavior**: Sets default times (8:20 AM, 5:20 PM), empty overrides

**`ScheduleConfig(TimeSpan morningTime, TimeSpan eveningTime)`**

- **Purpose**: Creates configuration with custom schedule times
- **Parameters**: `morningTime`, `eveningTime` - Custom schedule times
- **Behavior**: Sets specified times, empty overrides

#### Methods

**`TimeSpan GetEffectiveMorningTime(DateOnly date)`**

- **Purpose**: Gets effective morning time for specific date considering overrides
- **Parameters**: `date` - Date to check for overrides
- **Return**: Override morning time if exists, otherwise default morning time
- **Logic**: Searches overrides for matching date, falls back to default

**`TimeSpan GetEffectiveEveningTime(DateOnly date)`**

- **Purpose**: Gets effective evening time for specific date considering overrides
- **Parameters**: `date` - Date to check for overrides  
- **Return**: Override evening time if exists, otherwise default evening time
- **Logic**: Searches overrides for matching date, falls back to default

**`TimeSpan GetEffectiveMorningTimeToday()`**

- **Purpose**: Gets effective morning time for today
- **Static Dependency**: Uses `DateTime.Today`
- **Return**: Effective morning time for current date
- **Convenience**: Wrapper around `GetEffectiveMorningTime(today)`

**`TimeSpan GetEffectiveEveningTimeToday()`**

- **Purpose**: Gets effective evening time for today
- **Static Dependency**: Uses `DateTime.Today`
- **Return**: Effective evening time for current date
- **Convenience**: Wrapper around `GetEffectiveEveningTime(today)`

**`void SetOverride(DateOnly date, TimeSpan? morningTime = null, TimeSpan? eveningTime = null)`**

- **Purpose**: Adds or updates schedule override for specific date
- **Parameters**: `date` - Target date, `morningTime`/`eveningTime` - Optional override times
- **Business Logic**: Removes existing override for date, adds new if at least one time specified
- **Side Effects**: Modifies Overrides collection

**`void RemoveOverride(DateOnly date)`**

- **Purpose**: Removes any override for specific date
- **Parameters**: `date` - Date to remove override for
- **Side Effects**: Modifies Overrides collection
- **Logic**: Removes all overrides matching the date

**`ScheduleOverride? GetOverride(DateOnly date)`**

- **Purpose**: Retrieves override for specific date if exists
- **Parameters**: `date` - Date to search for
- **Return**: Override if found, null otherwise
- **Logic**: Uses `AppliesToDate()` method for matching

**`void CleanupOldOverrides()`**

- **Purpose**: Removes overrides older than 30 days
- **Business Rule**: Automatic cleanup of stale overrides
- **Static Dependency**: Uses `DateTime.Today.AddDays(-30)` for cutoff
- **Side Effects**: Modifies Overrides collection

## Testability Assessment

**Overall Testability Score: 7/10**

### Strengths

- ✅ **Clear Business Logic**: Well-defined override precedence and time calculation
- ✅ **Comprehensive API**: Complete set of configuration operations
- ✅ **Deterministic Behavior**: Most methods are pure or predictable
- ✅ **Good Separation**: Configuration logic separate from persistence
- ✅ **Nullable Support**: Proper handling of optional override times

### Challenges

- ⚠️ **DateTime.Today Dependencies**: Two methods use current date static dependency
- ⚠️ **ScheduleOverride Dependencies**: Relies on ScheduleOverride business logic
- ⚠️ **Collection Mutations**: Several methods modify internal state

### Required Refactoring

**Low Priority** - Current design is reasonably testable

**Potential Enhancement**: Abstract DateTime dependencies for more predictable testing, but current design allows for effective testing with date-focused test scenarios.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class ScheduleConfigTests
{
    private ScheduleConfig _config;
    private readonly TimeSpan _defaultMorning = new(8, 20, 0);
    private readonly TimeSpan _defaultEvening = new(17, 20, 0);
    private readonly DateOnly _testDate = new(2025, 6, 15);
    
    [SetUp]
    public void Setup()
    {
        _config = new ScheduleConfig();
    }
    
    private static ScheduleOverride CreateTestOverride(DateOnly date, TimeSpan? morning = null, TimeSpan? evening = null)
    {
        return new ScheduleOverride(date, morning, evening);
    }
}
```

### Mock Strategy

**No mocking required** - Testing domain configuration model with direct verification.

### Test Categories

1. **Constructor Tests**: Default and parameterized initialization
2. **Effective Time Tests**: Time calculation with and without overrides
3. **Override Management**: Add, update, remove, retrieve operations
4. **Business Logic Tests**: Override precedence, cleanup logic
5. **Serialization Tests**: JSON property mapping
6. **Edge Cases**: Null overrides, empty collections, boundary conditions

## Test Implementation Plan

### Phase 1: Core Configuration
- Constructor tests (default and custom times)
- Basic property access and assignment
- Default time verification

### Phase 2: Override Logic
- SetOverride method (add, update, remove scenarios)
- GetOverride method (found, not found)
- RemoveOverride method (existing, non-existing)

### Phase 3: Effective Time Calculation
- GetEffectiveMorningTime/EveningTime (with/without overrides)
- Today methods (wrapper functionality)
- Override precedence verification

### Phase 4: Collection Management
- CleanupOldOverrides (date-based removal)
- Multiple overrides handling
- Edge cases and boundary conditions

## Detailed Test Cases

### Constructor Tests

**Test**: `DefaultConstructor_ShouldSetDefaultTimes`

- Verify MorningTime = 08:20:00, EveningTime = 17:20:00
- Verify Overrides collection is empty

**Test**: `ParameterizedConstructor_ShouldSetCustomTimes`

- Create with custom times, verify properties set correctly
- Verify Overrides collection is empty

### Override Management Tests

**Test**: `SetOverride_WithBothTimes_ShouldAddOverride`

- Add override with both morning and evening times
- Verify override exists in collection

**Test**: `SetOverride_WithPartialTimes_ShouldAddOverride`

- Add override with only morning or only evening time
- Verify override created with specified time only

**Test**: `SetOverride_WithNoTimes_ShouldNotAddOverride`

- Call SetOverride with null morning and evening times
- Verify no override added to collection

**Test**: `SetOverride_WithExistingDate_ShouldReplaceOverride`

- Add override for date, then set different override for same date
- Verify only one override exists for that date

**Test**: `RemoveOverride_WithExistingDate_ShouldRemoveOverride`

- Add override, then remove it
- Verify override no longer exists

### Effective Time Tests

**Test**: `GetEffectiveMorningTime_WithoutOverride_ShouldReturnDefault`

- Get effective time for date without override
- Verify returns default morning time

**Test**: `GetEffectiveMorningTime_WithOverride_ShouldReturnOverrideTime`

- Add override with morning time, get effective time
- Verify returns override time instead of default

**Test**: `GetEffectiveMorningTime_WithPartialOverride_ShouldReturnDefault`

- Add override with only evening time, get effective morning time
- Verify returns default morning time (no morning override)

**Test**: `GetEffectiveEveningTime_WithOverride_ShouldReturnOverrideTime`

- Add override with evening time, get effective time
- Verify returns override time

### Today Methods Tests

**Test**: `GetEffectiveMorningTimeToday_ShouldUseCurrentDate`

- Add override for today's date, call GetEffectiveMorningTimeToday
- Verify returns today's override time

**Test**: `GetEffectiveEveningTimeToday_WithoutTodayOverride_ShouldReturnDefault`

- Call GetEffectiveEveningTimeToday without today override
- Verify returns default evening time

### Cleanup Tests

**Test**: `CleanupOldOverrides_ShouldRemoveOldOverrides`

- Add overrides from 40 days ago and 20 days ago
- Call CleanupOldOverrides
- Verify old override removed, recent override remains

**Test**: `CleanupOldOverrides_WithRecentOverrides_ShouldKeepAll`

- Add overrides from within last 30 days
- Call CleanupOldOverrides
- Verify all overrides remain

### Serialization Tests

**Test**: `JsonSerialization_ShouldIncludeAllProperties`

- Serialize ScheduleConfig to JSON
- Verify JSON contains "morningTime", "eveningTime", "overrides"

**Test**: `JsonDeserialization_ShouldRestoreProperties`

- Serialize and deserialize ScheduleConfig
- Verify all properties restored correctly

### Edge Case Tests

**Test**: `GetOverride_WithNonExistentDate_ShouldReturnNull`

- Get override for date that doesn't exist
- Verify returns null

**Test**: `EmptyOverrides_ShouldHandleGracefully`

- Test all methods with empty overrides collection
- Verify proper fallback to defaults

## Coverage Goals

- **Method Coverage**: 100% - All public methods and properties
- **Line Coverage**: 95% - All business logic and configuration operations
- **Branch Coverage**: 100% - All conditional logic paths
- **Business Logic Coverage**: 100% - All override precedence and time calculation scenarios

## Implementation Checklist

- [ ] **Constructor Tests**: Default and parameterized initialization
- [ ] **Property Tests**: Basic get/set functionality for all properties
- [ ] **Override Management**: SetOverride, RemoveOverride, GetOverride methods
- [ ] **Effective Time Tests**: Time calculation with override precedence
- [ ] **Today Methods**: Current date wrapper functionality
- [ ] **Cleanup Tests**: CleanupOldOverrides date-based removal
- [ ] **Serialization Tests**: JSON property mapping verification
- [ ] **Edge Case Tests**: Null inputs, empty collections, boundary conditions

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for ScheduleConfig configuration model`
- `^f - add override management tests for ScheduleConfig date-specific scheduling`
- `^f - add effective time calculation tests for ScheduleConfig business logic`
- `^f - add cleanup and serialization tests for ScheduleConfig maintenance operations`

## Risk Assessment

- **Low Risk**: Well-defined configuration logic with clear business rules
- **Medium Risk**: DateTime dependencies require careful date-based testing
- **Low Risk**: Pure calculation methods are deterministic and easily testable
- **Low Risk**: Configuration model with minimal external dependencies

## Refactoring Recommendations

**Current Design Assessment**: The `ScheduleConfig` demonstrates good design for a configuration model with clear override precedence logic and comprehensive time management capabilities.

**Recommendation**: Current design provides good testability for a configuration model. Focus on comprehensive test coverage of override logic, effective time calculations, and collection management. DateTime dependencies are manageable with date-focused testing scenarios. 🤖