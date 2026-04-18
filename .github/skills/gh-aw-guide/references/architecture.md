# gh-aw Architecture & Security Reference

Deep reference for gh-aw execution model, security boundaries, fork handling, safe outputs, and troubleshooting. Read this file when the SKILL.md quick-start and common patterns aren't sufficient.

## Execution Model

```
activation job  (renders prompt from base branch .md via runtime-import)
    ↓              ↳ saves .github/ and .agents/ as artifact for later restore
agent job:
  user steps:       (pre-agent, OUTSIDE firewall, has GITHUB_TOKEN)
    ↓
  platform steps:   (configure git → checkout_pr_branch.cjs → restore .github/ from artifact → install CLI)
    ↓
  pre-agent-steps:  (OPTIONAL, runs after checkout but before agent, OUTSIDE firewall)
    ↓
  agent:            (INSIDE sandboxed container, NO credentials)
    ↓
  post-steps:       (OPTIONAL, runs after agent completes, OUTSIDE firewall)
```

| Context | Has GITHUB_TOKEN | Has gh CLI | Has git creds | Can execute scripts |
|---------|-----------------|-----------|---------------|-------------------|
| `steps:` (user, pre-activation) | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — **be careful** |
| Platform steps | ✅ Yes | ✅ Yes | ✅ Yes | Platform-controlled |
| `pre-agent-steps:` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — runs after checkout |
| Agent container | ❌ Scrubbed | ❌ Scrubbed | ❌ Scrubbed | ✅ But sandboxed |
| `post-steps:` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — runs after agent |

**Agent container credential nuance:** `GITHUB_TOKEN` and `gh` CLI credentials are scrubbed inside the agent container. However, `COPILOT_TOKEN` (used for LLM inference) is present in the environment via `--env-all`. Any subprocess (e.g., `dotnet build`, `npm install`) inherits this variable. The AWF network firewall, `redact_secrets.cjs` (post-agent log scrubbing), and the threat detection agent limit the blast radius.

### Step Ordering

User `steps:` run in the **pre-activation job** (before the agent job starts). Within the agent job, the ordering is: platform steps → `pre-agent-steps:` → agent → `post-steps:`.

The platform's `checkout_pr_branch.cjs` runs with `if: (github.event.pull_request) || (github.event.issue.pull_request)` — it is **skipped** for `workflow_dispatch` triggers.

**`pre-agent-steps:`** run after platform checkout and `.github/` restore but before the agent starts. Use these for data preparation that needs the PR branch checked out (e.g., running analysis scripts on PR code). Declared in frontmatter:

```yaml
pre-agent-steps:
  - name: Analyze PR complexity
    run: |
      echo "Files changed: $(gh pr diff $PR_NUMBER --name-only | wc -l)" > complexity.txt
```

**`post-steps:`** run after the agent completes but before safe-outputs. Use these for cleanup, metrics, or post-processing.

### Prompt Rendering

The prompt is built in the **activation job** via `{{#runtime-import .github/workflows/<name>.md}}`. This reads the `.md` file from the **base branch** workspace (before any PR checkout). The rendered prompt is uploaded as an artifact and downloaded by the agent job.

- The agent prompt is always the base branch version — fork PRs cannot alter it
- The prompt references files on disk (e.g., `SKILL.md`) — those files must exist in the agent's workspace

### Fork PR Activation Gate

By default, `gh aw compile` automatically injects a fork guard into the activation job's `if:` condition: `head.repo.id == repository_id`. This blocks fork PRs on `pull_request` events.

To **allow fork PRs**, add `forks: ["*"]` to the `pull_request` trigger in the `.md` frontmatter. The compiler removes the auto-injected guard from the compiled `if:` conditions. This is safe when the workflow uses the `Checkout-GhAwPr.ps1` pattern (checkout + trusted-infra restore) and the agent is sandboxed.

---

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
| **Integrity filtering** | Filters untrusted GitHub content before agent sees it (DIFC proxy) | Requires explicit `min-integrity` configuration |
| **Protected files** | Blocks agent from modifying package manifests, `.github/`, etc. | Only applies to `create-pull-request` and `push-to-pull-request-branch` |
| **`max: N` on safe outputs** | Limits number of operations per type | That output could still contain sensitive data (mitigated by redaction) |
| **XPIA prompt** | Instructs LLM to resist prompt injection from untrusted content | LLM compliance is probabilistic, not guaranteed |
| **`pre_activation` role check** | Gates on write-access collaborators | Does not apply if `roles: all` is set |

### Integrity Filtering

Integrity filtering (`tools.github.min-integrity`) controls which GitHub content an agent can access during a workflow run. The MCP gateway filters content by trust level before the agent sees it.

```yaml
tools:
  github:
    min-integrity: approved
    blocked-users: ["known-spammer"]
    trusted-users: ["trusted-contributor"]
    approval-labels: ["approved-for-agent"]
```

**Integrity hierarchy** (highest to lowest):

| Level | What qualifies |
|-------|---------------|
| `merged` | Merged PRs, commits reachable from default branch |
| `approved` | `OWNER`, `MEMBER`, `COLLABORATOR`; non-fork PRs on public repos; all items in private repos; users in `trusted-users` |
| `unapproved` | `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR` |
| `none` | All content including `FIRST_TIMER` and no-association users |
| `blocked` | Users in `blocked-users` — always denied, cannot be promoted |

**Recommendation for our workflows:** Use `min-integrity: approved` for workflows that process PR content from external contributors. This prevents prompt injection via untrusted issue comments or PR descriptions.

### Protected Files (Auto-Enabled)

When `create-pull-request` or `push-to-pull-request-branch` is configured, protected files are automatically enforced. The agent cannot modify:
- Package manifests (`package.json`, `*.csproj` dependencies, etc.)
- `.github/` directory contents
- Agent instruction files

Configure behavior with `protected-files:` on the safe output:
- `blocked` (default) — PR creation fails if protected files are modified
- `fallback-to-issue` — PR branch is pushed but an issue is created instead for review
- `allowed` — Disables protection (use with caution)

### Rules for gh-aw Workflow Authors

- ✅ **DO** treat PR contents as passive data (read, analyze, diff)
- ✅ **DO** run data-gathering scripts in `steps:` (pre-agent, trusted context) not inside the agent
- ✅ **DO** use `Checkout-GhAwPr.ps1` for `workflow_dispatch` to restore trusted `.github/` from base
- ❌ **DO NOT** run `dotnet build`, `npm install`, or any build command on untrusted PR code inside the agent — build tool hooks (MSBuild targets, postinstall scripts) can read `COPILOT_TOKEN` from the environment
- ❌ **DO NOT** execute workspace scripts (`.ps1`, `.sh`, `.py`) after checking out a fork PR in `steps:` — those run with `GITHUB_TOKEN`
- ❌ **DO NOT** set `roles: all` on workflows that process PR content — this allows any user to trigger the workflow

---

## Fork PR Handling

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous — the vulnerability is executing workspace scripts with `GITHUB_TOKEN`.

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Platform `.github/` Restore (gh-aw#23769 — Resolved)

The platform now **automatically preserves `.github/` and `.agents/` from the base branch**. The activation job saves these directories as an artifact, and after `checkout_pr_branch.cjs` checks out the PR branch, the platform restores them from the artifact. Additionally, `.mcp.json` is deleted from the workspace to prevent injection. This means fork PRs can no longer overwrite agent infrastructure (skills, instructions, copilot-instructions) by including modified copies in their branch.

### Fork PR Behavior by Trigger

| Trigger | `checkout_pr_branch.cjs` runs? | Fork handling |
|---------|-------------------------------|---------------|
| `pull_request` (default) | ✅ Yes | Blocked by auto-generated activation gate unless `forks: ["*"]` is set |
| `pull_request` + `forks: ["*"]` | ✅ Yes | ✅ Works — platform restores `.github/` from base branch artifact after checkout |
| `workflow_dispatch` | ❌ Skipped | ✅ Works — user steps handle checkout and restore is final |
| `issue_comment` (same-repo) | ✅ Yes | ✅ Works — files already on PR branch |
| `issue_comment` (fork) | ✅ Yes | ✅ Works — platform restores `.github/` from base branch artifact after checkout |
| `slash_command` | ✅ Yes (compiles to `issue_comment` internally) | Same behavior as `issue_comment` above, but with platform-managed command matching, emoji reactions, and sanitized input. Prefer `slash_command:` over manual `issue_comment` + `startsWith()`. |

### Safe Pattern: Checkout + Restore

Use the shared `.github/scripts/Checkout-GhAwPr.ps1` script, which implements checkout + restore in a single reusable step:

```yaml
steps:
  - name: Checkout PR and restore agent infrastructure
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

The script:
1. Verifies the PR author has write access and rejects fork PRs
2. Captures the base branch SHA before checkout
3. Checks out the PR branch via `gh pr checkout`
4. Restores `.github/skills/`, `.github/instructions/`, and `.github/copilot-instructions.md` from the base branch SHA (fatal on failure)

**Behavior by trigger:**
- **`workflow_dispatch`**: Platform checkout is skipped, so the script's restore IS the final workspace state (trusted files from base branch)
- **`slash_command`** (same-repo): Platform's `checkout_pr_branch.cjs` handles checkout. Skill files typically match main unless the PR modified them.
- **`slash_command`** (fork): Platform restores `.github/` from base branch artifact after checkout — fork cannot inject modified skills/instructions

**Note:** While the platform now handles `.github/` restore automatically for fork PRs, our `Checkout-GhAwPr.ps1` script still provides defense-in-depth for `workflow_dispatch` triggers (where platform checkout is skipped) and adds the write-access check that the platform doesn't enforce.

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

---

## Safe Outputs Quick Reference

Safe outputs enforce security through separation: agents run read-only and request actions via structured output, while separate permission-controlled jobs execute those requests.

### Available Safe Output Types

| Category | Types |
|----------|-------|
| **Issues & Discussions** | `create-issue`, `update-issue`, `close-issue`, `link-sub-issue`, `create-discussion`, `update-discussion`, `close-discussion` |
| **Pull Requests** | `create-pull-request`, `update-pull-request`, `close-pull-request`, `create-pull-request-review-comment`, `reply-to-pull-request-review-comment`, `resolve-pull-request-review-thread`, `push-to-pull-request-branch`, `add-reviewer` |
| **Labels & Assignments** | `add-comment`, `hide-comment`, `add-labels`, `remove-labels`, `assign-milestone`, `assign-to-agent`, `assign-to-user`, `unassign-from-user` |
| **Projects & Releases** | `create-project`, `update-project`, `create-project-status-update`, `update-release`, `upload-asset` |
| **Workflow & Security** | `dispatch-workflow`, `call-workflow`, `dispatch_repository`, `create-code-scanning-alert`, `autofix-code-scanning-alert`, `create-agent-session` |
| **System (auto-enabled)** | `noop`, `missing-tool`, `missing-data` |
| **Custom** | `jobs:` (custom post-processing with MCP tool access), `actions:` (GitHub Action wrappers) |

### Key Safe Output Features

**`create-pull-request` notable options:**
- `draft: true` — Enforced as policy (agent cannot override)
- `expires: 14` — Auto-close after 14 days (same-repo only)
- `excluded-files: ["**/*.lock"]` — Strip files from the patch entirely
- `github-token-for-extra-empty-commit:` — Push empty commit with separate token to trigger CI
- `protected-files: fallback-to-issue` — Create issue instead of failing when protected files modified
- `base-branch: "vnext"` — Target non-default branch
- `auto-close-issue: false` — Don't add `Fixes #N` to PR description
- `allowed-events: [COMMENT, REQUEST_CHANGES]` — On `submit-pull-request-review`, blocks agent from approving PRs (bypasses branch protection). **Always use this** for review workflows.

**`add-comment` notable options:**
- `hide-older-comments: true` — Collapse previous comments from same workflow
- `max: N` — Limit comments per run (default: 1)
- `target: "*"` — Required for `workflow_dispatch` (no triggering PR context)

---

## Limitations

| What | Behavior | Workaround |
|------|----------|------------|
| Agent-created PRs don't trigger CI | GitHub Actions ignores pushes from `GITHUB_TOKEN` | Use `github-token-for-extra-empty-commit:` with a PAT or GitHub App token on `create-pull-request`. See [Triggering CI](https://github.github.com/gh-aw/reference/triggering-ci/) |
| `--allow-all-tools` in lock.yml | Emitted by `gh aw compile` | Cannot override from `.md` source |
| `gh` CLI inside agent | Credentials scrubbed | Use `steps:` for API calls, or MCP tools |
| `issue_comment` trigger | Requires workflow on default branch | Must merge to `main` before `/slash-commands` work |
| Duplicate runs | gh-aw sometimes creates 2 runs per dispatch | Harmless, use concurrency groups |

---

## Upstream References (All Resolved)

These issues are now **all closed** — documented here for historical context:

| Issue | Status | Resolution |
|-------|--------|------------|
| [gh-aw#18481](https://github.com/github/gh-aw/issues/18481) | ✅ Closed | Fork support tracking — umbrella issue, all sub-items shipped |
| [gh-aw#18518](https://github.com/github/gh-aw/issues/18518) | ✅ Closed | `gh aw init` now warns in forks, lists required secrets |
| [gh-aw#18521](https://github.com/github/gh-aw/issues/18521) | ✅ Closed | Fork support docs created — forks are not supported by default; agents will not run on fork PRs unless `forks:` is configured |
| [gh-aw#23769](https://github.com/github/gh-aw/issues/23769) | ✅ Closed | Platform now auto-restores `.github/` and `.agents/` from base branch after checkout; `.mcp.json` deleted to prevent injection |
| [gh-aw#25439](https://github.com/github/gh-aw/issues/25439) | ✅ Closed | `submit-pull-request-review` safe output previously allowed agents to accidentally approve PRs, bypassing branch protection. Resolution: use `allowed-events: [COMMENT, REQUEST_CHANGES]` to block approvals at infrastructure level |

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Agent evaluates wrong PR | `workflow_dispatch` checks out workflow branch | Add `gh pr checkout` in `steps:` |
| Agent can't find SKILL.md | Fork PR branch doesn't include `.github/skills/` | Platform now restores `.github/` from base branch; ensure workflow uses current compiler version |
| Fork PR skipped on `pull_request` | `forks: ["*"]` not in workflow frontmatter | Add `forks: ["*"]` under `pull_request:` in the `.md` source and recompile |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Agent-created PR has no CI checks | `GITHUB_TOKEN` pushes don't trigger Actions | Add `github-token-for-extra-empty-commit:` with a PAT or GitHub App |
| `/slash-command` doesn't trigger | Workflow not on default branch | Merge to `main` first |
| Agent sees stale issue/PR content | Integrity filtering removed it | Check `min-integrity` level; content from `FIRST_TIMER` is filtered at `approved` |
| Protected file error on PR creation | Agent modified `.github/` or package manifests | Set `protected-files: fallback-to-issue` or `allowed` if intentional |
