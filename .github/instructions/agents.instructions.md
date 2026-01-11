---
applyTo: ".github/agents/**"
---

# Custom Agent Guidelines for Copilot CLI

Agents in this repo target **Copilot CLI** as the primary interface.

## Copilot CLI vs VS Code

| Property | CLI | VS Code | Use It? |
|----------|-----|---------|---------|
| `name` | ✅ | ✅ | Yes |
| `description` | ✅ | ✅ | **Required** |
| `tools` | ✅ | ✅ | Optional |
| `infer` | ✅ | ✅ | Optional |
| `handoffs` | ❌ | ✅ | **No** - VS Code only |
| `model` | ❌ | ✅ | **No** - VS Code only |
| `argument-hint` | ❌ | ✅ | **No** - VS Code only |

---

## Constraints

| Constraint | Limit |
|------------|-------|
| Prompt body | **30,000 characters** max |
| Name | 64 chars, lowercase, letters/numbers/hyphens only |
| Description | **1,024 characters** max, **required** |
| Body length | < 300 lines ideal, < 500 max |

### Name Format

- ✅ `pr`, `uitest-coding-agent`, `sandbox-agent`
- ❌ `PR-Reviewer` (uppercase), `pr_reviewer` (underscores), `--name` (leading/consecutive hyphens)

---

## Anti-Patterns (Do NOT Do)

| Anti-Pattern | Why It's Bad |
|--------------|--------------|
| **Too long/verbose** | Wastes context tokens, slower responses |
| **Vague description** | Won't be discovered via `/agent` |
| **No "when to use" section** | Users won't know when to invoke |
| **Duplicating copilot-instructions.md** | Already loaded automatically |
| **Explaining what skills do** | Reference skill, don't duplicate docs |
| **Large inline code samples** | Move to separate files |
| **ASCII art diagrams** | Consume tokens - use sparingly |
| **VS Code features** | `handoffs`, `model`, `argument-hint` don't work in CLI |
| **GUI references** | No "click button" - CLI is terminal-based |

---

## Best Practices

### Description = Discovery

The `/agent` command and auto-inference use description keywords:

```yaml
# ✅ Good
description: Reviews PRs with independent analysis, validates tests catch bugs, proposes alternative fixes

# ❌ Bad
description: Helps with code review stuff
```

### One Agent = One Role

- ✅ `pr` - Reviews and works on PRs
- ❌ `everything-agent` - Too broad

### Commands Over Concepts

```markdown
# ✅ Good
git fetch origin pull/XXXXX/head:pr-XXXXX && git checkout pr-XXXXX

# ❌ Bad
First you should fetch the PR and check it out locally
```

### Reference Skills, Don't Duplicate

```markdown
# ✅ Good
Run: `pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android`

# ❌ Bad
The skill does: 1. Detects fix files... 2. Detects test classes... [30 more lines]
```

---

## Tool Aliases

| Alias | Purpose |
|-------|---------|
| `execute` / `shell` | Run shell commands |
| `read` | Read file contents |
| `edit` / `write` | Modify files |
| `search` / `grep` | Search files/content |
| `agent` | Invoke other agents |

```yaml
tools: ["read", "search"]           # Read-only agent
tools: ["read", "search", "edit", "execute"]  # Full dev agent
```

---

## Minimal Structure

```yaml
---
name: my-agent
description: Does X when user asks Y. Keywords: review, test, fix.
---

# Agent Title

Brief philosophy.

## When to Use
- ✅ "trigger phrase"

## When NOT to Use
- ❌ Other task → Use `other-agent`

## Workflow
1. Step one
2. Step two

## Quick Reference
| Task | Command |
|------|---------|
| Do X | `command` |

## Common Mistakes
- ❌ **Mistake** - Why it's wrong
```

---

## Checklist

- [ ] YAML frontmatter with `name` and `description`
- [ ] `description` has trigger keywords
- [ ] Body under 500 lines
- [ ] No `handoffs`, `model`, `argument-hint`
- [ ] No GUI/button references
- [ ] Skills referenced, not duplicated
- [ ] "When to Use" / "When NOT to Use" included
