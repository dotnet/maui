# PR Report — Final Recommendation

> **SCOPE:** Deliver the review recommendation. Output files only — no comments posted.

> 🚨 **DO NOT post any comments.** This phase only produces output files.

> 🚨 **DO NOT duplicate content from other phases.** Reference gate/try-fix results by status only (e.g., "Gate: ✅ PASSED") — do NOT copy their full output into report/content.md.

---

## Prerequisites

- Phases 1-2 (Pre-Flight, Try-Fix) must be complete before starting
- Gate result is available from the prompt (ran separately before this skill)

---

## Steps

1. **Determine recommendation:**

   | Condition | Recommendation |
   |-----------|----------------|
   | PR's fix selected and Gate passed | `✅ APPROVE` |
   | Alternative fix found via Try-Fix | `⚠️ REQUEST CHANGES` — suggest alternative |
   | Gate failed | `⚠️ REQUEST CHANGES` — fix doesn't work |

2. **Write output files** — Save recommendation to `content.md`

> 🚨 **DO NOT post comments.** This phase only produces output files.
>
> 🚨 **DO NOT run pr-finalize.** That is a separate skill invoked only when the user explicitly requests it.

---

## Output File

```bash
mkdir -p CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/report
```

Write `content.md`:
```markdown
## {✅/⚠️} Final Recommendation: {APPROVE/REQUEST CHANGES}

### Phase Status
| Phase | Status | Notes |
|---|---|---|
| Pre-Flight | ✅ COMPLETE | {notes} |
| Gate | ✅ PASSED | {platform} |
| Try-Fix | ✅ COMPLETE | {N} attempts, {M} passing |
| Report | ✅ COMPLETE | |

### Summary
{Brief summary of the review}

### Root Cause
{Root cause analysis}

### Fix Quality
{Assessment of the fix}
```

---

## Agent Labels (Automated)

After Report completes, `Review-PR.ps1` automatically applies labels based on `content.md` files:

| Label | When Applied |
|-------|-------------|
| `s/agent-approved` | Report recommends APPROVE |
| `s/agent-changes-requested` | Report recommends REQUEST CHANGES |
| `s/agent-review-incomplete` | Agent didn't complete all phases |
| `s/agent-gate-passed` | Gate phase passes |
| `s/agent-gate-failed` | Gate phase fails |
| `s/agent-fix-win` | Agent found a better alternative |
| `s/agent-fix-pr-picked` | PR's fix was best |
| `s/agent-reviewed` | Every completed run |

Standard markers in content.md: `✅ PASSED`, `❌ FAILED`, `Selected Fix: PR`, `Final Recommendation: APPROVE`.

---

## Common Mistakes

- ❌ Rushing the report — take time for clear justification
- ❌ Running git commands — user handles commit/push
- ❌ Posting comments — this phase only produces output files, never posts to GitHub
