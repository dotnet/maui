---
name: issue-triage
description: Queries and triages open GitHub issues that need attention. Helps identify issues needing milestones, labels, or investigation.
metadata:
  author: dotnet-maui
  version: "2.1"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# Issue Triage Skill

This skill helps triage open GitHub issues in the dotnet/maui repository by:
1. Initializing a session with current milestones and labels
2. Loading a batch of issues into memory
3. Presenting issues ONE AT A TIME for triage decisions
4. Suggesting milestones based on issue characteristics
5. Tracking progress through a triage session

## When to Use

- "Find issues to triage"
- "Let's triage issues"
- "Grab me 10 issues to triage"
- "Triage Android issues"

## Triage Workflow

### Step 1: Initialize Session

Start by initializing a session to load current milestones and labels:

```bash
pwsh .github/skills/issue-triage/scripts/init-triage-session.ps1
```

### Step 2: Load Issues Into Memory

Load a batch of issues (e.g., 20) but DO NOT display them all. Store them for one-at-a-time presentation:

```bash
pwsh .github/skills/issue-triage/scripts/query-issues.ps1 -Limit 100 -OutputFormat triage
```

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

**My Suggestion**: `Milestone` - Reason

---

What would you like to do with this issue?
```

### Step 4: Wait for User Decision

Wait for user to say:
- A milestone name (e.g., "Backlog", "current SR", "Servicing")
- "yes" to accept suggestion
- "skip" or "next" to move on without changes
- Specific instructions (e.g., "next SR and add regression label")

### Step 5: Apply Changes and Move to Next

After applying changes, automatically present the NEXT issue.

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

The script dynamically queries current milestones from dotnet/maui and suggests them based on issue characteristics:

| Condition | Suggested Milestone | Reason |
|-----------|---------------------|--------|
| Linked PR has milestone | PR's milestone | "PR already has milestone" |
| Has `i/regression` label | Current SR milestone (soonest due) | "Regression - current SR milestone" |
| Has open linked PR | Servicing milestone (or next SR) | "Has open PR" |
| Default | Backlog | "No PR, not a regression" |

**Note**: SR milestones are sorted by due date, so the soonest SR is suggested for regressions. Milestone names change monthly, so they are queried dynamically rather than hardcoded.

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

## Common Milestone Types

| Milestone Type | Use When |
|----------------|----------|
| Current SR (e.g., SR3) | Regressions, critical bugs with PRs ready |
| Next SR (e.g., SR4) | Important bugs, regressions being investigated |
| Servicing | General fixes with PRs, non-urgent improvements |
| Backlog | No PR, not a regression, nice-to-have fixes |

**Note**: Use `init-triage-session.ps1` to see current milestone names.

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

## Session Tracking (Optional)

```bash
# Record triaged issue
pwsh .github/skills/issue-triage/scripts/record-triage.ps1 -IssueNumber 33272 -Milestone "Backlog"

# View session stats
cat CustomAgentLogsTmp/Triage/triage-*.json | jq '.Stats'
```
