---
name: find-reviewable-pr
description: Finds open PRs in the dotnet/maui and dotnet/docs-maui repositories that are good candidates for review, prioritizing by milestone, priority labels, partner/community status.
metadata:
  author: dotnet-maui
  version: "2.1"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui and dotnet/docs-maui repositories.
---

# Find Reviewable PR

This skill searches the dotnet/maui and dotnet/docs-maui repositories for open pull requests that are good candidates for review, prioritized by importance.

## When to Use

- "Find a PR to review"
- "Find PRs that need review"
- "Show me milestoned PRs"
- "Find partner PRs to review"
- "What community PRs are open?"
- "Find docs-maui PRs to review"

## Priority Categories (in order)

1. **Priority (P/0)** - Critical priority PRs that need immediate attention
2. **Milestoned** - PRs assigned to SR3, SR4, or Servicing milestones
3. **Partner** - PRs from Syncfusion and other partners
4. **Community** - External contributions needing review
5. **Recent** - PRs created in the last 2 weeks
6. **docs-maui** - Documentation PRs (5 priority + 5 recent by default)

## Quick Start

```bash
# Find all reviewable PRs (shows top from each category including docs-maui)
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1

# Find only milestoned PRs
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category milestoned

# Find only docs-maui PRs
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category docs-maui

# Find Android PRs only
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Platform android

# Limit results per category
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Limit 5

# Adjust docs-maui limit (default is 5 per sub-category)
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -DocsLimit 10
```

## Script Parameters

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-Category` | milestoned, priority, recent, partner, community, docs-maui, all | all | Filter by category |
| `-Platform` | android, ios, windows, maccatalyst, all | all | Filter by platform |
| `-Limit` | 1-100 | 10 | Max PRs per category (maui repo) |
| `-DocsLimit` | 1-100 | 5 | Max PRs per sub-category for docs-maui |
| `-OutputFormat` | review, table, json | review | Output format |

## Workflow for Reviewing PRs

### Step 1: Find PRs to Review

**CRITICAL**: You MUST use the PowerShell script below. Do NOT attempt to query GitHub directly with `gh` commands or `jq` if the script fails. The script contains important prioritization logic (SR3 before SR4, P/0 first, etc.) that cannot be replicated with ad-hoc queries.

```bash
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Limit 5
```

**If the script fails** (e.g., HTTP 502, network error, authentication issue):
1. **STOP** - Do not attempt fallback queries
2. **Report the error** to the user
3. **Suggest retry** - Ask user to try again in a few minutes (GitHub API may be temporarily unavailable)

**Why no fallbacks?** Ad-hoc queries bypass the milestone prioritization logic and will return incorrectly ordered results (e.g., SR4 PRs before SR3 PRs).

### Step 2: Present ONE PR at a Time

When user asks to review, present only ONE PR in this format:

```markdown
## PR #XXXXX

**[Title]**

🔗 [URL]

| Field | Value |
|-------|-------|
| **Author** | username |
| **Platform** | platform |
| **Complexity** | Easy/Medium/Complex |
| **Milestone** | milestone or (none) |
| **Age** | X days |
| **Files** | X (+additions/-deletions) |
| **Labels** | labels |
| **Category** | priority/milestoned/partner/community/recent |

Would you like me to review this PR?
```

### Step 3: Invoke PR Reviewer

When user confirms, use the **pr-reviewer** agent:
- "Review PR #XXXXX"

## Complexity Levels

| Complexity | Criteria |
|------------|----------|
| **Easy** | ≤5 files, ≤200 additions |
| **Medium** | 6-15 files, or 200-500 additions |
| **Complex** | >15 files, or >500 additions |

## Tips

- **P/0 PRs** should be reviewed first - they're blocking releases
- **Milestoned PRs** have deadlines and should be prioritized
- **Partner PRs** often have business priority
- **Community PRs** may need more guidance and thorough review
- **Recent PRs** - quick turnaround keeps contributors engaged
