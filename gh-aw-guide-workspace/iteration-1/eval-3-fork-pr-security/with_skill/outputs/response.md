# Securely Handling Fork PRs in gh-aw Workflows

## The Core Principle: Read Code, Never Execute It

Your instinct is exactly right. The gh-aw security model is built around a fundamental insight from the [GitHub Security Lab](https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/):

> **Treating PR contents as passive data is safe.** Reading, analyzing, or diffing PR code is fine — the danger is *executing* it.

*(Ref: architecture.md → Security Boundaries → Key Principles, principle #2)*

The classic "pwn-request" attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous. Since your workflow only needs to *read* fork code (not build or run it), gh-aw's architecture is designed for exactly this use case.

---

## Step 1: Enable Fork PRs with `forks: ["*"]`

By default, `gh aw compile` injects a fork guard into the activation job's `if:` condition (`head.repo.id == repository_id`) that blocks fork PRs entirely. You must explicitly opt in.

*(Ref: architecture.md → Execution Model → Fork PR Activation Gate)*

```yaml
---
name: my-code-review-workflow
description: Reviews code from all contributors including forks

on:
  pull_request:
    forks: ["*"]          # ← Removes the auto-injected fork guard
    paths:
      - 'src/**'

concurrency:
  group: "review-${{ github.event.pull_request.number || github.run_id }}"
  cancel-in-progress: true
---
```

Without `forks: ["*"]`, fork PRs are silently skipped — the activation gate blocks them before the workflow even starts.

*(Ref: architecture.md → Troubleshooting: "Fork PR skipped on `pull_request`")*

---

## Step 2: Understand How the Platform Protects You

Once you enable fork PRs, several defense layers activate automatically. Here's the execution flow and what happens at each stage:

*(Ref: architecture.md → Execution Model)*

```
activation job     ← Renders prompt from BASE branch .md (fork can't alter it)
                   ← Saves .github/ and .agents/ as artifact for later restore
    ↓
agent job:
  user steps:      ← Pre-agent, OUTSIDE firewall, has GITHUB_TOKEN
    ↓
  platform steps:  ← checkout_pr_branch.cjs checks out fork code
                   ← Platform restores .github/ and .agents/ from base branch artifact
                   ← .mcp.json deleted from workspace to prevent injection
    ↓
  agent:           ← INSIDE sandboxed container — NO GITHUB_TOKEN, NO gh CLI, NO git creds
```

### What the platform does automatically for fork PRs:

| Protection | How it works |
|------------|-------------|
| **Prompt from base branch** | The prompt is rendered via `{{#runtime-import}}` from the base branch — fork PRs cannot alter the agent's instructions |
| **`.github/` auto-restore** | After `checkout_pr_branch.cjs` checks out the fork, the platform restores `.github/` and `.agents/` from a base-branch artifact. Fork PRs cannot overwrite skills, instructions, or copilot-instructions. *(Ref: architecture.md → Platform `.github/` Restore, gh-aw#23769 — Resolved)* |
| **`.mcp.json` deletion** | Deleted from workspace after checkout to prevent fork-injected MCP configuration |
| **Credential scrubbing** | `GITHUB_TOKEN` and `gh` CLI creds are scrubbed inside the agent container |
| **Network firewall** | The AWF firewall restricts outbound traffic to allowlisted domains |
| **`redact_secrets.cjs`** | Post-agent log scrubbing removes known secret values |
| **Threat detection agent** | Reviews agent outputs before safe-outputs publishes them |

*(Ref: architecture.md → Security Boundaries → gh-aw Defense Layers)*

---

## Step 3: Gather Data in `steps:` (Before Checkout)

Your `steps:` run **before** the platform checks out the fork PR, so they execute against the base branch (trusted code). Use them to collect any GitHub API data the agent will need:

*(Ref: SKILL.md → Common Patterns → Pre-Agent Data Prep)*

```yaml
steps:
  - name: Fetch PR metadata for agent
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      # These run BEFORE fork checkout — safe, trusted context
      gh pr view "$PR_NUMBER" --json title,body,author,labels > pr-metadata.json
      gh pr diff "$PR_NUMBER" --name-only > changed-files.txt
      gh pr diff "$PR_NUMBER" > full-diff.patch
```

**Why here?** The `steps:` context has `GITHUB_TOKEN` and `gh` CLI access. The agent container does not. Anything the agent needs from the GitHub API must be fetched here.

---

## Step 4: Configure Integrity Filtering

For workflows processing external contributor content, configure integrity filtering to prevent prompt injection via untrusted issue comments or PR descriptions:

*(Ref: architecture.md → Security Boundaries → Integrity Filtering)*

```yaml
tools:
  github:
    min-integrity: approved    # Filters content from FIRST_TIMER and unknown users
    blocked-users: ["known-spammer"]
    trusted-users: ["trusted-external-contributor"]
```

**Integrity hierarchy:**

| Level | Who qualifies |
|-------|--------------|
| `merged` | Merged PRs, commits on default branch |
| `approved` | `OWNER`, `MEMBER`, `COLLABORATOR`; non-fork PRs on public repos; `trusted-users` |
| `unapproved` | `CONTRIBUTOR`, `FIRST_TIME_CONTRIBUTOR` |
| `none` | All content including `FIRST_TIMER` |

With `min-integrity: approved`, the agent won't see PR descriptions or comments from first-time contributors — preventing prompt injection payloads from being passed to the LLM.

---

## Step 5: Constrain Safe Outputs

Limit what the agent can do after analyzing the fork code. The fewer write operations, the smaller the blast radius:

*(Ref: architecture.md → Safe Outputs Quick Reference)*

```yaml
safe-outputs:
  add-comment:
    max: 1                      # Agent can post at most 1 comment
    hide-older-comments: true   # Collapse stale comments from previous runs
```

If your workflow also creates PRs, use protected files to prevent the agent from modifying sensitive infrastructure:

```yaml
safe-outputs:
  create-pull-request:
    draft: true                          # Enforced — agent cannot create non-draft PRs
    expires: 14                          # Auto-close after 14 days
    protected-files: fallback-to-issue   # If agent touches .github/, create issue instead
```

*(Ref: architecture.md → Security Boundaries → Protected Files)*

---

## Complete Example: A Secure Fork-Friendly Code Review Workflow

Here's a full workflow `.md` file that reads fork PR code safely:

```yaml
---
name: pr-code-review
description: Reviews code quality on all PRs including forks

on:
  pull_request:
    forks: ["*"]
    paths:
      - 'src/**'
      - 'tests/**'

concurrency:
  group: "code-review-${{ github.event.pull_request.number || github.run_id }}"
  cancel-in-progress: true

tools:
  github:
    min-integrity: approved

safe-outputs:
  add-comment:
    max: 1
    hide-older-comments: true

steps:
  - name: Gather PR context
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      gh pr view "$PR_NUMBER" --json title,body,author,labels,changedFiles > pr-metadata.json
      gh pr diff "$PR_NUMBER" --name-only > changed-files.txt
      gh pr diff "$PR_NUMBER" > pr.diff

  - name: Gate — skip if no relevant code files
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      FILES=$(gh pr diff "$PR_NUMBER" --name-only | grep -E '\.(cs|xaml)$' || true)
      if [ -z "$FILES" ]; then exit 1; fi
---

You are a code reviewer. Analyze the PR diff in `pr.diff` and the metadata
in `pr-metadata.json`. Focus on correctness, safety, and consistency.

Post a single review comment summarizing your findings.

**CRITICAL: You are reviewing code as passive data. Do NOT run `dotnet build`,
`npm install`, or execute any scripts from the workspace.**
```

After editing, compile:

```bash
gh aw compile .github/workflows/pr-code-review.md
# Commit both the .md and the generated .lock.yml
```

---

## What NOT To Do (Anti-Patterns)

*(Ref: architecture.md → Fork PR Handling → Anti-Patterns)*

### ❌ Don't skip checkout for fork PRs

```bash
# ❌ ANTI-PATTERN: Agent evaluates wrong files
if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
  echo "Skipping checkout for fork PR"
  exit 0   # Agent evaluates base branch, not the PR!
fi
```

The correct approach is: always check out the PR, rely on the platform's `.github/` restore to protect agent infrastructure.

### ❌ Don't execute workspace scripts after fork checkout

```yaml
# ❌ DANGEROUS: Runs fork code with GITHUB_TOKEN
steps:
  - name: Checkout PR
    run: gh pr checkout "$PR_NUMBER"
  - name: Run analysis
    run: pwsh .github/skills/some-script.ps1   # This file could be from the fork!
```

If you need to run scripts, run them **before** checkout (from the base branch), or **inside the agent container** (sandboxed, no tokens).

*(Ref: architecture.md → Security Boundaries → Rules for gh-aw Workflow Authors)*

### ❌ Don't build or install fork code anywhere

```yaml
# ❌ DANGEROUS: MSBuild targets / npm postinstall can read COPILOT_TOKEN
steps:
  - run: dotnet build   # Fork's .csproj could include malicious MSBuild targets
```

Even inside the agent container, `COPILOT_TOKEN` is present in the environment (needed for LLM inference). Build tool hooks like MSBuild targets or `npm postinstall` scripts could read it. The AWF network firewall limits where it can be sent, but don't create the opportunity in the first place.

*(Ref: architecture.md → Execution Model → Agent container credential nuance)*

### ❌ Don't set `roles: all` on workflows that process PR content

```yaml
# ❌ DANGEROUS: Any user can trigger the workflow
on:
  pull_request:
    roles: all    # Bypasses the pre_activation role check
```

This removes the collaborator gate, allowing anyone to trigger the workflow. For fork PR workflows, this is unnecessary — `forks: ["*"]` is sufficient.

---

## Security Summary

| Concern | How it's addressed |
|---------|--------------------|
| Fork overwrites agent instructions | Platform auto-restores `.github/` and `.agents/` from base branch artifact after checkout *(gh-aw#23769)* |
| Fork alters the prompt | Prompt rendered from base branch via `runtime-import` — fork cannot change it |
| Fork injects `.mcp.json` | Platform deletes `.mcp.json` from workspace after checkout |
| Agent has write access to repo | Agent container has NO `GITHUB_TOKEN` — writes go through safe-outputs in a separate job |
| Fork code is executed | **Your responsibility** — never run `dotnet build`, `npm install`, or workspace scripts on fork code |
| Prompt injection via PR description | `min-integrity: approved` filters untrusted content before the agent sees it |
| Agent posts malicious comments | Threat detection agent reviews outputs; `max: 1` limits blast radius |
| `COPILOT_TOKEN` exfiltration | AWF network firewall restricts outbound; `redact_secrets.cjs` scrubs logs |

**Bottom line:** gh-aw's architecture is designed for exactly your use case — reading fork code without executing it. Enable `forks: ["*"]`, gather API data in `steps:`, configure integrity filtering, constrain safe outputs, and explicitly instruct the agent never to build or run workspace code. The platform handles the rest.
