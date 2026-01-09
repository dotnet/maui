---
name: pr-finalize
description: Finalizes any PR for merge by verifying title and description match actual implementation. Ensures commit message helps future agents understand the change. Use on any PR before merge, when description may be stale, or to review commit message quality.
---

# PR Finalize

Ensures PR title and description accurately reflect the implementation for a good commit message.

**Standalone skill** - Can be used on any PR, not just PRs created by the pr agent.

## When to Use

- "Finalize PR #XXXXX" 
- "Check PR description for #XXXXX"
- "Review commit message for PR #XXXXX"
- Before merging any PR
- When PR implementation changed during review

## Usage

```bash
# Get current state (no local checkout required)
gh pr view XXXXX --json title,body
gh pr view XXXXX --json files --jq '.files[].path'

# Review commit messages (helpful for squash/merge commit quality)
gh pr view XXXXX --json commits --jq '.commits[].messageHeadline'

# Review actual code changes
gh pr diff XXXXX

# Optional: if the PR branch is checked out locally
git diff origin/main...HEAD
```

Then produce:
- Recommended PR title
- Recommended PR description (including the required NOTE block)
- Optional: suggestions to improve commit message quality (usually align PR title/body with the intended squash commit title/body)

## Title Requirements

| Requirement | Good | Bad |
|-------------|------|-----|
| Platform prefix (if specific) | `[iOS] Fix Shell back button` | `Fix Shell back button` |
| Describes behavior, not issue | `Fix long-press not triggering events` | `Fix #23892` |
| No noise prefixes | `[iOS] Fix...` | `[PR agent] Fix...` |

## Description Requirements

PR description should:
1. Start with the required NOTE block (so users can test PR artifacts)
2. Include the standard sections from `.github/PULL_REQUEST_TEMPLATE.md`
3. Match the actual implementation

```markdown
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Description of Change
[Must match actual implementation]

### Issues Fixed
Fixes #XXXXX
```

## Content for Future Agents

Add these elements so future agents can understand the change:

| Element | Purpose |
|---------|---------|
| **Root cause** | Why the bug occurred |
| **Fix approach** | What the code now does |
| **Key insight** | Non-obvious understanding that made fix work |
| **What to avoid** | Patterns that would re-break it |
| **Regression chain** | PRs that caused/affected this (if applicable) |
| **Related issues** | Issues verified not to regress |

## Common Issues

| Problem | Cause | Solution |
|---------|-------|----------|
| Description doesn't match code | Implementation changed during review | Rewrite description from actual diff |
| Missing root cause | Author focused on "what" not "why" | Add root cause from issue/analysis |
| References wrong approach | Started with A, switched to B | Update to describe final approach |

## Output Template

```markdown
# Recommended PR Title

[Platform] Brief description of behavior fix

---

# Recommended PR Description

<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Description of Change

[One-line summary]

**Root cause:** [Why bug occurred]

**Fix:** [What code now does]

**Key insight:** [Non-obvious understanding]

**Regression chain:** (if applicable)
| PR | What happened |
|-----|---------------|
| #XXXXX | Caused regression |

**What to avoid:** [Patterns that would re-break]

### Issues Fixed

Fixes #XXXXX

**Related:** #YYYYY (verified not regressed ✅)
```
