---
name: issue-triage
description: Queries and triages open GitHub issues that need attention. Helps identify issues needing milestones, labels, or investigation.
metadata:
  author: dotnet-maui
  version: "2.3"
compatibility: Requires GitHub CLI (gh) installed and authenticated. Run `gh auth login` before using.
---

# Issue Triage Skill

This skill helps triage open GitHub issues in the dotnet/maui repository by:
1. Initializing a session with current milestones and labels
2. Loading a batch of issues into memory
3. Presenting issues ONE AT A TIME for triage decisions
4. Suggesting milestones based on issue characteristics
5. Tracking progress through a triage session

## Prerequisites

**GitHub CLI (gh) must be installed and authenticated:**

```bash
# Install
# Windows:
winget install --id GitHub.cli

# macOS:
brew install gh

# Linux:
# See https://cli.github.com/manual/installation

# Authenticate (required before first use)
gh auth login
```

The scripts will check for `gh` and exit with installation instructions if not found.

## When to Use

- "Find issues to triage"
- "Let's triage issues"
- "Grab me 10 issues to triage"
- "Triage Android issues"

## Triage Workflow

**🚨 CRITICAL: ALWAYS use the skill scripts. NEVER use ad-hoc GitHub API queries.**

The scripts have proper filters, exclusions, and milestone logic built-in. Don't bypass them.

### Step 1: Initialize Session

Start by initializing a session to load current milestones and labels:

```bash
pwsh .github/skills/issue-triage/scripts/init-triage-session.ps1
```

**What this does:**
- Fetches current milestones (SR4, SR5, etc.) from GitHub API
- Displays servicing milestones for reference during triage
- Creates session file to track progress

### Step 2: Load Issues Into Memory

**MANDATORY: Use query-issues.ps1 - it has the right filters!**

```bash
pwsh .github/skills/issue-triage/scripts/query-issues.ps1 -Limit 50 -OutputFormat triage
```

**What this does:**
- Queries GitHub with exclusion filters: `-label:s/needs-info -label:s/needs-repro -label:area-blazor -label:s/try-latest-version -label:s/move-to-vs-feedback`
- Returns issues without milestones
- Stores results for one-at-a-time presentation

**DON'T:**
- ❌ Use `github-mcp-server-list_issues` directly
- ❌ Use `github-mcp-server-search_issues` without the same filters
- ❌ Try to replicate the logic yourself - use the script!

### Step 3: Present ONE Issue at a Time

**IMPORTANT**: When user asks to triage, present only ONE issue at a time in this format:

```markdown
## Issue #XXXXX

**[Title]**

🔗 [URL]

| Field | Value |
|-------|-------|
| **Author** | username (Syncfusion if applicable) |
| **Platform** | platform |
| **Area** | area |
| **Labels** | labels |
| **Linked PR** | PR info with milestone if available |
| **Regression** | Yes/No |
| **Comments** | count |

**Comment Summary** (if any):
- [Author] Comment preview...

**My Suggestion**: `Milestone` - Reason (based on init session output)

---

What would you like to do with this issue?
```

### Step 4: Wait for User Decision

Wait for user to say:
- A milestone name (e.g., "Backlog", ".NET 10 SR5", ".NET 10 Servicing")
- "yes" to accept suggestion
- "skip" or "next" to move on without changes
- Specific instructions (e.g., "next SR and add regression label")

### Step 5: Move to Next Issue

After user decision, automatically present the NEXT issue.

### Step 6: When Batch is Empty

**🚨 CRITICAL: When you run out of issues, AUTOMATICALLY reload more issues.**

```bash
# Run query again to load next batch
pwsh .github/skills/issue-triage/scripts/query-issues.ps1 -Limit 50 -Skip <current_count> -OutputFormat triage
```

**DO NOT:**
- ❌ Stop and ask "Load more?"
- ❌ Say "No more issues found"
- ❌ Use different GitHub queries

**DO:**
- ✅ Automatically run `query-issues.ps1` again with `-Skip` parameter
- ✅ Continue presenting issues one at a time
- ✅ Only stop when query returns zero issues

## Script Parameters

### query-issues.ps1

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-Platform` | android, ios, windows, maccatalyst, all | all | Filter by platform |
| `-Area` | Any area label (e.g., collectionview, shell) | "" | Filter by area |
| `-Limit` | 1-1000 | 50 | Maximum issues to fetch |
| `-Skip` | 0+ | 0 | Skip first N issues (for pagination) |
| `-OutputFormat` | table, json, markdown, triage | table | Output format |
| `-RequireAreaLabel` | switch | false | Only return issues with area-* labels |
| `-SkipDetails` | switch | false | Skip fetching PRs/comments (faster) |

## Milestone Suggestion Logic

**🚨 CRITICAL: ALWAYS use actual milestone names from init-triage-session.ps1 output. NEVER guess or assume milestone names.**

The skill dynamically queries current milestones from dotnet/maui at session initialization. Milestone names change frequently (e.g., SR4, SR5, SR6), so **always reference the session output** when suggesting milestones.

### Suggestion Guidelines

| Condition | Suggested Milestone | Reason |
|-----------|---------------------|--------|
| Linked PR has milestone | PR's milestone | "PR already has milestone" |
| Has `i/regression` label | Highest numbered SR milestone | "Regression - needs servicing" |
| Has open linked PR | Current servicing milestone | "Has open PR" |
| Default | Backlog | "No PR, not a regression" |

**Example Session Output:**
```
Servicing Releases:
  - .NET 9 Servicing [246 open]
  - .NET 10 Servicing [213 open]
  - .NET 10 SR5 [55 open]         ← Use this for .NET 10 regressions
  - .NET 10.0 SR4 [103 open]

Other:
  - .NET 11 Planning [167 open]
  - .NET 11.0-preview1 [8 open]

Backlog:
  - Backlog [3037 open]
```

**How to suggest milestones:**
- ✅ "Assign to `.NET 10 SR5`" (from session output)
- ❌ "Assign to `.NET 10 SR2`" (guessing, might not exist)
- ❌ "Assign to current SR" (ambiguous, multiple active)

## Applying Triage Decisions

```bash
# Set milestone only
gh issue edit ISSUE_NUMBER --repo dotnet/maui --milestone "Backlog"

# Set milestone and add labels  
gh issue edit ISSUE_NUMBER --repo dotnet/maui --milestone "MILESTONE_NAME" --add-label "i/regression"

# Set milestone on both issue AND linked PR
gh issue edit ISSUE_NUMBER --repo dotnet/maui --milestone "MILESTONE_NAME"
gh pr edit PR_NUMBER --repo dotnet/maui --milestone "MILESTONE_NAME"
```

## Label Quick Reference

**Regression Labels:**
- `i/regression` - Confirmed regression
- `regressed-in-10.0.0` - Specific version

**Priority Labels:**
- `p/0` - Critical
- `p/1` - High
- `p/2` - Medium
- `p/3` - Low

**iOS 26 / macOS 26:**
- `version/iOS-26` - iOS 26 specific issue

## Common Mistakes to Avoid

| Mistake | Why It's Wrong | Correct Approach |
|---------|----------------|------------------|
| ❌ Using `github-mcp-server-list_issues` directly | Missing exclusion filters (needs-info, needs-repro, etc.) | ✅ Use `query-issues.ps1` script |
| ❌ Stopping when batch is empty | There are likely more issues available | ✅ Automatically run `query-issues.ps1 -Skip N` |
| ❌ Suggesting milestone names like "SR2" | Milestone doesn't exist, based on assumptions | ✅ Use actual milestone names from `init-triage-session.ps1` output |
| ❌ Asking "Load more?" when out of issues | Creates unnecessary interruption | ✅ Just load more automatically |
| ❌ Using ad-hoc API queries with custom filters | Likely to miss or include wrong issues | ✅ Trust the skill's scripts - they have the right logic |

## Session Tracking (Optional)

```bash
# Record triaged issue
pwsh .github/skills/issue-triage/scripts/record-triage.ps1 -IssueNumber 33272 -Milestone "Backlog"

# View session stats
cat CustomAgentLogsTmp/Triage/triage-*.json | jq '.Stats'
```
