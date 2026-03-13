---
name: find-build-errors
description: Queries open GitHub issues labeled with "Known Build Error" and identifies those without associated PRs. Used by the build-error-fixer agent and for manual triage.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh) installed and authenticated. Run `gh auth login` before using.
---

# Find Build Errors Skill

This skill queries the dotnet/maui repository for open issues labeled with `Known Build Error` and identifies which ones do not have an associated pull request — making them candidates for automated fixing.

## When to Use

- "Find known build errors"
- "Show build errors without PRs"
- "What build errors need fixing?"
- "List unaddressed build errors"

## Quick Start

```bash
# Find all known build errors without PRs
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1

# Show all known build errors (including those with PRs)
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1 -IncludeWithPRs

# Output in JSON format
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1 -OutputFormat json

# Output in triage format (for agent consumption)
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1 -OutputFormat triage

# Limit results
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1 -Limit 5
```

## Script Parameters

| Parameter | Values | Default | Description |
|-----------|--------|---------|-------------|
| `-Limit` | 1-100 | 50 | Maximum issues to return |
| `-IncludeWithPRs` | switch | false | Include issues that already have linked PRs |
| `-OutputFormat` | table, json, markdown, triage | table | Output format |
| `-SortBy` | created, updated | created | Sort field |
| `-SortOrder` | asc, desc | asc | Sort order (asc = oldest first) |

## Output Fields

Each issue includes:
- **Number**: GitHub issue number
- **Title**: Issue title
- **Author**: Issue author
- **Age**: Days since created
- **Labels**: All labels on the issue
- **Milestone**: Assigned milestone (if any)
- **LinkedPRs**: Associated PRs (with state: OPEN, MERGED, CLOSED)
- **HasOpenPR**: Whether there's an open PR addressing this issue
- **ErrorSummary**: First 200 characters of the issue body for context

## Workflow with build-error-fixer Agent

The `build-error-fixer` agent uses this skill in Phase 1 (Discovery):

1. Runs `query-build-errors.ps1 -OutputFormat triage`
2. Filters to issues without open PRs
3. Presents candidates to user one at a time
4. Delegates confirmed issues to the pr agent for fixing

## Common Mistakes to Avoid

| Mistake | Why It's Wrong | Correct Approach |
|---------|----------------|------------------|
| ❌ Querying GitHub API directly | Missing PR linkage detection logic | ✅ Use `query-build-errors.ps1` |
| ❌ Working on issues with open PRs | Duplicates existing work | ✅ Filter with default (no `-IncludeWithPRs`) |
| ❌ Assuming all build errors are fixable | Some are infrastructure/flaky | ✅ Present to user for confirmation |
