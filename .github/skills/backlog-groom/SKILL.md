---
name: backlog-groom
description: Evaluates and grooms the open issue backlog — identifies stale issues, missing reproductions, possibly-fixed issues, and label gaps. Supports interactive one-at-a-time grooming and batch report generation.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh) installed and authenticated. Run `gh auth login` before using.
---

# Backlog Grooming Skill

This skill helps groom the open issue backlog in the dotnet/maui repository by:
1. Querying issues that need grooming attention
2. Assessing each issue's health (reproduction quality, staleness, linked PRs, labels)
3. Suggesting actions (add labels, post comments, close)
4. Presenting issues one-at-a-time for interactive grooming OR producing batch reports

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

## When to Use

- "Groom the backlog"
- "Find stale issues"
- "Which issues might already be fixed?"
- "Find issues missing reproduction steps"
- "Generate a backlog health report"
- "Evaluate issue #12345"
- "Show me issues that need labels"
- "Clean up old issues"

## How It Differs from Issue Triage

| | Issue Triage | Backlog Groom |
|--|-------------|---------------|
| **Focus** | NEW issues without milestones | EXISTING backlog (all open issues) |
| **Goal** | Assign milestones | Assess health, clean up, close stale |
| **Includes milestoned** | ❌ No | ✅ Yes |
| **Health scoring** | ❌ No | ✅ Yes (A-F grades) |
| **Repro quality check** | ❌ No | ✅ Yes |
| **Linked PR check** | Basic | Deep (merged = possibly fixed) |
| **Batch reports** | ❌ No | ✅ Yes |

## Grooming Workflow

**🚨 CRITICAL: ALWAYS use the skill scripts. NEVER use ad-hoc GitHub API queries.**

### Mode 1: Interactive Grooming (One at a Time)

#### Step 1: Query Issues to Groom

```bash
pwsh .github/skills/backlog-groom/scripts/query-backlog.ps1 -GroomType all -Limit 30 -OutputFormat groom
```

**Available groom types:**

| GroomType | What It Finds |
|-----------|---------------|
| `all` | All issues with any grooming flag (default) |
| `stale` | Issues with no activity for 6+ months |
| `needs-repro` | Issues missing code blocks, steps, or XAML |
| `possibly-fixed` | Issues with linked merged PRs |
| `missing-labels` | Issues missing platform or area labels |

#### Step 2: Present ONE Issue at a Time

**Present each issue using this format:**

```markdown
## Issue #XXXXX — Health: B (65/100)

**[Title]**

🔗 [URL]

| Field | Value |
|-------|-------|
| **Age** | 245 days (updated 180d ago) |
| **Milestone** | Backlog |
| **Platform** | android |
| **Area** | collectionview |
| **Repro Quality** | weak (score: 2) |
| **Linked PRs** | #54321 [MERGED], #54322 [OPEN] |
| **Flags** | 😴 stale, 📝 weak-repro, ✅ possibly-fixed |

**Suggestions:**
- No activity for 180 days. May need re-evaluation.
- Has merged PR(s): #54321. May already be fixed — verify and close.

---

What would you like to do?
```

#### Step 3: Wait for User Decision

Wait for user to say:
- "close" — Close the issue with a comment
- "comment" — Post a comment asking for update
- "label X" — Add a specific label
- "needs-repro" — Add `s/needs-repro` label
- "skip" or "next" — Move to next issue
- "apply" — Apply all suggested actions
- Custom instructions

#### Step 4: Move to Next Issue

After user decision, automatically present the NEXT issue.

#### Step 5: When Batch is Empty

**🚨 CRITICAL: Automatically reload more issues.**

```bash
pwsh .github/skills/backlog-groom/scripts/query-backlog.ps1 -GroomType all -Limit 30 -Skip <current_count> -OutputFormat groom
```

**DO NOT:**
- ❌ Stop and ask "Load more?"
- ❌ Use different GitHub queries
- ❌ Use `github-mcp-server-list_issues` directly

### Mode 2: Batch Report

Generate a comprehensive report for team review:

```bash
# Full report (all categories)
pwsh .github/skills/backlog-groom/scripts/generate-report.ps1 -Limit 50

# Stale issues report
pwsh .github/skills/backlog-groom/scripts/generate-report.ps1 -GroomType stale -Limit 100

# Platform-specific report
pwsh .github/skills/backlog-groom/scripts/generate-report.ps1 -Platform android -Limit 30

# Specific issues
pwsh .github/skills/backlog-groom/scripts/generate-report.ps1 -IssueNumbers "12345,67890,11111"
```

**Report output:**
- `CustomAgentLogsTmp/BacklogGroom/groom-report-YYYY-MM-DD-HHMM.md` — Markdown report
- `CustomAgentLogsTmp/BacklogGroom/groom-report-YYYY-MM-DD-HHMM.json` — JSON data

### Mode 3: Single Issue Assessment

Deep-assess a single issue:

```bash
# Summary view
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345

# JSON output (for programmatic use)
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345 -OutputFormat json

# Dry-run actions
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345 -ApplyActions

# Apply actions for real
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345 -ApplyActions -DryRun:$false
```

## Script Parameters

### query-backlog.ps1

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-GroomType` | stale, needs-repro, possibly-fixed, missing-labels, all | all | Type of grooming to perform |
| `-Platform` | android, ios, windows, maccatalyst, all | all | Filter by platform |
| `-Area` | Any area label | "" | Filter by area |
| `-Limit` | 1-500 | 50 | Maximum issues to return |
| `-MinAge` | 0+ | varies | Minimum age in days |
| `-OutputFormat` | table, json, markdown, groom | table | Output format |
| `-OutputFile` | file path | "" | Save results to file |
| `-Skip` | 0+ | 0 | Skip first N issues (pagination) |
| `-IncludeMilestoned` | switch | true | Include milestoned issues |

### assess-issue.ps1

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-IssueNumber` | any issue # | (required) | Issue to assess |
| `-OutputFormat` | summary, json, full | summary | Output format |
| `-ApplyActions` | switch | false | Apply recommended actions |
| `-DryRun` | bool | true | Show actions without applying |

### generate-report.ps1

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-IssueNumbers` | "N,N,N" | "" | Specific issues (skips query) |
| `-GroomType` | stale, needs-repro, etc. | all | Passed to query-backlog |
| `-Platform` | android, ios, etc. | all | Passed to query-backlog |
| `-Limit` | 1-500 | 25 | Maximum issues to assess |
| `-OutputDir` | path | CustomAgentLogsTmp/BacklogGroom | Report directory |
| `-ReportName` | string | timestamp | Report filename |

## Health Scoring

Each issue gets a health score (0-100) and letter grade:

| Grade | Score | Meaning |
|-------|-------|---------|
| A | 80-100 | Healthy — no action needed |
| B | 60-79 | Minor issues |
| C | 40-59 | Needs attention |
| D | 20-39 | Significant problems |
| F | 0-19 | Critical — immediate action needed |

**Score deductions:**

| Flag | Deduction | Description |
|------|-----------|-------------|
| very-stale | -30 | No activity for 1+ year |
| stale | -15 | No activity for 6+ months |
| possibly-fixed | -25 | Has merged PR(s) |
| needs-repro | -20 | No reproduction steps at all |
| weak-repro | -10 | Description but no code/steps |
| incomplete-template | -10 | Bug-report template fields left empty |
| no-platform | -5 | Missing platform label |
| no-area | -5 | Missing area label |
| no-milestone | -5 | No milestone assigned |

## Reproduction Quality Assessment

The skill checks issue bodies for 8 key quality factors (in priority order):

| # | Factor | Signal | Score | What It Means |
|---|--------|--------|-------|---------------|
| 1 | **Repro steps/link** | Reproduction link | +4 | Has link to public repo with reproduction |
| 1 | | Code blocks (```) | +3 | Has code reproduction |
| 1 | | Numbered steps | +3 | Has step-by-step repro |
| 1 | | XAML content | +2 | Has XAML sample |
| 2 | **Expected vs actual** | Expected/actual keywords | +3 | Clear behavior description |
| 3 | **Platform/OS details** | OS version (iOS 17, Android 14, etc.) | +2 | Has platform version details |
| 3 | | Device model / emulator info | +1 | Has device or emulator/simulator mention |
| 4 | **SDK/MAUI version** | .NET SDK / MAUI workload version | +2 | Has specific version info |
| 5 | **Regression info** | Regression keywords | +2 | Mentions regression or version that worked |
| 6 | **Stack trace/logs** | Exception / stack trace | +2 | Has error information |
| 7 | **Frequency** | Always/sometimes/intermittent | +1 | Has reproducibility frequency |
| 8 | **Workaround** | Workaround keywords | +1 | Mentions workaround availability |
| — | Screenshots | Image markdown | +1 | Visual evidence |
| — | Long description (500+ chars) | Body length | +1 | Detailed report |
| — | Very short (<100 chars) | Body length | -2 | Insufficient information |

**Template field penalties** (when using bug-report.yml):

| Empty Field | Penalty |
|-------------|---------|
| Steps to Reproduce | -3 |
| Reproduction link | -1 |
| Affected platforms | -1 |
| Affected platform versions | -1 |
| Version with bug | -1 |

**Quality levels:**
- **good** (10+): Thorough reproduction with multiple quality factors
- **fair** (5-9): Has some reproduction info
- **weak** (1-4): Description only, missing most quality factors
- **missing** (0 or less): Virtually no useful information

## Applying Actions

**🚨 CRITICAL: Always review before applying. Default is dry-run mode.**

### In Interactive Mode

Use `gh` CLI commands:

```bash
# Add a label
gh issue edit ISSUE_NUMBER --repo dotnet/maui --add-label "s/needs-repro"

# Post a comment
gh issue comment ISSUE_NUMBER --repo dotnet/maui --body "This issue has been open for over a year..."

# Close an issue
gh issue close ISSUE_NUMBER --repo dotnet/maui --comment "Closing: linked PR #XXXXX was merged..."
```

### Via assess-issue.ps1

```bash
# Dry run (see what would happen)
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345 -ApplyActions

# Apply for real
pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber 12345 -ApplyActions -DryRun:$false
```

## Common Mistakes to Avoid

| Mistake | Why It's Wrong | Correct Approach |
|---------|----------------|------------------|
| ❌ Using `github-mcp-server-list_issues` directly | Missing grooming filters and health assessment | ✅ Use `query-backlog.ps1` |
| ❌ Closing issues without checking | Merged PR might not fix THIS specific issue | ✅ Always verify before closing |
| ❌ Applying actions without user approval | Could mass-label/comment incorrectly | ✅ Present suggestions, wait for approval |
| ❌ Stopping when batch is empty | More issues likely available | ✅ Auto-reload with `-Skip` parameter |
| ❌ Assessing hundreds at once | API rate limits, slow | ✅ Batch 25-50 at a time |
