# GridComponent Test Plan

## Object Analysis

**File**: `MauiApp/Graphics/EnhancedLineGraphDrawable.cs`  
**Type**: Graphics Component Class  
**Primary Purpose**: Draw grid lines for mood visualization graphs to provide visual reference structure  
**Key Functionality**: Renders vertical lines for days and horizontal lines for mood value levels

### Purpose & Responsibilities
The `GridComponent` is responsible for drawing a coordinate grid system on mood visualization graphs. It creates vertical lines representing each day (14 days total) and horizontal lines representing mood value levels based on the data range, providing visual reference points for reading the graph.

### Architecture Role
- **Layer**: Graphics/Presentation Layer
- **Pattern**: Strategy Pattern (implements `IGraphComponent`)
- **MVVM Role**: View-specific rendering component for graph infrastructure
- **Clean Architecture**: UI/Graphics layer component for visual reference structure

### Dependencies Analysis

#### Constructor Dependencies
- **None**: Default parameterless constructor

#### Method Dependencies
- **ICanvas canvas**: Microsoft.Maui.Graphics canvas abstraction for drawing operations
- **RectF bounds**: Drawing bounds rectangle for positioning calculations
- **MoodVisualizationData data**: Data context for determining horizontal grid line spacing

#### Static Dependencies
- **Colors.LightGray**: Hard dependency on static color for grid lines
- **Mathematical calculations**: Complex coordinate calculations for grid positioning
- **Math.Max, Math.Ceiling, Math.Abs**: Static mathematical operations

#### Platform Dependencies
- **Microsoft.Maui.Graphics**: MAUI graphics abstraction layer
- **ICanvas drawing operations**: DrawLine method for rendering grid lines

### Public Interface Documentation

#### Methods
**`void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)`**
- **Purpose**: Draws vertical day lines and horizontal value level lines for graph grid
- **Parameters**: 
  - `canvas`: Drawing surface for graphics operations
  - `bounds`: Rectangular bounds defining the drawing area
  - `data`: Visualization data containing MaxAbsoluteValue for horizontal grid calculation
- **Return Type**: void
- **Side Effects**: Modifies canvas state (stroke color, stroke size, draws multiple lines)
- **Async Behavior**: Synchronous operation

#### Properties
- **None**: No public properties

#### Commands
- **None**: Not applicable for graphics component

#### Events
- **None**: No events raised

## Testability Assessment

**Overall Testability Score: 6/10**

### Strengths
- ✅ **Single Responsibility**: Clear focus on grid line rendering
- ✅ **Mathematical Logic**: Grid calculations are deterministic and verifiable
- ✅ **Data-Driven**: Horizontal grid adapts to data range
- ✅ **Loop Logic**: Repetitive drawing operations are easy to test

### Challenges
- ⚠️ **Complex Grid Calculations**: Multiple coordinate transformations and scaling logic
- ⚠️ **Multiple Canvas Operations**: 15+ line drawing operations requiring verification
- ⚠️ **Hard Dependencies**: Static color and Math operations
- ⚠️ **Conditional Logic**: Dynamic horizontal grid intervals based on data range

### Current Testability Score Justification
Score: **6/10** - Moderate testability with mathematical complexity

**Deductions**:
- **-2 points**: Complex grid calculation logic with multiple formulas
- **-1 point**: Multiple canvas interactions requiring extensive mocking
- **-1 point**: Hard dependencies on static methods and colors

### Hard Dependencies Identified
1. **Static Color Creation**: `Colors.LightGray` for grid line color
2. **Static Math Operations**: `Math.Max`, `Math.Ceiling`, `Math.Abs` for calculations
3. **ICanvas Drawing**: Multiple DrawLine operations for grid rendering
4. **Magic Numbers**: Hard-coded values (margin: 20f, pointSpacing calculation, gridInterval logic)

### Required Refactoring
**Moderate refactoring recommended for improved testability**

#### Option 1: Extract Configuration Object (Recommended)
```csharp
public class GridConfiguration
{
    public float Margin { get; set; } = 20f;
    public Color GridColor { get; set; } = Colors.LightGray;
    public float StrokeSize { get; set; } = 0.5f;
    public int DayCount { get; set; } = 14;
}

public class GridComponent : IGraphComponent
{
    private readonly GridConfiguration _config;
    
    public GridComponent(GridConfiguration config = null)
    {
        _config = config ?? new GridConfiguration();
    }
    
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        canvas.StrokeColor = _config.GridColor;
        canvas.StrokeSize = _config.StrokeSize;
        // ... rest with configurable values
    }
}
```

#### Option 2: Extract Calculation Methods (Alternative)
```csharp
public class GridComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        SetupCanvasForGrid(canvas);
        DrawVerticalGridLines(canvas, bounds);
        DrawHorizontalGridLines(canvas, bounds, data);
    }
    
    protected virtual void SetupCanvasForGrid(ICanvas canvas) { }
    protected virtual void DrawVerticalGridLines(ICanvas canvas, RectF bounds) { }
    protected virtual void DrawHorizontalGridLines(ICanvas canvas, RectF bounds, MoodVisualizationData data) { }
}
```

**Recommendation**: Extract configuration object to make constants testable while keeping method simple.

## Test Implementation Strategy

### Test Class Structure
```csharp
[TestFixture]
public class GridComponentTests
{
    private GridComponent _component;
    private Mock<ICanvas> _mockCanvas;
    private RectF _standardBounds;
    
    [SetUp]
    public void Setup()
    {
        _component = new GridComponent();
        _mockCanvas = new Mock<ICanvas>();
        _standardBounds = new RectF(0, 0, 300, 200);
    }
    
    // Test methods here
}
```

### Mock Strategy
- **ICanvas**: Use Moq to mock stroke properties and DrawLine operations
- **MoodVisualizationData**: Create test data with various MaxAbsoluteValue scenarios
- **Line Verification**: Verify correct number and positioning of grid lines
- **Calculation Verification**: Test mathematical formulas for grid positioning

### Test Categories
1. **Vertical Grid Lines Tests**: Verify 15 vertical lines (days 0-14)
2. **Horizontal Grid Lines Tests**: Verify adaptive horizontal lines based on data range
3. **Canvas Property Tests**: Verify stroke color and size settings
4. **Mathematical Calculation Tests**: Verify grid positioning formulas
5. **Data Range Adaptation Tests**: Different MaxAbsoluteValue scenarios
6. **Edge Case Tests**: Boundary conditions and extreme values

## Detailed Test Cases

### Method: Draw

#### Purpose
Draws a complete grid system with vertical lines for each day and horizontal lines for mood value levels.

#### Test Cases

##### Vertical Grid Lines Tests

**Test**: `Draw_WithStandardBounds_ShouldDraw15VerticalLines`
```csharp
[Test]
public void Draw_WithStandardBounds_ShouldDraw15VerticalLines()
{
    // Arrange
    var data = CreateStandardVisualizationData();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw lines for days 0-14 (15 total)
    _mockCanvas.Verify(c => c.DrawLine(
        It.IsAny<float>(), 20f, It.IsAny<float>(), 180f), Times.Exactly(15));
}
```

**Test**: `Draw_ShouldPositionVerticalLinesCorrectly`
```csharp
[Test]
public void Draw_ShouldPositionVerticalLinesCorrectly()
{
    // Arrange
    var data = CreateStandardVisualizationData();
    var expectedSpacing = 260f / 13f; // (width - 2*margin) / 13 gaps
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Check first, middle, and last vertical lines
    _mockCanvas.Verify(c => c.DrawLine(20f, 20f, 20f, 180f), Times.Once); // First line
    _mockCanvas.Verify(c => c.DrawLine(20f + (7 * expectedSpacing), 20f, 20f + (7 * expectedSpacing), 180f), Times.Once); // Middle
    _mockCanvas.Verify(c => c.DrawLine(280f, 20f, 280f, 180f), Times.Once); // Last line
}
```

##### Horizontal Grid Lines Tests

**Test**: `Draw_WithStandardMaxValue_ShouldDrawCorrectHorizontalLines`
```csharp
[Test]
public void Draw_WithStandardMaxValue_ShouldDrawCorrectHorizontalLines()
{
    // Arrange
    var data = CreateVisualizationDataWithMaxValue(3.0);
    var centerY = 100f; // Center of standard bounds
    var scaleFactor = 160f / (2.0 * 3.0); // graphHeight / (2 * maxAbsValue)
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should have horizontal lines at -3, -2, -1, 1, 2, 3 (skip center)
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY + (1 * scaleFactor), 280f, centerY + (1 * scaleFactor)), Times.Once); // -1
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY - (1 * scaleFactor), 280f, centerY - (1 * scaleFactor)), Times.Once); // +1
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY + (3 * scaleFactor), 280f, centerY + (3 * scaleFactor)), Times.Once); // -3
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY - (3 * scaleFactor), 280f, centerY - (3 * scaleFactor)), Times.Once); // +3
}
```

**Test**: `Draw_ShouldSkipCenterLineInHorizontalGrid`
```csharp
[Test]
public void Draw_ShouldSkipCenterLineInHorizontalGrid()
{
    // Arrange
    var data = CreateVisualizationDataWithMaxValue(2.0);
    var centerY = 100f;
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should NOT draw line at center Y (zero line)
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY, 280f, centerY), Times.Never);
}
```

##### Canvas Property Tests

**Test**: `Draw_ShouldSetCorrectStrokeProperties`
```csharp
[Test]
public void Draw_ShouldSetCorrectStrokeProperties()
{
    // Arrange
    var data = CreateStandardVisualizationData();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.VerifySet(c => c.StrokeColor = Colors.LightGray, Times.Once);
    _mockCanvas.VerifySet(c => c.StrokeSize = 0.5f, Times.Once);
}
```

##### Mathematical Calculation Tests

**Test**: `Draw_WithDifferentBounds_ShouldCalculateSpacingCorrectly`
```csharp
[Test]
[TestCase(260, 160, 20f, 20f)] // Standard case
[TestCase(400, 300, 30f, 30f)] // Larger bounds
[TestCase(130, 100, 10f, 10f)] // Smaller bounds
public void Draw_WithDifferentBounds_ShouldCalculateSpacingCorrectly(
    float width, float height, float expectedMarginX, float expectedMarginY)
{
    // Arrange
    var bounds = new RectF(0, 0, width + (2 * expectedMarginX), height + (2 * expectedMarginY));
    var data = CreateStandardVisualizationData();
    var expectedSpacing = width / 13f;
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert - Check first and last vertical lines
    _mockCanvas.Verify(c => c.DrawLine(expectedMarginX, expectedMarginY, expectedMarginX, height + expectedMarginY), Times.Once);
    _mockCanvas.Verify(c => c.DrawLine(expectedMarginX + width, expectedMarginY, expectedMarginX + width, height + expectedMarginY), Times.Once);
}
```

**Test**: `Draw_WithSmallMaxValue_ShouldUseCorrectGridInterval`
```csharp
[Test]
public void Draw_WithSmallMaxValue_ShouldUseCorrectGridInterval()
{
    // Arrange - MaxValue <= 3 should use interval of 1
    var data = CreateVisualizationDataWithMaxValue(2.0);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw lines at -2, -1, 1, 2 (interval = 1)
    var expectedLineCount = 4; // -2, -1, 1, 2 (skipping center)
    _mockCanvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 280f, It.IsAny<float>()), 
        Times.Exactly(15 + expectedLineCount)); // 15 vertical + 4 horizontal
}
```

**Test**: `Draw_WithLargeMaxValue_ShouldUseCalculatedGridInterval`
```csharp
[Test]
public void Draw_WithLargeMaxValue_ShouldUseCalculatedGridInterval()
{
    // Arrange - MaxValue > 3 should use Ceiling(maxValue / 3)
    var data = CreateVisualizationDataWithMaxValue(10.0);
    var expectedInterval = Math.Ceiling(10.0 / 3); // Should be 4
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw lines at intervals of 4: -8, -4, 4, 8
    var expectedHorizontalLines = 4;
    _mockCanvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 280f, It.IsAny<float>()), 
        Times.Exactly(15 + expectedHorizontalLines)); // 15 vertical + 4 horizontal
}
```

##### Data Range Adaptation Tests

**Test**: `Draw_WithMaxValueOfOne_ShouldAdaptGridCorrectly`
```csharp
[Test]
public void Draw_WithMaxValueOfOne_ShouldAdaptGridCorrectly()
{
    // Arrange
    var data = CreateVisualizationDataWithMaxValue(1.0);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should use maxValue of 1.0 (Math.Max(1.0, 1.0))
    var centerY = 100f;
    var scaleFactor = 160f / (2.0 * 1.0);
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY + scaleFactor, 280f, centerY + scaleFactor), Times.Once); // -1
    _mockCanvas.Verify(c => c.DrawLine(20f, centerY - scaleFactor, 280f, centerY - scaleFactor), Times.Once); // +1
}
```

**Test**: `Draw_WithZeroMaxValue_ShouldUseMinimumValue`
```csharp
[Test]
public void Draw_WithZeroMaxValue_ShouldUseMinimumValue()
{
    // Arrange
    var data = CreateVisualizationDataWithMaxValue(0.0);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should use Math.Max(1.0, 0.0) = 1.0
    var expectedHorizontalLines = 2; // -1 and +1
    _mockCanvas.Verify(c => c.DrawLine(20f, It.IsAny<float>(), 280f, It.IsAny<float>()), 
        Times.Exactly(15 + expectedHorizontalLines));
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
    var data = CreateStandardVisualizationData();
    
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
    var data = CreateStandardVisualizationData();
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

**Test**: `Draw_WithVeryLargeMaxValue_ShouldNotCrash`
```csharp
[Test]
public void Draw_WithVeryLargeMaxValue_ShouldNotCrash()
{
    // Arrange
    var data = CreateVisualizationDataWithMaxValue(1000.0);
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, _standardBounds, data));
}
```

##### Error Conditions

**Test**: `Draw_WithNullCanvas_ShouldThrowException`
```csharp
[Test]
public void Draw_WithNullCanvas_ShouldThrowException()
{
    // Arrange
    var data = CreateStandardVisualizationData();
    
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
    
    // Should still draw vertical lines, but may have issues with horizontal
    _mockCanvas.Verify(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()), 
        Times.AtLeast(15)); // At least vertical lines
}
```

##### Canvas Operation Sequence Tests

**Test**: `Draw_ShouldSetPropertiesBeforeDrawing`
```csharp
[Test]
public void Draw_ShouldSetPropertiesBeforeDrawing()
{
    // Arrange
    var data = CreateStandardVisualizationData();
    var sequence = new MockSequence();
    
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeColor = Colors.LightGray);
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeSize = 0.5f);
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawLine(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert handled by sequence setup
    _mockCanvas.VerifyAll();
}
```

## Test Implementation Notes

### Testing Challenges
1. **Complex Grid Calculations**: Multiple mathematical formulas for positioning
2. **Dynamic Line Count**: Horizontal lines vary based on data range
3. **Loop Verification**: Testing repetitive line drawing operations
4. **Mathematical Precision**: Floating-point calculations may have precision issues

### Recommended Approach
1. **Formula Verification**: Focus on mathematical accuracy of grid calculations
2. **Count Verification**: Verify correct number of lines drawn
3. **Position Verification**: Test key positioning calculations
4. **Range Testing**: Test various MaxAbsoluteValue scenarios

### Test Data Helper Methods
```csharp
private static MoodVisualizationData CreateStandardVisualizationData()
{
    return new MoodVisualizationData
    {
        DailyValues = new DailyMoodValue[14],
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateVisualizationDataWithMaxValue(double maxValue)
{
    return new MoodVisualizationData
    {
        DailyValues = new DailyMoodValue[14],
        MaxAbsoluteValue = maxValue
    };
}

private static void VerifyVerticalLineCount(Mock<ICanvas> mockCanvas, int expectedCount)
{
    // Helper to verify vertical lines by checking Y coordinates span full height
    mockCanvas.Verify(c => c.DrawLine(
        It.IsAny<float>(), 20f, It.IsAny<float>(), 180f), Times.Exactly(expectedCount));
}
```

### Test Organization
```
MauiApp.Tests/
├── Graphics/
│   ├── GridComponentTests.cs
│   ├── TestHelpers/
│   │   ├── GridCalculationHelpers.cs
│   │   ├── CanvasLineVerification.cs
│   │   └── MathematicalTestHelpers.cs
```

## Coverage Goals

- **Method Coverage**: 100% - Single public method with all branches
- **Line Coverage**: 90% - All grid calculation and drawing logic
- **Branch Coverage**: 85% - Conditional grid interval calculations
- **Mathematical Coverage**: 100% - All coordinate transformation formulas
- **Loop Coverage**: 100% - Both vertical and horizontal line loops

## Implementation Checklist

### Phase 1 - Test Infrastructure Setup
- [ ] **Create Mathematical Helpers**: Grid calculation verification utilities
- [ ] **Setup Canvas Line Verification**: Pattern matching for line drawing verification
- [ ] **Create Data Range Builders**: Various MaxAbsoluteValue scenarios
- [ ] **Floating Point Comparison**: Utilities for precision-tolerant assertions

### Phase 2 - Grid Logic Tests
- [ ] **Vertical Grid Tests**: 15 vertical lines with correct positioning
- [ ] **Horizontal Grid Tests**: Adaptive horizontal lines based on data range
- [ ] **Canvas Property Tests**: Stroke color and size verification
- [ ] **Mathematical Formula Tests**: Grid spacing and positioning calculations

### Phase 3 - Data Range & Edge Cases
- [ ] **Range Adaptation Tests**: Various MaxAbsoluteValue scenarios
- [ ] **Grid Interval Tests**: Small vs large value interval calculations
- [ ] **Boundary Condition Tests**: Zero, negative, extreme bounds
- [ ] **Error Handling Tests**: Null parameters and invalid data

### Phase 4 - Integration & Performance
- [ ] **Complex Scenario Tests**: Multiple combinations of bounds and data
- [ ] **Performance Verification**: Efficient grid rendering
- [ ] **Canvas Operation Sequence**: Correct order of drawing operations
- [ ] **Coverage Analysis**: Verify 90%+ line coverage achieved

## Arlo's Commit Strategy

- `^r - extract grid configuration object for GridComponent testability`
- `^f - add comprehensive test suite for GridComponent with 90% coverage`
- `^f - add mathematical formula verification tests for GridComponent`
- `^f - add data range adaptation tests for GridComponent grid intervals`

## Risk Assessment

- **Medium Risk**: Complex mathematical calculations require careful verification
- **Medium Risk**: Multiple canvas operations need comprehensive verification
- **Low Risk**: Single responsibility principle makes testing focused
- **Low Risk**: Well-defined input/output behavior

## Refactoring Recommendations

### Current Design Assessment
The `GridComponent` is reasonably well-designed but has some hardcoded values:
- **Single Responsibility**: Only draws grid lines
- **Mathematical Logic**: Clear coordinate calculations
- **Data-Driven Behavior**: Adapts to data range appropriately

### Recommended Improvements
1. **Extract Constants**: Margin (20f), stroke size (0.5f), day count (14) as configuration
2. **Configuration Object**: Make grid appearance configurable
3. **Calculation Methods**: Extract complex formulas to separate methods for testability
4. **Color Configuration**: Make grid color configurable

**Recommendation**: Extract configuration object to improve testability and maintainability while preserving the core calculation logic.