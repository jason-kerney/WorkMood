# DataPointComponent Test Plan

## Object Analysis

**File**: `MauiApp/Graphics/EnhancedLineGraphDrawable.cs`  
**Type**: Graphics Component Class  
**Primary Purpose**: Draw data points as colored circles on mood visualization graphs  
**Key Functionality**: Renders filled circles with borders at calculated positions based on mood data values

### Purpose & Responsibilities
The `DataPointComponent` is responsible for drawing individual data points as visual circles on the mood graph. Each circle represents a day's mood value, positioned according to the day (X-axis) and mood level (Y-axis), with color-coding based on the mood data.

### Architecture Role
- **Layer**: Graphics/Presentation Layer
- **Pattern**: Strategy Pattern (implements `IGraphComponent`)
- **MVVM Role**: View-specific rendering component for data visualization
- **Clean Architecture**: UI/Graphics layer component for mood data presentation

### Dependencies Analysis

#### Constructor Dependencies
- **None**: Default parameterless constructor

#### Method Dependencies
- **ICanvas canvas**: Microsoft.Maui.Graphics canvas abstraction for drawing operations
- **RectF bounds**: Drawing bounds rectangle for positioning calculations
- **MoodVisualizationData data**: Mood data containing daily values, colors, and scaling information

#### Static Dependencies
- **Colors.DarkGray**: Hard dependency on static color for circle borders
- **Mathematical calculations**: Complex coordinate calculations for positioning and scaling

#### Platform Dependencies
- **Microsoft.Maui.Graphics**: MAUI graphics abstraction layer
- **ICanvas drawing operations**: FillCircle and DrawCircle methods for rendering

### Public Interface Documentation

#### Methods
**`void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)`**
- **Purpose**: Draws filled colored circles for each data point with borders
- **Parameters**: 
  - `canvas`: Drawing surface for graphics operations
  - `bounds`: Rectangular bounds defining the drawing area
  - `data`: Visualization data containing daily mood values and colors
- **Return Type**: void
- **Side Effects**: Modifies canvas state (fill color, stroke properties, draws circles)
- **Async Behavior**: Synchronous operation

#### Properties
- **None**: No public properties

#### Commands
- **None**: Not applicable for graphics component

#### Events
- **None**: No events raised

## Testability Assessment

**Overall Testability Score: 7/10**

### Strengths
- ✅ **Single Responsibility**: Clear focus on data point visualization
- ✅ **Data-Driven**: Behavior changes based on input data (good for testing variations)
- ✅ **Mathematical Logic**: Coordinate calculations are verifiable
- ✅ **Conditional Rendering**: Only draws points when data exists (testable branches)

### Challenges
- ⚠️ **Complex Calculations**: Multiple coordinate transformations require careful verification
- ⚠️ **Canvas Interaction**: Multiple canvas operations (fill, stroke, draw circles)
- ⚠️ **Hard Color Dependencies**: Static Colors.DarkGray and data.Color dependencies
- ⚠️ **Data Structure Dependency**: Relies on complex MoodVisualizationData structure

### Current Testability Score Justification
Score: **7/10** - Good testability with moderate complexity

**Deductions**:
- **-1 point**: Complex coordinate calculation logic
- **-1 point**: Multiple canvas interactions requiring sophisticated mocking
- **-1 point**: Hard dependencies on color systems

### Hard Dependencies Identified
1. **Static Color Creation**: `Colors.DarkGray` for border color
2. **ICanvas Drawing**: Multiple canvas operations (FillCircle, DrawCircle, color settings)
3. **Data Structure**: Deep dependency on MoodVisualizationData.DailyValues structure

### Required Refactoring
**Minimal refactoring recommended - well-structured component**

#### Option 1: Extract Color Configuration (Optional)
```csharp
public class DataPointComponent : IGraphComponent
{
    private readonly Color _borderColor;
    
    public DataPointComponent(Color? borderColor = null)
    {
        _borderColor = borderColor ?? Colors.DarkGray;
    }
    
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        // ... existing logic ...
        canvas.StrokeColor = _borderColor;
        // ... rest of method ...
    }
}
```

#### Option 2: Keep Current Design (Recommended)
The component is already well-designed for testing:
- Clear separation of concerns
- Deterministic calculations
- Testable through canvas mocking and data variation

**Recommendation**: Keep current design and test through comprehensive data scenarios and canvas verification.

## Test Implementation Strategy

### Test Class Structure
```csharp
[TestFixture]
public class DataPointComponentTests
{
    private DataPointComponent _component;
    private Mock<ICanvas> _mockCanvas;
    private RectF _standardBounds;
    
    [SetUp]
    public void Setup()
    {
        _component = new DataPointComponent();
        _mockCanvas = new Mock<ICanvas>();
        _standardBounds = new RectF(0, 0, 300, 200);
    }
    
    // Test methods here
}
```

### Mock Strategy
- **ICanvas**: Use Moq to mock all canvas operations (FillColor, StrokeColor, FillCircle, DrawCircle)
- **MoodVisualizationData**: Create various test data scenarios with different daily values
- **Color Verification**: Verify both fill colors (from data) and stroke colors (static)
- **Position Verification**: Verify circle positions based on coordinate calculations

### Test Categories
1. **Data Point Rendering Tests**: Verify circles are drawn for valid data points
2. **Positioning Tests**: Verify correct X/Y coordinate calculations
3. **Color Application Tests**: Verify fill and stroke color assignments
4. **Data Filtering Tests**: Verify only valid data points are rendered
5. **Canvas Interaction Tests**: Verify correct sequence of canvas operations
6. **Edge Case Tests**: Boundary conditions and missing data scenarios

## Detailed Test Cases

### Method: Draw

#### Purpose
Draws filled colored circles at calculated positions for each day that has valid mood data, with dark gray borders.

#### Test Cases

##### Happy Path Tests

**Test**: `Draw_WithValidDataPoints_ShouldDrawCirclesAtCorrectPositions`
```csharp
[Test]
public void Draw_WithValidDataPoints_ShouldDrawCirclesAtCorrectPositions()
{
    // Arrange
    var data = CreateDataWithTwoValidPoints();
    var expectedX1 = 20f + (0 * (260f / 13f)); // Day 0 position
    var expectedX2 = 20f + (1 * (260f / 13f)); // Day 1 position
    var expectedY1 = 100f - (2.0f * 40f); // Value 2.0 position
    var expectedY2 = 100f - (-1.0f * 40f); // Value -1.0 position
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(expectedX1, expectedY1, 4f), Times.Once);
    _mockCanvas.Verify(c => c.DrawCircle(expectedX1, expectedY1, 4f), Times.Once);
    _mockCanvas.Verify(c => c.FillCircle(expectedX2, expectedY2, 4f), Times.Once);
    _mockCanvas.Verify(c => c.DrawCircle(expectedX2, expectedY2, 4f), Times.Once);
}
```

**Test**: `Draw_WithValidData_ShouldSetCorrectColors`
```csharp
[Test]
public void Draw_WithValidData_ShouldSetCorrectColors()
{
    // Arrange
    var data = CreateDataWithColoredPoints();
    var expectedFillColor1 = Colors.Green;
    var expectedFillColor2 = Colors.Red;
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.VerifySet(c => c.FillColor = expectedFillColor1, Times.Once);
    _mockCanvas.VerifySet(c => c.FillColor = expectedFillColor2, Times.Once);
    _mockCanvas.VerifySet(c => c.StrokeColor = Colors.DarkGray, Times.AtLeast(1));
    _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.AtLeast(1));
}
```

**Test**: `Draw_WithAllValidDataPoints_ShouldDraw14Circles`
```csharp
[Test]
public void Draw_WithAllValidDataPoints_ShouldDraw14Circles()
{
    // Arrange
    var data = CreateDataWithAll14ValidPoints();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), 4f), Times.Exactly(14));
    _mockCanvas.Verify(c => c.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), 4f), Times.Exactly(14));
}
```

##### Data Filtering Tests

**Test**: `Draw_WithMissingDataPoints_ShouldOnlyDrawValidPoints`
```csharp
[Test]
public void Draw_WithMissingDataPoints_ShouldOnlyDrawValidPoints()
{
    // Arrange
    var data = CreateDataWithSomeMissingPoints(); // 10 valid, 4 missing
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), 4f), Times.Exactly(10));
    _mockCanvas.Verify(c => c.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), 4f), Times.Exactly(10));
}
```

**Test**: `Draw_WithNoValidData_ShouldNotDrawAnyCircles`
```csharp
[Test]
public void Draw_WithNoValidData_ShouldNotDrawAnyCircles()
{
    // Arrange
    var data = CreateDataWithNoValidPoints();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
    _mockCanvas.Verify(c => c.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
}
```

**Test**: `Draw_WithNullValueButHasData_ShouldNotDrawCircle`
```csharp
[Test]
public void Draw_WithNullValueButHasData_ShouldNotDrawCircle()
{
    // Arrange
    var data = CreateDataWithNullValues();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
}
```

##### Mathematical Calculation Tests

**Test**: `Draw_WithDifferentBounds_ShouldCalculatePositionsCorrectly`
```csharp
[Test]
[TestCase(100, 100, 10f, 50f)] // Small bounds
[TestCase(400, 300, 20f, 150f)] // Large bounds
[TestCase(200, 150, 20f, 75f)] // Medium bounds
public void Draw_WithDifferentBounds_ShouldCalculatePositionsCorrectly(
    float width, float height, float expectedMargin, float expectedCenterY)
{
    // Arrange
    var bounds = new RectF(0, 0, width, height);
    var data = CreateDataWithSingleValidPoint(day: 0, value: 0.0); // Center value
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(expectedMargin, expectedCenterY, 4f), Times.Once);
}
```

**Test**: `Draw_WithMaximumValues_ShouldPositionAtTopOfGraph`
```csharp
[Test]
public void Draw_WithMaximumValues_ShouldPositionAtTopOfGraph()
{
    // Arrange
    var data = CreateDataWithMaxValue(5.0); // Max value
    var expectedY = 20f; // Top margin
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), expectedY, 4f), Times.Once);
}
```

**Test**: `Draw_WithMinimumValues_ShouldPositionAtBottomOfGraph`
```csharp
[Test]
public void Draw_WithMinimumValues_ShouldPositionAtBottomOfGraph()
{
    // Arrange
    var data = CreateDataWithMinValue(-5.0); // Min value
    var expectedY = 180f; // Bottom margin position
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), expectedY, 4f), Times.Once);
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
    var data = CreateDataWithValidPoints();
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

**Test**: `Draw_WithVeryLargeValues_ShouldClampPositions`
```csharp
[Test]
public void Draw_WithVeryLargeValues_ShouldClampPositions()
{
    // Arrange
    var data = CreateDataWithExtremeValue(100.0); // Much larger than MaxAbsoluteValue
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, _standardBounds, data));
    
    // Verify positions are still within reasonable bounds
    _mockCanvas.Verify(c => c.FillCircle(
        It.Is<float>(x => x >= 0 && x <= 300),
        It.Is<float>(y => y >= -50 && y <= 250), // Allow some overflow
        4f), Times.Once);
}
```

##### Error Conditions

**Test**: `Draw_WithNullCanvas_ShouldThrowException`
```csharp
[Test]
public void Draw_WithNullCanvas_ShouldThrowException()
{
    // Arrange
    var data = CreateDataWithValidPoints();
    
    // Act & Assert
    Assert.Throws<NullReferenceException>(() => _component.Draw(null, _standardBounds, data));
}
```

**Test**: `Draw_WithNullData_ShouldHandleGracefully`
```csharp
[Test]
public void Draw_WithNullData_ShouldHandleGracefully()
{
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, _standardBounds, null));
    
    // Should not draw any circles
    _mockCanvas.Verify(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), Times.Never);
}
```

**Test**: `Draw_WithNullDailyValues_ShouldHandleGracefully`
```csharp
[Test]
public void Draw_WithNullDailyValues_ShouldHandleGracefully()
{
    // Arrange
    var data = new MoodVisualizationData { DailyValues = null };
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, _standardBounds, data));
}
```

##### Canvas Operation Sequence Tests

**Test**: `Draw_ShouldSetFillColorBeforeFillingCircle`
```csharp
[Test]
public void Draw_ShouldSetFillColorBeforeFillingCircle()
{
    // Arrange
    var data = CreateDataWithSingleValidPoint(day: 0, value: 1.0);
    var sequence = new MockSequence();
    
    _mockCanvas.InSequence(sequence).SetupSet(c => c.FillColor = It.IsAny<Color>());
    _mockCanvas.InSequence(sequence).Setup(c => c.FillCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert handled by sequence setup
    _mockCanvas.VerifyAll();
}
```

**Test**: `Draw_ShouldSetStrokePropertiesBeforeDrawingBorder`
```csharp
[Test]
public void Draw_ShouldSetStrokePropertiesBeforeDrawingBorder()
{
    // Arrange
    var data = CreateDataWithSingleValidPoint(day: 0, value: 1.0);
    var sequence = new MockSequence();
    
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeColor = Colors.DarkGray);
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeSize = 1f);
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawCircle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert handled by sequence setup
    _mockCanvas.VerifyAll();
}
```

## Test Implementation Notes

### Testing Challenges
1. **Complex Coordinate Calculations**: Multiple mathematical transformations requiring precise verification
2. **Data Structure Complexity**: MoodVisualizationData with nested DailyMoodValue array
3. **Color Management**: Both data-driven colors and static border colors
4. **Loop Logic**: 14-day iteration with conditional rendering

### Recommended Approach
1. **Data Builder Pattern**: Create comprehensive test data builders for various scenarios
2. **Calculation Verification**: Focus on mathematical accuracy of positioning
3. **Canvas Mock Verification**: Verify correct sequence and parameters of all canvas calls
4. **Scenario-Based Testing**: Test different combinations of valid/invalid data

### Test Data Helper Methods
```csharp
private static MoodVisualizationData CreateDataWithSingleValidPoint(int day, double value)
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = i == day,
            Value = i == day ? value : null,
            Color = i == day ? Colors.Blue : Colors.Transparent
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateDataWithNoValidPoints()
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = false,
            Value = null,
            Color = Colors.Transparent
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateDataWithAll14ValidPoints()
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = true,
            Value = (i - 7) * 0.5, // Range from -3.5 to 3.0
            Color = i % 2 == 0 ? Colors.Green : Colors.Red
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}
```

### Test Organization
```
MauiApp.Tests/
├── Graphics/
│   ├── DataPointComponentTests.cs
│   ├── TestHelpers/
│   │   ├── MoodVisualizationDataBuilder.cs
│   │   ├── CanvasVerificationExtensions.cs
│   │   └── CoordinateCalculationHelpers.cs
```

## Coverage Goals

- **Method Coverage**: 100% - Single public method with all branches
- **Line Coverage**: 95% - All calculation and rendering logic
- **Branch Coverage**: 90% - HasData and Value null checks
- **Data Scenario Coverage**: 100% - All combinations of valid/invalid data
- **Canvas Interaction**: 100% - All canvas operations verified

## Implementation Checklist

### Phase 1 - Test Infrastructure Setup
- [ ] **Create Test Data Builders**: Comprehensive builders for MoodVisualizationData scenarios
- [ ] **Setup Canvas Mock Extensions**: Common verification patterns for graphics operations
- [ ] **Coordinate Calculation Helpers**: Mathematical verification utilities
- [ ] **Color Verification Utilities**: Helper methods for color comparison

### Phase 2 - Core Functionality Tests
- [ ] **Data Point Rendering Tests**: Verify circles drawn for valid data
- [ ] **Position Calculation Tests**: Mathematical accuracy of X/Y coordinates
- [ ] **Color Application Tests**: Fill and stroke color verification
- [ ] **Data Filtering Tests**: Only valid points rendered

### Phase 3 - Edge Cases & Error Handling
- [ ] **Missing Data Tests**: Various combinations of missing/invalid data
- [ ] **Boundary Condition Tests**: Extreme values and edge bounds
- [ ] **Null Parameter Tests**: Graceful handling of null inputs
- [ ] **Canvas Operation Sequence**: Correct order of graphics operations

### Phase 4 - Integration & Performance
- [ ] **Complex Data Scenario Tests**: Multiple valid/invalid combinations
- [ ] **Performance Verification**: Efficient rendering for 14 data points
- [ ] **Visual Integration Tests**: Manual verification with actual canvas (optional)
- [ ] **Coverage Analysis**: Verify 95%+ line coverage achieved

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for DataPointComponent with 95% coverage`
- `^f - add coordinate calculation verification tests for DataPointComponent`
- `^f - add data filtering and edge case tests for DataPointComponent`
- `^f - add canvas interaction sequence tests for DataPointComponent`

## Risk Assessment

- **Medium Risk**: Complex coordinate calculations require careful verification
- **Medium Risk**: Multiple canvas interactions need comprehensive mocking
- **Low Risk**: Well-defined data structure and clear rendering logic
- **Low Risk**: Single responsibility principle makes testing focused

## Refactoring Recommendations

### Current Design Assessment
The `DataPointComponent` follows good design principles:
- **Single Responsibility**: Only draws data point circles
- **Data-Driven**: Behavior correctly varies with input data
- **Clear Interface**: Simple IGraphComponent contract
- **Separation of Concerns**: Positioning logic separate from rendering

### Potential Improvements (Optional)
1. **Extract Constants**: Circle radius (4f) and stroke size (1f) as configurable
2. **Color Configuration**: Make border color configurable
3. **Validation Logic**: Add explicit data validation before rendering
4. **Coordinate Calculation**: Extract positioning logic to separate method

**Recommendation**: Current design is excellent for testing - keep as-is unless specific requirements emerge.