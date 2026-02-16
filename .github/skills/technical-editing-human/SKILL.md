---
name: technical-editing-human
description: Edit technical documentation, guides, and content for human readers. Focuses on clarity, accessibility, and comprehension—ensuring readers grasp concepts without confusion. Covers simplifying explanations, improving information hierarchy, developing examples, and proofreading prose.
argument-hint: "[document section or file] [editing goal/concern]"
user-invokable: true
disable-model-invocation: false
---

# Technical Editing for Human Use

## Overview

This skill guides editing technical documentation, guides, and content for human readers. Focus is on clarity, accessibility, and comprehension—ensuring readers grasp concepts without confusion.

**Apply this skill when:**
- Editing user guides, tutorials, or getting-started docs
- Refining architecture documentation for team readiness
- Polishing README files or developer guides
- Clarifying explanations in inline code documentation
- Improving accessibility of technical content

---

## Core Principles

### 1. **Clarity Over Completeness**
- Strip jargon; define terms on first use
- One idea per sentence
- Active voice preferred
- Short paragraphs (3-4 sentences max)

### 2. **Reader-Centric Structure**
- **Hierarchy**: Use headers to signal importance and relationships
- **Scanability**: Readers skim; highlight key points with bold or bullets
- **Context First**: Say what readers will learn *before* the details
- **Examples Matter**: Concrete examples beat abstract explanations

### 3. **Tone for Technical Audiences**
- Friendly but professional (not casual)
- Confident but not arrogant (admit complexity and trade-offs)
- Direct and honest (avoid marketing speak)
- Inclusive language (assume varied backgrounds)

### 4. **Accesibility**
- Images have alt text
- Code blocks are properly labeled
- Links have descriptive anchor text (not "click here")
- Lists use parallel structure
- Color isn't the only indicator of meaning

---

## Common Editing Tasks

### Simplifying Dense Explanations

**Before:**
> The DataMigrationService orchestrates the asynchronous transfer of persisted mood data artifacts from legacy file system locations to the new XDG-compliant directory structure, performing recursive directory enumeration and atomic file operations with transactional semantics.

**After:**
> The DataMigrationService moves mood data from the old folder location to the new standard folder structure. It handles nested folders and ensures the transfer completes fully or doesn't happen at all.

*What changed:*
- Replaced jargon (orchestrates, artifacts, atomic, transactional) with plain verbs (moves, handles)
- Broke into two sentences
- Specific example: "old folder → new standard folder"

---

### Improving Information Hierarchy

**Before:**
```markdown
To build the application, use dotnet build --framework net9.0-windows10.0.19041.0 which requires that you have .NET 9.0 SDK installed and also make sure the workspace folder is the root directory and you need to run from PowerShell or cmd.
```

**After:**
```markdown
## Building for Windows

1. **Prerequisites**: .NET 9.0 SDK installed
2. **Command**: `dotnet build --framework net9.0-windows10.0.19041.0`
3. **Location**: Run from the workspace root directory
4. **Shell**: PowerShell or cmd recommended
```

*What changed:*
- Separate prerequisite from instructions
- Use numbered steps (implies sequence)
- Code properly formatted
- Easier to skim

---

### Making Examples Work

**Bad example:**
> Use `IMoodDataService` to access mood data.

**Good example:**
```csharp
// Constructor injection
public MoodViewModel(IMoodDataService dataService)
{
    _dataService = dataService;
}

// Usage in a method
var moods = await _dataService.GetMoodsAsync(startDate, endDate);
```

*What changed:*
- Shows context (where injection happens)
- Real code, not pseudo-code
- Clear input/output

---

### Active vs. Passive Voice

| ❌ Passive | ✅ Active |
|-----------|----------|
| "The migration will be performed..." | "The service migrates..." |
| "Data should be validated..." | "Validate data before..." |
| "It is recommended that..." | "We recommend..." |

---

## Editing Checklist for Human Documentation

- [ ] **Clarity**: Can a reader unfamiliar with the code understand the core idea?
- [ ] **Structure**: Do headers reveal the document's organization at a glance?
- [ ] **Example-Driven**: Does every concept have a concrete example?
- [ ] **No Jargon Overload**: Technical terms are defined or explained?
- [ ] **Tone**: Would a colleague enjoying this voice and approach?
- [ ] **Scannable**: Does skimming headers/bold give the main story?
- [ ] **Complete**: Does it answer "what," "why," and "how"?
- [ ] **Accurate**: Do code samples actually run? Are facts current?

---

## When NOT to Edit (Safety Guidelines)

- **Do NOT** oversimplify critical safety information
- **Do NOT** remove important warnings or edge cases for brevity
- **Do NOT** change technical accuracy for fluency
- **Do NOT** assume all readers have the same background (be inclusive)
- **Do NOT** use humor if it obscures the message

---

## WorkMood-Specific Guidance

### Documentation Locations & Styles
- **User Guides** ([userguide/](../../docs/userguide/)): Simple, visual, task-focused
- **Architecture Docs** ([build/](../../docs/build/)): Technical but clear, assume engineer audience
- **Getting Started** ([readme/](../../docs/readme/)): Zero assumptions, lots of examples
- **API Docs** (inline code): Concise, reference-style, consistent

### Common WorkMood Terms to Clarify
- **Mood Data**: User mood entries with timestamp and intensity
- **Migration**: Moving mood data from old folder structure to new standard location
- **Shim Abstraction**: Interface wrapping platform-specific behavior (e.g., FolderShim)
- **Visualization**: Graph display of mood trends over time

---

## Examples from WorkMood

### ✅ Good: [USER-GUIDE.md](../../../USER-GUIDE.md) - Clear task structure
> "To record a mood, click the 'Add Mood' button. Select your mood intensity using the slider (happy to sad). Optional: add a note about what influenced your mood. Click 'Save'."

*Why it works:* Steps are clear, context provided, example included

### ✅ Good: [README.md](../../../README.md) - Hierarchy and examples
> Shows feature bullets first, then prerequisites with clear formatting, then build commands in code blocks

### ⚠️ Opportunity: Dense architecture paragraphs
> Consider breaking multi-sentence explanations into numbered steps or bullet lists

---

## How to Request Edits Using This Skill

**In Copilot Chat:**
```
/technical-editing-human Review this README section for clarity. 
Is it accessible to someone new to the project?

/technical-editing-human Simplify this DataMigrationService explanation 
for a developer unfamiliar with our codebase.

/technical-editing-human Restructure these build instructions 
using clearer hierarchy and better examples.
```

---

## References
- _Chicago Manual of Style_ (technical writing conventions)
- _The Sense of Style_ by Steven Pinker (clarity principles)
- WorkMood docs: examples of what works well
