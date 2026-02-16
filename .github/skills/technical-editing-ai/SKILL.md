---
name: technical-editing-ai
description: Edit documentation, instructions, and specifications for AI/LLM consumption. Focuses on precision, unambiguity, and completeness—ensuring AI systems reliably understand intent and constraints. Covers explicit constraints, measurable criteria, eliminating ambiguity, and conditional logic.
argument-hint: "[instruction section or file] [clarity goal/concern]"
user-invokable: true
disable-model-invocation: false
---

# Technical Editing for AI Use

## Overview

This skill guides editing documentation, instructions, and specifications for AI/LLM consumption. Focus is on precision, unambiguity, and completeness—ensuring AI systems can reliably understand intent and constraints.

**Apply this skill when:**
- Writing or refining prompt instructions and system prompts
- Documenting architecture for code generation
- Creating specifications for AI-assisted development
- Editing `.github/copilot-instructions.md` and skill files
- Clarifying requirements that control AI behavior

---

## Core Principles

### 1. **Explicit Over Implicit**
- State assumptions explicitly
- Define all specialized terms
- Specify edge cases and exceptions
- Never rely on "common knowledge" or context the AI might miss

### 2. **Structural Clarity**
- Use consistent formatting (headers, lists, code blocks)
- Logical grouping with clear transitions
- Numbered steps for sequences
- Consistent terminology throughout

### 3. **Completeness & Constraints**
- **What to do**: Clear success criteria
- **What NOT to do**: Explicit constraints and anti-patterns
- **When to apply**: Scope and trigger conditions
- **Why it matters**: Reasoning behind rules

### 4. **Precision in Language**
- Specific verbs (not "handle", "process", "deal with")
- Quantifiers matter ("always", "never", "typically", "may")
- Technical accuracy over fluency
- Avoid ambiguous pronouns

---

## Common Editing Tasks for AI Instructions

### Adding Explicit Constraints

**Before:**
```
When refactoring code, keep tests passing.
```

**After:**
```
When refactoring code:
1. Run all affected tests BEFORE making changes (baseline)
2. Make ONE atomic change
3. Run all affected tests AFTER the change
4. If tests fail, revert and reassess
5. Never combine refactoring with feature changes
6. Document the risk level using Arlo's notation
```

*What changed:*
- Turn implicit "keep passing" into explicit sequential steps
- Add constraints ("ONE atomic change", "Never combine")
- Include verification at each stage

---

### Clarifying Ambiguous Directives

**Before:**
```
Follow the MVVM pattern when creating ViewModels.
```

**After:**
```
When creating ViewModels:
- Inherit from ViewModelBase (ensures consistent command handling)
- NO direct UI references (use data binding instead)
- NO platform-specific code (inject services for platform work)
- Inject dependencies in constructor (enable testability)
- Use RelayCommand for user interactions
- Implement INotifyPropertyChanged for binding updates
- Keep logic simple; move complex operations to services
- Test with mocked service dependencies

Example (good):
- ViewModel has `IMoodDataService` injected
- ViewModel exposes `ICommand RecordMoodCommand`
- ViewModel updates binding via `OnPropertyChanged()`

Example (bad):
- ViewModel directly calls File.ReadAllText()
- ViewModel contains complex business logic
- ViewModel references UI elements
```

*What changed:*
- Expanded into specific DO/DON'T rules
- Added inheritance requirement (why: consistency)
- Included example patterns (good and bad)
- Made testable and verifiable

---

### Defining Success Criteria

**Before:**
```
Refactoring should improve code quality.
```

**After:**
```
A refactoring is successful when:
1. ✅ Build completes without errors
2. ✅ All existing tests pass (same count, same assertions)
3. ✅ Behavior is identical to before (verified manually or by test)
4. ✅ Cyclomatic complexity of affected method decreases
5. ✅ Risk assessment assigned per Arlo's notation
6. ✅ Commit message follows format: [Risk][Intention] - [Description]

A refactoring is NOT successful if:
- ❌ Any test fails or is removed
- ❌ Functionality changes
- ❌ New dependencies introduced
- ❌ Code is less readable
```

*What changed:*
- Objective, verifiable criteria (not subjective "quality")
- Numbered checklist (AI can verify each item)
- Explicit rejection criteria (what fails the refactoring)

---

### Specifying Conditional Logic

**Before:**
```
Only refactor well-tested code.
```

**After:**
```
Only refactor when:
- ✅ Target code has test coverage ≥ 80% (measurable)
- ✅ Tests are NOT brittle (mock internal state, not external calls)
- ✅ Tests document expected behavior clearly

Do NOT refactor if:
- ❌ Code is legacy with zero tests
- ❌ Tests use PrivateObject or reflection to bypass encapsulation
- ❌ Tests are integration-only (no unit test safety net)
- ❌ Feature request pending (refactoring + feature = high risk)

When uncertainty exists:
- Ask the user: "I found 40% test coverage. Should I proceed?"
- Document assumptions you're making
```

*What changed:*
- Added measurable thresholds (80%, not "good")
- Explicit conditions for SKIP
- Handling of edge cases (uncertainty)
- Transparency about assumptions

---

### Sequence vs. Choice

**Before (ambiguous):**
```
Refactor using the shim factory pattern when applicable.
```

**After (sequential):**
```
When refactoring for dependency injection:

1. Identify the dependency (concrete class being created)
2. Search for all uses of that class in the file
3. Decide: Is this a good abstraction boundary?
   - YES → Proceed to step 4
   - NO → Refactor location instead (extract method first)
4. Create interface `I[ClassName]` defining the contract
5. Create shim factory: `IServiceProvider.GetRequiredService<I[ClassName]>()`
6. Inject factory in constructor instead of concrete class
7. Update ctor: receive `IServiceProvider` parameter
8. At instantiation point: replace `new ClassName()` with `factory.GetRequiredService<I[ClassName]>()`
9. Run tests; verify behavior identical
10. Commit with risk assessment

When to SKIP this pattern:
- ❌ Primitive types (string, int, DateTime)
- ❌ Immutable value objects without side effects
- ❌ One-off throwaway objects with no reuse
- ❌ Performance-critical paths (measure first)
```

*What changed:*
- Clear fork in the path (step 3 decision point)
- Explicit when to skip the pattern
- Each step is testable and verifiable

---

## Editing Checklist for AI Instructions

- [ ] **Zero Ambiguity**: Could different AIs interpret this differently?
- [ ] **Explicit Constraints**: Are all edge cases and exclusions stated?
- [ ] **Measurable Success**: Can success be objectively verified?
- [ ] **Complete Scope**: What triggers this? When does it stop?
- [ ] **Examples**: Do good and bad examples reinforce the rules?
- [ ] **No Pronouns**: Is there a referent for every "it", "this", "that"?
- [ ] **Technical Accuracy**: Are facts verifiable and current?
- [ ] **Verifiable Terms**: No fuzzy language ("improve", "better", "clean")

---

## Language Precision for AI

### Quantifiers
| Avoid | Use Instead |
|-------|-------------|
| "a few" | "1-3" or "less than 5" |
| "reasonable" | "< 50ms" or "fits in memory" |
| "appropriate" | specific criteria |
| "usually", "typically" | actual percentage or condition |

### Verbs (Be Specific)
| Avoid | Use Instead |
|-------|-------------|
| "handle" | create, validate, transform, delete, route |
| "process" | parse, execute, calculate, store, retrieve |
| "check" | verify, test, validate, assert, compare |
| "manage" | create, update, delete, organize, retrieve |

### Conditional Language
| Avoid | Use Instead |
|-------|-------------|
| "if possible" | specific condition (if X support Y, else Z) |
| "as needed" | explicit trigger (when Y > threshold, when input is null) |
| "if appropriate" | measurable criteria |

---

## WorkMood-Specific Formatting

### For Instructions
```markdown
## [Clear Title: Verb + Object]

### When to Apply
- Context or trigger for this instruction
- Scope (which components, which scenarios)

### Steps (if sequential)
1. Action 1
2. Action 2
   - Sub-condition A → proceed to step 3a
   - Sub-condition B → proceed to step 3b
3. Verification

### What NOT to Do (Constraints)
- ❌ Explicit anti-patterns
- ❌ Common mistakes
- ❌ Out-of-scope actions

### Examples
**Good**: [Code or pattern to follow]
**Bad**: [Anti-pattern or mistake]

### Success Criteria
- ✅ Verifiable outcome 1
- ✅ Verifiable outcome 2
```

### For Architecture Docs
```markdown
## [Component/Pattern Name]

### Responsibility
Single sentence: what does this do?

### Interface
```csharp
// Contract the AI should understand
```

### Implementation Pattern
Concrete example from WorkMood

### When to Use
Conditions triggering this pattern

### When NOT to Use
Anti-patterns or exclusions

### Dependencies
What this relies on (explicit list)
```

---

## Common Pitfalls in AI Instructions

### ❌ Assuming Context
```
Use the same pattern as DataMigrationService.
```
→ ✅ **Fix**: Link to the file and quote the pattern explicitly

### ❌ Vague Success Criteria
```
Write readable, maintainable tests.
```
→ ✅ **Fix**: "Each test verifies one behavior. Test name matches pattern `WhenContextExpectResult`."

### ❌ Implicit Constraints
```
Don't over-engineer.
```
→ ✅ **Fix**: "Don't add interfaces unless 2+ implementations exist or injection is needed."

### ❌ Ambiguous Pronouns
```
Update the service and test it.
```
→ ✅ **Fix**: "Update IMoodDataService interface and IDataMigrationService. Write new tests matching the pattern in DataMigrationServiceShould.cs."

### ❌ Missing Edge Cases
```
Validate user input.
```
→ ✅ **Fix**: "Validate input for: null, empty string, whitespace-only, negative numbers, dates in future. Specify what error to return for each."

---

## How to Request Edits Using This Skill

**In Copilot Chat:**
```
/technical-editing-ai Review the DataMigrationService refactoring instructions 
in SKILL.md. Are there ambiguous terms or missing edge cases?

/technical-editing-ai Edit this prompt to be more explicit about when 
to apply the shim factory pattern.

/technical-editing-ai Make this success criteria verifiable: "Write good tests"
```

---

## References
- _Thinking, Fast and Slow_ by Kahneman (decision clarity)
- _Precision_ by Suchman (shared understanding)
- WorkMood `.github/copilot-instructions.md` (applying this skill)
- WorkMood skills directory (examples of AI instruction precision)
