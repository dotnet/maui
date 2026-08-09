# Design rationale — `analyze-sessions`

This document records the concrete facts the implementation depends on and states
the privacy model. It is reference material for maintainers of the skill — not
part of the runtime path.

## Why this skill exists

Copilot CLI writes a complete, append-only event log per session at
`~/.copilot/session-state/<id>/events.jsonl`, covering prompts, assistant turns,
tool calls (with arguments and success), intents, subagents, skills, errors,
output tokens, mode changes, compaction, and task completion. It is a rich signal
about how agents actually behave and needs **zero setup**. This skill turns that
latent signal into a repeatable improvement loop that any contributor can run in
natural language — and turns recurring failures into **regression evals** so the
loop ratchets forward instead of re-discovering the same problems.

PR #36002 established the **emit-eval** half: a hand-written `vally` guard-eval
(`eval.gh-auth.vally.yaml`) froze a fixed failure as a regression test. This skill
joins that guard-eval mechanism with analysis over the *whole fleet* of a
contributor's local sessions.

## Build on `dotnet-replay`, don't re-implement a parser

`dotnet-replay` (NuGet, by Larry Ewing / `lewing`,
<https://github.com/lewing/dotnet-replay>, **v0.9.1**) is a public, purpose-built
reader for Copilot CLI `events.jsonl` (also Claude Code sessions and `waza` eval
transcripts). It is the **normalization layer** — we wrap it rather than maintain
our own JSONL parser.

- Zero-install (explicit opt-in): `dnx --yes dotnet-replay@0.9.1 …`. Or `dotnet
  tool install -g dotnet-replay` → `replay …`. The core uses the pinned download
  fallback only with `-AllowDnxDownload`; a preinstalled tool or
  `-ReplayCommand` is intentionally caller-controlled.
- **Primary extraction:** `replay <events.jsonl> --summary --json` → clean
  per-session stats: `duration_seconds`, turn counts (`user`/`assistant`/
  `tool_calls`), a `tools_used` histogram, `skills_invoked`, and `errors`.

### The `--json` gap we discovered (and work around)

`replay <file> --json` emits per-turn JSONL, but its **tool turns only carry
`status:"start"` with an empty `tool_name`** — it does **not** surface
`tool.execution_complete.success` or per-message `outputTokens`. Those are exactly
the signals we need to score *pain*. So the core adds a **thin supplemental raw
`events.jsonl` scan** for:

| Signal | Source event / field |
|--------|----------------------|
| tool-failure rate/detail | `tool.execution_complete.data.success == false`, paired to the tool by `toolCallId`; bounded redacted `error`/`message`/`result` detail |
| output tokens | `assistant.message.data.outputTokens` |
| context pressure | `session.truncation`, `session.compaction_start` |
| errors / aborts | `session.error`, `abort` |
| subagent failures | `subagent.failed` |
| retries | repeated identical `bash` / `edit` invocations |
| repository / branch | `session.start.data.context.{repository,branch}` |

This keeps `dotnet-replay` as the source of truth for everything it *does* expose,
and confines our own parsing to the narrow set of fields it omits.

### Session enumeration

`replay --db <session-store.db> --json` **errors** on the current
`session-store.db` schema, so the local front door queries SQLite directly:

```sql
SELECT id, branch, summary, updated_at
FROM sessions
WHERE repository = 'dotnet/maui'
ORDER BY updated_at DESC
LIMIT N;
```

(`sessions(id, cwd, repository, branch, summary, created_at, updated_at,
host_type)` — ~330 maui sessions on the reference machine.) Each id resolves to
`~/.copilot/session-state/<id>/events.jsonl`.

## Scoring model (deterministic, transparent)

Sessions are ranked worst-first by a documented composite in the core's
`$Weights`:

```
score = 2.0·tool_failures + 1.5·retries + 5.0·(errors+aborts)
      + 3.0·truncations  + 4.0·subagent_failures
      + output_tokens/50000 + tool_calls/50 + min(duration,7200)/600
```

Design choices:

- **Failures, aborts, and subagent failures dominate** — they are the clearest
  evidence of wasted effort.
- **Truncations/compactions** are weighted because context thrash is a recurring,
  fixable MAUI failure mode.
- **Wall-clock is capped at 7200 s for scoring.** Resumed sessions report a
  multi-day *calendar* span (resume events carry old timestamps), which would
  otherwise swamp the ranking. The true `duration_seconds` is still reported.

This is intentionally simple and inspectable: the score only ever *selects* which
sessions deserve LLM attention. All judgment is downstream.

## LLM-as-judge — but local, and only on the worst

The blueprint calls for **LLM-as-a-Judge** (arXiv:2306.05685) rubric grading of
trajectories, run **only on the failing/expensive sessions** surfaced by the
deterministic score to control cost. Critically, in this skill the "judge" is the
**contributor's own running Copilot session** reading the core's redacted digests
— there is **no third-party endpoint**, and it uses the contributor's own auth and
quota. The rubric and the learn-from-pr proposal taxonomy live in `SKILL.md`.

## One engine, two front doors

The deterministic work (select → extract → score → digest → redact) is factored
into a single PowerShell core, `scripts/Get-SessionAnalysis.ps1`:

- **Local front door:** `-Repository` / `-Last` / `-SessionId` / `-Since` select
  from `session-store.db`.
- **CI front door:** `-EventsPath` / `-EventsDir` point the *same* engine at
  already-downloaded AzDO `events.jsonl` artifacts, skipping the DB select. This
  lets the existing Python+bash CI-session pipeline (which produced
  `CONSOLIDATED_FINDINGS.md`) shell out to one shared core via a clean CLI + JSON
  contract, staying inside the AzDO-artifact boundary it already uses.

**Why PowerShell:** it matches every other shipping skill script
(`Get-ReleaseReadiness.ps1`, `query-issues.ps1`, the `run-*` skills) and the
production reviewer pipeline (`Review-PR.ps1`); `pwsh` is already a repo
prerequisite. The Python CI prototype is the *reference algorithm*, not shipping
code — CI reuse is unaffected because any job can shell out to the core's CLI.

## Privacy model (enforced + documented)

- **Local-only by default.** The core reads `~/.copilot/...` and writes a report
  to `-OutputDir`. It has **no** network egress, automatic downloads, or share
  flag. `-AllowDnxDownload` is explicit opt-in for the pinned public tool
  download and never uploads session data.
- **No exfiltration.** It NEVER opens a gist, NEVER POSTs a transcript, and NEVER
  ships session data to a third-party endpoint — including the judge step.
- **Redaction on by default.** Home paths → `~`, tokens (`ghp_`/`gho_`/`Bearer`/
  `password=`/`key=`), and emails are stripped from the report **and** must stay
  stripped in any emitted eval. `-NoRedact` exists only for local debugging.
- **Transcript snippets are untrusted data.** The report labels and code-fences
  transcript-derived content. The judgment workflow uses it only as evidence;
  embedded instructions, commands, links, and requests never alter the skill's
  workflow or privacy contract.
- **Output contract.** The Markdown report and JSON contract apply redaction to
  all dynamic strings, including session metadata and tool/skill identifiers.
  Redaction also covers AWS keys, current-format Azure DevOps PATs, Slack tokens,
  JWTs, and private-key blocks.
- **Minimal quotes over dumps.** Digests prefer structural metrics + event turn
  IDs (with a stable assistant-turn fallback) + short redacted snippets; raw
  transcripts are not re-emitted.
- **Cross-machine sharing is explicit, manual, opt-in.** Any gist / Kusto /
  dashboard path requires the user to ask, shares only the redacted report, and
  is never automated.

## Closing the loop — emit-eval

For each recurring failure mode, the skill emits a `vally` guard-eval using the
PR #36002 house pattern. An eval guarding this skill's own analysis workflow
belongs at `.github/skills/analyze-sessions/tests/eval.<short-mode>.vally.yaml`;
generic `.github/evals/` paths are not valid targets. A different skill's
`tests/` directory is appropriate only when that skill owns the guarded behavior.
Each eval pairs a refutation-proof **structural floor** (`output-matches` on a
forced token line) with **one LLM judge** (`scale_1_5`, `threshold: 0.6`) so the
judge carries ~half the weight. Each emitted file must pass
`npx -y @microsoft/vally-cli@0.10.0 lint --eval-spec <f> --strict`. This is the
mechanism that makes the analysis *iterative*: a failure found today becomes a
regression test that fails if we regress tomorrow.

## References

- `dotnet-replay` — <https://github.com/lewing/dotnet-replay>, NuGet `dotnet-replay` v0.9.1.
- PR #36002 — the guard-eval house pattern this skill emits
  (`eval.gh-auth.vally.yaml`, `eval.inline-findings.vally.yaml`).
- Zheng et al., *Judging LLM-as-a-Judge* — arXiv:2306.05685.
