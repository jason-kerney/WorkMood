# MoodEntry Test Plan

## Object Analysis

**File**: `MauiApp/Models/MoodEntry.cs`  
**Type**: Domain Model Class  
**Primary Purpose**: Represent daily mood entry with start/end work moods, validation, and business logic  
**Key Functionality**: Stores mood ratings (1-10), calculates mood changes, validates data, and provides business rule logic

### Purpose & Responsibilities

The `MoodEntry` class serves as a core domain model representing a single day's mood tracking data. It stores start-of-work and end-of-work mood ratings on a 1-10 scale, provides computed properties for mood change calculations, implements validation logic for mood value ranges, and enforces business rules for data persistence. The class includes JSON serialization support and automatic timestamp management.

### Architecture Role

- **Layer**: Domain Model Layer
- **Pattern**: Domain Model with Business Logic
- **MVVM Role**: Data model used by ViewModels and Services
- **Clean Architecture**: Domain entity with validation and business rules

### Dependencies Analysis

#### Constructor Dependencies

**MoodEntry()** (Default Constructor):
- **DateTime.Today**: Static dependency for current date
- **DateTime.Now**: Static dependency for timestamps (CreatedAt, LastModified)

**MoodEntry(DateOnly date)** (Parameterized Constructor):
- **DateTime.Now**: Static dependency for timestamps (CreatedAt, LastModified)

#### Method Dependencies

**PrepareForSave**:
- **DateTime.Now**: Static dependency for LastModified timestamp
- **ShouldSave()**: Internal method dependency
- **IsValid()**: Internal validation method dependency

**Value Property (Computed)**:
- **No external dependencies**: Pure computation based on properties

**ToString()**:
- **String interpolation**: Standard .NET string formatting

#### Static Dependencies

- **DateTime.Today**: Used in default constructor
- **DateTime.Now**: Used in constructors and PrepareForSave method

#### Platform Dependencies

- **.NET DateTime**: Standard library date/time functionality
- **System.Text.Json**: JSON serialization attributes

### Public Interface Documentation

#### Properties

**`DateOnly Date { get; set; }`**

- **Purpose**: The date of the mood entry (date only, no time component)
- **Type**: `DateOnly` - Date value without time
- **Behavior**: Simple get/set property for date tracking
- **Validation**: None (accepts any valid DateOnly value)

**`int? StartOfWork { get; set; }`**

- **Purpose**: Start of work mood rating (1-10 scale, nullable)
- **Type**: `int?` - Nullable integer for optional mood rating
- **Business Rule**: Must be between 1-10 if set, null indicates not recorded
- **Validation**: Validated by IsValid() method

**`int? EndOfWork { get; set; }`**

- **Purpose**: End of work mood rating (1-10 scale, nullable)
- **Type**: `int?` - Nullable integer for optional mood rating
- **Business Rule**: Must be between 1-10 if set, null indicates not recorded
- **Validation**: Validated by IsValid() method

**`DateTime CreatedAt { get; set; }`**

- **Purpose**: Timestamp when the entry was created
- **Type**: `DateTime` - Full date and time
- **Behavior**: Set automatically in constructors, can be modified
- **Usage**: Audit trail for entry creation

**`DateTime LastModified { get; set; }`**

- **Purpose**: Timestamp when the entry was last modified
- **Type**: `DateTime` - Full date and time
- **Behavior**: Set automatically in constructors and PrepareForSave
- **Usage**: Audit trail for entry updates

**`double? Value { get; }` [JsonIgnore]**

- **Purpose**: Computed mood change value (read-only computed property)
- **Formula**: `(EndOfWork ?? StartOfWork) - StartOfWork`
- **Return**: Positive = improved mood, Zero = no change, Negative = declined mood
- **Behavior**: Returns null if StartOfWork is not set
- **Serialization**: Excluded from JSON serialization via JsonIgnore attribute

#### Constructors

**`MoodEntry()`**

- **Purpose**: Creates new mood entry for today's date
- **Behavior**: Sets Date to today, timestamps to now
- **Static Dependencies**: DateTime.Today, DateTime.Now

**`MoodEntry(DateOnly date)`**

- **Purpose**: Creates new mood entry for specific date
- **Parameters**: `date` - The date for the mood entry
- **Behavior**: Sets Date to provided value, timestamps to now
- **Static Dependencies**: DateTime.Now

#### Methods

**`bool IsValid()`**

- **Purpose**: Validates that mood values are within valid range (1-10)
- **Return Type**: `bool` - True if all set mood values are valid
- **Validation Logic**:
  - StartOfWork must be 1-10 if set
  - EndOfWork must be 1-10 if set
  - Returns true if both conditions met
- **Side Effects**: None (pure validation function)

**`bool ShouldSave()`**

- **Purpose**: Determines if entry should be saved based on business rules
- **Business Rules**: Must have StartOfWork mood and be valid
- **Return Type**: `bool` - True if entry meets save criteria
- **Dependencies**: Calls IsValid() internally
- **Side Effects**: None (pure business logic function)

**`void PrepareForSave(bool useAutoSaveDefaults = false)`**

- **Purpose**: Prepares entry for saving by applying business rules
- **Parameters**: `useAutoSaveDefaults` - Whether to apply auto-save defaults
- **Business Logic**:
  - Only operates if ShouldSave() returns true
  - If useAutoSaveDefaults=true and EndOfWork is null, sets EndOfWork = StartOfWork
  - Updates LastModified timestamp to DateTime.Now
- **Side Effects**: Modifies EndOfWork and LastModified properties
- **Static Dependencies**: DateTime.Now

**`double? GetAverageMood()`**

- **Purpose**: Gets average mood for the day if both moods are set
- **Return Type**: `double?` - Average of StartOfWork and EndOfWork, or null
- **Logic**: Returns (StartOfWork + EndOfWork) / 2.0 if both values present
- **Side Effects**: None (pure calculation function)

**`double? GetAdjustedAverageMood()`**

- **Purpose**: Gets adjusted average mood for graphing purposes
- **Logic**: 
  - Uses EndOfWork if available, otherwise StartOfWork for both values
  - Calculates average and subtracts 5.0 to shift range from 1-10 to -4 to +5
- **Return Type**: `double?` - Adjusted average or null if no StartOfWork
- **Use Case**: Graph visualization with centered zero point
- **Side Effects**: None (pure calculation function)

**`string ToString()` (Override)**

- **Purpose**: Gets display string representation of mood entry
- **Format**: "{Date:yyyy-MM-dd}: Start of Work {start}, End of Work {end}"
- **Null Handling**: Shows "Not recorded" for null mood values
- **Return Type**: `string` - Formatted display string
- **Side Effects**: None (pure formatting function)

#### Commands

- **None**: Not applicable for domain model

#### Events

- **None**: No events raised

## Testability Assessment

**Overall Testability Score: 7/10**

### Strengths

- ✅ **Clear Business Logic**: Well-defined validation and business rules
- ✅ **Pure Functions**: Most methods are pure with no side effects (except PrepareForSave)
- ✅ **Simple Dependencies**: Only DateTime static dependencies
- ✅ **Comprehensive API**: All scenarios covered by public methods
- ✅ **Nullable Design**: Proper handling of optional mood values
- ✅ **Computed Properties**: Value property provides clean derived data
- ✅ **Validation Logic**: Built-in range validation for mood values

### Challenges

- ⚠️ **DateTime.Now Dependencies**: Static time dependencies in constructors and PrepareForSave
- ⚠️ **DateTime.Today Dependency**: Static date dependency in default constructor
- ⚠️ **Side Effects**: PrepareForSave modifies object state

### Current Testability Score Justification

Score: **7/10** - Good testability with minor static dependencies

**Deductions**:
- **-2 points**: DateTime.Now and DateTime.Today static dependencies make time-sensitive testing challenging
- **-1 point**: PrepareForSave method has side effects and timestamp dependency

### Hard Dependencies Identified

1. **DateTime.Now**: Used in constructors and PrepareForSave method
2. **DateTime.Today**: Used in default constructor
3. **System.Text.Json**: JSON serialization attributes (not problematic for testing)

### Required Refactoring

**Low Priority Refactoring Recommended**

While the current MoodEntry design is reasonably testable, the static DateTime dependencies could be abstracted for more predictable testing:

**Option 1: Constructor Injection with DateTime Provider**
```csharp
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
}

public class MoodEntry
{
    public MoodEntry(IDateTimeProvider dateTimeProvider = null)
    {
        var provider = dateTimeProvider ?? new SystemDateTimeProvider();
        Date = DateOnly.FromDateTime(provider.Today);
        CreatedAt = provider.Now;
        LastModified = provider.Now;
    }
    
    public void PrepareForSave(bool useAutoSaveDefaults = false, IDateTimeProvider dateTimeProvider = null)
    {
        if (ShouldSave())
        {
            // Business logic...
            LastModified = (dateTimeProvider ?? new SystemDateTimeProvider()).Now;
        }
    }
}
```

**Option 2: Static Factory Methods**
```csharp
public static class MoodEntryFactory
{
    public static MoodEntry CreateForToday() => new MoodEntry(DateOnly.FromDateTime(DateTime.Today));
    public static MoodEntry CreateForDate(DateOnly date) => new MoodEntry(date);
}
```

**Benefits of Refactoring**:
- **Predictable Testing**: Control timestamp values in tests
- **Time Travel Testing**: Test different creation dates/times
- **Better Test Isolation**: No dependency on system clock

**Current Testing Strategy**: Given the low priority of this refactoring, tests can work around DateTime dependencies using techniques like:
- Testing within time windows (e.g., timestamps within last few seconds)
- Focusing on business logic rather than exact timestamp values
- Using relative time assertions rather than absolute values

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class MoodEntryTests
{
    // Test methods - no complex setup required for simple domain model
    
    private static MoodEntry CreateTestEntry(DateOnly? date = null, int? startOfWork = null, int? endOfWork = null)
    {
        var entry = date.HasValue ? new MoodEntry(date.Value) : new MoodEntry();
        entry.StartOfWork = startOfWork;
        entry.EndOfWork = endOfWork;
        return entry;
    }
}
```

### Mock Strategy

**No mocking required** - Testing pure domain model with direct verification of properties and method results.

### Test Categories

1. **Constructor Tests**: Object initialization and timestamp setting
2. **Property Tests**: Get/set behavior for all properties
3. **Validation Tests**: IsValid method with various mood value combinations
4. **Business Logic Tests**: ShouldSave and PrepareForSave business rules
5. **Calculation Tests**: Value, GetAverageMood, GetAdjustedAverageMood computations
6. **String Representation Tests**: ToString method formatting
7. **Edge Case Tests**: Null values, boundary conditions, invalid ranges
8. **Serialization Tests**: JSON serialization behavior

## Detailed Test Cases

### Constructors: MoodEntry

#### Purpose

Initialize MoodEntry instances with appropriate default values and timestamps.

#### Test Cases

##### Default Constructor Tests

**Test**: `DefaultConstructor_ShouldSetDateToToday`

```csharp
[Test]
public void DefaultConstructor_ShouldSetDateToToday()
{
    // Arrange & Act
    var entry = new MoodEntry();
    var today = DateOnly.FromDateTime(DateTime.Today);
    
    // Assert
    Assert.That(entry.Date, Is.EqualTo(today));
}
```

**Test**: `DefaultConstructor_ShouldSetTimestamps`

```csharp
[Test]
public void DefaultConstructor_ShouldSetTimestamps()
{
    // Arrange
    var beforeCreation = DateTime.Now;
    
    // Act
    var entry = new MoodEntry();
    var afterCreation = DateTime.Now;
    
    // Assert
    Assert.That(entry.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation).And.LessThanOrEqualTo(afterCreation));
    Assert.That(entry.LastModified, Is.GreaterThanOrEqualTo(beforeCreation).And.LessThanOrEqualTo(afterCreation));
    Assert.That(entry.CreatedAt, Is.EqualTo(entry.LastModified).Within(TimeSpan.FromMilliseconds(1)));
}
```

**Test**: `DefaultConstructor_ShouldInitializeMoodValuesToNull`

```csharp
[Test]
public void DefaultConstructor_ShouldInitializeMoodValuesToNull()
{
    // Act
    var entry = new MoodEntry();
    
    // Assert
    Assert.That(entry.StartOfWork, Is.Null);
    Assert.That(entry.EndOfWork, Is.Null);
}
```

##### Parameterized Constructor Tests

**Test**: `ParameterizedConstructor_ShouldSetSpecifiedDate`

```csharp
[Test]
public void ParameterizedConstructor_ShouldSetSpecifiedDate()
{
    // Arrange
    var testDate = new DateOnly(2025, 6, 15);
    
    // Act
    var entry = new MoodEntry(testDate);
    
    // Assert
    Assert.That(entry.Date, Is.EqualTo(testDate));
}
```

**Test**: `ParameterizedConstructor_ShouldSetTimestamps`

```csharp
[Test]
public void ParameterizedConstructor_ShouldSetTimestamps()
{
    // Arrange
    var beforeCreation = DateTime.Now;
    var testDate = new DateOnly(2025, 1, 1);
    
    // Act
    var entry = new MoodEntry(testDate);
    var afterCreation = DateTime.Now;
    
    // Assert
    Assert.That(entry.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation).And.LessThanOrEqualTo(afterCreation));
    Assert.That(entry.LastModified, Is.GreaterThanOrEqualTo(beforeCreation).And.LessThanOrEqualTo(afterCreation));
}
```

### Property: Value (Computed)

#### Purpose

Calculate mood change value based on start and end of work moods.

#### Test Cases

##### Value Calculation Tests

**Test**: `Value_WithBothMoodsSet_ShouldCalculateCorrectly`

```csharp
[Test]
[TestCase(5, 7, 2.0)]  // Mood improved
[TestCase(8, 6, -2.0)] // Mood declined  
[TestCase(5, 5, 0.0)]  // No change
[TestCase(1, 10, 9.0)] // Maximum improvement
[TestCase(10, 1, -9.0)] // Maximum decline
public void Value_WithBothMoodsSet_ShouldCalculateCorrectly(int startOfWork, int endOfWork, double expectedValue)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.Value, Is.EqualTo(expectedValue));
}
```

**Test**: `Value_WithOnlyStartOfWorkSet_ShouldUseStartForBoth`

```csharp
[Test]
[TestCase(1, 0.0)]
[TestCase(5, 0.0)]
[TestCase(10, 0.0)]
public void Value_WithOnlyStartOfWorkSet_ShouldUseStartForBoth(int startOfWork, double expectedValue)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: null);
    
    // Act & Assert
    Assert.That(entry.Value, Is.EqualTo(expectedValue));
}
```

**Test**: `Value_WithNoStartOfWork_ShouldReturnNull`

```csharp
[Test]
public void Value_WithNoStartOfWork_ShouldReturnNull()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: null, endOfWork: 7);
    
    // Act & Assert
    Assert.That(entry.Value, Is.Null);
}
```

### Method: IsValid

#### Purpose

Validate that mood values are within the acceptable range (1-10).

#### Test Cases

##### Validation Tests

**Test**: `IsValid_WithValidMoodValues_ShouldReturnTrue`

```csharp
[Test]
[TestCase(1, 1)]
[TestCase(5, 5)]
[TestCase(10, 10)]
[TestCase(1, 10)]
[TestCase(null, 5)]   // Only end set
[TestCase(5, null)]   // Only start set
[TestCase(null, null)] // Neither set
public void IsValid_WithValidMoodValues_ShouldReturnTrue(int? startOfWork, int? endOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.IsValid(), Is.True);
}
```

**Test**: `IsValid_WithInvalidStartOfWork_ShouldReturnFalse`

```csharp
[Test]
[TestCase(0)]   // Below minimum
[TestCase(11)]  // Above maximum
[TestCase(-1)]  // Negative
[TestCase(100)] // Way above maximum
public void IsValid_WithInvalidStartOfWork_ShouldReturnFalse(int invalidStartOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: invalidStartOfWork, endOfWork: 5);
    
    // Act & Assert
    Assert.That(entry.IsValid(), Is.False);
}
```

**Test**: `IsValid_WithInvalidEndOfWork_ShouldReturnFalse`

```csharp
[Test]
[TestCase(0)]   // Below minimum
[TestCase(11)]  // Above maximum
[TestCase(-5)]  // Negative
[TestCase(50)]  // Way above maximum
public void IsValid_WithInvalidEndOfWork_ShouldReturnFalse(int invalidEndOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 5, endOfWork: invalidEndOfWork);
    
    // Act & Assert
    Assert.That(entry.IsValid(), Is.False);
}
```

**Test**: `IsValid_WithBothInvalid_ShouldReturnFalse`

```csharp
[Test]
public void IsValid_WithBothInvalid_ShouldReturnFalse()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 0, endOfWork: 11);
    
    // Act & Assert
    Assert.That(entry.IsValid(), Is.False);
}
```

### Method: ShouldSave

#### Purpose

Determine if entry meets business rules for persistence.

#### Test Cases

##### Business Rule Tests

**Test**: `ShouldSave_WithValidStartOfWork_ShouldReturnTrue`

```csharp
[Test]
[TestCase(1, null)]
[TestCase(5, null)]
[TestCase(10, null)]
[TestCase(1, 1)]
[TestCase(5, 7)]
[TestCase(10, 10)]
public void ShouldSave_WithValidStartOfWork_ShouldReturnTrue(int startOfWork, int? endOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.ShouldSave(), Is.True);
}
```

**Test**: `ShouldSave_WithNoStartOfWork_ShouldReturnFalse`

```csharp
[Test]
[TestCase(null, null)]
[TestCase(null, 5)]
[TestCase(null, 10)]
public void ShouldSave_WithNoStartOfWork_ShouldReturnFalse(int? startOfWork, int? endOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.ShouldSave(), Is.False);
}
```

**Test**: `ShouldSave_WithInvalidValues_ShouldReturnFalse`

```csharp
[Test]
[TestCase(0, 5)]    // Invalid start
[TestCase(11, 5)]   // Invalid start
[TestCase(5, 0)]    // Invalid end
[TestCase(5, 11)]   // Invalid end
[TestCase(0, 11)]   // Both invalid
public void ShouldSave_WithInvalidValues_ShouldReturnFalse(int startOfWork, int endOfWork)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.ShouldSave(), Is.False);
}
```

### Method: PrepareForSave

#### Purpose

Apply business rules and update timestamps when preparing for persistence.

#### Test Cases

##### Preparation Logic Tests

**Test**: `PrepareForSave_WithValidEntry_ShouldUpdateLastModified`

```csharp
[Test]
public void PrepareForSave_WithValidEntry_ShouldUpdateLastModified()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 5);
    var originalLastModified = entry.LastModified;
    Thread.Sleep(1); // Ensure time difference
    
    // Act
    entry.PrepareForSave();
    
    // Assert
    Assert.That(entry.LastModified, Is.GreaterThan(originalLastModified));
}
```

**Test**: `PrepareForSave_WithAutoSaveDefaults_ShouldSetEndOfWorkToStartOfWork`

```csharp
[Test]
public void PrepareForSave_WithAutoSaveDefaults_ShouldSetEndOfWorkToStartOfWork()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 7, endOfWork: null);
    
    // Act
    entry.PrepareForSave(useAutoSaveDefaults: true);
    
    // Assert
    Assert.That(entry.EndOfWork, Is.EqualTo(7));
}
```

**Test**: `PrepareForSave_WithAutoSaveDefaultsButEndOfWorkSet_ShouldNotOverrideEndOfWork`

```csharp
[Test]
public void PrepareForSave_WithAutoSaveDefaultsButEndOfWorkSet_ShouldNotOverrideEndOfWork()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 5, endOfWork: 8);
    
    // Act
    entry.PrepareForSave(useAutoSaveDefaults: true);
    
    // Assert
    Assert.That(entry.EndOfWork, Is.EqualTo(8)); // Should remain unchanged
}
```

**Test**: `PrepareForSave_WithoutAutoSaveDefaults_ShouldNotSetEndOfWork`

```csharp
[Test]
public void PrepareForSave_WithoutAutoSaveDefaults_ShouldNotSetEndOfWork()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 6, endOfWork: null);
    
    // Act
    entry.PrepareForSave(useAutoSaveDefaults: false);
    
    // Assert
    Assert.That(entry.EndOfWork, Is.Null);
}
```

**Test**: `PrepareForSave_WithInvalidEntry_ShouldNotModifyAnything`

```csharp
[Test]
public void PrepareForSave_WithInvalidEntry_ShouldNotModifyAnything()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: null, endOfWork: 5); // Invalid: no start mood
    var originalLastModified = entry.LastModified;
    var originalEndOfWork = entry.EndOfWork;
    Thread.Sleep(1); // Ensure time difference would be detectable
    
    // Act
    entry.PrepareForSave(useAutoSaveDefaults: true);
    
    // Assert
    Assert.That(entry.LastModified, Is.EqualTo(originalLastModified));
    Assert.That(entry.EndOfWork, Is.EqualTo(originalEndOfWork));
}
```

### Method: GetAverageMood

#### Purpose

Calculate average mood when both start and end moods are available.

#### Test Cases

##### Average Calculation Tests

**Test**: `GetAverageMood_WithBothMoodsSet_ShouldCalculateAverage`

```csharp
[Test]
[TestCase(1, 1, 1.0)]
[TestCase(1, 9, 5.0)]
[TestCase(5, 7, 6.0)]
[TestCase(10, 6, 8.0)]
[TestCase(3, 8, 5.5)]
public void GetAverageMood_WithBothMoodsSet_ShouldCalculateAverage(int startOfWork, int endOfWork, double expectedAverage)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.GetAverageMood(), Is.EqualTo(expectedAverage));
}
```

**Test**: `GetAverageMood_WithOnlyStartOfWork_ShouldReturnNull`

```csharp
[Test]
public void GetAverageMood_WithOnlyStartOfWork_ShouldReturnNull()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 5, endOfWork: null);
    
    // Act & Assert
    Assert.That(entry.GetAverageMood(), Is.Null);
}
```

**Test**: `GetAverageMood_WithOnlyEndOfWork_ShouldReturnNull`

```csharp
[Test]
public void GetAverageMood_WithOnlyEndOfWork_ShouldReturnNull()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: null, endOfWork: 7);
    
    // Act & Assert
    Assert.That(entry.GetAverageMood(), Is.Null);
}
```

**Test**: `GetAverageMood_WithNoMoods_ShouldReturnNull`

```csharp
[Test]
public void GetAverageMood_WithNoMoods_ShouldReturnNull()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: null, endOfWork: null);
    
    // Act & Assert
    Assert.That(entry.GetAverageMood(), Is.Null);
}
```

### Method: GetAdjustedAverageMood

#### Purpose

Calculate adjusted average mood for graphing with shifted range (-4 to +5).

#### Test Cases

##### Adjusted Average Tests

**Test**: `GetAdjustedAverageMood_WithBothMoodsSet_ShouldCalculateAdjustedAverage`

```csharp
[Test]
[TestCase(1, 1, -4.0)]   // Average 1.0, adjusted: 1.0 - 5.0 = -4.0
[TestCase(5, 5, 0.0)]    // Average 5.0, adjusted: 5.0 - 5.0 = 0.0  
[TestCase(10, 10, 5.0)]  // Average 10.0, adjusted: 10.0 - 5.0 = 5.0
[TestCase(1, 9, 0.0)]    // Average 5.0, adjusted: 5.0 - 5.0 = 0.0
[TestCase(3, 7, 0.0)]    // Average 5.0, adjusted: 5.0 - 5.0 = 0.0
[TestCase(2, 4, -2.0)]   // Average 3.0, adjusted: 3.0 - 5.0 = -2.0
[TestCase(7, 9, 3.0)]    // Average 8.0, adjusted: 8.0 - 5.0 = 3.0
public void GetAdjustedAverageMood_WithBothMoodsSet_ShouldCalculateAdjustedAverage(
    int startOfWork, int endOfWork, double expectedAdjusted)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act & Assert
    Assert.That(entry.GetAdjustedAverageMood(), Is.EqualTo(expectedAdjusted));
}
```

**Test**: `GetAdjustedAverageMood_WithOnlyStartOfWork_ShouldUseStartForBoth`

```csharp
[Test]
[TestCase(1, -4.0)]   // (1+1)/2 - 5 = -4.0
[TestCase(5, 0.0)]    // (5+5)/2 - 5 = 0.0
[TestCase(10, 5.0)]   // (10+10)/2 - 5 = 5.0
[TestCase(3, -2.0)]   // (3+3)/2 - 5 = -2.0
[TestCase(8, 3.0)]    // (8+8)/2 - 5 = 3.0
public void GetAdjustedAverageMood_WithOnlyStartOfWork_ShouldUseStartForBoth(int startOfWork, double expectedAdjusted)
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: startOfWork, endOfWork: null);
    
    // Act & Assert
    Assert.That(entry.GetAdjustedAverageMood(), Is.EqualTo(expectedAdjusted));
}
```

**Test**: `GetAdjustedAverageMood_WithNoStartOfWork_ShouldReturnNull`

```csharp
[Test]
public void GetAdjustedAverageMood_WithNoStartOfWork_ShouldReturnNull()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: null, endOfWork: 7);
    
    // Act & Assert
    Assert.That(entry.GetAdjustedAverageMood(), Is.Null);
}
```

### Method: ToString

#### Purpose

Provide formatted string representation of mood entry.

#### Test Cases

##### String Formatting Tests

**Test**: `ToString_WithBothMoodsSet_ShouldFormatCorrectly`

```csharp
[Test]
public void ToString_WithBothMoodsSet_ShouldFormatCorrectly()
{
    // Arrange
    var date = new DateOnly(2025, 3, 15);
    var entry = CreateTestEntry(date: date, startOfWork: 6, endOfWork: 8);
    
    // Act
    var result = entry.ToString();
    
    // Assert
    Assert.That(result, Is.EqualTo("2025-03-15: Start of Work 6, End of Work 8"));
}
```

**Test**: `ToString_WithNullMoods_ShouldShowNotRecorded`

```csharp
[Test]
public void ToString_WithNullMoods_ShouldShowNotRecorded()
{
    // Arrange
    var date = new DateOnly(2025, 1, 1);
    var entry = CreateTestEntry(date: date, startOfWork: null, endOfWork: null);
    
    // Act
    var result = entry.ToString();
    
    // Assert
    Assert.That(result, Is.EqualTo("2025-01-01: Start of Work Not recorded, End of Work Not recorded"));
}
```

**Test**: `ToString_WithPartialMoods_ShouldShowMixedFormat`

```csharp
[Test]
[TestCase(5, null, "2025-06-10: Start of Work 5, End of Work Not recorded")]
[TestCase(null, 7, "2025-06-10: Start of Work Not recorded, End of Work 7")]
public void ToString_WithPartialMoods_ShouldShowMixedFormat(int? startOfWork, int? endOfWork, string expectedFormat)
{
    // Arrange
    var date = new DateOnly(2025, 6, 10);
    var entry = CreateTestEntry(date: date, startOfWork: startOfWork, endOfWork: endOfWork);
    
    // Act
    var result = entry.ToString();
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedFormat));
}
```

### Property Tests: Basic Get/Set

#### Purpose

Verify basic property get/set functionality.

#### Test Cases

##### Property Assignment Tests

**Test**: `Properties_ShouldAllowGetSet`

```csharp
[Test]
public void Properties_ShouldAllowGetSet()
{
    // Arrange
    var entry = new MoodEntry();
    var testDate = new DateOnly(2025, 12, 25);
    var testCreated = new DateTime(2025, 1, 1, 10, 30, 0);
    var testModified = new DateTime(2025, 1, 1, 15, 45, 30);
    
    // Act
    entry.Date = testDate;
    entry.StartOfWork = 3;
    entry.EndOfWork = 9;
    entry.CreatedAt = testCreated;
    entry.LastModified = testModified;
    
    // Assert
    Assert.That(entry.Date, Is.EqualTo(testDate));
    Assert.That(entry.StartOfWork, Is.EqualTo(3));
    Assert.That(entry.EndOfWork, Is.EqualTo(9));
    Assert.That(entry.CreatedAt, Is.EqualTo(testCreated));
    Assert.That(entry.LastModified, Is.EqualTo(testModified));
}
```

### Serialization Tests

#### Purpose

Verify JSON serialization behavior, particularly JsonIgnore attribute.

#### Test Cases

##### JSON Serialization Tests

**Test**: `JsonSerialization_ShouldExcludeValueProperty`

```csharp
[Test]
public void JsonSerialization_ShouldExcludeValueProperty()
{
    // Arrange
    var entry = CreateTestEntry(startOfWork: 5, endOfWork: 8);
    
    // Act
    var json = JsonSerializer.Serialize(entry);
    
    // Assert
    Assert.That(json, Does.Not.Contain("Value"));
    Assert.That(json, Does.Not.Contain("\"value\""));
}
```

**Test**: `JsonSerialization_ShouldIncludeAllOtherProperties`

```csharp
[Test]
public void JsonSerialization_ShouldIncludeAllOtherProperties()
{
    // Arrange
    var entry = CreateTestEntry(date: new DateOnly(2025, 1, 15), startOfWork: 6, endOfWork: 9);
    
    // Act
    var json = JsonSerializer.Serialize(entry);
    
    // Assert
    Assert.That(json, Does.Contain("Date"));
    Assert.That(json, Does.Contain("StartOfWork"));
    Assert.That(json, Does.Contain("EndOfWork"));
    Assert.That(json, Does.Contain("CreatedAt"));
    Assert.That(json, Does.Contain("LastModified"));
}
```

**Test**: `JsonDeserialization_ShouldRestoreProperties`

```csharp
[Test]
public void JsonDeserialization_ShouldRestoreProperties()
{
    // Arrange
    var originalEntry = CreateTestEntry(date: new DateOnly(2025, 2, 28), startOfWork: 4, endOfWork: 7);
    var json = JsonSerializer.Serialize(originalEntry);
    
    // Act
    var deserializedEntry = JsonSerializer.Deserialize<MoodEntry>(json);
    
    // Assert
    Assert.That(deserializedEntry.Date, Is.EqualTo(originalEntry.Date));
    Assert.That(deserializedEntry.StartOfWork, Is.EqualTo(originalEntry.StartOfWork));
    Assert.That(deserializedEntry.EndOfWork, Is.EqualTo(originalEntry.EndOfWork));
    Assert.That(deserializedEntry.CreatedAt, Is.EqualTo(originalEntry.CreatedAt));
    Assert.That(deserializedEntry.LastModified, Is.EqualTo(originalEntry.LastModified));
}
```

## Test Implementation Notes

### Testing Challenges

1. **DateTime Dependencies**: Constructors and PrepareForSave use DateTime.Now
2. **Time-Sensitive Assertions**: Timestamp testing requires time window assertions
3. **Business Logic Integration**: ShouldSave calls IsValid internally
4. **Computed Properties**: Value property depends on multiple property states

### Recommended Approach

1. **Time Window Testing**: Use time ranges rather than exact timestamp matching
2. **Business Logic Testing**: Test business rules comprehensively with various combinations
3. **Property State Testing**: Test computed properties with all possible input combinations
4. **Edge Case Coverage**: Test boundary conditions and null scenarios

### Test Data Helper Methods

```csharp
private static MoodEntry CreateTestEntry(DateOnly? date = null, int? startOfWork = null, int? endOfWork = null)
{
    var entry = date.HasValue ? new MoodEntry(date.Value) : new MoodEntry();
    entry.StartOfWork = startOfWork;
    entry.EndOfWork = endOfWork;
    return entry;
}

private static void AssertTimestampWithinRange(DateTime actual, DateTime expectedStart, DateTime expectedEnd)
{
    Assert.That(actual, Is.GreaterThanOrEqualTo(expectedStart).And.LessThanOrEqualTo(expectedEnd));
}

private static void AssertMoodEntryProperties(MoodEntry entry, DateOnly expectedDate, 
    int? expectedStart, int? expectedEnd)
{
    Assert.That(entry.Date, Is.EqualTo(expectedDate));
    Assert.That(entry.StartOfWork, Is.EqualTo(expectedStart));
    Assert.That(entry.EndOfWork, Is.EqualTo(expectedEnd));
}
```

### Test Organization

```
MauiApp.Tests/
├── Models/
│   ├── MoodEntryTests.cs
│   ├── TestHelpers/
│   │   ├── MoodEntryBuilder.cs
│   │   ├── MoodEntryTestData.cs
│   │   └── TimestampAssertions.cs
```

## Coverage Goals

- **Method Coverage**: 100% - All public methods and properties
- **Line Coverage**: 95% - All business logic and calculations
- **Branch Coverage**: 100% - All validation conditions and computed property logic
- **Business Logic Coverage**: 100% - All business rules and edge cases

## Implementation Checklist

### Phase 1 - Core Functionality Tests

- [ ] **Constructor Tests**: Default and parameterized constructors with timestamp verification
- [ ] **Property Tests**: Basic get/set functionality for all properties
- [ ] **Value Property Tests**: Computed property calculation with all mood combinations
- [ ] **Validation Tests**: IsValid method with boundary conditions and null values

### Phase 2 - Business Logic Tests

- [ ] **ShouldSave Tests**: Business rule verification with valid/invalid scenarios
- [ ] **PrepareForSave Tests**: Business logic application and timestamp updates
- [ ] **Auto-save Logic Tests**: Auto-save defaults behavior verification
- [ ] **Integration Tests**: Business method interactions

### Phase 3 - Calculation Tests

- [ ] **GetAverageMood Tests**: Average calculation with various mood combinations
- [ ] **GetAdjustedAverageMood Tests**: Adjusted average calculation for graphing
- [ ] **Edge Case Tests**: Null value handling and boundary conditions
- [ ] **Mathematical Accuracy Tests**: Precise calculation verification

### Phase 4 - Additional Features Tests

- [ ] **ToString Tests**: String formatting with various property states
- [ ] **Serialization Tests**: JSON serialization behavior and JsonIgnore verification
- [ ] **Timestamp Tests**: DateTime dependency testing with time windows
- [ ] **Coverage Analysis**: Verify 95%+ line coverage achieved

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for MoodEntry domain model with business logic testing`
- `^f - add validation and calculation tests for MoodEntry mood value handling`
- `^f - add business rule tests for MoodEntry ShouldSave and PrepareForSave methods`
- `^f - add serialization and string formatting tests for MoodEntry`

## Risk Assessment

- **Low Risk**: Well-defined business logic with clear validation rules
- **Medium Risk**: DateTime dependencies require time window testing approach
- **Low Risk**: Pure calculation methods are deterministic and easily testable
- **Low Risk**: Domain model with minimal external dependencies

## Refactoring Recommendations

### Current Design Assessment

The `MoodEntry` domain model demonstrates good design for testing:

**Strengths**:
- **Clear Business Logic**: Well-defined validation and save rules
- **Pure Calculations**: Most methods are pure functions
- **Comprehensive API**: Complete set of operations for mood tracking
- **Nullable Design**: Proper handling of optional mood values
- **Computed Properties**: Clean derived data calculations

**Minor Considerations**:
- **DateTime Dependencies**: Static time dependencies could be abstracted
- **Side Effects**: PrepareForSave modifies object state

### Recommended Approach

1. **Current Design Is Adequate**: Domain model works well for current testing needs
2. **Work Around Time Dependencies**: Use time window assertions rather than exact matches
3. **Focus on Business Logic**: Prioritize testing validation and calculation logic
4. **Future Enhancement**: Consider DateTime abstraction for more advanced testing scenarios

**Recommendation**: Current design provides good testability for a domain model. Create comprehensive test suite focusing on business logic, validation rules, and calculations without major refactoring. The static DateTime dependencies are manageable with time window testing approaches. 🤖