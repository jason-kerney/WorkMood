# GitHub Copilot Instructions Extraction Plan

## Overview
This document outlines the plan to extract detailed sections from `.github/.copilot-instructions.md` into separate AI codexes, allowing the main instructions to focus on always-relevant portions while maintaining access to detailed information through references.

## Extraction Strategy

### File Naming Convention
- **Pattern**: `ai-codex-[topic].md`
- **Location**: `.github/` directory alongside main instructions
- **Purpose**: Specialized knowledge bases that GitHub Copilot can reference when needed

### Integration Approach
1. **Extract** detailed content into specialized codex files
2. **Replace** with concise summary and reference in main instructions
3. **Reference** format: "See `.github/ai-codex-[topic].md` for detailed guidance"
4. **Maintain** coherent flow in main instructions document

## Sections to Extract

### 1. Code Creation & Architecture → `ai-codex-architecture.md`
**Content to Extract**:
- MVVM Clean Architecture requirements
- Detailed folder structure with descriptions
- Complete service interfaces list
- Dependency injection patterns and examples
- Interface segregation principles

**Summary for Main Instructions**:
- Brief MVVM overview
- Reference to architecture codex
- Essential service interface names only

**When Copilot Uses**: Creating new classes, services, or ViewModels; organizing project structure; implementing dependency injection

---

### 2. Build & Testing Processes → `ai-codex-build-testing.md`
**Content to Extract**:
- Detailed framework targeting commands
- Complete testing strategy documentation
- Test organization patterns
- Quality assurance checklists
- Platform-specific build requirements

**Summary for Main Instructions**:
- Essential build commands only
- Basic test command
- Reference to build/testing codex

**When Copilot Uses**: Setting up build processes; configuring testing; troubleshooting compilation issues; cross-platform concerns

---

### 3. Refactoring Methodology → `ai-codex-refactoring.md`
**Content to Extract**:
- Complete Shim Factory Pattern implementation process
- Detailed existing shim abstractions
- Refactoring philosophy and methodology
- Phase-by-phase refactoring steps
- Refactoring targets by priority
- Anti-patterns to avoid

**Summary for Main Instructions**:
- Brief refactoring approach
- Reference to refactoring codex
- Key principle: "ONE method at a time"

**When Copilot Uses**: Planning refactoring sessions; implementing dependency injection; creating shim abstractions; systematic code improvement

---

### 4. Version Control & Commit Management → `ai-codex-commits.md`
**Content to Extract**:
- Complete Arlo's Commit Notation system
- Detailed risk assessment criteria
- All intention types and examples
- Quality gates before commit
- VS Code commit message generation methods
- PowerShell helper commands

**Summary for Main Instructions**:
- Basic commit format: `[Risk][Intention] - [Description]`
- Common examples only (`^r`, `^f`, `^d`)
- Reference to commit codex

**When Copilot Uses**: Generating commit messages; assessing commit risk; understanding commit notation; using commit tools

---

### 5. Documentation Maintenance → `ai-codex-documentation.md`
**Content to Extract**:
- Detailed maintenance triggers
- Complete update process workflow
- Responsibility matrix
- Validation procedures
- Periodic review guidelines
- Impact assessment criteria

**Summary for Main Instructions**:
- Basic update trigger: "structural changes affect instructions"
- Reference to documentation codex

**When Copilot Uses**: Updating documentation; managing instruction changes; validating documentation accuracy; planning maintenance

---

### 6. Code Transformation Examples → `ai-codex-examples.md`
**Content to Extract**:
- Complete before/after code examples
- Factory pattern implementations
- Historical reference points
- Success metrics and methodologies
- Detailed transformation patterns
- Commit history examples

**Summary for Main Instructions**:
- Brief mention of transformation approach
- Reference to examples codex

**When Copilot Uses**: Understanding transformation patterns; implementing factory patterns; learning from historical examples; planning code improvements

## Implementation Process

### For Each Section:
1. **Extract Content**: Copy relevant content to new codex file
2. **Create Summary**: Write concise summary for main instructions
3. **Update References**: Replace detailed content with summary + reference
4. **Test Integration**: Ensure references work and flow is maintained
5. **Commit**: Use format `^d - extract [section] to ai-codex-[topic] for focused reference`
6. **Wait for Confirmation**: Pause for validation before proceeding

### Quality Gates:
- ✅ **Coherence**: Main instructions remain coherent and actionable
- ✅ **Completeness**: No information is lost, only relocated
- ✅ **Accessibility**: Copilot can find and use referenced information
- ✅ **Maintainability**: Updates to codexes are manageable and clear

## Starting Point
**First Section**: Code Creation & Architecture
**Target File**: `.github/ai-codex-architecture.md`
**Expected Outcome**: Main instructions focus on essential architecture principles while detailed patterns are available via reference

## Success Criteria
- Main instructions file is significantly shorter and more focused
- All detailed information remains accessible through codex references
- GitHub Copilot can effectively use both main instructions and codexes
- Development workflow remains uninterrupted
- Maintenance becomes easier due to separation of concerns

---

*This plan ensures systematic extraction while maintaining the integrity and usefulness of the GitHub Copilot instructions system.*