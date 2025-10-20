# BaselineComponent Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Object Analysis

**File**: `MauiApp/Graphics/EnhancedLineGraphDrawable.cs`  
**Type**: Graphics Component Class  
**Primary Purpose**: Draw the baseline (zero line) in mood visualization graphs  
**Key Functionality**: Renders horizontal reference line at center of graph coordinate system

### Purpose & Responsibilities
The `BaselineComponent` is responsible for drawing a single horizontal line that represents the zero point (neutral mood) in mood visualization graphs. This component follows the Single Responsibility Principle by being solely focused on rendering the baseline reference line.

### Architecture Role
- **Layer**: Graphics/Presentation Layer
- **Pattern**: Strategy Pattern (implements `IGraphComponent`)
- **MVVM Role**: View-specific rendering component used by drawable objects
- **Clean Architecture**: UI/Graphics layer component for visualization rendering

### Dependencies Analysis

#### Constructor Dependencies
- **None**: Default parameterless constructor

#### Method Dependencies
- **ICanvas canvas**: Microsoft.Maui.Graphics canvas abstraction for drawing operations
- **RectF bounds**: Drawing bounds rectangle for positioning calculations
- **MoodVisualizationData data**: Data context (not actually used in baseline drawing)

#### Static Dependencies
- **Color.FromRgba()**: Hard dependency on static color creation
- **Mathematical calculations**: Margin and positioning calculations

#### Platform Dependencies
- **Microsoft.Maui.Graphics**: MAUI graphics abstraction layer
- **ICanvas drawing operations**: DrawLine method for rendering

### Public Interface Documentation

#### Methods
**`void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)`**
- **Purpose**: Draws the baseline zero line across the graph
- **Parameters**: 
  - `canvas`: Drawing surface for graphics operations
  - `bounds`: Rectangular bounds defining the drawing area
  - `data`: Visualization data context (unused in this component)
- **Return Type**: void
- **Side Effects**: Modifies canvas state (stroke color, stroke size, draws line)
- **Async Behavior**: Synchronous operation

#### Properties
- **None**: No public properties

#### Commands
- **None**: Not applicable for graphics component

#### Events
- **None**: No events raised

## Testability Assessment

**Overall Testability Score: 8/10**

### Strengths
- ✅ **Single Responsibility**: Clear, focused purpose makes testing straightforward
- ✅ **Deterministic Output**: Given same inputs, produces same visual result
- ✅ **No External Dependencies**: No file I/O, network calls, or complex dependencies
- ✅ **Pure Method Logic**: Mathematical calculations are easily verifiable
- ✅ **Interface Implementation**: Testable through interface contract

### Challenges
- ⚠️ **Canvas Interaction**: Direct ICanvas manipulation requires mocking or verification strategy
- ⚠️ **Hard Color Dependency**: Static Color.FromRgba call not easily mockable
- ⚠️ **Visual Output Verification**: Difficult to verify actual line drawing without visual testing

### Current Testability Score Justification
Score: **8/10** - Excellent testability with minor graphics-specific challenges

**Deductions**:
- **-1 point**: Canvas interaction requires sophisticated mocking
- **-1 point**: Static color creation dependency

### Hard Dependencies Identified
1. **Static Color Creation**: `Color.FromRgba(200, 200, 200, 255)` - hard-coded color creation
2. **ICanvas Drawing**: Direct canvas manipulation for line drawing

### Required Refactoring
**Minimal refactoring needed - component is already well-designed**

#### Option 1: Extract Color Factory (Recommended)
```csharp
public class BaselineComponent : IGraphComponent
{
    private readonly IColorFactory _colorFactory;
    
    public BaselineComponent(IColorFactory colorFactory = null)
    {
        _colorFactory = colorFactory ?? new DefaultColorFactory();
    }
    
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        // ... existing logic ...
        canvas.StrokeColor = _colorFactory.CreateBaselineColor();
        // ... rest of method ...
    }
}
```

#### Option 2: Keep Simple (Current Design)
The current design is already highly testable. We can test through:
- Canvas interaction verification via mocking
- Calculation verification through parameter inspection
- Integration testing with actual canvas implementations

**Recommendation**: Keep current simple design and test through canvas mocking and calculation verification.

## Test Implementation Strategy

### Test Class Structure
```csharp
[TestFixture]
public class BaselineComponentTests
{
    private BaselineComponent _component;
    private Mock<ICanvas> _mockCanvas;
    private MoodVisualizationData _testData;
    private RectF _testBounds;
    
    [SetUp]
    public void Setup()
    {
        _component = new BaselineComponent();
        _mockCanvas = new Mock<ICanvas>();
        _testData = CreateTestVisualizationData();
        _testBounds = new RectF(0, 0, 300, 200);
    }
    
    // Test methods here
}
```

### Mock Strategy
- **ICanvas**: Use Moq to mock all canvas operations
- **MoodVisualizationData**: Create test data objects with known values
- **RectF**: Use actual struct values (value type, no mocking needed)
- **Color**: Verify expected color values are set on canvas

### Test Categories
1. **Canvas Interaction Tests**: Verify correct canvas method calls
2. **Calculation Tests**: Verify positioning and sizing calculations  
3. **Color Application Tests**: Verify correct stroke color and size settings
4. **Boundary Condition Tests**: Edge cases with different bound sizes
5. **Data Independence Tests**: Verify component doesn't depend on data content

## Detailed Test Cases

### Method: Draw

#### Purpose
Draws a horizontal baseline (zero line) across the center of the graph area with proper margins.

#### Test Cases

##### Happy Path Tests

**Test**: `Draw_WithValidInputs_ShouldSetCorrectStrokeProperties`
```csharp
[Test]
public void Draw_WithValidInputs_ShouldSetCorrectStrokeProperties()
{
    // Arrange
    var bounds = new RectF(0, 0, 300, 200);
    var data = CreateTestVisualizationData();
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.VerifySet(c => c.StrokeColor = Color.FromRgba(200, 200, 200, 255), Times.Once);
    _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.Once);
}
```

**Test**: `Draw_WithValidBounds_ShouldDrawLineAtCorrectPosition`
```csharp
[Test]
public void Draw_WithValidBounds_ShouldDrawLineAtCorrectPosition()
{
    // Arrange
    var bounds = new RectF(0, 0, 300, 200);
    var data = CreateTestVisualizationData();
    var expectedCenterY = 20f + (160f / 2f); // margin + (graphHeight / 2)
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.DrawLine(20f, expectedCenterY, 280f, expectedCenterY), Times.Once);
}
```

**Test**: `Draw_WithDifferentBounds_ShouldCalculatePositionCorrectly`
```csharp
[Test]
[TestCase(100, 100, 10f, 50f, 90f)] // Small square
[TestCase(400, 300, 20f, 150f, 380f)] // Large rectangle  
[TestCase(200, 150, 20f, 75f, 180f)] // Medium rectangle
public void Draw_WithDifferentBounds_ShouldCalculatePositionCorrectly(
    float width, float height, float expectedStartX, float expectedY, float expectedEndX)
{
    // Arrange
    var bounds = new RectF(0, 0, width, height);
    var data = CreateTestVisualizationData();
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.DrawLine(expectedStartX, expectedY, expectedEndX, expectedY), Times.Once);
}
```

##### Edge Cases

**Test**: `Draw_WithZeroBounds_ShouldHandleGracefully`
```csharp
[Test]
public void Draw_WithZeroBounds_ShouldHandleGracefully()
{
    // Arrange
    var bounds = new RectF(0, 0, 0, 0);
    var data = CreateTestVisualizationData();
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

**Test**: `Draw_WithNegativeBounds_ShouldHandleGracefully`
```csharp
[Test]
public void Draw_WithNegativeBounds_ShouldHandleGracefully()
{
    // Arrange
    var bounds = new RectF(0, 0, -100, -100);
    var data = CreateTestVisualizationData();
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

**Test**: `Draw_WithVerySmallBounds_ShouldNotCrash`
```csharp
[Test]
public void Draw_WithVerySmallBounds_ShouldNotCrash()
{
    // Arrange
    var bounds = new RectF(0, 0, 1, 1);
    var data = CreateTestVisualizationData();
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

##### Error Conditions

**Test**: `Draw_WithNullCanvas_ShouldThrowArgumentNullException`
```csharp
[Test]
public void Draw_WithNullCanvas_ShouldThrowArgumentNullException()
{
    // Arrange
    var bounds = new RectF(0, 0, 300, 200);
    var data = CreateTestVisualizationData();
    
    // Act & Assert
    var ex = Assert.Throws<NullReferenceException>(() => _component.Draw(null, bounds, data));
}
```

**Test**: `Draw_WithNullData_ShouldStillDrawBaseline`
```csharp
[Test]
public void Draw_WithNullData_ShouldStillDrawBaseline()
{
    // Arrange
    var bounds = new RectF(0, 0, 300, 200);
    var expectedCenterY = 20f + (160f / 2f);
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, null);
    
    // Assert - Baseline should still be drawn regardless of data
    _mockCanvas.Verify(c => c.DrawLine(20f, expectedCenterY, 280f, expectedCenterY), Times.Once);
}
```

##### Canvas Interaction Verification

**Test**: `Draw_ShouldCallCanvasMethodsInCorrectOrder`
```csharp
[Test]
public void Draw_ShouldCallCanvasMethodsInCorrectOrder()
{
    // Arrange
    var bounds = new RectF(0, 0, 300, 200);
    var data = CreateTestVisualizationData();
    var sequence = new MockSequence();
    
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeColor = It.IsAny<Color>());
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeSize = It.IsAny<float>());
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert - Verify sequence is handled by Moq sequence setup
    _mockCanvas.VerifyAll();
}
```

##### Mathematical Calculations

**Test**: `Draw_ShouldCalculateMarginCorrectly`
```csharp
[Test]
public void Draw_ShouldCalculateMarginCorrectly()
{
    // Arrange - Test that 20f margin is consistently applied
    var bounds = new RectF(0, 0, 100, 100);
    var data = CreateTestVisualizationData();
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    // Start X should be margin (20f), End X should be width - margin (80f)
    _mockCanvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 80f, It.IsAny<float>()), Times.Once);
}
```

**Test**: `Draw_ShouldCalculateCenterYCorrectly`
```csharp
[Test]
public void Draw_ShouldCalculateCenterYCorrectly()
{
    // Arrange
    var bounds = new RectF(0, 0, 200, 160);
    var data = CreateTestVisualizationData();
    // Expected: margin (20) + (graphHeight (120) / 2) = 80
    var expectedCenterY = 80f;
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), expectedCenterY, It.IsAny<float>(), expectedCenterY), Times.Once);
}
```

## Test Implementation Notes

### Testing Challenges
1. **Visual Verification**: Cannot easily verify the actual visual output without rendering
2. **Canvas Mocking**: Need comprehensive mocking of ICanvas interface
3. **Color Equality**: Verifying Color.FromRgba values requires exact matching

### Recommended Approach
1. **Mock-Based Testing**: Primary strategy using Moq for ICanvas
2. **Calculation Verification**: Focus on mathematical correctness of positioning
3. **Behavior Verification**: Ensure correct sequence and parameters of canvas calls
4. **Edge Case Coverage**: Test boundary conditions and error scenarios

### Test Data Helper Methods
```csharp
private static MoodVisualizationData CreateTestVisualizationData()
{
    return new MoodVisualizationData
    {
        DailyValues = new DailyMoodValue[14],
        StartDate = new DateOnly(2023, 1, 1),
        EndDate = new DateOnly(2023, 1, 14),
        MaxAbsoluteValue = 5.0
    };
}

private static RectF CreateStandardBounds()
{
    return new RectF(0, 0, 300, 200);
}
```

### Test Organization
```
MauiApp.Tests/
├── Graphics/
│   ├── BaselineComponentTests.cs
│   ├── TestHelpers/
│   │   ├── GraphicsTestDataBuilder.cs
│   │   └── CanvasMockExtensions.cs
```

## Coverage Goals

- **Method Coverage**: 100% - Single public method
- **Line Coverage**: 95% - All calculation and drawing logic
- **Branch Coverage**: 90% - Edge cases and null handling
- **Canvas Interaction**: 100% - All canvas method calls verified

## Implementation Checklist

### Phase 1 - Test Infrastructure Setup
- [ ] **Create Test Project Reference**: Ensure Graphics namespace is accessible
- [ ] **Install Moq Package**: For canvas mocking capabilities
- [ ] **Create Test Data Builders**: Helper methods for test data creation
- [ ] **Setup Canvas Mock Extensions**: Common canvas verification patterns

### Phase 2 - Core Test Implementation
- [ ] **Canvas Interaction Tests**: Verify stroke properties and DrawLine calls
- [ ] **Calculation Tests**: Mathematical positioning and sizing verification
- [ ] **Happy Path Tests**: Standard usage scenarios with valid inputs
- [ ] **Data Independence Tests**: Verify component works regardless of data content

### Phase 3 - Edge Case & Error Testing
- [ ] **Boundary Condition Tests**: Zero, negative, and very small bounds
- [ ] **Null Parameter Tests**: Null canvas and data handling
- [ ] **Sequence Verification**: Correct order of canvas operations
- [ ] **Integration Tests**: With actual canvas implementations (if available)

### Phase 4 - Coverage Verification
- [ ] **Run Coverage Analysis**: Verify 95%+ line coverage achieved
- [ ] **Review Uncovered Lines**: Identify any missed scenarios
- [ ] **Performance Testing**: Ensure drawing operations are efficient
- [ ] **Visual Integration Test**: Manual verification of actual rendering (optional)

## Arlo's Commit Strategy

Given the high testability of this component, refactoring may not be needed:

- `^f - add comprehensive test suite for BaselineComponent with 95% coverage`
- `^r - extract color factory for BaselineComponent testability` (if color abstraction needed)
- `^f - add edge case tests for BaselineComponent boundary conditions`
- `^f - add canvas interaction verification tests for BaselineComponent`

## Risk Assessment

- **Low Risk**: Simple, focused component with deterministic behavior
- **Graphics Dependency**: Medium risk due to canvas interaction requirements
- **Mathematical Accuracy**: Low risk due to straightforward calculations
- **Maintenance**: Low risk due to single responsibility and clear interface

## Refactoring Recommendations

### Current Design Assessment
The `BaselineComponent` is already well-designed following SOLID principles:
- **Single Responsibility**: Only draws baseline
- **Open/Closed**: Extensible through interface
- **Interface Segregation**: Minimal interface contract
- **Dependency Inversion**: Depends on abstractions (ICanvas)

### Potential Improvements (Optional)
1. **Color Configuration**: Extract hard-coded color to configuration
2. **Margin Configuration**: Make margin value configurable
3. **Line Style Options**: Support different line styles (solid, dashed, etc.)

**Recommendation**: Keep current simple design unless additional requirements emerge.