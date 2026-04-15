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
   | PR's fix selected and Gate passed and code review LGTM | `✅ APPROVE` |
   | PR's fix selected and Gate passed but code review NEEDS_DISCUSSION | `⚠️ REQUEST CHANGES` — include code review concerns |
   | Code review verdict is NEEDS_CHANGES (any ❌ errors) | `⚠️ REQUEST CHANGES` — code review found errors |
   | Alternative fix found via Try-Fix | `⚠️ REQUEST CHANGES` — suggest alternative |
   | Gate failed | `⚠️ REQUEST CHANGES` — fix doesn't work |

   **🚨 Hard gate:** If the code review (from Pre-Flight) has verdict `NEEDS_CHANGES`, the final recommendation MUST be `REQUEST CHANGES` regardless of Gate or Try-Fix results. Code-review ❌ Errors cannot be overridden by passing tests alone.

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
| Code Review | {verdict} ({confidence}) | {error_count} errors, {warning_count} warnings |
| Gate | ✅ PASSED | {platform} |
| Try-Fix | ✅ COMPLETE | {N} attempts, {M} passing |
| Report | ✅ COMPLETE | |

### Code Review Impact on Try-Fix
{Brief description of how code-review findings influenced try-fix exploration. Did any model specifically address a code review ❌ Error? Did failure-mode probes reveal issues that guided fix approaches?}

### Summary
{Brief summary of the review}

### Root Cause
{Root cause analysis}

### Fix Quality
{Assessment of the fix — informed by both gate results and code review findings}
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
