---
name: pr-review
description: "End-to-end PR reviewer for dotnet/maui. Orchestrates 3 phases — Pre-Flight, Try-Fix, Report. Gate runs separately before this skill. Use when asked to 'review PR #XXXXX', 'work on PR #XXXXX', or 'fix issue #XXXXX'."
---

# PR Review — 3-Phase Orchestrator

End-to-end PR review workflow that orchestrates phases to explore independent fix alternatives and produce a recommendation.

**Trigger phrases:** "review PR #XXXXX", "work on PR #XXXXX", "fix issue #XXXXX"

> 🚨 **NEVER** use `gh pr review --approve` or `--request-changes`. AI agents must NEVER post review comments.
> 🚨 **DO NOT post any comments to the PR.** This skill only produces output files in `CustomAgentLogsTmp/PRState/`.

---

## Overview

```
Gate (pre-run)    → Already completed by Review-PR.ps1 before this skill runs
Phase 1: Pre-Flight   → Gather context, classify files, code review     → .github/pr-review/pr-preflight.md
Phase 2: Try-Fix      → ⚠️ MANDATORY multi-model exploration           → invoke try-fix skill (×4 models)
Phase 3: Report       → Write review recommendation                     → .github/pr-review/pr-report.md
```

> **Gate and Branch setup** are handled by `Review-PR.ps1` before this skill is invoked. The gate result is passed in the prompt. Do NOT re-run gate verification.

**All phases write output to:** `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/{phase}/content.md`
**Pre-Flight also writes:** `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/pre-flight/code-review.md`

---

## Critical Rules

- ❌ Never run `git checkout` or `git switch` to change branches — stay on the review branch set up by the caller
- ❌ Never stop and ask the user — use best judgment to skip blocked phases and continue
- ❌ Never mark a phase complete with pending fields
- ❌ **Never skip Phase 2 multi-model exploration — it is MANDATORY for every review, no exceptions**
- ❌ Never run git commands that change branch state during Phases 2-3 (scripts handle file manipulation)
- ❌ **Never duplicate phase content** — each phase writes ONLY to its own `content.md`. Do NOT copy gate results into try-fix or report content files.
- ✅ Always create `CustomAgentLogsTmp/` output files for every phase
- ✅ Always include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` in any commits
- ✅ Always use skills' scripts — don't bypass with manual commands
- ✅ Each phase's `content.md` must use the **exact template** from the phase instruction doc — no extra prose

### Multi-Model Configuration

Phase 2 uses these 4 AI models (run SEQUENTIALLY — they modify the same files):

| Order | Model |
|-------|-------|
| 1 | `claude-opus-4.6` |
| 2 | `claude-opus-4.7` |
| 3 | `gpt-5.3-codex` |
| 4 | `gpt-5.5` |

**🚨 MANDATORY: Use `mode: "sync"` for ALL try-fix task invocations.** Never use `mode: "background"`. Background mode causes the orchestrator to move on before the attempt finishes, which means `try-fix/content.md` is never written and try-fix results are lost from the PR comment. Each try-fix task MUST complete and return its result before you proceed to the next attempt or to the Phase 3 completion checklist.

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

Gather context from the issue, PR, comments, classify changed files, and **perform a deep code review** using the `code-review` skill.

Pre-Flight now has two parts:
- **Part A (Steps 1–6):** Context gathering — read issue, PR, comments, classify files
- **Part B (Step 7):** Code review — independence-first code analysis using `.github/skills/code-review/SKILL.md` and `.github/skills/code-review/references/review-rules.md`

**Outputs:**
- `pre-flight/content.md` — Context + code review summary
- `pre-flight/code-review.md` — Full code-review output (findings, blast radius, failure-mode probes, verdict)

**Gate:** None — always runs.

**Why code review runs here:** The code-review findings (❌ Errors, ⚠️ Warnings, failure-mode probes, blast radius) become **structured hints for Phase 2 (Try-Fix)**. Instead of each model starting from scratch, they receive concrete code concerns to address, leading to higher-quality fix exploration.

---

## Phase 2: Try-Fix → Invoke `try-fix` Skill (×4 Models)

> Read and follow `.github/skills/try-fix/SKILL.md`

> **⚠️ THIS PHASE IS MANDATORY. YOU MUST NEVER SKIP IT. NO EXCEPTIONS.**

Even if the PR's fix looks correct and Gate passed, you MUST still run all 4 models to explore alternative approaches. The purpose is to find the BEST fix, not just validate one.

### 🚨 CRITICAL: try-fix is Independent of PR's Fix

"Independent" means each model explores a **different fix approach** from the PR's fix — not that models are isolated from code-review context. Code-review findings are provided as advisory background to improve fix quality.

The purpose is NOT to re-test the PR's fix, but to:
1. **Generate independent fix ideas** — What would YOU do to fix this bug?
2. **Test those ideas empirically** — Actually implement and run tests
3. **Compare with PR's fix** — Is there a simpler/better alternative?
4. **Learn from failures** — Record WHY failed attempts didn't work

### Checklist (you MUST complete ALL of these)

- [ ] Attempt 1 launched with claude-opus-4.6
- [ ] `try-fix/content.md` updated with attempt 1 result
- [ ] Attempt 2 launched with claude-opus-4.7
- [ ] `try-fix/content.md` updated with attempt 2 result
- [ ] Attempt 3 launched with gpt-5.3-codex
- [ ] `try-fix/content.md` updated with attempt 3 result
- [ ] Attempt 4 launched with gpt-5.5
- [ ] `try-fix/content.md` updated with attempt 4 result
- [ ] Cross-pollination round completed (all models queried)
- [ ] Best fix selected with comparison table

### Round 1: Independent Exploration

For each model, invoke `try-fix` skill via a `general-purpose` task agent with that model:

```
prompt: |
  Invoke the try-fix skill for PR #XXXXX:
  - problem: {bug description from Pre-Flight}
  - platform: {platform from Platform Selection}
  - test_command: {test command from detected test type — use BuildAndRunHostApp.ps1 for UITest, Run-DeviceTests.ps1 for DeviceTest, dotnet test for UnitTest}
  - target_files:
    - src/{area}/{file1}.cs
    - src/{area}/{file2}.cs
  - hints: |
      Code review found the following concerns (advisory — use to inform your approach, not as a checklist):
      Errors:
        - {❌ Error finding 1 with file:line reference}
      # Include warnings ONLY if relevant to the root cause:
      # Warnings:
      #   - {⚠️ Warning — omit if unrelated to root cause}
      Failure modes:
        - {Failure mode 1}: {What happens in this scenario}
      Blast radius: {Summary — e.g., "Runs for ALL toolbar items at startup, not just badged ones"}
      Code review verdict: {LGTM / NEEDS_CHANGES / NEEDS_DISCUSSION} (confidence: {high/medium/low})

  Generate ONE independent fix idea. Review the PR's fix first to ensure your approach is DIFFERENT.
  "Independent" means exploring a different fix approach — the code review context above is background
  information to help you make better decisions, not a constraint on your exploration.
```

**Include code review context in the `hints` field** (try-fix's documented optional input). If Pre-Flight code review found no issues, use `hints: "Code review found no issues (verdict: LGTM)"`. If code review was SKIPPED, omit the `hints` field entirely.

**Selectivity:** Only include ❌ Error findings and failure-mode probes that are relevant to the bug being fixed. Omit 💡 Suggestions. Include ⚠️ Warnings only if directly related to the root cause.

**Wait for each to complete before starting the next.**

**🧹 MANDATORY: Clean up between attempts:**

```bash
# Restore baseline from previous attempt — this is the ONLY way to restore.
# Do NOT use manual git checkout/restore/reset commands.
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
```

**📝 MANDATORY: Update `try-fix/content.md` after EVERY attempt.** Do not wait until all attempts are done. After each try-fix attempt completes (pass or fail), immediately write/update `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/try-fix/content.md` with all results so far. This ensures the PR comment always reflects the latest try-fix state, even if a later attempt times out or the agent is interrupted.

### Round 2+: Cross-Pollination (MANDATORY)

After Round 1, invoke EACH model via task agent:
```
"Review PR #XXXXX fix attempts:
  - Attempt 1: {approach} - ✅/❌
  - Attempt 2: {approach} - ✅/❌
  ...
  Do you have any NEW fix ideas? Reply: 'NEW IDEA: {desc}' or 'NO NEW IDEAS'"
```

Run any new ideas as additional try-fix attempts. Repeat until all say "NO NEW IDEAS" (max 3 rounds).

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
- ❌ Running try-fix in parallel — SEQUENTIAL ONLY, always `mode: "sync"`
- ❌ Using `mode: "background"` for try-fix tasks — results will be lost
- ❌ Skipping cleanup between attempts — ALWAYS run cleanup commands
- ❌ Declaring exhaustion without querying all 4 models

---

## Phase 3: Report

> Read and follow `.github/pr-review/pr-report.md`

Deliver the final review recommendation.

> 🚨 **DO NOT post any comments.** All output goes to `CustomAgentLogsTmp/PRState/`.

**Gate:** Phases 1-2 must be complete.

---

## Output Directory Structure (MANDATORY)

```
CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/
├── pre-flight/
│   ├── content.md              # Phase 1 output (context + code review summary)
│   └── code-review.md          # Full code-review skill output (findings, blast radius, verdict)
├── gate/
│   └── content.md              # Gate output (pr-gate, run separately)
├── try-fix/
│   ├── content.md              # Phase 2 summary
│   └── attempt-{N}/            # Per-model attempt
│       ├── approach.md         # What was tried
│       ├── result.txt          # Pass / Fail / Blocked
│       ├── fix.diff            # git diff of changes
│       └── analysis.md         # Why it worked/failed
└── report/
    └── content.md              # Phase 3 output (pr-report)
```

---

## Quick Reference

| Phase | Instructions | Key Action | If Blocked |
|-------|--------------|------------|------------|
| Gate (pre-run) | `pr-gate.md` | Verify tests (run by Review-PR.ps1) | Result passed in prompt — if missing, document and continue |
| 1. Pre-Flight | `pr-preflight.md` | Read issue + PR context + **code review** | Skip missing info; if code review fails, set verdict to SKIPPED |
| 2. Try-Fix | `try-fix` skill (×4) | **4-model exploration with code-review hints (MANDATORY)** | Skip failing models, continue |
| 3. Report | `pr-report.md` | Write review recommendation | Never skip |

---

## Common Errors and Recovery

| Error | Cause | Fix |
|-------|-------|-----|
| `ENOENT: no such file` on skill | Dirty working tree from prior attempt | Run cleanup: `-Restore` + `git checkout HEAD -- .` + `git clean -fd --exclude=CustomAgentLogsTmp/` |
| Dirty working tree before attempt | Prior attempt didn't restore | Same cleanup as above |
| Build errors in unmodified files | Stale state | Cleanup + retry; if still fails, treat as environment blocker |
