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

### 1. Code Creation & Architecture → `ai-codex-architecture.md` ✅ COMPLETED
**Content Extracted**:
- ✅ MVVM Clean Architecture requirements with detailed principles
- ✅ Complete folder structure with comprehensive descriptions for all 12 directories
- ✅ All service interfaces with full method signatures and examples
- ✅ Dependency injection patterns with code examples
- ✅ Interface segregation principles and factory patterns
- ✅ Testing considerations and architecture validation patterns

**Summary in Main Instructions**:
- ✅ Concise MVVM overview (4 key principles)
- ✅ Essential service interface names only
- ✅ Reference link: "See `.github/ai-codex-architecture.md` for comprehensive architecture guidance"

**When Copilot Uses**: Creating new classes, services, or ViewModels; organizing project structure; implementing dependency injection

**Commit**: `^d - extract Code Creation & Architecture to ai-codex-architecture for focused reference` (6bfdfc9)

---

### 2. Build & Testing Processes → `ai-codex-build-testing.md` ✅ COMPLETED
**Content Extracted**:
- ✅ Comprehensive framework targeting commands for Windows and macOS
- ✅ Complete testing strategy with execution commands and filters
- ✅ Detailed test organization patterns mirroring project structure
- ✅ Quality gates and CI/CD pipeline configurations
- ✅ Platform-specific build requirements and troubleshooting
- ✅ Code coverage analysis and performance profiling guidance
- ✅ Debug configurations and diagnostic tools usage

**Summary in Main Instructions**:
- ✅ Essential build commands for primary development workflow
- ✅ Basic test execution commands
- ✅ Reference link: "See `.github/ai-codex-build-testing.md` for comprehensive build & testing guidance"

**When Copilot Uses**: Setting up build processes; configuring testing; troubleshooting compilation issues; cross-platform concerns

**Commit**: `^d - extract Build & Testing Processes to ai-codex-build-testing for comprehensive guidance` (3a542cb)

---

### 3. Refactoring Methodology → `ai-codex-refactoring.md` ✅ COMPLETED
**Content Extracted**:
- ✅ Complete Shim Factory Pattern implementation with two-phase approach
- ✅ Detailed existing shim abstractions for File System, Drawing, and Utilities
- ✅ Disciplined incremental refactoring philosophy with core principles
- ✅ Phase-by-phase refactoring steps with critical sequence methodology
- ✅ Refactoring targets prioritized by impact (High/Medium/Lower)
- ✅ Comprehensive anti-patterns section with specific examples
- ✅ Historical reference and success metrics from actual refactoring sessions
- ✅ Advanced shim patterns including argument-based and composite factories

**Summary in Main Instructions**:
- ✅ Essential refactoring philosophy with 4 core principles
- ✅ Key principle emphasized: "ONE method at a time"
- ✅ Reference link: "See `.github/ai-codex-refactoring.md` for comprehensive refactoring guidance"

**When Copilot Uses**: Planning refactoring sessions; implementing dependency injection; creating shim abstractions; systematic code improvement

**Commit**: `^d - extract Refactoring Methodology to ai-codex-refactoring for systematic guidance` (aa62232)

---

### 4. Documentation Maintenance → `ai-codex-documentation.md`
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

### 5. Code Transformation Examples → `ai-codex-examples.md`
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
6. **Update Plan**: Mark section as completed with commit details and accomplishments
7. **Wait for Confirmation**: Pause for validation before proceeding

### Plan Maintenance
**This plan document will be updated after each extraction to:**
- ✅ Mark completed sections with detailed accomplishments
- 📊 Track quantitative results (lines reduced, codex size, etc.)
- 🔗 Document commit hashes and messages for traceability
- 📋 Update progress status showing what's completed and what's next
- 🎯 Maintain clear roadmap for remaining work

**Update Pattern**: After each section extraction, the plan will be updated with completion status, then committed using: `^d - update extraction plan with [section] completion status`

### Quality Gates:
- ✅ **Coherence**: Main instructions remain coherent and actionable
- ✅ **Completeness**: No information is lost, only relocated
- ✅ **Accessibility**: Copilot can find and use referenced information
- ✅ **Maintainability**: Updates to codexes are manageable and clear

## Progress Status

### ✅ Completed: Code Creation & Architecture
- **File Created**: `.github/ai-codex-architecture.md` (400+ lines)
- **Main Instructions Updated**: Reduced from 45 to 10 lines with codex reference
- **Commit**: 6bfdfc9 - `^d - extract Code Creation & Architecture to ai-codex-architecture for focused reference`
- **Result**: Essential architecture info remains in main instructions; comprehensive guidance available via codex

### ✅ Completed: Build & Testing Processes
- **File Created**: `.github/ai-codex-build-testing.md` (500+ lines)
- **Main Instructions Updated**: Reduced from 32 to 11 lines with codex reference
- **Commit**: 3a542cb - `^d - extract Build & Testing Processes to ai-codex-build-testing for comprehensive guidance`
- **Result**: Essential build/test commands remain; comprehensive procedures available via codex

### ✅ Completed: Refactoring Methodology
- **File Created**: `.github/ai-codex-refactoring.md` (600+ lines)
- **Main Instructions Updated**: Reduced from 42 to 8 lines with codex reference
- **Commit**: aa62232 - `^d - extract Refactoring Methodology to ai-codex-refactoring for systematic guidance`
- **Result**: Essential refactoring principles remain; comprehensive Shim Factory methodology available via codex

### 🔄 Next Section: Documentation Maintenance  
**Target File**: `.github/ai-codex-documentation.md`
**Expected Outcome**: Essential maintenance triggers remain; detailed update processes available via reference

**Note**: Version Control & Commit Management (Section 5) will remain in main instructions as it's always-relevant information that developers need immediate access to.

## Success Criteria
- Main instructions file is significantly shorter and more focused
- All detailed information remains accessible through codex references
- GitHub Copilot can effectively use both main instructions and codexes
- Development workflow remains uninterrupted
- Maintenance becomes easier due to separation of concerns

---

*This plan ensures systematic extraction while maintaining the integrity and usefulness of the GitHub Copilot instructions system.*