---
name: test-data-builder
description: >
  Patterns and guidance for building maintainable, reusable test data in WorkMood and .NET/MAUI projects. Covers Test Data Builder, Data Mother, Object Mother, and fluent builder patterns for robust, DRY, and readable tests.
user-invokable: true
argument-hint: "[test subject] [data scenario]"
disable-model-invocation: false
---

# Test Data Builder Skill

## When to Use This Skill
- Writing or refactoring tests that require complex or varied data
- Reducing duplication in test setup
- Making tests more readable, maintainable, and robust
- Sharing test data setup across multiple test classes

---

## Core Concepts

- **Test Data Builder Pattern**: Create builder classes with fluent APIs to construct test objects with default values, allowing overrides for specific scenarios.
- **Data Mother/Object Mother**: Centralize creation of common test data (e.g., `MoodDataMother.Valid()`, `UserMother.WithAdminRole()`).
- **Fluent Builder**: Chain methods for clarity (e.g., `.WithMood("Happy").WithDate(today)`).
- **Immutability**: Prefer immutable test data to avoid side effects.
- **DRY Principle**: Avoid copy-pasting test setup; use builders/mothers for reuse.

---

## How-To: Create a Test Data Builder

1. **Identify a frequently used test object** (e.g., `MoodEntry`, `GraphData`).
2. **Create a builder class** (e.g., `MoodEntryBuilder`) with sensible defaults.
3. **Add fluent methods** to override properties:
   ```csharp
   public class MoodEntryBuilder {
       private DateTime _date = DateTime.Today;
       private string _mood = "Happy";
       public MoodEntryBuilder WithDate(DateTime date) { _date = date; return this; }
       public MoodEntryBuilder WithMood(string mood) { _mood = mood; return this; }
       public MoodEntry Build() => new MoodEntry(_date, _mood);
   }
   ```
4. **Use in tests**:
   ```csharp
   var entry = new MoodEntryBuilder().WithMood("Sad").Build();
   ```

---

## How-To: Data Mother Pattern

- Create a static class (e.g., `MoodDataMother`) with methods for common scenarios:
  ```csharp
  public static class MoodDataMother {
      public static MoodEntry Valid() => new MoodEntryBuilder().Build();
      public static MoodEntry WithMood(string mood) => new MoodEntryBuilder().WithMood(mood).Build();
      public static List<MoodEntry> TypicalWeek() => new List<MoodEntry> {
          new MoodEntryBuilder().WithDate(DateTime.Today.AddDays(-1)).WithMood("Happy").Build(),
          new MoodEntryBuilder().WithDate(DateTime.Today).WithMood("Sad").Build(),
          // ...more entries
      };
  }
  ```
- Use in tests for clarity and DRYness.

---

## Patterns & Checklist
- [ ] Use builders for all complex test objects
- [ ] Centralize common data in Data Mother/Object Mother
- [ ] Prefer fluent, readable APIs
- [ ] Avoid magic values in tests—use named methods/properties
- [ ] Keep test data immutable where possible
- [ ] Refactor duplicated test setup into builders/mothers

---

## Anti-Patterns/When NOT to Use
- Avoid builders for trivial objects (simple value types)
- Don’t over-engineer: start simple, extract builders/mothers as duplication grows
- Don’t use static mutable data (risk of test pollution)

---

## References
- [xUnit Test Patterns: Test Data Builders](http://xunitpatterns.com/Test%20Data%20Builder.html)
- [Data Mother vs. Object Mother](https://martinfowler.com/bliki/ObjectMother.html)
- [WorkMood Test Examples](../tdd/SKILL.md)
