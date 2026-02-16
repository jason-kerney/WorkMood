---
name: refactoring
description: Safe, incremental refactoring guidance for WorkMood. Covers extract method, shim factory pattern, extract interface, DRY principles, and code smell fixes. Ensures tests pass before and after changes with zero functional modifications.
argument-hint: "[code context] [refactoring goal]"
user-invokable: true
disable-model-invocation: false
---

# Refactoring Skill

## When to Use This Skill

Use this skill when you need guidance on:
- Breaking down complex methods into smaller, testable units
- Introducing abstractions for dependency injection
- Moving service implementations behind interfaces
- Removing code duplication while maintaining clarity
- Fixing code smells (long methods, complex logic, magic strings)
- Safely refactoring existing code without changing behavior

## Refactoring Approach

- **Incremental**: Make one method/class change at a time
- **Test-driven**: Always verify tests pass before and after each change
- **Safe**: Use automated tools when available (rename, extract method, etc.)
- **Reversible**: Small commits allow easy rollback if needed
- **Behavioral**: Zero functional changes—same behavior, cleaner code

## Common Refactoring Patterns in WorkMood

### Extract Method
Break down complex methods into smaller, testable units that each have a single responsibility.

### Shim Factory Pattern
Introduce abstractions for dependency injection capabilities. See `.github/ai-codex-refactoring.md` for detailed patterns.

### Extract Interface
Move service implementations behind interfaces (`I[ServiceName]`) to enable testability and loose coupling.

### Remove Duplication
Use DRY principles while maintaining clarity—avoid extracting code that is only coincidentally similar.

### Fix Code Smells
Address:
- Long methods (extract smaller methods)
- Complex conditional logic (extract helper methods or use strategy pattern)
- Magic strings/numbers (use named constants)
- Comments explaining "why" code exists (consider renaming or restructuring)

## Refactoring Checklist

✅ **Before refactoring**: Tests pass, code compiles  
✅ **During refactoring**: One responsibility per commit  
✅ **After refactoring**: Tests pass, behavior identical, code simpler  
✅ **Commit message**: Use `^r` notation with clear intent description  
✅ **Verification**: Manual testing if UI-related changes  

Example commits:
```
^r - extract DrawBackground to use color factory for dependency injection
^r - remove SKCanvas overloads and use drawShimFactory for object creation
.r - extract method using automated refactoring tool
```

## When NOT to Refactor

❌ Don't refactor and add features simultaneously  
❌ Don't refactor without passing tests  
❌ Don't batch multiple unrelated refactorings  
❌ Don't optimize prematurely—clarity first  
❌ Don't refactor untested legacy code (add tests first)

## Step-by-Step Refactoring Process

1. **Ensure tests pass** - Run all tests before starting
2. **Make one change** - Refactor a single method or responsibility
3. **Run tests** - Verify behavior hasn't changed
4. **Commit** - Use `^r` notation with clear description
5. **Repeat** - Move to the next refactoring opportunity

## Example Input and Output

**Input**: "I have a method that does three different things. How should I refactor it?"

**Output**: 
1. Identify the three distinct responsibilities
2. Extract each into its own method
3. Test after each extraction
4. Update the original method to call the new helper methods
5. Commit with clear intent

## Related Resources

- `.github/ai-codex-refactoring.md` - Detailed shim factory methodology and patterns
- `.github/copilot-instructions.md` - Arlo's Commit Notation for documenting refactorings
