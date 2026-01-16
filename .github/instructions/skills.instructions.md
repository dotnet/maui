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

## Core Principle: Self-Contained Skills

**Skills should be self-contained and portable.** Each skill folder should include everything needed for that skill to function, making it easy to copy to other repositories or share with others.

## Skill Location

Skills must be placed in the `.github/skills/` directory:

- Standard location for GitHub-integrated projects
- Works with GitHub Copilot, Copilot CLI, and coding agents
- Enables automatic discovery by AI agents

## Required Directory Structure

Each skill MUST be a directory containing at minimum a `SKILL.md` file:

```
.github/skills/
└── skill-name/
    ├── SKILL.md          # Required - skill definition
    ├── scripts/          # Optional - executable scripts (self-contained)
    ├── assets/           # Optional - templates/resources
    └── references/       # Optional - documentation
```

**Important:** Scripts should be placed in the skill's own `scripts/` folder to maintain self-containment and support progressive disclosure.

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
2. **When to Use** - Trigger phrases and scenarios with keywords for agent discovery
3. **Instructions** - Step-by-step guidance for the agent
4. **Examples** - Usage examples with code blocks
5. **Parameters** (if applicable) - Table of script parameters
6. **Related Files** - Links to scripts, workflows, etc.

## Context Efficiency Best Practices

To support progressive disclosure and minimize token usage:

- **Keep SKILL.md under 500 lines** - Move detailed content to separate files
- **Use three-level loading**:
  1. Metadata (~100 tokens) - name/description loaded at startup
  2. Instructions (<5000 tokens) - SKILL.md content loaded when skill activates
  3. Resources (as needed) - scripts/references loaded on-demand
- **Move large examples** to `references/` or `assets/` and link to them
- **Keep file references one level deep** from SKILL.md (e.g., `scripts/validate.ps1`)

## Writing Effective Descriptions

Descriptions are critical for agent discovery. They should:

1. **Include specific keywords** that agents will match (e.g., "triage", "validate", "review")
2. **Specify WHEN to use** the skill (trigger scenarios)
3. **Specify WHAT the skill does** (capabilities)

**Examples:**

✅ Good: "Validates that UI tests correctly fail without a fix and pass with a fix. Use after assess-test-type confirms UI tests are appropriate."

❌ Bad: "Handles test stuff"

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

Skills can include scripts and integrate with GitHub Actions:

| Component | Location | Purpose |
|-----------|----------|---------|
| Skill definition | `.github/skills/<name>/SKILL.md` | Agent instructions |
| Skill scripts | `.github/skills/<name>/scripts/` | Self-contained automation |
| Shared utilities (rare) | `.github/scripts/` | Only if used by 5+ skills or workflows |
| GitHub Action | `.github/workflows/<workflow>.yml` | Scheduled/triggered automation |

## Script Organization Guidelines

### Default: Self-Contained Scripts (Recommended)

**Each skill should include its own complete scripts** in the `scripts/` folder:

```
.github/skills/
└── validate-ui-tests/
    ├── SKILL.md
    └── scripts/
        └── validate-regression.ps1    # Complete implementation
```

**Benefits:**
- ✅ Progressive disclosure works correctly
- ✅ Skill is portable (copy folder = copy skill)
- ✅ Clear ownership and maintenance
- ✅ No hidden dependencies

**Script template:**
```powershell
# .github/skills/<skill-name>/scripts/<script-name>.ps1

param(
    [Parameter(Mandatory=$true)]
    [string]$RequiredParam,

    [string]$OptionalParam = "default"
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           <skill-name> - Description                      ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Implementation here
# ...
```

### Script Best Practices

When writing scripts for skills:

1. **Error Handling**
   - Use `$ErrorActionPreference = "Stop"` (PowerShell) or `set -e` (Bash)
   - Provide clear error messages with actionable guidance
   - Exit with appropriate codes (0 = success, non-zero = failure)

2. **Edge Cases**
   - Validate input parameters
   - Handle missing files gracefully
   - Check for required tools/dependencies at script start

3. **Documentation**
   - Include `.SYNOPSIS` and `.DESCRIPTION` in PowerShell scripts
   - Add `--help` flag for Bash scripts
   - Document all parameters with examples

4. **Self-Contained or Documented**
   - Either include all logic in the script, OR
   - Clearly document external dependencies in SKILL.md (see Dependencies section above)

### Supported Script Languages

Skills can include scripts in various languages:

| Language | Extension | Use When | Notes |
|----------|-----------|----------|-------|
| PowerShell | `.ps1` | Windows-centric, .NET tooling | Used in this repository |
| Bash | `.sh` | Unix/Linux, system automation | Add shebang: `#!/bin/bash` |
| Python | `.py` | Cross-platform, data processing | Add shebang: `#!/usr/bin/env python3` |
| JavaScript/Node | `.js` | Frontend projects, npm tooling | Requires Node.js in environment |

**Note:** Agent support for languages varies by implementation. Document requirements in the `compatibility` field.

### Exception: Shared Infrastructure

**Only extract to `.github/scripts/` if:**
1. Used by **5 or more** skills/workflows
2. Truly infrastructure/utility (not skill-specific logic)
3. Maintenance benefits outweigh portability costs

**When using shared scripts, document dependencies clearly:**

```markdown
## Dependencies

This skill uses the shared infrastructure script:
- `.github/scripts/BuildAndRunHostApp.ps1` - Test runner for UI tests

See that file for additional requirements.
```

## How Agents Discover and Activate Skills

Understanding how agents select skills helps you write better descriptions:

1. **Discovery Phase** (Startup)
   - Agent loads `name` and `description` from all skills (~100 tokens each)
   - Creates an index of available capabilities

2. **Matching Phase** (User request)
   - Agent compares user prompt against skill descriptions
   - Matches keywords and trigger phrases
   - Multiple skills can activate if relevant

3. **Activation Phase** (Skill selected)
   - Full `SKILL.md` content loads into context (<5000 tokens)
   - Agent follows instructions step-by-step
   - Resources (scripts, examples) load on-demand

**This is why keyword-rich descriptions matter!** Skills are automatically discovered by agents—no manual registration required.

For human documentation purposes, you may optionally list skills in `.github/copilot-instructions.md`.

## Validation (Optional)

You can optionally validate skills using the skills-ref reference tool:

```bash
# Install
pip install skills-ref

# Validate a skill
skills-ref validate .github/skills/skill-name/
```

This checks:
- SKILL.md exists and has valid YAML frontmatter
- `name` matches folder and follows naming rules
- `description` is present and within limits
- File structure follows the specification

**Note:** skills-ref is a reference implementation for demonstration. For critical validation, manually review against the [specification](https://agentskills.io/specification).

## Security Considerations

When creating or using skills:

1. **Review Shared Skills**
   - Always review skills from external sources before using
   - Verify scripts don't contain malicious code
   - Check what permissions/tools scripts require

2. **Script Execution**
   - Skills may execute scripts via the terminal/agent tools
   - Document required permissions in `compatibility` field
   - Test scripts in isolation before deploying

3. **Sensitive Data**
   - Never hardcode credentials or secrets in skills
   - Use environment variables or secure credential stores
   - Document required environment setup in SKILL.md

## Troubleshooting

### Skill Not Activating

**Problem:** Agent doesn't use your skill when expected

**Solutions:**
1. Check description includes keywords from user's likely prompts
2. Verify `name` matches folder name exactly
3. Ensure YAML frontmatter is valid (use `skills-ref validate`)
4. Check skill location (must be in `.github/skills/`)

### Scripts Not Found

**Problem:** Agent references script but gets "file not found"

**Solutions:**
1. Use relative paths from SKILL.md (e.g., `scripts/my-script.ps1`)
2. Verify scripts have correct permissions (executable on Unix systems)
3. Check file actually exists in skill folder

### SKILL.md Too Long

**Problem:** Skill consumes too much context

**Solutions:**
1. Move detailed examples to `references/` folder
2. Move command reference to `assets/` folder
3. Link to external docs for comprehensive guides
4. Target <500 lines in SKILL.md

## Checklist for New Skills

- [ ] Created directory: `.github/skills/<skill-name>/`
- [ ] Created `SKILL.md` with valid YAML frontmatter
- [ ] `name` field matches directory name (lowercase, hyphenated)
- [ ] `description` includes keywords and explains when to use the skill
- [ ] SKILL.md is under 500 lines (move detailed content to references/)
- [ ] Markdown body includes instructions, examples, and usage
- [ ] Scripts are self-contained in `.github/skills/<name>/scripts/` folder
- [ ] Scripts document their parameters and usage
- [ ] Dependencies on shared scripts (if any) are documented in SKILL.md
- [ ] GitHub Action workflow created (if scheduled automation needed)

## Examples of Skill Structures

### Self-Contained Executable Skill (Recommended)

```
.github/skills/validate-ui-tests/
├── SKILL.md
└── scripts/
    └── validate-regression.ps1       # Complete self-contained implementation
```

### Information-Only Skill

```
.github/skills/assess-test-type/
├── SKILL.md                          # Decision framework only
└── references/                       # Optional reference docs
    └── test-type-examples.md
```

### Skill with Multiple Scripts

```
.github/skills/issue-triage/
├── SKILL.md
└── scripts/
    ├── query-issues.ps1              # Main script
    └── format-results.ps1            # Helper script
```

### Skill Using Shared Infrastructure (Exception)

```
.github/skills/validate-ui-tests/
├── SKILL.md                          # Documents dependency on BuildAndRunHostApp.ps1
└── scripts/
    └── validate-regression.ps1       # Calls ../../scripts/BuildAndRunHostApp.ps1

.github/scripts/
└── BuildAndRunHostApp.ps1            # Shared by 5+ skills/workflows
```
