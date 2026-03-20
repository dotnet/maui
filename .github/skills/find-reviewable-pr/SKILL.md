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

1. **Priority (P/0)** - Critical priority PRs that need immediate attention (always on top)
2. **Approved (Not Merged)** - PRs with human approval that haven't been merged yet
3. **Ready To Review (Project Board)** - PRs in "Ready To Review" column of the MAUI SDK Ongoing project board (requires `read:project` scope)
4. **Milestoned** - PRs assigned to current milestone(s), sorted by lowest SR number first (e.g., SR5 before SR6), then Servicing
5. **Agent Reviewed** - PRs reviewed by AI agent workflow (detected via labels)
6. **Partner** - PRs from Syncfusion and other partners
7. **Community** - External contributions needing review
8. **Recent Waiting for Review** - PRs created in last 2 weeks that need review (minimum 5)
9. **docs-maui Waiting for Review** - Documentation PRs needing review (minimum 5)

## Quick Start

```bash
# Find P/0 and milestoned PRs (default behavior, excludes changes-requested)
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1

# Find all reviewable PRs across all categories
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category all

# Find only milestoned PRs
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category milestoned

# Find only docs-maui PRs waiting for review
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category docs-maui

# Find approved PRs waiting to be merged
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category approved

# Find PRs in "Ready To Review" on the project board
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category ready-to-review

# Find agent-reviewed PRs with merge summaries
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category agent-reviewed

# Find recent PRs waiting for review
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Category recent

# Find Android PRs only
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Platform android

# Limit results per category
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -Limit 5

# Adjust docs-maui limit (default is 5, minimum enforced)
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1 -DocsLimit 10
```

## Script Parameters

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-Category` | default, milestoned, priority, recent, partner, community, docs-maui, approved, ready-to-review, agent-reviewed, all | default | Filter by category. `default` shows only P/0 + milestoned, excluding changes-requested PRs. |
| `-Platform` | android, ios, windows, maccatalyst, all | all | Filter by platform |
| `-Limit` | 1-100 | 100 | Max PRs per category (maui repo) |
| `-RecentLimit` | 1-100 | 5 | Max recent PRs waiting for review from maui repo (minimum 5 enforced) |
| `-DocsLimit` | 1-100 | 5 | Max PRs for docs-maui waiting for review (minimum 5 enforced) |
| `-ExcludeAuthors` | string[] | (none) | Exclude PRs from specific authors (e.g., `-ExcludeAuthors PureWeen,rmarinho`) |
| `-IncludeAuthors` | string[] | (none) | Only include PRs from specific authors |
| `-OutputFormat` | review, table, json | review | Output format |

## Workflow for Reviewing PRs

### Step 1: Find PRs to Review

**CRITICAL**: You MUST use the PowerShell script below. Do NOT attempt to query GitHub directly with `gh` commands or `jq` if the script fails. The script contains important prioritization logic (SR5 before SR6, P/0 first, etc.) that cannot be replicated with ad-hoc queries.

```bash
pwsh .github/skills/find-reviewable-pr/scripts/query-reviewable-prs.ps1
```

**If the script fails** (e.g., HTTP 502, network error, authentication issue):
1. **STOP** - Do not attempt fallback queries
2. **Report the error** to the user
3. **Suggest retry** - Ask user to try again in a few minutes (GitHub API may be temporarily unavailable)

**Why no fallbacks?** Ad-hoc queries bypass the milestone prioritization logic and will return incorrectly ordered results (e.g., later SR milestones before earlier ones).

### Step 2: Check for Warnings

**IMPORTANT**: If the script output contains a warning about missing `read:project` scope, you MUST include this warning at the TOP of your response to the user:

```
⚠️ **Note**: Your GitHub token is missing the `read:project` scope. Project board data is not available.
To enable: `gh auth refresh -s read:project`
```

### Step 3: Present Results from ALL Categories

**CRITICAL**: When presenting PR results, you MUST include PRs from ALL categories returned by the script:

1. 🔴 **Priority (P/0)** - Always include if present (always first)
2. 🟢 **Approved (Not Merged)** - Always include if present
3. 📋 **Ready To Review (Board)** - Always include if present
4. 📅 **Milestoned** - Always include if present  
5. 🤖 **Agent Reviewed** - Always include if present
6. 🤝 **Partner** - Always include if present
7. ✨ **Community** - Always include if present
8. 🕐 **Recent** - Always include if present
9. 📖 **docs-maui** - Always include if present

**DO NOT** omit any category. Each category table should include columns for: PR, Title, Author, Assignees, Platform/Repo, Status, Agent Review, Age, Updated.

## Complexity Levels

| Complexity | Criteria |
|------------|----------|
| **Easy** | ≤5 files, ≤200 additions |
| **Medium** | 6-15 files, or 200-500 additions |
| **Complex** | >15 files, or >500 additions |

## Tips

- **P/0 PRs** should always be reviewed first - they're blocking releases
- **Approved PRs** are ready to merge - verify CI is green and merge
- **Ready To Review PRs** are in the project board pipeline and need timely review
- **Agent Reviewed PRs** have been analyzed by the AI agent workflow - check their labels for status
- **Milestoned PRs** have deadlines and should be prioritized
- **Partner PRs** often have business priority
- **Community PRs** may need more guidance and thorough review
- **Recent PRs** - quick turnaround keeps contributors engaged
