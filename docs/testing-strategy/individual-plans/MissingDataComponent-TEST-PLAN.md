# MissingDataComponent Test Plan

## Object Analysis

**File**: `MauiApp/Graphics/EnhancedLineGraphDrawable.cs`  
**Type**: Graphics Component Class  
**Primary Purpose**: Draw visual indicators (circles) for missing data points on visualization graphs  
**Key Functionality**: Renders small white circles with blue outlines at calculated positions for days without valid mood data

### Purpose & Responsibilities

The `MissingDataComponent` is responsible for drawing visual indicators (circles) that represent missing data points in the mood visualization. It identifies days where mood data is missing (HasData=false) and renders small white circles with blue outlines to clearly indicate data gaps in the trend graph.

### Architecture Role

- **Layer**: Graphics/Presentation Layer
- **Pattern**: Strategy Pattern (implements `IGraphComponent`)
- **MVVM Role**: View-specific rendering component for missing data visualization  
- **Clean Architecture**: UI/Graphics layer component for data gap representation

### Dependencies Analysis

#### Constructor Dependencies

- **None**: Default parameterless constructor

#### Method Dependencies

- **ICanvas canvas**: Microsoft.Maui.Graphics canvas abstraction for drawing operations
- **RectF bounds**: Drawing bounds rectangle for positioning calculations
- **MoodVisualizationData data**: Mood data containing daily values for missing data identification

#### Static Dependencies

- **Colors.White**: Hard dependency on static color for circle fill
- **Colors.Blue**: Hard dependency on static color for circle outline  
- **PointF creation**: Creates individual PointF for each missing data position
- **Mathematical calculations**: Complex coordinate calculations and scaling logic

#### Platform Dependencies

- **Microsoft.Maui.Graphics**: MAUI graphics abstraction layer
- **ICanvas drawing operations**: FillEllipse and DrawEllipse methods for rendering circles

### Public Interface Documentation

#### Methods

**`void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)`**

- **Purpose**: Draws small circles at positions representing missing mood data
- **Parameters**: 
  - `canvas`: Drawing surface for graphics operations
  - `bounds`: Rectangular bounds defining the drawing area
  - `data`: Visualization data containing daily values and missing data flags
- **Return Type**: void
- **Side Effects**: Modifies canvas state (fill color, stroke properties, draws multiple circles)
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

- ✅ **Single Responsibility**: Clear focus on missing data visualization
- ✅ **Simple Logic**: Straightforward filtering and drawing operations
- ✅ **Data-Driven Behavior**: Behavior varies based on missing data patterns
- ✅ **Predictable Output**: Deterministic circle drawing based on input data

### Challenges

- ⚠️ **Hard Color Dependencies**: Static Colors.White and Colors.Blue references
- ⚠️ **Circle Drawing Verification**: Multiple canvas operations per missing data point

### Current Testability Score Justification

Score: **8/10** - Excellent testability with minimal complexity

**Deductions**:
- **-1 point**: Hard dependencies on static color system
- **-1 point**: Multiple canvas operations make sequence verification complex

### Hard Dependencies Identified

1. **Static Color Creation**: `Colors.White` for circle fill and `Colors.Blue` for outline
2. **ICanvas Drawing**: Multiple FillEllipse and DrawEllipse operations
3. **Data Structure Navigation**: Deep dependency on MoodVisualizationData.DailyValues

### Required Refactoring

**Minimal refactoring recommended - excellent structure for testing**

#### Option 1: Extract Circle Configuration (Optional)

```csharp
public class MissingDataConfiguration
{
    public Color FillColor { get; set; } = Colors.White;
    public Color OutlineColor { get; set; } = Colors.Blue;
    public float CircleRadius { get; set; } = 3f;
    public float StrokeWidth { get; set; } = 1f;
}

public class MissingDataComponent : IGraphComponent
{
    private readonly MissingDataConfiguration _config;
    
    public MissingDataComponent(MissingDataConfiguration config = null)
    {
        _config = config ?? new MissingDataConfiguration();
    }
    
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        canvas.FillColor = _config.FillColor;
        canvas.StrokeColor = _config.OutlineColor;
        canvas.StrokeSize = _config.StrokeWidth;
        // ... rest of method
    }
}
```

**Recommendation**: Keep current design and test through comprehensive missing data scenarios and circle position verification.

## Test Implementation Strategy

### Test Class Structure

```csharp
[TestFixture]
public class MissingDataComponentTests
{
    private MissingDataComponent _component;
    private Mock<ICanvas> _mockCanvas;
    private RectF _standardBounds;
    
    [SetUp]
    public void Setup()
    {
        _component = new MissingDataComponent();
        _mockCanvas = new Mock<ICanvas>();
        _standardBounds = new RectF(0, 0, 300, 200);
    }
    
    // Test methods here
}
```

### Mock Strategy

- **ICanvas**: Use Moq to mock fill color, stroke properties, and ellipse drawing operations
- **MoodVisualizationData**: Create various data scenarios with different patterns of missing data
- **Circle Position Verification**: Verify correct circles are drawn at missing data positions
- **Canvas Operation Verification**: Test fill and stroke operations for each circle

### Test Categories

1. **Missing Data Detection Tests**: Verify correct identification of missing data days
2. **Circle Drawing Tests**: Verify circles drawn at correct positions for missing data
3. **Canvas Property Tests**: Verify fill/stroke colors and circle properties
4. **Position Calculation Tests**: Mathematical coordinate positioning verification
5. **Pattern Tests**: Various combinations of missing/valid data
6. **Edge Case Tests**: No missing data, all missing data, boundary conditions

## Detailed Test Cases

### Method: Draw

#### Purpose

Identifies days with missing mood data (HasData=false) and draws small white circles with blue outlines at calculated positions to indicate data gaps.

#### Test Cases

##### Missing Data Detection Tests

**Test**: `Draw_WithNoMissingData_ShouldNotDrawAnyCircles`

```csharp
[Test]
public void Draw_WithNoMissingData_ShouldNotDrawAnyCircles()
{
    // Arrange
    var data = CreateDataWithAllValidPoints();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Never);
    _mockCanvas.Verify(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Never);
}
```

**Test**: `Draw_WithAllMissingData_ShouldDraw14Circles`

```csharp
[Test]
public void Draw_WithAllMissingData_ShouldDraw14Circles()
{
    // Arrange
    var data = CreateDataWithAllMissingPoints();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw 14 circles (one for each missing day)
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Exactly(14));
    _mockCanvas.Verify(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Exactly(14));
}
```

**Test**: `Draw_WithSpecificMissingDays_ShouldDrawCorrectCount`

```csharp
[Test]
public void Draw_WithSpecificMissingDays_ShouldDrawCorrectCount()
{
    // Arrange - Missing data on days 1, 3, 5, 7
    var missingDays = new[] { 1, 3, 5, 7 };
    var data = CreateDataWithMissingDays(missingDays);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw 4 circles
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Exactly(4));
    _mockCanvas.Verify(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Exactly(4));
}
```

##### Circle Position Tests

**Test**: `Draw_WithKnownMissingDay_ShouldDrawAtCorrectPosition`

```csharp
[Test]
public void Draw_WithKnownMissingDay_ShouldDrawAtCorrectPosition()
{
    // Arrange - Missing data on day 5
    var data = CreateDataWithSingleMissingDay(5);
    
    var pointSpacing = 260f / 13f; // (300-40)/13
    var centerY = 100f; // (200-40)/2 + 20
    var expectedX = 20f + (5 * pointSpacing);
    var expectedPosition = new PointF(expectedX, centerY);
    var expectedSize = new SizeF(6f, 6f); // radius 3 = diameter 6
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(expectedPosition, expectedSize), Times.Once);
    _mockCanvas.Verify(c => c.DrawEllipse(expectedPosition, expectedSize), Times.Once);
}
```

**Test**: `Draw_WithMultipleMissingDays_ShouldDrawAtCorrectPositions`

```csharp
[Test]
public void Draw_WithMultipleMissingDays_ShouldDrawAtCorrectPositions()
{
    // Arrange - Missing data on days 0, 6, 13
    var missingDays = new[] { 0, 6, 13 };
    var data = CreateDataWithMissingDays(missingDays);
    
    var pointSpacing = 260f / 13f;
    var centerY = 100f;
    var expectedSize = new SizeF(6f, 6f);
    
    var expectedPositions = missingDays.Select(day => 
        new PointF(20f + (day * pointSpacing), centerY)).ToArray();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    foreach (var position in expectedPositions)
    {
        _mockCanvas.Verify(c => c.FillEllipse(position, expectedSize), Times.Once);
        _mockCanvas.Verify(c => c.DrawEllipse(position, expectedSize), Times.Once);
    }
}
```

##### Canvas Property Tests

**Test**: `Draw_ShouldSetCorrectCanvasProperties`

```csharp
[Test]
public void Draw_ShouldSetCorrectCanvasProperties()
{
    // Arrange
    var data = CreateDataWithSingleMissingDay(3);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.VerifySet(c => c.FillColor = Colors.White, Times.Once);
    _mockCanvas.VerifySet(c => c.StrokeColor = Colors.Blue, Times.Once);
    _mockCanvas.VerifySet(c => c.StrokeSize = 1f, Times.Once);
}
```

##### Mathematical Calculation Tests

**Test**: `Draw_WithDifferentBounds_ShouldScalePositionsCorrectly`

```csharp
[Test]
[TestCase(400, 300, 20f, 150f)] // Large bounds
[TestCase(200, 150, 20f, 75f)]  // Medium bounds
[TestCase(100, 100, 20f, 50f)]  // Small bounds
public void Draw_WithDifferentBounds_ShouldScalePositionsCorrectly(
    float width, float height, float expectedMargin, float expectedCenterY)
{
    // Arrange
    var bounds = new RectF(0, 0, width, height);
    var data = CreateDataWithSingleMissingDay(0); // First day missing
    
    var expectedX = expectedMargin; // Day 0 position
    var expectedPosition = new PointF(expectedX, expectedCenterY);
    var expectedSize = new SizeF(6f, 6f);
    
    // Act
    _component.Draw(_mockCanvas.Object, bounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(expectedPosition, expectedSize), Times.Once);
    _mockCanvas.Verify(c => c.DrawEllipse(expectedPosition, expectedSize), Times.Once);
}
```

**Test**: `Draw_WithSpacingCalculation_ShouldPositionCorrectly`

```csharp
[Test]
public void Draw_WithSpacingCalculation_ShouldPositionCorrectly()
{
    // Arrange - Test days 0, 7, 13 (first, middle, last)
    var missingDays = new[] { 0, 7, 13 };
    var data = CreateDataWithMissingDays(missingDays);
    
    var margin = 20f;
    var graphWidth = 300f - (2 * margin); // 260f
    var pointSpacing = graphWidth / 13f; // ~20f
    var centerY = 100f;
    
    var expectedPositions = new[]
    {
        new PointF(margin + (0 * pointSpacing), centerY), // Day 0
        new PointF(margin + (7 * pointSpacing), centerY), // Day 7  
        new PointF(margin + (13 * pointSpacing), centerY) // Day 13
    };
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    foreach (var position in expectedPositions)
    {
        _mockCanvas.Verify(c => c.FillEllipse(position, It.IsAny<SizeF>()), Times.Once);
    }
}
```

##### Pattern Tests

**Test**: `Draw_WithAlternatingMissingData_ShouldDrawCorrectPattern`

```csharp
[Test]
public void Draw_WithAlternatingMissingData_ShouldDrawCorrectPattern()
{
    // Arrange - Missing on even days (0, 2, 4, 6, 8, 10, 12)
    var missingDays = Enumerable.Range(0, 14).Where(i => i % 2 == 0).ToArray();
    var data = CreateDataWithMissingDays(missingDays);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should draw 7 circles for even days
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), 
        Times.Exactly(7));
}
```

**Test**: `Draw_WithRandomMissingPattern_ShouldHandleCorrectly`

```csharp
[Test]
public void Draw_WithRandomMissingPattern_ShouldHandleCorrectly()
{
    // Arrange - Random pattern: days 1, 4, 8, 11
    var missingDays = new[] { 1, 4, 8, 11 };
    var data = CreateDataWithMissingDays(missingDays);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), 
        Times.Exactly(4));
    _mockCanvas.Verify(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), 
        Times.Exactly(4));
}
```

##### Circle Size and Properties Tests

**Test**: `Draw_ShouldUseCorrectCircleSize`

```csharp
[Test]
public void Draw_ShouldUseCorrectCircleSize()
{
    // Arrange
    var data = CreateDataWithSingleMissingDay(5);
    var expectedSize = new SizeF(6f, 6f); // radius 3 = diameter 6
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), expectedSize), Times.Once);
    _mockCanvas.Verify(c => c.DrawEllipse(It.IsAny<PointF>(), expectedSize), Times.Once);
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
    var data = CreateDataWithSingleMissingDay(3);
    
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
    var bounds = new RectF(-10, -10, 50, 50);
    var data = CreateDataWithSingleMissingDay(3);
    
    // Act & Assert
    Assert.DoesNotThrow(() => _component.Draw(_mockCanvas.Object, bounds, data));
}
```

##### Error Conditions

**Test**: `Draw_WithNullCanvas_ShouldThrowException`

```csharp
[Test]
public void Draw_WithNullCanvas_ShouldThrowException()
{
    // Arrange
    var data = CreateDataWithSingleMissingDay(3);
    
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
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Never);
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

**Test**: `Draw_ShouldSetPropertiesBeforeDrawingCircles`

```csharp
[Test]
public void Draw_ShouldSetPropertiesBeforeDrawingCircles()
{
    // Arrange
    var data = CreateDataWithSingleMissingDay(3);
    var sequence = new MockSequence();
    
    _mockCanvas.InSequence(sequence).SetupSet(c => c.FillColor = Colors.White);
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeColor = Colors.Blue);
    _mockCanvas.InSequence(sequence).SetupSet(c => c.StrokeSize = 1f);
    _mockCanvas.InSequence(sequence).Setup(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert handled by sequence setup
    _mockCanvas.VerifyAll();
}
```

**Test**: `Draw_ShouldDrawFillBeforeStrokeForEachCircle`

```csharp
[Test]
public void Draw_ShouldDrawFillBeforeStrokeForEachCircle()
{
    // Arrange - Two missing days to verify sequence per circle
    var data = CreateDataWithMissingDays(new[] { 2, 8 });
    var sequence = new MockSequence();
    
    // First circle
    _mockCanvas.InSequence(sequence).Setup(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    
    // Second circle  
    _mockCanvas.InSequence(sequence).Setup(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    _mockCanvas.InSequence(sequence).Setup(c => c.DrawEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()));
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert handled by sequence setup
    _mockCanvas.VerifyAll();
}
```

##### Data Validation Tests

**Test**: `Draw_WithMixedDataStates_ShouldOnlyDrawMissingData`

```csharp
[Test]
public void Draw_WithMixedDataStates_ShouldOnlyDrawMissingData()
{
    // Arrange - Complex mix: some valid data, some missing, some null values
    var data = CreateMixedDataPattern();
    
    // Expected: Only days marked as HasData=false should have circles
    var expectedMissingCount = data.DailyValues.Count(d => !d.HasData);
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), 
        Times.Exactly(expectedMissingCount));
}
```

**Test**: `Draw_WithDataButNoValues_ShouldNotDrawCircles`

```csharp
[Test]
public void Draw_WithDataButNoValues_ShouldNotDrawCircles()
{
    // Arrange - HasData=true but Value=null (edge case)
    var data = CreateDataWithNullValues();
    
    // Act
    _component.Draw(_mockCanvas.Object, _standardBounds, data);
    
    // Assert - Should not draw circles for HasData=true cases
    _mockCanvas.Verify(c => c.FillEllipse(It.IsAny<PointF>(), It.IsAny<SizeF>()), Times.Never);
}
```

## Test Implementation Notes

### Testing Challenges

1. **Circle Position Accuracy**: Verifying exact coordinate calculations
2. **Canvas Operation Sequence**: Two operations per circle (fill + stroke)
3. **Missing Data Detection**: Correct filtering based on HasData flag
4. **Circle Size Consistency**: Same size for all missing data indicators

### Recommended Approach

1. **Pattern-Based Testing**: Test various missing data patterns
2. **Position Verification**: Mathematical accuracy of circle positioning
3. **Canvas Sequence Testing**: Verify correct fill-then-stroke order
4. **Edge Case Coverage**: Handle boundary conditions and null data

### Test Data Helper Methods

```csharp
private static MoodVisualizationData CreateDataWithSingleMissingDay(int missingDay)
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = (i != missingDay),
            Value = (i != missingDay) ? 0.0 : null,
            Color = Colors.Blue
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateDataWithMissingDays(int[] missingDays)
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = !missingDays.Contains(i),
            Value = missingDays.Contains(i) ? null : 0.0,
            Color = Colors.Blue
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateDataWithAllMissingPoints()
{
    var dailyValues = new DailyMoodValue[14];
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = false,
            Value = null,
            Color = Colors.Blue
        };
    }
    
    return new MoodVisualizationData
    {
        DailyValues = dailyValues,
        MaxAbsoluteValue = 5.0
    };
}

private static MoodVisualizationData CreateMixedDataPattern()
{
    var dailyValues = new DailyMoodValue[14];
    var missingDays = new[] { 1, 3, 7, 9, 12 }; // 5 missing days
    
    for (int i = 0; i < 14; i++)
    {
        dailyValues[i] = new DailyMoodValue
        {
            Date = new DateOnly(2023, 1, i + 1),
            HasData = !missingDays.Contains(i),
            Value = missingDays.Contains(i) ? null : (i % 3 - 1), // -1, 0, 1 pattern
            Color = Colors.Blue
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
│   ├── MissingDataComponentTests.cs
│   ├── TestHelpers/
│   │   ├── MissingDataPatternBuilder.cs
│   │   ├── CirclePositionVerificationHelpers.cs
│   │   └── CanvasOperationSequenceVerification.cs
```

## Coverage Goals

- **Method Coverage**: 100% - Single public method with all branches
- **Line Coverage**: 98% - All data filtering and circle drawing logic
- **Branch Coverage**: 95% - HasData flag validation checks
- **Data Pattern Coverage**: 100% - All combinations of missing/valid data patterns  
- **Position Accuracy**: 95% - Mathematical coordinate calculation verification

## Implementation Checklist

### Phase 1 - Test Infrastructure Setup

- [ ] **Create Missing Data Pattern Builders**: Various missing data combinations
- [ ] **Setup Circle Position Verification**: Coordinate calculation and comparison utilities
- [ ] **Canvas Operation Sequence Verification**: Fill/stroke operation verification patterns
- [ ] **Mathematical Test Helpers**: Position calculation verification

### Phase 2 - Core Logic Tests

- [ ] **Missing Data Detection Tests**: HasData flag filtering logic
- [ ] **Circle Drawing Tests**: Fill and stroke operations for each missing day
- [ ] **Canvas Property Tests**: Fill/stroke colors and circle size verification
- [ ] **Position Calculation Tests**: Mathematical positioning accuracy

### Phase 3 - Pattern & Edge Cases

- [ ] **Pattern Variation Tests**: Different combinations of missing data
- [ ] **All/None Missing Tests**: Edge cases with complete missing or complete valid data
- [ ] **Boundary Condition Tests**: Extreme bounds and edge positions
- [ ] **Error Handling Tests**: Null parameters and invalid data

### Phase 4 - Integration & Performance

- [ ] **Complex Pattern Tests**: Real-world missing data scenarios
- [ ] **Canvas Operation Efficiency**: Verify minimal canvas state changes
- [ ] **Position Accuracy Verification**: Precise coordinate calculations
- [ ] **Coverage Analysis**: Verify 98%+ line coverage achieved

## Arlo's Commit Strategy

- `^f - add comprehensive test suite for MissingDataComponent with 98% coverage`
- `^f - add missing data detection and circle positioning tests for MissingDataComponent`  
- `^f - add canvas operation sequence tests for MissingDataComponent`
- `^f - add pattern variation and edge case tests for MissingDataComponent`

## Risk Assessment

- **Low Risk**: Simple single responsibility for missing data visualization
- **Low Risk**: Straightforward filtering logic with clear HasData flag
- **Low Risk**: Deterministic circle drawing with predictable output
- **Low Risk**: Well-defined input/output behavior with minimal complexity

## Refactoring Recommendations

### Current Design Assessment

The `MissingDataComponent` demonstrates excellent design principles:

- **Single Responsibility**: Only draws circles for missing data points
- **Clear Logic**: Simple HasData flag filtering with deterministic output
- **Appropriate Rendering**: Visually distinct indicators for data gaps  
- **Consistent Appearance**: Uniform circle size and styling

### Potential Improvements (Optional)

1. **Configuration Object**: Make circle appearance configurable
2. **Extract Constants**: Make circle radius and colors configurable
3. **Validation Methods**: Add explicit data validation before processing
4. **Performance Optimization**: Consider caching position calculations

**Recommendation**: Current design is excellent for testing - maintain existing architecture while adding comprehensive test coverage for all missing data patterns and circle positioning accuracy.