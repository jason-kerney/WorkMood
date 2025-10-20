# LineGraphService Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Object Analysis

**File**: `MauiApp/Services/LineGraphService.cs`  
**Type**: Service Implementation  
**Primary Purpose**: Orchestrate graph generation for different modes with customizable options  
**Key Functionality**: Graph mode delegation, data transformation coordination, file operations

### Purpose & Responsibilities

The `LineGraphService` serves as a high-level orchestration service for graph generation. It provides:

- Graph mode routing (Impact, Average, RawData)
- Unified API for different graph generation scenarios
- Data transformation coordination with `IGraphDataTransformer`
- Graph rendering delegation to `ILineGraphGenerator`
- File save operations and byte array generation
- Background image support (white background or custom)

### Architecture Role

- **Layer**: Service Layer
- **Pattern**: Facade/Orchestration Service
- **MVVM Role**: Business service used by ViewModels
- **Clean Architecture**: Application service coordinating domain operations

### Dependencies Analysis

#### Constructor Dependencies

**IGraphDataTransformer graphDataTransformer**:
- Purpose: Transform mood entries into graph data based on mode
- Usage: Called for each graph generation with appropriate GraphMode

**ILineGraphGenerator lineGraphGenerator**:
- Purpose: Generate actual graph images from transformed data
- Usage: Called with graph data and rendering options

#### Method Dependencies

**Default Constructor Fallback**:
- Creates `new GraphDataTransformer()` and `new LineGraphGenerator()`
- Used when dependency injection not available

#### Model Dependencies

- **MoodEntry**: Input data for graph generation
- **GraphMode**: Enum for graph type selection (Impact, Average, RawData)
- **DateRangeInfo**: Date range configuration for graphs
- **Color**: Line color specification

### Public Interface Documentation

#### Constructors

**`LineGraphService(IGraphDataTransformer, ILineGraphGenerator)`**

- **Purpose**: Primary constructor with dependency injection
- **Parameters**: Transformer and generator services
- **Best Practice**: Preferred for testability and DI

**`LineGraphService()`**

- **Purpose**: Default constructor with concrete implementations
- **Behavior**: Creates default instances of dependencies
- **Use Case**: When DI container not available

#### Methods

**`Task<byte[]> GenerateGraphAsync(moodEntries, graphMode, dateRange, options...)`**

- **Purpose**: Generate graph as byte array with white background
- **Parameters**: MoodEntry data, GraphMode, rendering options
- **Return**: PNG image as byte array
- **Pattern**: Delegates to mode-specific private methods

**`Task<byte[]> GenerateGraphAsync(moodEntries, graphMode, dateRange, backgroundImagePath, options...)`**

- **Purpose**: Generate graph as byte array with custom background
- **Parameters**: Additional backgroundImagePath parameter
- **Return**: PNG image as byte array
- **Background**: Custom image overlay support

**`Task SaveGraphAsync(moodEntries, graphMode, dateRange, filePath, options...)`**

- **Purpose**: Save graph directly to file with white background
- **Parameters**: Additional filePath for save destination
- **Side Effects**: Creates file on disk
- **Error Handling**: File system exceptions possible

**`Task SaveGraphAsync(moodEntries, graphMode, dateRange, filePath, backgroundImagePath, options...)`**

- **Purpose**: Save graph directly to file with custom background
- **Parameters**: Both filePath and backgroundImagePath
- **Side Effects**: Creates file on disk
- **Background**: Custom image overlay support

#### Private Mode-Specific Methods

**Impact Graph Methods**:
- `GenerateImpactGraphAsync()` - Generate impact mode graph
- `SaveImpactGraphAsync()` - Save impact mode graph

**Average Graph Methods**:
- `GenerateAverageGraphAsync()` - Generate average mode graph
- `SaveAverageGraphAsync()` - Save average mode graph

**Raw Data Graph Methods**:
- `GenerateRawGraphAsync()` - Generate raw data mode graph
- `SaveRawGraphAsync()` - Save raw data mode graph

## Testability Assessment

**Overall Testability Score: 8/10**

### Strengths

- ✅ **Dependency Injection**: Constructor injection enables easy mocking
- ✅ **Interface Abstraction**: Works with abstractions for all dependencies
- ✅ **Pure Orchestration**: Service primarily coordinates other services
- ✅ **Clear Separation**: Data transformation and rendering separated
- ✅ **Async Design**: Proper async/await patterns throughout
- ✅ **Mode Switching**: Clear switch-based routing logic

### Challenges

- ⚠️ **Default Constructor**: Creates concrete dependencies (less testable path)
- ⚠️ **File Operations**: Save methods perform file system operations
- ⚠️ **Complex Parameter Lists**: Many parameters make setup verbose

### Required Refactoring

**None Required** - Good testability with dependency injection

**Enhancement Opportunity**: Consider parameter objects to reduce method complexity, but current design is adequately testable.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class LineGraphServiceTests
{
    private Mock<IGraphDataTransformer> _mockGraphDataTransformer;
    private Mock<ILineGraphGenerator> _mockLineGraphGenerator;
    private LineGraphService _service;
    private List<MoodEntry> _testMoodEntries;
    private DateRangeInfo _testDateRange;
    
    [SetUp]
    public void Setup()
    {
        _mockGraphDataTransformer = new Mock<IGraphDataTransformer>();
        _mockLineGraphGenerator = new Mock<ILineGraphGenerator>();
        _service = new LineGraphService(_mockGraphDataTransformer.Object, _mockLineGraphGenerator.Object);
        
        _testMoodEntries = CreateTestMoodEntries();
        _testDateRange = CreateTestDateRange();
    }
}
```

### Mock Strategy

**Required Mocks**:
- `Mock<IGraphDataTransformer>` - Mock data transformation
- `Mock<ILineGraphGenerator>` - Mock graph generation

**Mock Behaviors**:
- TransformMoodEntries returns test graph data
- GenerateLineGraphAsync returns test byte array
- SaveLineGraphAsync completes successfully

### Test Categories

1. **Constructor Tests**: DI constructor and default constructor behavior
2. **Graph Mode Routing**: Proper delegation to mode-specific methods
3. **Generate Methods**: Byte array generation with various options
4. **Save Methods**: File save operations with various options
5. **Error Handling**: Invalid parameters and dependency failures
6. **Integration Tests**: End-to-end graph generation workflows

## Test Implementation Plan

### Phase 1: Constructor and Basic Setup
- Constructor dependency injection verification
- Default constructor fallback behavior
- Service initialization validation

### Phase 2: Graph Mode Routing
- GenerateGraphAsync mode switching (Impact, Average, RawData)
- SaveGraphAsync mode switching
- Invalid mode error handling

### Phase 3: Method Delegation Verification
- Verify calls to IGraphDataTransformer with correct parameters
- Verify calls to ILineGraphGenerator with correct parameters
- Parameter pass-through validation

### Phase 4: Error Handling and Edge Cases
- Invalid graph mode handling
- Null parameter validation
- File system error scenarios

## Detailed Test Cases

### Constructor Tests

**Test**: `Constructor_WithDependencies_ShouldInitializeCorrectly`

- Create service with mock dependencies
- Verify service initializes without errors
- Verify dependencies are stored correctly

**Test**: `DefaultConstructor_ShouldCreateConcreteImplementations`

- Create service with default constructor
- Verify service can be used for graph generation
- Integration test ensuring concrete types work

### Graph Mode Routing Tests

**Test**: `GenerateGraphAsync_WithImpactMode_ShouldCallCorrectTransformation`

```csharp
[Test]
public async Task GenerateGraphAsync_WithImpactMode_ShouldCallCorrectTransformation()
{
    // Arrange
    var expectedGraphData = new GraphData();
    var expectedResult = new byte[] { 1, 2, 3 };
    
    _mockGraphDataTransformer
        .Setup(x => x.TransformMoodEntries(_testMoodEntries, GraphMode.Impact, _testDateRange))
        .Returns(expectedGraphData);
    
    _mockLineGraphGenerator
        .Setup(x => x.GenerateLineGraphAsync(expectedGraphData, _testDateRange, 
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), 
            It.IsAny<Color>(), It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(expectedResult);
    
    // Act
    var result = await _service.GenerateGraphAsync(_testMoodEntries, GraphMode.Impact, 
        _testDateRange, true, true, true, true, Colors.Blue);
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedResult));
    _mockGraphDataTransformer.Verify(x => x.TransformMoodEntries(_testMoodEntries, GraphMode.Impact, _testDateRange), Times.Once);
    _mockLineGraphGenerator.Verify(x => x.GenerateLineGraphAsync(expectedGraphData, _testDateRange, 
        true, true, true, true, Colors.Blue, 800, 600), Times.Once);
}
```

**Test**: `GenerateGraphAsync_WithAverageMode_ShouldCallCorrectTransformation`

- Similar test structure for Average mode
- Verify GraphMode.Average passed to transformer

**Test**: `GenerateGraphAsync_WithRawDataMode_ShouldCallCorrectTransformation`

- Similar test structure for RawData mode
- Verify GraphMode.RawData passed to transformer

**Test**: `GenerateGraphAsync_WithInvalidMode_ShouldThrowArgumentException`

```csharp
[Test]
public void GenerateGraphAsync_WithInvalidMode_ShouldThrowArgumentException()
{
    // Arrange
    var invalidMode = (GraphMode)999;
    
    // Act & Assert
    var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        await _service.GenerateGraphAsync(_testMoodEntries, invalidMode, _testDateRange, 
            true, true, true, true, Colors.Blue));
    
    Assert.That(ex.Message, Does.Contain("Unsupported graph mode"));
}
```

### Custom Background Tests

**Test**: `GenerateGraphAsync_WithCustomBackground_ShouldPassBackgroundPath`

```csharp
[Test]
public async Task GenerateGraphAsync_WithCustomBackground_ShouldPassBackgroundPath()
{
    // Arrange
    var backgroundPath = "/path/to/background.png";
    var expectedGraphData = new GraphData();
    var expectedResult = new byte[] { 1, 2, 3 };
    
    _mockGraphDataTransformer
        .Setup(x => x.TransformMoodEntries(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<GraphMode>(), It.IsAny<DateRangeInfo>()))
        .Returns(expectedGraphData);
    
    _mockLineGraphGenerator
        .Setup(x => x.GenerateLineGraphAsync(expectedGraphData, _testDateRange, 
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), 
            backgroundPath, It.IsAny<Color>(), It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(expectedResult);
    
    // Act
    var result = await _service.GenerateGraphAsync(_testMoodEntries, GraphMode.Impact, 
        _testDateRange, true, true, true, true, backgroundPath, Colors.Blue);
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedResult));
    _mockLineGraphGenerator.Verify(x => x.GenerateLineGraphAsync(expectedGraphData, _testDateRange, 
        true, true, true, true, backgroundPath, Colors.Blue, 800, 600), Times.Once);
}
```

### Save Method Tests

**Test**: `SaveGraphAsync_WithFilePath_ShouldCallSaveOnGenerator`

```csharp
[Test]
public async Task SaveGraphAsync_WithFilePath_ShouldCallSaveOnGenerator()
{
    // Arrange
    var filePath = "/path/to/save/graph.png";
    var expectedGraphData = new GraphData();
    
    _mockGraphDataTransformer
        .Setup(x => x.TransformMoodEntries(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<GraphMode>(), It.IsAny<DateRangeInfo>()))
        .Returns(expectedGraphData);
    
    // Act
    await _service.SaveGraphAsync(_testMoodEntries, GraphMode.Average, _testDateRange, 
        true, true, true, true, filePath, Colors.Red);
    
    // Assert
    _mockLineGraphGenerator.Verify(x => x.SaveLineGraphAsync(expectedGraphData, _testDateRange, 
        true, true, true, true, filePath, Colors.Red, 800, 600), Times.Once);
}
```

### Parameter Validation Tests

**Test**: `GenerateGraphAsync_WithNullMoodEntries_ShouldHandleGracefully`

- Test behavior with null mood entries
- Verify appropriate error handling or empty result

**Test**: `GenerateGraphAsync_WithNullDateRange_ShouldHandleGracefully`

- Test behavior with null date range
- Verify error handling or default behavior

### Error Propagation Tests

**Test**: `GenerateGraphAsync_WhenTransformerThrows_ShouldPropagateException`

```csharp
[Test]
public void GenerateGraphAsync_WhenTransformerThrows_ShouldPropagateException()
{
    // Arrange
    _mockGraphDataTransformer
        .Setup(x => x.TransformMoodEntries(It.IsAny<IEnumerable<MoodEntry>>(), It.IsAny<GraphMode>(), It.IsAny<DateRangeInfo>()))
        .Throws(new InvalidOperationException("Transform failed"));
    
    // Act & Assert
    var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await _service.GenerateGraphAsync(_testMoodEntries, GraphMode.Impact, _testDateRange, 
            true, true, true, true, Colors.Blue));
    
    Assert.That(ex.Message, Is.EqualTo("Transform failed"));
}
```

**Test**: `GenerateGraphAsync_WhenGeneratorThrows_ShouldPropagateException`

- Similar test for ILineGraphGenerator exceptions
- Verify exception propagation without modification

### Test Helper Methods

```csharp
private List<MoodEntry> CreateTestMoodEntries()
{
    return new List<MoodEntry>
    {
        new MoodEntry(new DateOnly(2025, 1, 1)) { StartOfWork = 5, EndOfWork = 7 },
        new MoodEntry(new DateOnly(2025, 1, 2)) { StartOfWork = 6, EndOfWork = 8 },
        new MoodEntry(new DateOnly(2025, 1, 3)) { StartOfWork = 4, EndOfWork = 6 }
    };
}

private DateRangeInfo CreateTestDateRange()
{
    return new DateRangeInfo 
    { 
        StartDate = new DateOnly(2025, 1, 1),
        EndDate = new DateOnly(2025, 1, 7)
    };
}
```

## Coverage Goals

- **Method Coverage**: 100% - All public methods
- **Line Coverage**: 95% - All orchestration logic and error handling
- **Branch Coverage**: 100% - All graph mode switch cases
- **Integration Coverage**: Key workflows with dependency coordination

## Implementation Checklist

- [ ] **Constructor Tests**: DI and default constructor behavior
- [ ] **Mode Routing Tests**: Graph mode switch logic verification
- [ ] **Generate Method Tests**: Byte array generation with various parameters
- [ ] **Save Method Tests**: File save operations with proper delegation
- [ ] **Background Image Tests**: Custom background parameter handling
- [ ] **Error Handling Tests**: Invalid modes, dependency failures
- [ ] **Parameter Validation**: Null inputs and edge cases
- [ ] **Integration Tests**: End-to-end workflows with mocked dependencies

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for LineGraphService orchestration service`
- `^f - add graph mode routing tests for LineGraphService delegation logic`
- `^f - add dependency coordination tests for LineGraphService with mocked components`
- `^f - add error handling and parameter validation tests for LineGraphService`

## Risk Assessment

- **Low Risk**: Well-structured orchestration service with clear dependencies
- **Low Risk**: Interface-based dependencies enable comprehensive mocking
- **Medium Risk**: Complex parameter lists require careful test setup
- **Low Risk**: Pure coordination logic without complex business rules

## Refactoring Recommendations

**Current Design Assessment**: The `LineGraphService` demonstrates good orchestration service design with proper dependency injection and clear separation of concerns.

**Recommendation**: Current design provides good testability for an orchestration service. Focus on comprehensive test coverage of mode routing logic, dependency coordination, and error handling scenarios. The service effectively delegates complex operations to specialized components while providing a clean unified interface. 🤖