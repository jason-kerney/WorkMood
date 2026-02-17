---
name: tdd
description: Test-Driven Development guidance for WorkMood. Covers the Red-Green-Refactor cycle, test naming conventions, Arrange-Act-Assert structure, and W# testing patterns with xUnit. Emphasizes tests as design and behavior specification.
argument-hint: "[feature/method to implement] [test scenario]"
user-invokable: true
disable-model-invocation: false
---

# Test-Driven Development Skill

## When to Use This Skill

Use this skill when you:
- Are implementing a new feature and want to ensure it's testable from the start
- Need guidance on how to structure a test for a specific scenario
- Want to understand how tests drive design decisions
- Are unsure about test naming or what to assert
- Want to practice the Red-Green-Refactor cycle
- Need to add tests to existing untested code

## Core TDD Philosophy

**Tests are not verification—they are specification.**

Tests define:
- What the code should do (behavior)
- When it should do it (scenarios)
- What inputs it accepts
- What outputs it produces
- How it handles edge cases

## The Red-Green-Refactor Cycle

### 1. Red: Write a Failing Test
Write a test that describes the behavior you want to implement. The test **must fail** initially because the feature doesn't exist yet.

```csharp
[Fact]
public void ShouldCalculateMoodAverageForDateRange()
{
    // Test code here
    Assert.Equal(3.5, average);
}
```

**Why:** The failing test proves your test infrastructure works and that the feature is actually missing.

### 2. Green: Write Minimal Code
Write the **simplest code possible** to make the test pass. Don't over-engineer. Don't think about refactoring yet.

```csharp
public double CalculateMoodAverage(DateRange range)
{
    return 3.5; // Simplest possible implementation
}
```

**Why:** Speed. Get to green quickly. The test defines what "correct" means.

### 3. Refactor: Improve Without Changing Behavior
Now that tests pass, safely refactor:
- Extract methods
- Remove duplication
- Improve naming
- Introduce better abstractions

**Tests protect you:** Any change that breaks the test is a behavior violation. Go back and fix it.

## Test Structure: Arrange-Act-Assert (AAA)

Every test should follow this clear pattern:

```csharp
[Fact]
public void ShouldReturnErrorWhenMoodValueOutOfRange()
{
    // ARRANGE: Set up test data and dependencies
    var moodService = new MoodDataService(fakeRepository);
    var invalidMoodValue = 6; // WorkMood supports 1-5

    // ACT: Call the method being tested
    var result = moodService.ValidateMoodValue(invalidMoodValue);

    // ASSERT: Verify the expected behavior
    Assert.False(result.IsValid);
    Assert.Equal("Mood value must be between 1 and 5", result.ErrorMessage);
}
```

**Benefits:**
- Instantly clear what's being tested
- Easy to spot when tests are too complex
- Simpler to debug failures

## Test Naming Conventions for C#/xUnit

### Pattern: `Should[ExpectedBehavior]When[Scenario]`

```csharp
[Fact]
public void ShouldReturnTodaysMoodWhenDateIsToday() { }

[Fact]
public void ShouldThrowArgumentExceptionWhenValueIsNull() { }

[Fact]
public void ShouldIgnoreDuplicateEntriesWhenMergingData() { }
```

### Alternative: `[MethodName]_[Scenario]_[ExpectedResult]`

```csharp
[Fact]
public void CalculateAverage_WithValidDates_ReturnsCorrectValue() { }
```

**Choose one style and be consistent.** The first style (Story-like) aligns better with TDD philosophy.

## Common TDD Patterns in WorkMood

### Testing Service Methods
```csharp
[Fact]
public void ShouldSaveNewMoodEntryWithCorrectTimestamp()
{
    // ARRANGE
    var moodService = new MoodDataService(mockRepository);
    var moodEntry = new MoodEntry { Value = 4, Date = DateTime.Today };

    // ACT
    moodService.AddMoodEntry(moodEntry);

    // ASSERT
    mockRepository.Verify(x => x.Save(It.IsAny<MoodEntry>()), Times.Once);
}
```

### Testing ViewModels
```csharp
[Fact]
public void ShouldUpdateDisplayWhenMoodDataChanges()
{
    // ARRANGE
    var mockService = new Mock<IMoodDataService>();
    var viewModel = new MoodDashboardViewModel(mockService.Object);
    var newMoodData = new[] { /* test data */ };

    // ACT
    mockService.Setup(s => s.GetRecentMoods()).Returns(newMoodData);
    viewModel.RefreshData();

    // ASSERT
    Assert.Equal(newMoodData.Length, viewModel.MoodEntries.Count);
}
```

### Testing Edge Cases
```csharp
[Theory]
[InlineData(0)]       // Below minimum
[InlineData(6)]       // Above maximum
[InlineData(-1)]      // Negative
public void ShouldRejectInvalidMoodValues(int invalidValue)
{
    // ARRANGE
    var validator = new MoodValidator();

    // ACT & ASSERT
    Assert.Throws<ArgumentOutOfRangeException>(() => 
        validator.Validate(invalidValue));
}
```

## Step-by-Step TDD Process

### For a New Feature

1. **Write one failing test** that describes the smallest piece of behavior
   ```
   Goal: "Add a method that calculates 7-day mood average"
   Test: "Should return 0 when there are no mood entries for the range"
   ```

2. **Run the test** - Confirm it fails with the right error
   ```
   dotnet test
   ```

3. **Write minimal code** to pass that one test
   ```csharp
   public decimal GetAverageMood(DateRange range) => 0m;
   ```

4. **Run tests** - Confirm they pass
   ```
   dotnet test
   ```

5. **Write the next test** - Add another scenario
   ```
   Test: "Should return correct average when entries exist"
   ```

6. **Repeat** - Steps 2-5 until the feature is complete

7. **Refactor** - Once all tests pass, improve the code structure

### For Existing Code

If code lacks tests:

1. **Write tests for current behavior first** (golden master)
2. **Verify tests pass** with the existing implementation
3. **Now you're safe** to refactor or add features

## Test Assertions: What to Check

### Good Assertions (Specific)
```csharp
Assert.Equal(5, moodEntry.Value);           // Exact value
Assert.NotNull(result);                     // Not null
Assert.True(result.IsValid);                // Specific boolean
Assert.StartsWith("Error:", message);       // String pattern
Assert.Single(collection);                  // Collection size
```

### Weak Assertions (Too General)
```csharp
Assert.NotNull(result);  // Doesn't verify anything about result
Assert.True(something);  // What is something? Unclear.
```

## Common TDD Anti-Patterns

❌ **Testing Implementation, Not Behavior**
```csharp
// BAD - Tests how, not what
[Fact]
public void ShouldCallRepositorySaveMethod() { /* ... */ }
```

✅ **Testing Observable Behavior**
```csharp
// GOOD - Tests what happens
[Fact]
public void ShouldPersistMoodEntryToStorage() { /* ... */ }
```

---

❌ **Testing Too Much in One Test**
```csharp
// BAD - Multiple responsibilities
[Fact]
public void ShouldDoEverything()
{
    // Setup, load, save, validate, format... all in one test
}
```

✅ **One Behavior Per Test**
```csharp
// GOOD - Single responsibility
[Fact]
public void ShouldValidateBeforeSaving() { /* ... */ }

[Fact]
public void ShouldFormatOutputCorrectly() { /* ... */ }
```

---

❌ **Brittle Tests That Break on Refactoring**
```csharp
// BAD - Tests internal structure
[Fact]
public void ShouldCallPrivateHelperMethod() { /* ... */ }
```

✅ **Tests That Survive Refactoring**
```csharp
// GOOD - Tests public behavior
[Fact]
public void ShouldComputeCorrectValue() { /* ... */ }
```

## Example: TDD in Action

### Feature: Calculate 7-Day Mood Trend

**Iteration 1:**
```csharp
[Fact]
public void ShouldReturnEmptyWhenNoEntriesExist()
{
    var service = new MoodAnalysisService(new MockRepository());
    var trend = service.GetSevenDayTrend();
    Assert.Empty(trend);
}
```

Implementation: `public IEnumerable<MoodEntry> GetSevenDayTrend() => Enumerable.Empty<MoodEntry>();`

**Iteration 2:**
```csharp
[Fact]
public void ShouldReturnEntriesFromLastSevenDays()
{
    var service = new MoodAnalysisService(mockRepository);
    mockRepository.Setup(r => r.GetEntriesSince(It.IsAny<DateTime>()))
        .Returns(testEntries);
    
    var trend = service.GetSevenDayTrend();
    Assert.Equal(7, trend.Count());
}
```

Implementation: Add real logic to fetch entries from the last 7 days

**Iteration 3:** Continue adding tests, refactoring as needed

## Key Reminders

✅ **Tests document behavior** - Good test names are living documentation  
✅ **Test first, code second** - Write the test before the feature  
✅ **One assertion per test** (generally) - Easier to understand what failed  
✅ **Tests protect refactoring** - The safety net that enables continuous improvement  
✅ **Test at the right level** - Unit tests for methods, integration tests for workflows  
✅ **Keep tests simple** - If tests are hard to write, the code probably needs refactoring  

## Related Resources

- `.github/copilot-personas/jason-kerney.md` - XP principles and philosophy
- `.github/skills/refactoring/SKILL.md` - Refactoring safely with test coverage
- `.github/ai-codex-build-testing.md` - WorkMood testing strategies and quality gates
