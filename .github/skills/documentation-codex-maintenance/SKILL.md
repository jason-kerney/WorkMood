---
name: documentation-codex-maintenance
description: >
  Guidance for keeping documentation, codexes, and architecture references in sync with code changes in WorkMood. Covers update triggers, best practices, checklists, and anti-patterns for sustainable documentation.
user-invokable: true
argument-hint: "[doc section] [update scenario]"
disable-model-invocation: false
---

# Documentation & Codex Maintenance Skill

## When to Use This Skill
- After making code, architecture, or workflow changes
- When adding new features, services, or abstractions
- Before committing or merging changes
- During code review to verify documentation accuracy

---

## Core Concepts
- **Codexes**: Living documentation files (e.g., `.github/ai-codex-architecture.md`) that describe architecture, patterns, and project rules
- **Update Triggers**: Any change to code structure, services, patterns, or workflows
- **Single-Commit Rule**: Code and documentation updates must be in the same commit
- **Accuracy First**: Outdated docs are worse than no docs—always update or flag for review

---

## How-To: Maintain Documentation & Codexes
1. **Identify what changed**: New service, refactor, pattern, or workflow
2. **Locate relevant codex/docs**: Architecture, refactoring, build/testing, examples, documentation codex
3. **Update docs in the same commit**: Never defer documentation updates
4. **Check for accuracy**: Ensure docs match actual code and patterns
5. **Flag outdated docs**: If unsure, add a TODO or flag for review

---

## Patterns & Checklist
- [ ] Update codexes for new/changed services, interfaces, or abstractions
- [ ] Add examples for new patterns or transformations
- [ ] Revise build/testing docs for new commands or frameworks
- [ ] Sync documentation with code in the same commit
- [ ] Remove or flag outdated sections
- [ ] Use clear, concise language and structure

---

## Anti-Patterns/When NOT to Use
- Never defer documentation updates to a later commit
- Don’t leave TODOs or placeholders without follow-up
- Avoid duplicating information across multiple docs—link instead
- Don’t let codexes drift from actual code or practices

---

## References
- [ai-codex-architecture.md](../../ai-codex-architecture.md)
- [ai-codex-refactoring.md](../../ai-codex-refactoring.md)
- [ai-codex-build-testing.md](../../ai-codex-build-testing.md)
- [ai-codex-examples.md](../../ai-codex-examples.md)
- [ai-codex-documentation.md](../../ai-codex-documentation.md)
- [Technical Editing for Human Readers](../technical-editing-human/SKILL.md)
- [Technical Editing for AI/LLM Systems](../technical-editing-ai/SKILL.md)
