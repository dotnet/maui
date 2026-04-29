---
applyTo:
  - ".github/workflows/*.md"
  - ".github/workflows/*.lock.yml"
---

# gh-aw (GitHub Agentic Workflows) Guidelines

## 🚨 Before You Build: Prefer Built-in gh-aw Features

**CRITICAL RULE:** Before implementing any trigger, output, scheduling, or interaction mechanism in a gh-aw workflow, check whether gh-aw has a built-in feature that does it. gh-aw extends GitHub Actions with many convenience features — manually reimplementing them is always worse (more code, more bugs, missing platform integration like emoji reactions, sanitized inputs, and noise reduction).

### Step 1: Check the anti-patterns table below
### Step 2: If not listed, check the [triggers reference](https://github.github.com/gh-aw/reference/triggers/), [frontmatter reference](https://github.github.com/gh-aw/reference/frontmatter/), and [safe-outputs reference](https://github.github.com/gh-aw/reference/safe-outputs/)
### Step 3: If a built-in exists, use it. If not, proceed with manual implementation.

### Anti-Patterns: Manual Reimplementations to Avoid

| If you're about to implement... | Use this built-in instead | Docs |
|---------------------------------|--------------------------|------|
| `issue_comment` + `startsWith(comment.body, '/cmd')` | `slash_command:` trigger | [Command Triggers](https://github.github.com/gh-aw/reference/command-triggers/) |
| Manual emoji reaction on triggering comment | `reaction:` field under `on:` | [Frontmatter](https://github.github.com/gh-aw/reference/frontmatter/) |
| Posting "workflow started/completed" status comments | `status-comment: true` under `on:` | [Frontmatter](https://github.github.com/gh-aw/reference/frontmatter/) |
| Fixed cron schedule (`0 9 * * 1`) for non-critical timing | `schedule: weekly on monday around 9:00` (fuzzy) | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual `if:` to skip bot-authored PRs | `skip-bots:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual `if:` to skip by author role | `skip-roles:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual label check + removal for one-shot commands | `label_command:` trigger | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Editing old comments to collapse them | `hide-older-comments: true` on `add-comment:` | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |
| Creating no-op report issues | `noop: report-as-issue: false` | [Safe Outputs / Monitoring](https://github.github.com/gh-aw/patterns/monitoring/) |
| Auto-closing older issues from same workflow | `close-older-issues: true` on `create-issue:` | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |
| Disabling workflow after a date | `stop-after:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual approval gating | `manual-approval:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Search-based skip logic in `steps:` | `skip-if-match:` / `skip-if-no-match:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |

**Note:** gh-aw is actively developed. If a capability feels like something a framework would provide natively, check the reference docs — it probably exists even if it's not in this table yet.

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

**⚠️ Agent container credential nuance:** `GITHUB_TOKEN` and `gh` CLI credentials are scrubbed inside the agent container. However, `COPILOT_TOKEN` (used for LLM inference) is present in the environment via `--env-all`. Any subprocess (e.g., `dotnet build`, `npm install`) inherits this variable. The AWF network firewall, `redact_secrets.cjs` (post-agent log scrubbing), and the threat detection agent limit the blast radius. See [Security Boundaries](#security-boundaries) below.

### Step Ordering (Critical)

User `steps:` **always run before** platform-generated steps. You cannot insert user steps after platform steps.

The platform's `checkout_pr_branch.cjs` runs with `if: (github.event.pull_request) || (github.event.issue.pull_request)` — it is **skipped** for `workflow_dispatch` triggers.

### Prompt Rendering

The prompt is built in the **activation job** via `{{#runtime-import .github/workflows/<name>.md}}`. This reads the `.md` file from the **base branch** workspace (before any PR checkout). The rendered prompt is uploaded as an artifact and downloaded by the agent job.

- The agent prompt is always the base branch version — fork PRs cannot alter it
- The prompt references files on disk (e.g., `SKILL.md`) — those files must exist in the agent's workspace

### Fork PR Activation Gate

By default, `gh aw compile` automatically injects a fork guard into the activation job's `if:` condition: `head.repo.id == repository_id`. This blocks fork PRs on `pull_request` events.

To **allow fork PRs**, add `forks: ["*"]` to the `pull_request` trigger in the `.md` frontmatter. The compiler removes the auto-injected guard from the compiled `if:` conditions. This is safe when the workflow uses inline checkout + trusted-infra restore and the agent is sandboxed.

## Security Boundaries

### Key Principles (from [GitHub Security Lab](https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/))

1. **Never execute untrusted PR code with elevated credentials.** The classic "pwn-request" attack is `pull_request_target` + checkout PR + run build scripts with `GITHUB_TOKEN`. The attack surface includes build scripts (`make`, `build.ps1`), package manager hooks (`npm postinstall`, MSBuild targets), and test runners.

2. **Treating PR contents as passive data is safe.** Reading, analyzing, or diffing PR code is fine — the danger is *executing* it. Our gh-aw workflows read code for evaluation; they never build or run it.

3. **`pull_request_target` grants write permissions and secrets access.** This is by design — the workflow YAML comes from the base branch (trusted). But any step that checks out and runs fork code in this context creates a vulnerability.

4. **`pull_request` from forks has no secrets access.** GitHub withholds secrets because the workflow YAML comes from the fork (untrusted). This is the safe default for CI builds on fork PRs.

5. **The `workflow_run` pattern separates privilege from code execution.** Build in an unprivileged `pull_request` job → pass artifacts → process in a privileged `workflow_run` job. This is architecturally what gh-aw does: agent runs read-only, `safe_outputs` job has write permissions.

### gh-aw Defense Layers

| Layer | What it does | What it doesn't do |
|-------|-------------|-------------------|
| **AWF network firewall** | Restricts outbound to allowlisted domains | Doesn't prevent reading env vars inside the container |
| **`redact_secrets.cjs`** | Scrubs known secret values from logs/artifacts post-agent | Doesn't catch encoded/obfuscated values |
| **Threat detection agent** | Reviews agent outputs before safe-outputs publishes them | Can miss novel exfiltration techniques |
| **Safe-outputs permission separation** | Write operations happen in separate job, not the agent | Agent can still request writes via safe-output tools |
| **`max: 1` on `add-comment`** | Limits agent to one comment | That one comment could contain sensitive data (mitigated by redaction) |
| **XPIA prompt** | Instructs LLM to resist prompt injection from untrusted content | LLM compliance is probabilistic, not guaranteed |
| **`pre_activation` role check** | Gates on write-access collaborators | Does not apply if `roles: all` is set |

### Rules for gh-aw Workflow Authors

- ✅ **DO** treat PR contents as passive data (read, analyze, diff)
- ✅ **DO** run data-gathering scripts in `steps:` (pre-agent, trusted context) not inside the agent
- ✅ **DO** inline checkout + trusted-infra restore in `steps:` for `workflow_dispatch` to restore `.github/` from base
- ❌ **DO NOT** run `dotnet build`, `npm install`, or any build command on untrusted PR code inside the agent — build tool hooks (MSBuild targets, postinstall scripts) can read `COPILOT_TOKEN` from the environment
- ❌ **DO NOT** execute workspace scripts (`.ps1`, `.sh`, `.py`) after checking out a fork PR in `steps:` — those run with `GITHUB_TOKEN`
- ❌ **DO NOT** set `roles: all` on workflows that process PR content — this allows any user to trigger the workflow

## Fork PR Handling

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous — the vulnerability is executing workspace scripts with `GITHUB_TOKEN`.

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Fork PR Behavior by Trigger

| Trigger | `checkout_pr_branch.cjs` runs? | Fork handling |
|---------|-------------------------------|---------------|
| `pull_request` (default) | ✅ Yes | Blocked by auto-generated activation gate unless `forks: ["*"]` is set |
| `pull_request` + `forks: ["*"]` | ✅ Yes | ✅ Works — user steps restore trusted infra before agent runs |
| `workflow_dispatch` | ❌ Skipped | ✅ Works — user steps handle checkout and restore is final |
| `issue_comment` (same-repo) | ✅ Yes | ✅ Works — files already on PR branch |
| `issue_comment` (fork) | ✅ Yes | ⚠️ Works — `checkout_pr_branch.cjs` re-checks out fork branch after user steps, potentially overwriting restored infra. Acceptable because agent is sandboxed (no credentials, max 1 comment via safe-outputs). Pre-flight check catches missing `SKILL.md` if fork isn't rebased. |
| `slash_command` | ✅ Yes (compiles to `issue_comment` internally) | Same behavior as `issue_comment` above, but with platform-managed command matching, emoji reactions, and sanitized input. Prefer `slash_command:` over manual `issue_comment` + `startsWith()`. |

### The `issue_comment` + Fork Problem

For `/slash-command` triggers on fork PRs, `checkout_pr_branch.cjs` runs AFTER all user steps and re-checks out the fork branch. This overwrites any files restored by user steps (e.g., `.github/skills/`). A fork could include a crafted `SKILL.md` that alters the agent's evaluation behavior.

**Accepted residual risk:** The agent runs in a sandboxed container with `GITHUB_TOKEN` and `gh` CLI credentials scrubbed. `COPILOT_TOKEN` (for LLM inference) remains in the environment but the AWF network firewall restricts outbound connections to an allowlist of domains, `redact_secrets.cjs` scrubs known secret values from logs/outputs post-agent, and the threat detection agent reviews outputs before they are published. The worst practical outcome is a manipulated evaluation comment (`safe-outputs: add-comment: max: 1`). The pre-flight check in the agent prompt catches the case where `SKILL.md` is missing entirely (fork not rebased on `main`).

**Upstream issue:** [github/gh-aw#18481](https://github.com/github/gh-aw/issues/18481) — "Using gh-aw in forks of repositories"

### Safe Pattern: Checkout + Restore

Inline the checkout and restore logic directly in the workflow step. The fork/permission checks are redundant for `workflow_dispatch` (already write-gated by GitHub and `roles:` config):

```yaml
steps:
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: |
      set -euo pipefail
      gh pr checkout "$PR_NUMBER"
      BASE_SHA=$(gh pr view "$PR_NUMBER" --json baseRefOid --jq '.baseRefOid')
      git checkout "$BASE_SHA" -- .github/ 2>&1 \
        && echo "✅ Restored .github/ from base ($BASE_SHA)" \
        || echo "⚠️ Could not restore .github/ from base"
```

The step:
1. Checks out the PR branch via `gh pr checkout`
2. Restores `.github/` from the base branch SHA (defense-in-depth)

No fork/permission checks are needed because `workflow_dispatch` already requires write access.

**Behavior by trigger:**
- **`workflow_dispatch`**: Platform checkout is skipped, so the inline restore IS the final workspace state (trusted files from base branch)
- **`slash_command`** (same-repo): Platform's `checkout_pr_branch.cjs` handles checkout. Skill files typically match main unless the PR modified them.
- **`slash_command`** (fork): Platform re-checks out fork branch after user steps, overwriting restored files. Agent is sandboxed; pre-flight in the prompt catches missing `SKILL.md`

### Anti-Patterns

**Do NOT skip checkout for fork PRs:**

```bash
# ❌ ANTI-PATTERN: Makes fork PRs unevaluable
if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
  echo "Skipping checkout for fork PR"
  exit 0  # Agent evaluates workflow branch instead of PR
fi
```

Skipping checkout means the agent evaluates the wrong files. The correct approach is: always check out the PR, then restore agent infrastructure from the base branch.

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
| User steps always before platform steps | Cannot run user code after `checkout_pr_branch.cjs` | For `issue_comment` fork PRs, accept sandboxed residual risk; see [gh-aw#18481](https://github.com/github/gh-aw/issues/18481) |
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
| Agent can't find SKILL.md | Fork PR branch doesn't include `.github/skills/` | Rebase fork on `main`, or use `workflow_dispatch` with `pr_number` input |
| Fork PR skipped on `pull_request` | `forks: ["*"]` not in workflow frontmatter | Add `forks: ["*"]` under `pull_request:` in the `.md` source and recompile |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Integrity filtering warning | MCP reading fork PR data | Expected, non-blocking |
| `/slash-command` doesn't trigger | Workflow not on default branch | Merge to `main` first |
