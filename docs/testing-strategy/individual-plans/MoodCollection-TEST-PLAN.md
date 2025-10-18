# MoodCollection Test Plan

## Object Analysis

**File**: `MauiApp/Models/MoodCollection.cs`  
**Type**: Domain Collection Class  
**Primary Purpose**: Manage collection of MoodEntry objects with business logic operations  
**Key Functionality**: Collection management, filtering, calculations, and data operations

### Purpose & Responsibilities

The `MoodCollection` class serves as a domain collection for managing `MoodEntry` objects. It provides:
- Collection management (add/update, remove, retrieve)
- Date-based filtering and range queries
- Aggregate calculations (averages, trends)
- Business logic for mood data operations
- Chronological sorting and data integrity

### Architecture Role

- **Layer**: Domain Model Layer
- **Pattern**: Collection Domain Object with Business Logic
- **MVVM Role**: Data model used by Services and ViewModels
- **Clean Architecture**: Domain entity collection with business rules

### Dependencies Analysis

#### Direct Dependencies

**MoodEntry Class**:
- Used throughout for collection items
- Calls `entry.ShouldSave()`, `entry.PrepareForSave()`, `entry.GetAverageMood()`

**DateTime Static Dependencies**:
- `DateTime.Today` in `GetRecentEntries()` method
- `DateTime.Now` in `AddOrUpdate()` for LastModified timestamp

#### Platform Dependencies

- **.NET Collections**: `List<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`
- **LINQ**: Extensive use for filtering and calculations

### Public Interface Documentation

#### Properties

**`IReadOnlyList<MoodEntry> Entries`**
- **Purpose**: Read-only access to all mood entries in chronological order
- **Type**: `IReadOnlyList<MoodEntry>` - Immutable collection view
- **Behavior**: Returns entries sorted by date (most recent first)

**`int Count`**
- **Purpose**: Total number of entries in collection
- **Type**: `int` - Count of entries
- **Behavior**: Returns current collection size

#### Constructors

**`MoodCollection()`**
- **Purpose**: Creates empty mood collection
- **Behavior**: Initializes empty internal list

**`MoodCollection(IEnumerable<MoodEntry> entries)`**
- **Purpose**: Creates collection with existing entries
- **Parameters**: `entries` - Initial mood entries
- **Behavior**: Adds entries and sorts by date

#### Methods

**`void AddOrUpdate(MoodEntry entry, bool useAutoSaveDefaults = false)`**
- **Purpose**: Adds new entry or updates existing entry for same date
- **Business Logic**: Only saves if `entry.ShouldSave()` returns true
- **Parameters**: `entry` - Entry to add/update, `useAutoSaveDefaults` - Apply auto-save rules
- **Behavior**: Merges mood values if entry exists, adds new if not

**`MoodEntry? GetEntry(DateOnly date)`**
- **Purpose**: Retrieves entry for specific date
- **Return**: Entry if found, null otherwise
- **Behavior**: Linear search by date

**`IEnumerable<MoodEntry> GetEntriesInRange(DateOnly startDate, DateOnly endDate)`**
- **Purpose**: Gets entries within date range (inclusive)
- **Parameters**: `startDate`, `endDate` - Date range boundaries
- **Return**: Filtered entries within range

**`IEnumerable<MoodEntry> GetRecentEntries(int count = 7)`**
- **Purpose**: Gets most recent entries excluding today
- **Business Rule**: Excludes today's entry for historical view
- **Parameters**: `count` - Number of entries to return
- **Static Dependency**: Uses `DateTime.Today`

**`double? GetOverallAverageMood()`**
- **Purpose**: Calculates average mood across all valid entries
- **Return**: Average mood or null if no valid entries
- **Logic**: Uses `MoodEntry.GetAverageMood()` for each entry

**`double? GetAverageMoodForPeriod(DateOnly startDate, DateOnly endDate)`**
- **Purpose**: Calculates average mood for specific period
- **Parameters**: Date range for calculation
- **Return**: Period average or null if no valid entries

**`string GetMoodTrend(int days = 7)`**
- **Purpose**: Analyzes mood trend over recent days
- **Algorithm**: Compares first half vs second half of recent entries
- **Return**: "Improving", "Declining", "Stable", or "Insufficient data"
- **Threshold**: 0.5 mood point difference for trend classification

**`int RemoveEntriesOlderThan(DateOnly cutoffDate)`**
- **Purpose**: Removes entries older than specified date
- **Parameters**: `cutoffDate` - Cutoff for removal
- **Return**: Number of entries removed
- **Side Effects**: Modifies internal collection

## Testability Assessment

**Overall Testability Score: 8/10**

### Strengths

- ✅ **Clear Business Logic**: Well-defined collection operations and calculations
- ✅ **Deterministic Behavior**: Most methods are pure or predictable
- ✅ **Comprehensive API**: Complete set of collection operations
- ✅ **Separation of Concerns**: Collection logic separate from persistence
- ✅ **Null Safety**: Proper handling of null entries and empty collections

### Challenges

- ⚠️ **DateTime.Today Dependency**: `GetRecentEntries()` uses current date
- ⚠️ **DateTime.Now Dependency**: `AddOrUpdate()` sets timestamps
- ⚠️ **MoodEntry Dependencies**: Relies on MoodEntry business logic

### Required Refactoring

**Low Priority** - Current design is very testable

**Potential Enhancement**: Abstract DateTime dependencies for more predictable testing, but current design allows for effective testing with time-window approaches.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class MoodCollectionTests
{
    private MoodCollection _collection;
    private readonly DateOnly _testDate1 = new(2025, 1, 15);
    private readonly DateOnly _testDate2 = new(2025, 1, 16);
    
    [SetUp]
    public void Setup()
    {
        _collection = new MoodCollection();
    }
    
    private static MoodEntry CreateTestEntry(DateOnly date, int? start = null, int? end = null)
    {
        var entry = new MoodEntry(date);
        entry.StartOfWork = start;
        entry.EndOfWork = end;
        return entry;
    }
}
```

### Mock Strategy

**No mocking required** - Testing domain collection with direct verification of operations.

### Test Categories

1. **Constructor Tests**: Empty and parameterized initialization
2. **Collection Management**: Add/update, retrieve, remove operations
3. **Filtering Tests**: Date ranges, recent entries, specific dates
4. **Calculation Tests**: Averages, trends, aggregations
5. **Business Logic Tests**: Auto-save behavior, update logic
6. **Edge Cases**: Empty collections, null inputs, boundary conditions

## Test Implementation Plan

### Phase 1: Core Collection Operations
- Constructor tests (empty and with data)
- AddOrUpdate method (new entries, updates, business rules)
- GetEntry method (found, not found)
- Basic property access (Entries, Count)

### Phase 2: Filtering and Queries
- GetEntriesInRange (various ranges, edge cases)
- GetRecentEntries (excluding today, count limits)
- Date-based filtering logic

### Phase 3: Calculations and Analytics
- GetOverallAverageMood (various scenarios)
- GetAverageMoodForPeriod (date ranges)
- GetMoodTrend (trend analysis algorithm)

### Phase 4: Data Management
- RemoveEntriesOlderThan (cleanup operations)
- Chronological sorting verification
- Collection integrity tests

## Detailed Test Cases

### Constructor Tests

**Test**: `DefaultConstructor_ShouldCreateEmptyCollection`
- Verify Count = 0, Entries.Count = 0

**Test**: `ParameterizedConstructor_ShouldAddAndSortEntries`
- Create with multiple entries, verify sorting and count

### AddOrUpdate Tests

**Test**: `AddOrUpdate_WithValidEntry_ShouldAddToCollection`
- Add new entry, verify count increase and entry presence

**Test**: `AddOrUpdate_WithInvalidEntry_ShouldNotAdd`
- Add entry that fails ShouldSave(), verify no change

**Test**: `AddOrUpdate_WithExistingDate_ShouldUpdateMoodValues`
- Update existing entry, verify mood value merging

**Test**: `AddOrUpdate_WithAutoSaveDefaults_ShouldApplyDefaults`
- Test auto-save behavior on updates

### Filtering Tests

**Test**: `GetEntriesInRange_WithValidRange_ShouldReturnFilteredEntries`
- Test various date ranges with inclusive boundaries

**Test**: `GetRecentEntries_ShouldExcludeTodayAndLimitCount`
- Verify today exclusion and count limiting

### Calculation Tests

**Test**: `GetOverallAverageMood_WithValidEntries_ShouldCalculateCorrectly`
- Test average calculation across multiple entries

**Test**: `GetMoodTrend_WithSufficientData_ShouldClassifyTrend`
- Test "Improving", "Declining", "Stable" classifications

### Edge Case Tests

**Test**: `EmptyCollection_ShouldHandleGracefully`
- All methods should handle empty collection appropriately

**Test**: `NullInputs_ShouldHandleGracefully`
- Methods should handle null parameters appropriately

## Coverage Goals

- **Method Coverage**: 100% - All public methods and properties
- **Line Coverage**: 95% - All business logic and calculations  
- **Branch Coverage**: 100% - All conditional logic paths
- **Business Logic Coverage**: 100% - All collection operations and calculations

## Implementation Checklist

- [ ] **Constructor Tests**: Empty and parameterized initialization
- [ ] **AddOrUpdate Tests**: Business logic, merging, auto-save behavior
- [ ] **Retrieval Tests**: GetEntry, GetEntriesInRange, GetRecentEntries
- [ ] **Calculation Tests**: Averages, trends, aggregations
- [ ] **Management Tests**: Remove operations, sorting, integrity
- [ ] **Edge Case Tests**: Empty collection, null inputs, boundaries
- [ ] **Integration Tests**: MoodEntry interaction verification

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for MoodCollection domain collection`
- `^f - add collection management tests for MoodCollection add/update operations`  
- `^f - add filtering and query tests for MoodCollection date-based operations`
- `^f - add calculation and analytics tests for MoodCollection business logic`

## Risk Assessment

- **Low Risk**: Well-defined collection operations with clear business rules
- **Medium Risk**: DateTime dependencies require careful time-based testing
- **Low Risk**: Pure calculation methods are deterministic and easily testable
- **Low Risk**: Domain collection with minimal external dependencies

## Refactoring Recommendations

**Current Design Assessment**: The `MoodCollection` demonstrates excellent design for a domain collection with clear separation of concerns and comprehensive business logic.

**Recommendation**: Current design provides very good testability. Focus on comprehensive test coverage of business logic, collection operations, and calculation algorithms. DateTime dependencies are manageable with time-window testing approaches. 🤖