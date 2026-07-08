---
name: "Agentic Workflow Version Auto-Update"
description: |
  Detects whether the gh-aw infrastructure has pending updates. The detection
  run itself has zero write surface: its only outputs are `noop` (nothing to do —
  the normal steady state) or `create-agent-session` (delegate the actual
  upgrade + recompile + PR to a Copilot Coding Agent session).

  Rationale: `gh aw compile` writes to `.github/workflows/*.lock.yml`, but the
  default `GITHUB_TOKEN` cannot push under `.github/workflows/` (GitHub platform
  rule). This workflow therefore delegates the write work to a Copilot Coding
  Agent session, which runs under its own identity (a token gh-aw resolves from
  `GH_AW_AGENT_TOKEN` → `GH_AW_GITHUB_TOKEN` → `GITHUB_TOKEN`) and can write
  workflow files.

# ###############################################################
# Select a PAT from the pool and override COPILOT_GITHUB_TOKEN.
# Run agentic jobs in an isolated `copilot-pat-pool` environment.
#
# When org-level billing is available, this will be removed.
# See `shared/pat_pool.README.md` for more information.
# ###############################################################
imports:
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool

environment: copilot-pat-pool

on:
  # Start manual-only while maintainers validate the delegation payload and
  # dedupe behavior. MAUI has the PAT pool needed to enable this later:
  # uncomment `schedule: weekly` after a few clean workflow_dispatch runs.
  # schedule: weekly
  workflow_dispatch:
  # Forces a no-op pre_activation job, required by the pat_pool import.
  # See shared/pat_pool.README.md (Known Issues).
  permissions: {}

# Runs on dotnet/maui only, and never on a scheduled/background run inside a fork
# that inherited this file. workflow_dispatch stays as the explicit maintainer
# escape hatch. (Wrapped in ${{ }} so the leading `!` is not parsed as a YAML tag.)
if: |
  ${{ github.repository == 'dotnet/maui' && (github.event_name == 'workflow_dispatch' || !github.event.repository.fork) }}

permissions: read-all

# Lets maintainers find every asset this workflow triggers (agent sessions).
tracker-id: aw-version-update

network:
  allowed:
    - defaults
    - go
    - github

checkout:
  ref: main

engine:
  id: copilot
  env:
    COPILOT_GITHUB_TOKEN: |
      ${{ case(
        needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0,
        needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1,
        needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2,
        needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3,
        needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4,
        needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5,
        needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6,
        needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7,
        needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8,
        needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9,
        'NO COPILOT PAT AVAILABLE')
      }}

concurrency:
  group: "aw-version-update"
  cancel-in-progress: false

timeout-minutes: 15

tools:
  github:
    toolsets: [pull_requests, issues]
    # The detector reads only its own bot-authored dedupe PRs/issues; `approved`
    # keeps community content out of the agent's view (defense-in-depth) without
    # breaking dedupe, since github-actions[bot] items are already `approved`.
    min-integrity: approved
  bash: ["gh", "git", "grep", "sed", "awk", "sort", "uniq", "jq", "cat", "echo", "date", "test", "true", "false", "bash", "sh"]

safe-outputs:
  # Wire the pat_pool job into the safe_outputs job's needs graph so the whole
  # workflow shares one selected pool entry. NOTE: the delegated
  # `create-agent-session` step does NOT read this pool — gh-aw resolves its
  # `gh agent-task create` token from `secrets.GH_AW_AGENT_TOKEN` →
  # `GH_AW_GITHUB_TOKEN` → `GITHUB_TOKEN`. For the real (non-staged) delegation
  # to spawn a Copilot Coding Agent that can write `.github/workflows/`, a
  # maintainer must configure `GH_AW_AGENT_TOKEN` (or `GH_AW_GITHUB_TOKEN`) with
  # the scope `gh agent-task create` requires. See PR description / rollout notes.
  needs: [pat_pool]
  # STAGED: the first runs preview the create-agent-session request in the Actions
  # run summary WITHOUT spawning a real Copilot Coding Agent session. Remove
  # `staged: true` (and enable `schedule: weekly` above) only after maintainers
  # confirm both the no-op path and the previewed delegation payload are correct.
  staged: true
  noop:
    report-as-issue: false
  create-agent-session:
    base: main
    max: 1
---

# Agentic Workflow Version Auto-Update — Detection

You detect whether the gh-aw infrastructure has pending updates and, if so, delegate the actual upgrade work to a Copilot Coding Agent session. **You never commit, push, comment, create issues directly, or open PRs.** Your only allowed safe outputs are `noop` (the normal/expected steady state) and `create-agent-session` (delegate to CCA).

## Background

- This repo uses **GitHub Agentic Workflows (`gh aw`)** — source: <https://github.com/github/gh-aw>, docs: <https://github.github.com/gh-aw/>.
- Files managed by `gh aw` in this repo (the only paths that should ever change as a result of this upgrade):
  - `.github/workflows/*.md` — agentic workflow sources
  - `.github/workflows/*.lock.yml` — compiled lock files (generated by `gh aw compile`)
  - `.github/workflows/shared/**` — shared gh-aw imports, including the PAT-pool import
  - `.github/aw/**` — gh-aw project metadata, including `actions-lock.json`
  - `.github/agents/**` — MAUI agent definitions
- The `create-agent-session` safe output invokes `gh agent-task create` using the session token gh-aw resolves from `secrets.GH_AW_AGENT_TOKEN` → `GH_AW_GITHUB_TOKEN` → `GITHUB_TOKEN` — **not** `COPILOT_GITHUB_TOKEN` (that is only the engine's inference token). That spawns a Copilot Coding Agent run with the permissions required to write `.github/workflows/`. A maintainer must configure `GH_AW_AGENT_TOKEN` (or `GH_AW_GITHUB_TOKEN`) with that scope before the non-staged delegation can create a session.

## Task

Run these steps in order:

1. **Install/upgrade `gh-aw` to the latest release.** This detector must run the newest gh-aw — otherwise a stale pre-installed extension makes `gh aw upgrade && gh aw compile` a no-op and real upgrades are missed. If `gh aw --version` succeeds, run `gh extension upgrade gh-aw` to force the latest; otherwise run `gh extension install github/gh-aw`. If both fail, emit `noop` and stop.
2. **Upgrade.** Run `gh aw upgrade`. If it fails, emit `noop` and stop — do **not** try to fix the failure.
3. **Compile.** Run `gh aw compile`. If it reports errors, emit `noop` and stop.
4. **Capture state for the delegation payload (only used if changes are detected):**
   - `NEW_VERSION` ← `gh aw --version`
   - `DIFF_STAT` ← `git diff --stat`
   - `CHANGED_FILES` ← `git diff --name-only`
5. **Reset working tree.** Run `git reset --hard && git clean -fd` so no local changes leak out of this detection run.
6. **Dedupe check.** Search for an already-open follow-up:
   - Open PRs titled `[Auto Update] Agentic workflows` (`gh pr list --search "[Auto Update] Agentic workflows in:title" --state open`).
   - Open issues titled `[Auto Update] Agentic workflows` (`gh issue list --search "[Auto Update] Agentic workflows in:title" --state open`) — these are the Copilot agent-session issues from previous runs.
   - If **either** exists, emit `noop` and stop. The previous run's PR/session is still pending.
7. **Decide and emit exactly one safe output:**
   - `CHANGED_FILES` is empty → emit a single `noop`. **This is the normal/expected outcome on most runs — do not treat it as a failure and do not create an issue.**
   - Otherwise → emit a single `create-agent-session` whose `body` is the template in the next section, with `<NEW_VERSION>`, `<DIFF_STAT>`, and `<CHANGED_FILES>` substituted.

## Agent session description (template)

Use this exact body for the `create-agent-session` output. Title prefix `[Auto Update] Agentic workflows` is required (the dedupe check in step 6 looks for it).

````markdown
# [Auto Update] Agentic workflows → gh-aw <NEW_VERSION>

The `aw-version-update` detection run found that re-running
`gh aw upgrade && gh aw compile` against `main` produces a non-empty diff.
Please apply that diff and open a PR.

## Steps

1. Install the gh-aw CLI extension (skip if already installed):
   ```bash
   gh extension install github/gh-aw
   ```
2. Apply gh-aw codemods/version updates and refresh gh-aw metadata:
   ```bash
   gh aw upgrade
   ```
3. Recompile all workflows (regenerates `.github/workflows/*.lock.yml`):
   ```bash
   gh aw compile
   ```
4. Run the verification gate before opening a PR: check for gh-aw migration residue that was not auto-fixed and for security/XPIA regressions in the changed workflow sources and generated locks. Do not hand-edit generated lock files.
5. Stage **only** files managed by `gh aw`:
   - `.github/workflows/*.md`
   - `.github/workflows/*.lock.yml`
   - `.github/workflows/shared/**`
   - `.github/aw/**`
   - `.github/agents/**`
6. Commit with message: `Update agentic workflows via gh aw upgrade`
7. Open a PR to `main` titled `[Auto Update] Agentic workflows`. Keep the body short — one line summarizing what changed (for example, "gh-aw v0.X → v0.Y, codemods applied"). The Files tab shows the diff; do not list files in the body.

## Hard rules

- Do **not** run `go`, `npm`, or any package manager / build tool.
- Do **not** hand-edit `go.mod`, `go.sum`, `package.json`, or any dependency manifest.
- Do **not** hand-edit generated `*.lock.yml` files — only `gh aw compile` writes those.
- Do **not** stage or commit files outside the gh-aw-managed path list above.
- If `gh aw upgrade` or `gh aw compile` fails, stop and report the error output. Do **not** try to fix it — open an issue for a human instead.

## References

- gh-aw repo: <https://github.com/github/gh-aw>
- gh-aw docs: <https://github.github.com/gh-aw/>
- `gh aw upgrade` reference: <https://github.github.com/gh-aw/tools/cli/#upgrade>
- `gh aw compile` reference: <https://github.github.com/gh-aw/tools/cli/#compile>

## Detection evidence

- `gh aw --version` on the detection run: `<NEW_VERSION>`
- `git diff --stat`:
  ```
  <DIFF_STAT>
  ```
- `git diff --name-only`:
  ```
  <CHANGED_FILES>
  ```
````

## Rules (for this detection workflow)

- Only commands you may run: `gh extension install github/gh-aw`, `gh extension upgrade gh-aw`, `gh aw --version`, `gh aw upgrade`, `gh aw compile`, `git diff`, `git reset`, `git clean`, `gh pr list`, `gh issue list`.
- Never run `go`, `npm`, or any package manager / build tool.
- Never commit, push, comment, create issues directly, or open PRs. Your only safe outputs are `noop` and `create-agent-session`.
- Emit exactly one safe output per run.
- `noop` is the expected steady state — do not report it as a failure and do not create an issue for it.
