---
name: code-review
description: >
  Technical code review guidance for WorkMood and similar .NET/MAUI projects. Covers correctness, testability, performance, idiomatic C#/.NET, and API design. Provides checklists, review heuristics, and verification steps for safe, incremental improvements.
user-invokable: true
argument-hint: "[code context] [review focus/question]"
disable-model-invocation: false
---

# Code Review Skill for WorkMood

## Purpose

This skill guides technical code review for C#/.NET (including MAUI) projects, with a focus on:
- Correctness and safety (does it work, is it robust?)
- Testability (can it be covered by unit/integration tests?)
- Performance (allocations, CPU, UI responsiveness)
- Idiomatic .NET/C# and project conventions (MVVM, DI, SOLID)
- API and architecture clarity

---

## Review Checklist

- [ ] **Correctness**: Does the code do what it claims? Are edge cases handled?
- [ ] **Tests**: Are there tests? Do they cover happy/sad paths? Are they clear and minimal?
- [ ] **Performance**: Any obvious allocation, CPU, or UI responsiveness issues? (Use BenchmarkDotNet, PerfView, VS Diagnostics)
- [ ] **Readability**: Is the code clear, small, and idiomatic? Any long methods, magic values, or code smells?
- [ ] **API Design**: Are interfaces clear, minimal, and DI-friendly? Any leaky abstractions?
- [ ] **MVVM/Architecture**: Does it follow project patterns (MVVM, DI, shims, factories)?
- [ ] **Documentation**: Are comments, XML docs, and codexes up to date?

---

## Review Heuristics

- Prefer small, focused diffs and single-responsibility changes.
- Write or update tests before/after changes to guarantee behavior.
- Use Arrange-Act-Assert in tests; prefer xUnit patterns.
- For performance: measure before/after, optimize only when needed.
- For refactoring: preserve behavior, commit in small increments, update codexes.

---

## Example Review Prompts

- "/code-review [file.cs] Review for testability and performance."
- "/code-review [method] Is this idiomatic C#? Any code smells?"
- "/code-review [ViewModel] Does this follow MVVM and DI patterns?"
- "/code-review [diff] Are there any risks or missing tests?"

---

## Verification Steps

- Build: `dotnet build --framework net9.0-windows10.0.19041.0`
- Test: `dotnet test` (or filter for specific tests)
- Benchmark: Add BenchmarkDotNet, run microbenchmarks for hot paths
- Profile: Use PerfView or VS Diagnostic Tools for UI/CPU/memory issues

---

## Resources

- [Refactoring Skill](../refactoring/SKILL.md)
- [TDD Skill](../tdd/SKILL.md)
- [Code Smells Detection](../code-smells-detection/SKILL.md)
- [.NET Performance Guide](https://learn.microsoft.com/en-us/dotnet/standard/performance/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)

---

## Maintenance

- Update this skill as project patterns or review priorities evolve.
- Ensure checklists and heuristics match current codexes and architecture docs.
