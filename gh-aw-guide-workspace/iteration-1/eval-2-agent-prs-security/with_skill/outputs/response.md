# Configuring a gh-aw Workflow That Creates PRs with CI, Protected Files, and No Self-Approval

You need three things: (1) CI triggering on agent-created PRs, (2) `.github/` directory protection, and (3) blocking the agent from approving its own PRs. gh-aw has built-in features for all three — no manual reimplementation needed.

---

## 1. The Core Problem: Agent-Created PRs Don't Trigger CI

GitHub Actions intentionally ignores pushes made by `GITHUB_TOKEN` to prevent infinite workflow loops. Since gh-aw's `create-pull-request` safe output uses `GITHUB_TOKEN` under the hood, the resulting PR will have **no CI checks** by default.

**Source:** The skill's architecture reference (`references/architecture.md`) documents this under **Limitations**:

> | What | Behavior | Workaround |
> |------|----------|------------|
> | Agent-created PRs don't trigger CI | GitHub Actions ignores pushes from `GITHUB_TOKEN` | Use `github-token-for-extra-empty-commit:` with a PAT or GitHub App token on `create-pull-request` |

And in **Troubleshooting**:

> | Agent-created PR has no CI checks | `GITHUB_TOKEN` pushes don't trigger Actions | Add `github-token-for-extra-empty-commit:` with a PAT or GitHub App |

### Solution: `github-token-for-extra-empty-commit:`

This built-in feature pushes an empty commit using a **separate token** (a PAT or GitHub App installation token) after the PR is created. Because the push comes from a different identity than `GITHUB_TOKEN`, GitHub Actions recognizes it and triggers CI.

The SKILL.md anti-patterns table (line 61) explicitly calls this out:

> | Triggering CI on agent-created PRs | `github-token-for-extra-empty-commit:` on `create-pull-request` | [Triggering CI](https://github.github.com/gh-aw/reference/triggering-ci/) |

---

## 2. Protecting `.github/` from Agent Modification

gh-aw has a **Protected Files** mechanism that is **automatically enabled** whenever `create-pull-request` or `push-to-pull-request-branch` is configured.

**Source:** `references/architecture.md`, **Protected Files (Auto-Enabled)** section:

> When `create-pull-request` or `push-to-pull-request-branch` is configured, protected files are automatically enforced. The agent cannot modify:
> - Package manifests (`package.json`, `*.csproj` dependencies, etc.)
> - `.github/` directory contents
> - Agent instruction files

This means **by default, the agent already cannot modify `.github/`**. If the agent attempts to include `.github/` changes in its PR, the behavior depends on the `protected-files:` setting:

| Setting | Behavior |
|---------|----------|
| `blocked` (default) | PR creation **fails entirely** if protected files are modified |
| `fallback-to-issue` | PR branch is pushed, but an **issue** is created for human review instead of a PR |
| `allowed` | Disables protection (**don't use this** for your scenario) |

### Recommended: `fallback-to-issue`

Using `blocked` is safe but harsh — one stray `.github/` edit kills the entire PR. Using `fallback-to-issue` is more resilient: the agent's code changes are preserved on a branch, but a human must review and create the PR manually when protected files are involved.

---

## 3. Preventing Agent Self-Approval

gh-aw discovered (tracked in [gh-aw#25439](https://github.com/github/gh-aw/issues/25439)) that agents using `submit-pull-request-review` could accidentally approve their own PRs, **bypassing branch protection rules**. The fix is the `allowed-events:` option.

**Source:** `references/architecture.md`, under **`create-pull-request` notable options**:

> `allowed-events: [COMMENT, REQUEST_CHANGES]` — On `submit-pull-request-review`, blocks agent from approving PRs (bypasses branch protection). **Always use this** for review workflows.

Note the emphasis: **"Always use this"** for review workflows. If your workflow also reviews its own PRs or any PRs, this is critical.

---

## 4. Complete Workflow Configuration

Here's a complete `.md` workflow source file incorporating all three requirements:

```yaml
---
name: agent-pr-creator
description: Creates PRs from agent analysis output

on:
  # Example: triggered by slash command on an issue
  slash_command:
    command: generate-fix
    description: "Generate a fix PR for this issue"
    reaction: rocket

  # Or triggered manually
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Issue number to fix"
        required: true

concurrency:
  group: "agent-pr-${{ github.event.issue.number || inputs.issue_number || github.run_id }}"
  cancel-in-progress: true

tools:
  github:
    min-integrity: approved       # Filter untrusted content from agent

safe-outputs:
  create-pull-request:
    max: 1                        # Agent can create at most 1 PR per run
    draft: true                   # Enforced: PRs are always drafts (agent can't override)
    protected-files: fallback-to-issue   # If agent touches .github/, create issue instead
    github-token-for-extra-empty-commit: ${{ secrets.CI_TRIGGER_PAT }}  # Triggers CI on the PR

  add-comment:
    max: 1
    hide-older-comments: true
    target: "*"                   # Required for workflow_dispatch

  # If the agent also does code review:
  submit-pull-request-review:
    allowed-events: [COMMENT, REQUEST_CHANGES]   # NEVER allow APPROVE
---

# Agent PR Creator

You are an agent that analyzes GitHub issues and creates pull requests with fixes.

## Instructions

1. Read the issue description and any linked reproduction steps
2. Analyze the codebase to understand the problem
3. Create a fix and open a pull request
4. Do NOT modify any files in `.github/`, `.agents/`, or package manifests
5. Do NOT approve any pull requests — you can only comment or request changes

## Context

Issue: ${{ github.event.issue.number || inputs.issue_number }}
Repository: ${{ github.repository }}
```

### Setting Up the CI Trigger Token

The `github-token-for-extra-empty-commit:` requires a token with write access that is **not** `GITHUB_TOKEN`. You have two options:

**Option A: Personal Access Token (PAT)**
1. Create a fine-grained PAT with `contents: write` permission on the repository
2. Add it as a repository secret (e.g., `CI_TRIGGER_PAT`)
3. Reference it: `github-token-for-extra-empty-commit: ${{ secrets.CI_TRIGGER_PAT }}`

**Option B: GitHub App Installation Token (Recommended for organizations)**
1. Create a GitHub App with `contents: write` permission
2. Use a step or action to generate an installation token
3. Reference the generated token in the safe output

Option B is preferred because GitHub App tokens are scoped, rotated automatically, and don't expire with a user account.

---

## 5. How the Security Layers Stack Up

Here's how the defense-in-depth model applies to your workflow, referencing the **gh-aw Defense Layers** table from `references/architecture.md`:

| Your Requirement | gh-aw Layer | How It Works |
|-----------------|-------------|--------------|
| Agent can't modify `.github/` | **Protected files** (auto-enabled) | `create-pull-request` automatically blocks `.github/`, package manifests, and agent instruction files. Set `protected-files: fallback-to-issue` for graceful handling. |
| Agent can't approve its own PRs | **`allowed-events`** on `submit-pull-request-review` | Restricts the agent to `COMMENT` and `REQUEST_CHANGES` only. `APPROVE` events are rejected at infrastructure level, not prompt level. |
| PRs trigger CI | **`github-token-for-extra-empty-commit:`** | Pushes an empty commit using a separate token identity, which GitHub Actions recognizes as a triggering event. |
| Agent can't exfiltrate secrets | **AWF network firewall** + **credential scrubbing** | `GITHUB_TOKEN` and `gh` CLI credentials are scrubbed inside the agent container. Network is restricted to allowlisted domains. |
| Agent output is reviewed before publishing | **Threat detection agent** + **safe-outputs separation** | Write operations happen in a separate job; threat detection reviews outputs before safe-outputs publishes them. |
| Untrusted content filtered | **Integrity filtering** | `min-integrity: approved` prevents the agent from seeing content from untrusted users (first-time contributors, etc.). |

### What "Protected Files" Covers

The protection is **automatic and infrastructure-enforced** — it doesn't rely on the agent following prompt instructions. Even if the agent ignores your "do not modify `.github/`" instruction, the platform will:

- With `blocked`: Reject the PR creation entirely
- With `fallback-to-issue`: Push the branch but create an issue for human review

This is a critical distinction from prompt-only protection, which is probabilistic. Protected files enforcement is deterministic.

---

## 6. Compilation and Deployment

After creating your `.md` file, compile and commit both files:

```bash
# Compile the workflow
gh aw compile .github/workflows/agent-pr-creator.md

# This generates/updates:
#   .github/workflows/agent-pr-creator.lock.yml
#   .github/aw/actions-lock.json

# Commit both together
git add .github/workflows/agent-pr-creator.md \
       .github/workflows/agent-pr-creator.lock.yml \
       .github/aw/actions-lock.json
git commit -m "Add agent PR creator workflow with CI triggering and protected files"
```

**Source:** SKILL.md, Quick Start section:

> **Always commit the compiled lock file alongside the source `.md`.**

---

## 7. Key Gotchas

1. **`draft: true` is enforced as policy** — the agent cannot create non-draft PRs even if it tries. This gives humans a review gate before merging. (Source: `references/architecture.md`, `create-pull-request` notable options)

2. **Don't set `roles: all`** if this workflow processes external content — it would allow any user to trigger the workflow, including those who might inject malicious content into issues. (Source: `references/architecture.md`, Rules for gh-aw Workflow Authors)

3. **Don't use `protected-files: allowed`** — that disables the protection entirely. Use `blocked` (strictest) or `fallback-to-issue` (graceful degradation).

4. **`allowed-events` is infrastructure-level, not prompt-level** — even if the LLM hallucinates an approval, the platform will reject it. This was the fix for [gh-aw#25439](https://github.com/github/gh-aw/issues/25439).

5. **The `expires:` option** (e.g., `expires: 14`) can auto-close stale agent PRs that nobody reviewed. Consider adding it to prevent abandoned draft PRs from accumulating.
