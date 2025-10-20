# ScheduleOverride Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Object Analysis

**File**: `MauiApp/Models/ScheduleOverride.cs`  
**Type**: Value Domain Model  
**Primary Purpose**: Represent date-specific schedule time overrides  
**Key Functionality**: Date-specific time overrides, application logic, validation

### Purpose & Responsibilities

The `ScheduleOverride` class serves as a value object representing date-specific schedule overrides. It provides:

- Date-specific morning and evening time overrides (nullable)
- Override validation (at least one time must be specified)
- Date matching logic for override application
- JSON serialization support for persistence
- Convenience methods for date checking

### Architecture Role

- **Layer**: Domain Model Layer
- **Pattern**: Value Object with Business Logic
- **MVVM Role**: Data model used by ScheduleConfig and ViewModels
- **Clean Architecture**: Domain value object with validation rules

### Dependencies Analysis

#### Direct Dependencies

**DateTime Static Dependencies**:
- `DateTime.Today` in `AppliesToToday()` method

#### Platform Dependencies

- **System.Text.Json**: JSON serialization attributes
- **.NET DateTime**: Standard library date/time functionality

### Public Interface Documentation

#### Properties

**`DateOnly Date { get; set; }`**

- **Purpose**: The specific date this override applies to
- **Type**: `DateOnly` - Date without time component
- **Serialization**: JSON property name "date"

**`TimeSpan? MorningTime { get; set; }`**

- **Purpose**: Override morning time (nullable - no override if null)
- **Type**: `TimeSpan?` - Optional time override
- **Business Rule**: At least one of MorningTime or EveningTime must be set for valid override
- **Serialization**: JSON property name "morningTime"

**`TimeSpan? EveningTime { get; set; }`**

- **Purpose**: Override evening time (nullable - no override if null)
- **Type**: `TimeSpan?` - Optional time override
- **Business Rule**: At least one of MorningTime or EveningTime must be set for valid override
- **Serialization**: JSON property name "eveningTime"

**`bool HasOverride { get; }` (Computed)**

- **Purpose**: Indicates if this override has at least one time specified
- **Type**: `bool` - Computed property
- **Logic**: Returns `MorningTime.HasValue || EveningTime.HasValue`
- **Use Case**: Validation to ensure override has meaning

#### Constructors

**`ScheduleOverride()`**

- **Purpose**: Creates empty override (for serialization)
- **Behavior**: All properties uninitialized

**`ScheduleOverride(DateOnly date, TimeSpan? morningTime = null, TimeSpan? eveningTime = null)`**

- **Purpose**: Creates override for specific date with optional times
- **Parameters**: `date` - Target date, `morningTime`/`eveningTime` - Optional override times
- **Behavior**: Sets all properties based on parameters

#### Methods

**`bool AppliesToDate(DateOnly date)`**

- **Purpose**: Determines if this override applies to the specified date
- **Parameters**: `date` - Date to check
- **Return**: `true` if override date matches parameter date
- **Logic**: Simple equality comparison (`Date == date`)

**`bool AppliesToToday()`**

- **Purpose**: Determines if this override applies to today's date
- **Static Dependency**: Uses `DateTime.Today`
- **Return**: `true` if override applies to current date
- **Convenience**: Wrapper around `AppliesToDate(DateOnly.FromDateTime(DateTime.Today))`

## Testability Assessment

**Overall Testability Score: 9/10**

### Strengths

- ✅ **Pure Value Object**: Simple data structure with clear validation logic
- ✅ **Deterministic Behavior**: All methods are pure or predictable
- ✅ **Clear Business Rules**: Well-defined override validation and date matching
- ✅ **Minimal Dependencies**: Only one static DateTime dependency
- ✅ **Comprehensive API**: Complete set of operations for override management
- ✅ **Immutable Design**: Properties can be set but object behavior is pure

### Challenges

- ⚠️ **DateTime.Today Dependency**: `AppliesToToday()` uses current date static dependency

### Required Refactoring

**None Required** - Excellent testability as designed

**Assessment**: The single DateTime.Today dependency is minimal and easily tested with date-window approaches. The value object design is ideal for comprehensive testing.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class ScheduleOverrideTests
{
    private readonly DateOnly _testDate = new(2025, 6, 15);
    private readonly TimeSpan _testMorning = new(9, 0, 0);
    private readonly TimeSpan _testEvening = new(18, 0, 0);
    
    private static ScheduleOverride CreateTestOverride(DateOnly? date = null, TimeSpan? morning = null, TimeSpan? evening = null)
    {
        return new ScheduleOverride(date ?? new DateOnly(2025, 1, 1), morning, evening);
    }
}
```

### Mock Strategy

**No mocking required** - Testing pure value object with direct verification.

### Test Categories

1. **Constructor Tests**: Default and parameterized initialization
2. **Property Tests**: Basic assignment and computed properties
3. **Validation Tests**: HasOverride business logic
4. **Date Matching Tests**: AppliesToDate and AppliesToToday methods
5. **Serialization Tests**: JSON property mapping
6. **Edge Cases**: Null values, boundary conditions

## Test Implementation Plan

### Phase 1: Core Value Object
- Constructor tests (default and parameterized)
- Property assignment and retrieval
- HasOverride computed property logic

### Phase 2: Business Logic
- AppliesToDate method (matching and non-matching dates)
- HasOverride validation (various time combinations)
- Edge cases with null time values

### Phase 3: Convenience Methods
- AppliesToToday method (current date testing)
- Date boundary testing
- Integration scenarios

### Phase 4: Serialization and Edge Cases
- JSON serialization/deserialization
- Boundary conditions and null inputs
- Value object equality scenarios

## Detailed Test Cases

### Constructor Tests

**Test**: `DefaultConstructor_ShouldCreateEmptyOverride`

- Create default override
- Verify all properties are default/uninitialized

**Test**: `ParameterizedConstructor_ShouldSetProperties`

- Create override with all parameters
- Verify properties set correctly

**Test**: `ParameterizedConstructor_WithPartialTimes_ShouldSetSpecifiedTimes`

- Create override with only morning or only evening time
- Verify specified time set, other time remains null

### HasOverride Property Tests

**Test**: `HasOverride_WithBothTimes_ShouldReturnTrue`

- Create override with both morning and evening times
- Verify HasOverride returns true

**Test**: `HasOverride_WithOnlyMorningTime_ShouldReturnTrue`

- Create override with only morning time
- Verify HasOverride returns true

**Test**: `HasOverride_WithOnlyEveningTime_ShouldReturnTrue`

- Create override with only evening time
- Verify HasOverride returns true

**Test**: `HasOverride_WithNoTimes_ShouldReturnFalse`

- Create override with null morning and evening times
- Verify HasOverride returns false

### AppliesToDate Tests

**Test**: `AppliesToDate_WithMatchingDate_ShouldReturnTrue`

- Create override for specific date
- Call AppliesToDate with same date
- Verify returns true

**Test**: `AppliesToDate_WithDifferentDate_ShouldReturnFalse`

- Create override for specific date
- Call AppliesToDate with different date
- Verify returns false

**Test**: `AppliesToDate_WithSameDateDifferentTimes_ShouldReturnTrue`

- Override date is only factor, not times
- Verify date matching works regardless of time values

### AppliesToToday Tests

**Test**: `AppliesToToday_WithTodayDate_ShouldReturnTrue`

- Create override for today's date
- Call AppliesToToday
- Verify returns true (within time window for test reliability)

**Test**: `AppliesToToday_WithDifferentDate_ShouldReturnFalse`

- Create override for date that is not today
- Call AppliesToToday
- Verify returns false

### Property Assignment Tests

**Test**: `Properties_ShouldAllowGetSet`

- Create override and set all properties
- Verify all properties can be retrieved correctly

**Test**: `DateProperty_ShouldStoreCorrectly`

- Set various DateOnly values
- Verify property stores and retrieves correctly

**Test**: `TimeProperties_ShouldHandleNullValues`

- Set time properties to null
- Verify null values handled correctly

### Serialization Tests

**Test**: `JsonSerialization_ShouldIncludeAllProperties`

- Serialize ScheduleOverride to JSON
- Verify JSON contains "date", "morningTime", "eveningTime"

**Test**: `JsonDeserialization_ShouldRestoreProperties`

- Serialize and deserialize ScheduleOverride
- Verify all properties restored correctly

**Test**: `JsonSerialization_WithNullTimes_ShouldHandleGracefully`

- Serialize override with null time values
- Verify null values serialized and deserialized correctly

### Edge Case Tests

**Test**: `Constructor_WithNullTimes_ShouldCreateValidObject`

- Create override with null morning and evening times
- Verify object created successfully, HasOverride returns false

**Test**: `AppliesToDate_WithMinMaxDates_ShouldWorkCorrectly`

- Test with DateOnly.MinValue and DateOnly.MaxValue
- Verify date matching works at boundaries

**Test**: `TimeProperties_WithMinMaxTimeSpan_ShouldWorkCorrectly`

- Set times to TimeSpan.Zero and TimeSpan.MaxValue
- Verify properties handle extreme values

### Business Logic Integration Tests

**Test**: `HasOverride_ReflectsPropertyChanges`

- Create override, modify time properties
- Verify HasOverride updates accordingly

**Test**: `OverrideWithSingleTime_ShouldBeValid`

- Test business scenario where only one time needs override
- Verify HasOverride returns true with partial override

## Coverage Goals

- **Method Coverage**: 100% - All public methods and properties
- **Line Coverage**: 100% - All logic paths in simple value object
- **Branch Coverage**: 100% - All conditional logic in HasOverride and date matching
- **Business Logic Coverage**: 100% - All validation and date matching scenarios

## Implementation Checklist

- [ ] **Constructor Tests**: Default and parameterized initialization
- [ ] **Property Tests**: Basic assignment and computed property logic
- [ ] **HasOverride Tests**: Business validation with various time combinations
- [ ] **Date Matching Tests**: AppliesToDate and AppliesToToday methods
- [ ] **Serialization Tests**: JSON property mapping verification
- [ ] **Edge Case Tests**: Null values, boundary conditions, extreme values
- [ ] **Integration Tests**: Business logic scenarios with partial overrides

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for ScheduleOverride value object`
- `^f - add validation and date matching tests for ScheduleOverride business logic`
- `^f - add serialization and edge case tests for ScheduleOverride model`

## Risk Assessment

- **Very Low Risk**: Simple value object with clear business rules
- **Very Low Risk**: Minimal static dependencies, easily testable
- **Very Low Risk**: Pure methods with deterministic behavior
- **Very Low Risk**: Well-defined validation logic with clear scenarios

## Refactoring Recommendations

**Current Design Assessment**: The `ScheduleOverride` represents exemplary design for a domain value object - simple, focused, and highly testable.

**Recommendation**: No refactoring needed. Current design provides excellent testability with minimal dependencies and clear business logic. Focus on comprehensive test coverage of all validation scenarios and edge cases. The single DateTime.Today dependency is negligible and easily handled with date-focused testing approaches. 🤖