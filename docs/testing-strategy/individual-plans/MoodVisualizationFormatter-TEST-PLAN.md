# MoodVisualizationFormatter Test Plan

## Object Analysis

**File**: `MauiApp/Processors/MoodVisualizationFormatter.cs`  
**Type**: Static Utility Class  
**Primary Purpose**: Format and transform mood visualization data for UI consumption  
**Key Functionality**: Provides static methods for transforming service data into UI-friendly formats, generating summaries, and converting colors to hex strings

### Purpose & Responsibilities

The `MoodVisualizationFormatter` serves as a static utility class that transforms raw mood visualization data into formats suitable for UI binding and display. It handles data formatting, color conversion, value descriptions, and summary generation for mood visualization components. The class bridges between service-layer data structures and UI-friendly representations.

### Architecture Role

- **Layer**: Processor/Utility Layer
- **Pattern**: Static Utility Functions
- **MVVM Role**: Data transformation support for ViewModels and Views
- **Clean Architecture**: Application layer utility for data presentation formatting

### Dependencies Analysis

#### Static Dependencies

**GetTwoWeekMoodVisualizationAsync**:
- **MoodDataService**: Concrete service dependency for data retrieval
- **Task<T>**: Async operation support

**GetVisualizationSummary**:
- **MoodVisualizationData**: Data structure dependency
- **LINQ**: Enumerable operations (Count, Where, Select, Average, DefaultIfEmpty)
- **String interpolation**: Formatting operations

**GetMoodDayInfoListAsync**:
- **MoodDataService**: Concrete service dependency
- **Task<T>**: Async operation support
- **ColorToHex**: Internal static method dependency
- **GetValueDescription**: Internal static method dependency

**ColorToHex**:
- **Microsoft.Maui.Graphics.Color**: Color manipulation

**GetValueDescription**:
- **Pattern matching**: C# switch expressions

#### Platform Dependencies

- **.NET LINQ**: Standard library operations
- **Microsoft.Maui.Graphics**: MAUI graphics color system

### Public Interface Documentation

#### Static Methods

**`Task<MoodVisualizationData> GetTwoWeekMoodVisualizationAsync(MoodDataService moodDataService)`**

- **Purpose**: Retrieves 2-week mood visualization data from service
- **Parameters**: `moodDataService` - Concrete service instance for data access
- **Return Type**: `Task<MoodVisualizationData>` - Service data structure
- **Side Effects**: None (pass-through to service)
- **Async Behavior**: Async delegation to service call

**`string GetVisualizationSummary(MoodVisualizationData visualizationData)`**

- **Purpose**: Generates human-readable summary of visualization data
- **Parameters**: `visualizationData` - Visualization data to summarize
- **Return Type**: `string` - Formatted summary text
- **Side Effects**: None (pure function)
- **Logic**: 
  - Counts days with data vs total days
  - Calculates average mood value
  - Handles no-data case with encouraging message
  - Formats summary with statistics and date range

**`Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(MoodDataService moodDataService)`**

- **Purpose**: Transforms service data into UI-friendly format for data binding
- **Parameters**: `moodDataService` - Concrete service instance
- **Return Type**: `Task<List<MoodDayInfo>>` - UI-friendly data objects
- **Side Effects**: None (data transformation)
- **Transformation Logic**:
  - Converts raw daily values to MoodDayInfo objects
  - Transforms colors to hex strings via ColorToHex
  - Adds day of week information
  - Generates value descriptions via GetValueDescription

#### Static Utility Methods

**`string ColorToHex(Microsoft.Maui.Graphics.Color color)` (Private)**

- **Purpose**: Converts Color to hex string format for UI binding
- **Parameters**: `color` - MAUI Graphics color object
- **Return Type**: `string` - Hex format (#RRGGBB)
- **Logic**: RGB component conversion to 0-255 range and hex formatting

**`string GetValueDescription(double? value)` (Private)**

- **Purpose**: Converts numeric mood change to human-readable description
- **Parameters**: `value` - Nullable mood change value
- **Return Type**: `string` - Descriptive text
- **Logic**: Pattern matching on value ranges for improvement/decline descriptions

#### Properties

- **None**: Static utility class with no properties

#### Commands

- **None**: Not applicable for static utility class

#### Events

- **None**: No events raised

### Data Structures

#### MoodDayInfo Class

**Purpose**: UI binding data structure for mood visualization

**Properties**:
- `DateOnly Date` - Date of the mood entry
- `double? Value` - Nullable mood change value
- `bool HasData` - Indicates if mood data exists for the date
- `string ColorHex` - Hex color string for UI binding
- `string DayOfWeek` - Day of week name
- `string ValueDescription` - Human-readable mood change description

## Testability Assessment

**Overall Testability Score: 8/10**

### Strengths

- ✅ **Static Utility Functions**: Easy to test with direct input/output verification
- ✅ **Pure Functions**: GetVisualizationSummary, ColorToHex, GetValueDescription have no side effects
- ✅ **Clear Transformations**: Predictable data transformations easy to verify
- ✅ **Isolated Logic**: Each method has single responsibility
- ✅ **Data Structure Testing**: MoodDayInfo is simple POCO with clear properties

### Challenges

- ⚠️ **Concrete Service Dependency**: Uses concrete MoodDataService instead of interface
- ⚠️ **Async Testing**: Requires async test patterns for service-dependent methods
- ⚠️ **Service Mocking**: Concrete dependency requires careful mocking strategy

### Current Testability Score Justification

Score: **8/10** - Good testability with minor dependency issues

**Deductions**:
- **-1 point**: Concrete MoodDataService dependency makes mocking more complex
- **-1 point**: Async methods require additional test complexity

### Hard Dependencies Identified

1. **MoodDataService**: Concrete service class (should be IMoodDataService interface)
2. **MoodVisualizationData**: Service data structure
3. **Microsoft.Maui.Graphics.Color**: Platform color system
4. **Task<T>**: Async operation framework

### Required Refactoring

**Medium Priority Refactoring Needed**

The concrete `MoodDataService` dependency should be refactored to use the `IMoodDataService` interface for better testability:

```csharp
// Current problematic dependencies
public static async Task<MoodVisualizationData> GetTwoWeekMoodVisualizationAsync(MoodDataService moodDataService)
public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(MoodDataService moodDataService)

// Recommended interface-based approach
public static async Task<MoodVisualizationData> GetTwoWeekMoodVisualizationAsync(IMoodDataService moodDataService)
public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(IMoodDataService moodDataService)
```

**Benefits of Refactoring**:
- **Easier Mocking**: Interface-based mocking is more straightforward
- **Better Testability**: Reduces dependency on concrete implementation
- **Improved Design**: Follows dependency inversion principle
- **Test Isolation**: Better separation of concerns in testing

**Alternative**: If maintaining static utility pattern, consider moving these methods to a non-static service class that properly takes interface dependencies through constructor injection.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class MoodVisualizationFormatterTests
{
    // For testing methods that require mocked service
    private Mock<MoodDataService> _mockMoodDataService;
    
    [SetUp]
    public void Setup()
    {
        // Note: This will require careful mocking of concrete class
        _mockMoodDataService = new Mock<MoodDataService>();
    }
    
    // Test methods here
}
```

### Mock Strategy

**Service-Dependent Methods**: Mock concrete MoodDataService (complex due to concrete dependency)
**Pure Functions**: Direct testing without mocking
**Data Structure**: Simple POCO testing

### Test Categories

1. **Pure Function Tests**: GetVisualizationSummary, ColorToHex, GetValueDescription
2. **Service Integration Tests**: GetTwoWeekMoodVisualizationAsync, GetMoodDayInfoListAsync  
3. **Data Transformation Tests**: MoodDayInfo creation and property mapping
4. **Edge Case Tests**: Null values, empty data, boundary conditions
5. **Async Behavior Tests**: Proper async delegation to service calls

## Detailed Test Cases

### Method: GetVisualizationSummary

#### Purpose

Generates human-readable summary of mood visualization data with statistics and encouraging messages.

#### Test Cases

##### Normal Data Cases

**Test**: `GetVisualizationSummary_WithMixedData_ShouldReturnFormattedSummary`

```csharp
[Test]
public void GetVisualizationSummary_WithMixedData_ShouldReturnFormattedSummary()
{
    // Arrange
    var startDate = new DateOnly(2025, 1, 1);
    var endDate = new DateOnly(2025, 1, 3);
    var dailyValues = new[]
    {
        new DailyMoodValue { Date = startDate, HasData = true, Value = 2.0 },
        new DailyMoodValue { Date = startDate.AddDays(1), HasData = false, Value = null },
        new DailyMoodValue { Date = endDate, HasData = true, Value = -1.0 }
    };
    var visualizationData = new MoodVisualizationData
    {
        StartDate = startDate,
        EndDate = endDate,
        DailyValues = dailyValues
    };
    
    // Act
    var result = MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    
    // Assert
    Assert.That(result, Does.Contain("Showing 2 days of data out of 3 days"));
    Assert.That(result, Does.Contain("Average mood change value: 0.50")); // (2.0 + (-1.0)) / 2 = 0.5
    Assert.That(result, Does.Contain("Date range: 2025-01-01 to 2025-01-03"));
}
```

**Test**: `GetVisualizationSummary_WithAllDataPresent_ShouldShowCompleteStatistics`

```csharp
[Test]
public void GetVisualizationSummary_WithAllDataPresent_ShouldShowCompleteStatistics()
{
    // Arrange
    var startDate = new DateOnly(2025, 1, 1);
    var endDate = new DateOnly(2025, 1, 2);
    var dailyValues = new[]
    {
        new DailyMoodValue { Date = startDate, HasData = true, Value = 1.5 },
        new DailyMoodValue { Date = endDate, HasData = true, Value = 2.5 }
    };
    var visualizationData = new MoodVisualizationData
    {
        StartDate = startDate,
        EndDate = endDate,
        DailyValues = dailyValues
    };
    
    // Act
    var result = MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    
    // Assert
    Assert.That(result, Does.Contain("Showing 2 days of data out of 2 days"));
    Assert.That(result, Does.Contain("Average mood change value: 2.00")); // (1.5 + 2.5) / 2 = 2.0
}
```

##### No Data Cases

**Test**: `GetVisualizationSummary_WithNoData_ShouldReturnEncouragingMessage`

```csharp
[Test]
public void GetVisualizationSummary_WithNoData_ShouldReturnEncouragingMessage()
{
    // Arrange
    var startDate = new DateOnly(2025, 1, 1);
    var endDate = new DateOnly(2025, 1, 3);
    var dailyValues = new[]
    {
        new DailyMoodValue { Date = startDate, HasData = false, Value = null },
        new DailyMoodValue { Date = startDate.AddDays(1), HasData = false, Value = null },
        new DailyMoodValue { Date = endDate, HasData = false, Value = null }
    };
    var visualizationData = new MoodVisualizationData
    {
        StartDate = startDate,
        EndDate = endDate,
        DailyValues = dailyValues
    };
    
    // Act
    var result = MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    
    // Assert
    Assert.That(result, Does.Contain("No mood data available"));
    Assert.That(result, Does.Contain("2025-01-01 to 2025-01-03"));
    Assert.That(result, Does.Contain("Start recording your daily moods"));
}
```

**Test**: `GetVisualizationSummary_WithEmptyDailyValues_ShouldHandleGracefully`

```csharp
[Test]
public void GetVisualizationSummary_WithEmptyDailyValues_ShouldHandleGracefully()
{
    // Arrange
    var visualizationData = new MoodVisualizationData
    {
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 1),
        DailyValues = Array.Empty<DailyMoodValue>()
    };
    
    // Act
    var result = MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    
    // Assert
    Assert.That(result, Does.Contain("No mood data available"));
}
```

##### Edge Cases

**Test**: `GetVisualizationSummary_WithSingleDay_ShouldHandleSingularForm`

```csharp
[Test]
public void GetVisualizationSummary_WithSingleDay_ShouldHandleSingularForm()
{
    // Arrange
    var date = new DateOnly(2025, 1, 1);
    var dailyValues = new[]
    {
        new DailyMoodValue { Date = date, HasData = true, Value = 1.0 }
    };
    var visualizationData = new MoodVisualizationData
    {
        StartDate = date,
        EndDate = date,
        DailyValues = dailyValues
    };
    
    // Act
    var result = MoodVisualizationFormatter.GetVisualizationSummary(visualizationData);
    
    // Assert
    Assert.That(result, Does.Contain("Showing 1 days of data out of 1 days"));
    Assert.That(result, Does.Contain("Average mood change value: 1.00"));
}
```

### Method: ColorToHex (Private - Test via GetMoodDayInfoListAsync)

#### Purpose

Converts MAUI Graphics Color to hex string format for UI binding.

#### Test Cases

##### Basic Color Conversion Tests

**Test**: `GetMoodDayInfoListAsync_ShouldConvertColorsToHexCorrectly`

```csharp
[Test]
public async Task GetMoodDayInfoListAsync_ShouldConvertColorsToHexCorrectly()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var visualizationData = new MoodVisualizationData
    {
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 1),
        DailyValues = new[]
        {
            new DailyMoodValue 
            { 
                Date = new DateOnly(2025, 1, 1), 
                HasData = true, 
                Value = 1.0,
                Color = Color.FromRgb(1.0f, 0.5f, 0.0f) // Pure red, half green, no blue
            }
        }
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(visualizationData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetMoodDayInfoListAsync(mockService.Object);
    
    // Assert
    Assert.That(result[0].ColorHex, Is.EqualTo("#FF8000")); // 255, 128, 0 in hex
}
```

### Method: GetValueDescription (Private - Test via GetMoodDayInfoListAsync)

#### Purpose

Converts numeric mood change values to human-readable descriptions.

#### Test Cases

##### Value Description Tests

**Test**: `GetMoodDayInfoListAsync_ShouldGenerateCorrectValueDescriptions`

```csharp
[Test]
[TestCase(3.0, "Significantly improved")]
[TestCase(2.0, "Significantly improved")]
[TestCase(1.5, "Moderately improved")]
[TestCase(1.0, "Moderately improved")]
[TestCase(0.5, "Slightly improved")]
[TestCase(0.0, "No change")]
[TestCase(-0.5, "Slightly declined")]
[TestCase(-1.0, "Moderately declined")]
[TestCase(-1.5, "Moderately declined")]
[TestCase(-2.0, "Moderately declined")]
[TestCase(-3.0, "Significantly declined")]
public async Task GetMoodDayInfoListAsync_ShouldGenerateCorrectValueDescriptions(
    double value, string expectedDescription)
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var visualizationData = new MoodVisualizationData
    {
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 1),
        DailyValues = new[]
        {
            new DailyMoodValue 
            { 
                Date = new DateOnly(2025, 1, 1), 
                HasData = true, 
                Value = value,
                Color = Colors.Blue
            }
        }
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(visualizationData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetMoodDayInfoListAsync(mockService.Object);
    
    // Assert
    Assert.That(result[0].ValueDescription, Is.EqualTo(expectedDescription));
}
```

**Test**: `GetMoodDayInfoListAsync_WithNullValue_ShouldReturnNoData`

```csharp
[Test]
public async Task GetMoodDayInfoListAsync_WithNullValue_ShouldReturnNoData()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var visualizationData = new MoodVisualizationData
    {
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 1),
        DailyValues = new[]
        {
            new DailyMoodValue 
            { 
                Date = new DateOnly(2025, 1, 1), 
                HasData = false, 
                Value = null,
                Color = Colors.LightGray
            }
        }
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(visualizationData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetMoodDayInfoListAsync(mockService.Object);
    
    // Assert
    Assert.That(result[0].ValueDescription, Is.EqualTo("No data"));
}
```

### Method: GetMoodDayInfoListAsync

#### Purpose

Transforms service data into UI-friendly MoodDayInfo objects for data binding.

#### Test Cases

##### Data Transformation Tests

**Test**: `GetMoodDayInfoListAsync_ShouldTransformAllProperties`

```csharp
[Test]
public async Task GetMoodDayInfoListAsync_ShouldTransformAllProperties()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var testDate = new DateOnly(2025, 1, 15); // Wednesday
    var visualizationData = new MoodVisualizationData
    {
        StartDate = testDate,
        EndDate = testDate,
        DailyValues = new[]
        {
            new DailyMoodValue 
            { 
                Date = testDate, 
                HasData = true, 
                Value = 1.5,
                Color = Color.FromRgb(0.0f, 1.0f, 0.0f) // Pure green
            }
        }
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(visualizationData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetMoodDayInfoListAsync(mockService.Object);
    
    // Assert
    var moodDayInfo = result[0];
    Assert.That(moodDayInfo.Date, Is.EqualTo(testDate));
    Assert.That(moodDayInfo.Value, Is.EqualTo(1.5));
    Assert.That(moodDayInfo.HasData, Is.True);
    Assert.That(moodDayInfo.ColorHex, Is.EqualTo("#00FF00"));
    Assert.That(moodDayInfo.DayOfWeek, Is.EqualTo("Wednesday"));
    Assert.That(moodDayInfo.ValueDescription, Is.EqualTo("Moderately improved"));
}
```

**Test**: `GetMoodDayInfoListAsync_WithMultipleDays_ShouldPreserveOrder`

```csharp
[Test]
public async Task GetMoodDayInfoListAsync_WithMultipleDays_ShouldPreserveOrder()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var startDate = new DateOnly(2025, 1, 1);
    var visualizationData = new MoodVisualizationData
    {
        StartDate = startDate,
        EndDate = startDate.AddDays(2),
        DailyValues = new[]
        {
            new DailyMoodValue { Date = startDate, HasData = true, Value = 1.0, Color = Colors.Green },
            new DailyMoodValue { Date = startDate.AddDays(1), HasData = false, Value = null, Color = Colors.LightGray },
            new DailyMoodValue { Date = startDate.AddDays(2), HasData = true, Value = -1.0, Color = Colors.Red }
        }
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(visualizationData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetMoodDayInfoListAsync(mockService.Object);
    
    // Assert
    Assert.That(result, Has.Count.EqualTo(3));
    Assert.That(result[0].Date, Is.EqualTo(startDate));
    Assert.That(result[1].Date, Is.EqualTo(startDate.AddDays(1)));
    Assert.That(result[2].Date, Is.EqualTo(startDate.AddDays(2)));
    
    Assert.That(result[0].HasData, Is.True);
    Assert.That(result[1].HasData, Is.False);
    Assert.That(result[2].HasData, Is.True);
}
```

### Method: GetTwoWeekMoodVisualizationAsync

#### Purpose

Pass-through method that delegates to service for 2-week visualization data.

#### Test Cases

##### Service Delegation Tests

**Test**: `GetTwoWeekMoodVisualizationAsync_ShouldDelegateToService`

```csharp
[Test]
public async Task GetTwoWeekMoodVisualizationAsync_ShouldDelegateToService()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    var expectedData = new MoodVisualizationData
    {
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 14),
        DailyValues = Array.Empty<DailyMoodValue>()
    };
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(expectedData);
    
    // Act
    var result = await MoodVisualizationFormatter.GetTwoWeekMoodVisualizationAsync(mockService.Object);
    
    // Assert
    Assert.That(result, Is.SameAs(expectedData));
    mockService.Verify(s => s.GetTwoWeekVisualizationAsync(), Times.Once);
}
```

**Test**: `GetTwoWeekMoodVisualizationAsync_WithServiceException_ShouldPropagateException`

```csharp
[Test]
public void GetTwoWeekMoodVisualizationAsync_WithServiceException_ShouldPropagateException()
{
    // Arrange
    var mockService = new Mock<MoodDataService>();
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ThrowsAsync(new InvalidOperationException("Service error"));
    
    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(
        () => MoodVisualizationFormatter.GetTwoWeekMoodVisualizationAsync(mockService.Object));
}
```

### Data Structure: MoodDayInfo

#### Purpose

POCO for UI binding of mood visualization information.

#### Test Cases

##### Property Tests

**Test**: `MoodDayInfo_ShouldAllowPropertyAssignment`

```csharp
[Test]
public void MoodDayInfo_ShouldAllowPropertyAssignment()
{
    // Arrange & Act
    var moodDayInfo = new MoodDayInfo
    {
        Date = new DateOnly(2025, 1, 15),
        Value = 2.5,
        HasData = true,
        ColorHex = "#FF0000",
        DayOfWeek = "Monday",
        ValueDescription = "Significantly improved"
    };
    
    // Assert
    Assert.That(moodDayInfo.Date, Is.EqualTo(new DateOnly(2025, 1, 15)));
    Assert.That(moodDayInfo.Value, Is.EqualTo(2.5));
    Assert.That(moodDayInfo.HasData, Is.True);
    Assert.That(moodDayInfo.ColorHex, Is.EqualTo("#FF0000"));
    Assert.That(moodDayInfo.DayOfWeek, Is.EqualTo("Monday"));
    Assert.That(moodDayInfo.ValueDescription, Is.EqualTo("Significantly improved"));
}
```

**Test**: `MoodDayInfo_DefaultValues_ShouldBeAppropriate`

```csharp
[Test]
public void MoodDayInfo_DefaultValues_ShouldBeAppropriate()
{
    // Arrange & Act
    var moodDayInfo = new MoodDayInfo();
    
    // Assert
    Assert.That(moodDayInfo.Date, Is.EqualTo(default(DateOnly)));
    Assert.That(moodDayInfo.Value, Is.Null);
    Assert.That(moodDayInfo.HasData, Is.False);
    Assert.That(moodDayInfo.ColorHex, Is.EqualTo(string.Empty));
    Assert.That(moodDayInfo.DayOfWeek, Is.EqualTo(string.Empty));
    Assert.That(moodDayInfo.ValueDescription, Is.EqualTo(string.Empty));
}
```

## Test Implementation Notes

### Testing Challenges

1. **Concrete Service Dependency**: Mocking concrete MoodDataService requires careful setup
2. **Static Methods**: Cannot use standard dependency injection for testing
3. **Async Testing**: Service-dependent methods require async test patterns
4. **Private Method Testing**: ColorToHex and GetValueDescription tested indirectly

### Recommended Approach

1. **Mock Concrete Service**: Use Moq to mock MoodDataService carefully
2. **Pure Function Testing**: Direct testing for GetVisualizationSummary
3. **Indirect Testing**: Test private methods through public method calls
4. **Data Structure Testing**: Simple POCO property testing

### Test Data Helper Methods

```csharp
private static MoodVisualizationData CreateTestVisualizationData(
    DateOnly startDate, 
    DateOnly endDate, 
    params (DateOnly date, bool hasData, double? value, Color color)[] dailyData)
{
    var dailyValues = dailyData.Select(d => new DailyMoodValue
    {
        Date = d.date,
        HasData = d.hasData,
        Value = d.value,
        Color = d.color
    }).ToArray();

    return new MoodVisualizationData
    {
        StartDate = startDate,
        EndDate = endDate,
        DailyValues = dailyValues
    };
}

private static Mock<MoodDataService> CreateMockServiceWithData(MoodVisualizationData data)
{
    var mockService = new Mock<MoodDataService>();
    mockService.Setup(s => s.GetTwoWeekVisualizationAsync())
               .ReturnsAsync(data);
    return mockService;
}

private static void AssertMoodDayInfo(MoodDayInfo actual, DateOnly expectedDate, 
    double? expectedValue, bool expectedHasData, string expectedColorHex, 
    string expectedDayOfWeek, string expectedDescription)
{
    Assert.That(actual.Date, Is.EqualTo(expectedDate));
    Assert.That(actual.Value, Is.EqualTo(expectedValue));
    Assert.That(actual.HasData, Is.EqualTo(expectedHasData));
    Assert.That(actual.ColorHex, Is.EqualTo(expectedColorHex));
    Assert.That(actual.DayOfWeek, Is.EqualTo(expectedDayOfWeek));
    Assert.That(actual.ValueDescription, Is.EqualTo(expectedDescription));
}
```

### Test Organization

```
MauiApp.Tests/
├── Processors/
│   ├── MoodVisualizationFormatterTests.cs
│   ├── MoodDayInfoTests.cs
│   ├── TestHelpers/
│   │   ├── VisualizationDataHelper.cs
│   │   ├── MockServiceHelper.cs
│   │   └── MoodDayInfoAssertions.cs
```

## Coverage Goals

- **Method Coverage**: 100% - All public static methods and data structure
- **Line Coverage**: 95% - All logic paths and transformations
- **Branch Coverage**: 100% - All value conditions and pattern matches
- **Async Coverage**: 100% - All async delegation patterns

## Implementation Checklist

### Phase 1 - Pure Function Tests

- [ ] **GetVisualizationSummary Tests**: Summary generation with various data scenarios
- [ ] **MoodDayInfo Tests**: POCO property assignment and default values
- [ ] **Edge Case Tests**: Empty data, null values, single day scenarios

### Phase 2 - Service Integration Tests

- [ ] **GetTwoWeekMoodVisualizationAsync Tests**: Service delegation and error propagation
- [ ] **GetMoodDayInfoListAsync Tests**: Data transformation and property mapping
- [ ] **Mock Service Setup**: Proper mocking of concrete MoodDataService

### Phase 3 - Data Transformation Tests

- [ ] **Color Conversion Tests**: ColorToHex accuracy via indirect testing
- [ ] **Value Description Tests**: GetValueDescription patterns via indirect testing
- [ ] **Multi-Day Tests**: Order preservation and data integrity

### Phase 4 - Comprehensive Verification

- [ ] **Async Pattern Tests**: Proper async/await behavior verification
- [ ] **Exception Handling Tests**: Error propagation from service layer
- [ ] **Coverage Analysis**: Verify 95%+ line coverage achieved

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for MoodVisualizationFormatter with service mocking`
- `^f - add pure function tests for GetVisualizationSummary and data transformation logic`
- `^f - add MoodDayInfo POCO tests and indirect testing for private utility methods`
- `^f - add async service delegation tests with error handling verification`

## Risk Assessment

- **Medium Risk**: Concrete service dependency requires careful mocking approach
- **Low Risk**: Pure functions have predictable behavior and clear test cases
- **Low Risk**: Data structure testing is straightforward with POCO patterns
- **Medium Risk**: Async testing patterns require proper setup and verification

## Refactoring Recommendations

### Priority Refactoring (Recommended before Testing)

**Interface Dependency Injection**:
```csharp
// Current: Concrete dependency
public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(MoodDataService moodDataService)

// Recommended: Interface dependency
public static async Task<List<MoodDayInfo>> GetMoodDayInfoListAsync(IMoodDataService moodDataService)
```

**Alternative Architecture**:
Consider converting from static utility to instance-based service with proper dependency injection:

```csharp
public interface IMoodVisualizationFormatter
{
    Task<MoodVisualizationData> GetTwoWeekMoodVisualizationAsync();
    string GetVisualizationSummary(MoodVisualizationData visualizationData);
    Task<List<MoodDayInfo>> GetMoodDayInfoListAsync();
}

public class MoodVisualizationFormatter : IMoodVisualizationFormatter
{
    private readonly IMoodDataService _moodDataService;
    
    public MoodVisualizationFormatter(IMoodDataService moodDataService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
    }
    
    // Implementation with injected dependencies
}
```

This would improve:
- **Testability**: Standard dependency injection patterns
- **Design**: Better separation of concerns
- **Maintainability**: Easier to extend and modify

### Current Testing Strategy

Given the current static utility design, the test plan focuses on:
1. **Mock concrete dependencies** carefully with Moq
2. **Test pure functions** directly for immediate value
3. **Plan for refactoring** to improve long-term maintainability 🤖