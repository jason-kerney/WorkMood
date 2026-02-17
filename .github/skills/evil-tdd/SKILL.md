---
name: evil-tdd
description: Use "evil" mode to force proper TDD discipline and best test design. An evil implementer writes code to pass tests while breaking the spirit of TDD, forcing you to write tests so precise they prevent bad implementations. Ensures tests truly specify behavior, not just documentation.
argument-hint: [feature to test] [test scenario]
user-invokable: true
disable-model-invocation: false
---

# Evil TDD: A Forcing Function for Perfect Tests

This skill guides you through **Evil TDD**—a collaborative practice where one developer writes deliberately "evil" implementations designed to pass bad tests while violating the spirit of proper design. This forces the other developer to write tests so precise and comprehensive that only correct, well-designed code can pass them.

## What is Evil TDD?

Evil TDD is a **design forcing function** based on the Red-Green-Refactor cycle:

1. **RED**: You write a test for a feature
2. **EVIL GREEN**: A partner writes the worst possible code that passes your test (while technically passing)
3. **Your response**: Recognize what your test failed to specify, and write a **better test**
4. **REAL GREEN**: Only when tests are ironclad can "good" code pass them
5. **REFACTOR**: Clean up the implementation with confidence tests won't break

The goal: **Write tests so precise they guarantee good design**, because they've already defeated every cheap hack and shortcut.

---

## Why Evil TDD Works

When you write a test, you think it's perfect. But an evil implementer will find **every loophole**:
- They'll return hardcoded values that technically pass
- They'll exploit vague assertions
- They'll implement the minimum possible to escape your test
- They'll implement behavior you didn't explicitly forbid

**This is the point.** Each loophole reveals what your test failed to specify. You're forced to:
- Write more specific assertions
- Test edge cases you missed
- Verify actual behavior, not just happy paths
- Think deeply about what "correct" means

Result: Tests become **ironclad specifications** that prevent bad design automatically.

---

## Evil Implementation Patterns & How to Counter Them

### 1. **The Hardcoded Return** (Evil Returns Your Exact Expected Value)

**Your Test (Round 1 - Too Vague):**
```csharp
[Fact]
public void GetMoodHistory_WithValidPeriod_ReturnsData()
{
    var service = new MoodHistoryService(new InMemoryStorage());
    service.AddMood("happy", DateTime.Now);
    service.AddMood("sad", DateTime.Now.AddDays(-1));
    
    var result = service.GetMoodHistory(DateRange.LastSevenDays);
    
    Assert.NotNull(result); // Way too loose!
    Assert.NotEmpty(result);
}
```

**Evil's Implementation (Technically Passes):**
```csharp
public class MoodHistoryService
{
    public IEnumerable<MoodEntry> GetMoodHistory(DateRange period)
    {
        return new[] { new MoodEntry("fake", DateTime.Now) }; // Ignores period!
    }
}
```

**What Your Test Failed To Specify:**
- You didn't verify the data is actually filtered by the date range
- You didn't test that it returns the SPECIFIC moods you added
- You didn't test boundary conditions (edge of the time period)
- You didn't forbid fake data

**Your Test (Round 2 - Precise Specifications):**
```csharp
[Fact]
public void GetMoodHistory_ReturnsOnlyMoodsInSpecifiedPeriod()
{
    var storage = new InMemoryStorage();
    var service = new MoodHistoryService(storage);
    
    var sevenDaysAgo = DateTime.Now.AddDays(-7);
    service.AddMood("happy", DateTime.Now);
    service.AddMood("sad", sevenDaysAgo.AddHours(1)); // Just inside range
    service.AddMood("angry", sevenDaysAgo.AddHours(-1)); // Just outside range
    
    var result = service.GetMoodHistory(DateRange.LastSevenDays).ToList();
    
    Assert.Equal(2, result.Count); // Only happy and sad, not angry
    Assert.Contains(result, m => m.Mood == "happy" && m.DateTime == DateTime.Now);
    Assert.Contains(result, m => m.Mood == "sad");
    Assert.DoesNotContain(result, m => m.Mood == "angry");
}

[Fact]
public void GetMoodHistory_WithEmptyPeriod_ReturnsEmptyList()
{
    var service = new MoodHistoryService(new InMemoryStorage());
    var result = service.GetMoodHistory(DateRange.LastSevenDays);
    
    Assert.Empty(result);
}
```

**Now Evil Can't Fake It:**
```csharp
public class MoodHistoryService
{
    public IEnumerable<MoodEntry> GetMoodHistory(DateRange period)
    {
        // Now must actually filter by date range
        var startDate = period.CalculateStartDate();
        return _storage.GetMoods()
            .Where(m => m.DateTime >= startDate && m.DateTime <= DateTime.Now)
            .ToList();
    }
}
```

### 2. **The Ignore-the-Parameter** (Evil Ignores What You Pass In)

**Your Test (Round 1 - Parameters Don't Matter):**
```csharp
[Fact]
public void RecordMood_WithAnyMood_UpdatesLastMoodDate()
{
    var service = new MoodService();
    service.RecordMood("happy");
    
    Assert.NotNull(service.LastMoodDate); // Doesn't verify the mood itself
}
```

**Evil's Implementation:**
```csharp
public class MoodService
{
    public void RecordMood(string mood)
    {
        LastMoodDate = DateTime.Now;
        // Ignores the mood parameter completely!
    }
}
```

**Your Test (Round 2 - Parameters Are Specifications):**
```csharp
[Fact]
public void RecordMood_StoresTheSpecificMoodProvided()
{
    var service = new MoodService();
    service.RecordMood("happy");
    
    Assert.Equal("happy", service.GetLastMood()); // Verify stored mood
}

[Fact]
public void RecordMood_WithDifferentMoods_StoresDifferentValues()
{
    var service = new MoodService();
    
    service.RecordMood("happy");
    Assert.Equal("happy", service.GetLastMood());
    
    service.RecordMood("anxious");
    Assert.Equal("anxious", service.GetLastMood());
}

[Fact]
public void RecordMood_WithInvalidMood_ThrowsArgumentException()
{
    var service = new MoodService();
    Assert.Throws<ArgumentException>(() => service.RecordMood(""));
    Assert.Throws<ArgumentException>(() => service.RecordMood(null));
}
```

---

### 3. **The Ignore-the-State** (Evil Ignores Current State)

**Your Test (Round 1 - Doesn't Test State Transitions):**
```csharp
[Fact]
public void IsWorkingHard_WithHighMood_ReturnsTrue()
{
    var analyzer = new WorkMoodAnalyzer();
    analyzer.SetCurrentMood("energized");
    
    var result = analyzer.IsWorkingHard();
    
    Assert.True(result); // Doesn't verify STATE changed
}
```

**Evil's Implementation:**
```csharp
public class WorkMoodAnalyzer
{
    public bool IsWorkingHard()
    {
        return true; // Always returns true, ignores state!
    }
}
```

**Your Test (Round 2 - State Matters):**
```csharp
[Fact]
public void IsWorkingHard_WithEnergizedMood_ReturnsTrue()
{
    var analyzer = new WorkMoodAnalyzer();
    analyzer.SetCurrentMood("energized");
    
    Assert.True(analyzer.IsWorkingHard());
}

[Fact]
public void IsWorkingHard_WithTiredMood_ReturnsFalse()
{
    var analyzer = new WorkMoodAnalyzer();
    analyzer.SetCurrentMood("tired");
    
    Assert.False(analyzer.IsWorkingHard());
}

[Fact]
public void IsWorkingHard_WithDifferentStates_ReturnsAppropriateValue()
{
    var analyzer = new WorkMoodAnalyzer();
    
    analyzer.SetCurrentMood("stressed");
    Assert.False(analyzer.IsWorkingHard()); // Stressed ≠ working hard
    
    analyzer.SetCurrentMood("focused");
    Assert.True(analyzer.IsWorkingHard()); // Focused = working hard
}
```

---

### 4. **The Ignore-Dependencies** (Evil Uses Wrong Dependencies)

**Your Test (Round 1 - Dependencies Unspecified):**
```csharp
[Fact]
public void SaveMood_PersistsToStorage()
{
    var mockStorage = new Mock<IDataStorage>();
    var service = new MoodDataService(mockStorage.Object);
    
    service.SaveMood("happy");
    
    // Doesn't verify if the right dependency was actually called!
}
```

**Evil's Implementation:**
```csharp
public class MoodDataService
{
    private IDataStorage _storage;
    
    public void SaveMood(string mood)
    {
        // Ignores the injected storage, uses hardcoded location
        File.WriteAllText("C:\\hardcoded\\mood.txt", mood);
    }
}
```

**Your Test (Round 2 - Verify Correct Dependencies Are Used):**
```csharp
[Fact]
public void SaveMood_CallsStorageWithMood()
{
    var mockStorage = new Mock<IDataStorage>();
    var service = new MoodDataService(mockStorage.Object);
    
    service.SaveMood("happy");
    
    mockStorage.Verify(x => x.Save("happy"), Times.Once); // Verify it was called
}

[Fact]
public void SaveMood_WithEmptyMood_DoesNotCallStorage()
{
    var mockStorage = new Mock<IDataStorage>();
    var service = new MoodDataService(mockStorage.Object);
    
    Assert.Throws<ArgumentException>(() => service.SaveMood(""));
    mockStorage.Verify(x => x.Save(It.IsAny<string>()), Times.Never); // Never called!
}

[Fact]
public void SaveMood_WithMultipleMoods_CallsStorageMultipleTimes()
{
    var mockStorage = new Mock<IDataStorage>();
    var service = new MoodDataService(mockStorage.Object);
    
    service.SaveMood("happy");
    service.SaveMood("sad");
    
    mockStorage.Verify(x => x.Save(It.IsAny<string>()), Times.Exactly(2));
}
```

---

### 5. **The Lazy Constant** (Evil Returns Convenient Constants)

**Your Test (Round 1 - Only Tests One Case):**
```csharp
[Fact]
public void CalculateStressLevel_WithHighWorkload_Returns80()
{
    var analyzer = new WorkMoodAnalyzer();
    var result = analyzer.CalculateStressLevel(highWorkload: true);
    
    Assert.Equal(80, result);
}
```

**Evil's Implementation:**
```csharp
public class WorkMoodAnalyzer
{
    public int CalculateStressLevel(bool highWorkload)
    {
        return 80; // Always returns 80!
    }
}
```

**Your Test (Round 2 - Test Boundary Values and Variations):**
```csharp
[Fact]
public void CalculateStressLevel_WithHighWorkload_ReturnsHighValue()
{
    var analyzer = new WorkMoodAnalyzer();
    var result = analyzer.CalculateStressLevel(highWorkload: true);
    
    Assert.InRange(result, 70, 100); // High but covers range
}

[Fact]
public void CalculateStressLevel_WithLowWorkload_ReturnsLowValue()
{
    var analyzer = new WorkMoodAnalyzer();
    var result = analyzer.CalculateStressLevel(highWorkload: false);
    
    Assert.InRange(result, 0, 30); // Must be different!
}

[Fact]
public void CalculateStressLevel_WithOppositeInputs_ReturnsDifferentValues()
{
    var analyzer = new WorkMoodAnalyzer();
    var highStress = analyzer.CalculateStressLevel(highWorkload: true);
    var lowStress = analyzer.CalculateStressLevel(highWorkload: false);
    
    Assert.NotEqual(highStress, lowStress); // Can't return same value
}
```

---

### 6. **The Off-by-One** (Evil Gets The Boundary Wrong)

**Your Test (Round 1 - Loose Boundaries):**
```csharp
[Fact]
public void GetConsecutiveMoods_WithSevenDays_ReturnsData()
{
    var service = new MoodHistoryService();
    service.AddMood("happy", DateTime.Now);
    service.AddMood("sad", DateTime.Now.AddDays(-6)); // 6 days ago
    
    var result = service.GetConsecutiveMoods(dayCount: 7);
    
    Assert.Equal(2, result.Count()); // Off-by-one not caught!
}
```

**Evil's Implementation:**
```csharp
public IEnumerable<MoodEntry> GetConsecutiveMoods(int dayCount)
{
    var cutoff = DateTime.Now.AddDays(-(dayCount - 1)); // Wrong boundary!
    return _moods.Where(m => m.DateTime >= cutoff);
}
```

**Your Test (Round 2 - Explicit Boundaries):**
```csharp
[Fact]
public void GetConsecutiveMoods_WithSevenDays_IncludesExactlySevenDaysBack()
{
    var service = new MoodHistoryService();
    var now = DateTime.Now;
    var sevenDaysAgo = now.AddDays(-7);
    
    service.AddMood("happy", now);
    service.AddMood("sad", sevenDaysAgo.AddSeconds(1)); // Just inside boundary
    service.AddMood("angry", sevenDaysAgo.AddSeconds(-1)); // Just outside
    
    var result = service.GetConsecutiveMoods(dayCount: 7).ToList();
    
    Assert.Equal(2, result.Count); // Only happy and sad
    Assert.Contains(result, m => m.Mood == "sad");
    Assert.DoesNotContain(result, m => m.Mood == "angry");
}

[Fact]
public void GetConsecutiveMoods_WithOnDay_ReturnsOnlyToday()
{
    var service = new MoodHistoryService();
    var now = DateTime.Now;
    
    service.AddMood("happy", now.AddSeconds(1)); // Today
    service.AddMood("sad", now.AddDays(-1)); // Yesterday
    
    var result = service.GetConsecutiveMoods(dayCount: 1).ToList();
    
    Assert.Single(result);
    Assert.Equal("happy", result.First().Mood);
}
```

---

## How to Use Evil TDD in Your Team

### Setup
1. **Pair with a partner**: One writes tests, one writes code
2. **Take turns**: Rotate who's the "test writer" and "code writer"
3. **Strictly follow roles**: Don't peek at each other's work until round is done

### The Cycle

1. **RED**: Test writer writes a test that fails
2. **EVIL GREEN**: Code writer writes the worst code that passes the test
3. **REVIEW**: Test writer reviews the evil code and recognizes the loopholes
4. **IMPROVE TEST**: Test writer strengthens the test to prevent the evil hack
5. **REAL GREEN**: Code writer now writes proper code that passes ironclad tests
6. **REFACTOR**: Both optimize implementation and test code
7. **REPEAT**: Next feature, or swap roles

### Example Session (45 minutes)

**Person A (Test Writer) - Round 1:**
```csharp
[Fact]
public void GetMoodTrendForWeek_WithFiveEntries_CalculatesTrend()
{
    var service = new MoodAnalysisService();
    var moods = new[] { 3, 4, 5, 6, 7 }; // Ascending
    
    var trend = service.GetMoodTrendForWeek(moods);
    
    Assert.True(trend > 0); // Positive trend
}
```

**Person B (Evil Code Writer) - First Attempt:**
```csharp
public double GetMoodTrendForWeek(int[] moods)
{
    return 1.0; // Always returns positive! Passes the test!
}
```

**Person A's Response:**
"Got me! Your implementation doesn't actually calculate the trend."

**Person A (Improved Test):**
```csharp
[Fact]
public void GetMoodTrendForWeek_WithDescendingMoods_CalculatesNegativeTrend()
{
    var service = new MoodAnalysisService();
    var moods = new[] { 7, 6, 5, 4, 3 };
    
    var trend = service.GetMoodTrendForWeek(moods);
    
    Assert.True(trend < 0); // Must be negative here!
}

[Fact]
public void GetMoodTrendForWeek_WithAscendingVsDescending_ReturnsDifferentTrends()
{
    var service = new MoodAnalysisService();
    
    var ascending = service.GetMoodTrendForWeek(new[] { 3, 4, 5 });
    var descending = service.GetMoodTrendForWeek(new[] { 5, 4, 3 });
    
    Assert.NotEqual(ascending, descending);
    Assert.True(ascending > descending);
}

[Fact]
public void GetMoodTrendForWeek_WithFlatMoods_CalculatesZeroTrend()
{
    var service = new MoodAnalysisService();
    var moods = new[] { 5, 5, 5, 5 };
    
    var trend = service.GetMoodTrendForWeek(moods);
    
    Assert.Equal(0, trend);
}
```

**Person B (Real Code) - Now Forced to Implement Correctly:**
```csharp
public double GetMoodTrendForWeek(int[] moods)
{
    if (moods.Length < 2) return 0;
    
    var firstHalf = moods.Take(moods.Length / 2).Average();
    var secondHalf = moods.Skip(moods.Length / 2).Average();
    
    return secondHalf - firstHalf; // Or any real trend calculation
}
```

---

## Why Evil TDD Forces Better Design

| Traditional TDD | Evil TDD |
|---|---|
| Write test, write code, refactor | Write test, evil code reveals loopholes, strengthen test, real code, refactor |
| Tests might miss edge cases | Tests debugged against malicious implementation |
| Parameters might be ignored silently | Tests forced to verify parameters matter |
| State transitions might not be tested | Evil implementation reveals missing state tests |
| Boundary conditions often missed | Off-by-one forced into open by evil boundary tests |
| Can pass with sloppy assertions | Loose assertions get destroyed by evil code |

---

## The Learning Opportunity

When evil code breaks your test:
- **Don't blame the code writer** — they revealed a gap in your test
- **Appreciate the insight** — see what behavior you failed to specify
- **Write better tests** — tests that can only pass with correct implementation
- **Understand TDD** — tests are specifications, not just documentation

Each evil implementation teaches you to think like:
- A specification writer (what does this really need to do?)
- A quality assurance engineer (how do I verify it works?)
- A designer (what are all the ways this could go wrong?)

---

## Anti-Patterns to Avoid in Evil TDD

| Anti-Pattern | Why It Fails |
|---|---|
| **The Code Writer Goes Too Evil** | Writing code that's technicallyIncorrect (throws exceptions, corrupts data) instead of just insufficiently specific | Tests should fail because they didn't specify behavior, not because code is malicious |
| **The Test Writer Gives Up** | Writing tests so complex they're unmaintainable instead of precise | Tests should be clarity and simplicity—if they're hard to write, something is wrong |
| **Silent Swaps** | Code writer secretly writes good code instead of evil code | The whole point is revealing loopholes—non-evil code defeats the purpose |
| **Test Writer Tests Implementation** | Writing tests that verify internal state instead of behavior | Tests internal details that evil code can satisfy while doing wrong thing |
| **No Refactoring** | Keeping test and code messy after green | Refactoring is where you clean up and solidify the design |

---

## When to Use Evil TDD

**Great for:**
- Complex behavioral logic
- Specifications that are easy to misunderstand
- Learning proper test specification
- Code reviews with junior developers
- Building shared understanding of "what correct looks like"

**Less suitable for:**
- Simple CRUD operations
- UI integration tests (too many moving parts)
- Performance-critical code (doesn't help with optimization)
- Existing code with weak tests (start with real TDD first)

---

## See Also

- [TDD Skill](./tdd/SKILL.md) - Standard Test-Driven Development practice
- [Test Organization & Hierarchy Skill](./test-organization-hierarchy/SKILL.md) - Organize tests rationally as they grow
- [Code Smells Detection Skill](./code-smells-detection/SKILL.md) - Recognize when designs are problematic
