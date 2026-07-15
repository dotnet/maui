---
name: analyze-sessions
description: >-
  Analyzes your local Copilot CLI sessions for dotnet/maui to drive iterative
  improvements to the PR-review agent (and other agents, skills, and instruction
  files). Runs a select → extract → score → judge → cluster → propose → emit-eval
  loop: a deterministic core ranks your worst / most-expensive sessions, then the
  agent rubric-tags recurring failure modes, proposes concrete repo edits, and
  emits a vally guard-eval per failure mode so each one becomes a regression test.
  Triggers on: "analyze my recent maui sessions", "what's making my agent runs
  expensive", "find failure modes in my Copilot sessions", "turn my session
  failures into guard evals". LOCAL-ONLY — never uploads, shares, or posts
  transcripts. Do NOT use for: reviewing a single PR (use pr-review), running
  tests, or analyzing a GitHub issue.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires pwsh 7+, sqlite3, and dotnet-replay (auto-resolved with pinned dnx v0.9.1 fallback)
---

# Analyze Sessions

Mines your local Copilot CLI session logs to find where agents waste effort or
fail, then turns those findings into **concrete repo edits + regression evals**.
It automates — for the whole fleet of your local sessions — the same loop the
team ran by hand over 64 CI sessions (`CONSOLIDATED_FINDINGS.md`) and the
guard-eval mechanism shipped in PR #36002.

**Trigger phrases:** "analyze my recent maui sessions for agent improvements",
"what's making my Copilot runs expensive / fail", "find recurring failure modes
in my sessions", "turn my session failures into guard evals".

**Do NOT use for:** reviewing a single PR (use `pr-review`), running tests,
investigating CI failures (use `azdo-build-investigator`), or any informational
question — answer those directly.

> **Privacy contract (non-negotiable):** This skill is **local-only**. It reads
> `~/.copilot/...` and writes a **redacted** report into your session workspace.
> It NEVER opens a gist, NEVER POSTs a transcript, and NEVER ships session data
> to a third-party endpoint. The LLM-judge step runs **inside your own Copilot
> session** (your auth, your quota). Any cross-machine sharing is explicit,
> manual, opt-in — see [Privacy & safety](#privacy--safety).

## Architecture — one engine, two front doors

A deterministic PowerShell **shared core** does the heavy, reproducible work
(select → extract → score → digest + redact). The **judgment** work (tag →
cluster → propose → emit-eval) is done by *you, the agent*, reading the core's
redacted output — no third-party endpoint is involved.

```
                  ┌──────────────────────────────────────────────┐
  local front door │  scripts/Get-SessionAnalysis.ps1 (NO LLM)    │
  -Repository/-Last│   select → extract → score → digest → redact │
  -SessionId  ─────►│   • dotnet-replay --summary --json (primary)│
                   │   • thin raw events.jsonl scan (supplemental)│
  CI front door    │   emits: session-analysis.md + .json contract│
  -EventsDir   ────►│                                              │
  -EventsPath      └───────────────────┬──────────────────────────┘
                                       │ redacted digests + ranking
                                       ▼
                   ┌──────────────────────────────────────────────┐
   agent, in your   │  judge → cluster → propose → emit-eval        │
   own session ─────►│   (rubric tagging, learn-from-pr taxonomy,   │
                   │    vally guard-eval per recurring mode)       │
                   └──────────────────────────────────────────────┘
```

The **same core** powers the existing CI-session pipeline: point it at downloaded
AzDO `events.jsonl` artifacts with `-EventsDir` / `-EventsPath` and it skips the
local DB select entirely. See `references/design-rationale.md`.

## Inputs

| Input | Required | Default | Notes |
|-------|----------|---------|-------|
| Repository | No | `dotnet/maui` | Filters `session-store.db` |
| Last N | No | `10` | Most recently-updated sessions |
| Session id(s) | No | — | One or more explicit GUIDs (`-SessionId`) |
| Since | No | — | ISO date; `updated_at >= Since` |
| Top K | No | `5` | How many worst sessions get full digests |
| Events path/dir | No | — | CI front door (`-EventsPath` / `-EventsDir`) |

## Outputs

1. **Ranked report** (`session-analysis.md`) — sessions ordered worst-first by a
   transparent cost/pain score, plus a redacted digest per worst session (intent
   flow, tool histogram, failure events with event turn IDs (or a stable
   assistant-turn fallback).
2. **JSON contract** (`session-analysis.json`) — machine-readable per-session
   metrics + ranking (also emitted to stdout with `-Json`).
3. **Failure-mode analysis** — your rubric tags + clusters with frequency.
4. **Proposals** — concrete edits to `.github/instructions/*`, `.github/skills/*`,
   and agent files (learn-from-pr taxonomy).
5. **Guard evals** — one `vally` eval per recurring failure mode, under the
   relevant `<skill>/tests/`, so the failure becomes a regression test.

## The loop — 6 phases

### Phase 1 — Select & extract & score (deterministic core)

Run the shared core. It selects sessions, normalizes them via `dotnet-replay`,
scores them, and writes the redacted report + JSON.

```bash
# Most-recent local maui sessions (writes report into your session workspace):
pwsh -NoProfile -File .github/skills/analyze-sessions/scripts/Get-SessionAnalysis.ps1 \
  -Last 15 -Top 5 -OutputDir "$ARTIFACTS_DIR" -Json
```

```bash
# Specific sessions:
pwsh -NoProfile -File .github/skills/analyze-sessions/scripts/Get-SessionAnalysis.ps1 \
  -SessionId <guid-a> -SessionId <guid-b> -Top 2 -OutputDir "$ARTIFACTS_DIR"
```

```bash
# CI front door — already-downloaded AzDO events.jsonl artifacts:
pwsh -NoProfile -File .github/skills/analyze-sessions/scripts/Get-SessionAnalysis.ps1 \
  -EventsDir ./downloaded-sessions -Top 8 -Json
```

> `dotnet-replay` is resolved automatically: if `replay` is on `PATH` it's used,
> otherwise the core falls back to `dnx --yes dotnet-replay@0.9.1`. Override with
> `-ReplayCommand` if needed.
> A preinstalled `replay` command or explicit `-ReplayCommand` remains under the
> caller's version control; only the automatic download fallback is pinned.

**Scoring (transparent, in the core's `$Weights`):** higher = more pain/cost.
`2·tool_failures + 1.5·retries + 5·(errors+aborts) + 3·truncations +
4·subagent_failures + tokens/50k + tool_calls/50 + min(duration,7200)/600`.
Wall-clock is capped because resumed sessions report multi-day calendar spans.

### Phase 2 — Surface the worst

Read `session-analysis.md`. Focus on the **Top K** digests. Prefer the metrics +
the minimal quoted snippets the core already extracted; **do not** re-open raw
transcripts unless a digest is ambiguous (re-opening risks pulling in un-redacted
text and burns context).

### Phase 3 — Judge (rubric tagging, per worst session)

For each worst session, tag failure modes against this rubric, **citing the exact
turn index / tool call** the core surfaced:

| # | Rubric question | Failure mode if "no" |
|---|-----------------|----------------------|
| 1 | Did it achieve the user's goal? | `goal-miss` |
| 2 | Minimal steps, or thrashing? | `inefficient-path` |
| 3 | Right tool for each job? | `wrong-tool` |
| 4 | Avoided repeating a failed command? | `repeated-failure` |
| 5 | Followed MAUI conventions (branch rules, PR note block, platform file naming)? | `convention-violation` |
| 6 | Avoided hallucinated paths/APIs? | `hallucination` |
| 7 | Recovered from errors gracefully? | `poor-recovery` |
| 8 | Stayed under context pressure (few truncations)? | `context-thrash` |

Cite evidence as `session <shortId> · turn <n> · <tool>` so every tag is
falsifiable against the digest.

### Phase 4 — Cluster

Group tags **across** sessions into recurring modes with a frequency count
(e.g. "`repeated-failure` on `bash` git push — 4/15 sessions"). A mode is
**recurring** if it appears in ≥ 2 sessions, or is severe (`goal-miss` /
`convention-violation`) in even one. Only recurring/severe modes proceed.

### Phase 5 — Propose (learn-from-pr taxonomy)

For each recurring cluster, write a concrete proposal targeting a **real file**:

| Field | Content |
|-------|---------|
| **Category** | Instruction file · Skill · Agent file · Architecture doc · Inline comment · Linting |
| **Priority** | High · Medium · Low |
| **Location** | Exact path, e.g. `.github/instructions/android.instructions.md` or `.github/skills/pr-review/SKILL.md` |
| **Specific Change** | The precise edit (quote the line/section) |
| **Why It Helps** | Tie back to the cited sessions/turns |

Map clusters to targets the way `learn-from-pr` does: behavioral rules →
`.github/instructions/*`; skill-workflow gaps → that skill's `SKILL.md`; agent
orchestration → the agent file. Write the proposals into a Markdown report in the
session workspace. **Do not silently apply edits** — present them; apply only
what the user approves (mirrors `learn-from-pr`'s analysis-vs-apply split).

### Phase 6 — Emit-eval (close the loop)

This is what makes the loop *iterative*. For each recurring failure mode, emit a
`vally` guard-eval under the **relevant skill's** `tests/` directory, named
`eval.<short-mode>.vally.yaml`, using the PR #36002 house pattern:

- A **refutation-proof structural floor**: force the agent to end with a
  structured token line (e.g. `BRANCH_TARGET: main`) and assert it via
  `output-matches`.
- **One LLM judge** (`type: prompt`, `scoring: scale_1_5`, `threshold: 0.6`) so
  the judge carries ~half the weight.

Template:

```yaml
name: <skill>-<mode>-guard
description: Regression guard for <failure mode> observed in session analysis.
version: "1.0"
type: capability
defaults:
  runs: 1
  model: claude-opus-4.6
  judge_model: claude-opus-4.6
  executor: copilot-sdk
stimuli:
  - name: <mode>-floor
    prompt: |
      <scenario that reproduces the failure mode>
      End your response with exactly one line: `<TOKEN>: <value>`
    graders:
      - type: output-matches
        pattern: '<TOKEN>:\s*<expected>'
      - type: prompt
        scoring: scale_1_5
        rubric:
          - <what a correct, non-regressing answer must do>
scoring:
  threshold: 0.6
```

Then validate every emitted file:

```bash
npx -y @microsoft/vally-cli@0.6.0 lint --eval-spec <path-to-eval> --strict
```

## Privacy & safety

- **Local-only by default.** The core reads `~/.copilot/...` and writes to
  `-OutputDir`. It has **no** network egress and **no** share flag.
- **Redaction is on by default.** Home paths → `~`, tokens (`ghp_`/`gho_`/
  `Bearer`/`password=`/`key=`), and emails are stripped from the report **and**
  must stay stripped in any emitted eval. `-NoRedact` exists only for local
  debugging — never use it for anything that leaves your machine.
- **Output contract.** The Markdown report and JSON contract apply redaction to
  all dynamic strings, including session metadata and tool/skill identifiers.
  Redaction also covers AWS keys, Slack tokens, JWTs, and private-key blocks.
- **The judge is you.** Tagging/clustering happen in your own Copilot session.
  Do not paste transcripts into any external tool.
- **Cross-machine sharing is opt-in and manual.** If the user explicitly asks to
  share findings (gist, Kusto, dashboard), confirm first, share only the
  **redacted** report, and never the raw `events.jsonl`.

## When NOT to use

- Reviewing a specific PR → `pr-review` / `code-review`.
- Investigating CI / build / Helix failures → `azdo-build-investigator`.
- Extracting lessons from one finished PR → `learn-from-pr`.
- Any "how does X work?" question → answer directly; do not launch analysis.

## Completion criteria

- [ ] Core ran; `session-analysis.md` + `.json` written to the workspace.
- [ ] Worst sessions rubric-tagged with cited turns.
- [ ] Recurring modes clustered with frequency.
- [ ] ≥ 1 concrete proposal in learn-from-pr taxonomy targeting a real file.
- [ ] ≥ 1 `vally` guard-eval emitted and passing `lint --strict`.
- [ ] Nothing uploaded/shared; report is redacted.
