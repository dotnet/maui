---
name: "Agentic Workflow Version Auto-Update"
description: |
  Detects whether the gh-aw infrastructure has a pending framework upgrade and,
  if so, files a single tracking issue asking a maintainer to run `gh aw upgrade`
  locally and open the PR. The detection run has zero write surface beyond that
  issue: its only safe outputs are `noop` (nothing to do — the normal steady
  state) or `create-issue`.

  Why an issue instead of an automated PR: `gh aw upgrade` / `gh aw compile`
  regenerate `.github/workflows/*.lock.yml`, which the default `GITHUB_TOKEN`
  cannot push (GitHub platform rule) and which would otherwise require a
  Copilot-licensed / `workflow`-scoped agent token this repo intentionally does
  not provision. Filing an issue keeps the workflow fully automatic and
  credential-free; a maintainer performs the actual upgrade.

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
  # Weekly detector. Fully self-contained: the only write is a tracking issue via
  # safe-outputs (needs just the default GITHUB_TOKEN / issues: write) — no
  # Copilot-licensed or workflow-scoped agent token required, so this runs live on
  # a schedule. Steady state is `noop` (no issue); an available upgrade files one
  # `[Auto Update]` issue for a maintainer to action.
  schedule: weekly
  workflow_dispatch:
  # Forces a no-op pre_activation job, required by the pat_pool import.
  # See shared/pat_pool.README.md (Known Issues).
  permissions: {}

# Runs on dotnet/maui only, and never on a scheduled/background run inside a fork
# that inherited this file. The repository check already excludes every fork (a
# fork's `github.repository` is never `dotnet/maui`), so no separate fork/dispatch
# escape hatch is needed. (Wrapped in ${{ }} so the leading `!` is not parsed as a
# YAML tag.)
if: |
  ${{ github.repository == 'dotnet/maui' && !github.event.repository.fork }}

# Least privilege for the agent job: read the repo (git) and its own dedupe issues
# (gh issue list). Writes happen only via safe-outputs (issues: write), which gh-aw
# grants to the separate safe_outputs job.
permissions:
  contents: read
  issues: read

# Lets maintainers find every issue this workflow files.
tracker-id: aw-version-update

network:
  allowed:
    - defaults
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
    toolsets: [issues]
    # The detector reads only its own bot-authored dedupe issues; `approved`
    # keeps community content out of the agent's view (defense-in-depth) without
    # breaking dedupe, since github-actions[bot] items are already `approved`.
    min-integrity: approved
  bash: ["gh", "git", "grep", "sed", "awk", "sort", "uniq", "jq", "cat", "echo", "date", "test", "true", "false", "bash", "sh"]

safe-outputs:
  # Wire the pat_pool job into the safe_outputs job's needs graph so the whole
  # workflow shares one selected pool entry (the detector's engine inference is
  # billed to the selected COPILOT_PAT_*). The tracking issue itself is created
  # with the default GITHUB_TOKEN (issues: write) — no Copilot-licensed or
  # workflow-scoped token is involved, so this workflow needs no extra secret to
  # run live on a schedule.
  needs: [pat_pool]
  noop:
    report-as-issue: false
  create-issue:
    title-prefix: "[Auto Update] "
    labels: [dependencies, agentic-workflows]
    max: 1
---

# Agentic Workflow Version Auto-Update — Detection

You detect whether the gh-aw infrastructure has a pending framework upgrade and, if so, file **one** tracking issue asking a maintainer to perform the upgrade locally. **You never commit, push, comment, open PRs, or create agent sessions.** Your only allowed safe outputs are `noop` (the normal/expected steady state) and `create-issue`.

## Background

- This repo uses **GitHub Agentic Workflows (`gh aw`)** — source: <https://github.com/github/gh-aw>, docs: <https://github.github.com/gh-aw/>.
- Files managed by `gh aw` in this repo (the only paths a maintainer should change when applying the upgrade):
  - `.github/workflows/*.md` — agentic workflow sources
  - `.github/workflows/*.lock.yml` — compiled lock files (generated by `gh aw compile`)
  - `.github/workflows/shared/**` — shared gh-aw imports, including the PAT-pool import
  - `.github/aw/**` — gh-aw project metadata, including `actions-lock.json`
  - `.github/agents/**` — MAUI agent definitions
- Applying the upgrade regenerates `.github/workflows/*.lock.yml`, which the default `GITHUB_TOKEN` cannot push (GitHub platform rule). Rather than provisioning a Copilot-licensed / `workflow`-scoped token, this workflow files an issue and a **maintainer** runs `gh aw upgrade` locally and opens the PR.

## Task

Run these steps in order:

1. **Install/upgrade `gh-aw` to the latest release.** This detector must run the newest gh-aw — otherwise a stale pre-installed extension makes `gh aw upgrade && gh aw compile` a no-op and real upgrades are missed. If `gh aw --version` succeeds, run `gh extension upgrade gh-aw` to force the latest; otherwise run `gh extension install github/gh-aw`. If both fail, emit `noop` and stop.
2. **Upgrade.** Run `gh aw upgrade`. If it fails, emit `noop` and stop — do **not** try to fix the failure.
3. **Compile.** Run `gh aw compile`. If it reports errors, emit `noop` and stop.
4. **Capture state for the issue body (only used if changes are detected):**
   - `NEW_VERSION` ← `gh aw --version`
   - `DIFF_STAT` ← `git diff --stat`
   - `CHANGED_FILES` ← `git status --porcelain` (captures untracked generated files too, so a brand-new lock/workflow file produced by the upgrade is not missed)
5. **Reset working tree.** Run `git reset --hard && git clean -fd` so no local changes leak out of this detection run.
6. **Dedupe check.** Search for an already-open follow-up issue:
   - `gh issue list --search "[Auto Update] Agentic workflows in:title" --state open --author "app/github-actions"` — scoped to this workflow's own bot-created issues (safe-outputs creates them as `app/github-actions`) so an unrelated user-opened issue with a colliding title cannot suppress detection, and the agent only reads trusted bot titles.
   - If one exists, emit `noop` and stop — a previous run's upgrade issue is still open and unresolved.
7. **Decide and emit exactly one safe output:**
   - `CHANGED_FILES` is empty → emit a single `noop`. **This is the normal/expected outcome on most runs — do not treat it as a failure and do not create an issue.**
   - Otherwise → emit a single `create-issue` with title `Agentic workflows → gh-aw <NEW_VERSION>` (the `[Auto Update] ` prefix is added automatically) and `body` set to the template in the next section, with `<NEW_VERSION>`, `<DIFF_STAT>`, and `<CHANGED_FILES>` substituted.

## Issue body (template)

Use this exact body for the `create-issue` output.

````markdown
# Agentic workflows → gh-aw <NEW_VERSION>

The `aw-version-update` detector found that re-running `gh aw upgrade && gh aw compile`
against `main` produces a non-empty diff. A maintainer should apply it and open a PR —
the default `GITHUB_TOKEN` cannot push the regenerated `.github/workflows/*.lock.yml`,
so this cannot be automated without a Copilot-licensed / `workflow`-scoped token.

## Steps (run locally by a maintainer)

1. Install/refresh the gh-aw CLI extension:
   ```bash
   gh extension install github/gh-aw   # or: gh extension upgrade gh-aw
   ```
2. Apply gh-aw codemods/version updates and refresh gh-aw metadata:
   ```bash
   gh aw upgrade
   ```
3. Recompile all workflows (regenerates `.github/workflows/*.lock.yml`):
   ```bash
   gh aw compile
   ```
4. Verify before committing: no leftover gh-aw migration residue that was not auto-fixed,
   and no security/XPIA regressions in the changed workflow sources or generated locks.
   Do **not** hand-edit generated lock files.
5. Stage **only** files managed by `gh aw`:
   - `.github/workflows/*.md`
   - `.github/workflows/*.lock.yml`
   - `.github/workflows/shared/**`
   - `.github/aw/**`
   - `.github/agents/**`
6. Commit with message: `Update agentic workflows via gh aw upgrade`
7. Open a PR to `main` titled `[Auto Update] Agentic workflows`. Keep the body short —
   one line summarizing what changed (for example, "gh-aw v0.X → v0.Y, codemods applied").
   The Files tab shows the diff; do not list files in the body. Then close this issue.

## Hard rules

- Do **not** run `go`, `npm`, or any package manager / build tool.
- Do **not** hand-edit `go.mod`, `go.sum`, `package.json`, or any dependency manifest.
- Do **not** hand-edit generated `*.lock.yml` files — only `gh aw compile` writes those.
- Do **not** stage or commit files outside the gh-aw-managed path list above.
- If `gh aw upgrade` or `gh aw compile` fails, note the error in the PR (or a comment here) and stop — do not try to fix it by hand.

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
- `git status --porcelain`:
  ```
  <CHANGED_FILES>
  ```
````

## Rules (for this detection workflow)

- Only commands you may run: `gh extension install github/gh-aw`, `gh extension upgrade gh-aw`, `gh aw --version`, `gh aw upgrade`, `gh aw compile`, `git status`, `git diff`, `git reset`, `git clean`, `gh issue list`.
- Never run `go`, `npm`, or any package manager / build tool.
- Never commit, push, comment, open PRs, or create agent sessions. Your only safe outputs are `noop` and `create-issue`.
- Emit exactly one safe output per run.
- `noop` is the expected steady state — do not report it as a failure and do not create an issue for it.
