---
name: legacy-code-rescue
description: >
  Safe, incremental strategies for improving legacy code in WorkMood and similar .NET/MAUI projects. Covers characterization tests, seams, refactoring, and risk management for untested or complex code.
user-invokable: true
argument-hint: "[code context] [legacy rescue goal]"
disable-model-invocation: false
---

# Legacy Code Rescue Skill

## When to Use This Skill

Use this skill when:
- You need to change or refactor code with little or no test coverage
- The code is hard to understand, risky, or tightly coupled
- You want to add tests to legacy code safely
- You need to introduce seams for dependency injection or testability

---


## Rescue Approach

- **Characterization Tests**: Write tests that capture current behavior before changing anything.
- **Introduce Seams**: Add interfaces, delegates, or factories to break dependencies and enable testing.
- **Peel and Slicing Technique**: Gradually peel away or slice off small, testable pieces of logic from the legacy code. Move these into new, well-tested methods or classes, leaving the original code smaller and easier to understand. Repeat until the legacy code is fully replaced or manageable.
- **Incremental Refactoring**: Make one small, safe change at a time; verify tests after each.
- **Safe Commits**: Commit after every passing test and refactor step.
- **Risk Management**: Avoid large rewrites; prefer gradual improvement.

---


## Rescue Patterns

- **Characterization Test Example**:
  ```csharp
  [Fact]
  public void ShouldBehaveAsBefore_WhenInputIsX()
  {
      // Arrange: set up legacy object
      // Act: call legacy method
      // Assert: verify current (even if odd) behavior
  }
  ```
- **Seam Introduction**: Use interfaces or delegates to inject dependencies.
- **Peel and Slicing Example**: Identify a small, self-contained piece of logic in a large method. Extract it into a new method or class, write tests for it, and replace the original code with a call to the new method. Repeat this process to gradually reduce legacy complexity.
- **Sprout Method/Class**: Add new code alongside old, then migrate usage.
- **Wrap & Delegate**: Use adapter or wrapper to intercept and test legacy logic.

---

## Example Prompts

- "/legacy-code-rescue [file.cs] Add characterization tests before refactoring."
- "/legacy-code-rescue [method] How do I introduce a seam for DI?"
- "/legacy-code-rescue [class] What’s the safest way to start refactoring?"

---

## Verification Steps

- Build: `dotnet build --framework net9.0-windows10.0.19041.0`
- Test: `dotnet test` (ensure new and old behavior is covered)
- Commit: After every passing test and refactor step

---

## Resources

- [Refactoring Skill](../refactoring/SKILL.md)
- [TDD Skill](../tdd/SKILL.md)
- [Code Smells Detection](../code-smells-detection/SKILL.md)
- [Working Effectively with Legacy Code (Feathers)](https://www.goodreads.com/book/show/44919.Working_Effectively_with_Legacy_Code)

---

## Maintenance

- Update as new rescue patterns or project-specific strategies emerge.
- Ensure examples and checklists match current best practices.
