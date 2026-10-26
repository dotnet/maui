---
name: "Action Pin Refresh"
description: |
  Weekly gh-aw maintenance workflow for dotnet/maui. Runs `gh aw update --verbose`
  to refresh pinned GitHub Actions SHAs in `.github/aw/actions-lock.json`, then opens
  a PR only for that lockfile. Generated `.github/workflows/*.lock.yml` files, and
  any sourced workflow drift, are intentionally excluded from this action-pin PR.

# ###############################################################
# Select a PAT from the pool and override COPILOT_GITHUB_TOKEN.
# Run agentic jobs in an isolated `copilot-pat-pool` environment.
#
# When org-level billing is available, this will be removed.
# See `shared/pat_pool.README.md` for more information.
# ###############################################################
imports:
  - shared/gpt-6.1-sol.md
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool

environment: copilot-pat-pool

on:
  schedule: weekly
  workflow_dispatch:
  # Forces a no-op pre_activation job, required by the pat_pool import.
  # See shared/pat_pool.README.md (Known Issues).
  permissions: {}

# Runs on dotnet/maui only. `!github.event.repository.fork` is cheap
# defense-in-depth so forks that inherit this file never run it on schedule.
if: |
  github.repository == 'dotnet/maui' && !github.event.repository.fork

permissions:
  contents: read
  # Required by the github `default` toolset (includes the issues toolset). The
  # agent's own work uses only bash `gh pr list` + git + the create-pull-request
  # safe-output, but the declared toolset needs issues:read to compile clean.
  issues: read
  pull-requests: read

# Pin dispatch/schedule runs to main so the refresh always operates on the
# canonical actions-lock.json (a workflow_dispatch could otherwise select a
# stale branch).
checkout:
  ref: main

# Lets maintainers find every asset this workflow creates (PRs, comments).
tracker-id: aw-actions-update

model: gpt-6.1-sol
engine:
  id: copilot
  env:
    COPILOT_PROVIDER_WIRE_API: responses
    # Authenticate the agent's `gh` CLI commands with this workflow's read-only
    # GitHub Actions token, not the Copilot inference PAT.
    GH_TOKEN: ${{ github.token }}
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}

concurrency:
  group: "aw-actions-update"
  cancel-in-progress: false

# Keep installation and updates out of the approval-gated inference process.
# No extension needs to be mounted into the AWF chroot: the agent only inspects
# the resulting workspace diff and requests the bounded safe-output PR.
pre-agent-steps:
  - name: Refresh action pins with the lock-pinned compiler
    shell: bash
    timeout-minutes: 15
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      # v0.86.2 omits custom step timeouts from the compiled lock.
      timeout --signal=TERM --kill-after=30s 15m bash <<'GH_AW_ACTION_PIN_REFRESH'
      set -euo pipefail
      report=/tmp/gh-aw/action-pin-refresh.txt
      prs="$(gh pr list --state open --search '"[actions]" in:title' --author "app/github-actions" --json number,title,url --limit 20)"
      existing="$(printf '%s\n' "$prs" | jq -r '.[] | select(.title | startswith("[actions]")) | .url')"
      if [ -n "$existing" ]; then
        printf 'Skipped refresh: existing bot action-pin PR:\n%s\n' "$existing" > "$report"
        cat "$report"
        exit 0
      fi

      lock=.github/workflows/aw-actions-update.lock.yml
      metadata="$(head -n 1 "$lock")"
      version="$(printf '%s\n' "$metadata" | sed 's/^# gh-aw-metadata: //' | jq -er '.compiler_version | select(type == "string" and test("^v[0-9]+\\.[0-9]+\\.[0-9]+$"))')" || {
        echo "::error::Invalid compiler_version in $lock"
        exit 1
      }
      # Isolate extension state; never remove or upgrade a runner-wide install.
      cli_home="$(mktemp -d "${RUNNER_TEMP}/action-pin-cli.XXXXXX")"
      export GH_CONFIG_DIR="$cli_home/config"
      export XDG_DATA_HOME="$cli_home/data"
      export XDG_STATE_HOME="$cli_home/state"
      if ! gh extension install github/gh-aw --pin "$version"; then
        echo "::error::Failed to install gh-aw $version"
        exit 1
      fi
      if ! installed="$(gh aw --version 2>&1)"; then
        echo "::error::Failed to read the installed gh-aw version"
        exit 1
      fi
      if [ "$installed" != "gh aw version $version" ]; then
        echo "::error::Installed gh-aw does not match $version"
        exit 1
      fi
      update_log="$cli_home/update.log"
      if ! gh aw update --verbose > "$update_log" 2>&1; then
        cat "$update_log"
        echo "::error::Action-pin refresh failed; no PR may be created"
        exit 1
      fi
      cat "$update_log"
      # v0.86.2 logs action-lock failures without failing the update command.
      if grep -Fq 'Could not update actions-lock.json:' "$update_log"; then
        echo "::error::Action-pin lock refresh failed; no PR may be created"
        exit 1
      fi
      printf 'Completed gh aw update --verbose with %s.\n' "$version" > "$report"
      GH_AW_ACTION_PIN_REFRESH

timeout-minutes: 15

network:
  allowed:
    - defaults
    - github

tools:
  github:
    # Dedup uses bash `gh pr list` and the PR is a safe-output, so the agent
    # needs only the default toolset (matches the upstream reference workflow).
    toolsets: [default]
  bash: ["gh", "git", "grep", "sed", "awk", "sort", "uniq", "jq", "cat", "echo", "date", "test", "true", "false", "bash", "sh"]

safe-outputs:
  needs: [pat_pool]
  create-pull-request:
    title-prefix: "[actions] "
    labels: [dependencies, agentic-workflows]
    draft: false
    # The only write target, `.github/aw/actions-lock.json`, lives under the
    # protected `.github/` dot-folder. `allowed` intentionally permits writing it;
    # the single-entry `allowed-files` list below is the real file-scope guard, so
    # nothing else — including any `.github/workflows/**` file — can ever be touched.
    protected-files: allowed
    # Enforced allowlist: the only file this PR may ever touch. Fails closed if the
    # agent forgets to discard generated locks or sourced-workflow drift.
    allowed-files:
      - .github/aw/actions-lock.json
  # Most weekly runs are no-ops (pins already fresh). Without this, gh-aw's default
  # (noop: report-as-issue: true) files a "no action taken" issue every idle run.
  noop:
    report-as-issue: false
---

# Action Pin Refresh — dotnet/maui

You are an AI automation agent that keeps dotnet/maui's pinned GitHub Actions SHAs fresh.

Your only intended code change is `.github/aw/actions-lock.json`. Do not include generated
`.github/workflows/*.lock.yml` files in the PR. Do not include gh-aw version upgrades or compiled
workflow refreshes; those belong to the separate gh-aw version-updater workflow.

## Step 0 — Read the trusted refresh result and de-duplicate existing action-pin PRs

Trusted pre-agent setup already checked for a bot-created action-pin PR, installed
the exact `compiler_version` from this workflow's committed lock in isolated CLI
state, verified the installed version, and ran `gh aw update --verbose`. Setup
fails the job on invalid metadata, install/version mismatch, or update failure.
It uses only the read-only GitHub Actions token, never the Copilot inference PAT.
The extension is used on the host, not inside the AWF chroot.

Read `/tmp/gh-aw/action-pin-refresh.txt` first. If it reports a skipped refresh,
emit a noop mentioning the existing PR and stop. If the report is missing or does
not report successful completion, report the missing setup result and stop without
creating a PR. **Do not install, remove, upgrade, or invoke gh-aw yourself.**

Before creating a PR, recheck for an open action-pin PR:

```bash
gh pr list --state open --search '"[actions]" in:title' --author "app/github-actions" --json number,title,url,headRefName --limit 20
```

If there is already an open PR whose title starts with `[actions]`, exit gracefully without
creating another PR. Mention the existing PR in the run output only. (The `--author` filter scopes
the check to this workflow's own bot-created PRs — safe-outputs opens them as `app/github-actions` —
so an unrelated user-opened PR with a colliding title cannot suppress the refresh.)

## Steps 1–2 — Inspect the completed trusted update

The lock-pinned CLI caps native action-pin resolution at its own version. Bumping
gh-aw remains a deliberate, coordinated change handled by `aw-version-update`,
not this weekly pin-refresher. The trusted update may:

- refresh pinned action SHAs in `.github/aw/actions-lock.json`;
- recompile `.github/workflows/*.lock.yml` files;
- refresh workflows that use `source:` from upstream repositories.

MAUI currently sources some workflows from `githubnext/agentics` (for example,
`daily-repo-status`). If `gh aw update` reports or produces upstream-source drift, capture the
paths and summarize that in the PR body, but do not include those source/compiled workflow changes
in this PR.

## Step 3 — Inspect changes

Run:

```bash
git status
git diff .github/aw/actions-lock.json
```

Only create a PR if `.github/aw/actions-lock.json` changed. If it did not change, restore any
compiled/source workflow changes and exit gracefully:

```bash
git checkout -- .github/workflows/*.lock.yml 2>/dev/null || true
git checkout -- .github/workflows/*.md 2>/dev/null || true
echo "All action pins are already up to date; no PR needed."
```

## Step 4 — Exclude workflow lock/source drift from this PR

If `.github/aw/actions-lock.json` changed, first record any workflow drift for the PR body:

```bash
git status --porcelain -- .github/workflows | grep -E '\.(md|lock\.yml)$' || true
```

Then discard generated lock files:

```bash
git checkout -- .github/workflows/*.lock.yml 2>/dev/null || true
```

If sourced workflow `.md` files also changed, discard them too after recording their paths:

```bash
git checkout -- .github/workflows/*.md 2>/dev/null || true
```

`git checkout --` only restores **tracked** files. `gh aw update` can also create brand-new
**untracked** files under `.github/workflows/` (for example, a lock for a newly sourced workflow).
Remove those too so they cannot leak into the PR or hide from the verification below:

```bash
git clean -f -- .github/workflows/
```

Verify the final change set contains only the action pin lockfile. Use `git status --porcelain`
(not `git diff --name-only`) so **untracked** files are also caught:

```bash
git status --porcelain
```

If `git status --porcelain` reports anything other than a single `.github/aw/actions-lock.json`
entry, do not create a PR.

## Step 5 — Create the PR only for `.github/aw/actions-lock.json`

Use the `create-pull-request` safe-output. The safe-output already adds the `[actions] ` title
prefix, so use this title text:

```text
Update GitHub Actions pins
```

PR body template:

~~~~markdown
### GitHub Actions pin refresh

This PR updates pinned GitHub Actions SHAs in `.github/aw/actions-lock.json` using:

~~~bash
gh aw update --verbose
~~~

### Updated actions

[List each updated action from `git diff .github/aw/actions-lock.json`, including before/after refs or SHAs when visible.]

### Scope

- Included: `.github/aw/actions-lock.json`
- Excluded: generated `.github/workflows/*.lock.yml` files; they will regenerate on the next compile
- Excluded: gh-aw version upgrades and workflow source updates; those are handled separately

### Sourced workflow drift

[If `gh aw update` reported or produced changes for workflows sourced from upstream, list the paths and say they were intentionally not included in this action-pin PR. Otherwise write: "None observed."]

### Validation

- Trusted pre-agent setup ran `gh aw update --verbose` with the committed compiler version
- Confirmed final diff contains only `.github/aw/actions-lock.json`
~~~~

## Edge cases

- If no action pins changed, create no PR.
- If only `.github/workflows/*.lock.yml` or sourced workflow `.md` files changed, restore them and create no PR.
- If `gh aw update --verbose` fails, report the failure in the run output and create no PR.
- If an open `[actions]` PR already exists, create no duplicate PR.
