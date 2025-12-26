---
applyTo: ".github/skills/**"
---

# Agent Skills Development Guidelines

This instruction file provides guidance for creating and modifying Agent Skills in the `.github/skills/` directory.

## Specification Reference

Agent Skills follow the open standard defined at:
- **Official Specification**: https://agentskills.io/specification
- **VS Code Documentation**: https://code.visualstudio.com/docs/copilot/customization/agent-skills
- **GitHub Documentation**: https://docs.github.com/en/copilot/concepts/agents/about-agent-skills

## Required Directory Structure

Each skill MUST be a directory containing at minimum a `SKILL.md` file:

```
.github/skills/
└── skill-name/
    ├── SKILL.md          # Required - skill definition
    ├── scripts/          # Optional - executable scripts
    ├── assets/           # Optional - templates/resources
    └── references/       # Optional - documentation
```

## SKILL.md Format

### Required YAML Frontmatter

Every `SKILL.md` MUST start with YAML frontmatter containing:

```yaml
---
name: skill-name
description: A clear description of what the skill does and when to use it.
---
```

### Required Frontmatter Fields

| Field | Requirements | Example |
|-------|--------------|---------|
| `name` | Lowercase, max 64 chars, letters/numbers/hyphens only. Must match folder name. | `deploy-staging` |
| `description` | Max 1024 chars. Explains what the skill does and when to use it. | `Deploys the application to the staging environment.` |

### Optional Frontmatter Fields

| Field | Purpose | Example |
|-------|---------|---------|
| `license` | License name or reference | `MIT` |
| `metadata` | Arbitrary key-value info | `author: my-org` |
| `compatibility` | Environment requirements | `Requires docker, kubectl` |
| `allowed-tools` | Pre-approved tools (experimental) | `curl jq` |

### Full Frontmatter Example

```yaml
---
name: deploy-staging
description: Deploys the application to the staging environment. Use when asked to deploy or release to staging.
license: MIT
metadata:
  author: my-org
  version: "1.0"
compatibility: Requires docker and kubectl. Must have cluster access configured.
---
```

## Markdown Body Structure

After the YAML frontmatter, include:

1. **Title** - `# Skill Name`
2. **When to Use** - Trigger phrases and scenarios
3. **Instructions** - Step-by-step guidance for the agent
4. **Examples** - Usage examples with code blocks
5. **Parameters** (if applicable) - Table of script parameters
6. **Related Files** - Links to scripts, workflows, etc.

## Name Validation Rules

✅ **Valid names**:
- `deploy-staging`
- `run-tests`
- `data-migration-v2`

❌ **Invalid names**:
- `Deploy-Staging` (uppercase)
- `-deploy-staging` (starts with hyphen)
- `deploy--staging` (consecutive hyphens)
- `deploy_staging` (underscores not allowed)

## Integration with Scripts and Workflows

Skills often have associated scripts and GitHub Actions:

| Component | Location | Purpose |
|-----------|----------|---------|
| Skill definition | `.github/skills/<name>/SKILL.md` | Agent instructions |
| Reusable script | `.github/scripts/<ScriptName>.ps1` | Actual automation logic |
| GitHub Action | `.github/workflows/<workflow>.yml` | Scheduled/triggered automation |

**Note**: Scripts can live in `.github/scripts/` (shared across skills) or within the skill's own `scripts/` subdirectory if they're specific to that skill.

## Registering Skills in copilot-instructions.md

After creating a skill, add it to the "Reusable Skills" section in `.github/copilot-instructions.md`:

```markdown
1. **skill-name** (`.github/skills/skill-name/SKILL.md`)
   - **Script**: `.github/scripts/ScriptName.ps1`
   - **Purpose**: Brief description
   - **Trigger phrases**: "phrase 1", "phrase 2"
   - **Usage**: `pwsh .github/scripts/ScriptName.ps1 -Param "value"`
```

## Checklist for New Skills

- [ ] Created directory: `.github/skills/<skill-name>/`
- [ ] Created `SKILL.md` with valid YAML frontmatter
- [ ] `name` field matches directory name (lowercase, hyphenated)
- [ ] `description` clearly explains when to use the skill
- [ ] Markdown body includes instructions, examples, and usage
- [ ] Associated script created (if applicable)
- [ ] GitHub Action workflow created (if scheduled automation needed)
- [ ] Skill registered in `.github/copilot-instructions.md`
