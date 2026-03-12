---
name: pr-review
description: "End-to-end PR reviewer for dotnet/maui. Orchestrates 4 phases — Pre-Flight, Gate, Try-Fix, Report. Use when asked to 'review PR #XXXXX', 'work on PR #XXXXX', or 'fix issue #XXXXX'."
---

# PR Review — 4-Phase Orchestrator

End-to-end PR review workflow that orchestrates phases to verify tests, explore independent fix alternatives, and produce a recommendation.

**Trigger phrases:** "review PR #XXXXX", "work on PR #XXXXX", "fix issue #XXXXX"

> 🚨 **NEVER** use `gh pr review --approve` or `--request-changes`. AI agents must NEVER post review comments.
> 🚨 **DO NOT post any comments to the PR.** This skill only produces output files in `CustomAgentLogsTmp/PRState/`.

---

## Overview

```
Phase 1: Pre-Flight   → Gather context, classify files                 → .github/pr-review/pr-preflight.md
Phase 2: Gate         → ⛔ MUST PASS — verify tests FAIL/PASS          → .github/pr-review/pr-gate.md
Phase 3: Try-Fix      → ⚠️ MANDATORY multi-model exploration           → invoke try-fix skill (×2 models)
Phase 4: Report       → Write review recommendation                     → .github/pr-review/pr-report.md
```

> **Branch setup** is handled by `Review-PR.ps1` before this skill is invoked. By the time this skill runs, the review branch already exists with the PR commits cherry-picked and squashed.

**All phases write output to:** `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/{phase}/content.md`

---

## Critical Rules

- ❌ Never run `git checkout` or `git switch` to change branches — stay on the review branch set up by the caller
- ❌ Never stop and ask the user — use best judgment to skip blocked phases and continue
- ❌ Never mark a phase complete with pending fields
- ❌ **Never skip Phase 3 multi-model exploration — it is MANDATORY for every review, no exceptions**
- ❌ Never run git commands that change branch state during Phases 2-3 (scripts handle file manipulation)
- ✅ Always create `CustomAgentLogsTmp/` output files for every phase
- ✅ Always include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` in any commits
- ✅ Always use skills' scripts — don't bypass with manual commands

### Multi-Model Configuration

Phase 3 uses these 2 AI models (run SEQUENTIALLY — they modify the same files):

| Order | Model |
|-------|-------|
| 1 | `claude-sonnet-4.6` |
| 2 | `claude-opus-4.6` |

### Environment Blockers

| Blocker Type | Max Retries | Then Do |
|--------------|-------------|---------|
| Missing tool/driver | 1 install attempt | Skip phase, continue |
| Server errors (500, timeout) | 1 retry | Skip phase, continue |
| Port conflicts | 1 (kill process) | Skip phase, continue |
| Build failures in try-fix | 2 attempts | Skip remaining models, proceed to Report |
| Configuration issues | 1 fix attempt | Skip phase, continue |

---

## Phase 1: Pre-Flight

> Read and follow `.github/pr-review/pr-preflight.md`

Gather context from the issue, PR, comments, and classify changed files.

**Gate:** None — always runs.

---

## Phase 2: Gate

> Read and follow `.github/pr-review/pr-gate.md`

Verify that the PR's tests actually catch the bug (FAIL without fix, PASS with fix).

**Gate:** Pre-Flight must be ✅ COMPLETE.

**If Gate fails:**
- Tests PASS without fix → Tests don't catch the bug. Proceed to Try-Fix anyway.
- Tests FAIL with fix → PR's fix doesn't work. Skip Try-Fix, proceed to Report.

---

## Phase 3: Try-Fix → Invoke `try-fix` Skill (×2 Models)

> Read and follow `.github/skills/try-fix/SKILL.md`

> **⚠️ THIS PHASE IS MANDATORY. YOU MUST NEVER SKIP IT. NO EXCEPTIONS.**

Even if the PR's fix looks correct and Gate passed, you MUST still run all 2 models to explore alternative approaches. The purpose is to find the BEST fix, not just validate one.

### 🚨 CRITICAL: try-fix is Independent of PR's Fix

The purpose is NOT to re-test the PR's fix, but to:
1. **Generate independent fix ideas** — What would YOU do to fix this bug?
2. **Test those ideas empirically** — Actually implement and run tests
3. **Compare with PR's fix** — Is there a simpler/better alternative?
4. **Learn from failures** — Record WHY failed attempts didn't work

### Checklist (you MUST complete ALL of these)

- [ ] Attempt 1 launched with claude-sonnet-4.6
- [ ] Attempt 2 launched with claude-opus-4.6
- [ ] Cross-pollination round completed (both models queried)
- [ ] Best fix selected with comparison table

### Round 1: Independent Exploration

For each model, invoke `try-fix` skill via a `general-purpose` task agent with that model:

```
prompt: |
  Invoke the try-fix skill for PR #XXXXX:
  - problem: {bug description from Pre-Flight}
  - platform: {platform from Platform Selection}
  - test_command: pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform {platform} -TestFilter "IssueXXXXX"
  - target_files:
    - src/{area}/{file1}.cs
    - src/{area}/{file2}.cs

  Generate ONE independent fix idea. Review the PR's fix first to ensure your approach is DIFFERENT.
```

**Wait for each to complete before starting the next.**

**🧹 MANDATORY: Clean up between attempts:**

```bash
# 1. Restore any baseline state from previous attempt
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore

# 2. Restore all tracked files to HEAD
git checkout HEAD -- .

# 3. Remove untracked files
git clean -fd --exclude=CustomAgentLogsTmp/
```

### Round 2+: Cross-Pollination (MANDATORY)

After Round 1, invoke EACH model via task agent:
```
"Review PR #XXXXX fix attempts:
  - Attempt 1: {approach} - ✅/❌
  - Attempt 2: {approach} - ✅/❌
  ...
  Do you have any NEW fix ideas? Reply: 'NEW IDEA: {desc}' or 'NO NEW IDEAS'"
```

Run any new ideas as additional try-fix attempts. Repeat until both say "NO NEW IDEAS" (max 3 rounds).

### Selecting the Best Fix

Compare all passing candidates on:
1. **Must pass tests** — Only consider ✅ PASS candidates
2. **Simplest solution** — Fewer files, fewer lines
3. **Most robust** — Handles edge cases
4. **Matches codebase style** — Consistent with existing patterns

### Output File

```bash
mkdir -p CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/try-fix
```

Write `content.md`:
```markdown
### Fix Candidates
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | try-fix | {approach} | ✅/❌ | 1 file | {insight} |
| ... | ... | ... | ... | ... | ... |
| PR | PR #XXXXX | {approach} | ✅ PASSED (Gate) | 2 files | Original PR |

### Cross-Pollination
| Model | Round | New Ideas? | Details |
|-------|-------|------------|---------|
| ... | 2 | Yes/No | {idea or "NO NEW IDEAS"} |

**Exhausted:** {Yes/No}
**Selected Fix:** {PR's fix / Candidate #N} — {Reason}
```

### Common Mistakes

- ❌ Looking at PR's fix before generating ideas — generate independently first
- ❌ Running try-fix in parallel — SEQUENTIAL ONLY
- ❌ Skipping cleanup between attempts — ALWAYS run cleanup commands
- ❌ Declaring exhaustion without querying all 5 models

---

## Phase 4: Report

> Read and follow `.github/pr-review/pr-report.md`

Deliver the final review recommendation.

> 🚨 **DO NOT post any comments.** All output goes to `CustomAgentLogsTmp/PRState/`.

**Gate:** Phases 1-3 must be complete.

---

## Output Directory Structure (MANDATORY)

```
CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/
├── pre-flight/
│   └── content.md              # Phase 1 output (pr-preflight)
├── gate/
│   └── content.md              # Phase 2 output (pr-gate)
├── try-fix/
│   ├── content.md              # Phase 3 summary
│   └── attempt-{N}/            # Per-model attempt
│       ├── approach.md         # What was tried
│       ├── result.txt          # Pass / Fail / Blocked
│       ├── fix.diff            # git diff of changes
│       └── analysis.md         # Why it worked/failed
└── report/
    └── content.md              # Phase 4 output (pr-report)
```

---

## Quick Reference

| Phase | Instructions | Key Action | If Blocked |
|-------|--------------|------------|------------|
| 1. Pre-Flight | `pr-preflight.md` | Read issue + PR context | Skip missing info, continue |
| 2. Gate | `pr-gate.md` | Verify tests via task agent | Document, continue to Try-Fix |
| 3. Try-Fix | `try-fix` skill (×2) | **2-model exploration (MANDATORY)** | Skip failing models, continue |
| 4. Report | `pr-report.md` | Write review recommendation | Never skip |

---

## Common Errors and Recovery

| Error | Cause | Fix |
|-------|-------|-----|
| `ENOENT: no such file` on skill | Dirty working tree from prior attempt | Run cleanup: `-Restore` + `git checkout HEAD -- .` + `git clean -fd --exclude=CustomAgentLogsTmp/` |
| Dirty working tree before attempt | Prior attempt didn't restore | Same cleanup as above |
| Build errors in unmodified files | Stale state | Cleanup + retry; if still fails, treat as environment blocker |
