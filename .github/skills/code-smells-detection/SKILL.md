---
name: code-smells-detection
description: Identify and address code smells in WorkMood. Teaches pattern recognition for long methods, inappropriate intimacy, magic values, duplicate logic, and MVVM violations. Guides incremental refactoring with safety-first approach.
argument-hint: "[code snippet or method] [context/concern]"
user-invokable: true
disable-model-invocation: false
---

# Code Smells Detection Skill

## When to Use This Skill

Use this skill when:
- You're reviewing code and sense something is "off" but can't articulate why
- You notice repeated patterns or logic across files
- A method is doing multiple things or has growing complexity
- Tests are hard to write for a particular piece of code
- You're unsure if refactoring is justified (smell vs. false positive)
- You want to identify *potential* problems before they compound

**This skill teaches recognition, not just fixes.** Understanding why something is a smell prevents creating new ones during refactoring.

## Philosophy: Why Code Smells Matter in XP

Martin Fowler defined code smells as "surface-level indicators of deeper design problems." In Extreme Programming, you **refactor continuously** because smells are where refactoring opportunities live.

**Unchecked smells lead to:**
- Harder-to-test code
- Scattered concerns and tangled logic
- Slower feature implementation (more code to understand)
- Higher bug density (duplicate logic = duplicate bugs)

## WorkMood-Specific Code Smells

### 1. **Long Methods**

**Recognition Pattern:**
- Method > 20 lines (especially in ViewModels, Services)
- Mixed levels of abstraction (some lines handle UI, others do business logic)
- Nested conditionals (read like paragraphs, not scannable)
- Single responsibility becomes ambiguous by line 15+

**Common in WorkMood:**
```csharp
public async Task LoadMoodDataAsync()
{
    // Validation logic
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("User ID required");
    
    // Service call
    var data = await _moodService.GetAsync(userId);
    
    // Data transformation
    var viewModel = new MoodViewModel
    {
        AverageMood = data.Average(),
        TrendIndicator = CalculateTrend(data),
        // ... 15 more lines
    };
    
    // UI state updates
    // ... more logic
}
```

**Why It's a Problem:**
- Hard to test individual concerns
- Reusing logic requires copying code
- One failure point makes the whole method fail

**Refactoring Path:**
Use the `/refactoring` skill with "Extract method" patterns:
1. Extract validation → `ValidateUserIdAsync()`
2. Extract transformation → `TransformToViewModelAsync(data)`
3. Extract UI state → `UpdateMoodDisplayAsync(viewModel)`

Each method now does one thing, each can be tested independently.

---

### 2. **Inappropriate Intimacy**

**Recognition Pattern:**
- Directly accessing another object's private fields or internal state
- Reaching through multiple layers (ViewModel → ViewModel → Model)
- Brittle dependencies on internal implementation details
- Changes to one class force changes in another

**Common in WorkMood:**
```csharp
// In MoodDisplayViewModel
public void UpdateVisualization()
{
    var config = _scheduleService._internalConfig; // ❌ Private field access
    var rawData = _moodService._moodCache.Where(m => m > config.Min); // ❌ Intimate knowledge
}

// In a Page or another ViewModel
var moodData = _moodViewModel._moodList; // ❌ Accessing internal list
foreach (var mood in moodData)
{
    mood.IsSelected = true; // Direct mutation of model
}
```

**Why It's a Problem:**
- Breaks encapsulation; internal implementations become contract
- Services can't refactor internals without breaking clients
- Hard to test because dependencies are implicit and tangled
- Difficult to mock or substitute implementations

**Refactoring Path:**
1. Expose behavior through public methods, not data
2. Use dependency injection to decouple implementation details
3. Add interface barriers where needed

```csharp
// Better approach
public void UpdateVisualization()
{
    var data = _moodService.GetMoodDataInRange(_scheduleService.GetTimeRange());
    // Now using public contracts, not private implementation
}
```

---

### 3. **Duplicate Logic (DRY Violations)**

**Recognition Pattern:**
- Same 3+ lines of code appear in multiple places
- Similar-looking methods with small variations
- Copy-paste followed by minor edits
- Bug fix in one place requires identical fix elsewhere

**Common in WorkMood:**
```csharp
// In MoodService
public async Task<double> GetAverageMoodAsync(DateRange range)
{
    var moods = _repository.GetMoods(range.Start, range.End);
    if (!moods.Any())
        return 0;
    return moods.Average(m => m.Value);
}

// In GraphService (nearly identical)
public async Task<double> CalculateDisplayAverageAsync(DateRange range)
{
    var moods = _repository.GetMoods(range.Start, range.End);
    if (!moods.Any())
        return 0;
    return moods.Average(m => m.Value);
}
```

**Why It's a Problem:**
- Changes must be made in multiple places (maintenance burden)
- Easy to miss a location, causing inconsistency bugs
- Logic diverges over time as one place evolves differently
- Higher review burden and cognitive load

**Refactoring Path:**
1. Extract to shared service method
2. Or create utility method both can call
3. Or use higher-order function/strategy pattern

```csharp
// Single source of truth
public async Task<double> CalculateAverageAsync(IEnumerable<Mood> moods)
{
    var moodList = moods.ToList();
    return moodList.Any() ? moodList.Average(m => m.Value) : 0;
}

// Both services call it
public async Task<double> GetAverageMoodAsync(DateRange range)
{
    var moods = _repository.GetMoods(range.Start, range.End);
    return await CalculateAverageAsync(moods);
}
```

---

### 4. **Magic Numbers / Magic Strings**

**Recognition Pattern:**
- Unexplained constants scattered in code
- Numbers without context (31, 100, -1, 0.5)
- Strings that represent meaningful states ("PENDING", "CRITICAL")
- Same magic value used in multiple places
- Hard to understand "why" this particular number

**Common in WorkMood:**
```csharp
// In MoodService
public bool IsMoodExtreme(int moodValue)
{
    return moodValue > 8 || moodValue < 2; // What do 8 and 2 represent?
}

// In GraphService
if (moodValue > 8) // Same magic number, different place
{
    chart.HighlightPoint();
}

// In Schedule processor
var threshold = 31; // Days? Minutes? Smells of magic...
if (dayCount > threshold)
{
    ResetData();
}
```

**Why It's a Problem:**
- Future maintainers don't understand the intent
- Changing the threshold means hunting through code
- Easy to use wrong value in similar context
- Businesspeople can't grasp the logic

**Refactoring Path:**
1. Extract to named constant with business meaning
2. Or move to configuration/settings
3. Add comment explaining "why" if not obvious

```csharp
// Clear intent
private const int EXTREME_HIGH_MOOD_THRESHOLD = 8;
private const int EXTREME_LOW_MOOD_THRESHOLD = 2;
private const int ARCHIVE_AFTER_DAYS = 31;

public bool IsMoodExtreme(int moodValue)
{
    return moodValue >= EXTREME_HIGH_MOOD_THRESHOLD || 
           moodValue <= EXTREME_LOW_MOOD_THRESHOLD;
}
```

---

### 5. **MVVM Violations (Mixed Concerns)**

**Recognition Pattern:**
- ViewModel contains UI-specific logic (formatting, visibility rules)
- View directly accesses Service (should go through ViewModel)
- Model contains business logic (should be in ViewModel/Service)
- Data transformation mixed with state management
- Direct UI manipulation in ViewModels

**Common in WorkMood:**
```csharp
// ❌ Bad: UI logic in ViewModel
public class MoodViewModel : ViewModelBase
{
    public string FormattedMood => _currentMood > 7 ? 
        $"🎉 Great ({_currentMood})" : 
        $"😔 Struggling ({_currentMood})"; // UI formatting in ViewModel
    
    public bool IsHighMood => _currentMood > 7; // View state, not model state
    
    public void ProcessMood()
    {
        var html = $"<html><body>{_currentMood}</body></html>"; // ❌ UI generation
    }
}

// ❌ Bad: View accessing Service directly
public partial class MoodPage : ContentPage
{
    public MoodPage()
    {
        _service = App.ServiceProvider.GetService<IMoodService>(); // ❌ Direct service access
    }
}
```

**Why It's a Problem:**
- ViewModel becomes bloated and hard to test
- Difficult to change UI presentation without touching ViewModel
- View logic scattered makes refactoring risky
- Dependency injection doesn't work properly

**Refactoring Path:**
```csharp
// ✅ Better: Separate Concerns
// ViewModel holds data and state
public class MoodViewModel : ViewModelBase
{
    public int CurrentMood { get; set; }
}

// Converter handles UI presentation
public class MoodDisplayConverter : IValueConverter
{
    public object Convert(object value, ...) =>
        (int)value > 7 ? "🎉 Great" : "😔 Struggling";
}

// Page only binds, doesn't call services
public partial class MoodPage : ContentPage
{
    public MoodPage(MoodViewModel viewModel)
    {
        BindingContext = viewModel; // Dependency injected
    }
}
```

---

### 6. **Comments That Explain What the Code Does**

**Recognition Pattern:**
```csharp
// ❌ This comment explains WHAT (redundant with code)
// Increment the counter
counter++;

// ❌ This explains implementation details
// Loop through the moods and sum them, then divide by count
var average = moods.Sum() / moods.Count();
```

**Why It's a Smell:**
- If code needs a comment to explain what it does, it's not clear enough
- Comments get stale and contradict reality
- Better solution: rename or restructure code

**Better Approach (The Smell's Fix):**
```csharp
// ✅ Clear method name, no comment needed
IncrementMoodCount();

// ✅ Clear method name for complex logic
var average = CalculateMoodAverage(moods);

public double CalculateMoodAverage(IEnumerable<Mood> moods)
{
    const int safety = moods.Count();
    return safety == 0 ? 0 : moods.Sum() / safety;
}
```

---

### 7. **God Classes / God ViewModels**

**Recognition Pattern:**
- Class does multiple unrelated things
- ViewModel handles mood data, visualization, schedule, AND settings
- Large constructor with many dependencies
- Grows without clear boundaries
- Methods have nothing to do with each other

**Common in WorkMood:**
```csharp
// ❌ God ViewModel doing too much
public class DashboardViewModel
{
    private IMoodService _moodService;
    private IVisualizationService _vizService;
    private IScheduleService _scheduleService;
    private ISettingsService _settingsService;
    private INavigationService _navService;
    
    // Methods for mood management
    public async Task LoadMoodsAsync() { }
    public async Task SaveNewMoodAsync() { }
    
    // Methods for visualization
    public async Task RenderGraphAsync() { }
    public void UpdateChartColors() { }
    
    // Methods for schedule
    public void ApplyScheduleSettings() { }
    
    // And more...
}
```

**Why It's a Problem:**
- Impossible to test in isolation
- Changes in one area affect unrelated code
- Cognitive overload when modifying
- Violates Single Responsibility Principle

**Refactoring Path:**
Split into focused ViewModels:
- `MoodHistoryViewModel`
- `MoodVisualizationViewModel`
- `ScheduleAdjustmentViewModel`
- Compose them in a parent ViewModel if needed

---

## Smell Assessment Checklist

When you suspect a smell, ask:

- **Is this hard to test?** (Likely a smell)
- **Does changing it require changes elsewhere?** (Inappropriate intimacy, duplication)
- **Would you explain this to a junior dev?** (Comments explaining what, magic values)
- **Is this method trying to do multiple things?** (Long method, God class)
- **Does the name accurately describe what it does?** (If not, rename or refactor)
- **Would I copy-paste this code?** (If yes, extract instead)

## When NOT to Refactor

**False Positives—These Are NOT Smells:**

1. **Stable code in heavy use** — If it works, is tested, and rarely changes, leave it
2. **Intentionally complex algorithms** — Some algorithms are legitimately complex (graphics rendering, complex calculations); clarity > simplicity
3. **Framework-mandated patterns** — MAUI requires certain ceremony; that's not a smell
4. **Performance-critical sections** — Sometimes clarity is sacrificed for speed (document why)
5. **Temporary scaffolding** — Code meant to be replaced soon doesn't need refactoring

## Next Steps After Identifying a Smell

1. **Write a test** that currently passes (validates current behavior)
2. **Use `/refactoring` skill** for guidance on safe refactoring
3. **Make one small change** at a time
4. **Verify tests still pass**
5. **Commit with `^r` notation** as per Arlo's convention

Remember: **Identifying a smell is the first step to improvement.** Not all smells must be fixed immediately, but recognizing them enables intentional, safe refactoring over time.
