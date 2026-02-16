---
name: creating-agent-skills
description: Create new Agent Skills for VS Code using the open standard format. Covers skill structure, YAML frontmatter, content organization, examples, and resources. Enables building reusable capabilities that work across multiple AI agents.
argument-hint: "[skill concept] [use case/context]"
user-invokable: true
disable-model-invocation: false
---

# Creating Agent Skills

## Overview

Agent Skills are reusable capabilities that teach GitHub Copilot and other AI agents specialized knowledge and workflows. They work across VS Code, GitHub Copilot CLI, and other agents supporting the open standard.

Skills are more comprehensive than custom instructions:
- **Include scripts, examples, and resources** alongside instructions
- **Are task-specific** and loaded on-demand for efficiency
- **Are portable** across different AI tools
- **Can be shared** with a wider community

---

## When to Create a Skill

Create a skill when you need to:
- Enable a **specialized workflow** (testing strategy, refactoring approach, debugging process)
- **Reduce repetition** by capturing domain knowledge once
- **Include resources** beyond text (scripts, templates, examples)
- **Compose complex capabilities** from multiple concepts
- **Share capabilities** across projects or with your team

**Do NOT create a skill for:**
- Project-specific coding standards (use custom instructions instead)
- One-time guidance that won't be reused
- Content that changes frequently (use documentation/guides)

---

## Agent Skills Standard (agentskills.io)

Skills follow the open Agent Skills standard. This ensures:
- ✅ Compatibility across multiple AI agents
- ✅ Consistent structure and behavior
- ✅ Portability between different tools and platforms
- ✅ Interoperability with community-shared skills

---

## Directory Structure and File Organization

### Required Structure

Each skill must follow this folder layout:

```
.github/skills/
└── my-skill-name/           # Directory name matches the `name` field in SKILL.md
    ├── SKILL.md             # Required: contains YAML frontmatter + instructions
    ├── README.md            # Optional: skill overview for reference
    ├── examples/            # Optional: example usage, templates, scenarios
    │   ├── example1.txt
    │   └── example2.txt
    ├── templates/           # Optional: reusable templates or boilerplate
    │   └── template.md
    └── scripts/             # Optional: helper scripts, automation
        └── helper.sh
```

### Naming Conventions

- **Directory name**: lowercase, hyphens for spaces (max 64 chars)
  - ✅ `creating-agent-skills`
  - ✅ `test-driven-development`
  - ❌ `CreatingAgentSkills` (use hyphens, not camelCase)
  - ❌ `create-copilot-agent-skills` (misleading; be specific)

- **Must match**: Directory name = `name` field in SKILL.md frontmatter
  - If mismatch, skill won't load

---

## SKILL.md File Format

The `SKILL.md` file is Markdown with YAML frontmatter (metadata at the top).

### YAML Frontmatter (Required)

```yaml
---
name: skill-name
description: Clear description of what skill does and when to use it (max 1024 chars)
argument-hint: "[context] [additional info]"
user-invokable: true
disable-model-invocation: false
---
```

#### Frontmatter Fields

| Field | Required | Value | Purpose |
|-------|----------|-------|---------|
| `name` | Yes | Lowercase, hyphens, max 64 chars, matches directory name | Unique skill identifier |
| `description` | Yes | What it does + when to use it, max 1024 chars | Called at skill discovery; AI uses this to decide when to auto-load |
| `argument-hint` | No | `[param1] [param2]` | Shown in chat input when invoking as `/skill-name` |
| `user-invokable` | No | `true` (default) or `false` | Whether skill appears in `/` menu |
| `disable-model-invocation` | No | `false` (default) or `true` | Whether AI can auto-load based on relevance |

#### Invocation Modes

| `user-invokable` | `disable-model-invocation` | Behavior | Use Case |
|-----------------|---------------------------|----------|----------|
| `true` (default) | `false` (default) | ✅ Slash command + ✅ Auto-load | General-purpose skills |
| `false` | `false` | ❌ Slash command + ✅ Auto-load | Background knowledge skills |
| `true` | `true` | ✅ Slash command + ❌ Auto-load | On-demand-only skills |
| `false` | `true` | ❌ Slash command + ❌ Auto-load | Disabled skills (rare) |

### Markdown Body

The body (after frontmatter) contains instructions, examples, and guidance.

**Structure:**
```markdown
## When to Use This Skill
- Use cases and triggers
- What problems it solves

## Core Concepts
- Fundamental ideas
- Philosophy or approach

## How-To: [Task 1]
- Step-by-step instructions
- Examples and code snippets

## How-To: [Task 2]
- Similar structure

## Common Patterns
- Template or checklist
- Examples from your domain

## When NOT to Use
- Anti-patterns
- Exclusions

## References
- Links to related resources
- Troubleshooting
```

---

## Creating a New Skill: Step-by-Step

### Step 1: Define the Skill Concept

Answer these questions:
- **What domain?** (refactoring, testing, documentation, etc.)
- **What problem does it solve?** (unclear how to approach task X)
- **When should AI load it?** (always available, or auto-load when relevant?)
- **What's the audience?** (all developers, specific role, project-specific?)

### Step 2: Create the Directory and SKILL.md File

```bash
mkdir -p .github/skills/my-skill-name
cd .github/skills/my-skill-name
touch SKILL.md
```

### Step 3: Write the YAML Frontmatter

```yaml
---
name: my-skill-name
description: One-sentence what + when. Include concrete use cases (goal-oriented). Max 1024 chars.
argument-hint: "[context] [additional info needed from user]"
user-invokable: true
disable-model-invocation: false
---
```

**Tips for the description:**
- **Be specific** about use cases (not "helps with refactoring" but "guides extract method refactoring for WorkMood services")
- **State when to use it** (triggers and contexts)
- **Mention key capabilities** (so AI recognizes relevance)
- **Keep under 1024 chars** (be concise)

### Step 4: Write the Skill Body

Structure your content with clear sections:

1. **Overview/When to Use** (first)
   - Problem statement
   - Applicable scenarios
   - Use cases

2. **Core Concepts** (second)
   - Philosophy or approach
   - Key terminology
   - Design principles

3. **How-To Sections** (middle)
   - Step-by-step procedures
   - Concrete examples
   - Code snippets or templates

4. **Patterns & Checklists** (later)
   - Reusable templates
   - Verification checklist
   - Common decisions

5. **Anti-Patterns/When NOT to Use** (near end)
   - What to explicitly avoid
   - Boundary cases

6. **References** (last)
   - Related resources
   - Troubleshooting
   - Links to WorkMood docs

### Step 5: Add Supporting Resources (Optional)

Create additional files for:
- **examples/** — Real-world usage examples for your domain
- **templates/** — Copy-paste templates users can adapt
- **scripts/** — Helper scripts for automation
- **README.md** — Overview of the skill (for reference)

### Step 6: Test and Verify

1. **Check directory name** matches `name` in SKILL.md
2. **Verify YAML frontmatter** is valid (common: missing `---`, trailing colons)
3. **Test in VS Code chat**:
   ```
   Type `/` → Should see your skill in the menu
   Type `/<skill-name>` → Should auto-complete
   Invoke it → Should load instructions
   ```
4. **Ask Copilot questions** related to skill's domain → Should auto-load (if `disable-model-invocation: false`)

---

## WorkMood Skill Examples

### ✅ Good: TDD Skill

```yaml
---
name: tdd
description: Test-Driven Development guidance for WorkMood. Covers Red-Green-Refactor cycle, test naming conventions, Arrange-Act-Assert structure, and xUnit testing patterns with C#. Emphasizes tests as design and behavior specification.
argument-hint: "[feature/method to implement] [test scenario]"
user-invokable: true
disable-model-invocation: false
---
```

**Why it works:**
- ✅ Specific to WorkMood context
- ✅ Lists concrete use cases (RGR cycle, naming, patterns)
- ✅ Mentions tech stack (xUnit, C#)
- ✅ Clear when to apply (implementing features, designing with tests)

### ✅ Good: Code Smells Detection Skill

```yaml
---
name: code-smells-detection
description: Identify and address code smells in WorkMood. Teaches pattern recognition for long methods, inappropriate intimacy, magic values, duplicate logic, and MVVM violations. Guides incremental refactoring with safety-first approach.
argument-hint: "[code snippet or method] [context/concern]"
user-invokable: true
disable-model-invocation: false
---
```

**Why it works:**
- ✅ Domain-specific (code smells in WorkMood)
- ✅ Lists types of smells (long methods, MVVM violations, etc.)
- ✅ Implies when to use (code review, unclear code)
- ✅ Mentions approach (incremental, safety-first)

### ❌ Avoid: Generic or Vague Descriptions

```yaml
---
name: refactoring-guide
description: This skill helps with refactoring code.  # Too vague
argument-hint: "[code]"  # Unclear what code context means
user-invokable: true
disable-model-invocation: false
---
```

**Problems:**
- ❌ "helps with refactoring" is too broad
- ❌ No mention of methodology (incremental, test-driven, etc.)
- ❌ Agent won't know when to auto-load
- ❌ No specific use cases

---

## Content Guidelines

### Be Specific to Your Domain

**Bad:**
```
This skill helps you understand testing.
```

**Good (for WorkMood):**
```
Learn to write xUnit tests for MAUI ViewModels using the Arrange-Act-Assert pattern, 
with examples of mocking IMoodDataService and testing RelayCommand behavior.
```

### Include Concrete Examples

**Always show:**
- ✅ Before/after code or text
- ✅ Real examples from your codebase (reference WorkMood files)
- ✅ Common patterns you want to establish
- ✅ Anti-patterns to avoid

### Reference Resources from Your Skill Directory

Link to files in your skill directory using relative paths:

```markdown
See the [test template](./templates/test-template.cs) for structure.
Refer to [example scenarios](./examples/) for patterns.
```

### Make It Template-Ready

Provide copyable templates:

```markdown
## Test Template

Copy this template and adapt as needed:

\`\`\`csharp
public class YourComponentShould
{
    [Fact]
    public void WhenConditionExpectBehavior()
    {
        // Arrange
        var dependency = new Mock<IDependency>();
        
        // Act
        var result = dependency.DoSomething();
        
        // Assert
        Assert.Equal(expected, result);
    }
}
\`\`\`
```

---

## Progressive Disclosure: How Copilot Loads Skills

Skills use efficient, three-level loading:

### Level 1: Skill Discovery
- **When:** Copilot starts or user types `/`
- **Loads:** YAML frontmatter only (`name`, `description`, `argument-hint`)
- **Used for:** Deciding if skill is relevant, showing in menu
- **Cost:** Minimal (metadata only)

### Level 2: Instructions Loading
- **When:** User invokes skill or AI detects relevance
- **Loads:** Full `SKILL.md` body (instructions, examples, guidance)
- **Used for:** Providing detailed guidance
- **Cost:** Moderate (full markdown document)

### Level 3: Resource Access
- **When:** Copilot references external resources
- **Loads:** Scripts, templates, examples in skill directory
- **Used for:** Providing concrete templates, running helper scripts
- **Cost:** Varies (only what's needed)

**You can have many skills installed without context bloat** because only relevant content loads.

---

## Sharing Skills

### Share Within a Repository
- Store skills in `.github/skills/`, `.claude/skills/`, or `.agents/skills/`
- Team members get skills automatically

### Share Across Repositories
- Use the `chat.agentSkillsLocations` VS Code setting
- Point to a centralized location for shared skills

### Contribute to Community
- Share skills in [github/awesome-copilot](https://github.com/github/awesome-copilot) repository
- Or [anthropics/skills](https://github.com/anthropics/skills) reference skills repository

### Security Considerations
- ✅ Review shared skills before using (if they include scripts)
- ✅ VS Code's terminal tool has auto-approve safeguards
- ✅ Understand what scripts will do before execution

---

## Skill Maintenance Checklist

- [ ] **Naming**: Directory name = `name` field (lowercase, hyphens)
- [ ] **Frontmatter**: All required fields present and valid YAML
- [ ] **Description**: Specific to domain, mentions use cases, max 1024 chars
- [ ] **Structure**: Clear sections (Overview → Concepts → How-To → Examples → References)
- [ ] **Examples**: Concrete, from your domain, before/after patterns
- [ ] **Anti-patterns**: Explicit about what to avoid
- [ ] **Resource links**: Reference included files with relative paths
- [ ] **Testing**: Loadable in VS Code chat, works with `/skill-name`
- [ ] **Accuracy**: Content is current and reflects actual practice
- [ ] **Scope**: One focused domain, not scattered topics

---

## VS Code Agent Skills Documentation

For official documentation and examples:
- **Standard**: [agentskills.io](https://agentskills.io/)
- **VS Code guide**: [VS Code Agent Skills docs](https://code.visualstudio.com/docs/copilot/customization/agent-skills)
- **Reference skills**: [github/awesome-copilot](https://github.com/github/awesome-copilot) and [anthropics/skills](https://github.com/anthropics/skills)

---

## Troubleshooting

### Skill Not Appearing in Menu
- ✅ Check: Directory name = `name` field (case-sensitive, hyphens required)
- ✅ Check: YAML frontmatter valid (all `---` markers present)
- ✅ Check: `user-invokable: true` (default, but verify)
- ✅ Reload VS Code or use `/skills` to refresh

### Skill Not Auto-Loading
- ✅ Check: `disable-model-invocation: false` (default, but verify)
- ✅ Check: Description is specific enough (mentions use cases)
- ✅ Check: Your question actually matches the skill's domain
- ✅ Try explicit invocation: `/skill-name [your question]`

### Invalid YAML in Frontmatter
- ✅ Ensure colons after field names: `name: value`
- ✅ Ensure `---` on separate lines before and after metadata
- ✅ No trailing spaces after field values
- ✅ Use quotes for values with special characters: `description: "Contains: colons"`

### Resources Not Loading
- ✅ Use relative paths from SKILL.md directory: `[link](./templates/file.md)`
- ✅ Include file extension (e.g., `.md`, `.cs`)
- ✅ Verify file exists in subdirectory

---

## Next Steps

1. **Identify a domain** where you repeatedly give guidance
2. **Create the skill directory** and SKILL.md file
3. **Write frontmatter** with clear, specific description
4. **Draft content** with examples from your projects
5. **Add supporting resources** (templates, examples, scripts)
6. **Test in VS Code** chat
7. **Iterate** based on how well AI uses the skill

Your skills will improve over time as you refine them with real usage.
