---
name: creating-agents
description: Step-by-step guidance for creating custom Copilot agents in VS Code, including YAML frontmatter, file structure, handoffs, and best practices. Use when you want to define a new persona, workflow, or specialized agent for your workspace or team.
argument-hint: "[agent purpose] [desired tools or workflow]"
user-invokable: true
disable-model-invocation: false
---

# Creating Custom Copilot Agents in VS Code

## When to Use This Skill
- When you want to create a new custom agent (persona, workflow, or specialized role) for Copilot Chat in VS Code.
- When you need to define agent instructions, available tools, handoffs, or agent-specific workflows.
- When you want to share or maintain agents for your team or workspace.

## Core Concepts
- **Custom agents** let you tailor Copilot's behavior for specific tasks, roles, or workflows.
- Agents are defined in `.agent.md` files with YAML frontmatter and Markdown instructions.
- You can control available tools, handoffs, and model selection per agent.
- Agents can be workspace-specific, user-profile, or organization-wide.

## How-To: Create a Custom Agent

### 1. Create the Agent File
- Place your agent file in `.github/agents/` (workspace), your user profile, or organization folder.
- Name it with a `.agent.md` extension (e.g., `my-reviewer.agent.md`).

### 2. Add YAML Frontmatter
Example:
```yaml
---
name: my-reviewer
# Optional: If omitted, file name is used
model: GPT-4.1 (copilot)
description: Code review agent for security and quality. Suggests improvements and flags issues.
tools: [search, fetch, read, apply_patch, runTests]
argument-hint: "[file or code] [review focus]"
user-invokable: true
disable-model-invocation: false
target: vscode
handoffs:
  - label: Handoff to Implementation
    agent: implementation
    prompt: Implement the changes suggested above.
    send: false
    model: GPT-4.1 (copilot)
---
```
- **Fields:**
  - `name`: Unique agent name (lowercase, hyphens, max 64 chars)
  - `description`: What the agent does and when to use it (max 1024 chars)
  - `tools`: List of allowed tools (e.g., search, fetch, read, apply_patch, runTests)
  - `argument-hint`: Input hint for chat
  - `model`: (Optional) Preferred model(s)
  - `user-invokable`: Show in agent picker (default true)
  - `disable-model-invocation`: Prevent as subagent (default false)
  - `target`: vscode or github-copilot
  - `handoffs`: (Optional) List of next-step actions

### 3. Write Agent Instructions (Markdown Body)
- Below the YAML, add persona, workflow, and usage instructions.
- Use sections for principles, decision-making, communication style, etc.
- Reference tools with `#tool:<tool-name>` if needed.

### 4. (Optional) Add Handoffs
- Use `handoffs` in YAML to guide users to the next agent or workflow step.
- Each handoff includes a label, target agent, prompt, and options.

### 5. Test and Share
- In VS Code, use the agents dropdown or `/agents` to manage and test agents.
- Ensure your agent appears and loads as expected.
- Share agent files in your repo, user profile, or organization for team use.

## Patterns & Templates
- Use clear, specific descriptions and argument hints.
- Limit tool access to only what's needed for the agent's purpose.
- Use handoffs for multi-step workflows (e.g., Plan → Implement → Review).
- Keep instructions concise and actionable.

## Anti-Patterns / When NOT to Use
- Don't use agents for one-off instructions (use custom instructions instead).
- Avoid vague descriptions or tool lists (be explicit for best results).
- Don't overload a single agent with unrelated roles or workflows.

## References
- [VS Code Copilot Custom Agents](https://code.visualstudio.com/docs/copilot/customization/custom-agents)
- [VS Code Agent Skills Docs](https://code.visualstudio.com/docs/copilot/customization/agent-skills)
- [agentskills.io](https://agentskills.io/)
- [github/awesome-copilot](https://github.com/github/awesome-copilot)
- [anthropics/skills](https://github.com/anthropics/skills)
