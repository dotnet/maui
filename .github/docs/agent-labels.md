# Agent Workflow Labels

GitHub labels for tracking outcomes of the AI agent PR review workflow (`Review-PR.ps1`).

All labels use the **`s/agent-*`** prefix for easy querying on GitHub.

---

## Label Categories

### Outcome Labels

Mutually exclusive — exactly **one** is applied per PR review run.

| Label | Color | Description | Applied When |
|-------|-------|-------------|--------------|
| `s/agent-approved` | 🟢 `#2E7D32` | AI agent recommends approval — PR fix is correct and optimal | Report phase recommends APPROVE |
| `s/agent-changes-requested` | 🟠 `#E65100` | AI agent recommends changes — found a better alternative or issues | Report phase recommends REQUEST CHANGES |
| `s/agent-review-incomplete` | 🔴 `#B71C1C` | AI agent could not complete all phases (blocker, timeout, error) | Agent exits without completing all phases |

When a new outcome label is applied, any previously applied outcome label is automatically removed.

### Signal Labels

Additive — **multiple** can coexist on a single PR.

| Label | Color | Description | Applied When |
|-------|-------|-------------|--------------|
| `s/agent-gate-passed` | 🟢 `#4CAF50` | AI verified tests catch the bug (fail without fix, pass with fix) | Gate phase passes |
| `s/agent-gate-failed` | 🟠 `#FF9800` | AI could not verify tests catch the bug | Gate phase fails |
| `s/agent-fix-win` | 🟢 `#66BB6A` | AI found a better alternative fix than the PR | Fix phase: alternative selected over PR's fix |
| `s/agent-fix-lose` | 🟠 `#FF7043` | AI could not beat the PR fix — PR is the best among all candidates | Fix phase: PR selected as best after comparison |

Gate labels (`gate-passed`/`gate-failed`) are mutually exclusive with each other. Fix labels (`fix-win`/`fix-lose`) are mutually exclusive with each other.

### Tracking Label

Always applied on every completed agent run.

| Label | Color | Description | Applied When |
|-------|-------|-------------|--------------|
| `s/agent-reviewed` | 🔵 `#1565C0` | PR was reviewed by AI agent workflow (full 4-phase review) | Every completed agent run |

### Manual Label

Applied by MAUI maintainers, not by automation.

| Label | Color | Description | Applied When |
|-------|-------|-------------|--------------|
| `s/agent-fix-implemented` | 🟣 `#7B1FA2` | PR author implemented the agent's suggested fix | Maintainer applies when PR author adopts agent's recommendation |

---

## How It Works

### Architecture

```
Review-PR.ps1
├── Phase 1: PR Agent Review (Copilot CLI)
│   ├── Pre-Flight → writes content.md
│   ├── Gate       → writes content.md
│   ├── Fix        → writes content.md
│   └── Report     → writes content.md
├── Phase 2: PR Finalize (optional)
├── Phase 3: Post Comments (optional)
└── Phase 4: Apply Labels  ← labels are applied here
    ├── Parse content.md files
    ├── Determine outcome + signal labels
    ├── Apply via GitHub REST API
    └── Non-fatal: errors warn but don't fail the workflow
```

Labels are applied exclusively from `Review-PR.ps1` Phase 4. No other script applies agent labels. This single-source design avoids label conflicts and simplifies debugging.

### How Labels Are Parsed

The `Parse-PhaseOutcomes` function in `Update-AgentLabels.ps1` reads `content.md` files from each phase directory:

| Source File | What's Parsed | Resulting Label |
|-------------|---------------|-----------------|
| `gate/content.md` | `**Result:** ✅ PASSED` | `s/agent-gate-passed` |
| `gate/content.md` | `**Result:** ❌ FAILED` | `s/agent-gate-failed` |
| `try-fix/content.md` | `**Selected Fix:** Candidate ...` | `s/agent-fix-win` |
| `try-fix/content.md` | `**Selected Fix:** PR ...` | `s/agent-fix-lose` |
| `report/content.md` | `Final Recommendation: APPROVE` | `s/agent-approved` |
| `report/content.md` | `Final Recommendation: REQUEST CHANGES` | `s/agent-changes-requested` |
| *(missing report)* | No report file exists | `s/agent-review-incomplete` |

### Self-Bootstrapping

Labels are created automatically on first use via `Ensure-LabelExists`. No manual setup required. If a label already exists but has a stale description or color, it is updated.

---

## Querying Labels

All labels use the `s/agent-*` prefix, making them easy to filter on GitHub.

### Common Queries

```
# PRs the agent approved
is:pr label:s/agent-approved

# PRs where agent found a better fix
is:pr label:s/agent-fix-lose

# PRs where agent found better fix AND author implemented it
is:pr label:s/agent-changes-requested label:s/agent-fix-implemented

# PRs where tests don't catch the bug
is:pr label:s/agent-gate-failed

# Agent-reviewed PRs that are still open
is:pr is:open label:s/agent-reviewed

# All agent-reviewed PRs (total count)
is:pr label:s/agent-reviewed
```

### Metrics You Can Derive

| Metric | Query |
|--------|-------|
| Total agent reviews | `is:pr label:s/agent-reviewed` |
| Approval rate | Compare `label:s/agent-approved` vs `label:s/agent-changes-requested` counts |
| Gate pass rate | Compare `label:s/agent-gate-passed` vs `label:s/agent-gate-failed` counts |
| Fix win rate | Compare `label:s/agent-fix-win` vs `label:s/agent-fix-lose` counts |
| Agent adoption rate | `label:s/agent-fix-implemented` / `label:s/agent-changes-requested` |
| Incomplete review rate | `label:s/agent-review-incomplete` / `label:s/agent-reviewed` |

---

## Implementation Details

### Files

| File | Purpose |
|------|---------|
| `.github/scripts/shared/Update-AgentLabels.ps1` | Label helper module (all label logic) |
| `.github/scripts/Review-PR.ps1` | Orchestrator that calls `Apply-AgentLabels` in Phase 4 |
| `.github/agents/pr/SHARED-RULES.md` | Documents label system for the PR agent |

### Key Functions

| Function | Description |
|----------|-------------|
| `Apply-AgentLabels` | Main entry point — parses phases and applies all labels |
| `Parse-PhaseOutcomes` | Reads `content.md` files, returns outcome/gate/fix results |
| `Update-AgentOutcomeLabel` | Applies one outcome label, removes conflicting ones |
| `Update-AgentSignalLabels` | Adds/removes gate and fix signal labels |
| `Update-AgentReviewedLabel` | Ensures tracking label is present |
| `Ensure-LabelExists` | Creates or updates a label in the repository |

### Design Principles

- **Idempotent**: Safe to re-run — checks before add/remove, GitHub ignores duplicate adds
- **Non-fatal**: Label failures emit warnings but never fail the overall workflow
- **Single source**: All labels applied from `Review-PR.ps1` only — no other scripts touch labels
- **Self-bootstrapping**: Labels are created on first use via GitHub API
- **Mutual exclusivity enforced**: Outcome labels and same-category signal labels automatically remove their counterpart

---

## Migrated From

The following old infrastructure was removed as part of this implementation:

- **`Update-VerificationLabels`** function in `verify-tests-fail.ps1` — removed (labels now come from `Review-PR.ps1` only)
- **`s/ai-reproduction-confirmed`** / **`s/ai-reproduction-failed`** labels — superseded by `s/agent-gate-passed` / `s/agent-gate-failed`
