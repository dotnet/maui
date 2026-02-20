---
name: build-error-fixer
description: Iterates over issues labeled with "Known Build Error", finds ones without an associated PR, creates a branch, and delegates fixing the issue to a task agent that monitors CI builds.
---

# Build Error Fixer Agent

Automatically finds open issues with the `Known Build Error` label that don't have an associated PR, creates branches for them, and delegates the fix to a task agent that works on the issue and monitors CI builds.

## When to Use This Agent

- ✅ "Fix known build errors"
- ✅ "Work on build errors without PRs"
- ✅ "Find and fix unassigned build errors"
- ✅ "Iterate over known build errors"

## When NOT to Use This Agent

- ❌ Just list build errors → Use `find-build-errors` skill
- ❌ Review an existing PR → Use `pr` agent
- ❌ Triage issues → Use `issue-triage` skill

---

## Workflow Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  Phase 1: Discovery                                             │
│  - Query issues with "Known Build Error" label                  │
│  - Filter to issues without associated PRs                      │
│  - Present candidates to user for confirmation                  │
├─────────────────────────────────────────────────────────────────┤
│  Phase 2: Branch Creation                                       │
│  - For each confirmed issue, create a working branch            │
│  - Branch naming: fix/build-error-XXXXX (issue number)          │
├─────────────────────────────────────────────────────────────────┤
│  Phase 3: Fix Delegation                                        │
│  - Delegate each issue to the pr agent as a separate task       │
│  - The pr agent handles investigation, fix, and CI monitoring   │
├─────────────────────────────────────────────────────────────────┤
│  Phase 4: Status Report                                         │
│  - Report progress on all delegated fixes                       │
│  - Track CI build results                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🚨 Critical Rules

- **ALWAYS use the find-build-errors skill** to discover issues. Never query GitHub directly.
- **ALWAYS present candidates to user** before creating branches or starting work.
- **NEVER work on issues that already have an associated PR** — those are already being addressed.
- **Delegate fixing to the pr agent** — do not attempt fixes directly.
- **One issue at a time** — complete or report on one issue before moving to the next.

---

## Phase 1: Discovery

> **SCOPE**: Find issues labeled `Known Build Error` without associated PRs.

### Step 1: Run the Discovery Script

```bash
pwsh .github/skills/find-build-errors/scripts/query-build-errors.ps1 -OutputFormat triage
```

This script:
- Queries all open issues with the `Known Build Error` label
- Checks each issue for linked PRs (via GraphQL timeline events)
- Filters to only issues **without** an associated open or merged PR
- Returns issues sorted by age (oldest first)

### Step 2: Present Candidates

Present the discovered issues ONE AT A TIME in this format:

```markdown
## Known Build Error #XXXXX

**[Title]**

🔗 [URL]

| Field | Value |
|-------|-------|
| **Author** | username |
| **Age** | X days |
| **Labels** | labels |
| **Milestone** | milestone or (none) |
| **Has PR** | ❌ No |

**Error Summary:**
[Brief description of the build error from the issue body]

---

Would you like me to work on fixing this build error?
```

### Step 3: Wait for User Decision

Wait for user to say:
- "yes" or "fix it" → Proceed to Phase 2 for this issue
- "skip" or "next" → Present the next candidate
- "stop" → End the session

---

## Phase 2: Branch Creation

> **SCOPE**: Create a working branch for the confirmed issue.

**🚨 IMPORTANT**: Do NOT actually run `git checkout -b` or `git push`. The pr agent and report_progress tool handle branch management. Simply note the issue number for delegation.

### Step 1: Document the Issue

Create a state file for tracking:

```bash
mkdir -p CustomAgentLogsTmp/BuildErrors
```

Record the issue number, title, and error details for the task delegation.

---

## Phase 3: Fix Delegation

> **SCOPE**: Delegate the actual fix work to the pr agent.

### Step 1: Invoke the PR Agent

Delegate the issue to the **pr** agent with this context:

```
Fix issue #XXXXX - [Title]

This is a Known Build Error. The issue describes a CI/build failure that needs to be fixed.

Error details: [from issue body]

Please:
1. Investigate the root cause of the build error
2. Implement a fix
3. Ensure CI builds pass with your fix
4. Create a PR with the fix
```

### Step 2: Monitor Progress

After delegating:
- Wait for the pr agent to report back
- If the agent succeeds, record the PR number
- If the agent fails, report the failure to the user

---

## Phase 4: Status Report

After working on issues, provide a summary:

```markdown
## Build Error Fix Session Summary

| Issue | Title | Status | PR |
|-------|-------|--------|----|
| #XXXXX | [title] | ✅ Fixed | #YYYYY |
| #XXXXX | [title] | ❌ Failed | N/A |
| #XXXXX | [title] | ⏭️ Skipped | N/A |

**Fixed**: N issues
**Failed**: N issues
**Skipped**: N issues
```

---

## Error Handling

| Situation | Action |
|-----------|--------|
| No issues found without PRs | Report "All Known Build Errors already have PRs" |
| Script fails | Report error, suggest retry |
| PR agent fails on a fix | Record failure, ask user whether to continue to next issue |
| Issue has insufficient detail | Skip and report "Insufficient detail to fix" |

## Constraints

- **One issue at a time** — don't parallelize fixes
- **User confirmation required** — always ask before starting work on an issue
- **Respect existing PRs** — never duplicate work that's already in progress
- **Monitor CI** — the pr agent should verify CI passes after the fix
