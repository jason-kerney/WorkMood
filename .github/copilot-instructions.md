# GitHub Copilot Instructions for WorkMood

## Quick Reference

### Project Context
**WorkMood** - Cross-platform desktop mood tracking application
- **.NET 9.0 MAUI** (Windows/macOS only, no mobile)
- **4-project solution**: Main app, Tests, Version library, Version tests
- **Current Version**: 0.3.0 | **App ID**: `com.workmood.mauiapp`

### Essential Commands
```bash
# Build (specify framework)
dotnet build --framework net9.0-windows10.0.19041.0

# Run application  
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# Test everything
dotnet test

# Documentation setup
npm install
```

### Commit Pattern (MANDATORY)
**Format**: `[Risk][Intention] - [Description]`
- `^r` - Validated refactoring (most common)
- `^f` - Internal feature | `^F` - User feature  
- `^b` - Internal bugfix | `^B` - User bugfix
- `^d` - Documentation changes

---

## Available Personas

### 🎯 XP Developer Jason Kerney
**File:** [`copilot-personas/xp-developer-jason-kerney.md`](copilot-personas/xp-developer-jason-kerney.md)

Embodies Extreme Programming (XP) principles with a focus on:
- Test-Driven Development (TDD) and the Red-Green-Refactor cycle
- Simple, clean code with clarity as the primary goal
- Continuous refactoring with passing tests
- Collaborative pair programming mindset
- SOLID principles and best practices

Perfect for:
- Teams committed to code quality and testing
- Projects requiring maintainable, well-tested code
- Learning XP principles and practices
- Code reviews focused on craftsmanship

### How to Use a Persona

**Option 1: Reference in Copilot Chat (Recommended)**
In your Copilot Chat requests, simply reference the persona:

```
@copilot Using the XP Developer Jason Kerney persona, 
how should I approach testing this new feature?
```

Or paste the persona instructions directly into your chat message for immediate application.

**Option 2: Configure as Default**
To always use a persona in your workspace:
1. Copy the persona file content
2. Paste it into `.github/copilot-instructions.md`
3. This will apply the persona globally to all Copilot interactions in the workspace

---

## Copilot Skills

This workspace includes a library of Agent Skills that guide Copilot's behavior for specific tasks. Skills are auto-loaded when relevant to your request, or you can invoke them manually using `/skill-name` in Copilot Chat.

### 📚 Available Skills

#### Refactoring
**Directory:** `.github/skills/refactoring/`

Safe, incremental refactoring guidance for WorkMood. Covers:
- Extract method, shim factory pattern, extract interface
- DRY principles and code smell fixes
- Test-driven approach with passing tests before and after
- Zero functional modifications—behavior must remain identical

**Invoke with:** `/refactoring [code context] [refactoring goal]`

#### Test-Driven Development (TDD)
**Directory:** `.github/skills/tdd/`

Red-Green-Refactor cycle guidance for WorkMood. Covers:
- Test-driven development philosophy and workflow
- Arrange-Act-Assert pattern and test structure
- C# and xUnit testing patterns and conventions
- Common TDD patterns in WorkMood services and ViewModels
- Anti-patterns and when NOT to test

**Invoke with:** `/tdd [feature/method to implement] [test scenario]`

#### Code Smells Detection
**Directory:** `.github/skills/code-smells-detection/`

Identify and address code smells in WorkMood. Covers:
- Long methods, inappropriate intimacy, duplicate logic
- Magic numbers/strings, MVVM violations
- God classes, poor abstraction boundaries
- When to refactor vs. when to leave code alone
- WorkMood-specific smell patterns with examples

**Invoke with:** `/code-smells [code snippet or method] [context/concern]`

#### Test Organization & Hierarchy
**Directory:** `.github/skills/test-organization-hierarchy/`

Test suite organization and hierarchy guidance for WorkMood. Covers:
- File structure mirroring source code organization
- Test naming conventions and xUnit patterns
- Test grouping with regions and nested classes
- Test data builders and fixtures (DRY test setup)
- Scaling from small to large test suites
- Anti-patterns that cause test suite maintenance problems

**Invoke with:** `/test-organization [test component] [organization question]`

#### Technical Editing for Human Readers
**Directory:** `.github/skills/technical-editing-human/`

Edit technical documentation for human comprehension. Covers:
- Clarity, accessibility, and scaffolding explanations
- Information hierarchy, scanability, and reader-centric structure
- Simplifying jargon, improving prose, developing examples
- Tone for technical audiences, accessibility standards

**Invoke with:** `/technical-editing-human [document section] [editing goal/concern]`

#### Technical Editing for AI/LLM Systems
**Directory:** `.github/skills/technical-editing-ai/`

Edit instructions and specifications for AI system consumption. Covers:
- Precision, explicitness, and unambiguity in requirements
- Measurable criteria, conditional logic, edge cases
- Eliminating implicit assumptions, defining all terms
- Verifiable success criteria and anti-patterns

**Invoke with:** `/technical-editing-ai [instruction section] [clarity goal/concern]`

#### Creating Agent Skills
**Directory:** `.github/skills/creating-agent-skills/`

Create new Agent Skills for VS Code using open standard format. Covers:
- Skill structure, YAML frontmatter, progressive disclosure
- Content organization, examples, and resources
- Sharing skills, community contribution, maintenance

**Invoke with:** `/creating-agent-skills [skill concept] [use case/context]`

### Using Skills in Copilot Chat

**Option 1: Automatic Loading (Recommended)**
Simply ask a question related to refactoring or testing and Copilot will automatically load the relevant skill:
```
How should I break down this complex method?
I need to write tests for a new feature.
```

**Option 2: Manual Invocation**
Use the slash command to explicitly invoke a skill:
```
/refactoring Extract this 50-line method into smaller functions
/tdd How should I test this MoodDataService method?
/code-smells I noticed this ViewModel has too many dependencies
/test-organization Where should tests for the new graph service go?
/technical-editing-ai Review the DataMigrationService instructions for clarity
/technical-editing-human Edit this README section for accessibility
/creating-agent-skills Help me create a new skill for deployment
```

**Option 3: Combine with Personas**
Mix skills with personas for targeted guidance:
```
@copilot Using the XP Developer Jason Kerney persona, 
/refactoring guide me through this refactoring safely

@copilot Using the XP Developer persona and /tdd skill,
help me write tests for this feature using Red-Green-Refactor cycle
```

---

## When Creating Code

### Architecture Requirements
**MAUI MVVM Clean Architecture** - All code must follow:
- **MVVM Pattern**: ViewModels (no direct UI), Views (XAML), Models (data)
- **Dependency Injection**: Constructor injection for all services
- **Interface Segregation**: Services behind interfaces (`I[ServiceName]`)
- **Nullable Reference Types**: Enabled project-wide

**Key Service Interfaces**: `IMoodDataService`, `IMoodVisualizationService`, `ILineGraphService`, `IScheduleConfigService`, `INavigationService`, `IDataArchiveService`, `ILoggingService`

**📚 For comprehensive architecture guidance**: See `.github/ai-codex-architecture.md` for detailed folder structure, service patterns, dependency injection examples, MVVM implementation, and shim abstractions.

---

## When Building & Testing

### Framework Targeting (CRITICAL)
**Always specify target framework** - MAUI requires explicit targeting:

```bash
# Windows (primary development)
dotnet build --framework net9.0-windows10.0.19041.0
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# Solution-wide operations
dotnet build WorkMood.sln           # All projects  
dotnet test                         # All tests
```

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

---

## When Refactoring (Shim Factory Pattern)

### Refactoring Philosophy
**Disciplined Incremental Refactoring** - Safety and testability first
- **ONE method at a time** - Never batch refactor multiple methods
- **Manual verification** - Test after each change (ask user to verify)
- **Small commits** - One concept per commit with Arlo's notation
- **Preserve behavior** - Zero functional changes during refactoring

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

---

## When Committing Code

### Arlo's Commit Notation (MANDATORY)
**Format**: `[Risk][Intention] - [Description]`

#### Risk Assessment
- **`.`** (Safe) - Automated refactoring tools OR test-supported procedural with full coverage
- **`^`** (Validated) - Manual changes with adequate test coverage and manual verification  
- **`!`** (Risky) - Manual changes with limited test coverage but following established patterns
- **`@`** (Broken) - Code doesn't compile, tests fail, or functionality incomplete

#### Intentions
- **`r`** - Refactoring (no behavior change) 
- **`f`** - Internal feature | **`F`** - User-visible feature
- **`b`** - Internal bugfix | **`B`** - User-visible bugfix  
- **`d`** - Documentation changes
- **`t`** - Tests only (new or modified tests)

#### Common Commit Examples
```bash
# Refactoring (most common)
^r - extract DrawBackground to use color factory for dependency injection
^r - remove SKCanvas overloads and use drawShimFactory for object creation
.r - extract method using automated refactoring tool

# Features & Fixes
^f - add new internal API method for graph data processing
^b - fix null reference in data validation logic  
^B - fix user-visible crash when loading invalid mood data
^F - add new user dashboard feature (manual testing only)

# Documentation
^d - update architecture documentation for shim factory pattern
^d - add maintenance guidelines for copilot instructions
```

### Quality Gates Before Commit
✅ **Build succeeds** without errors  
✅ **Tests pass** (automated + manual verification when needed)
✅ **Single responsibility** - One method/concept per commit
✅ **Behavior preserved** - No functional changes during refactoring
✅ **Risk assessed** - Appropriate risk level assigned

### Ensuring Arlo's Commit Notation in VS Code

**❌ VS Code "Generate Commit Message" Limitation**: The built-in generate feature doesn't automatically follow local templates or project-specific notation.

**✅ Recommended Approaches**:

1. **GitHub Copilot Chat Method (Most Effective)**:
   - Use `Ctrl+Shift+I` or open Copilot Chat
   - Ask: "Generate a commit message using Arlo's Commit Notation for my staged changes"
   - Copilot will use the instructions in `.github/copilot-instructions.md`

2. **Manual Template Reference**:
   - Git template (`.gitmessage`) shows in commit dialog as comments
   - Use as reference while typing commit messages manually

3. **PowerShell Helper** (Fastest for common patterns):
   - Source `commit-helper.ps1` for command shortcuts
   - `arlorefactor "description"` - Quick `^r` commits
   - `arlofeat "description"` - Quick `^f` commits  
   - `arlodoc "description"` - Quick `^d` commits
   - `Show-ArloNotation` - Display notation reference

4. **VS Code Snippets**: 
   - Type `^r`, `^f`, `^d`, etc. in commit input for autocomplete
   - Available in `.vscode/commit-message.code-snippets`

**Best Practice**: Use Copilot Chat for commit generation rather than the built-in "Generate Commit Message" button to ensure Arlo's notation compliance.

---

## When Updating Documentation

### Codex Maintenance (CRITICAL)

The AI codexes are **living documentation** that must stay synchronized with actual project state. These are used by AI systems (including Copilot) to understand WorkMood architecture and patterns.

**When codexes become inaccurate, AI guidance becomes unreliable.**

#### AI Codexes Requiring Updates

1. **`.github/ai-codex-architecture.md`**
   - Update when: New services added, service interfaces change, folder structure modified
   - Verify: All services documented, interfaces match actual code, DI examples current
   - Using skill: `/technical-editing-ai [section] Does this match actual implementation?`

2. **`.github/ai-codex-refactoring.md`**
   - Update when: New shim abstractions created, refactoring patterns evolve, anti-patterns identified
   - Verify: Factory patterns match actual implementation, examples compile, methodology still valid
   - Using skill: `/technical-editing-ai [section] Is factory pattern documentation accurate?`

3. **`.github/ai-codex-build-testing.md`**
   - Update when: Framework versions change, build commands updated, test procedures modified
   - Verify: All commands execute successfully, framework targeting correct, test examples work
   - Using skill: Run documented commands; verify success before merging changes

4. **`.github/ai-codex-examples.md`**
   - Update when: New transformation patterns discovered, historical examples added
   - Verify: Code examples compile, before/after patterns match actual patterns, metrics are accurate
   - Using skill: `/technical-editing-ai [example] Does this pattern still represent current practice?`

5. **`.github/ai-codex-documentation.md`**
   - Update when: Documentation processes change, maintenance procedures evolve
   - Verify: Process still followed, checklists match current practices, responsibility matrix current
   - Using skill: `/technical-editing-ai [section] Is this maintenance process being followed?`

#### Update Workflow (MANDATORY)

**Every structural change must include documentation updates in the SAME commit:**

```bash
# ✅ GOOD: Documentation included
^f - add IDataMigrationService with architecture codex updates
# (Updates: service interface list, usage examples, testing patterns)

# ❌ BAD: Documentation deferred
# ^ Never use "docs TODO" in commit messages
```

**Before committing any change, ask:**
1. What documentation describes what I just changed?
2. Does that documentation still match the code?
3. Are my updates included in this commit?

### Copilot Instructions Update Triggers

Update `.github/copilot-instructions.md` when making changes that affect:

1. **Project Structure** - Adding/removing projects, new folders, framework changes
2. **Service Architecture** - New services, interface changes, DI patterns  
3. **Build Process** - New commands, target frameworks, testing procedures
4. **Development Workflow** - Tools, dependencies, refactoring patterns, commit conventions
5. **Skill Changes** - New skills created, existing skills significantly modified

**📚 For comprehensive documentation maintenance guidance**: See `.github/ai-codex-documentation.md` for detailed update processes, validation procedures, responsibility matrix, and enforcement mechanisms.

---

## Platform & Environment Details

### Target Frameworks & Requirements
- **Windows**: `net9.0-windows10.0.19041.0` (Min: Windows 10 v1809)
- **macOS**: `net9.0-maccatalyst` (Min: macOS 15.0 Monterey, x64/ARM64)
- **Language**: C# with nullable reference types enabled
- **UI**: XAML with compiled bindings enabled

### Dependencies & Tools
- **Primary**: .NET 9.0 MAUI, SkiaSharp for graphics  
- **Documentation**: Node.js + Doculisp (`npm install`)
- **Version Utility**: `whats-your-version` library with build date attribution
- **Testing**: xUnit (standard .NET testing)

### Application Metadata
- **Name**: "WorkMood - Daily Mood Tracker"  
- **ID**: `com.workmood.mauiapp` 
- **Version**: 0.3.0 (ApplicationVersion: 4)
- **Icon**: `smiles.ico` (root), `Resources/AppIcon/smiles.png` (MAUI)
- **Scope**: Desktop-only work mood tracking, local storage, no cloud services

---

**📚 For comprehensive code transformation examples**: See `.github/ai-codex-examples.md` for detailed before/after patterns, factory implementations, historical references, and proven transformation methodologies.

---

## Response Guidelines for GitHub Copilot

**When responding, always:**
- Use Arlo's Commit Notation format when suggesting commits
- Include risk assessment reasoning for commit suggestions
- **Proactively check instruction accuracy** - suggest updates when detecting project changes
- **Flag mismatches** - alert if project state differs from documented instructions  
- **Suggest instruction updates** when making structural changes
- **Verify codex accuracy** - catch outdated patterns before they mislead development
- **Recommend codex updates** when you detect actual behavior diverging from documented patterns
- Consider MVVM compliance, service injection, platform requirements, data binding, MAUI lifecycle
- End responses with 🤖 to indicate use of this context

**Action-oriented priorities:**
1. **Creating Code** → Architecture patterns, existing services, shim abstractions
2. **Building/Testing** → Framework targeting, test organization, quality gates
3. **Refactoring** → Incremental methodology, shim factory pattern, commit discipline  
4. **Committing** → Arlo's notation, risk assessment, single responsibility + **codex updates**
5. **Maintaining** → Instruction updates, codex synchronization, project state accuracy

## Codex Health & Accuracy

**Critical Responsibility**: Keep AI codexes accurate or they become actively harmful to development.

- **Inaccurate codexes** expose wrong patterns to AI systems
- **Outdated examples** lead to copy-paste errors and anti-patterns
- **Missing documentation** of new features leaves AI systems unaware of capabilities
- **Inconsistent factory patterns** between codex and actual code confuse AI guidance

**When you notice:**
- Code that doesn't match documentation → Flag for codex update
- New patterns that should be documented → Add to appropriate codex
- Outdated examples in codexes → Mark for review and replacement
- Ambiguous instructions that confuse AI → Use technical-editing-ai skill to clarify

**Each commit includes documentation updates** ensures codexes stay in sync with code.
