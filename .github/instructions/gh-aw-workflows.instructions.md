---
applyTo:
  - ".github/workflows/*.md"
  - ".github/workflows/*.lock.yml"
---

# gh-aw (GitHub Agentic Workflows) Guidelines

## Architecture

gh-aw workflows are authored as `.md` files with YAML frontmatter, compiled to `.lock.yml` via `gh aw compile`. The lock file is auto-generated — **never edit it manually**.

### Execution Model

```
activation job  (renders prompt from base branch .md via runtime-import)
    ↓
agent job:
  user steps:       (pre-agent, OUTSIDE firewall, has GITHUB_TOKEN)
    ↓
  platform steps:   (configure git → checkout_pr_branch.cjs → install CLI)
    ↓
  agent:            (INSIDE sandboxed container, NO credentials)
```

| Context | Has GITHUB_TOKEN | Has gh CLI | Has git creds | Can execute scripts |
|---------|-----------------|-----------|---------------|-------------------|
| `steps:` (user) | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — **be careful** |
| Platform steps | ✅ Yes | ✅ Yes | ✅ Yes | Platform-controlled |
| Agent container | ❌ Scrubbed | ❌ Scrubbed | ❌ Scrubbed | ✅ But sandboxed |

### Step Ordering (Critical)

User `steps:` **always run before** platform-generated steps. You cannot insert user steps after platform steps.

The platform's `checkout_pr_branch.cjs` runs with `if: (github.event.pull_request) || (github.event.issue.pull_request)` — it is **skipped** for `workflow_dispatch` triggers.

### Prompt Rendering

The prompt is built in the **activation job** via `{{#runtime-import .github/workflows/<name>.md}}`. This reads the `.md` file from the **base branch** workspace (before any PR checkout). The rendered prompt is uploaded as an artifact and downloaded by the agent job.

- The agent prompt is always the base branch version — fork PRs cannot alter it
- The prompt references files on disk (e.g., `SKILL.md`) — those files must exist in the agent's workspace

### Fork PR Activation Gate

`gh aw compile` automatically injects a fork guard into the activation job's `if:` condition: `head.repo.id == repository_id`. This blocks fork PRs on `pull_request` events. This is **platform behavior** — do not add it manually.

## Fork PR Handling

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous — the vulnerability is executing workspace scripts with `GITHUB_TOKEN`.

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Fork PR Behavior by Trigger

| Trigger | `checkout_pr_branch.cjs` runs? | Fork handling |
|---------|-------------------------------|---------------|
| `pull_request` | ✅ Yes | Blocked by auto-generated activation gate |
| `workflow_dispatch` | ❌ Skipped | ✅ Works — user steps handle checkout and restore |
| `issue_comment` | ✅ Yes (re-checkouts after user steps) | ⚠️ Platform limitation — see below |

### The `issue_comment` + Fork Problem

For `/slash-command` triggers on fork PRs, `checkout_pr_branch.cjs` runs AFTER all user steps and re-checks out the fork branch. This overwrites any files restored by user steps (e.g., `.github/skills/`). There is no way to run user steps after platform steps.

**Current workaround:** Hard-fail for fork PRs on `issue_comment`, direct users to `workflow_dispatch`.

**Upstream issue:** [github/gh-aw#18481](https://github.com/github/gh-aw/issues/18481) — "Using gh-aw in forks of repositories"

### Safe Pattern: Checkout + Restore

```yaml
steps:
  - name: Checkout PR branch
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || inputs.pr_number }}
    run: |
      # Save base SHA to GITHUB_ENV (not a shell variable) so the restore step can read it
      echo "BASE_SHA=$(git rev-parse HEAD)" >> "$GITHUB_ENV"
      gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"

  - name: Restore agent infrastructure from base branch
    run: |
      # Fork PR branches won't have .github/skills/ or .github/instructions/.
      # rm -rf first to prevent fork-added files from surviving the restore.
      if [ -n "$BASE_SHA" ]; then
        rm -rf .github/skills/ .github/instructions/
        git checkout "$BASE_SHA" -- .github/skills/ .github/instructions/ .github/copilot-instructions.md
      fi
```

**Note:** For `workflow_dispatch`, `checkout_pr_branch.cjs` is skipped so this restore is the final workspace state. For same-repo PRs, the restore replaces files with copies from the base branch (effectively a no-op unless the PR modifies those files).

### Fork Guard for `issue_comment` Triggers

When blocking fork PRs, always use **fail-closed** logic — API errors must block, not pass through:

```bash
# ✅ CORRECT: fail-closed — API errors block the workflow
HEAD_REPO_ID=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" \
  --json headRepository --jq '.headRepository.id' 2>/dev/null) \
  || { echo "❌ API error. Blocking for safety."; exit 1; }
BASE_REPO_ID=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" \
  --json baseRepository --jq '.baseRepository.id' 2>/dev/null) \
  || { echo "❌ API error. Blocking for safety."; exit 1; }
if [ "$HEAD_REPO_ID" != "$BASE_REPO_ID" ]; then
  echo "❌ Fork PRs not supported. Use workflow_dispatch instead."
  exit 1
fi
```

```bash
# ❌ WRONG: fail-open — empty IDs from API errors bypass the guard
HEAD_REPO_ID=$(gh pr view ... --jq '.headRepository.id // ""')
if [ -n "$HEAD_REPO_ID" ] && [ -n "$BASE_REPO_ID" ] && [ "$HEAD_REPO_ID" != "$BASE_REPO_ID" ]; then
  # This NEVER fires when API returns empty strings
fi
```

### Anti-Patterns

**Do NOT skip checkout for fork PRs:**

```bash
# ❌ ANTI-PATTERN: Makes fork PRs unevaluable
if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
  echo "Skipping checkout for fork PR"
  exit 0  # Agent evaluates workflow branch instead of PR
fi
```

This is different from the hard-fail guard above. Skipping checkout means the agent gets the wrong files. Hard-failing tells the user to use `workflow_dispatch` instead.

**Do NOT execute workspace code after fork checkout:**

```yaml
# ❌ DANGEROUS: runs fork code with GITHUB_TOKEN
- name: Checkout PR
  run: gh pr checkout "$PR_NUMBER" ...
- name: Run analysis
  run: pwsh .github/skills/some-script.ps1
```

If you need to run scripts, either:
1. Run them **before** the checkout (from the base branch)
2. Run them **inside the agent container** (sandboxed, no tokens)

## Compilation

```bash
# Compile after every change to the .md source
gh aw compile .github/workflows/<name>.md

# This updates:
# - .github/workflows/<name>.lock.yml (auto-generated)
# - .github/aw/actions-lock.json
```

**Always commit the compiled lock file alongside the source `.md`.**

## Common Patterns

### Pre-Agent Data Prep (the `steps:` pattern)

Use `steps:` for any operation requiring GitHub API access that the agent needs:

```yaml
steps:
  - name: Fetch PR data
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      gh pr view "$PR_NUMBER" --json title,body > pr-metadata.json
      gh pr diff "$PR_NUMBER" --name-only > changed-files.txt
```

### Safe Outputs (Posting Comments)

```yaml
safe-outputs:
  add-comment:
    max: 1
    target: "*"    # Required for workflow_dispatch (no triggering PR context)
```

### Concurrency

Include all trigger-specific PR number sources:

```yaml
concurrency:
  group: "my-workflow-${{ github.event.issue.number || github.event.pull_request.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true
```

### Noise Reduction

Filter `pull_request` triggers to relevant paths and add a gate step:

```yaml
on:
  pull_request:
    paths:
      - 'src/**/tests/**'

steps:
  - name: Gate — skip if no relevant files
    if: github.event_name == 'pull_request'
    run: |
      FILES=$(gh pr diff "$PR_NUMBER" --name-only | grep -E '\.cs$' || true)
      if [ -z "$FILES" ]; then exit 1; fi
```

Manual triggers (`workflow_dispatch`, `issue_comment`) should bypass the gate. Note: `exit 1` causes a red ❌ on non-matching PRs — this is intentional (no built-in "skip" mechanism in gh-aw steps).

## Limitations

| What | Behavior | Workaround |
|------|----------|------------|
| User steps always before platform steps | Cannot run user code after `checkout_pr_branch.cjs` | Use `workflow_dispatch` for fork PRs; see [gh-aw#18481](https://github.com/github/gh-aw/issues/18481) |
| `--allow-all-tools` in lock.yml | Emitted by `gh aw compile` | Cannot override from `.md` source |
| MCP integrity filtering | Fork PRs blocked as "unapproved" | Use `steps:` checkout instead of MCP |
| `gh` CLI inside agent | Credentials scrubbed | Use `steps:` for API calls, or MCP tools |
| `issue_comment` trigger | Requires workflow on default branch | Must merge to `main` before `/slash-commands` work |
| Duplicate runs | gh-aw sometimes creates 2 runs per dispatch | Harmless, use concurrency groups |

### Upstream References

- [github/gh-aw#18481](https://github.com/github/gh-aw/issues/18481) — Fork support tracking issue
- [github/gh-aw#18518](https://github.com/github/gh-aw/issues/18518) — Fork detection in `gh aw init`
- [github/gh-aw#18521](https://github.com/github/gh-aw/issues/18521) — Fork support documentation

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Agent evaluates wrong PR | `workflow_dispatch` checks out workflow branch | Add `gh pr checkout` in `steps:` |
| Agent can't find SKILL.md | Fork PR branch doesn't have `.github/skills/` | Add base branch restore step |
| Fork `/command` gets garbled output | `checkout_pr_branch.cjs` overwrites restored files | Hard-fail for fork + `issue_comment`, use `workflow_dispatch` |
| Fork guard silently passes | API error returns empty IDs, fail-open logic | Use `|| exit 1` on API calls (fail-closed) |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Integrity filtering warning | MCP reading fork PR data | Expected, non-blocking |
| `/slash-command` doesn't trigger | Workflow not on default branch | Merge to `main` first |
