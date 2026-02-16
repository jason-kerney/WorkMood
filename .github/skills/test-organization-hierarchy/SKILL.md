---
name: test-organization-hierarchy
description: Test organization and hierarchy guidance for WorkMood. Covers test file structure, naming conventions, test grouping, shared test data, and xUnit patterns. Prevents test suite from becoming unmaintainable as it grows.
argument-hint: "[test component] [organization question/context]"
user-invokable: true
disable-model-invocation: false
---

# Test Organization & Hierarchy Skill

## When to Use This Skill

Use this skill when:
- Adding tests to a new component or service
- Refactoring existing tests that have become hard to navigate
- Setting up test data or fixtures across related tests
- Trying to decide where a test belongs in the file hierarchy
- Standardizing test naming across the team
- Managing test dependencies and shared test setup
- Scaling test suite as components grow in complexity

**Philosophy:** As test suites grow, organization becomes critical. Poorly organized tests are as problematic as untested code—hard to find, easy to duplicate, and increasingly expensive to maintain.

## File Organization: Mirror Source Structure

### The Pattern

Test files mirror the source code structure in `WorkMood.MauiApp.Tests`:

```
MauiApp/                          WorkMood.MauiApp.Tests/
├── Services/                      ├── Services/
│   ├── LineGraphService.cs        │   ├── LineGraphServiceShould.cs
│   ├── MoodDataService.cs         │   └── MoodDataServiceShould.cs
├── ViewModels/                    ├── ViewModels/
│   └── DashboardViewModel.cs      │   └── DashboardViewModelShould.cs
├── Models/                        ├── Models/
│   ├── AxisRange.cs              │   ├── AxisRangeShould.cs
│   └── GraphData.cs              │   └── GraphDataShould.cs
└── Converters/                    └── Converters/
    └── MoodConverters.cs          └── MoodConvertersShould.cs
```

### Why This Works

- **Intuitive navigation** — Find test for component at predictable location
- **Parallel mental models** — Folder hierarchy in source matches test hierarchy
- **Reduced cognitive load** — No guessing where tests live
- **Scales naturally** — Adding new components adds new test files in expected places
- **Easier onboarding** — New team members immediately understand structure

### Exception: Test Helpers & Shared Fixtures

Create `TestHelpers/` directory for cross-cutting test utilities:

```
WorkMood.MauiApp.Tests/
├── Services/
├── ViewModels/
├── TestHelpers/              ← Shared across test suites
│   ├── MockFactory.cs        ← Creates common mocks
│   ├── TestDataBuilder.cs    ← Builds test data consistently
│   └── FixtureBase.cs        ← Base class with common setup
```

**What belongs in TestHelpers:**
- Mock factories (e.g., `MockMoodServiceFactory`)
- Common test data builders
- Shared fixture base classes
- Custom assertions
- Test constants (magic values used in multiple test files)

**What does NOT:**
- Tests that could logically belong in component-specific folder
- One-off test utilities for a single component

---

## Test File Naming: `[Component]Should`

### xUnit Convention in WorkMood

**Format:** `[ComponentName]Should.cs`

```csharp
// ✅ Correct
LineGraphServiceShould.cs          // Matches LineGraphService.cs
AxisRangeShould.cs                  // Matches AxisRange.cs
MoodConvertersShould.cs             // Matches MoodConverters.cs
DashboardViewModelShould.cs          // Matches DashboardViewModel.cs

// ❌ Avoid
LineGraphServiceTests.cs            // "Tests" suffix is redundant, "Should" is clearer
LineGraphService_Tests.cs           // Underscore unnecessarily complicates
TestLineGraphService.cs             // Prefix is backwards from convention
LineGraphServiceTest.cs             // Singular vs plural is inconsistent
```

### Why `[Component]Should`?

- **Reads naturally** — File name becomes implicit English statement: "LineGraphService should..."
- **Consistent with test method naming** — File + method combined read as behavior statement
- **Prevents "Test" overload** — "Test" appears multiple times in naming; "Should" is distinctive
- **Aligns with xUnit idiom** — Fact-based testing uses "should" semantics

---

## Test Class Naming: `[Component]Should`

### Class Name Matches File Name

```csharp
// File: LineGraphServiceShould.cs
namespace WorkMood.MauiApp.Tests.Services
{
    public class LineGraphServiceShould
    {
        // All tests for LineGraphService go here
    }
}
```

### Summary Documentation

Every test class starts with a `/// <summary>` documenting purpose:

```csharp
/// <summary>
/// Tests for LineGraphService - complex service handling graph generation 
/// from mood data with GraphMode switching and background image support
/// </summary>
public class LineGraphServiceShould
{
    // Tests here
}
```

**What goes in summary:**
- Component name and purpose
- Key responsibilities being tested
- Significant dependencies or complexity notes
- Reference to source file location (optional but helpful)

```csharp
/// <summary>
/// Tests for AxisRange immutable record
/// Location: MauiApp/Models/AxisRange.cs
/// Purpose: Verify record behavior, factory methods, and range operations
/// </summary>
public class AxisRangeShould
```

---

## Test Method Naming: Behavior as Specification

### The Convention: `[Method/Aspect]_[Condition]_Should[Behavior]`

```csharp
// Single concern
[Fact]
public void Constructor_WithValidDependencies_ShouldCreateInstance()

// Edge case
[Fact]
public void Constructor_WithNullTransformer_ShouldCreateInstanceWithoutThrowingException()

// Transformation behavior
[Fact]
public void TransformMoodData_WithEmptyList_ShouldReturnEmptyGraphData()

// Validation
[Fact]
public void ValidateDateRange_WithInvalidDates_ShouldThrowArgumentException()

// Property calculation
[Fact]
public void Average_WithMultipleMoods_ShouldCalculateCorrectAverage()
```

### Breakdown of the Convention

| Part | Purpose | Example |
|------|---------|---------|
| `[Method]` | What code is being tested | `Constructor`, `TransformMoodData`, `Average` |
| `_With[Condition]` | Input state or scenario | `WithValidDependencies`, `WithEmptyList`, `WithNullTransformer` |
| `_Should[Behavior]` | Expected outcome | `ShouldCreateInstance`, `ShouldReturnEmptyGraphData`, `ShouldThrowArgumentException` |

### Why This Works

- **Self-documenting** — Reading the test name tells you what it tests and why
- **Searchable** — Method names are easy to grep for related tests
- **Encourages single behavior per test** — Long names discourage testing multiple things
- **Reduces need for comments** — The name *is* the specification

### When to Break the Convention (Rarely)

```csharp
// ✅ Acceptable for very simple tests
[Fact]
public void BeCreatable()

// ✅ Acceptable for parameterized tests with single responsibility
[Theory]
[InlineData(1, 10, true)]
[InlineData(5, 5, false)]
public void BeValidWhen_MinAndMaxProvided(int min, int max, bool isValid)
```

---

## Test Grouping with Regions: Organize for Navigation

### Region Structure

Group related tests using `#region` sections within the class:

```csharp
public class LineGraphServiceShould
{
    #region Constructor Tests
    
    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance() { }
    
    [Fact]
    public void Constructor_WithNullTransformer_ShouldCreateInstance() { }
    
    #endregion

    #region GenerateLineGraph Tests
    
    [Fact]
    public void GenerateLineGraph_WithValidData_ShouldReturnGraph() { }
    
    [Fact]
    public void GenerateLineGraph_WithEmptyData_ShouldReturnEmptyGraph() { }
    
    #endregion

    #region Mode Switching Tests
    
    [Fact]
    public void SwitchMode_FromCompactToDetailed_ShouldUpdateGraphSettings() { }
    
    #endregion
}
```

### Region Organization Strategy

1. **Constructors/Setup** — Most critical, readers expect it first
2. **Main method/property** — Primary responsibility of the class
3. **Secondary methods** — Supporting functionality
4. **Edge cases** — Boundary conditions, error handling
5. **Integration** — Tests requiring multiple components working together

### Why Regions Help

- **Fast navigation** — Fold regions to see structure at a glance
- **Logical grouping** — Related tests cluster together
- **Prevents file sprawl** — Even large 400+ line test files remain navigable
- **Clear test boundaries** — Shows where one behavior group ends and another begins

---

## Test Data Management: Builders and Fixtures

### The Problem: Test Data Duplication

```csharp
// ❌ Bad: Duplicate setup scattered everywhere
[Fact]
public void CalculateAverage_WithThreeMoods_ShouldReturnAverage()
{
    var moods = new List<Mood>
    {
        new Mood { Value = 5, RecordedAt = DateTime.Now },
        new Mood { Value = 7, RecordedAt = DateTime.Now.AddDays(-1) },
        new Mood { Value = 3, RecordedAt = DateTime.Now.AddDays(-2) }
    };
    // Test code...
}

[Fact]
public void FindTrend_WithThreeMoods_ShouldIdentifyDecline()
{
    var moods = new List<Mood>
    {
        new Mood { Value = 5, RecordedAt = DateTime.Now },
        new Mood { Value = 7, RecordedAt = DateTime.Now.AddDays(-1) },
        new Mood { Value = 3, RecordedAt = DateTime.Now.AddDays(-2) }
    };
    // Different test code...
}
```

**Problems:**
- Duplicate setup code
- Changes to `Mood` require updating in multiple places
- Hard to understand which properties matter for the test

### The Solution: Test Data Builders

```csharp
// TestHelpers/MoodDataBuilder.cs
public class MoodDataBuilder
{
    private List<Mood> _moods = new();
    private int _defaultValue = 5;
    private DateTime _defaultDate = DateTime.Now;

    public static MoodDataBuilder CreateDefault() => new();

    public MoodDataBuilder WithValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public MoodDataBuilder WithDecline()
    {
        _moods.Add(new Mood { Value = 5, RecordedAt = _defaultDate });
        _moods.Add(new Mood { Value = 7, RecordedAt = _defaultDate.AddDays(-1) });
        _moods.Add(new Mood { Value = 3, RecordedAt = _defaultDate.AddDays(-2) });
        return this;
    }

    public List<Mood> Build() => _moods;
}
```

### Usage in Tests

```csharp
// ✅ Good: Clear intent, reusable
[Fact]
public void CalculateAverage_WithThreeMoods_ShouldReturnAverage()
{
    var moods = MoodDataBuilder.CreateDefault().WithDecline().Build();
    
    var average = _service.CalculateAverage(moods);
    
    Assert.Equal(5, average);
}

[Fact]
public void FindTrend_WithDecline_ShouldIdentifyDecline()
{
    var moods = MoodDataBuilder.CreateDefault().WithDecline().Build();
    
    var trend = _service.FindTrend(moods);
    
    Assert.Equal(TrendType.Declining, trend);
}
```

### When to Use Builders

- **Complex test data** — Multiple relationships, many properties
- **Reused across tests** — Pattern appears in 3+ test methods
- **Frequently changing** — Reduces refactoring burden

### When Not to Bother

- **Simple one-off data** — `new Mood { Value = 5 }` is clear enough
- **Test-specific data** — Setup only makes sense for one test
- **Primitive values** — Simple integers/strings aren't worth abstraction

---

## Test Fixtures and Shared Setup

### Base Class Pattern for Common Setup

```csharp
// TestHelpers/LineGraphServiceFixture.cs
public class LineGraphServiceFixture : IDisposable
{
    public Mock<IGraphDataTransformer> MockTransformer { get; }
    public Mock<ILineGraphGenerator> MockGenerator { get; }
    public LineGraphService Service { get; }

    public LineGraphServiceFixture()
    {
        MockTransformer = new Mock<IGraphDataTransformer>();
        MockGenerator = new Mock<ILineGraphGenerator>();
        
        Service = new LineGraphService(
            MockTransformer.Object, 
            MockGenerator.Object
        );
    }

    public void Dispose()
    {
        MockTransformer.Reset();
        MockGenerator.Reset();
    }
}
```

### Using the Fixture in Tests

```csharp
public class LineGraphServiceShould : IClassFixture<LineGraphServiceFixture>
{
    private readonly LineGraphServiceFixture _fixture;

    public LineGraphServiceShould(LineGraphServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GenerateGraph_WithValidData_ShouldCallGenerator()
    {
        // Arrange - Use pre-configured mocks
        _fixture.MockTransformer.Setup(t => t.Transform(It.IsAny<List<Mood>>()))
            .Returns(new GraphData());
        
        // Act
        _fixture.Service.GenerateGraph(new List<Mood>());
        
        // Assert
        _fixture.MockGenerator.Verify(g => g.Generate(It.IsAny<GraphData>()), Times.Once);
    }
}
```

### When to Create a Fixture

- **5+ tests** need identical setup
- **Setup is complex** (multiple mocks, intricate configuration)
- **Teardown is required** (cleanup resources, reset state)
- **Component has many dependencies** (reduces duplication)

### When Not to Bother

- **Setup is trivial** — `new Mock<IService>()` directly in test
- **Tests need different setup** — Fixture forces common ground that doesn't exist
- **Shared state causes test coupling** — Tests become dependent on fixture state

---

## Test Hierarchy: Organizing Large Test Suites

### Example: Scaling LineGraphService Tests

**As tests grow, organize into nested classes:**

```csharp
public class LineGraphServiceShould
{
    #region Constructor Tests
    
    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance() { }
    
    #endregion

    #region GenerateLineGraph Tests
    
    public class GenerateLineGraphShould
    {
        private readonly LineGraphService _service;
        private readonly Mock<IGraphDataTransformer> _mockTransformer;

        public GenerateLineGraphShould()
        {
            _mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            _service = new LineGraphService(_mockTransformer.Object, mockGenerator.Object);
        }

        [Fact]
        public void WithValidData_ShouldReturnGraphData() { }

        [Fact]
        public void WithEmptyData_ShouldReturnEmptyGraphData() { }

        [Fact]
        public void WithNullData_ShouldThrowArgumentNullException() { }
    }
    
    #endregion

    #region GraphMode Switching Tests
    
    public class SwitchModeShould
    {
        // Nested class for related tests
        [Fact]
        public void FromCompactToDetailed_ShouldUpdateSettings() { }

        [Fact]
        public void ToUnsupportedMode_ShouldThrowInvalidOperationException() { }
    }
    
    #endregion
}
```

### Benefits of Nested Classes

- **Hierarchical organization** — Related tests group logically
- **Shared setup per group** — Each nested class has its own fixture
- **Improved test discovery** — Test runner shows nested structure
- **Scoped mocks/fixtures** — Nested class setup only affects its tests
- **Natural hierarchy for complex components** — Method + condition organization

### When to Use Nested Classes

- **10+ tests** for a single component
- **Tests naturally divide** into behavior groups
- **Each group needs different setup** (factories, fixtures, mocks)
- **Component has many public methods** (one nested class per method)

---

## Test Constants and Magic Values

### Centralize Magic Values in TestHelpers

```csharp
// TestHelpers/TestConstants.cs
public static class MoodTestConstants
{
    // Standard mood values
    public const int NEUTRAL_MOOD = 5;
    public const int HAPPY_MOOD = 8;
    public const int SAD_MOOD = 2;
    
    // Ranges
    public const int MIN_VALID_MOOD = 1;
    public const int MAX_VALID_MOOD = 10;
    
    // Time constants
    public static readonly DateTime STANDARD_TEST_DATE = new(2024, 01, 15);
    public static readonly DateTime FUTURE_TEST_DATE = new(2040, 01, 15);
    
    // Array/Collection sizes
    public const int SMALL_DATASET_SIZE = 3;
    public const int MEDIUM_DATASET_SIZE = 30;
    public const int LARGE_DATASET_SIZE = 300;
}
```

### Usage

```csharp
[Fact]
public void CalculateAverage_WithMoodValues_ShouldCalculateCorrectly()
{
    var moods = new List<int> 
    { 
        MoodTestConstants.HAPPY_MOOD,
        MoodTestConstants.NEUTRAL_MOOD,
        MoodTestConstants.SAD_MOOD
    };
    
    var average = _service.CalculateAverage(moods);
    
    Assert.Equal(MoodTestConstants.NEUTRAL_MOOD, average);
}
```

### Benefits

- **Self-documenting** — Constant names explain meaning
- **Centralized changes** — Update value once, affects all tests
- **Prevents scattered magic** — Clear where test values come from
- **Easier maintenance** — Business changes update constants, not individual tests

---

## Anti-Patterns to Avoid

### ❌ God Test Files

```csharp
// ❌ Bad: 1000+ lines, tests multiple components
public class AllServicesShould
{
    // Tests for MoodService
    // Tests for GraphService  
    // Tests for ScheduleService
    // Tests for VisualizationService
    // ... and 5 more services
}
```

**Fix:** Split into separate files matching component design

### ❌ Shared Mutable State

```csharp
// ❌ Bad: Class-level state shared across tests
public class MoodServiceShould
{
    private List<Mood> _testMoods = new(); // ❌ Shared state

    [Fact]
    public void FirstTest()
    {
        _testMoods.Add(new Mood { Value = 5 });
        // Now state depends on test execution order!
    }

    [Fact]
    public void SecondTest()
    {
        // _testMoods might already have items from FirstTest
        _testMoods.Add(new Mood { Value = 7 });
    }
}
```

**Fix:** Create fresh state per test

```csharp
// ✅ Good: Fresh state per test
[Fact]
public void FirstTest()
{
    var testMoods = new List<Mood> { new Mood { Value = 5 } };
    // Fresh state, no coupling
}

[Fact]
public void SecondTest()
{
    var testMoods = new List<Mood> { new Mood { Value = 7 } };
    // Completely independent
}
```

### ❌ Test Organization by Test Type

```csharp
// ❌ Bad: Organization by "type of test"
WorkMood.MauiApp.Tests/
├── UnitTests/
│   ├── LineGraphServiceTests.cs
│   ├── MoodDataServiceTests.cs
├── IntegrationTests/
│   ├── GraphVisualizationTests.cs
└── EdgeCaseTests/
    ├── NullHandlingTests.cs
```

**Problem:** Doesn't match source structure; hard to correlate tests with components

**Fix:** Mirror source structure

```csharp
// ✅ Good: Organization mirrors source
WorkMood.MauiApp.Tests/
├── Services/
│   ├── LineGraphServiceShould.cs
│   ├── MoodDataServiceShould.cs
├── ViewModels/
│   └── DashboardViewModelShould.cs
```

### ❌ Over-Complex Test Data Builders

```csharp
// ❌ Bad: Builders that do too much
var mood = MoodBuilder
    .Create()
    .WithValue(5)
    .WithDate(DateTime.Now)
    .WithNotes("test")
    .AndColorCode(MoodColor.Neutral)
    .AndBehaviorFlag(BehaviorFlag.Reflective)
    .AndWeatherContext(Weather.Rainy)
    .AndLocation("home")
    .AndCompany(Company.Alone)
    .WithPreviousMood(new Mood { Value = 7 })
    .AndPredictedNextMood(new Mood { Value = 6 })
    .Build();
```

**Problem:** Builder complexity obscures what the test actually needs

**Fix:** Keep builders focused; use targeted methods

```csharp
// ✅ Better: Clear, focused builder
var mood = MoodBuilder.CreateWithValue(5);

// Or even simpler when not needed:
var mood = new Mood { Value = 5 };
```

---

## Quick Checklist: Is My Test Suite Well-Organized?

- ✅ Test files mirror source structure (Services tests in Services/, Models tests in Models/)
- ✅ Test files named `[Component]Should.cs`
- ✅ Test methods named `[Method]_[Condition]_Should[Behavior]`
- ✅ Related tests grouped in regions (Constructor, main method, edge cases)
- ✅ Each test class has a summary comment explaining purpose
- ✅ Common setup extracted to TestHelpers/ or fixtures
- ✅ Magic values centralized in TestConstants.cs
- ✅ Test data builders for complex, reused scenarios
- ✅ No shared mutable state between tests
- ✅ Test file under 400 lines (consider nested classes if larger)

---

## Next Steps with Organization Foundation

Once your tests are well-organized:

1. **Use `/tdd` skill** for writing new tests with Red-Green-Refactor
2. **Use `/refactoring` skill** to refactor production code with test safety
3. **Use `/code-smells` skill** to identify issues test structure reveals
4. **Run test suite regularly** — organized tests are easier to maintain and extend

Remember: **Organization is maintenance.** As your test suite grows, good organization prevents it from becoming a liability rather than an asset. 🤖
